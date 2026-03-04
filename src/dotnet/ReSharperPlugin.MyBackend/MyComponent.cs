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

        // Handle a frontend "request": give me all function names in a file
        model.GetFunctionNames.SetAsync((lt, request) =>
        {
            using (ReadLockCookie.Create())
            {
                // Inputs:
                var filePath = request.FilePath;
                var caretOffset = request.CaretOffset;

                // Process request:
                var path = VirtualFileSystemPath.Parse(filePath, InteractionContext.SolutionContext);
                var projectFile = solution.FindProjectItemsByLocation(path)
                    .OfType<IProjectFile>()
                    .FirstOrDefault();
                var psiSourceFile = projectFile.ToSourceFile();
                if (psiSourceFile == null)
                {
                    MessageBox.ShowInfo($@"
                                        psiSourceFile is null
                                        ");
                }

                var psiFile = psiSourceFile.GetPrimaryPsiFile();

                if (psiFile == null)
                {
                    MessageBox.ShowInfo($@"
                                        psiFile is null
                                        ");
                }

                var finder = solution.GetPsiServices().ParallelFinder;
                var searchDomain = solution.GetPsiServices().SearchDomainFactory.CreateSearchDomain(solution, true);

                if (psiFile is CppFile cppFile)
                {
                    var documentOffset = new DocumentOffset(psiSourceFile.Document, caretOffset);
                    var nodeAtOffset = cppFile.FindNodeAt(documentOffset);

                    /*
                     Find all:
                         - if statements
                         - for statements
                         - lambda declarations
                     Enclosing the caret
                     */

                    // Create a walked function:
                    //      - Get info about the enclosing function at the caret.
                    //          - Collect it's usage locations.
                    //      - Walk the enclosed function to collect the statements.
                    //      - Walk the usage locations.


                    // var collectedStatements = new List<StatementInfo>();
                    // collectedStatements = WalkFunctionFromNode(nodeAtOffset);

                    // Print information about the function enclosing the caret.
                    ICppFunctionDeclaratorResolveEntity resolvedEnclosingFunction = nodeAtOffset.GetEnclosingFunction();
                    var enclosingFuncDecl = resolvedEnclosingFunction.TryGetDeclarator() as IDeclaration;

                    MessageBox.ShowInfo($@"
                                            resolved enclosing func: {enclosingFuncDecl.DeclaredName}
                                            ");

                    if (enclosingFuncDecl.DeclaredElement is ICppDeclaredElement enclosingFunctionCppElement)
                    {
                        MessageBox.ShowInfo($@"
                                            Found cpp element: {enclosingFunctionCppElement.ShortName}
                                            ");
                        // Enclosing function name.
                        var name = enclosingFunctionCppElement.ShortName;
                        var type = enclosingFunctionCppElement.GetElementType().PresentableName;

                        // Enclosing function declaration.
                        var currentFuncPath = enclosingFunctionCppElement.GetSourceFiles().FirstOrDefault()
                            .GetLocation().FullPath;
                        var currentFuncOffset = enclosingFunctionCppElement.GetDeclarations().FirstOrDefault()
                            .GetDocumentRange()
                            .TextRange.StartOffset;

                        var currentFunc = new WalkedFunction(enclosingFunctionCppElement.ShortName,
                            enclosingFunctionCppElement.GetSourceFiles().FirstOrDefault().GetLocation().FullPath,
                            currentFuncOffset,
                            WalkFunctionFromNode(nodeAtOffset)
                        );

                        var seed = new DeclaredElementInstance(enclosingFunctionCppElement);
                        ICollection<DeclaredElementInstance> relatedInstances =
                            CppDeclaredElementUtil.FindBaseAndOverridingDeclaredElements(
                                new List<DeclaredElementInstance> {seed});
                        MessageBox.ShowInfo($@"

                                            relatedInstances.Count: {relatedInstances.Count}

                                            ");
                        var targets = relatedInstances.Select(i => i.Element).ToList();

                        foreach (var declaredElement in targets)
                        {
                            MessageBox.ShowInfo($@"

                                            target declared element: {declaredElement.ShortName}
                                            ");
                        }

                        // Log enclosing function usages.
                        var usagesLog = new StringBuilder();
                        int usageCount = 0;
                        List<(IDocument Document, int Offset)> offsetsToWalk = new List<(IDocument Document, int Offset)>();

                        var consumer = new FindResultConsumer(result =>
                        {
                            if (result is IFindResultReference refResult)
                            {
                                DocumentRange usageRange = refResult.Reference.GetDocumentRange();
                                offsetsToWalk.Add((usageRange.Document, usageRange.TextRange.StartOffset));

                                var usageFile =
                                    usageRange.Document.GetPsiSourceFile(solution)?.GetLocation().Name ??
                                    "Unknown File";

                                usagesLog.AppendLine(
                                    $" - Used in {usageFile} at offset {usageRange.TextRange.StartOffset}");
                                usageCount++;
                            }

                            return FindExecution.Continue;
                        });
                        // finder.FindReferences(enclosingFunctionCppElement, searchDomain, consumer, NullProgressIndicator.Instance);
                        finder.FindReferences(targets, searchDomain, consumer, NullProgressIndicator.Instance);

                        MessageBox.ShowInfo($@"
                                            Entity Name: {name}
                                            Entity Type: {type}

                                            Usages ({usageCount}):
                                            {usagesLog}

                                            Declaration: {currentFuncPath} : {currentFuncOffset}

                                            Num offsets to walk: {offsetsToWalk.Count}
                                            ");

                        var usageFuncs = new List<WalkedFunction>();
                        foreach (var (usageDocument, offset) in offsetsToWalk)
                        {
                            MessageBox.ShowInfo($@"
                                                Looping: {offset}
                                                Document: {usageDocument.GetText()}
                                                ");

                            var docOffset = new DocumentOffset(usageDocument, offset);
                            MessageBox.ShowInfo($@"
                                                docOffset: {docOffset}
                                                ");
                            var usagePsiSourceFile = usageDocument.GetPsiSourceFile(solution);
                            var usageCppFile = usagePsiSourceFile?.GetPrimaryPsiFile() as CppFile ?? cppFile;
                            var nodeToWalk = usageCppFile.FindNodeAt(docOffset);
                            if (nodeToWalk == null)
                            {
                                MessageBox.ShowInfo($@"
                                                nodeToWalk is null
                                                ");
                            }

                            MessageBox.ShowInfo($@"
                                                nodeToWalk: {nodeToWalk.GetText()}
                                                ");

                            var enclosingFunction = nodeToWalk.GetEnclosingFunction();
                            var funcDeclarator = enclosingFunction.TryGetDeclarator() as IDeclaration;

//                             MessageBox.ShowInfo($@"
//                                                 docOffset: {docOffset}
//                                                 nodeToWalk: {nodeToWalk}
//
//                                                 {nodeToWalk.GetType().FullName}
//                                                 {nodeToWalk.NodeType}
//                                                 {nodeToWalk.Language}
//                                                 {nodeToWalk.GetText()}
//                                                 {funcDeclarator.GetType().FullName}
//                                                 {funcDeclarator.NodeType}
//                                                 {funcDeclarator.Language}
//                                                 {funcDeclarator.GetText()}
//                                                 ");

                            string usageFunctionName = "<name not found>";
                            string usageFunctionPath = "<path not found>";
                            if (funcDeclarator is {DeclaredElement: ICppDeclaredElement cppFuncElement})
                            {
                                MessageBox.ShowInfo($@"Walking: {offset}
                                                       funcDeclarator is ICppDeclaredElement.
                                                        usageFunctionName: {usageFunctionName}
                                                    ");

                                usageFunctionName = cppFuncElement.ShortName;
                                usageFunctionPath = cppFuncElement.GetSourceFiles().FirstOrDefault().GetLocation()
                                    .FullPath;
                            }
                            else if (nodeToWalk.GetContainingNode<LambdaExpression>() is { } lambda)
                            {
                                // Enclosing function is a lambda.
                                var range = lambda.GetDocumentRange().TextRange;
                                var lambdaOffset = range.StartOffset;
                                var lambdaParameters = lambda.LambdaDeclaratorNode.GetText();
                                var lambdaCapture = lambda.LambdaIntroducerNode.GetText();
                                var variableDecl = lambda.GetContainingNode<IDeclaration>();
                                var variableElement = variableDecl?.DeclaredElement;
                                var lambdaName = variableElement?.ShortName;
                                var lambdaBody = lambda.LambdaBodyNode.GetText();

                                usageFunctionName = $"{lambdaCapture} {lambdaName} {lambdaParameters}";
                                usageFunctionPath = lambda.GetLocation().FilePath;

                                MessageBox.ShowInfo($@"Walking: {offset}
                                                       funcDeclarator is a lambda.
                                                       usageFunctionName: {usageFunctionName}
                                                    ");
                            }

                            MessageBox.ShowInfo($@"
                                                Creating a new WalkedFunction
                                                ");

                            var func = new WalkedFunction(
                                $"{usageFunctionName}",
                                $"{usageFunctionPath}",
                                offset,
                                WalkFunctionFromNode(nodeToWalk)
                            );

                            MessageBox.ShowInfo($@"
                                            Adding usage func:
                                            {func.Name}
                                            {func.Path}
                                            {func.Offset}
                                            {func.Statements.Count}
                                           ");
                            usageFuncs.Add(func);
                        }

                        var walkedResult = new WalkedResult(currentFunc, usageFuncs);
                        return Task.FromResult(walkedResult);
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
                var escapedCondition = condition.ToString();
                var name = $"if ({escapedCondition})";

                // Detect if caret is inside the else-branch
                var elseNode = ifStmt.ElseStatement;
                if (elseNode != null)
                {
                    var elseRange = elseNode.GetDocumentRange().TextRange;
                    var caretOffset = node.GetDocumentRange().TextRange.StartOffset;

                    if (elseRange.Contains(caretOffset))
                    {
                        // We are in the else-part of this if
                        name = $"else : {name}";
                        offset = elseNode.GetTreeStartOffset().Offset;
                    }
                }

                var statement = new StatementInfo(name, offset);
                result.Add(statement);
                // MessageBox.ShowInfo($@"
                //                     found enclosing if
                //                     Offset: {offset}
                //                     Name: {name}
                //                     ");
            }
            else if (current is ForStatement forStmt)
            {
                var cppFor = forStmt.GetResolveEntity();
                var offset = cppFor.GetTextOffset();
                var condition = cppFor.GetCondition();
                var escapedCondition = condition.ToString();
                var name = $"for ({escapedCondition})";
                var statement = new StatementInfo(name, offset);
                result.Add(statement);
                // MessageBox.ShowInfo($@"
                //                     found enclosing for
                //                     Offset: {offset}
                //                     Name: {name}
                //                     ");
            }
            else if (current is LambdaExpression lambda)
            {
                var range = lambda.GetDocumentRange().TextRange;
                var lambdaParameters = lambda.LambdaDeclaratorNode.GetText();
                var lambdaCapture = lambda.LambdaIntroducerNode.GetText();
                var variableDecl = lambda.GetContainingNode<IDeclaration>();
                var variableElement = variableDecl?.DeclaredElement;
                var lambdaName = variableElement?.ShortName;
                var lambdaBody = lambda.LambdaBodyNode.GetText();

                var name = $"{lambdaCapture} {lambdaName} {lambdaParameters}";
                var offset = range.StartOffset;
                var statement = new StatementInfo(name, offset);
                result.Add(statement);

                // MessageBox.ShowInfo($@"
                //                     found enclosing lambda
                //                     ShortName : {lambdaName}
                //                     Offset : {offset}
                //                     Parameters : {lambdaParameters}
                //                     Capture : {lambdaCapture}
                //                     Body : {lambdaBody}
                //                     ");
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
            MessageBox.ShowInfo($"[{tag}] Node is null");
            return;
        }

        var type = node.GetType();
        var nodeType = node.NodeType;

        MessageBox.ShowInfo(
            $"[{tag}]\n" +
            $"CLR type: {type.FullName}\n" +
            $"PSI nodeType: {nodeType}\n" +
            $"Language: {node.Language}\n" +
            $"Text: '{node.GetText()}'"
        );
    }
}