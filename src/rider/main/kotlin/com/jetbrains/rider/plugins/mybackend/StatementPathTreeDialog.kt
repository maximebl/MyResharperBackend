package com.jetbrains.rider.plugins.mybackend

import com.intellij.icons.AllIcons
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.editor.colors.EditorColorsManager
import com.intellij.openapi.fileEditor.OpenFileDescriptor
import com.intellij.openapi.vfs.LocalFileSystem
import javax.swing.JProgressBar
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
import java.awt.BorderLayout
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
    private val caretLineText: String,
    private val originalPath: String,
    private val originalOffset: Int
) : DialogWrapper(project, true) {

    private lateinit var treeModel: DefaultTreeModel
    private lateinit var usagesHeader: DefaultMutableTreeNode
    private lateinit var progressBar: JProgressBar
    private var progressValue = 0

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

        // Section: usages (initially empty while loading)
        usagesHeader = DefaultMutableTreeNode(SectionHeader("Usages (loading…)"))
        root.add(usagesHeader)

        treeModel = DefaultTreeModel(root)
        val tree = Tree(treeModel)
        tree.isRootVisible = false
        tree.showsRootHandles = true
        tree.cellRenderer = StatementCellRenderer(fileType, caretLineText)
        tree.addTreeSelectionListener { e ->
            val node = e.path.lastPathComponent as? DefaultMutableTreeNode ?: return@addTreeSelectionListener
            navigateToNode(node, requestFocus = false)
        }
        tree.addKeyListener(object : java.awt.event.KeyAdapter() {
            override fun keyPressed(e: java.awt.event.KeyEvent) {
                if (e.keyCode == java.awt.event.KeyEvent.VK_ENTER) {
                    val node = tree.lastSelectedPathComponent as? DefaultMutableTreeNode ?: return
                    navigateToNode(node, requestFocus = true)
                    close(OK_EXIT_CODE)
                }
            }
        })

        var row = 0
        while (row < tree.rowCount) {
            tree.expandRow(row)
            row++
        }

        progressBar = JProgressBar(0, 1).apply {
            value = 0
            isStringPainted = true
            string = "Searching for usages…"
        }

        val scrollPane = JBScrollPane(tree)
        scrollPane.preferredSize = Dimension(500, 400)

        val panel = javax.swing.JPanel(BorderLayout())
        panel.add(scrollPane, BorderLayout.CENTER)
        panel.add(progressBar, BorderLayout.SOUTH)
        return panel
    }

    fun addUsage(usage: WalkedFunction) {
        ApplicationManager.getApplication().invokeLater {
            val usageNode = DefaultMutableTreeNode(usage)
            usagesHeader.add(usageNode)
            var usageParent = usageNode
            for (stmt in usage.statements) {
                val stmtNode = DefaultMutableTreeNode(stmt)
                usageParent.add(stmtNode)
                usageParent = stmtNode
            }
            progressValue++
            usagesHeader.userObject = SectionHeader("Usages ($progressValue)")
            treeModel.nodeStructureChanged(usagesHeader)
            progressBar.maximum = progressValue + 1
            progressBar.value = progressValue
            progressBar.string = "Found $progressValue usage(s)…"
        }
    }

    fun onSearchComplete() {
        ApplicationManager.getApplication().invokeLater {
            progressBar.isVisible = false
        }
    }

    override fun doCancelAction() {
        val virtualFile = LocalFileSystem.getInstance().findFileByPath(originalPath) ?: return super.doCancelAction()
        OpenFileDescriptor(project, virtualFile, originalOffset).navigate(true)
        super.doCancelAction()
    }

    private fun navigateToNode(node: DefaultMutableTreeNode, requestFocus: Boolean) {
        val (filePath, offset) = when (val obj = node.userObject) {
            is WalkedFunction -> obj.path to obj.offset
            is StatementInfo -> {
                // Walk up to find the enclosing WalkedFunction for the file path,
                // falling back to the current function if none is found (path-to-caret section).
                val path = generateSequence(node.parent as? DefaultMutableTreeNode) { it.parent as? DefaultMutableTreeNode }
                    .mapNotNull { (it.userObject as? WalkedFunction)?.path }
                    .firstOrNull() ?: walkedResult.current.path
                path to obj.offset
            }
            else -> return
        }
        val virtualFile = LocalFileSystem.getInstance().findFileByPath(filePath) ?: return
        OpenFileDescriptor(project, virtualFile, offset).navigate(requestFocus)
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
