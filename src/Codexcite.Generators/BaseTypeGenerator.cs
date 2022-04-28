using Codexcite.Generators.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codexcite.Generators;

public abstract class BaseTypeGenerator<TToGenerate> : BaseGenerator<TypeDeclarationSyntax, TToGenerate> where TToGenerate : TypeInformation, new()
{
  protected override TToGenerate ExtractTypeToGenerate(INamedTypeSymbol typeSymbol)
  {
    var typeName = typeSymbol.Name;
    var typeNamespace = GeneratorHelper.GetFullNamespace(typeSymbol.ContainingNamespace);

    // Get all the members in the type
    var typeMembers = typeSymbol.GetMembers();
    var members = new List<PropertyInformation>(typeMembers.Length);

    // Get all the properties from the type
    foreach (var member in typeMembers)
      if (member is IPropertySymbol { IsReadOnly: false } propertySymbol)
      {
        var propertyType = GeneratorHelper.GetFullTypeName(propertySymbol.Type);
        var extracted = GeneratorHelper.ExtractTypeIfNullable(propertySymbol.Type);
        var isEnumerable = propertySymbol.Type.SpecialType != SpecialType.System_String &&
                           propertySymbol.Type.AllInterfaces.Any(x => x.Name == "IEnumerable");

        members.Add(new PropertyInformation
                    {
                      Name = propertySymbol.Name,
                      Type = new ReferencedTypeInformation
                             {
                               Type = propertyType,
                               TypeWithoutNullable = GeneratorHelper.GetFullTypeName(extracted),
                               IsNullableAnnotated = propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated,
                               IsEnum = extracted.TypeKind == TypeKind.Enum,
                               IsNullableT = GeneratorHelper.IsNullableT(propertySymbol.Type),
                               IsValueType = extracted.IsValueType,
                               TypeNamespace = GeneratorHelper.GetFullNamespace(propertySymbol.Type.ContainingNamespace),
                               TypeWithoutNullableNamespace =
                                 GeneratorHelper.GetFullNamespace(extracted is IArrayTypeSymbol arrayTypeSymbol
                                                                    ? arrayTypeSymbol.ElementType.ContainingNamespace
                                                                    : extracted.ContainingNamespace),
                               IsEnumerable = isEnumerable
                             },
                      Attributes = GeneratorHelper.ExtractAttributeInfos(propertySymbol.GetAttributes(), InterestingAttributes),
                      IsReadOnly = propertySymbol.IsReadOnly
                    });
      }

    var typeToGenerate = new TToGenerate
                         {
                           Namespace = typeNamespace,
                           Name = typeName,
                           Properties = members.ToArray(),
                           Attributes = GeneratorHelper.ExtractAttributeInfos(typeSymbol.GetAttributes(), InterestingAttributes),
                           Accessibility = typeSymbol.DeclaredAccessibility
                         };
    return typeToGenerate;
  }

  protected override string GetGeneratedFileName(TToGenerate typeToGenerate) 
    => $"{typeToGenerate.Name}Extensions.g.cs";


  /// <summary>
  ///   By default selecting class, struct and record with at least one attribute.
  ///   Override to change the focus of the types to be targeted.
  /// </summary>
  /// <param name="node"></param>
  /// <returns></returns>
  protected override bool IsSyntaxTargetForGeneration(SyntaxNode node)
    => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 }
         or StructDeclarationSyntax { AttributeLists.Count: > 0 }
         or RecordDeclarationSyntax { AttributeLists.Count: > 0 };
}