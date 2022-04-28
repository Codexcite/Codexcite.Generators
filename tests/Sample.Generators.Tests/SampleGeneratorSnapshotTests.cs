
namespace Sample.Generators.Tests;

[UsesVerify]
public class SampleGeneratorSnapshotTests
{
  [Fact]
  public Task GeneratesClassExtensionsCorrectly()
  {
    // The source code to test
    var source = @"
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
";

    var (diagnostics, output) = TestHelper.GetGeneratedOutput<PropertyChangesGenerator>(source);

    Assert.Empty(diagnostics);
    return Verify(output).UseDirectory("Snapshots");
  }

  
}