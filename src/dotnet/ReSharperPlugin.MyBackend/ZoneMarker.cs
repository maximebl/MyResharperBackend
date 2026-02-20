using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;

namespace ReSharperPlugin.MyBackend;

[ZoneMarker]
public class ZoneMarker : IRequire<IProjectModelZone>, IRequire<PsiFeaturesImplZone>
{
}