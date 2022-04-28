using System.Runtime.CompilerServices;
using VerifyTests;

namespace Sample.Generators.Tests;

public static class ModuleInitializer
{
  [ModuleInitializer]
  public static void Init()
  {
    VerifySourceGenerators.Enable();
  }
}