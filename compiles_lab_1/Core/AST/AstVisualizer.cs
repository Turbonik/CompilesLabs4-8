using System.Collections.Generic;
using SkiaSharp;

namespace compiles_lab_1.Core.Ast
{
    public static class AstVisualizer
    {
        private const float NodeWidth = 180f;
        private const float NodeHeight = 60f;
        private const float HSpacing = 40f;
        private const float VSpacing = 80f;
        private const float Margin = 20f;

        public static void DrawAst(IReadOnlyList<AstNode> roots, SKCanvas canvas, SKImageInfo info)
        {
            canvas.Clear(SKColors.White);

            float y = Margin;

            foreach (var root in roots)
            {
                float centerX = info.Width / 2f;
                DrawSubtree(canvas, root, centerX, y, out float subtreeHeight);
                y += subtreeHeight + VSpacing;
            }
        }

        private static void DrawSubtree(SKCanvas canvas, AstNode node, float centerX, float y, out float height)
        {
            float subtreeWidth = MeasureWidth(node);

            var rect = new SKRect(
                centerX - NodeWidth / 2,
                y,
                centerX + NodeWidth / 2,
                y + NodeHeight);

            DrawNode(canvas, rect, FormatNode(node));

            if (node.Children.Count == 0)
            {
                height = NodeHeight;
                return;
            }

            float startX = centerX - subtreeWidth / 2;
            float childY = y + NodeHeight + VSpacing;

            foreach (var child in node.Children)
            {
                float w = MeasureWidth(child);
                float childCenter = startX + w / 2;

                canvas.DrawLine(rect.MidX, rect.Bottom, childCenter, childY, EdgePaint);

                DrawSubtree(canvas, child, childCenter, childY, out float childHeight);

                startX += w + HSpacing;
            }

            height = NodeHeight + VSpacing + MaxChildHeight(node);
        }

        private static float MeasureWidth(AstNode node)
        {
            if (node.Children.Count == 0)
                return NodeWidth;

            float sum = 0;
            foreach (var ch in node.Children)
                sum += MeasureWidth(ch) + HSpacing;

            return sum - HSpacing;
        }

        private static float MaxChildHeight(AstNode node)
        {
            float max = 0;
            foreach (var ch in node.Children)
                max = System.Math.Max(max, MeasureHeight(ch));
            return max;
        }

        private static float MeasureHeight(AstNode node)
        {
            if (node.Children.Count == 0)
                return NodeHeight;

            float max = 0;
            foreach (var ch in node.Children)
                max = System.Math.Max(max, MeasureHeight(ch));

            return NodeHeight + VSpacing + max;
        }

        private static readonly SKPaint FillPaint = new SKPaint
        {
            Color = new SKColor(230, 240, 255),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        private static readonly SKPaint BorderPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            StrokeWidth = 2,
            Style = SKPaintStyle.Stroke
        };

        private static readonly SKPaint TextPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            TextSize = 14
        };

        private static readonly SKPaint EdgePaint = new SKPaint
        {
            Color = SKColors.Gray,
            IsAntialias = true,
            StrokeWidth = 1.5f
        };

        private static void DrawNode(SKCanvas canvas, SKRect rect, string text)
        {
            canvas.DrawRoundRect(rect, 8, 8, FillPaint);
            canvas.DrawRoundRect(rect, 8, 8, BorderPaint);

            var lines = text.Split('\n');
            float lineHeight = TextPaint.TextSize + 2;
            float totalHeight = lines.Length * lineHeight;
            float y = rect.MidY - totalHeight / 2 + TextPaint.TextSize;

            foreach (var line in lines)
            {
                float w = TextPaint.MeasureText(line);
                float x = rect.MidX - w / 2;
                canvas.DrawText(line, x, y, TextPaint);
                y += lineHeight;
            }
        }

        private static string FormatNode(AstNode node)
        {
            return node switch
            {
                ConstDeclNode => "ConstDecl",
                ModifiersNode => "Modifiers",
                IdentifierNode id => $"Identifier\nname={id.Name}",
                IntNode t => $"IntNode\nname={t.Name}",
                IntLiteralNode v => $"IntLiteralNode\nvalue={v.Value}",
                TokenNode t => t.Text,
                _ => node.GetType().Name
            };
        }
 
    }
}
