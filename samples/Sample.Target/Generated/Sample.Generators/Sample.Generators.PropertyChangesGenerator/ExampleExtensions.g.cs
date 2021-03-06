#nullable enable
using System;
using Sample.Generators.Shared;
using Sample.Generators.Shared.Exceptions;
using System.Collections.Generic;
using System.Linq;
namespace Sample.Target
{
	public static partial class ExampleExtensions
	{

		public static IEnumerable<PropertyChangedRecord> ExtractPropertyChangeRecords(this Example target, Example original)
		{
			var changes = new List<PropertyChangedRecord>(3);
			if (!target.Name.NullableEquals(original.Name))
				changes.Add(new PropertyChangedRecord(nameof(Example.Name), target.Name, original.Name));
			if (!target.Age.NullableEquals(original.Age))
				changes.Add(new PropertyChangedRecord(nameof(Example.Age), target.Age, original.Age));
			return changes;
		}

	}
}
