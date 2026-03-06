using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Application.Parts;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.IDE.UI.Extensions;
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
using JetBrains.ReSharper.Feature.Services.CSharp.PredictiveDebugger;
using JetBrains.ReSharper.Psi.Cpp.Language;
using JetBrains.ReSharper.Psi.Cpp.Symbols;
using JetBrains.ReSharper.Psi.Cpp.Tree.Util;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.UI.Controls.TreeListView;
using JetBrains.ReSharper.Feature.Services.Breadcrumbs;
using JetBrains.ReSharper.Feature.Services.Cpp.Breadcrumbs;
using JetBrains.ReSharper.Feature.Services.Cpp.Navigation.Goto;
using JetBrains.ReSharper.Feature.Services.Cpp.RmlDfa;
using JetBrains.ReSharper.Feature.Services.Cpp.Tree;
using JetBrains.ReSharper.Psi.Cpp.Types;
using JetBrains.ReSharper.Psi.Resx.Utils;
using JetBrains.ReSharper.TestRunner.Abstractions.Extensions;
using IfStatementNavigator = JetBrains.ReSharper.Psi.Cpp.Tree.IfStatementNavigator;
using JetBrains.Diagnostics;
using JetBrains.Rd.Util;
using JetBrains.ReSharper.Feature.Services.Cpp.Finder;
using JetBrains.ReSharper.Feature.Services.Cpp.UE4;

namespace ReSharperPlugin.MyBackend;

[SolutionComponent(Instantiation.ContainerAsyncAnyThreadSafe)]
public class MyComponent
{
    public MyComponent(Lifetime lifetime, ISolution solution)
    {
        var model = solution.GetProtocolSolution().GetMyBackendModel();

        model.GetFunctionNames.SetAsync((lt, request) =>
        {
            using (ReadLockCookie.Create())
            {
                var filePath = request.FilePath;
                var caretOffset = request.CaretOffset;

                var path = VirtualFileSystemPath.Parse(filePath, InteractionContext.SolutionContext);
                var projectFile = solution.FindProjectItemsByLocation(path)
                    .OfType<IProjectFile>()
                    .FirstOrDefault();

                PluginLog.BeginSection($"Request: {path.Name} @ {caretOffset}");

                var psiSourceFile = projectFile.ToSourceFile();
                if (psiSourceFile == null)
                {
                    PluginLog.Log($"Error: psiSourceFile is null\nFile: {filePath}");
                    return Task.FromResult<WalkedResult>(null);
                }

                var psiFile = psiSourceFile.GetPrimaryPsiFile();
                if (psiFile == null)
                {
                    PluginLog.Log($"Error: psiFile is null\nFile: {filePath}");
                    return Task.FromResult<WalkedResult>(null);
                }

                if (psiFile is CppFile cppFile)
                {
                    var documentOffset = new DocumentOffset(psiSourceFile.Document, caretOffset);
                    var nodeAtOffset = cppFile.FindNodeAt(documentOffset);

                    ICppFunctionDeclaratorResolveEntity resolvedEnclosingFunction = nodeAtOffset.GetEnclosingFunction();
                    var enclosingFuncDecl = resolvedEnclosingFunction.TryGetDeclarator() as IDeclaration;

                    if (enclosingFuncDecl.DeclaredElement is ICppDeclaredElement enclosingFunctionCppElement)
                    {
                        var name = enclosingFunctionCppElement.ShortName;
                        var type = enclosingFunctionCppElement.GetElementType().PresentableName;
                        var currentFuncPath = enclosingFunctionCppElement.GetSourceFiles().FirstOrDefault()
                            .GetLocation().FullPath;
                        var currentFuncOffset = enclosingFunctionCppElement.GetDeclarations().FirstOrDefault()
                            .GetDocumentRange().TextRange.StartOffset;

                        PluginLog.BeginSection("Enclosing Function");
                        PluginLog.Log($"Name:   {name}\nType:   {type}\nFile:   {currentFuncPath}\nOffset: {currentFuncOffset}");

                        var currentFunc = new WalkedFunction(
                            enclosingFunctionCppElement.ShortName,
                            currentFuncPath,
                            currentFuncOffset,
                            WalkFunctionFromNode(nodeAtOffset)
                        );

                        return Task.FromResult(new WalkedResult(currentFunc, new List<WalkedFunction>()));
                    }
                }
            }

            return Task.FromResult<WalkedResult>(null);
        });

        model.GetUsages.SetAsync((lt, request) =>
        {
            using (ReadLockCookie.Create())
            {
                var filePath = request.FilePath;
                var caretOffset = request.CaretOffset;

                var path = VirtualFileSystemPath.Parse(filePath, InteractionContext.SolutionContext);
                var projectFile = solution.FindProjectItemsByLocation(path)
                    .OfType<IProjectFile>()
                    .FirstOrDefault();

                var psiSourceFile = projectFile.ToSourceFile();
                if (psiSourceFile == null)
                    return Task.FromResult<WalkedResult>(null);

                var psiFile = psiSourceFile.GetPrimaryPsiFile();
                if (psiFile == null)
                    return Task.FromResult<WalkedResult>(null);

                var finder = solution.GetPsiServices().ParallelFinder;
                var searchDomain = solution.GetPsiServices().SearchDomainFactory.CreateSearchDomain(solution, true);

                if (psiFile is CppFile cppFile)
                {
                    var documentOffset = new DocumentOffset(psiSourceFile.Document, caretOffset);
                    var nodeAtOffset = cppFile.FindNodeAt(documentOffset);

                    ICppFunctionDeclaratorResolveEntity resolvedEnclosingFunction = nodeAtOffset.GetEnclosingFunction();
                    var enclosingFuncDecl = resolvedEnclosingFunction.TryGetDeclarator() as IDeclaration;

                    if (enclosingFuncDecl.DeclaredElement is ICppDeclaredElement enclosingFunctionCppElement)
                    {
                        var currentFuncPath = enclosingFunctionCppElement.GetSourceFiles().FirstOrDefault()
                            .GetLocation().FullPath;
                        var currentFuncOffset = enclosingFunctionCppElement.GetDeclarations().FirstOrDefault()
                            .GetDocumentRange().TextRange.StartOffset;
                        var currentFunc = new WalkedFunction(
                            enclosingFunctionCppElement.ShortName,
                            currentFuncPath,
                            currentFuncOffset,
                            WalkFunctionFromNode(nodeAtOffset)
                        );

                        var seed = new DeclaredElementInstance(enclosingFunctionCppElement);
                        ICollection<DeclaredElementInstance> relatedInstances =
                            CppDeclaredElementUtil.FindBaseAndOverridingDeclaredElements(
                                new List<DeclaredElementInstance> { seed });
                        var targets = relatedInstances.Select(i => i.Element).ToList();

                        PluginLog.BeginSection($"Related Instances ({relatedInstances.Count})");
                        foreach (var declaredElement in targets)
                            PluginLog.Log(declaredElement.ShortName);

                        int usageCount = 0;
                        List<(IDocument Document, int Offset)> offsetsToWalk = new List<(IDocument Document, int Offset)>();

                        PluginLog.BeginSection("Usages");
                        var consumer = new FindResultConsumer(result =>
                        {
                            if (result is IFindResultReference refResult)
                            {
                                DocumentRange usageRange = refResult.Reference.GetDocumentRange();
                                offsetsToWalk.Add((usageRange.Document, usageRange.TextRange.StartOffset));

                                var usageFile =
                                    usageRange.Document.GetPsiSourceFile(solution)?.GetLocation().Name ??
                                    "Unknown File";
                                PluginLog.Log($"{usageFile}\nOffset: {usageRange.TextRange.StartOffset}");
                                usageCount++;
                            }

                            return FindExecution.Continue;
                        });
                        finder.FindReferences(targets, searchDomain, consumer, NullProgressIndicator.Instance);

                        var usageFuncs = new List<WalkedFunction>();

                        PluginLog.BeginSection($"Walking {offsetsToWalk.Count} Usage Location(s)");
                        foreach (var (usageDocument, offset) in offsetsToWalk)
                        {
                            var docOffset = new DocumentOffset(usageDocument, offset);
                            var usagePsiSourceFile = usageDocument.GetPsiSourceFile(solution);
                            var usageCppFile = usagePsiSourceFile?.GetPrimaryPsiFile() as CppFile ?? cppFile;
                            var nodeToWalk = usageCppFile.FindNodeAt(docOffset);
                            if (nodeToWalk == null)
                            {
                                PluginLog.Log($"Error: nodeToWalk is null at offset {offset}");
                                continue;
                            }

                            var enclosingFunction = nodeToWalk.GetEnclosingFunction();
                            var funcDeclarator = enclosingFunction.TryGetDeclarator() as IDeclaration;

                            string usageFunctionName = "<name not found>";
                            string usageFunctionPath = "<path not found>";
                            if (funcDeclarator is { DeclaredElement: ICppDeclaredElement cppFuncElement })
                            {
                                usageFunctionName = cppFuncElement.ShortName;
                                usageFunctionPath = cppFuncElement.GetSourceFiles().FirstOrDefault().GetLocation().FullPath;
                            }
                            else if (nodeToWalk.GetContainingNode<LambdaExpression>() is { } lambda)
                            {
                                var lambdaParameters = lambda.LambdaDeclaratorNode.GetText();
                                var lambdaCapture = lambda.LambdaIntroducerNode.GetText();
                                var variableDecl = lambda.GetContainingNode<IDeclaration>();
                                var lambdaName = variableDecl?.DeclaredElement?.ShortName;

                                usageFunctionName = $"{lambdaCapture} {lambdaName} {lambdaParameters}";
                                usageFunctionPath = lambda.GetLocation().FilePath;
                            }

                            var func = new WalkedFunction(
                                usageFunctionName,
                                usageFunctionPath,
                                offset,
                                WalkFunctionFromNode(nodeToWalk)
                            );

                            PluginLog.Log($"Function:   {func.Name}\nFile:       {func.Path}\nOffset:     {func.Offset}\nStatements: {func.Statements.Count}");
                            usageFuncs.Add(func);
                        }

                        return Task.FromResult(new WalkedResult(currentFunc, usageFuncs));
                    }
                }
            }

            return Task.FromResult<WalkedResult>(null);
        });
    }

    // Return path to reach a given node.
    List<StatementInfo> WalkFunctionFromNode(ITreeNode node)
    {
        var result = new List<StatementInfo>();

        var current = node;
        while (current != null)
        {
            if (current is IfStatement ifStmt)
            {
                var cppIf = ifStmt.GetIfStatementResolveEntity();
                var offset = cppIf.GetTextOffset();
                var condition = cppIf.GetCondition();
                var name = $"if ({condition})";

                var elseNode = ifStmt.ElseStatement;
                if (elseNode != null)
                {
                    var elseRange = elseNode.GetDocumentRange().TextRange;
                    var caretOffset = node.GetDocumentRange().TextRange.StartOffset;
                    if (elseRange.Contains(caretOffset))
                    {
                        name = $"else : {name}";
                        offset = elseNode.GetTreeStartOffset().Offset;
                    }
                }

                result.Add(new StatementInfo(name, offset));
            }
            else if (current is ForStatement forStmt)
            {
                var cppFor = forStmt.GetResolveEntity();
                var offset = cppFor.GetTextOffset();
                var condition = cppFor.GetCondition();
                result.Add(new StatementInfo($"for ({condition})", offset));
            }
            else if (current is LambdaExpression lambda)
            {
                var range = lambda.GetDocumentRange().TextRange;
                var lambdaParameters = lambda.LambdaDeclaratorNode.GetText();
                var lambdaCapture = lambda.LambdaIntroducerNode.GetText();
                var lambdaName = lambda.GetContainingNode<IDeclaration>()?.DeclaredElement?.ShortName;
                result.Add(new StatementInfo($"{lambdaCapture} {lambdaName} {lambdaParameters}", range.StartOffset));
            }

            current = current.Parent;
        }

        result.Reverse();
        return result;
    }

    void DumpNodeInfo(ITreeNode node, string tag = "")
    {
        if (node == null)
        {
            PluginLog.Log($"[{tag}] Node is null");
            return;
        }

        PluginLog.Log(
            $"[{tag}]\n" +
            $"Type:     {node.GetType().FullName}\n" +
            $"NodeType: {node.NodeType}\n" +
            $"Language: {node.Language}\n" +
            $"Text:     {node.GetText()}"
        );
    }
}
