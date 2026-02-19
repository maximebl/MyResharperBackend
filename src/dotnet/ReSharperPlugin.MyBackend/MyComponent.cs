using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Application.Parts;
using JetBrains.Application.Progress;
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
using JetBrains.ReSharper.Psi.Cpp.Language;
using JetBrains.ReSharper.Psi.Cpp.Symbols;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Search;

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
        model.Mycoolvalue.Advise(lifetime, value => { MessageBox.ShowInfo(value + " from c#"); });

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
                var finder = solution.GetPsiServices().SingleThreadedFinder;
                var searchDomain = solution.GetPsiServices().SearchDomainFactory.CreateSearchDomain(solution, true);

                if (psiFile is CppFile cppFile)
                {
                    foreach (var decl in cppFile.Descendants<IDeclaration>())
                    {
                        var element = decl.DeclaredElement;
                        ICppResolveEntity resolvedEntity = element.GetResolveEntityFromDeclaredElement();

                        if (element is ICppDeclaredElement cppElement)
                        {
                            var name = cppElement.ShortName;
                            var type = cppElement.GetElementType().PresentableName;
                            var complexOffset = cppElement.GetSymbolLocation().ComplexOffset;
                            var textOffset = cppElement.GetSymbolLocation().TextOffset;
                            var locateOffset = cppElement.GetSymbolLocation().LocateTextOffset();
                            var dbgDescription = cppElement.GetPrimarySymbol().DbgDescription;

                            var declaredFile = decl.GetSourceFile()?.GetLocation()?.FullPath ?? "unknown";
                            var declaredOffset = decl.GetDocumentRange().TextRange.StartOffset;
                            
                            var usagesLog = new StringBuilder();
                            int usageCount = 0;

                            // Define a consumer action that collects results
                            var consumer = new FindResultConsumer(result =>
                            {
                                if (result is FindResultReference refResult)
                                {
                                    var usageRange = refResult.Reference.GetDocumentRange();
                                    var usageFile = usageRange.Document.GetPsiSourceFile(solution)?.GetLocation().Name ?? "Unknown File";
                            
                                    if (usageCount < 10) // Limit output to first 10 to avoid giant message boxes
                                    {
                                        usagesLog.AppendLine($" - Used in {usageFile} at offset {usageRange.TextRange.StartOffset}");
                                    }
                                    usageCount++;
                                }
                                return FindExecution.Continue;
                            });

                            // Execute the search
                            finder.FindReferences(cppElement, searchDomain, consumer, NullProgressIndicator.Instance);
                            
                            if (usageCount == 0) usagesLog.AppendLine(" - No usages found.");
                            if (usageCount > 10) usagesLog.AppendLine($" - ... and {usageCount - 10} more.");

                            MessageBox.ShowInfo($@"
                                                Entity Name: {name}
                                                Entity Type: {type}
                                                complexOffset: {complexOffset}
                                                Text offset: {textOffset}
                                                LocateTextOffset(): {locateOffset}
                                                dbgDescription: {dbgDescription}

                                                Declared in: {declaredFile} at offset {declaredOffset}

                                                Usages ({usageCount}):
                                                {usagesLog}
                                                ");
                        }
                    }
                }
            }

            return Task.FromResult(names.ToArray());
        });
    }
}