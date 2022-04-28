namespace Sample.Generators.Shared;

public record PropertyChangedRecord(
  string PropertyName,
  object? NewValue,
  object? OldValue);