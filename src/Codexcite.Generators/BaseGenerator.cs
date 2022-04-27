using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Codexcite.Generators;

public abstract class BaseGenerator<TDeclarationSyntax, TToGenerate> : IIncrementalGenerator where TDeclarationSyntax : MemberDeclarationSyntax
{
  protected abstract string MarkerAttribute { get; }
  protected abstract string[] InterestingAttributes { get; }

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    RegisterGlobalGeneratedCode(context);
    // Do a simple filter for types
    IncrementalValuesProvider<TDeclarationSyntax> enumDeclarations = context.SyntaxProvider
                                                                               .CreateSyntaxProvider(
                                                                                 (s, _) => IsSyntaxTargetForGeneration(s), // select enums with attributes
                                                                                 (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
                                                                               .Where(static m => m is not null)
                                                                                !; // filter out attributed enums that we don't care about

    // Combine the selected enums with the `Compilation`
    IncrementalValueProvider<(Compilation, ImmutableArray<TDeclarationSyntax>)> compilationAndTypes
      = context.CompilationProvider.Combine(enumDeclarations.Collect());

    // Generate the source using the compilation and enums
    context.RegisterSourceOutput(compilationAndTypes,
                                 (spc, source) => Execute(source.Item1, source.Item2, spc));
  }

  protected virtual void RegisterGlobalGeneratedCode(IncrementalGeneratorInitializationContext context)
  {
    //context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
    //                                                              "ValueChangeEntityAttribute.g.cs",
    //                                                              SourceText.From("AttributeHelper.Attribute", Encoding.UTF8)));
  }


  protected virtual void Execute(Compilation compilation, ImmutableArray<TDeclarationSyntax> types, SourceProductionContext context)
  {
    if (types.IsDefaultOrEmpty)
      // nothing to do yet
      return;

    // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
    var distinctEnums = types.Distinct();

    // Convert each EnumDeclarationSyntax to an EnumToGenerate
    var typesToGenerate = GetTypesToGenerate(compilation, distinctEnums, context.CancellationToken);

    // If there were errors in the EnumDeclarationSyntax, we won't create an
    // EnumToGenerate for it, so make sure we have something to generate
    if (typesToGenerate.Count > 0)
      // generate the source code and add it to the output
      foreach (var typeToGenerate in typesToGenerate)
      {
        var result = GenerateCodeForType(typeToGenerate);
        context.AddSource(GetGeneratedFileName(typeToGenerate), SourceText.From(result, Encoding.UTF8));
      }
  }

  protected abstract TToGenerate ExtractTypeToGenerate(INamedTypeSymbol typeSymbol);

  protected abstract string GenerateCodeForType(TToGenerate typeToGenerate);

  protected abstract string GetGeneratedFileName(TToGenerate typeToGenerate);

  protected virtual TDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
  {
    // we know the node is a ClassDeclarationSyntax thanks to IsSyntaxTargetForGeneration
    var typeDeclarationSyntax = (TDeclarationSyntax)context.Node;

    // loop through all the attributes on the method
    foreach (var attributeListSyntax in typeDeclarationSyntax.AttributeLists)
    {
      foreach (var attributeSyntax in attributeListSyntax.Attributes)
      {
        if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
          // weird, we couldn't get the symbol, ignore it
          continue;

        var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
        var fullName = attributeContainingTypeSymbol.ToDisplayString();

        // Is the attribute the [EnumExtensions] attribute?
        if (fullName == MarkerAttribute)
          // return the enum
          return typeDeclarationSyntax;
      }
    }

    // we didn't find the attribute we were looking for
    return null;
  }

  protected virtual List<TToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<TDeclarationSyntax> types, CancellationToken ct)
  {
    // Create a list to hold our output
    var typesToGenerate = new List<TToGenerate>();
    // Get the semantic representation of our marker attribute 
    var targetAttribute = compilation.GetTypeByMetadataName(MarkerAttribute);

    if (targetAttribute == null)
      // If this is null, the compilation couldn't find the marker attribute type
      // which suggests there's something very wrong! Bail out..
      return typesToGenerate;

    foreach (var typeDeclarationSyntax in types)
    {
      // stop if we're asked to
      ct.ThrowIfCancellationRequested();

      // Get the semantic representation of the enum syntax
      var semanticModel = compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
      if (semanticModel.GetDeclaredSymbol(typeDeclarationSyntax) is not INamedTypeSymbol typeSymbol)
        // something went wrong, bail out
        continue;

      var typeToGenerate = ExtractTypeToGenerate(typeSymbol);

      typesToGenerate.Add(typeToGenerate);
    }

    return typesToGenerate;
  }

  protected abstract bool IsSyntaxTargetForGeneration(SyntaxNode node);
}