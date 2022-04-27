using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Codexcite.Generators;

public record TypeToGenerate
{
#pragma warning disable CS8618
  public string Namespace { get; init; }
  public string Name { get; init; }
  public PropertyToGenerate[] Properties { get; init; }
  public AttributeInfo[] Attributes { get; init; }
  public Accessibility Accessibility { get; init; }
#pragma warning restore CS8618
}

public record PropertyToGenerate
{
#pragma warning disable CS8618
  public string Name { get; init; }
  public string TypeWithoutNullable { get; init; }
  public string Type { get; init; }
  public string TypeWithoutNullableNamespace { get; init; }
  public string TypeNamespace { get; init; }
  public bool IsNullableAnnotated { get; init; }
  public bool IsEnum { get; init; }
  public bool IsValueType { get; init; }
  public bool IsNullableT { get; init; }
  public AttributeInfo[] Attributes { get; init; }
  public bool IsReadOnly { get; init; }
  public bool IsEnumerable { get; set; }
  
#pragma warning restore CS8618
}

public record AttributeInfo(string ClassName,
                            KeyValuePair<string, object?>[] NamedArguments,
                            KeyValuePair<int, object?>[] ConstructorArguments);