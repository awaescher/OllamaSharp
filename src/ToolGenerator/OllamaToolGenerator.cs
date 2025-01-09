// Datei: OllamaSourceGenerator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OllamaSharp
{
	/// <summary>
	/// Ein Beispiel für einen Source-Generator, der Tool-Klassen
	/// basierend auf Methoden mit [OllamaToolAttribute] generiert.
	/// </summary>
	[Generator]
	public class OllamaSourceGenerator : ISourceGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
			// Hier könnte man ggf. Syntax-Receiver registrieren, etc.
			// Für dieses Beispiel genügt es aber, alles in Execute zu machen.
		}

		public void Execute(GeneratorExecutionContext context)
		{
			try
			{
				// Alle SyntaxTrees durchlaufen
				foreach (var syntaxTree in context.Compilation.SyntaxTrees)
				{
					var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

					// Alle Klassen (ClassDeclarationSyntax) im SyntaxTree abfragen:
					var classNodes = syntaxTree
						.GetRoot()
						.DescendantNodes()
						.OfType<ClassDeclarationSyntax>();

					foreach (var classNode in classNodes)
					{
						// Namespace herausfinden
						var ns = GetNamespace(classNode);

						// Für jede statische (oder auch nicht-statische) Methode prüfen, ob das [OllamaTool]-Attribut vorhanden ist
						var methodNodes = classNode
							.DescendantNodes()
							.OfType<MethodDeclarationSyntax>()
							.Where(m => m.AttributeLists
										  .SelectMany(a => a.Attributes)
										  .Any(a => HasOllamaToolAttribute(a, semanticModel)));

						foreach (var methodNode in methodNodes)
						{
							GenerateToolClassForMethod(context, semanticModel, ns, classNode, methodNode);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Falls du Debug-Infos brauchst, kannst du sie hier in den Generator ausgeben
				context.ReportDiagnostic(Diagnostic.Create(
					new DiagnosticDescriptor(
						"OLLAMA001",
						"Generator Error",
						"Fehler im OllamaSourceGenerator: {0}",
						"OllamaSourceGenerator",
						DiagnosticSeverity.Error,
						isEnabledByDefault: true),
					Location.None,
					ex.Message));
			}
		}

		/// <summary>
		/// Erzeugt die Tool-Klasse (z.B. GetWeatherTool) passend zur angegebenen Methode.
		/// </summary>
		private void GenerateToolClassForMethod(
			GeneratorExecutionContext context,
			SemanticModel semanticModel,
			string namespaceName,
			ClassDeclarationSyntax classNode,
			MethodDeclarationSyntax methodNode)
		{
			// Name der Methode z.B. "GetWeather"
			var methodName = methodNode.Identifier.Text;

			// Name der generierten Tool-Klasse z.B. "GetWeatherTool"
			var toolClassName = methodName + "Tool";

			// Summary aus DocComment
			// Dazu extrahieren wir den Kommentar über der Methode
			var summaryText = ExtractSummaryFromMethod(methodNode);

			// Parameter (Name, Typ, Kommentar, ggf. Enum etc.)
			var parameterInfos = GetParameterInfos(methodNode, semanticModel);

			// Generieren wir den Quellcode für die Tool-Klasse
			var sourceCode = CreateToolClassSource(namespaceName, toolClassName, methodName, summaryText, parameterInfos);

			// An das Kompilat anhängen
			context.AddSource(toolClassName + ".g.cs", sourceCode);
		}

		/// <summary>
		/// Erzeugt den C#-Code für die Tool-Klasse.
		/// </summary>
		private string CreateToolClassSource(
			string namespaceName,
			string toolClassName,
			string methodName,
			string summaryText,
			List<ParameterInfo> parameters)
		{
			// Code, der am Ende generiert wird
			// Du kannst natürlich noch mehr Formatierungen, Einrückungen etc. ergänzen
			var sb = new StringBuilder();

			sb.Append("namespace ").Append(namespaceName).AppendLine();
			sb.AppendLine("{");
			sb.Append("    internal sealed partial class ").Append(toolClassName)
			  .Append(" : OllamaSharp.Models.Chat.Tool").AppendLine();
			sb.AppendLine("    {");
			sb.AppendLine("        public ").Append(toolClassName).Append("()");
			sb.AppendLine("        {");
			sb.AppendLine("            this.Function = new OllamaSharp.Models.Chat.Function");
			sb.AppendLine("            {");
			sb.Append("                Name = \"").Append(methodName).AppendLine("\",");
			// Beschreibung in Anführungszeichen säubern
			sb.Append("                Description = \"").Append(EscapeString(summaryText)).AppendLine("\",");
			sb.AppendLine("                Parameters = new OllamaSharp.Models.Chat.Parameters");
			sb.AppendLine("                {");
			sb.AppendLine("                    Type = \"object\",");
			sb.AppendLine("                    Properties = new System.Collections.Generic.Dictionary<string, OllamaSharp.Models.Chat.Property>");
			sb.AppendLine("                    {");

			for (int i = 0; i < parameters.Count; i++)
			{
				var p = parameters[i];
				sb.Append("                        { \"").Append(p.Name).Append("\", new OllamaSharp.Models.Chat.Property");
				sb.AppendLine(" {");
				sb.Append("                            Type = \"").Append(p.Type).Append("\",");
				sb.Append(" Description = \"").Append(EscapeString(p.DocComment)).Append("\"");

				// Falls es ein Enum ist, fügen wir hier noch das "enum"-Feld hinzu
				if (p.EnumValues.Any())
				{
					sb.Append(", Enum = new string[] { ");
					sb.Append(string.Join(", ", p.EnumValues.Select(e => "\"" + e + "\"")));
					sb.Append(" }");
				}

				sb.AppendLine(" } },");
			}

			sb.AppendLine("                    },");
			// Required Felder: alles, was nicht optional ist. Im Beispiel ignorieren wir optional vs. required.
			// Du könntest hier Logik ergänzen, falls du Parameter defaulten möchtest o.ä.
			var requiredNames = parameters.Select(x => x.Name).ToArray();
			sb.Append("                    Required = new string[] { ");
			sb.Append(string.Join(", ", requiredNames.Select(r => "\"" + r + "\"")));
			sb.AppendLine(" }");

			sb.AppendLine("                }");
			sb.AppendLine("            };");
			sb.AppendLine("        }"); // Ende Konstruktor
			sb.AppendLine("    }"); // Ende class
			sb.AppendLine("}"); // Ende namespace

			return sb.ToString();
		}

		/// <summary>
		/// Prüft, ob es sich beim angegebenen Attribut um [OllamaTool] handelt.
		/// </summary>
		private bool HasOllamaToolAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
		{
			var attributeSymbol = semanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;
			if (attributeSymbol == null)
			{
				return false;
			}
			var attrContainingType = attributeSymbol.ContainingType;
			return attrContainingType.ToDisplayString() == "OllamaSharp.OllamaToolAttribute";
		}

		/// <summary>
		/// Ermittelt den Namespace, in dem sich die Klasse befindet (kann verschachtelt sein).
		/// </summary>
		private static string GetNamespace(SyntaxNode classNode)
		{
			var current = classNode;
			while (current != null && !(current is NamespaceDeclarationSyntax) && !(current is FileScopedNamespaceDeclarationSyntax))
			{
				current = current.Parent;
			}

			if (current is NamespaceDeclarationSyntax ns)
			{
				return ns.Name.ToString();
			}

			if (current is FileScopedNamespaceDeclarationSyntax fs)
			{
				return fs.Name.ToString();
			}

			// Falls keiner gefunden wird, Default-Namespace nehmen (oder leer)
			return "GlobalNamespace";
		}

		/// <summary>
		/// DocComment-XML parsen, um den Summary-Teil zu extrahieren.
		/// </summary>
		private static string ExtractSummaryFromMethod(MethodDeclarationSyntax methodNode)
		{
			var rawXml = methodNode.GetLeadingTrivia()
				.Select(trivia => trivia.GetStructure())
				.OfType<DocumentationCommentTriviaSyntax>()
				.FirstOrDefault();

			if (rawXml == null)
			{
				return string.Empty;
			}

			// summary-Element finden
			var summaryElement = rawXml.DescendantNodes()
				.OfType<XmlElementSyntax>()
				.FirstOrDefault(x => x.StartTag.Name.LocalName.Text == "summary");

			if (summaryElement == null)
				return string.Empty;

			var sb = new StringBuilder();
			foreach (var node in summaryElement.Content)
			{
				sb.Append(node.ToString().Trim());
				sb.Append(" ");
			}
			return sb.ToString().Trim();
		}

		/// <summary>
		/// Liest für jede Methode die Parameter ein (Typ, Name, Doc-Comment).
		/// </summary>
		private List<ParameterInfo> GetParameterInfos(MethodDeclarationSyntax methodNode, SemanticModel semanticModel)
		{
			var result = new List<ParameterInfo>();

			var rawXml = methodNode.GetLeadingTrivia()
				.Select(trivia => trivia.GetStructure())
				.OfType<DocumentationCommentTriviaSyntax>()
				.FirstOrDefault();

			// Mapping von Parameternamen -> Param-Dokumentation
			var paramDocMap = new Dictionary<string, string>();
			if (rawXml != null)
			{
				var paramElements = rawXml.DescendantNodes()
					.OfType<XmlElementSyntax>()
					.Where(x => x.StartTag.Name.LocalName.Text == "param");

				foreach (var p in paramElements)
				{
					// Attribut "name" aus dem <param name="...">
					var nameAttr = p.StartTag.Attributes
						.OfType<XmlNameAttributeSyntax>()
						.FirstOrDefault(a => a.Name.LocalName.Text == "name");

					if (nameAttr != null)
					{
						var paramName = nameAttr.Identifier.Identifier.Text;
						var commentText = string.Concat(p.Content.Select(c => c.ToString())).Trim();
						paramDocMap[paramName] = commentText;
					}
				}
			}

			// Parameter durchgehen
			foreach (var p in methodNode.ParameterList.Parameters)
			{
				var paramName = p.Identifier.Text;
				var typeInfo = semanticModel.GetTypeInfo(p.Type!).Type;
				var paramDoc = paramDocMap.ContainsKey(paramName) ? paramDocMap[paramName] : "";

				// Standardmäßig "string", "number", etc. angeben.
				// Für Enums sammelst du die Values und gibst "type=string" mit "enum" an.
				if (typeInfo != null && typeInfo.TypeKind == TypeKind.Enum)
				{
					var enumMembers = typeInfo.GetMembers()
						.Where(m => m.Kind == SymbolKind.Field && m is IFieldSymbol)
						.Select(m => m.Name)
						.ToList();

					result.Add(new ParameterInfo
					{
						Name = paramName,
						Type = "string",
						DocComment = paramDoc,
						EnumValues = enumMembers
					});
				}
				else
				{
					// Minimallogik, die man natürlich anpassen kann
					var simpleType = MapToJsonSchemaType(typeInfo?.Name ?? "object");
					result.Add(new ParameterInfo
					{
						Name = paramName,
						Type = simpleType,
						DocComment = paramDoc
					});
				}
			}

			return result;
		}

		/// <summary>
		/// Hilfsfunktion, um simple .NET-Typen auf mögliche JSON-Schema-Typen abzubilden.
		/// </summary>
		private static string MapToJsonSchemaType(string dotNetTypeName)
		{
			switch (dotNetTypeName.ToLowerInvariant())
			{
				case "string":
					return "string";
				case "int32":
				case "int64":
				case "double":
				case "float":
				case "decimal":
					return "number";
				case "boolean":
					return "boolean";
				default:
					return "string"; // Fallback
			}
		}

		/// <summary>
		/// Entfernt im Text störende Zeichen und Backslashes, damit wir den Text sicher in Anführungszeichen einschließen können.
		/// </summary>
		private static string EscapeString(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}

			return text
				.Replace("\\", "\\\\")
				.Replace("\"", "\\\"");
		}

		/// <summary>
		/// Container für Parameterinformationen.
		/// </summary>
		private class ParameterInfo
		{
			public string Name { get; set; } = "";
			public string Type { get; set; } = "";
			public string DocComment { get; set; } = "";
			public List<string> EnumValues { get; set; } = new List<string>();
		}
	}
}
