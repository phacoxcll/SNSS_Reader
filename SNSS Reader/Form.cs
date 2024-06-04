using System;
using System.Windows.Forms;

namespace SNSS_Reader
{
    public partial class Form : System.Windows.Forms.Form
    {
        private SNSS File;

        public Form()
        {
            InitializeComponent();
            File = null;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
                Open(args[1]);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Simplest reader of SNSS format. Version 21-02-09\n\n" +
                "By phacox.cll\n\n" +
                "Based on:\n" +
                "  https://digitalinvestigation.wordpress.com/2012/09/03/chrome-session-and-tabs-files-and-the-puzzle-of-the-pickle/",
                "About", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private void Open(string path)
        {
            try
            {
                File = new SNSS(path);

                richTextBox.Clear();
                treeView.Nodes.Clear();

                TreeNode root = new TreeNode("SNSS");
                root.Name = "SNSS";
                treeView.Nodes.Add(root);

                for (int i = 0; i < File.Commands.Count; i++)
                {
                    TreeNode node = new TreeNode("[" + i.ToString() + "] Id: " + File.Commands[i].Id.ToString());
                    root.Nodes.Add(node);
                }
            }
            catch
            {
                richTextBox.AppendText("Could not open file: \"" + path + "\"");
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Open(openFileDialog.FileName);
                if (File != null)
                    ShowURLs();
            }
        }

        private void ShowURLs()
        {
            richTextBox.Clear();
            richTextBox.AppendText(File.ToString() + "\n");
            if (File.Version != 0)
            {
                richTextBox.AppendText("URLs:\n");
                for (int i = 0; i < File.Commands.Count; i++)
                {
                    if (File.Commands[i].Content is SNSS.Tab)
                    {
                        int t = ((SNSS.Tab)File.Commands[i].Content).Index;
                        while (t-- > 0)
                            richTextBox.AppendText("  ");
                        richTextBox.AppendText(((SNSS.Tab)File.Commands[i].Content).URL + "\n");
                    }
                }
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (File != null && treeView.SelectedNode != null)
            {
                if (treeView.SelectedNode.Name == "SNSS")
                {
                    ShowURLs();
                }
                else
                {
                    richTextBox.Clear();
                    richTextBox.AppendText(File.Commands[treeView.SelectedNode.Index].ToString());
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Undo()
        {
            if (richTextBox.CanUndo)
                richTextBox.Undo();
        }

        private void Redo()
        {
            if (richTextBox.CanRedo)
                richTextBox.Redo();
        }

        private void Cut()
        {
            if (richTextBox.SelectionLength > 0)
                richTextBox.Cut();
        }

        private void Copy()
        {
            if (richTextBox.SelectionLength > 0)
                richTextBox.Copy();
        }

        private void Paste()
        {
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
                richTextBox.Paste();
        }

        private void Delete()
        {
            if (richTextBox.SelectionLength > 0)
                richTextBox.SelectedText = "";
        }

        private void SelectAll()
        {
            richTextBox.Select();
            richTextBox.SelectAll();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void selectAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SelectAll();
        }
    }
}
