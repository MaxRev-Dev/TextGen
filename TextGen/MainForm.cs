using CefSharp;
using CefSharp.WinForms;
using HtmlAgilityPack;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TextGen
{

    public partial class MainForm : Form
    {

        public ChromiumWebBrowser chromeBrowser;

        public MainForm()
        {
            InitializeComponent();
            docc = new HtmlAgilityPack.HtmlDocument()
            {
                OptionUseIdAttribute = true,
                OptionFixNestedTags = true, OptionOutputOriginalCase = true,
            };
            filepick.FileName = "";            
        }
        private enum local
        {
            def_En,uk_UA
        }
        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            page = string.Format(@"{0}\Resources\Interface.html", Application.StartupPath);

            if (!File.Exists(page))
            {
                MessageBox.Show("Error The html file doesn't exists : " + page);
            }
            if (!Cef.IsInitialized)
            {
                Cef.Initialize(settings);
            }
            chromeBrowser = new ChromiumWebBrowser(page);

            Invoke(new Action(() => { Controls.Add(chromeBrowser); }));

            chromeBrowser.RegisterJsObject("CefObj", new embebbedChromium.CefCustomObject(chromeBrowser, this));
            chromeBrowser.IsBrowserInitializedChanged += ChrFix;

        }

        private void ChrFix(object sender, IsBrowserInitializedChangedEventArgs e)
        {
            chromeBrowser.Dock = DockStyle.Fill;
            chromeBrowser.MenuHandler = new CustomMenuHandler();
            BrowserSettings browserSettings = new BrowserSettings();
            int x = Properties.Settings.Default.Local;
            if (x == Convert.ToInt32(local.def_En))
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-150");
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-150");

            }
            else if (x == Convert.ToInt32(local.uk_UA))
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("uk-UA");
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("uk-UA");
                Task.Delay(1300).ContinueWith(_ =>
                {
                    FrameJsLocaleUA();
                });

            }
            Invoke(new Action(() => pictureBox1.Hide()));
            if (jsIn != "")
            {
                Task.Delay(1300).ContinueWith(_ =>
                {
                    chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(jsIn);
                    FrameJsShowInfo(Properties.Resources.DefLoaded, 2000);                    
                });
            }
            if (FirstLaunchNeeded)
            {
                Task.Delay(1700).ContinueWith(_ =>
                {
                    FrameJsFirstLaunch();
                });
            }
        }

        public String page;
        private bool FirstLaunchNeeded=false;
        public TextGen.Lead s = new TextGen.Lead();
        private volatile bool cef_started = false;
        public string[] tmpArr, invstrarr;
        private string jsIn = "";
        public volatile bool stop = false;

        string textinp = null;
        private void Form1_Load(object sender, EventArgs e)
        {
            // int r = 0;
            //foreach (string i in DateTime.Now.GetDateTimeFormats()) { Console.WriteLine(r++ +"  " + i); };
            Thread x = new Thread(Init);
            x.Start();
            try
            {
              textinp = File.ReadAllText(Properties.Settings.Default.TestsPath, Encoding.Default);
            }
            catch (System.ArgumentException)
            {
                FirstLaunchNeeded = true;           
            }
            if (textinp != null)
            {
                var jstextinp = EncodeJsString(textinp);
                var str = string.Format("\nx.value = {0}", jstextinp);
                jsIn = "(function(){var x = document.getElementById(\'slave\'); " + str + " ;})(); " +
               "showtab2();";

                richTextBox1.Text = textinp;
            } 
            
            //FrameJsLocale();
        }
        private void Init() //Thread of CefSharp
        {
            if (!cef_started)
            {
                InitializeChromium();
                cef_started = true;
            }
            while (cef_started)
            {
                Thread.Sleep(500);
            }
            try { Cef.Shutdown(); } catch (Exception) { } 
        }

        private void GeneratedEvents(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                HtmlNode xd = docc.DocumentNode.SelectSingleNode("/html[1]/body[1]");
                var jstextinp = EncodeJsString(xd.InnerHtml.ToString());
                Action ac = () =>
                   {
                       FrameJsSetText(jstextinp);
                   };
                if (InvokeRequired) Invoke(ac); else ac();

                FrameJsNonGenBtn();
                SaveHTML();
            }
            catch (Exception)
            {
                Console.WriteLine("Browser instance is null");
            }
        }
        private void MainBackgroundTask(object sender, DoWorkEventArgs e)
        {
            try
            {
                FrameJsShowInfo(Properties.Resources.Gen, 1000);
                FrameJsIsGenBtn();

                BackgroundWorker worker = sender as BackgroundWorker;
                stop = false;
                TextGen TG = new TextGen();
                Action ac = () =>
                {
                    FrameJsShowPB();
                    FrameJsSetVal(0);
                };
                if (InvokeRequired) Invoke(ac); else ac();
                GetArray();
                TG.NodeAccrd = LoadHtml();
                TG.MainArr = invstrarr;
                int maxiumBar = TG.GetLock();
                ac = () =>
                {
                    FrameJsPBMax(maxiumBar);
                };
                if (InvokeRequired) Invoke(ac); else ac();
                NewDocument();

                TG.doc = new HtmlAgilityPack.HtmlDocument()
                {
                    OptionUseIdAttribute = true
                };
                TG.doc.LoadHtml(TG.NodeAccrd.OuterHtml);
                TG.AccordElem = TG.NodeAccrd;
                TG.acrd = TG.doc.GetElementbyId("accordion");


                if (InvokeRequired) Invoke(ac); else ac();

                for (TG.LV.currentPanel = 0; TG.LV.currentPanel < TG.mainques.Length; ++TG.LV.currentPanel)
                {
                    TG.CheckStrPanel();
                    int curpan = TG.mainques[TG.LV.currentPanel] + 1, nextpan = 0;
                    if (TG.LV.currentPanel != TG.mainques.Length - 1) nextpan = TG.mainques[TG.LV.currentPanel + 1];
                    else
                    {
                        curpan = TG.mainques[TG.mainques.Length - 1] + 1;
                        nextpan = TG.MainArr.Length - 1;
                    }

                    for (TG.LV.iter = curpan; TG.LV.iter < nextpan; TG.LV.iter++)
                    {
                        if (TG.LV.iter <= TG.MainArr.Length)
                        {
                            Thread StringAnswerThread = new Thread(TG.CheckStr);
                            StringAnswerThread.Start();
                            StringAnswerThread.Join();
                        }
                        Application.DoEvents();
                        ac = () =>
                        {
                            FrameJsSetVal(TG.LV.currentPanel);
                        };
                        if (InvokeRequired) Invoke(ac); else ac();
                    }
                    Console.WriteLine("Completed: " + (TG.LV.currentPanel + 1));
                    if (stop) break;
                }
                stop = true;
            
                FrameJsShowInfo(Properties.Resources.Wait, 3000);
                docc.OptionUseIdAttribute = true;
                docc.GetElementbyId("main").AppendChild(TG.TimeStamp());

                ac = () =>
                  {
                      FrameJsSetVal(TG.mainques.Length);
                      Thread.Sleep(3000);
                      FrameJsHidePB();
                  };
                if (InvokeRequired) BeginInvoke(ac); else ac();
                Thread.Sleep(800);
                TG = null;
            }
            catch (Exception x) { Console.WriteLine(x); }
        }


        //helpers
        public HtmlAgilityPack.HtmlDocument docc;
        public HtmlNode LoadHtml()
        {
            string html = "";
            try
            {
                string Templ = Properties.Settings.Default.TemplatePath;
                string defpath = Path.GetDirectoryName(Properties.Settings.Default.TestsPath) + "\\";
                if (Templ != "" && Templ != defpath)
                {
                    docc.LoadHtml(File.ReadAllText(Templ, Encoding.Default));
                }
                else
                {
                    html = CreateTemplate();
                    string fullp = Path.GetDirectoryName(Properties.Settings.Default.TestsPath);
                    string index = fullp + "\\" + "template.html";
                    Properties.Settings.Default.TemplatePath = index;
                    docc.LoadHtml(html);
                    Properties.Settings.Default.Save();
                    StreamWriter outFile = new StreamWriter(index, false, Encoding.UTF8);
                    outFile.Write(html);
                    outFile.Close();
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
            }
            return docc.GetElementbyId("accordion");
        }
        public void GetArray()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => GetArray()));
            }
            else
            {
                invstrarr = new string[richTextBox1.Lines.Length];
                invstrarr = richTextBox1.Lines;
            }
        }


        private void NewDocument()
        {
            HtmlNode cd = new HtmlNode(HtmlNodeType.Document, docc, 0)
            {
                InnerHtml = CreateTemplate()
            };
        }
        public string[] GetLines()
        {
            if (richTextBox1.Text.Length > 20)
            {
                tmpArr = new string[richTextBox1.Lines.Length];
                tmpArr = richTextBox1.Lines;
                return tmpArr;
            }
            else
            {
                MessageBox.Show("Виберіть файл для редагування!");
                filepick.ShowDialog();
                return GetLines();
            }
        }
        public void UniInvoke()
        {
            switch (s.invoker)
            {
                case TextGen.Lead.DInvoke.getArr:
                    {

                        tmpArr = new string[richTextBox1.Lines.Length];
                        tmpArr = richTextBox1.Lines;
                        break;
                    }
                case TextGen.Lead.DInvoke.progrBar:
                    {
                        Int32 x = 0, iter = 0;
                        tmpArr = richTextBox1.Lines;
                        while (iter != tmpArr.Length)
                        {
                            if (tmpArr.GetValue(iter).ToString().Contains("//"))
                            {
                                x++;
                            }
                            iter++;
                        }
                        // customProgressBar1.Maximum = x;
                        //pbarqn.Visible = true;
                        break;
                    }
                case TextGen.Lead.DInvoke.genElem:
                    {
                        // TG.newP();
                        break;
                    }
                default:
                    break;
            }
        }
        public void InvokeTry()
        {
            try
            {
                s.invoker = TextGen.Lead.DInvoke.genElem;
                Application.DoEvents();
            }
            catch (TargetInvocationException x)
            {
                Console.WriteLine(x.ToString());
            }

            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
            }
        }
        public String CreateTemplate()
        {
            String str = "";

            str += "<!DOCTYPE html>\n";
            str += "\n";
            str += "<html>\n";
            str += "<head>\n";
            str += "	<meta http-equiv=\"X - UA - Compatible\" content=\"IE = 11\" >\n";
            str += "	<meta charset = \"utf-8\">\n";
            str += "	<meta name = \"viewport\" content = \"width=device-width, initial-scale=1\">\n";
            str += "	<link rel = \"stylesheet\" href = \"https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/css/bootstrap.min.css\">\n";
            str += "	<script src = \"https://code.jquery.com/jquery-3.1.1.slim.min.js\"></script>\n" +
                  "  <script src = \" https://cdnjs.cloudflare.com/ajax/libs/tether/1.4.0/js/tether.min.js\"></script>\n" +
                   "	<script src = \"https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/js/bootstrap.min.js\"></script>\n";


            str += "\n";
            str += "	<style type = \"text/css\">\n";
            str += "		.col-sm-2 {\n";
            str += "			overflow: auto;\n";
            str += "			margin: 5px;\n";
            str += "			border: #adadae;\n";
            str += "			border-width: 2px;\n";
            str += "			border-style: solid;\n";
            str += "		}\n";
            str += @"       #main>.container-fluid {  
                                margin: 3em auto 10em;
                            }";
            str += "\n";
            str += "		a:focus {\n";
            str += "			text-decoration: none;\n";
            str += "		}\n";
            str += "		.center{\n";
            str += "			text-align: center;\n";
            str += "		}\n";
            str += "\n";
            str += "	</style>\n";
            str += "\n";
            str += "<title>TESTS</title>\n";
            str += "</head>\n";
            str += "<body id=\"main\">\n";
            str += "\n";
            str += "<div class=\"container-fluid\">\n";
            str += "	<div class= \"row\">\n";
            str += "		<div class=\"container\">\n";
            str += "			<h2 class=\"center\">TESTS</h2>\n";
            str += "				<div id=\"accordion\" aria-multiselectable=\"true\" role=\"tablist\" >\n</div>\n";
            str += "			</div>\n";
            str += "		</div>\n";
            str += "	</div>\n";
            str += "</div>\n";


            str += "</body>\n";
            str += "</html>\n";

            return str;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Invoke(new Action(async () => { await FrameJsSaveSettings(); }));
            //   Thread.Sleep(2000);
            Invoke(new Action(() => { cef_started = false; }));
            Thread.Sleep(1500);
        }
     
        public void Exit()
        {
            Application.Exit();
        }
        public void SaveTXT()
        {
            string path = Properties.Settings.Default.TestsPath;
            if (path != "")
            {
                Invoke(new Action(async () =>
                {
                    StreamWriter outFile = new StreamWriter(path, false, Encoding.UTF8);
                    outFile.WriteLine(await FrameJsGetText());
                    outFile.Close();
                }));

                FrameJsShowInfo(Properties.Resources.TextSaved, 2000);
            }
            else
            {
                FrameJsShowInfo(Properties.Resources.CantFindDefPath, 3000, 300);
            }
        }
        public async void SaveAsTXT()
        {
            var fullp = Properties.Settings.Default.TestsPath;
            if (fullp != "")
            {
                FrameJsRequestName(Path.GetDirectoryName(fullp), ".txt");
                var path = await FrameJsGetName();
                if (path != null && path != "")
                {
                    Invoke(new Action(async () =>
                    {
                        StreamWriter outFile = new StreamWriter(Path.GetDirectoryName(fullp) + "\\" + path + ".txt", false, Encoding.UTF8);
                        outFile.WriteLine(await FrameJsGetText());
                        outFile.Close();
                    }
                    ));

                    FrameJsShowInfo(Properties.Resources.TextSaved, 2000);
                }
            }
            else FrameJsShowInfo(Properties.Resources.CantFindDefPath2, 4000,300);
        }
        public void SaveHTML()
        {
            try
            {
                string path = Properties.Settings.Default.IndexPath;
                if (path != "")
                {

                    StreamWriter outFile = new StreamWriter(path, false, Encoding.UTF8);
                    outFile.Write(docc.DocumentNode.OuterHtml.ToString());
                    outFile.Close();
                    FrameJsShowInfo(Properties.Resources.HtmlSaved, 2000, 900);
                }
                else if (Properties.Settings.Default.TestsPath != "")
                {
                        string fullp = Path.GetDirectoryName(Properties.Settings.Default.TestsPath) + "\\" + "index.html";
                        StreamWriter outFile = new StreamWriter(fullp, false, Encoding.UTF8);
                        outFile.Write(docc.DocumentNode.OuterHtml.ToString());
                        outFile.Close();
                        Properties.Settings.Default.IndexPath = fullp;

                        Properties.Settings.Default.Save();
                    FrameJsShowInfo(Properties.Resources.HtmlSaved, 2000, 900);
                }
               else  FrameJsShowInfo(Properties.Resources.HtmlNotAutoSaved, 4000, 400);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async void SaveAsHTML()
        {
            var fullp = Properties.Settings.Default.IndexPath;
            if (fullp != "" && docc.DocumentNode.ChildNodes.Count != 0)
            {
                FrameJsRequestName(Path.GetDirectoryName(fullp), ".html");
                var path = await FrameJsGetName();
                if (path != null && path != "")
                {
                    Invoke(new Action(() =>
                    {
                        string newpath = Path.GetDirectoryName(fullp) + "\\" + path + ".html";
                        StreamWriter outFile = new StreamWriter(newpath, false, Encoding.UTF8);
                        outFile.Write(docc.DocumentNode.OuterHtml.ToString());
                        outFile.Close();

                        Properties.Settings.Default.IndexPath = newpath;

                        Properties.Settings.Default.Save();
                    }
                    ));

                    FrameJsShowInfo(Properties.Resources.HtmlSaved, 2000);
                }

            }
            else { FrameJsShowInfo(Properties.Resources.ChouseFileFst, 2000); }          

        }
        public void Generate()
        {
            if (!backgroundWorker1.IsBusy)
            {
                string textinp;
                Action ac = async () =>
                {
                    try
                    {
                        textinp = await FrameJsGetText();
                        if (textinp.Length > 50)
                        {
                            richTextBox1.Text = textinp;
                            backgroundWorker1.RunWorkerAsync();
                        }
                        else
                        {
                            FrameJsShowInfo(Properties.Resources.TextToShort, 3000, 200);
                        }
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine(x.ToString());
                    }

                };
                if (InvokeRequired) Invoke(ac); else ac();
            }
        }
        public void Open()
        {
            try
            {
                Action ac = () =>
                {
                    filepick.FileName = "";
                    filepick.ShowDialog();
                    if (filepick.FileName != "")
                    {
                        try
                        {
                            String textinp = File.ReadAllText(filepick.FileName, Encoding.Default);
                            if (textinp != null)
                            {
                                var jstextinp = EncodeJsString(textinp);
                                var str = string.Format("\nx.value = {0}", jstextinp);
                                var str1 = "(function(){var x = document.getElementById(\'slave\'); " + str + " ;})(); " +
                                "showtab2();";

                                chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(str1);
                                richTextBox1.Text = textinp;
                            }

                            Properties.Settings.Default.TestsPath = filepick.FileName;
                            Properties.Settings.Default.Save();
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine(x.ToString());
                        }
                    }
                };
                if (InvokeRequired) Invoke(ac); else ac();
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
            }

        }
        public void Prefs()
        {
            Action ac = () =>
             {
                 PreferencesForm c = new PreferencesForm();
                 c.ShowDialog();
             };
            if (InvokeRequired) Invoke(ac); else ac();
        }
        public void StopGen()
        {
            stop = true;
        }
        public void Resfresh()
        {
            Action ac = () =>
            {
                chromeBrowser.Refresh();
            };
            if (InvokeRequired) Invoke(ac); else ac();
        }

        //Js Work
        private void FrameJsIsGenBtn()
        {
            string x = @"$('#loc-gen').fadeOut('fast'); setTimeout(function(){$('#loc-stop').fadeIn('fast');},200);";
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(x);
        }
        private void FrameJsNonGenBtn()
        {
            string x = @"$('#loc-stop').fadeOut('fast'); setTimeout(function(){$('#loc-gen').fadeIn('fast'); },200);";
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(x);
        }
        private void FrameJsPBMax(int maxval)
        {
            var x = "$( '#progressbar' ).progressbar({ max: " + maxval + "});";
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(x);
        }
        private void FrameJsSetVal(int val)
        {
            var x = "$('#progressbar').progressbar({ value:" + val + "}); ";
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(x);
        }
        private void FrameJsShowPB()
        {
            var x = "$('#pbwr').fadeIn(900)";
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(x);
        }
        private void FrameJsHidePB()
        {
            var x = "$('#pbwr').fadeOut(900)";
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(x);
        }
        private async void FrameJsRequestName(string path, string ext)
        {
            string x = @"(function(){
                             $('#default-path').text('" + EncodeJsString(path) + @"');
                             $('#default-ext').text('" + ext + @"');
                             $('#m2').modal('show');
                    })();";
            try
            {
                await ReturnJsFuncString(x);
            }
            catch (Exception xs) { Console.WriteLine(xs.ToString()); }

        }
        private async Task<string> FrameJsGetName()
        {
            string retval = null;
            try
            {
                string x = @"(function(){ if ($('#m2').hasClass('show')==false) { return $('#save-as-path').val(); };})();";

                while (retval == null)
                {
                    await Task.Delay(500).ContinueWith(_ =>
                    {
                        Action ac = async () => { retval = await ReturnJsFuncString(x); };
                        if (InvokeRequired) Invoke(ac); else ac();
                    });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return retval;

        }
        private async void FrameJsShowInfo(string text, int durationMS, int delay)
        {
            string x = @"(function(){ setTimeout(function(){ $('#info').text('" + text + "'); $('#info').fadeIn('slow'); setTimeout(function(){$('#info').fadeOut('slow');}," + durationMS + "); }, "+delay+"); })();";
            try
            {
                await ReturnJsFuncString(x);
            }
            catch (Exception xc)
            {
                Console.WriteLine(xc.ToString());
            }
        }
        private async void FrameJsShowInfo(string text, int durationMS)
        {
            string x = @"(function(){ $('#info').text('" + text + "'); $('#info').fadeIn('slow'); setTimeout(function(){$('#info').fadeOut('slow');}," + durationMS + ");  })();";
            try
            {
                await ReturnJsFuncString(x);
            }
            catch (Exception xc)
            {
                Console.WriteLine(xc.ToString());
            }
        }
        private async Task<string> ReturnJsFuncString(string script)
        {
            return await chromeBrowser.GetMainFrame().EvaluateScriptAsync(script).ContinueWith(t =>
            {
                var response = t.Result;
                return (response.Success && response.Result != null) ? (string)response.Result : null;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        private async Task<string> FrameJsGetText()
        {
            const string x = @"(function()
    					{
                            var x = document.getElementById('slave');
                            return x.value;
    					})();";
            return await ReturnJsFuncString(x);
        }
        private void FrameJsSetText(string text)
        {
            var str1 = "setFrame(" + text + ");" +
               "showtab1();";
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(str1);
        }
        private async Task FrameJsSaveSettings()
        {
            string x = @"(function(){ return $('#local').val();})();";

            string retval = null;
            Action ac = async () => { retval = await ReturnJsFuncString(x); };
            if (InvokeRequired) Invoke(ac); else ac();
            while (retval == null)
            {
                await Task.Delay(500);

            }
            int locint = Convert.ToInt32(retval);
            if (locint == 0)
            {
                Properties.Settings.Default.Local = Convert.ToInt32(local.def_En);
            }
            else
            {
                Properties.Settings.Default.Local = Convert.ToInt32(local.uk_UA);
            }

            Properties.Settings.Default.Save();
        }
        private async Task<string> FrameJsGetSettings()
        {
            string x = @"return $('#local').val();";
            return await ReturnJsFuncString(x);
        }
        private void FrameJsLocaleUA()
        {
            Invoke(new Action(async () => { await ReturnJsFuncString(Properties.Resources.localize_UA); }));
        }
        private void FrameJsFirstLaunch()
        {
            try
            {
                string cul;
                cul = (Thread.CurrentThread.CurrentUICulture.ToString() == "en-150") ? Properties.Resources.easy_start_tour_EN : Properties.Resources.easy_start_tour_UA;
             
                Invoke(new  Action(async () => { await ReturnJsFuncString(cul); }));
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Not inited");
            }
            
        }
        private static string EncodeJsString(string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }
    }

    public partial class TextGen : MainForm
    {
        public BackgroundWorker BW;
        public HtmlNode NodeAccrd;
        public string[] MainArr;


        public int[] mainques;
        public HtmlNode AccordElem;
        public HtmlNode[] ArchivePanel;
        public HtmlNode acrd;
        public HtmlAgilityPack.HtmlDocument doc;
        public Lead LV = new Lead();
        public class Lead
        {
            public volatile Int32 iter = 0, clss = 0, now = 0, collapseIter = 0, panelID = 0, currentPanel = 0, consIt = 0;
            public PanelsEnum x;
            public DInvoke invoker;
            public volatile int progress = 1;
            public enum PanelsEnum
            {
                paneldef, pvar
            };
            public enum DInvoke
            {
                getArr, progrBar, genElem, copytx
            };
            public string[] strArr;
            public int citr = 0;
            public char[] charsToTrim = { ' ', '/', '{', '}', '~', '%' };
        };
        public int GetLock()
        {
            ArchivePanel = new HtmlNode[MainArr.Length];
            mainques = new int[2];
            int x = 0;
            for (int i = 0; i < MainArr.Length; ++i)
            {
                if (MainArr[i].Contains("//"))
                {
                    if (MainArr[i].Length < 4)
                    {
                        do ++i;
                        while (MainArr[i].Length < 4);
                    }
                    if (x == mainques.Length)
                    {
                        mainques = Expand(mainques, 1);
                    }
                    mainques[x] = i; ++x;

                }

            }
            return mainques.Length;
        }
        public HtmlNode TimeStamp()
        {
            HtmlNode node = new HtmlNode(HtmlNodeType.Element, doc, LV.citr++);
            node.Name = "div";
            node.SetAttributeValue("class", "footer fixed-bottom bg-faded navbar");

            string time = "Generated on " + DateTime.Now.GetDateTimeFormats()[7];
            node.InnerHtml =
                            "    <div class=\"border-round w-50 text-center mx-auto pt-3 pb-3 m-auto\">" +
                            "      <p id = \"timewas\">" + time + "</p>" +
                            "      <a href = \"http://maxrev.pp.ua\"> Powered by MaxRev<span>©</span> 2017</a>" +
                            "   </div>";
            return node;
        }
        public void CheckStrPanel()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            CreateHTML_Panel(MainArr[mainques[LV.currentPanel]].Trim(LV.charsToTrim));
            Console.WriteLine("PanelTitle[" + (LV.currentPanel + 1).ToString() + "] - OK");

        }
        public void CheckStr()
        {
            String rex = MainArr[LV.iter];
            if (rex.Contains("~"))
            {
                CreateHTML_Answer(rex.Trim(LV.charsToTrim));
                Console.WriteLine("PanelAnswer[" + (LV.currentPanel + 1).ToString() + "][" + (++LV.consIt).ToString() + "] - OK");

            }
            else if (rex.Contains("%"))
            {
                CreateHTML_Answer("<strong>" + rex.Trim(LV.charsToTrim) + "</strong>");
                Console.WriteLine("PanelAnswer[" + (LV.currentPanel + 1).ToString() + "][" + (++LV.consIt).ToString() + "] - OK");

            }
            if (s.iter == MainArr.Length)
            {
                stop = true;
            }


        }

        private void CreateHTML_Panel(string title)
        {
            try
            {
                String collapseId = String.Concat("collapse", LV.currentPanel);
               // string tb = "\n\t\t\t\t\t\t";
                HtmlNode pandef = new HtmlNode(HtmlNodeType.Element, doc, LV.citr++)
                {
                    Name = "div"
                };
                pandef.SetAttributeValue("class", "card");
                pandef.SetAttributeValue("id", String.Concat("pan" + Convert.ToString(LV.currentPanel)));
             //  acrd.InnerHtml = tb;
                acrd.AppendChild(pandef);

                HtmlNode panelh = new HtmlNode(HtmlNodeType.Element, doc, LV.citr++)
                {
                    Name = "div"
                };
            //  panelh.InnerHtml = tb;
                panelh.SetAttributeValue("class", "card-header");
                panelh.SetAttributeValue("role", "tab");
                panelh.SetAttributeValue("id", "heading" + LV.currentPanel);

                pandef.AppendChild(panelh);
                HtmlNode panelt = new HtmlNode(HtmlNodeType.Element, doc, LV.citr++)
                {
                    Name = "h4"
                };
             //   panelt.InnerHtml = tb;
                panelt.SetAttributeValue("class", "mb-0");

                panelh.AppendChild(panelt);

                HtmlNode panela = new HtmlNode(HtmlNodeType.Element, doc, LV.citr++)
                {
                    Name = "a"
                };
                //panela.InnerHtml =tb;
                panela.SetAttributeValue("data-toggle", "collapse");
                panela.SetAttributeValue("data-parent", "#accordion");
                panela.SetAttributeValue("aria-expanded", "false");
                panela.SetAttributeValue("aria-controls", collapseId);
                panela.SetAttributeValue("href", String.Concat("#", collapseId));
                panela.InnerHtml = title;  /*MainArr[mainques[LV.currentPanel]].Trim(LV.charsToTrim)*/
                panelt.AppendChild(panela);

                HtmlNode _panelColl = new HtmlNode(HtmlNodeType.Element, doc, LV.citr++)
                {
                    Name = "div"
                };
             // _panelColl.InnerHtml=tb;
                _panelColl.SetAttributeValue("class", "collapse");
                _panelColl.SetAttributeValue("role", "tabpanel");
                _panelColl.SetAttributeValue("aria-labelledby", "heading" + LV.currentPanel);

                _panelColl.SetAttributeValue("id", collapseId);
                HtmlNode panelb = new HtmlNode(HtmlNodeType.Element, doc, LV.citr++)
                {
                    Name = "div"
                };
              // panelb.InnerHtml = tb;
                panelb.SetAttributeValue("class", "card-block row");
                _panelColl.AppendChild(panelb);

                pandef.AppendChild(_panelColl);
                ArchivePanel[LV.panelID++] = pandef;
                AccordElem.AppendChild(pandef);
                _panelColl = panela = panelb=pandef=panelt = null;
               // Thread.Sleep(100);
            } catch(Exception x) { MessageBox.Show(x.ToString()); }
        }
        private void CreateHTML_Answer(string answ)
        {
            while (true)
            {
                try
                {
                    HtmlNode curpan = doc.GetElementbyId(String.Concat("pan" + Convert.ToString(LV.currentPanel)));
                    HtmlNode panelB = curpan.ChildNodes[1].ChildNodes[0];
                    HtmlNode panelV = new HtmlNode(HtmlNodeType.Element, doc, LV.citr++)
                    {
                        Name = "div"
                    };
                    panelV.SetAttributeValue("class", "col-sm-2");
                    panelV.InnerHtml = answ;                   
                    panelB.AppendChild(panelV);
                    break;
                }
                catch (Exception x)
                {
                    Thread.Sleep(10);
                    Console.WriteLine("Exception - Not Found A Nest For Answer :" + x.ToString());
                }
            }
        }


        private int[] Expand(int[] x, int add)
        {
            int[] tmp = new int[x.Length + add];
            for (int i = 0; i < x.Length; ++i)
            {
                tmp[i] = x[i];
            }
            return tmp;
        }
        public class htmlForce
        {
            public htmlForce(string _text, string _name, int _padding)
            {
                Attributes = new Dictionary<string, string>();
                Name = _name;
                Text = _text;
                Padding = _padding;
            }
            public htmlForce(string _text, string _name)
            {
                Attributes = new Dictionary<string, string>();
                Name = _name;
                Text = _text;
                Padding = 1;
            }
            public htmlForce() {  }
            public override string ToString()
            {
                return "\r"+ new String('\t', Padding) + @"<" + Name + GetAttributes() + ">" + Text + "</" + Name + ">";
            }
            public void AddAttribute(string _name, string _value)
            {
                Attributes.Add(_name, _value);                
            }
            public void AppendChild(string _text)
            {
                Text = new String('\t', Padding) + _text;
            }
            public string Text
            { 
                get; set;
            }
            public string Name
            {
                get; set;
            }
            public int Padding { get; set; }
            public Dictionary<string, string> Attributes
            {
                get; set;
            }
            private string GetAttributes()
            {
                string all = "";
                foreach (var pair in Attributes)
                {
                    all += " " + pair.Key + "=\"" + pair.Value + "\"";
                }
                return all;
            }
        }

    }

};


public class CustomMenuHandler : CefSharp.IContextMenuHandler
{
    public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
    {
        model.Clear();
    }

    public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
    {
        return false;
    }

    public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
    {

    }

    public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
    {
        return false;
    }
}
namespace embebbedChromium
{
    class CefCustomObject
    {
        private static ChromiumWebBrowser _instanceBrowser = null;
        private static TextGen.MainForm _instanceMainForm = null;
        public CefCustomObject(ChromiumWebBrowser originalBrowser, TextGen.MainForm mainForm)
        {
            _instanceBrowser = originalBrowser;
            _instanceMainForm = mainForm;
        }
        public void dev()
        {
            try
            {
                _instanceBrowser.ShowDevTools();
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
                Console.WriteLine("Browser is not initialized!");
            }
        }

        public void open()
        {
            // _instanceBrowser.ShowDevTools();
            _instanceMainForm.Open();
        }
        public void preferences()
        {
            _instanceMainForm.Prefs();
        }
        public void savetxt()
        {
            _instanceMainForm.SaveTXT();
        }
        public void saveastxt()
        {
            _instanceMainForm.SaveAsTXT();
        }
        public void savehtml()
        {
            _instanceMainForm.SaveHTML();
        }
        public void saveashtml()
        {
            _instanceMainForm.SaveAsHTML();
        }
        public void exit()
        {
            _instanceMainForm.Exit();
        }
        public void generate()
        {
            _instanceMainForm.Generate();
        }
        public void refresh()
        {
            _instanceMainForm.Resfresh();
        }
        public void stop()
        {
            _instanceMainForm.StopGen();
        }
    }

}
