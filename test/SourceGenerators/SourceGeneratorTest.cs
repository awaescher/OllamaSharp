using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using Shouldly;

namespace Tests.SourceGenerators;

/// <summary>
/// Base class for source generator tests providing utilities to run generators and invoke generated tools.
/// </summary>
public abstract class SourceGeneratorTest
{
	protected static SourceGeneratorResult RunGenerator(string source, bool allowErrors = false)
	{
		var generator = new ToolSourceGenerator();

		source = """
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using OllamaSharp;

	""" + source;

		var syntaxTree = CSharpSyntaxTree.ParseText(source);

		var compilation = CSharpCompilation.Create(
			assemblyName: "Tests",
			syntaxTrees: [syntaxTree]);

		var driver = (GeneratorDriver)CSharpGeneratorDriver.Create(generator);
		driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics);
		var generatedTrees = updatedCompilation.SyntaxTrees.Skip(1).ToList(); // Skip original input

		var hasErrors = diagnostics.Any();

		if (hasErrors && allowErrors)
			return new SourceGeneratorResult(GeneratedCode: "", GeneratedTool: null, diagnostics);

		diagnostics.ShouldBeEmpty("there should be no compilation errors");
		generatedTrees.Count.ShouldBe(1, "only one file should be generated");

		var generatedCode = generatedTrees[0].ToString();

		// add System and OllamaSharp references
		var references = AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(a => (a.FullName?.StartsWith("System.") ?? false) || (a.FullName?.Contains("OllamaSharp") ?? false))
			.Select(a => MetadataReference.CreateFromFile(a.Location))
			.ToList();

		var finalCompilation = CSharpCompilation.Create(
			"GeneratedAssembly",
			syntaxTrees: updatedCompilation.SyntaxTrees, // compiles the original source and the generated source
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using var stream = new MemoryStream();
		var emitResult = finalCompilation.Emit(stream);
		var compileerrors = emitResult.Diagnostics.Where(d => d.WarningLevel == 0);
		compileerrors.ShouldBeEmpty();

		// load dynamic assembly
		stream.Seek(0, SeekOrigin.Begin);
		var generatedAssembly = Assembly.Load(stream.ToArray());
		var generatedToolType = generatedAssembly.GetTypes().Single(t => t.Name.EndsWith("Tool")); // find generated tool
		var tool = Activator.CreateInstance(generatedToolType) as Tool;

		return new SourceGeneratorResult(generatedCode, tool, diagnostics);
	}

	protected static async Task<object?> InvokeTool(SourceGeneratorResult sourceGeneratorResult, Dictionary<string, object?>? args = default)
	{
		if (sourceGeneratorResult.GeneratedTool is OllamaSharp.Tools.IInvokableTool t)
			return t.InvokeMethod(args);
		else if (sourceGeneratorResult.GeneratedTool is OllamaSharp.Tools.IAsyncInvokableTool at)
			return await at.InvokeMethodAsync(args);
		else
			throw new NotSupportedException("Tool is not IInvokableTool or IAsyncInvokableTool");
	}

	/// <summary>
	/// Represents the result of running a source generator, including generated code,
	/// the instantiated tool, and any diagnostics produced.
	/// </summary>
	/// <param name="GeneratedCode">The source code that was generated.</param>
	/// <param name="GeneratedTool">An instance of the generated <see cref="Tool"/>, or <c>null</c> if creation failed.</param>
	/// <param name="Diagnostics">A collection of diagnostics emitted during generation.</param>
	public record SourceGeneratorResult(string GeneratedCode, Tool? GeneratedTool, IEnumerable<Diagnostic> Diagnostics);
}