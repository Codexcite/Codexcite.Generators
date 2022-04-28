using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sample.Generators.Shared;
using VerifyXunit;

namespace Sample.Generators.Tests;

public static class TestHelper
{
  public static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(string source, params Type[] additionalTypes)
    where T : IIncrementalGenerator, new()
  {
    var syntaxTree = CSharpSyntaxTree.ParseText(source);
    var references = AppDomain.CurrentDomain.GetAssemblies()
                              .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
                              .Select(_ => MetadataReference.CreateFromFile(_.Location))
                              .Concat(new[]
                                      {
                                        MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                                        MetadataReference.CreateFromFile(typeof(SampleMarkerAttribute).Assembly.Location)
                                      })
                              .Concat(additionalTypes.Select(x=>MetadataReference.CreateFromFile(x.Assembly.Location)));

    var compilation = CSharpCompilation.Create(
                                               "generator",
                                               new[] { syntaxTree },
                                               references,
                                               new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    var originalTreeCount = compilation.SyntaxTrees.Length;
    var generator = new T();

    var driver = CSharpGeneratorDriver.Create(generator);
    driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

    var trees = outputCompilation.SyntaxTrees.ToList();

    return (diagnostics, trees.Count != originalTreeCount ? trees[trees.Count - 1].ToString() : string.Empty);
  }
}