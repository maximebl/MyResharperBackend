using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Application.Parts;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Cpp.DeclaredElements;
using JetBrains.ReSharper.Feature.Services.Protocol;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Rider.Model;
using JetBrains.Util;
using JetBrains.ReSharper.Psi.Cpp;
using JetBrains.ReSharper.Psi.Cpp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Feature.Services.Cpp;

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
        model.GetFunctionNames.SetAsync((lt, filePath) =>
        {
            var names = new List<string>();
            
            MessageBox.ShowInfo("Entering SetAsync");
            using (ReadLockCookie.Create())
            {
                var path = VirtualFileSystemPath.Parse(filePath, InteractionContext.SolutionContext);
                var projectFile = solution.FindProjectItemsByLocation(path)
                    .OfType<IProjectFile>()
                    .FirstOrDefault();
                var psiSourceFile = projectFile.ToSourceFile();
                var psiFile = psiSourceFile.GetPrimaryPsiFile();
                
                if (psiFile is CppFile cppFile)
                {
                    foreach (var decl in cppFile.Descendants<IDeclaration>())
                    {
                        var element = decl.DeclaredElement;
                        var resolvedEntity = element.GetResolveEntityFromDeclaredElement();
                        var name = resolvedEntity.Name.ToString();
                        
                        MessageBox.ShowInfo("Found resolved entity: " + name);
                        names.Add(name);
                    }
                }
            }
            return Task.FromResult(names.ToArray());
        });
    }
}