package com.jetbrains.rider.plugins.mybackend


import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.CommonDataKeys
import com.intellij.openapi.editor.ScrollType
import com.intellij.openapi.fileEditor.OpenFileDescriptor
import com.intellij.openapi.rd.util.lifetime
import com.intellij.openapi.ui.Messages
import com.intellij.openapi.vfs.LocalFileSystem
import com.jetbrains.rider.model.MyFindRequest
import com.jetbrains.rider.model.myBackendModel // Ensure this import resolves
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

        // Observe the result.
        task.result.advise(project.lifetime) { result ->
            val statementInfos = result.unwrap()

            // Parse received statements.
            val text = if (statementInfos.isEmpty()) {
                "No statements found."
            } else {
                statementInfos.joinToString(
                    separator = "\n"
                ) { info ->
                    "${info.offset}: ${info.name}"
                }
            }

            Messages.showMessageDialog(
                project,
                text,
                "Front end received string:",
                Messages.getInformationIcon()
            )

//            val targetPath = statementInfos[0]
//            val targetOffset = statementInfos[1]

//            val targetVirtualFile = LocalFileSystem.getInstance().findFileByPath(targetPath)
//            if (targetVirtualFile != null) {
//                // 2. Open the file and navigate to offset
//                // OpenFileDescriptor handles both opening the file (if closed) and navigating (if open)
//                OpenFileDescriptor(project, targetVirtualFile, targetOffset).navigate(true)
//            } else {
//                Messages.showErrorDialog("Could not find file: $targetPath", "Navigation Error")
//            }
//
//            editor.caretModel.moveToOffset(targetOffset)
//            editor.scrollingModel.scrollToCaret(ScrollType.CENTER)
        }
    }

    override fun update(e: AnActionEvent) {
        // Enable the action only if a project is open
        e.presentation.isEnabledAndVisible = e.project != null
    }
}
