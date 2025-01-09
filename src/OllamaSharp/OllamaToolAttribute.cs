namespace OllamaSharp;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class OllamaToolAttribute : Attribute
{
	public OllamaToolAttribute() { }
}

//// Extension-Klasse, um im Execute() auf IMethodSymbol zuzugreifen
//// (Sinnvoll: an passender Stelle in Execute() "fixen")
//public static class GeneratorExtensions
//{
//	public static void FixCandidateMethods(this OllamaToolGenerator.SyntaxReceiver receiver, Compilation compilation)
//	{
//		for (int i = 0; i < receiver.CandidateMethods.Count; i++)
//		{
//			// Falls wir den Placeholder null! gesetzt hatten, hier „reparieren“:
//			// Nicht in diesem Minimal-Beispiel gezeigt,
//			// weil wir das Symbolmatching gleich direkt beim Visit machen könnten.
//		}
//	}
//}