using System.Collections.Generic;
using System.Windows.Forms;

namespace compiles_lab_1.Core
{
    public class UndoRedoManager
    {
        private class EditorState
        {
            public string Text;
            public int Caret;
        }

        private readonly Stack<EditorState> _undo = new();
        private readonly Stack<EditorState> _redo = new();

        private string _lastText;
        private int _lastCaret;
        private bool _hasLast;
        private bool _suppress;

        public void ResetInitial(RichTextBox box)
        {
            _undo.Clear();
            _redo.Clear();
            _lastText = box.Text;
            _lastCaret = box.SelectionStart;
            _hasLast = true;
        }

        public void OnTextChanged(RichTextBox box)
        {
            if (_suppress)
                return;

            if (!_hasLast)
            {
                _lastText = box.Text;
                _lastCaret = box.SelectionStart;
                _hasLast = true;
                return;
            }

            _undo.Push(new EditorState
            {
                Text = _lastText,
                Caret = _lastCaret
            });

            _redo.Clear();

            _lastText = box.Text;
            _lastCaret = box.SelectionStart;
        }

        public void Undo(RichTextBox box)
        {
            if (_undo.Count == 0)
                return;

            var state = _undo.Pop();

            _redo.Push(new EditorState
            {
                Text = box.Text,
                Caret = box.SelectionStart
            });

            _suppress = true;
            box.Text = state.Text;
            box.SelectionStart = state.Caret;
            _suppress = false;

            _lastText = box.Text;
            _lastCaret = box.SelectionStart;
        }

        public void Redo(RichTextBox box)
        {
            if (_redo.Count == 0)
                return;

            var state = _redo.Pop();

            _undo.Push(new EditorState
            {
                Text = box.Text,
                Caret = box.SelectionStart
            });

            _suppress = true;
            box.Text = state.Text;
            box.SelectionStart = state.Caret;
            _suppress = false;

            _lastText = box.Text;
            _lastCaret = box.SelectionStart;
        }
    }
}
