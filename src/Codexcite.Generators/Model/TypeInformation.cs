using Microsoft.CodeAnalysis;

namespace Codexcite.Generators.Model;

public record TypeInformation
{
#pragma warning disable CS8618
  /// <summary>
  /// Target type namespace
  /// </summary>
  public string Namespace { get; init; }
  /// <summary>
  /// Target type name
  /// </summary>
  public string Name { get; init; }
  /// <summary>
  /// The information for all the target type's properties
  /// </summary>
  public PropertyInformation[] Properties { get; init; }
  /// <summary>
  /// All the interesting attributes applied to the target type
  /// </summary>
  public AttributeInformation[] Attributes { get; init; }
  /// <summary>
  /// The accessibility level of the target type (private, protected, public...)
  /// </summary>
  public Accessibility Accessibility { get; init; }
#pragma warning restore CS8618
}