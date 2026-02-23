package com.jetbrains.rider.plugins.mybackend

import com.intellij.icons.AllIcons
import com.intellij.openapi.editor.colors.EditorColorsManager
import com.intellij.openapi.fileTypes.FileType
import com.intellij.openapi.fileTypes.SyntaxHighlighterFactory
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.DialogWrapper
import com.intellij.ui.ColoredTreeCellRenderer
import com.intellij.ui.SimpleTextAttributes
import com.intellij.ui.components.JBScrollPane
import com.intellij.ui.treeStructure.Tree
import com.jetbrains.rider.model.StatementInfo
import java.awt.Dimension
import javax.swing.Action
import javax.swing.JComponent
import javax.swing.JTree
import javax.swing.tree.DefaultMutableTreeNode
import javax.swing.tree.DefaultTreeModel

class StatementPathTreeDialog(
    private val project: Project,
    private val statementInfos: Array<StatementInfo>,
    private val fileType: FileType,
    private val caretLineText: String
) : DialogWrapper(project, true) {

    init {
        isModal = false
        title = "Statement Path"
        init()
    }

    override fun createCenterPanel(): JComponent {
        val root = DefaultMutableTreeNode()

        var parent = root
        for (info in statementInfos) {
            val node = DefaultMutableTreeNode(info)
            parent.add(node)
            parent = node
        }
        parent.add(DefaultMutableTreeNode(null)) // caret leaf

        val tree = Tree(DefaultTreeModel(root))
        tree.isRootVisible = false
        tree.showsRootHandles = true
        tree.cellRenderer = StatementCellRenderer(fileType, caretLineText)

        var row = 0
        while (row < tree.rowCount) {
            tree.expandRow(row)
            row++
        }

        val panel = JBScrollPane(tree)
        panel.preferredSize = Dimension(420, 300)
        return panel
    }

    override fun getDimensionServiceKey() = "com.jetbrains.rider.plugins.mybackend.StatementPathTreeDialog"

    override fun createActions(): Array<Action> {
        cancelAction.putValue(Action.NAME, "Close")
        return arrayOf(cancelAction)
    }
}

private class StatementCellRenderer(
    private val fileType: FileType,
    private val caretLineText: String
) : ColoredTreeCellRenderer() {

    override fun customizeCellRenderer(
        tree: JTree, value: Any, selected: Boolean,
        expanded: Boolean, leaf: Boolean, row: Int, hasFocus: Boolean
    ) {
        val node = value as? DefaultMutableTreeNode ?: return

        when (val info = node.userObject) {
            is StatementInfo -> {
                icon = AllIcons.Nodes.Method
                append(info.name, SimpleTextAttributes.REGULAR_BOLD_ATTRIBUTES)
                append("  offset: ${info.offset}", SimpleTextAttributes.GRAYED_SMALL_ATTRIBUTES)
            }
            null -> {
                // Caret leaf: tokenize the line and render each token with its highlight color.
                icon = AllIcons.General.ArrowRight
                appendHighlighted(caretLineText, fileType)
            }
        }
    }

    private fun appendHighlighted(code: String, fileType: FileType) {
        val highlighter = SyntaxHighlighterFactory.getSyntaxHighlighter(fileType, null, null)
        if (highlighter == null) {
            append(code, SimpleTextAttributes.REGULAR_ATTRIBUTES)
            return
        }

        val scheme = EditorColorsManager.getInstance().globalScheme
        val lexer = highlighter.highlightingLexer
        lexer.start(code)

        while (lexer.tokenType != null) {
            val tokenText = code.substring(lexer.tokenStart, lexer.tokenEnd)
            val attrs = highlighter.getTokenHighlights(lexer.tokenType!!)
                .mapNotNull { scheme.getAttributes(it) }
                .firstOrNull()

            append(
                tokenText,
                if (attrs != null) SimpleTextAttributes.fromTextAttributes(attrs)
                else SimpleTextAttributes.REGULAR_ATTRIBUTES
            )
            lexer.advance()
        }
    }
}
