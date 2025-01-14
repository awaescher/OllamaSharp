using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OllamaSharp;

/// <summary>
/// A source generator that produces Tool-classes (with an InvokeMethodAsync) 
/// from methods marked with [OllamaToolAttribute], using IIncrementalGenerator.
/// </summary>
[Generator]
public class ToolSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// 1) Syntax-Provider einrichten
		var methodCandidates = context.SyntaxProvider
			.CreateSyntaxProvider(
				static (syntaxNode, cancellationToken) => IsCandidateMethod(syntaxNode),
				static (ctx, cancellationToken) => GetMethodSymbolIfMarked(ctx)
			)
			.Where(static methodSymbol => methodSymbol is not null)!;
		// remove nulls

		// 2) Kombiniere die gefundene Methodensymbole mit der Compilation
		var compilationAndMethods = context.CompilationProvider.Combine(methodCandidates.Collect());

		// 3) Register Source Output
		context.RegisterSourceOutput(
			compilationAndMethods,
			(spc, source) => ExecuteGeneration(spc, source.Left, source.Right)
		);
	}

	/// <summary>
	/// Erzeugt den finalen Code basierend auf den gefundenen Methoden-Symbolen.
	/// </summary>
	private static void ExecuteGeneration(
		SourceProductionContext context,
		Compilation compilation,
		IReadOnlyList<IMethodSymbol> methods
	)
	{
		// Für jede gefundene [OllamaTool]-Methode generieren wir eine Tool-Klasse
		foreach (var methodSymbol in methods)
		{
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

	/// <summary>
	/// Prüft grob, ob das SyntaxNode ein Methodendeclaration ist, 
	/// das das Attribut [OllamaToolAttribute] enthalten könnte.
	/// </summary>
	private static bool IsCandidateMethod(SyntaxNode node)
	{
		// Schnelle Prüfung, ob es eine MethodDeclaration sein kann
		if (node is MethodDeclarationSyntax { AttributeLists.Count: > 0 })
			return true;

		return false;
	}

	/// <summary>
	/// Liest bei einem MethodDeclarationSyntax per SemanticModel, 
	/// ob [OllamaToolAttribute] gesetzt ist. Liefert dann das IMethodSymbol.
	/// </summary>
	private static IMethodSymbol? GetMethodSymbolIfMarked(
		GeneratorSyntaxContext context
	)
	{
		var methodDecl = (MethodDeclarationSyntax)context.Node;
		var semanticModel = context.SemanticModel;

		// Prüfe, ob Attribut [OllamaToolAttribute] vorliegt
		var hasOllamaToolAttribute = methodDecl.AttributeLists
			.SelectMany(al => al.Attributes)
			.Any(a => IsOllamaToolAttribute(a, semanticModel));

		if (!hasOllamaToolAttribute)
			return null;

		// Dann hole das IMethodSymbol
		return semanticModel.GetDeclaredSymbol(methodDecl);
	}

	/// <summary>
	/// Stellt fest, ob ein Attribut "OllamaToolAttribute" heißt.
	/// </summary>
	private static bool IsOllamaToolAttribute(AttributeSyntax attr, SemanticModel model)
	{
		var typeInfo = model.GetTypeInfo(attr);
		var name = typeInfo.Type?.ToDisplayString() ?? "";
		return name.EndsWith("OllamaToolAttribute", StringComparison.Ordinal);
	}

	/// <summary>
	/// Extrahiert Summary und Param-Kommentare aus der XML-Doku.
	/// </summary>
	private static (string methodSummary, Dictionary<string, string> paramComments) ExtractDocComments(string xmlDoc)
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
	/// Erzeugt "properties" (Dictionary) und "required" Felder basierend auf den Method-Parametern.
	/// </summary>
	private static (string propertiesCode, string requiredParams) GeneratePropertiesCode(
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

			// Enum?
			if (paramType.TypeKind == TypeKind.Enum)
			{
				jsonType = "string";
				if (paramType is INamedTypeSymbol enumSym)
				{
					enumValues = enumSym.GetMembers()
						.OfType<IFieldSymbol>()
						.Where(f => f.ConstantValue != null)
						.Select(f => f.Name);
				}
			}
			// Numeric (int, long, double, float, etc.)
			else if (paramTypeName.Equals("Int32", StringComparison.OrdinalIgnoreCase)
				  || paramTypeName.Equals("Int64", StringComparison.OrdinalIgnoreCase)
				  || paramTypeName.Equals("Double", StringComparison.OrdinalIgnoreCase)
				  || paramTypeName.Equals("Single", StringComparison.OrdinalIgnoreCase))
			{
				jsonType = "number";
			}
			// bool oder weitere Typen nach Bedarf ergänzen

			if (!param.IsOptional)
			{
				requiredList.Add("\"" + paramName + "\"");
			}

			sbProps.Append("                    { \"");
			sbProps.Append(paramName);
			sbProps.Append("\", new OllamaSharp.Models.Chat.Property { Type = \"");
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
	/// Generiert die Methode zum tatsächlichen Aufruf der Originalmethode:
	/// InvokeMethodAsync(IDictionary<string, object?>? args).
	/// </summary>
	private static string GenerateInvokeMethodCode(IMethodSymbol methodSymbol)
	{
		var parameters = methodSymbol.Parameters;
		var methodName = methodSymbol.Name;
		var className = methodSymbol.ContainingType.ToDisplayString(); // inkl. Namespace
		var isAsync = false;
		var returnType = methodSymbol.ReturnType;

		// Prüfen, ob returnType Task oder Task<T> ist
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
				resultType = null;
			}
		}
		else
		{
			// kein Task => sync
			isAsync = false;
			resultType = returnType.ToDisplayString(); // z.B. "String" oder "GoogleResult"
		}

		// Baue die InvokeMethodAsync
		var sb = new StringBuilder();
		sb.Append("        public ");
		if (isAsync)
			sb.Append("async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)\r\n");
		else
			sb.Append("object? InvokeMethod(IDictionary<string, object?>? args)\r\n");
		sb.Append("        {\r\n");
		sb.Append("            if (args == null) args = new Dictionary<string, object?>();\r\n");

		var argList = new List<string>();
		foreach (var p in parameters)
		{
			var paramName = p.Name;
			var pType = p.Type;
			var pTypeName = pType.Name;
			var safeParamName = ToValidIdentifier(paramName);

			sb.Append("            ");

			// enum?
			if (pType.TypeKind == TypeKind.Enum)
			{
				// (Unit)Enum.Parse(typeof(Unit), ...)
				sb.Append(pType.ToDisplayString());
				sb.Append(" ");
				sb.Append(safeParamName);
				sb.Append(" = ");
				if (p.IsOptional)
				{
					// default fallback
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
			// Numeric
			else if (pTypeName.Equals("Int32", StringComparison.OrdinalIgnoreCase) ||
					 pTypeName.Equals("Int64", StringComparison.OrdinalIgnoreCase) ||
					 pTypeName.Equals("Double", StringComparison.OrdinalIgnoreCase) ||
					 pTypeName.Equals("Single", StringComparison.OrdinalIgnoreCase))
			{
				sb.Append(pType.ToDisplayString());
				sb.Append(" ");
				sb.Append(safeParamName);
				sb.Append(" = ");
				if (p.IsOptional)
				{
					sb.Append("args.ContainsKey(\"");
					sb.Append(paramName);
					sb.Append("\") ? Convert.To");
					sb.Append(pTypeName);
					sb.Append("(args[\"");
					sb.Append(paramName);
					sb.Append("\"]) : ");
					sb.Append(p.ExplicitDefaultValue ?? 0);
					sb.Append(";\r\n");
				}
				else
				{
					sb.Append("Convert.To");
					sb.Append(pTypeName);
					sb.Append("(args[\"");
					sb.Append(paramName);
					sb.Append("\"]);\r\n");
				}
			}
			// string oder andere Typen
			else
			{
				// string => (string?)args["xyz"] ?? "default"
				sb.Append(pType.ToDisplayString());
				sb.Append(" ");
				sb.Append(safeParamName);
				sb.Append(" = ");
				if (pTypeName.Equals("String", StringComparison.OrdinalIgnoreCase))
				{
					sb.Append("(" + pType.ToDisplayString() + "?)args[\"");
					sb.Append(paramName);
					sb.Append("\"]");
					if (p.IsOptional)
					{
						sb.Append(" ?? ");
						if (p.ExplicitDefaultValue == null)
							sb.Append("\"\"");
						else
							sb.Append("\"" + p.ExplicitDefaultValue + "\"");
						sb.Append(";\r\n");
					}
					else
					{
						sb.Append(" ?? \"\";\r\n");
					}
				}
				else
				{
					// fallback => cast + optional handling
					sb.Append("(" + pType.ToDisplayString() + "?)args[\"");
					sb.Append(paramName);
					sb.Append("\"]");
					if (p.IsOptional && p.ExplicitDefaultValue != null)
					{
						sb.Append(" ?? (");
						sb.Append(pType.ToDisplayString());
						sb.Append(")");
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
			// sync
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
				sb.Append(className + "." + methodName);
				sb.Append("(" + string.Join(", ", argList) + ");\r\n");
				sb.Append("            return result;\r\n");
			}
		}

		sb.Append("        }\r\n");
		return sb.ToString();
	}

	/// <summary>
	/// Erzeugt die komplette generierte Tool-Klasse (inkl. Konstruktor und InvokeMethodAsync).
	/// </summary>
	private static string GenerateToolClassCode(
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
		sb.Append("\r\n");
		sb.Append("namespace ");
		sb.Append(containingNamespace);
		sb.Append("\r\n{\r\n");
		sb.Append("    /// <summary>\r\n");
		sb.Append("    /// This class was auto-generated by the OllamaSharp ToolSourceGenerator.\r\n");
		sb.Append("    /// </summary>\r\n");
		sb.Append("    public class ");
		sb.Append(toolClassName);
		sb.Append($" : OllamaSharp.Models.Chat.Tool, {(isAsync ? "OllamaSharp.Tools.IAsyncInvokableTool" : "OllamaSharp.Tools.IInvokableTool")}\r\n");
		sb.Append("    {\r\n");
		sb.Append("        /// <summary>\r\n");
		sb.Append("        /// Initializes a new instance with metadata about the original method.\r\n");
		sb.Append("        /// </summary>\r\n");
		sb.Append("        public ");
		sb.Append(toolClassName);
		sb.Append("()\r\n");
		sb.Append("        {\r\n");
		sb.Append("            this.Function = new OllamaSharp.Models.Chat.Function {\r\n");
		sb.Append("                Name = \"");
		sb.Append(originalMethodName);
		sb.Append("\",\r\n");
		sb.Append("                Description = \"");
		sb.Append(EscapeString(methodSummary));
		sb.Append("\"\r\n");
		sb.Append("            };\r\n");
		sb.Append("\r\n");
		sb.Append("            this.Function.Parameters = new OllamaSharp.Models.Chat.Parameters {\r\n");
		sb.Append("                Properties = new Dictionary<string, OllamaSharp.Models.Chat.Property> {\r\n");
		sb.Append(propertiesCode);
		sb.Append("\r\n                },\r\n");
		sb.Append("                Required = ");
		sb.Append(requiredParams);
		sb.Append("\r\n");
		sb.Append("            };\r\n");
		sb.Append("        }\r\n");
		sb.Append("\r\n");
		sb.Append(invokeMethodCode);
		sb.Append("    }\r\n");
		sb.Append("}\r\n");

		return sb.ToString();
	}

	/// <summary>
	/// Minimales String-Escaping.
	/// </summary>
	private static string EscapeString(string input)
	{
		return input
			.Replace("\\", "\\\\")
			.Replace("\"", "\\\"");
	}

	/// <summary>
	/// Sorgt dafür, dass man z.B. keine Parameter-Namen hat, die Keywords sind.
	/// </summary>
	private static string ToValidIdentifier(string name)
	{
		if (SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
		{
			return "_" + name;
		}
		return name;
	}
}
