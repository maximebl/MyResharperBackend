package com.jetbrains.rider.plugins.mybackend

import com.intellij.openapi.project.Project
import com.jetbrains.rdclient.util.idea.LifetimedProjectComponent
import com.jetbrains.rider.model.myBackendModel
import com.jetbrains.rider.projectView.solution

class MyComponent(project: Project) : LifetimedProjectComponent(project){
    init {
        val model = project.solution.myBackendModel;

    }
}