using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;

namespace ReSharperPlugin.MyBackend;

[ZoneMarker]
public class ZoneMarker : IRequire<IProjectModelZone>, IRequire<PsiFeaturesImplZone>
{
}

// CppTreeUtil
// CppStatementUtil

/* Find all enclosing if statements */
// DocumentOffset documentOffset = new DocumentOffset(psiSourceFile.Document, caretOffset);
// ITreeNode nodeAtOffset = cppFile.FindNodeAt(documentOffset);
// IfStatement foundIf = nodeAtOffset.GetContainingNode<IfStatement>();
// while (foundIf != null)
// {
//     CppIfStatementResolveEntity cppIf = foundIf.GetIfStatementResolveEntity();
//
//     // Get if statement information:
//     var offset = cppIf.GetTextOffset();
//     var condition = cppIf.GetCondition();
//     MessageBox.ShowInfo($@"
//                    found if statement
//                    Offset: {offset}
//                    Condition: {condition?.ToString()}
//                    ");
//     foundIf = foundIf.GetContainingNode<IfStatement>();
// }

/* Find the enclosing if statement */
// DocumentOffset documentOffset = new DocumentOffset(psiSourceFile.Document, caretOffset);
// ITreeNode nodeAtOffset = cppFile.FindNodeAt(documentOffset);
//
// IfStatement ifStatement = nodeAtOffset.GetContainingNode<IfStatement>();
//
// // Get if statement information:
// CppIfStatementResolveEntity cppIf = ifStatement.GetIfStatementResolveEntity();
// var offset = cppIf.GetTextOffset();
// var condition = cppIf.GetCondition();
// MessageBox.ShowInfo($@"
//                    found if statement
//                    Offset: {offset}
//                    Condition: {condition?.ToString()}
//                    ");

/* Get enclosing function name */
// DocumentOffset documentOffset = new DocumentOffset(psiSourceFile.Document, caretOffset);
// ITreeNode nodeAtOffset = cppFile.FindNodeAt(documentOffset);
// ICppFunctionDeclaratorResolveEntity enclosingFunction = nodeAtOffset.GetEnclosingFunction();
// CppQualifiedNamePart enclosingFunctionName = enclosingFunction.GetFullNameForEntity().QualName.Name;
//
// MessageBox.ShowInfo($@"
//                     Enclosing function name: {enclosingFunctionName}
//                    ");

/* Symbols */
// IEnumerable<ICppSymbol> syms = CppGotoSymbolUtil.GetSymbolsFromPsiFile(psiSourceFile);
// var symbolNames = new List<string>();
//
// foreach (var sym in syms)
// {
//     symbolNames.Add(sym.GetShortName());
// }
//
// MessageBox.ShowInfo(
//     $"Symbols in psiFile {psiSourceFile.DisplayName}:\n{string.Join("\n", symbolNames)}");

/* Breadcrumbs */
// var documentOffset = new DocumentOffset(psiSourceFile.Document, caretOffset);
// var crumbs = new List<CrumbModel>();
// breadcrumbsProvider.CollectBreadcrumbs(psiSourceFile, documentOffset, crumbs);
// var sb = new StringBuilder();
// foreach (var crumb in crumbs)
//     sb.AppendLine(crumb.Text);
//
// MessageBox.ShowInfo($"Breadcrumbs at offset {caretOffset}:\n{sb}");

// ITreeNode token = psiFile.FindTokenAt(new DocumentOffset(psiSourceFile.Document, caretOffset));

// if (token != null && token.Language.Is<CppLanguage>())
// {
//
//     MessageBox.ShowInfo($@"
//                       Node type:
//                        ");
// }

// var condition = token.GetContainingCodeFragment().IfStatement().Condition.ToString();
// var statement = token?.GetContainingNode<ICppIfStatementResolveEntity>(); 

// var statement = WalkFunctionFromOffset(caretOffset);

//                     var token = psiFile.FindTokenAt(new DocumentOffset(psiSourceFile.Document, caretOffset));
//                     var reference = token.Parent?.GetReferences().FirstOrDefault();
//                     if (reference != null)
//                     {
//                         var targetElement = reference.Resolve().DeclaredElement;
//                         if (targetElement is ICppDeclaredElement cppElement)
//                         {
//                             var firstFoundDeclaration = cppElement.GetSourceFiles().FirstOrDefault().GetLocation().FullPath;
//                             var declarationOffset = cppElement.GetDeclarations().FirstOrDefault().GetDocumentRange().TextRange.StartOffset;
//                             var name = cppElement.ShortName;
//                             var type = cppElement.GetElementType().PresentableName;
//                             var usagesLog = new StringBuilder();
//                             int usageCount = 0;
//
//                             var consumer = new FindResultConsumer(result =>
//                             {
//                                 if (result is FindResultReference refResult)
//                                 {
//                                     var usageRange = refResult.Reference.GetDocumentRange();
//                                     var usageFile =
//                                         usageRange.Document.GetPsiSourceFile(solution)?.GetLocation().Name ??
//                                         "Unknown File";
//
//                                     usagesLog.AppendLine(
//                                         $" - Used in {usageFile} at offset {usageRange.TextRange.StartOffset}");
//                                     usageCount++;
//                                 }
//
//                                 return FindExecution.Continue;
//                             });
//
//                             // Execute the search
//                             finder.FindReferences(cppElement, searchDomain, consumer, NullProgressIndicator.Instance);
//                             MessageBox.ShowInfo($@"
//                                                  Entity Name: {name}
//                                                  Entity Type: {type}
//
//                                                  Usages ({usageCount}):
//                                                  {usagesLog}
//
//                                                  Declaration: {firstFoundDeclaration} : {declarationOffset}
//                                                  ");
//                             names.Add(firstFoundDeclaration);
//                             names.Add(declarationOffset.ToString());
//                         }
//                     }

//                     foreach (var decl in cppFile.Descendants<IDeclaration>())
//                     {
//                         var element = decl.DeclaredElement;
//                         ICppResolveEntity resolvedEntity = element.GetResolveEntityFromDeclaredElement();
//
//                         if (element is ICppDeclaredElement cppElement)
//                         {
//                             var name = cppElement.ShortName;
//                             var type = cppElement.GetElementType().PresentableName;
//                             var complexOffset = cppElement.GetSymbolLocation().ComplexOffset;
//                             var textOffset = cppElement.GetSymbolLocation().TextOffset;
//                             var locateOffset = cppElement.GetSymbolLocation().LocateTextOffset();
//                             var dbgDescription = cppElement.GetPrimarySymbol().DbgDescription;
//
//                             var declaredFile = decl.GetSourceFile()?.GetLocation()?.FullPath ?? "unknown";
//                             var declaredOffset = decl.GetDocumentRange().TextRange.StartOffset;
//                             
//                             var usagesLog = new StringBuilder();
//                             int usageCount = 0;
//
//                             // Define a consumer action that collects results
//                             var consumer = new FindResultConsumer(result =>
//                             {
//                                 if (result is FindResultReference refResult)
//                                 {
//                                     var usageRange = refResult.Reference.GetDocumentRange();
//                                     var usageFile = usageRange.Document.GetPsiSourceFile(solution)?.GetLocation().Name ?? "Unknown File";
//                             
//                                     if (usageCount < 10) // Limit output to first 10 to avoid giant message boxes
//                                     {
//                                         usagesLog.AppendLine($" - Used in {usageFile} at offset {usageRange.TextRange.StartOffset}");
//                                     usageCount++;
//                                 }
//                                 return FindExecution.Continue;
//                             });
//
//                             // Execute the search
//                             finder.FindReferences(cppElement, searchDomain, consumer, NullProgressIndicator.Instance);
//                             
//                             if (usageCount == 0) usagesLog.AppendLine(" - No usages found.");
//                             if (usageCount > 10) usagesLog.AppendLine($" - ... and {usageCount - 10} more.");
//
//                             MessageBox.ShowInfo($@"
//                                                 Entity Name: {name}
//                                                 Entity Type: {type}
//                                                 complexOffset: {complexOffset}
//                                                 Text offset: {textOffset}
//                                                 LocateTextOffset(): {locateOffset}
//                                                 dbgDescription: {dbgDescription}
//
//                                                 Declared in: {declaredFile} at offset {declaredOffset}
//
//                                                 Usages ({usageCount}):
//                                                 {usagesLog}
//                                                 ");
//                             names.Add(name);
//                         }
//                     }