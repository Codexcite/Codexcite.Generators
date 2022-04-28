# Codexcite.Generators

Base Generators and helper functions for Source Generators.

## Usage
Basic usage - just inherit from BaseTypeGenerator, define the marker attribute to look for and customize the code to be generated for each target type.
```csharp
using Codexcite.Generators;
using Microsoft.CodeAnalysis;
using Sample.Generators.Shared;

namespace Sample.Generators;

[Generator]
public class PropertyChangesGenerator : BaseTypeGenerator<TypeInformation>
{
  protected override string MarkerAttribute => typeof(SampleMarkerAttribute).FullName;
  private static readonly string IgnorePropertyAttribute = typeof(SampleIgnoreAttribute).FullName;

  protected override string[] InterestingAttributes => new[] { MarkerAttribute, IgnorePropertyAttribute };

  protected override string GenerateCodeForType(TypeInformation typeInformation)
  { 
    // generate the necessary code for each one of the target types 
    return string.Empty;
  }
}
```

Focus only on specific types, for example only records:

```
  protected override bool IsSyntaxTargetForGeneration(SyntaxNode node)
    => node is RecordDeclarationSyntax { AttributeLists.Count: > 0 };
```
    
There will be a file generated for each target type. You can customize the name of the generated file.

```
  protected override string GetGeneratedFileName(TypeInformation typeToGenerate) 
    => $"{typeToGenerate.Name}Extensions.g.cs";
```

You can generate code that is global (not specific to each target type). Examples include common classes or attributes.

```

  protected override void RegisterGlobalGeneratedCode(IncrementalGeneratorInitializationContext context)
  {
    context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                                                              "GlobalGeneratedCode.g.cs",
                                                              SourceText.From("[code goes here]", Encoding.UTF8)));
  }
```

Use TypeInformation data to generate the necessary code:

```
  private static string GenerateExtensionClass(TypeInformation typeToGenerate)
  {
    var sb = new StringBuilder();
    sb.AppendLine("#nullable enable");
    sb.AppendLine("using System;");
    sb.AppendLine("using Sample.Generators.Shared;");
    sb.AppendLine("using Sample.Generators.Shared.Exceptions;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using System.Linq;");
    sb.AppendLine($"namespace {typeToGenerate.Namespace}"); // match the target type namespace
    sb.AppendLine("{");
    sb.AppendLine($"\tpublic static partial class {typeToGenerate.Name}Extensions"); // use the target type name
    sb.AppendLine("\t{");
    
    sb.AppendLine();
    // iterate through the target type's properties to generate code
    GenerateExtractValueChangeRequests(sb, typeToGenerate);
    sb.AppendLine();
    
    sb.AppendLine("\t}");
    sb.AppendLine("}");
    return sb.ToString();
  }
```

Generate one or more methods using the available TypeInformation:

```
  private static void GenerateExtractValueChangeRequests(StringBuilder sb, TypeInformation typeToGenerate)
  {
    sb.AppendLine($"\t\tpublic static IEnumerable<PropertyChangedRecord> ExtractPropertyChangeRecords(this {typeToGenerate.Name} target, " +
                  $"{typeToGenerate.Name} original)");
    sb.AppendLine("\t\t{");
    sb.AppendLine($"\t\t\tvar changes = new List<PropertyChangedRecord>({typeToGenerate.Properties.Length});");
    foreach (var member in typeToGenerate.Properties)
    {
      // skip properties marked with the ignore attribute
      if (member.Attributes.All(x => x.ClassName != IgnorePropertyAttribute)) 
      {
        sb.AppendLine($"\t\t\tif (!target.{member.Name}.{(member.Type.IsEnumerable ? "SequenceNullableEquals" : "NullableEquals")}(original.{member.Name}))");
        sb.AppendLine($"\t\t\t\tchanges.Add(new PropertyChangedRecord(nameof({typeToGenerate.Name}.{member.Name}), target.{member.Name}, original.{member.Name}));");
      }
    }

    sb.AppendLine("\t\t\treturn changes;");
    sb.AppendLine("\t\t}");
  }
```

## Generator project configuration
Create a .Net Standard 2.0 project.
```
 <!-- Source generators must target netstandard 2.0 -->
 <TargetFramework>netstandard2.0</TargetFramework>
 <!-- We don't want to reference the source generator dll directly in consuming projects -->
 <IncludeBuildOutput>false</IncludeBuildOutput>
```

Reference the Codexcite.Generators nuget. Set PrivateAssets="all".
```
  <!-- The following libraries include the source generator interfaces and types we need -->
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.3" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.1.0" PrivateAssets="all" />
    <PackageReference Include="Codexcite.Generators" Version="1.1.3" PrivateAssets="all" />
  </ItemGroup>
```
If you use a shared project for the marker attributes, reference it too.
```
  <ItemGroup>
    <ProjectReference Include="..\Sample.Generators.Shared\Sample.Generators.Shared.csproj" PrivateAssets="All" />
  </ItemGroup>
```
## Nuget package configuration
You'll probably want to pack your generator in a nuget package. There are several important steps to follow.
Basic nuget package configuration. "CopyLocalLockFileAssemblies" ensures that all assemblies are copied to the output folder.
```
<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
<PackageId>Sample.Generators</PackageId>
<Description>Example of source generators created using Codexcite.Generators package.</Description>
<Version>1.0.0</Version>
<PackageReleaseNotes>
Sample.Generators 1.0.0 - Initial version.
PackageReleaseNotes>
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
```
Include the generators assembly in the package as an analyzer.
```
<!-- Pack the generator dll in the analyzers/dotnet/cs path -->
<None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
```
The analyzers do not have access to the referenced assemblies of the code they are analyzing, so you must include any referenced dlls in the package.
```
    <!-- Pack the referenced dlls in the analyzers/dotnet/cs path -->
    <None Include="$(OutputPath)\Codexcite.Generators.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
    <None Include="$(OutputPath)\Sample.Generators.Shared.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
```
That is enough for running the analyzer locally, but sometimes you also need to include all the framework dlls if you want to run the analyzer as part of a CI build.
```
<!-- Pack the default referenced dlls in the analyzers/dotnet/cs path - workaround for running analyzer during CI/CD pipeline -->
<None Include="$(OutputPath)\Microsoft.CodeAnalysis.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
<None Include="$(OutputPath)\Microsoft.CodeAnalysis.CSharp.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />

<None Include="$(OutputPath)\System.Buffers.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
<None Include="$(OutputPath)\System.Collections.Immutable.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
<None Include="$(OutputPath)\System.Memory.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
<None Include="$(OutputPath)\System.Numerics.Vectors.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
<None Include="$(OutputPath)\System.Reflection.Metadata.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
<None Include="$(OutputPath)\System.Runtime.CompilerServices.Unsafe.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
<None Include="$(OutputPath)\System.Text.Encoding.CodePages.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
<None Include="$(OutputPath)\System.Threading.Tasks.Extensions.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
```

Finally, if you use a shared dll, include it in the lib folder, so the target assembly can reference it.
```
    <!-- Also pack the shared dll in the lib\netstandard2.0 path - this allows it to be referenced by the target assembly -->
    <None Include="$(OutputPath)\Sample.Generators.Shared.dll" Pack="true" PackagePath="lib\netstandard2.0" Visible="true" />
```

## Target project configuration

Reference your generator nuget as an Analyzer. All the dependencies should be included.
```
<PackageReference Include="Sample.Analyzer" Version="1.0.0" OutputItemType="Analyzer" ReferenceOutputAssembly="false"/>
```
If you prefer to reference it as a project reference, then you need to also reference the dependencies. Notice the Shared project has ReferenceOutputAssembly="true" so that the assembly can be used in the target code.
```
<PackageReference Include="Codexcite.Generators" Version="1.1.3" OutputItemType="Analyzer" ReferenceOutputAssembly="false"/>
<ProjectReference Include="..\Sample.Generators.Shared\Sample.Generators.Shared.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="true" />
<ProjectReference Include="..\Sample.Generators\Sample.Generators.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```
Highly recommended: turn on EmitCompilerGeneratedFiles so the generated code is also written to files and you can commit it to source control. The files will be generated in folders named using the generator assembly and class name, inside a folder you can customize.
```
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
```
Make sure to exclude the generated files from compilation, otherwise you'll get duplicated code errors.
```
  <ItemGroup>
    <Compile Remove="Generated/**/*.cs" />
  </ItemGroup>
  <ItemGroup>
    <None Include="Generated/**/*.cs" />
  </ItemGroup>
```
If the analyzer doesn't run on the CI pipeline on Azure DevOps, you can use the generated files to compile as normal by setting a condition for TF_BUILD.
```
  <ItemGroup>
    <!-- Condition for when building in Azure Devops Pipeline-->
    <Compile Remove="Generated/**/*.cs" Condition="'$(TF_BUILD)' != 'true'" />
  </ItemGroup>
```

## Advanced use
You can inherit directly from BaseGenerator<TDeclarationSyntax, TToGenerate> in order to fully customize your source generation.
- TDeclarationSyntax: the type of MemberDeclarationSyntax that will be handled by this generator. Usually is TypeDeclarationSyntax.
- TToGenerate: the intermediate type containing information about the target member used for generation. For example, Codexcite.Generators.Model.TypeInformation.
BaseGenerator<TDeclarationSyntax, TToGenerate> has 3 customizable steps used in source generation.
1. Quick filtering of SyntaxNode elements to select the potential candidates for generation. Override IsSyntaxTargetForGeneration to customize.
```
protected override bool IsSyntaxTargetForGeneration(SyntaxNode node)
  => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 }
       or StructDeclarationSyntax { AttributeLists.Count: > 0 }
       or RecordDeclarationSyntax { AttributeLists.Count: > 0 };
```
2. Extracting information about the target and saving it in a TToGenerate object.
```
protected override TToGenerate ExtractTypeToGenerate(INamedTypeSymbol typeSymbol)
{
  // extract information here
}
```
3. Generate code, based on the information contained in the TToGenerate
```
protected override string GenerateCodeForType(TToGenerate type)
{ 
  // generate the necessary code for the target type 
}
```