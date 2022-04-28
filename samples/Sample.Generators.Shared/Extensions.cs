using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Generators.Shared
{
  public static class Extensions
  {
    public static bool SequenceNullableEquals<T>(this IEnumerable<T>? source, IEnumerable<T>? other)
      => other == null ? source == null : source?.SequenceEqual(other) == true;

    public static bool NullableEquals(this object? source, object? target) => source?.Equals(target) ?? target is null;
  }
}
