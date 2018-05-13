using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace dzEmpi
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<string> GetJavaFiles(string path)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(path).Where(p => p.EndsWith(".java")).ToArray());

            string[] directories = Directory.GetDirectories(path);

            if (directories.Length == 0)
            {
                return files;
            }

            foreach (string d in directories)
            {
                files.AddRange(GetJavaFiles(d));
            }

            return files;
        }

        private int CalculateLOC(string path)
        {
            int answer = 0;

            string[] lines = File.ReadAllLines(path);
            foreach (string s in lines)
            {
                if (Regex.Matches(s, @"[a-zA-Z]").Count != 0)
                {
                    answer++;
                }
            }

            return answer;
        }

        private int CalculateNOM(string path)
        {
            int answer = 0;

            string[] lines = File.ReadAllLines(path);
            foreach (string s in lines)
            {
                if (Regex.Matches(s, @"(public|protected|private|static|\s) +[\w\<\>\[\]]+\s+(\w+) *\([^\)]*\) *(\{?|[^;])").Count != 0)
                {
                    answer++;
                }
            }

            return answer;
        }

        private int CalculateNOC(string path)
        {
            int answer = 0;

            string[] lines = File.ReadAllLines(path);
            foreach (string s in lines)
            {
                if (Regex.Matches(s, @"\b(class)\b").Count != 0)
                {
                    answer++;
                }
            }

            return answer;
        }

        private List<string> CalculateNOP(string path)
        {
            string pattern = @"^package\s+(.*)$";
            string[] lines = File.ReadAllLines(path);

            List<string> packages = new List<string>();
            foreach (string s in lines)
            {
                if (Regex.Match(s, @"^package\s+(.*)$").Length > 0)
                {
                    if (!packages.Contains(Regex.Match(s, @"^package\s+(.*)$").Groups[1].Value.ToString()))
                    {
                        packages.Add(Regex.Match(s, @"^package\s+(.*)$").Groups[1].Value.ToString());
                    }
                }
            }

            return packages;
        }

        private void btnOpenDir_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                int LOC = 0;
                int NOM = 0;
                int NOC = 0;
                List<string> NOP = new List<string>();
                DialogResult result = fbd.ShowDialog();
                lblDirectory.Content = fbd.SelectedPath.ToString();

                List<string> files = GetJavaFiles(fbd.SelectedPath);

                foreach (string f in files)
                {
                    LOC += CalculateLOC(f);
                    NOM += CalculateNOM(f);
                    NOC += CalculateNOC(f);
                    if (!NOP.Equals(CalculateNOP(f)))
                    {
                        foreach (string s in CalculateNOP(f))
                        {
                            if(!NOP.Contains(s))
                            {
                                NOP.Add(s);
                            }
                        }
                    }
                }
                lblLOC.Content = LOC.ToString();
                lblNOM.Content = NOM.ToString();
                lblNOC.Content = NOC.ToString();
                lblNOP.Content = NOP.Count.ToString();
            }
        }
    }
}
