using System.Text;
using Codexcite.Generators;
using Codexcite.Generators.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Sample.Generators.Shared;

namespace Sample.Generators;

[Generator]
public class PropertyChangesGenerator : BaseTypeGenerator<TypeInformation>
{
  protected override string MarkerAttribute => typeof(SampleMarkerAttribute).FullName;
  private static readonly string IgnorePropertyAttribute = typeof(SampleIgnoreAttribute).FullName;

  protected override string[] InterestingAttributes => new[] { MarkerAttribute, IgnorePropertyAttribute };

  protected override string GenerateCodeForType(TypeInformation type)
  { 
    // generate the necessary code for the target type 
    return GenerateExtensionClass(type);
  }

  protected override bool IsSyntaxTargetForGeneration(SyntaxNode node)
    => node is RecordDeclarationSyntax { AttributeLists.Count: > 0 };

  protected override void RegisterGlobalGeneratedCode(IncrementalGeneratorInitializationContext context)
  {
    //context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
    //                                                          "GlobalGeneratedCode.g.cs",
    //                                                          SourceText.From("[code goes here]", Encoding.UTF8)));
  }

  private static string GenerateExtensionClass(TypeInformation typeToGenerate)
  {
    var sb = new StringBuilder();
    sb.AppendLine("#nullable enable");
    sb.AppendLine("using System;");
    sb.AppendLine("using Sample.Generators.Shared;");
    sb.AppendLine("using Sample.Generators.Shared.Exceptions;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using System.Linq;");
    sb.AppendLine($"namespace {typeToGenerate.Namespace}");
    sb.AppendLine("{");
    sb.AppendLine($"\tpublic static partial class {typeToGenerate.Name}Extensions");
    sb.AppendLine("\t{");
    
    sb.AppendLine();
    GenerateExtractValueChangeRequests(sb, typeToGenerate);
    sb.AppendLine();
    

    sb.AppendLine("\t}");
    sb.AppendLine("}");
    return sb.ToString();
  }

  private static void GenerateExtractValueChangeRequests(StringBuilder sb, TypeInformation typeToGenerate)
  {
    sb.AppendLine($"\t\tpublic static IEnumerable<PropertyChangedRecord> ExtractPropertyChangeRecords(this {typeToGenerate.Name} target, " +
                  $"{typeToGenerate.Name} original)");
    sb.AppendLine("\t\t{");
    sb.AppendLine($"\t\t\tvar changes = new List<PropertyChangedRecord>({typeToGenerate.Properties.Length});");
    foreach (var member in typeToGenerate.Properties)
    {
      if (member.Attributes.All(x => x.ClassName != IgnorePropertyAttribute))
      {
        sb.AppendLine($"\t\t\tif (!target.{member.Name}.{(member.Type.IsEnumerable ? "SequenceNullableEquals" : "NullableEquals")}(original.{member.Name}))");
        sb.AppendLine($"\t\t\t\tchanges.Add(new PropertyChangedRecord(nameof({typeToGenerate.Name}.{member.Name}), target.{member.Name}, original.{member.Name}));");
      }
    }

    sb.AppendLine("\t\t\treturn changes;");
    sb.AppendLine("\t\t}");
  }
}