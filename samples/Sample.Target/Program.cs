using Sample.Target;

var example1 = new Example(){ Name = "Alpha", Age = 20};
var example2 = new Example(){ Name = "Alpha", Age = 22};

var changes = example1.ExtractPropertyChangeRecords(example2);

foreach (var change in changes)
{
  Console.WriteLine($"Property '{change.PropertyName}' changed from '{change.OldValue}' to '{change.NewValue}'");
}
