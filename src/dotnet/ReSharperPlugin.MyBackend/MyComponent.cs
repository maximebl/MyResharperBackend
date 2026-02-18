using System.Linq;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Application.Parts;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Protocol;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Rider.Model;
using JetBrains.Util;
//using JetBrains.ReSharper.Psi.Cpp;

namespace ReSharperPlugin.MyBackend;

[ZoneMarker]
public class ZoneMarker : IRequire<IProjectModelZone>
{
}

[SolutionComponent(Instantiation.ContainerAsyncAnyThreadSafe)]
public class MyComponent
{
    public MyComponent(Lifetime lifetime, ISolution solution)
    {
        var model = solution.GetProtocolSolution().GetMyBackendModel();
        model.Mycoolvalue.Advise(lifetime, value =>
        {
            MessageBox.ShowInfo(value + " from c#");
        });

        // Handle a frontend "request": give me all function names in a file
        // model.GetFunctionNames.SetAsync((lt, filePath) =>
        // {
        //     using (ReadLockCookie.Create())
        //     {
        //         var path = VirtualFileSystemPath.Parse(filePath, InteractionContext.SolutionContext);
        //         var projectFile = solution.FindProjectItemsByLocation(path)
        //             .OfType<IProjectFile>()
        //             .FirstOrDefault();
        //         var psiSourceFile = projectFile.ToSourceFile();
        //         var psiFile = psiSourceFile.GetPrimaryPsiFile();
        //         
        //         
        //     }
        //
        //     return null;
        // });
    }
}