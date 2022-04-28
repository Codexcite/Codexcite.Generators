namespace Codexcite.Generators.Model;

public record ReferencedTypeInformation
{
#pragma warning disable CS8618
  /// <summary>
  /// Original type name, ex: string or Nullable&lt;int&gt;
  /// </summary>
  public string Type { get; init; }

  /// <summary>
  /// The namespace for the Type
  /// </summary>
  public string TypeNamespace { get; init; }

  /// <summary>
  /// Extracted type name, if original is Nullable. Ex: Nullable&lt;int&gt; turns into int.
  /// Only applies to Nullable&lt;&gt; value types, not annotated nullables like string?.
  /// </summary>
  public string TypeWithoutNullable { get; init; }
  /// <summary>
  /// The namespace for the extracted TypeWithoutNullable
  /// </summary>
  public string TypeWithoutNullableNamespace { get; init; }
  /// <summary>
  /// True if the type is nullable annotated, like "string?".
  /// </summary>
  public bool IsNullableAnnotated { get; init; }
  /// <summary>
  /// True of the type is an Enum.
  /// </summary>
  public bool IsEnum { get; init; }
  /// <summary>
  /// True if the type is a value type (struct).
  /// </summary>
  public bool IsValueType { get; init; }
  /// <summary>
  /// True if the type is a Nullable&lt;&gt; value type (not annotated nullable like "string?").
  /// </summary>
  public bool IsNullableT { get; init; }
  /// <summary>
  /// True if the type implements IEnumerable&lt;&gt;.
  /// </summary>
  public bool IsEnumerable { get; set; }

#pragma warning restore CS8618
}