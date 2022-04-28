namespace Codexcite.Generators.Model;

public record PropertyInformation
{
#pragma warning disable CS8618
  /// <summary>
  /// Property name
  /// </summary>
  public string Name { get; init; }
  /// <summary>
  /// Information about the type of the property
  /// </summary>
  public ReferencedTypeInformation Type { get; init; }
  /// <summary>
  /// All the interesting attributes applied to the property
  /// </summary>
  public AttributeInformation[] Attributes { get; init; }
  /// <summary>
  /// The property is readonly (no setter)
  /// </summary>
  public bool IsReadOnly { get; init; }
  
#pragma warning restore CS8618
}