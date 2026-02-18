using System.Threading;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

namespace ReSharperPlugin.MyBackend.Tests
{
    [ZoneDefinition]
    public class MyBackendTestEnvironmentZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>, IRequire<IMyBackendZone>
    {
    }

    [ZoneMarker]
    public class ZoneMarker : IRequire<ICodeEditingZone>, IRequire<ILanguageCSharpZone>,
        IRequire<MyBackendTestEnvironmentZone>
    {
    }

    [SetUpFixture]
    public class MyBackendTestsAssembly : ExtensionTestEnvironmentAssembly<MyBackendTestEnvironmentZone>
    {
    }
}