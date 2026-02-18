package com.jetbrains.rider.plugins.mybackend


import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.CommonDataKeys
import com.intellij.openapi.rd.util.lifetime
import com.intellij.openapi.ui.Messages
import com.jetbrains.rider.model.myBackendModel // Ensure this import resolves
import com.jetbrains.rider.projectView.solution

class MyAction : AnAction() {
    override fun actionPerformed(e: AnActionEvent) {
        val project = e.project ?: return
        val solution = project.solution
        val model = solution.myBackendModel
        val editor = e.getData(CommonDataKeys.EDITOR) ?: return
        val vfile = e.getData(CommonDataKeys.VIRTUAL_FILE) ?: return

        // 2. Fire the signal (sends "Hello from Kotlin" to C#)
        //model.mycoolvalue.fire("mycoolvalue")

        // Call backend (request/response)
        val task = model.getFunctionNames.start(project.lifetime, vfile.path, null)

        // 3. Observe the result
        task.result.advise(project.lifetime) { result ->
            val functionNames = result.unwrap()
            val text = functionNames.joinToString("\n")
            Messages.showMessageDialog(project, text, "Functions", Messages.getInformationIcon())
        }
    }

    override fun update(e: AnActionEvent) {
        // Enable the action only if a project is open
        e.presentation.isEnabledAndVisible = e.project != null
    }
}
