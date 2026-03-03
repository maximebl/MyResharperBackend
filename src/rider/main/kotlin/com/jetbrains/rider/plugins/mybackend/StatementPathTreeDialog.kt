package com.jetbrains.rider.plugins.mybackend

import com.intellij.icons.AllIcons
import com.intellij.openapi.editor.colors.EditorColorsManager
import com.intellij.openapi.fileTypes.FileType
import com.intellij.openapi.fileTypes.SyntaxHighlighterFactory
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.DialogWrapper
import com.intellij.ui.SimpleColoredComponent
import com.intellij.ui.SimpleTextAttributes
import com.intellij.util.ui.UIUtil
import java.awt.Component
import com.intellij.ui.components.JBScrollPane
import com.intellij.ui.treeStructure.Tree
import com.jetbrains.rider.model.StatementInfo
import com.jetbrains.rider.model.WalkedFunction
import com.jetbrains.rider.model.WalkedResult
import java.awt.Dimension
import java.awt.Font
import javax.swing.Action
import javax.swing.JComponent
import javax.swing.JTree
import javax.swing.tree.DefaultMutableTreeNode
import javax.swing.tree.DefaultTreeModel
import javax.swing.tree.TreeCellRenderer

class StatementPathTreeDialog(
    private val project: Project,
    private val walkedResult: WalkedResult,
    private val fileType: FileType,
    private val caretLineText: String
) : DialogWrapper(project, true) {

    init {
        isModal = false
        title = "Statement Path — ${walkedResult.current.name}"
        init()
    }

    override fun createCenterPanel(): JComponent {
        val root = DefaultMutableTreeNode()

        // Section: path to caret
        val pathHeader = DefaultMutableTreeNode(SectionHeader("Path to caret"))
        root.add(pathHeader)
        var parent: DefaultMutableTreeNode = pathHeader
        for (info in walkedResult.current.statements) {
            val node = DefaultMutableTreeNode(info)
            parent.add(node)
            parent = node
        }
        parent.add(DefaultMutableTreeNode(null)) // caret leaf

        // Section: usages
        val usagesHeader = DefaultMutableTreeNode(SectionHeader("Usages (${walkedResult.usages.size})"))
        root.add(usagesHeader)
        for (usage in walkedResult.usages) {
            val usageNode = DefaultMutableTreeNode(usage)
            usagesHeader.add(usageNode)
            var usageParent = usageNode
            for (stmt in usage.statements) {
                val stmtNode = DefaultMutableTreeNode(stmt)
                usageParent.add(stmtNode)
                usageParent = stmtNode
            }
        }

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
        panel.preferredSize = Dimension(500, 400)
        return panel
    }

    override fun getDimensionServiceKey() = "com.jetbrains.rider.plugins.mybackend.StatementPathTreeDialog"

    override fun createActions(): Array<Action> {
        cancelAction.putValue(Action.NAME, "Close")
        return arrayOf(cancelAction)
    }
}

private data class SectionHeader(val text: String)

private class StatementCellRenderer(
    private val fileType: FileType,
    private val caretLineText: String
) : SimpleColoredComponent(), TreeCellRenderer {

    override fun getTreeCellRendererComponent(
        tree: JTree, value: Any?, selected: Boolean,
        expanded: Boolean, leaf: Boolean, row: Int, hasFocus: Boolean
    ): Component {
        // Start with a clean slate — no HTML mode, no leftover text or icons.
        clear()

        isOpaque = selected
        if (selected) background = UIUtil.getTreeSelectionBackground(hasFocus)

        val uiFont = tree.font
        val scheme = EditorColorsManager.getInstance().globalScheme
        val codeFont = Font(scheme.editorFontName, Font.PLAIN, scheme.editorFontSize)

        val node = value as? DefaultMutableTreeNode ?: return this
        when (val obj = node.userObject) {
            is SectionHeader -> {
                font = uiFont
                icon = AllIcons.Nodes.Folder
                append(obj.text, SimpleTextAttributes.REGULAR_BOLD_ATTRIBUTES)
            }
            is WalkedFunction -> {
                font = codeFont
                icon = AllIcons.Nodes.Function
                appendHighlighted(obj.name, fileType)
                append("  ${obj.path}:${obj.offset}", SimpleTextAttributes.GRAYED_ATTRIBUTES)
            }
            is StatementInfo -> {
                font = codeFont
                icon = AllIcons.Nodes.Method
                appendHighlighted(obj.name, fileType)
                append("  offset: ${obj.offset}", SimpleTextAttributes.GRAYED_ATTRIBUTES)
            }
            null -> {
                font = codeFont
                icon = AllIcons.General.ArrowRight
                appendHighlighted(caretLineText, fileType)
            }
        }
        return this
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
                if (attrs != null) SimpleTextAttributes(attrs.fontType, attrs.foregroundColor)
                else SimpleTextAttributes.REGULAR_ATTRIBUTES
            )
            lexer.advance()
        }
    }
}
