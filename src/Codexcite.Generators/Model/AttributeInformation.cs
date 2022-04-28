namespace Codexcite.Generators.Model;

public record AttributeInformation(string ClassName,
                                   KeyValuePair<string, object?>[] NamedArguments,
                                   KeyValuePair<int, object?>[] ConstructorArguments);