package com.jetbrains.rider.plugins.mybackend


import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.CommonDataKeys
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.rd.util.lifetime
import com.intellij.openapi.util.TextRange
import com.jetbrains.rider.model.MyFindRequest
import com.jetbrains.rider.model.WalkedResult
import com.jetbrains.rider.model.myBackendModel
import com.jetbrains.rider.projectView.solution

class MyAction : AnAction() {
    override fun actionPerformed(e: AnActionEvent) {
        val project = e.project ?: return
        val solution = project.solution
        val model = solution.myBackendModel
        val editor = e.getData(CommonDataKeys.EDITOR) ?: return
        val vfile = e.getData(CommonDataKeys.VIRTUAL_FILE) ?: return

        // Call backend (request/response).
        val request = MyFindRequest(vfile.path, editor.caretModel.offset)
        val task = model.getFunctionNames.start(project.lifetime, request)
        val usagesTask = model.getUsages.start(project.lifetime, request)

        // Capture the current line text before the async callback.
        val lineNumber = editor.caretModel.logicalPosition.line
        val caretLineText = editor.document.getText(
            TextRange(
                editor.document.getLineStartOffset(lineNumber),
                editor.document.getLineEndOffset(lineNumber)
            )
        ).trim()

        var dialog: StatementPathTreeDialog? = null

        // Open dialog as soon as the current function is ready (fast path).
        task.result.advise(project.lifetime) { result ->
            val walkedResult: WalkedResult = result.unwrap()
            ApplicationManager.getApplication().invokeLater {
                val dlg = StatementPathTreeDialog(project, walkedResult, vfile.fileType, caretLineText)
                dialog = dlg
                dlg.show()
            }
        }

        // Update usages once the slow reference search completes.
        usagesTask.result.advise(project.lifetime) { result ->
            val walkedResult: WalkedResult = result.unwrap()
            dialog?.setUsages(walkedResult.usages)
        }
    }

    override fun update(e: AnActionEvent) {
        // Enable the action only if a project is open
        e.presentation.isEnabledAndVisible = e.project != null
    }
}
