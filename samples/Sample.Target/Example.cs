using Sample.Generators.Shared;

namespace Sample.Target
{
  [SampleMarker]
  public record Example
  {
    public string? Name { get; set; }
    public int Age { get; set; }
    [SampleIgnore]
    public double? Height { get; set; }
  }
}