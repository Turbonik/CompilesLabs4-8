using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using compiles_lab_1.Core;
using System.Diagnostics;

namespace compiles_lab_1
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        const int SB_VERT = 1;
        const int WM_VSCROLL = 0x115;
        const int SB_THUMBPOSITION = 4;

        private FileManager fileManager;


        private class DocumentTab
        {
            public int SelectedResultsTabIndex = 0;

            public string Title;
            public string FilePath;
            public string Text;
            public bool IsModified;

            public UndoRedoManager UndoRedoManager;
            public ToolStripButton Button;

            public ScanResult LastScan;
            public DataGridView ScannerGrid;
        }


        private readonly List<DocumentTab> documents = new();
        private DocumentTab currentDocument;
        private bool _internalTextUpdate = false;

        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true;
            fileManager = new FileManager();

            AttachEvents();
            HotKeysBinder();

            this.Resize += (s, e) => ScaleUI();

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                RunSyntaxAnalysis(richTextBox1.Text);
                e.Handled = true;
            }
        }

        private void ScaleUI()
        {
            float baseHeight = 720f;
            float scale = this.Height / baseHeight;

            if (scale < 0.8f) scale = 0.8f;
            if (scale > 1.4f) scale = 1.4f;

            toolStrip1.AutoSize = false;
            toolStrip1.Height = (int)(40 * scale);

            toolStrip1.ImageScalingSize = new Size(
                (int)(24 * scale),
                (int)(24 * scale)
            );

            lineNumberBox.Font = richTextBox1.Font;
            lineNumberBox.Margin = new Padding(0);
            lineNumberBox.Padding = new Padding(0);
            lineNumberBox.AutoSize = false;
            lineNumberBox.Multiline = true;
            lineNumberBox.WordWrap = false;
   

            foreach (ToolStripItem item in toolStrip1.Items)
            {
                item.ImageScaling = ToolStripItemImageScaling.SizeToFit;
                item.AutoSize = false;
                item.Width = (int)(48 * scale);
                item.Height = (int)(48 * scale);
            }

            menuStrip1.AutoSize = false;
            menuStrip1.Height = (int)(40 * scale);
            menuStrip1.Font = new Font(menuStrip1.Font.FontFamily, 10 * scale);

            statusStrip.AutoSize = false;
            statusStrip.Height = (int)(32 * scale);

            foreach (ToolStripItem item in statusStrip.Items)
                item.Font = new Font(item.Font.FontFamily, 9 * scale);

            tabControlResults.Font = new Font(tabControlResults.Font.FontFamily, 10 * scale);
 
        }



        private void SetLanguage(string culture)
        {
            if (currentDocument != null)
                currentDocument.Text = richTextBox1.Text;

            int index = documents.IndexOf(currentDocument);

            float editorFontSize = richTextBox1.Font.Size;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            var res = new ComponentResourceManager(typeof(Form1));

            SuspendLayout();
            ApplyResourcesRecursive(this, res);
            
            foreach (var doc in documents)
{
    if (doc.ScannerGrid != null)
    {
        var grid = doc.ScannerGrid;

        grid.Columns[0].HeaderText = Strings.Incfrag;
        grid.Columns[1].HeaderText = Strings.Location;
        grid.Columns[2].HeaderText = Strings.Description;
    }
}

            ResumeLayout(true);

            tabsStrip.Items.Clear();
            foreach (var doc in documents)
            {
                if (doc.FilePath == null)
                    doc.Title = Strings.NewDocument;

                var btn = new ToolStripButton(doc.Title);
                btn.MouseUp += TabMouseUp;
                btn.Click += (s, e) => SwitchToDocument(doc);
                doc.Button = btn;
                tabsStrip.Items.Add(btn);
            }

            foreach (var doc in documents)
            { 
                var grid = doc.ScannerGrid;
                if (grid != null)
                {
 
                }
                doc.ScannerGrid?.Rows.Clear();
                doc.LastScan = null;
 
            }



            if (index >= 0 && index < documents.Count)
            {
                currentDocument = documents[index];
                _internalTextUpdate = true;
                richTextBox1.Text = currentDocument.Text;
                _internalTextUpdate = false;
                SwitchToDocument(currentDocument);
            }
            else
            {
                currentDocument = null;
                richTextBox1.Clear();
            }



            richTextBox1.Enabled = currentDocument != null;

            SetEditorFontSize(editorFontSize);
            UpdateLineNumbers();

            CloseTabMenuItem.Text = Strings.CloseTab;
            ScaleUI();

            tabPageResults.ImageIndex = 0;
        }

        private void ApplyResourcesRecursive(Control control, ComponentResourceManager res)
        {
            if (!string.IsNullOrEmpty(control.Name) &&
                control != richTextBox1 &&
                control != lineNumberBox &&
                control is not DataGridView)
            {
                res.ApplyResources(control, control.Name);
            }

            if (control is MenuStrip ms)
                foreach (ToolStripItem item in ms.Items)
                    ApplyResourcesToolStripItem(item, res);

            if (control is ToolStrip ts)
                foreach (ToolStripItem item in ts.Items)
                    ApplyResourcesToolStripItem(item, res);

            foreach (Control child in control.Controls)
                ApplyResourcesRecursive(child, res);
        }



        private void ApplyResourcesToolStripItem(ToolStripItem item, ComponentResourceManager res)
        {
            if (!string.IsNullOrEmpty(item.Name))
                res.ApplyResources(item, item.Name);

            if (item is ToolStripDropDownItem dd)
                foreach (ToolStripItem sub in dd.DropDownItems)
                    ApplyResourcesToolStripItem(sub, res);
        }

        private void AttachEvents()
        {
            resultIcons = new ImageList();
            resultIcons.ImageSize = new Size(16, 16);
            
            string basePath = Path.Combine(Application.StartupPath, "IconsForCompiles");

            resultIcons.Images.Add("ok", Image.FromFile(Path.Combine(basePath, "ok.png")));      
            resultIcons.Images.Add("error", Image.FromFile(Path.Combine(basePath, "error.png")));  

            tabControlResults.ImageList = resultIcons;
            tabPageResults.ImageIndex = 0; 


            TextSizeComboBox.Text = "9";
            splitContainer1.Panel1.AllowDrop = true;
            richTextBox1.AllowDrop = true;

            localizationMenu.DropDownItems.Add("English", null, (s, e) => SetLanguage("en"));
            localizationMenu.DropDownItems.Add("Ðóññêèé", null, (s, e) => SetLanguage("ru"));
            this.InputLanguageChanged += (s, e) => UpdateStatus();

            tabControlResults.SelectedIndexChanged += tabControlResults_SelectedIndexChanged;

            CreateToolStripMenuItem.Click += CreateFile;
            OpenToolStripMenuItem.Click += OpenFile;
            SaveToolStripMenuItem.Click += SaveFile;
            SaveAsToolStripMenuItem.Click += SaveFileAs;
            ExitToolStripMenuItem.Click += ExitApp;
             
            CancelToolStripMenuItem.Click += UndoText;
            RepeatToolStripMenuItem.Click += RedoText;
            CutToolStripMenuItem.Click += CutText;
            CopyToolStripMenuItem.Click += CopyText;
            PasteToolStripMenuItem.Click += PasteText;
            DeleteToolStripMenuItem.Click += DeleteText;
            HighlightAllÂñåToolStripMenuItem.Click += SelectAllText;

            TaskToolStripMenuItem.Click += (s, e) => HtmlOpener.OpenTask();
            GrammarToolStripMenuItem.Click += (s, e) => HtmlOpener.OpenGrammar();
            ClassificationToolStripMenuItem.Click += (s, e) => HtmlOpener.OpenClassification();
            AnalysisToolStripMenuItem.Click += (s, e) => HtmlOpener.OpenAnalysis();
            ExampleToolStripMenuItem.Click += (s, e) => HtmlOpener.OpenExample();
            SourceListToolStripMenuItem.Click += (s, e) => HtmlOpener.OpenSourceList();
            CodeToolStripMenuItem.Click += (s, e) => HtmlOpener.OpenCode();

            toolStripButton2.Click += CreateFile;
            toolStripButton3.Click += OpenFile;
            toolStripButton4.Click += SaveFile;
            toolStripButton5.Click += UndoText;
            toolStripButton11.Click += RedoText;
            toolStripButton10.Click += CopyText;
            toolStripButton9.Click += CutText;
            toolStripButton8.Click += PasteText;
            runMenu.Click += RunAnalysis;

            CallHelpToolStripMenuItem.Click += CallHelp;
            AboutToolStripMenuItem.Click += CallAbout;
            toolStripButton6.Click += CallHelp;    
            toolStripButton1.Click += CallAbout; 

            richTextBox1.VScroll += (s, e) => { SyncScroll(); UpdateLineNumberWidth(); };

            FormClosing += Form1_FormClosing;

            TextSizeComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (float.TryParse(TextSizeComboBox.Text, out float size))
                   {
                    SetEditorFontSize(size);
                    SyntaxHighlighter.Highlight(richTextBox1);
                   }

                }
                ;

            splitContainer1.Panel1.DragEnter += File_DragEnter;
            splitContainer1.Panel1.DragDrop += File_DragDrop;
            richTextBox1.DragEnter += File_DragEnter;
            richTextBox1.DragDrop += File_DragDrop;
            richTextBox1.SelectionChanged += (s, e) => UpdateStatus();

            CloseTabMenuItem.Text = Strings.CloseTab;
            CloseTabMenuItem.Click += (s, e) =>
            {
                if (currentDocument != null)
                    CloseDocument(currentDocument);
            };

            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);

            richTextBox1.TextChanged += (s, e) =>
            {
                if (_internalTextUpdate) return;

                _internalTextUpdate = true;
 
                int scrollPos = GetScrollPos(richTextBox1.Handle, SB_VERT);
 
                currentDocument?.UndoRedoManager?.OnTextChanged(richTextBox1);

                if (currentDocument != null)
                {
                    currentDocument.IsModified = true;
                }

                UpdateLineNumbers();
 
                SyntaxHighlighter.Highlight(richTextBox1);
 
                SetScrollPos(richTextBox1.Handle, SB_VERT, scrollPos, true);
                SendMessage(richTextBox1.Handle, WM_VSCROLL,
                    (IntPtr)(scrollPos << 16 | SB_THUMBPOSITION), IntPtr.Zero);
 
                SyncScroll();

                _internalTextUpdate = false;

            };

        }

        private void RichTextBox1_TextChanged(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void HotKeysBinder()
        {
            CreateToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            OpenToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            SaveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;

            CancelToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
            RepeatToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Y;

            CopyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            CutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            PasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;

            CallHelpToolStripMenuItem.ShortcutKeys = Keys.F1;
            AboutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F1;
            runMenu.ShortcutKeys = Keys.F5;

            toolStripButton6.Click += (s, e) => CallHelpToolStripMenuItem.PerformClick();
            toolStripButton1.Click += (s, e) => AboutToolStripMenuItem.PerformClick();
            toolStripButton7.Click += (s, e) => runMenu.PerformClick();

        }

        private void CreateFile(object sender, EventArgs e)
        {
            if (!CanCreateNewDocument()) return;

            var doc = new DocumentTab
            {
                Title = Strings.NewDocument,
                FilePath = null,
                Text = "",
                IsModified = false,
                UndoRedoManager = new UndoRedoManager(),
                ScannerGrid = CreateScannerGrid()
            };


            var btn = new ToolStripButton(doc.Title);
            btn.MouseUp += TabMouseUp;
            btn.Click += (s, ev) => SwitchToDocument(doc);

            doc.Button = btn;
            documents.Add(doc);
            tabsStrip.Items.Add(btn);
            richTextBox1.Enabled = true;
 
            SwitchToDocument(doc);
            doc.UndoRedoManager.ResetInitial(richTextBox1);
        }

        private DataGridView CreateScannerGrid()
        {
            var grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.AllowUserToResizeColumns = false;
 
            grid.Font = new Font("Segoe UI", 10);

            grid.Columns.Add("Fragment", Strings.Incfrag);
            grid.Columns.Add("Location", Strings.Location);
            grid.Columns.Add("Message", Strings.Description);
 
            grid.CellClick += ScannerGrid_CellClick;

            grid.RowPrePaint += (s, e) =>
            {
                var row = grid.Rows[e.RowIndex];
                if (row.Tag is Lexeme lx && lx.Code == LexemeCode.Error)
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    row.DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
                }
            };


            return grid;
        }
 
        private void TabMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var btn = sender as ToolStripButton;
                var doc = documents.FirstOrDefault(d => d.Button == btn);
                if (doc != null)
                {
                    SwitchToDocument(doc);
                    tabContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void SwitchToDocument(DocumentTab doc)
        {
            if (currentDocument != null)
                currentDocument.Text = richTextBox1.Text;

            currentDocument = doc;

            _internalTextUpdate = true;
            richTextBox1.Text = doc.Text;
            _internalTextUpdate = false;

            tabPageResults.Controls.Clear();
            tabPageResults.Controls.Add(doc.ScannerGrid);

            tabControlResults.SelectedIndex = 0;  

            foreach (ToolStripButton b in tabsStrip.Items)
                b.BackColor = SystemColors.Control;

            doc.Button.BackColor = Color.LightGray;
            UpdateStatus();
        }
 
        private void OpenFile(object sender, EventArgs e)
        {
            if (!CanCreateNewDocument())
                return;

            string path, text;

            if (!fileManager.TryOpenFileDialog(out path, out text))
                return;

            if (IsFileAlreadyOpen(path))
            {
                var doc = documents.First(d => d.FilePath == path);
                SwitchToDocument(doc);
                return;
            }

            OpenFileFromPath(path);
            richTextBox1.Enabled = true;
            UpdateStatus();
        }


        private void OpenFileFromPath(string path)
        {
            if (IsFileAlreadyOpen(path))
            {
                var existing = documents.First(d => d.FilePath == path);
                SwitchToDocument(existing);
                return;
            }

            if (!CanCreateNewDocument())
                return;

            string text;

            if (!fileManager.TryReadFile(path, out text))
                return;

            var newDoc = new DocumentTab
            {
                Title = Path.GetFileName(path),
                FilePath = path,
                Text = text,
                IsModified = false,
                UndoRedoManager = new UndoRedoManager(),
                ScannerGrid = CreateScannerGrid()
            };
 

            var btn = new ToolStripButton(newDoc.Title);
            btn.MouseUp += TabMouseUp;
            btn.Click += (s, ev) => SwitchToDocument(newDoc);

            newDoc.Button = btn;
            documents.Add(newDoc);
            tabsStrip.Items.Add(btn);

            SwitchToDocument(newDoc);
            newDoc.UndoRedoManager.ResetInitial(richTextBox1);
        }



        private bool CanCreateNewDocument()
        {
            if (documents.Count >= 14)
            {
                MessageBox.Show(Strings.MaxFiles, Strings.Limit, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void SaveFile(object sender, EventArgs e)
        {
            if (currentDocument == null)
                return;

            currentDocument.Text = richTextBox1.Text;

            if (currentDocument.FilePath == null)
            {
                SaveFileAs(sender, e);
                return;
            }

            if (fileManager.TrySaveFile(currentDocument.FilePath, currentDocument.Text))
            {
                currentDocument.IsModified = false;
                UpdateStatus();
            }
        }


        private void SaveFileAs(object sender, EventArgs e)
        {
            if (currentDocument == null)
                return;

            currentDocument.Text = richTextBox1.Text;

            string path;

            if (fileManager.TrySaveFileDialog(currentDocument.Text, out path))
            {
                currentDocument.FilePath = path;
                currentDocument.Title = Path.GetFileName(path);
                currentDocument.Button.Text = currentDocument.Title;
                currentDocument.IsModified = false;
                UpdateStatus();
            }
        }


        private void CloseDocument(DocumentTab doc)
        {
            if (doc.IsModified)
            {
                var result = MessageBox.Show(
                    Strings.SaveChanges,
                    Strings.CloseDocument,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                    return;

                if (result == DialogResult.Yes)
                {
                    currentDocument = doc;
                    SaveFile(null, null);
                }
            }
 
            if (currentDocument == doc)
            {
                tabPageResults.Controls.Clear();
            }

            tabsStrip.Items.Remove(doc.Button);
            documents.Remove(doc);

            if (documents.Count > 0)
            {
                SwitchToDocument(documents.Last());
            }
            else
            {
                currentDocument = null;
                richTextBox1.Clear();
                richTextBox1.Enabled = false;
 
                tabPageResults.Controls.Clear();
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show(Strings.ExitConfirm, Strings.Exit, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            foreach (var doc in documents.ToList())
            {
                if (!doc.IsModified)
                    continue;

                var r = MessageBox.Show($"{Strings.SaveChangesIn} \"{doc.Title}\"?", Strings.Saving, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (r == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (r == DialogResult.Yes)
                {
                    currentDocument = doc;
                    SaveFile(null, null);
                }
            }
        }

        private void UpdateLineNumbers()
        {
            int lineCount = richTextBox1.Lines.Length;
            var sb = new StringBuilder();

            for (int i = 1; i <= lineCount; i++)
                sb.AppendLine(i.ToString());

            lineNumberBox.Text = sb.ToString();
            UpdateLineNumberWidth();
        }

        private void UpdateLineNumberWidth()
        {
            int lineCount = Math.Max(1, richTextBox1.Lines.Length);
            string maxNumber = lineCount.ToString();
            int width = TextRenderer.MeasureText(maxNumber, lineNumberBox.Font).Width + 10;
            splitContainerLines.SplitterDistance = width;
        }

        private void SetEditorFontSize(float size)
        {
            bool wasEmpty = richTextBox1.TextLength == 0;

            if (wasEmpty)
            {
                _internalTextUpdate = true;
                richTextBox1.Text = " ";
                _internalTextUpdate = false;
            }

            richTextBox1.Font = new Font(richTextBox1.Font.FontFamily, size);
            lineNumberBox.Font = new Font(richTextBox1.Font.FontFamily, size);

            richTextBox1.Update();
            richTextBox1.Refresh();
            Application.DoEvents();

            UpdateLineNumberWidth();

            if (wasEmpty)
            {
                _internalTextUpdate = true;
                richTextBox1.Clear();
                _internalTextUpdate = false;
            }


        }


        private void File_DragEnter(object sender, DragEventArgs e)
        {
           
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !e.Data.GetDataPresent(DataFormats.Bitmap))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void File_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var path in files)
            {
                if (!CanCreateNewDocument())
                    break;

                OpenFileFromPath(path);
            }

            richTextBox1.Enabled = currentDocument != null;
        }

        private bool IsFileAlreadyOpen(string path)
        {
            return documents.Any(d => d.FilePath != null &&
                                      string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase));
        }

        private void SyncScroll()
        {
            int pos = GetScrollPos(richTextBox1.Handle, SB_VERT);
            int firstVisibleChar = richTextBox1.GetCharIndexFromPosition(new Point(1, 1));
            int firstVisibleLine = richTextBox1.GetLineFromCharIndex(firstVisibleChar);
            int correctedPos = Math.Max(pos, firstVisibleLine);

            SetScrollPos(lineNumberBox.Handle, SB_VERT, correctedPos, true);

            SendMessage(
                lineNumberBox.Handle,
                WM_VSCROLL,
                (IntPtr)(correctedPos << 16 | SB_THUMBPOSITION),
                IntPtr.Zero
            );
        }

        private void UpdateStatus()
        {
            if (currentDocument != null)
            {
                statusFile.Text = $"{Strings.StatusFile} {currentDocument.Title}" + (currentDocument.IsModified ? " *" : "");

                int index = richTextBox1.SelectionStart;
                int line = richTextBox1.GetLineFromCharIndex(index) + 1;
                int col = index - richTextBox1.GetFirstCharIndexOfCurrentLine() + 1;

                statusPosition.Text = $"{Strings.StatusLine} {line}, {Strings.StatusColumn} {col}";
                statusLines.Text = $"{Strings.StatusLines} {richTextBox1.Lines.Length}";

                if (currentDocument.IsModified)
                {
                    int textBytes = Encoding.UTF8.GetByteCount(richTextBox1.Text);
                    statusSize.Text = $"{Strings.StatusSize} {textBytes} {Strings.BytesUnsaved}";
                }
                else if (currentDocument.FilePath != null)
                {
                    long size = new FileInfo(currentDocument.FilePath).Length;
                    statusSize.Text = $"{Strings.StatusSize} {size} {Strings.Bytes}";
                }
                else
                {
                    statusSize.Text = $"{Strings.StatusSize} -";
                }
            }
            else
            {
                statusFile.Text = $"{Strings.StatusFile} -";
                statusPosition.Text = $"{Strings.StatusLine} -";
                statusLines.Text = $"{Strings.StatusLines} -";
                statusSize.Text = $"{Strings.StatusSize} -";
            }

            statusLang.Text = $"{Strings.StatusLang} {InputLanguage.CurrentInputLanguage.Culture.TwoLetterISOLanguageName.ToUpper()}";
        }


        private void UndoText(object sender, EventArgs e)
        {
            currentDocument?.UndoRedoManager?.Undo(richTextBox1);
        }

        private void RedoText(object sender, EventArgs e)
        {
            currentDocument?.UndoRedoManager?.Redo(richTextBox1);
        }

        private void CutText(object sender, EventArgs e)
        {
            if (currentDocument == null)
            {
                MessageBox.Show(Strings.Run, Strings.RunHead, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            richTextBox1.Cut();
        }
        private void CopyText(object sender, EventArgs e)
        {
            if (currentDocument == null)
            {
                MessageBox.Show(Strings.Run, Strings.RunHead, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            richTextBox1.Copy();
        }
        private void PasteText(object sender, EventArgs e)
        {
            if (currentDocument == null)
            {
                MessageBox.Show(Strings.Run, Strings.RunHead, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Clipboard.ContainsImage())
                richTextBox1.Paste();
        }
        private void CallHelp(object sender, EventArgs e)
        {
            HtmlOpener.Help();
        }
        private void CallAbout(object sender, EventArgs e)
        {
            HtmlOpener.About();
        }
        private void DeleteText(object sender, EventArgs e) => richTextBox1.SelectedText = "";
        private void SelectAllText(object sender, EventArgs e) => richTextBox1.SelectAll();

        private void ExitApp(object sender, EventArgs e) => Close();
        private void RunAnalysis(object sender, EventArgs e)
        {
            if (currentDocument == null)
            {
                MessageBox.Show(Strings.Run, Strings.RunHead, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string text = richTextBox1.Text;

            var parseResult = Parser.Analyze(text);

            FillParserErrors(parseResult);

            tabControlResults.SelectedIndex = 0;
 
            UpdateResultTabIndicator(parseResult.Errors.Count);

        }

        private void FillParserErrors(ParseResult result)
        {
            if (currentDocument == null || currentDocument.ScannerGrid == null)
                return;

            var grid = currentDocument.ScannerGrid;
            grid.Rows.Clear();

            foreach (var err in result.Errors)
            {
                int row = grid.Rows.Add(
                    err.Fragment,
                    $"{Strings.LineLowered} {err.Line}, {err.StartColumn}-{err.EndColumn}",
                    err.Message
                );

                grid.Rows[row].Tag = err;
            }

            tabPageResults.Controls.Clear();
            tabPageResults.Controls.Add(grid);
            tabControlResults.SelectedIndex = 0;
        }






        private void ScannerGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = sender as DataGridView;

            var err = grid?.Rows[e.RowIndex].Tag as ParseError;
            if (err == null) return;

            int targetIndex = GetCharIndexFromLineColumn(err.Line, err.StartColumn);
            richTextBox1.SelectionStart = targetIndex;
            richTextBox1.ScrollToCaret();

            richTextBox1.Focus();
            richTextBox1.SelectionStart = targetIndex;
            richTextBox1.SelectionLength = Math.Max(1, err.EndColumn - err.StartColumn + 1);
        }


        private int GetCharIndexFromLineColumn(int line, int column)
        {
            int index = 0;

            for (int i = 0; i < line - 1; i++)
                index += richTextBox1.Lines[i].Length + 1;

            index += column - 1;
            return index;
        }
 
        private void tabControlResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentDocument != null)
                currentDocument.SelectedResultsTabIndex = tabControlResults.SelectedIndex;
        }

        private void RunSyntaxAnalysis(string code)
        {
            var result = AntlrWrapper.Analyze(code);

            if (result.Errors.Count == 0)
            {
                MessageBox.Show("Ñèíòàêñè÷åñêèé àíàëèç: OK");
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("Ñèíòàêñè÷åñêèå îøèáêè:");

                foreach (var err in result.Errors)
                {
                    sb.AppendLine(
                        $"Ñòðîêà {err.Line}, {err.StartColumn}-{err.EndColumn}: {err.Message} (\"{err.Fragment}\")");
                }

                MessageBox.Show(sb.ToString());
            }
        }



        private void UpdateResultTabIndicator(int errorCount)
        {
            if (errorCount == 0)
            {
                tabPageResults.ImageIndex = 0; 
                tabPageResults.Text = $"{Strings.Result} — {Strings.Nomistakes}";
            }
            else
            {
                tabPageResults.ImageIndex = 1;  

                string word = errorCount == 1 ? Strings.Mistake1 :
                              errorCount < 5 ? Strings.Mistake2 : Strings.Mistake3;

                tabPageResults.Text = $"{Strings.Result} — {errorCount} {word}";
            }
        }


    }
}
