namespace Sample.Generators.Shared.Exceptions;

public class SampleException : Exception
{
  public SampleException(string propertyName, Type objectType, string message) : base(message)
  {
    PropertyName = propertyName;
    ObjectType = objectType;
  }

  public string PropertyName { get; set; }
  public Type ObjectType { get; set; }

  public override string ToString() => $"{base.ToString()} Type: {ObjectType.Name} Property: {PropertyName}";
}