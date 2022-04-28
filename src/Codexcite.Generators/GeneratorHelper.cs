using Codexcite.Generators.Model;
using Microsoft.CodeAnalysis;

namespace Codexcite.Generators;

public static class GeneratorHelper
{
  public const string NullableStart = "Nullable<";

  public static readonly string[] ConvertibleTypes =
  {
    nameof(String),
    nameof(Boolean),
    nameof(Int16),
    nameof(Int32),
    nameof(Int64),
    nameof(Double),
    nameof(Decimal),
    nameof(DateTime),
    nameof(Byte),
    nameof(Char),
    nameof(SByte),
    nameof(Single),
    nameof(UInt16),
    nameof(UInt32),
    nameof(UInt64),

    $"System.{nameof(String)}",
    $"System.{nameof(Boolean)}",
    $"System.{nameof(Int16)}",
    $"System.{nameof(Int32)}",
    $"System.{nameof(Int64)}",
    $"System.{nameof(Double)}",
    $"System.{nameof(Decimal)}",
    $"System.{nameof(DateTime)}",
    $"System.{nameof(Byte)}",
    $"System.{nameof(Char)}",
    $"System.{nameof(SByte)}",
    $"System.{nameof(Single)}",
    $"System.{nameof(UInt16)}",
    $"System.{nameof(UInt32)}",
    $"System.{nameof(UInt64)}"
};

  public static AttributeInformation ExtractAttributeInfo(AttributeData attributeData)
  {
    var className = attributeData.AttributeClass is null ? string.Empty : GetFullTypeName(attributeData.AttributeClass);

    var namedArguments = new List<KeyValuePair<string, object?>>();
    foreach (var argument in attributeData.NamedArguments)
      if (TryGetArgumentValue(argument.Value, out var value))
        namedArguments.Add(new KeyValuePair<string, object?>(argument.Key, value));

    var ctorArguments = new List<KeyValuePair<int, object?>>();
    for (var i = 0; i < attributeData.ConstructorArguments.Length; i++)
      if (TryGetArgumentValue(attributeData.ConstructorArguments[i], out var value))
        ctorArguments.Add(new KeyValuePair<int, object?>(i, value));

    return new AttributeInformation(className, namedArguments.ToArray(), ctorArguments.ToArray());
  }

  public static AttributeInformation[] ExtractAttributeInfos(IEnumerable<AttributeData> attributes,
                                                      string[] interestingAttributes,
                                                      bool allAttributes = false)
  {
    var output = new List<AttributeInformation>();
    foreach (var attributeData in attributes)
    {
      if (attributeData.AttributeClass is null)
        continue;
      var name = GetFullTypeName(attributeData.AttributeClass);
      if (allAttributes || interestingAttributes.Contains(name))
        output.Add(ExtractAttributeInfo(attributeData));
    }

    return output.ToArray();
  }

  public static ITypeSymbol ExtractTypeIfNullable(ITypeSymbol typeSymbol)
  => typeSymbol switch
  {
    INamedTypeSymbol { IsGenericType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T } namedTypeSymbol =>
      namedTypeSymbol.TypeArguments.FirstOrDefault() ?? typeSymbol,
    //IArrayTypeSymbol arrayTypeSymbol => arrayTypeSymbol.ElementType,
    _                                => typeSymbol
  };


  public static string ExtractTypeIfNullable(string typeName)
    => typeName.StartsWith(NullableStart) ? typeName.Substring(NullableStart.Length, typeName.Length - NullableStart.Length - 1) : typeName;

  public static string GetFullNamespace(INamespaceSymbol? namespaceSymbol)
    => namespaceSymbol is null
         ? string.Empty
         : namespaceSymbol.IsGlobalNamespace || namespaceSymbol.ContainingNamespace.IsGlobalNamespace
           ? namespaceSymbol.Name
           : $"{GetFullNamespace(namespaceSymbol.ContainingNamespace)}.{namespaceSymbol.Name}";

  public static string GetFullTypeName(ITypeSymbol typeSymbol)
  {
    var fullNamespace = GetFullNamespace(typeSymbol.ContainingNamespace);
    return $"{fullNamespace}{".".If(!string.IsNullOrEmpty(fullNamespace))}{GetTypeName(typeSymbol)}";
  }

  public static string GetTypeName(ITypeSymbol typeSymbol)
    => typeSymbol switch
       {
         INamedTypeSymbol { IsGenericType: true } namedTypeSymbol =>
           $"{typeSymbol.Name}<{string.Join(",", namedTypeSymbol.TypeArguments.Select(GetFullTypeName))}>",
         IArrayTypeSymbol arrayTypeSymbol => $"{GetFullTypeName(arrayTypeSymbol.ElementType)}{"[]".Times(arrayTypeSymbol.Rank)}",
         _                                => typeSymbol.Name
       };

  public static string If(this string target, bool condition) => condition ? target : string.Empty;
  public static string Times(this string target, int times) => string.Concat(Enumerable.Range(0, times).Select(x => target));

  public static bool IsNullableT(ITypeSymbol typeSymbol)
    => typeSymbol is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T };


  public static string ToKeyword(this Accessibility accessibility)
    => accessibility switch
       {
         Accessibility.Internal             => "internal",
         Accessibility.Private              => "private",
         Accessibility.Protected            => "protected",
         Accessibility.Public               => "public",
         Accessibility.ProtectedAndInternal => "private protected",
         Accessibility.ProtectedOrInternal  => "protected internal",
         _                                  => string.Empty
       };

  public static bool TryGetArgumentValue(TypedConstant argumentValue, out object? value)
  {
    value = null;
    if (argumentValue.Kind == TypedConstantKind.Error)
      return false;
    value = argumentValue.Kind == TypedConstantKind.Array
              ? argumentValue.Values.Select(x => x.Value).ToArray()
              : argumentValue.Value;
    return true;
  }

  public static string? TrimStart(this string? target, string textToTrim)
  {
    if (target == null || !target.StartsWith(textToTrim, StringComparison.InvariantCulture))
      return target;

    return target.Remove(0, textToTrim.Length);
  }
}