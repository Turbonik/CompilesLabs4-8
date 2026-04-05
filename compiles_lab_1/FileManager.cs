namespace compiles_lab_1
{
    public class FileManager
    {
        public bool TryOpenFileDialog(out string path, out string text)
        {
            path = null;
            text = null;

            var dialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы|*.txt|Все файлы|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return false;

            path = dialog.FileName;

            try
            {
                text = File.ReadAllText(path);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения файла:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool TryReadFile(string path, out string text)
        {
            text = null;

            try
            {
                text = File.ReadAllText(path);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения файла:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool TrySaveFile(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool TrySaveFileDialog(string text, out string path)
        {
            path = null;

            var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы|*.txt|Все файлы|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return false;

            path = dialog.FileName;

            try
            {
                File.WriteAllText(path, text);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }

}
