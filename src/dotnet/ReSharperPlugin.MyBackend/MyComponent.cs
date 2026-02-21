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

namespace ReSharperPlugin.MyBackend;

[SolutionComponent(Instantiation.ContainerAsyncAnyThreadSafe)]
public class MyComponent
{
    public MyComponent(Lifetime lifetime, ISolution solution, BreadcrumbsProvider breadcrumbsProvider)
    {
        var model = solution.GetProtocolSolution().GetMyBackendModel();

        // Handle a frontend "request": give me all function names in a file
        model.GetFunctionNames.SetAsync((lt, request) =>
        {
            var collectedStatements = new List<StatementInfo>();

            MessageBox.ShowInfo("Entering SetAsync");
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
                var psiFile = psiSourceFile.GetPrimaryPsiFile();
                var finder = solution.GetPsiServices().SingleThreadedFinder;
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
                    collectedStatements = WalkFunctionFromNode(nodeAtOffset);
                }
            }

            return Task.FromResult(collectedStatements.ToArray());
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
                var escapedCondition = WebUtility.HtmlEncode(condition.ToString());
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
                MessageBox.ShowInfo($@"
                                    found enclosing if
                                    Offset: {offset}
                                    Name: {name}
                                    ");
            }
            else if (current is ForStatement forStmt)
            {
                var cppFor = forStmt.GetResolveEntity();
                var offset = cppFor.GetTextOffset();
                var condition = cppFor.GetCondition();
                var escapedCondition = WebUtility.HtmlEncode(condition.ToString());
                var name = $"for ({escapedCondition})";
                var statement = new StatementInfo(name, offset);
                result.Add(statement);
                MessageBox.ShowInfo($@"
                                    found enclosing for
                                    Offset: {offset}
                                    Name: {name}
                                    ");
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

                MessageBox.ShowInfo($@"
                                    found enclosing lambda
                                    ShortName : {lambdaName}
                                    Offset : {offset}
                                    Parameters : {lambdaParameters}
                                    Capture : {lambdaCapture}
                                    Body : {lambdaBody}
                                    ");
            }

            current = current.Parent;
        }

        return result;
    }
}