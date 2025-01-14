using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OllamaSharp;

/// <summary>
/// A source generator that produces Tool-classes (with an InvokeMethodAsync) from methods marked with [OllamaToolAttribute].
/// </summary>
[Generator]
public class OllamaToolGenerator : ISourceGenerator
{
	public void Initialize(GeneratorInitializationContext context)
	{
		// Optional init logic
	}

	public void Execute(GeneratorExecutionContext context)
	{
		var compilation = context.Compilation;

		foreach (var syntaxTree in compilation.SyntaxTrees)
		{
			var semanticModel = compilation.GetSemanticModel(syntaxTree);
			var root = syntaxTree.GetRoot(context.CancellationToken);

			// Suche nach Methoden mit [OllamaToolAttribute].
			var methodNodes = root.DescendantNodes()
				.OfType<MethodDeclarationSyntax>()
				.Where(md => md.AttributeLists
					.SelectMany(al => al.Attributes)
					.Any(a => IsOllamaToolAttribute(a, semanticModel)));

			foreach (var methodNode in methodNodes)
			{
				var methodSymbol = semanticModel.GetDeclaredSymbol(methodNode, context.CancellationToken);
				if (methodSymbol == null)
					continue;

				var containingNamespace = methodSymbol.ContainingType.ContainingNamespace?.ToString() ?? "";
				var containingClassName = methodSymbol.ContainingType.Name;
				var toolClassName = methodSymbol.Name + "Tool";

				// Hole XML-Doku (Summary, Param) aus dem Symbol
				var docCommentXml = methodSymbol.GetDocumentationCommentXml();
				var (methodSummary, paramComments) = ExtractDocComments(docCommentXml);

				// Generiere den Code für Properties/Required
				var (propertiesCode, requiredParams) = GeneratePropertiesCode(methodSymbol.Parameters, paramComments);

				// Gen. Code für die InvokeMethodAsync
				var invokeMethodCode = GenerateInvokeMethodCode(methodSymbol);

				// Erzeuge finalen Code
				var sourceCode = GenerateToolClassCode(
					containingNamespace,
					containingClassName,
					toolClassName,
					methodSymbol.Name,
					methodSummary,
					propertiesCode,
					requiredParams,
					invokeMethodCode
				);

				var hintName = containingNamespace + "." + containingClassName + "." + toolClassName + ".g.cs";
				context.AddSource(hintName, sourceCode);
			}
		}
	}

	/// <summary>
	/// Checks if a given attribute is named "OllamaToolAttribute".
	/// </summary>
	private bool IsOllamaToolAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
	{
		var typeInfo = semanticModel.GetTypeInfo(attributeSyntax);
		var name = typeInfo.Type?.ToDisplayString() ?? "";
		return name.EndsWith("OllamaToolAttribute", StringComparison.Ordinal);
	}

	/// <summary>
	/// Extract summary and param descriptions from XML doc comments.
	/// </summary>
	private (string methodSummary, Dictionary<string, string> paramComments) ExtractDocComments(string xmlDoc)
	{
		var summaryText = "";
		var paramDict = new Dictionary<string, string>();

		if (!string.IsNullOrEmpty(xmlDoc))
		{
			// Summary
			var summaryStart = xmlDoc.IndexOf("<summary>", StringComparison.OrdinalIgnoreCase);
			var summaryEnd = xmlDoc.IndexOf("</summary>", StringComparison.OrdinalIgnoreCase);
			if (summaryStart != -1 && summaryEnd != -1)
			{
				summaryText = xmlDoc.Substring(
					summaryStart + "<summary>".Length,
					summaryEnd - (summaryStart + "<summary>".Length)
				).Trim();
			}

			// Param
			var paramTag = "<param name=\"";
			var currentIndex = 0;
			while (true)
			{
				var paramStart = xmlDoc.IndexOf(paramTag, currentIndex, StringComparison.OrdinalIgnoreCase);
				if (paramStart == -1)
					break;

				var quoteEnd = xmlDoc.IndexOf("\"", paramStart + paramTag.Length, StringComparison.OrdinalIgnoreCase);
				if (quoteEnd == -1)
					break;
				var paramName = xmlDoc.Substring(paramStart + paramTag.Length, quoteEnd - (paramStart + paramTag.Length));

				var closeTag = "</param>";
				var paramEnd = xmlDoc.IndexOf(closeTag, quoteEnd, StringComparison.OrdinalIgnoreCase);
				if (paramEnd == -1)
					break;

				var contentStart = xmlDoc.IndexOf(">", quoteEnd, StringComparison.OrdinalIgnoreCase);
				if (contentStart == -1)
					break;

				var paramContent = xmlDoc.Substring(contentStart + 1, paramEnd - (contentStart + 1)).Trim();
				paramDict[paramName] = paramContent;

				currentIndex = paramEnd + closeTag.Length;
			}
		}

		return (summaryText, paramDict);
	}

	/// <summary>
	/// Generates the code for the parameter 'properties' dictionary and 'required' fields from method parameters.
	/// </summary>
	private (string propertiesCode, string requiredParams) GeneratePropertiesCode(
		IReadOnlyList<IParameterSymbol> parameters,
		Dictionary<string, string> paramComments)
	{
		var sbProps = new StringBuilder();
		var requiredList = new List<string>();

		foreach (var param in parameters)
		{
			var paramName = param.Name;
			var paramType = param.Type;
			var paramTypeName = paramType.Name;
			var description = paramComments.ContainsKey(paramName) ? paramComments[paramName] : "No description.";
			var jsonType = "string";  // default
			IEnumerable<string>? enumValues = null;

			// Enum
			if (paramType.TypeKind == TypeKind.Enum)
			{
				jsonType = "string";
				var enumSym = paramType as INamedTypeSymbol;
				if (enumSym != null)
				{
					enumValues = enumSym.GetMembers()
						.OfType<IFieldSymbol>()
						.Where(f => f.ConstantValue != null)
						.Select(f => f.Name);
				}
			}
			// Numeric
			else if (paramTypeName.Equals("Int32", StringComparison.OrdinalIgnoreCase)
				  || paramTypeName.Equals("Int64", StringComparison.OrdinalIgnoreCase)
				  || paramTypeName.Equals("Double", StringComparison.OrdinalIgnoreCase)
				  || paramTypeName.Equals("Single", StringComparison.OrdinalIgnoreCase))
			{
				jsonType = "number";
			}
			// bool oder andere Fälle ggf. ergänzen

			// required?
			if (!param.IsOptional)
			{
				requiredList.Add("\"" + paramName + "\"");
			}

			sbProps.Append("                    { \"");
			sbProps.Append(paramName);
			sbProps.Append("\", new Property { Type = \"");
			sbProps.Append(jsonType);
			sbProps.Append("\", Description = \"");
			sbProps.Append(EscapeString(description));
			sbProps.Append("\"");

			if (enumValues != null)
			{
				sbProps.Append(", Enum = new[] {");
				bool first = true;
				foreach (var val in enumValues)
				{
					if (!first)
						sbProps.Append(", ");
					sbProps.Append("\"");
					sbProps.Append(val);
					sbProps.Append("\"");
					first = false;
				}
				sbProps.Append("}");
			}

			sbProps.Append(" } },\r\n");
		}

		var propStr = sbProps.ToString().TrimEnd('\r', '\n', ',');
		var reqStr = requiredList.Count > 0
			? "new[] {" + string.Join(", ", requiredList) + "}"
			: "Array.Empty<string>()";

		return (propStr, reqStr);
	}

	/// <summary>
	/// Generates code for an async invocation method that calls the original method with the correct parameters.
	/// </summary>
	/// <param name="methodSymbol">Symbol for the original method.</param>
	private string GenerateInvokeMethodCode(IMethodSymbol methodSymbol)
	{
		// Methode: static string|Task|Task<T>|T ...
		// Wir erzeugen "InvokeMethodAsync(IDictionary<string, object?> args)"
		//  1) Parameter entpacken
		//  2) Originalmethode aufrufen
		//  3) Rückgabewert (ggf. await)
		//  4) Als object? zurück

		var parameters = methodSymbol.Parameters;
		var methodName = methodSymbol.Name;
		var className = methodSymbol.ContainingType.ToDisplayString(); // inkl. Namespace
		var isAsync = false;
		var returnType = methodSymbol.ReturnType;

		// Prüfen auf Task oder Task<T>
		string? resultType = null;
		if (returnType.Name.Equals("Task", StringComparison.OrdinalIgnoreCase))
		{
			// Entweder Task oder Task<T>
			if (returnType is INamedTypeSymbol namedType && namedType.IsGenericType)
			{
				// Task<T>
				var typeArg = namedType.TypeArguments.FirstOrDefault();
				if (typeArg != null)
				{
					isAsync = true;
					resultType = typeArg.ToDisplayString(); // z.B. "GoogleResult"
				}
			}
			else
			{
				// plain Task
				isAsync = true;
				resultType = null; // void
			}
		}
		else
		{
			// kein Task => sync
			isAsync = false;
			resultType = returnType.ToDisplayString(); // z.B. "String" oder "GoogleResult"
		}

		// Bilde Aufrufzeile "className.methodName(...params...)"
		var sb = new StringBuilder();
		// Signatur
		sb.Append("        public ");
		if (isAsync)
			sb.Append("async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)\r\n");
		else
			sb.Append("object? InvokeMethod(IDictionary<string, object?>? args)\r\n");
		sb.Append("        {\r\n");
		sb.Append("            if (args == null) args = new Dictionary<string, object?>();\r\n");

		// Parameter entpacken
		var argList = new List<string>();
		foreach (var p in parameters)
		{
			// z.B.: string location = (string?)args["location"] ?? "";
			// bei Enums => Enum.Parse
			// bei optional => Default
			// bei int => Convert.ToInt32
			var paramName = p.Name;
			var pType = p.Type;
			var pTypeName = pType.Name;

			var safeParamName = ToValidIdentifier(paramName);

			sb.Append("            ");

			// wenn Enum
			if (pType.TypeKind == TypeKind.Enum)
			{
				// z.B.: var unit = args.ContainsKey("unit") ? (Unit)Enum.Parse(typeof(Unit), args["unit"]?.ToString() ?? "Celsius") : Unit.Celsius;
				sb.Append(pType.ToDisplayString());
				sb.Append(" ");
				sb.Append(safeParamName);
				sb.Append(" = ");
				if (p.IsOptional)
				{
					// default-Wert ermitteln
					sb.Append("(" + pType.ToDisplayString() + ")");
					sb.Append("Enum.Parse(typeof(" + pType.ToDisplayString() + "), args.ContainsKey(\"");
					sb.Append(paramName);
					sb.Append("\") ? args[\"");
					sb.Append(paramName);
					sb.Append("\"]?.ToString() ?? \"");
					sb.Append(p.ExplicitDefaultValue?.ToString() ?? pType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault()?.Name);
					sb.Append("\", true) : \"");
					sb.Append(p.ExplicitDefaultValue?.ToString() ?? pType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault()?.Name);
					sb.Append("\", true);\r\n");
				}
				else
				{
					sb.Append("(" + pType.ToDisplayString() + ")");
					sb.Append("Enum.Parse(typeof(" + pType.ToDisplayString() + "), args[\"");
					sb.Append(paramName);
					sb.Append("\"]?.ToString() ?? \"\", true);\r\n");
				}
			}
			// Numeric (int, long, double, etc.)
			else if (pTypeName.Equals("Int32", StringComparison.OrdinalIgnoreCase) ||
					 pTypeName.Equals("Int64", StringComparison.OrdinalIgnoreCase) ||
					 pTypeName.Equals("Double", StringComparison.OrdinalIgnoreCase) ||
					 pTypeName.Equals("Single", StringComparison.OrdinalIgnoreCase))
			{
				// var userId = args.ContainsKey("userId") ? Convert.ToInt32(args["userId"]) : -1;
				sb.Append(pType.ToDisplayString());
				sb.Append(" ");
				sb.Append(safeParamName);
				sb.Append(" = ");
				if (p.IsOptional)
				{
					sb.Append("args.ContainsKey(\"");
					sb.Append(paramName);
					sb.Append("\") ? Convert.To");
					sb.Append(pType.Name);
					sb.Append("(args[\"");
					sb.Append(paramName);
					sb.Append("\"]) : ");
					sb.Append(p.ExplicitDefaultValue ?? 0);
					sb.Append(";\r\n");
				}
				else
				{
					sb.Append("Convert.To");
					sb.Append(pType.Name);
					sb.Append("(args[\"");
					sb.Append(paramName);
					sb.Append("\"]);\r\n");
				}
			}
			// string, bool, etc. -> nur cast
			else
			{
				// string => z.B. var location = (string?)args["location"] ?? "";
				sb.Append(pType.ToDisplayString());
				sb.Append(" ");
				sb.Append(safeParamName);
				sb.Append(" = ");
				if (pTypeName.Equals("String", StringComparison.OrdinalIgnoreCase))
				{
					// string?
					sb.Append("(" + pType.ToDisplayString() + "?)args[\"");
					sb.Append(paramName);
					sb.Append("\"]");
					if (p.IsOptional)
					{
						// Falls default = null => "??" ... 
						// oder falls default != null => z.B. "?? \"XYZ\""
						sb.Append(" ?? ");
						if (p.ExplicitDefaultValue == null)
							sb.Append("\"\"");
						else
							sb.Append("\"" + p.ExplicitDefaultValue.ToString() + "\"");
						sb.Append(";\r\n");
					}
					else
					{
						sb.Append(" ?? \"\";\r\n");
					}
				}
				else
				{
					// bool, custom classes, etc. => Best guess
					sb.Append("(" + pType.ToDisplayString() + "?)args[\"");
					sb.Append(paramName);
					sb.Append("\"]");
					if (p.IsOptional && p.ExplicitDefaultValue != null)
					{
						sb.Append(" ?? ");
						sb.Append("(" + pType.ToDisplayString() + ")");
						sb.Append(p.ExplicitDefaultValue);
					}
					sb.Append(";\r\n");
				}
			}

			argList.Add(safeParamName);
		}

		// Originalmethode aufrufen
		sb.Append("\r\n            ");
		if (isAsync)
		{
			sb.Append("var result = await ");
			sb.Append(className);
			sb.Append(".");
			sb.Append(methodName);
			sb.Append("(");
			sb.Append(string.Join(", ", argList));
			sb.Append(");\r\n");
			sb.Append("            return result;\r\n");
		}
		else
		{
			if (returnType.SpecialType == SpecialType.System_Void)
			{
				// void
				sb.Append(className + "." + methodName);
				sb.Append("(" + string.Join(", ", argList) + ");\r\n");
				sb.Append("            return null;\r\n");
			}
			else
			{
				sb.Append("var result = ");
				sb.Append(className);
				sb.Append(".");
				sb.Append(methodName);
				sb.Append("(");
				sb.Append(string.Join(", ", argList));
				sb.Append(");\r\n");
				sb.Append("            return result;\r\n");
			}
		}

		sb.Append("        }\r\n");
		return sb.ToString();
	}

	/// <summary>
	/// Generates the final code for the tool class including the invoke method.
	/// </summary>
	private string GenerateToolClassCode(
		string containingNamespace,
		string containingClass,
		string toolClassName,
		string originalMethodName,
		string methodSummary,
		string propertiesCode,
		string requiredParams,
		string invokeMethodCode)
	{
		var isAsync = invokeMethodCode.Contains("async Task");

		var sb = new StringBuilder();
		sb.Append("using System;\r\n");
		sb.Append("using System.Collections.Generic;\r\n");
		sb.Append("using System.ComponentModel;\r\n");
		sb.Append("using System.Text.Json.Serialization;\r\n");
		sb.Append("using System.Threading.Tasks;\r\n");
		sb.Append("using OllamaSharp.Models.Chat;\r\n");
		sb.Append("\r\n");
		sb.Append("namespace ");
		sb.Append(containingNamespace);
		sb.Append("\r\n{\r\n");
		sb.Append("    /// <summary>\r\n");
		sb.Append("    /// This class was auto-generated by the OllamaSharp ToolSourceGenerator.\r\n");
		sb.Append("    /// </summary>\r\n");
		sb.Append("    public class ");
		sb.Append(toolClassName);
		sb.Append($" : Tool, {(isAsync ? "IAsyncInvokableTool" : "IInvokableTool")}\r\n");
		sb.Append("    {\r\n");
		sb.Append("        /// <summary>\r\n");
		sb.Append("        /// Initializes a new instance with metadata about the original method.\r\n");
		sb.Append("        /// </summary>\r\n");
		sb.Append("        public ");
		sb.Append(toolClassName);
		sb.Append("()\r\n");
		sb.Append("        {\r\n");
		sb.Append("            this.Function = new Function {\r\n");
		sb.Append("                Name = \"");
		sb.Append(originalMethodName);
		sb.Append("\",\r\n");
		sb.Append("                Description = \"");
		sb.Append(EscapeString(methodSummary));
		sb.Append("\",\r\n");
		sb.Append("            };\r\n");
		sb.Append("\r\n");
		sb.Append("            this.Function.Parameters = new Parameters {\r\n");
		sb.Append("                Properties = new Dictionary<string, Property> {\r\n");
		sb.Append(propertiesCode);
		sb.Append("\r\n                },\r\n");
		sb.Append("                Required = ");
		sb.Append(requiredParams);
		sb.Append("\r\n");
		sb.Append("            };\r\n");
		sb.Append("        }\r\n");
		sb.Append("\r\n");
		// Füge die InvokeMethodAsync ein
		sb.Append(invokeMethodCode);
		sb.Append("    }\r\n");
		sb.Append("}\r\n");

		return sb.ToString();
	}

	private string EscapeString(string input)
	{
		return input
			.Replace("\\", "\\\\")
			.Replace("\"", "\\\"");
	}

	/// <summary>
	/// Make sure param-names don't collide with keywords, etc.
	/// </summary>
	private string ToValidIdentifier(string name)
	{
		// Minimalbeispiel: wir hängen einen Underscore an, wenn's unguenstig ist
		// (Bsp. "class", "return", etc.)
		if (SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
		{
			return "_" + name;
		}
		return name;
	}
}
