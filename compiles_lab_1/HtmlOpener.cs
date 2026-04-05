using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiles_lab_1
{
    internal static class HtmlOpener
    {
        readonly static string basePath = AppDomain.CurrentDomain.BaseDirectory;


        public static void Help()
        {
            string lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            string HelpFile = lang == "ru"
                ? "HTMLs/help_ru.html"
                : "HTMLs/help_en.html";

            string helpPath = Path.Combine(basePath, HelpFile);


            CheckPath(helpPath);
        }

        public static void About()
        {
            string lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            string AboutFile = lang == "ru"
             ? "HTMLs/about_ru.html"
             : "HTMLs/about_en.html";

            string aboutPath = Path.Combine(basePath, AboutFile);

            CheckPath(aboutPath);
        }

        public static void OpenTask() =>
            OpenHtml("HTMLs/task.html");

        public static void OpenGrammar() =>
            OpenHtml("HTMLs/grammar.html");

        public static void OpenClassification() =>
            OpenHtml("HTMLs/classification.html");

        public static void OpenAnalysis() =>
            OpenHtml("HTMLs/analysis.html");

        public static void OpenExample() =>
            OpenHtml("HTMLs/example.html");

        public static void OpenSourceList() =>
            OpenHtml("HTMLs/sourcelist.html");

        public static void OpenCode() =>
            OpenHtml("HTMLs/code.html");


        private static void OpenHtml(string relativePath)
        {
            string path = Path.Combine(basePath, relativePath);
            CheckPath(path);
        }



        public static void CheckPath(string Path)
        {

            if (File.Exists(Path))
                System.Diagnostics.Process.Start
                    (new System.Diagnostics.ProcessStartInfo(Path) { UseShellExecute = true });
            else
                MessageBox.Show("Файл не найден:\n" + Path);
        }

    }
}
