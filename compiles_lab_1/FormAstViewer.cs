using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using compiles_lab_1.Core.Ast;
using compiles_lab_1.Core;

namespace compiles_lab_1
{
    public sealed class FormAstViewer : Form
    {
        private readonly SKControl _skControl;
        private readonly Panel _scrollPanel;
        private readonly List<AstNode> _nodes;

        public FormAstViewer(IEnumerable<AstNode> nodes)
        {
            _nodes = new List<AstNode>(nodes ?? Array.Empty<AstNode>());

            Text = "AST Viewer";
            WindowState = FormWindowState.Maximized;
 
            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
 
            _skControl = new SKControl
            {
                Location = new System.Drawing.Point(0, 0),
                Size = new System.Drawing.Size(2000, 5000)  
            };
            _skControl.PaintSurface += SkControl_PaintSurface;

            _scrollPanel.Controls.Add(_skControl);
            Controls.Add(_scrollPanel);
        }

        private void SkControl_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            AstVisualizer.DrawAst(_nodes, e.Surface.Canvas, e.Info);
 
            _scrollPanel.AutoScrollMinSize = new System.Drawing.Size(
                e.Info.Width,
                e.Info.Height
            );
        }

        public static void ShowAst(SemanticResult result)
        {
            if (result == null || result.AstNodes.Count == 0)
            {
                MessageBox.Show("AST пустой", "AST", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var form = new FormAstViewer(result.AstNodes);
            form.Show();
        }
    }
}
