using System;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;

namespace TextGen
{
    public partial class PreferencesForm : Form
    {
        public PreferencesForm()
        {
            InitializeComponent();
            openFileDialog1.FileName = "";
            if (Properties.Settings.Default.TemplatePath != "")
            {
                textBox1.Text = Path.GetFileName(Properties.Settings.Default.TemplatePath);
            };
            if (Properties.Settings.Default.IndexPath != "")
            {
                textBox2.Text = Path.GetFileName(Properties.Settings.Default.IndexPath);
            }
            if (Properties.Settings.Default.TestsPath != "")
            {
                textBox3.Text = Path.GetFileName(Properties.Settings.Default.TestsPath);
            }
        }
        private async void BtnSaveClick(object sender, EventArgs e)
        {
            await CloseSave();
            Close();
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void T1MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(textBox1, "Натисніть щоб вибрати файл");
        }

        private void T2MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(textBox2, "Натисніть щоб вибрати файл");
        }

        private void T3MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(textBox3, "Натисніть щоб вибрати файл");
        }

        private void T1MouseClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog x = new OpenFileDialog()
            {
                FileName = "",
                Filter = "HTML файл|*.html",
                Title = label1.Text
            };
            x.ShowDialog();

            if (x.FileName != "")
            {
                Properties.Settings.Default.TemplatePath = x.FileName;
                textBox1.Text = Path.GetFileName(x.FileName);
            }
            else
            {
                textBox1.Text = x.FileName;
                Properties.Settings.Default.TemplatePath = "";
            }
        }

        private void T2MouseClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog x = new OpenFileDialog()
            {
                Filter = "HTML файл|*.html",
                Title = label2.Text
            };
            x.ShowDialog();
            if (x.FileName != "")
            {
                Properties.Settings.Default.IndexPath = x.FileName;
                textBox2.Text = Path.GetFileName(x.FileName);
            } else
            {
                textBox2.Text = x.FileName;
                Properties.Settings.Default.IndexPath = "";
            }
        }

        private void T3MouseClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog x = new OpenFileDialog()
            {
                Filter = "Tекстовий файл|*.txt"
            };
            x.ShowDialog();
            x.Title = label3.Text;
            if (x.FileName != "")
            {
                Properties.Settings.Default.TestsPath = x.FileName;
                textBox3.Text = Path.GetFileName(x.FileName);
            } else
            {
                textBox3.Text = x.FileName;
                Properties.Settings.Default.TestsPath = "";
            }
        }
        private Task CloseSave()
        {
            if (textBox1.Text == "")
            {
                Properties.Settings.Default.TemplatePath = "";
            }
            if (textBox2.Text == "")
            {
                Properties.Settings.Default.IndexPath = "";
            }
            if (textBox3.Text == "")
            {
                Properties.Settings.Default.TestsPath = "";
            }
            Properties.Settings.Default.Save();
            return Task.CompletedTask;
        }
        private  void PreferencesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
