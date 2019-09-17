using CefSharp;
using CefSharp.WinForms;
using HtmlAgilityPack;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                OptionFixNestedTags = true,
                OptionOutputOriginalCase = true,
            };
            filepick.FileName = "";
        }
        private enum local
        {
            default_En, uk_UA
        }

        private string page;
        private bool FirstLaunchNeeded;
        protected TextGen.ParserState s = new TextGen.ParserState();
        private volatile bool cef_started;
        public string[] invstrarr;
        private string jsIn = "";
        public volatile bool stop;


        public void InitializeChromium()
        {
            using (var settings = new CefSettings())
            {
                CefSharpSettings.LegacyJavascriptBindingEnabled = true;
                page = string.Format(@"{0}\Resources\Interface.html", Application.StartupPath);

                if (!File.Exists(page))
                {
                    MessageBox.Show(@"Error The html file doesn't exists : " + page);
                }
                if (!Cef.IsInitialized)
                {
                    Cef.Initialize(settings);
                }
            }

            chromeBrowser = new ChromiumWebBrowser(page);

            Invoke(new Action(() => Controls.Add(chromeBrowser)));

            chromeBrowser.RegisterJsObject("CefObj", new CefCustomObject(chromeBrowser, this));
            chromeBrowser.IsBrowserInitializedChanged += ChrFix;

        }

        private void ChrFix(object sender, IsBrowserInitializedChangedEventArgs e)
        {
            chromeBrowser.Dock = DockStyle.Fill;
            chromeBrowser.MenuHandler = new CustomMenuHandler();
            int x = Properties.Settings.Default.Local;
            if (x == Convert.ToInt32(local.default_En))
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

        private void Form1_Load(object sender, EventArgs e)
        {
            _ = StartChromiumAsync();

            string textinp = null;
            var testsPath = Properties.Settings.Default.TestsPath;
            if (testsPath != default)
                textinp = File.ReadAllText(testsPath, Encoding.Default);
            else FirstLaunchNeeded = true;
            if (textinp != null)
            {
                var jstextinp = EncodeJsString(textinp);
                var str = string.Format("\nx.value = {0}", jstextinp);
                jsIn = "(function(){var x = document.getElementById(\'slave\'); " + str + " ;})(); " +
               "showtab2();";

                richTextBox1.Text = textinp;
            }
        }
        private Task StartChromiumAsync() //Thread of CefSharp
        {
            if (!cef_started)
            {
                InitializeChromium();
                cef_started = true;
            }
            return Task.CompletedTask;
        }

        private void GeneratedEvents(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                HtmlNode xd = docc.DocumentNode.SelectSingleNode("/html[1]/body[1]");
                var jstextinp = EncodeJsString(xd.InnerHtml);
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
                Console.WriteLine(@"Browser instance is null");
            }
        }
        private void MainBackgroundTask(object sender, DoWorkEventArgs e)
        {
            try
            {
                FrameJsShowInfo(Properties.Resources.Gen, 1000);
                FrameJsIsGenBtn();

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

                TG.doc = new HtmlAgilityPack.HtmlDocument()
                {
                    OptionUseIdAttribute = true
                };
                TG.doc.LoadHtml(TG.NodeAccrd.OuterHtml);
                TG.AccordElem = TG.NodeAccrd;
                TG.acrd = TG.doc.GetElementbyId("accordion");


                if (InvokeRequired) Invoke(ac); else ac();

                for (TG.State.currentPanel = 0; TG.State.currentPanel < TG.mainques.Length; ++TG.State.currentPanel)
                {
                    TG.CheckStrPanel();
                    int curpan = TG.mainques[TG.State.currentPanel] + 1, nextpan;
                    if (TG.State.currentPanel != TG.mainques.Length - 1) nextpan = TG.mainques[TG.State.currentPanel + 1];
                    else
                    {
                        curpan = TG.mainques[TG.mainques.Length - 1] + 1;
                        nextpan = TG.MainArr.Length - 1;
                    }

                    for (TG.State.iter = curpan; TG.State.iter < nextpan; TG.State.iter++)
                    {
                        if (TG.State.iter <= TG.MainArr.Length)
                        {
                            Thread StringAnswerThread = new Thread(TG.CheckStr);
                            StringAnswerThread.Start();
                            StringAnswerThread.Join();
                        }
                        Application.DoEvents();
                        ac = () =>
                        {
                            FrameJsSetVal(TG.State.currentPanel);
                        };
                        if (InvokeRequired) Invoke(ac); else ac();
                    }
                    Console.WriteLine(@"Completed: " + (TG.State.currentPanel + 1));
                    if (stop) break;
                }
                stop = true;

                FrameJsShowInfo(Properties.Resources.Wait, 3000);
                docc.OptionUseIdAttribute = true;
                docc.GetElementbyId("main").AppendChild(TG.TimeStamp());

                TextGen tg = TG;
                ac = () =>
                  {
                      FrameJsSetVal(tg.mainques.Length);
                      Thread.Sleep(3000);
                      FrameJsHidePB();
                  };
                if (InvokeRequired) BeginInvoke(ac); else ac();
                Thread.Sleep(800);
            }
            catch (Exception x) { Console.WriteLine(x); }
        }


        //helpers
        public HtmlAgilityPack.HtmlDocument docc;
        public HtmlNode LoadHtml()
        {
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
                    var html = CreateTemplate();
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

        public string CreateTemplate()
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
            Invoke(new Action(async () => { await FrameJsSaveSettingsAsync().ConfigureAwait(false); }));
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
                    await outFile.WriteLineAsync(await FrameJsGetTextAsync().ConfigureAwait(false)).ConfigureAwait(false);
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
                var path = await FrameJsGetNameAsync().ConfigureAwait(false);
                if (path != null && path != "")
                {
                    Invoke(new Action(async () =>
                    {
                        StreamWriter outFile = new StreamWriter(Path.GetDirectoryName(fullp) + "\\" + path + ".txt", false, Encoding.UTF8);
                        await outFile.WriteLineAsync(await FrameJsGetTextAsync().ConfigureAwait(false)).ConfigureAwait(false);
                        outFile.Close();
                    }
                    ));

                    FrameJsShowInfo(Properties.Resources.TextSaved, 2000);
                }
            }
            else FrameJsShowInfo(Properties.Resources.CantFindDefPath2, 4000, 300);
        }
        public void SaveHTML()
        {
            try
            {
                string path = Properties.Settings.Default.IndexPath;
                if (path != "")
                {

                    StreamWriter outFile = new StreamWriter(path, false, Encoding.UTF8);
                    outFile.Write(docc.DocumentNode.OuterHtml);
                    outFile.Close();
                    FrameJsShowInfo(Properties.Resources.HtmlSaved, 2000, 900);
                }
                else if (Properties.Settings.Default.TestsPath != "")
                {
                    string fullp = Path.GetDirectoryName(Properties.Settings.Default.TestsPath) + "\\" + "index.html";
                    StreamWriter outFile = new StreamWriter(fullp, false, Encoding.UTF8);
                    outFile.Write(docc.DocumentNode.OuterHtml);
                    outFile.Close();
                    Properties.Settings.Default.IndexPath = fullp;

                    Properties.Settings.Default.Save();
                    FrameJsShowInfo(Properties.Resources.HtmlSaved, 2000, 900);
                }
                else FrameJsShowInfo(Properties.Resources.HtmlNotAutoSaved, 4000, 400);
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
                var path = await FrameJsGetNameAsync().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(path))
                {
                    Invoke(new Action(() =>
                    {
                        string newpath = Path.GetDirectoryName(fullp) + "\\" + path + ".html";
                        StreamWriter outFile = new StreamWriter(newpath, false, Encoding.UTF8);
                        outFile.Write(docc.DocumentNode.OuterHtml);
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
                string frameJsGetText;
                Action ac = async () =>
                {
                    try
                    {
                        frameJsGetText = await FrameJsGetTextAsync().ConfigureAwait(false);
                        if (frameJsGetText.Length > 50)
                        {
                            richTextBox1.Text = frameJsGetText;
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
                            var readAllText = File.ReadAllText(filepick.FileName, Encoding.Default);
                            var jstextinp = EncodeJsString(readAllText);
                            var str = string.Format("\nx.value = {0}", jstextinp);
                            var str1 = "(function(){var x = document.getElementById(\'slave\'); " + str + " ;})(); " +
                                       "showtab2();";

                            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(str1);
                            richTextBox1.Text = readAllText;

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
                await ReturnJsFuncStringAsync(x).ConfigureAwait(false);
            }
            catch (Exception xs) { Console.WriteLine(xs.ToString()); }

        }
        private async Task<string> FrameJsGetNameAsync()
        {
            string retval = null;
            try
            {
                string x = @"(function(){ if ($('#m2').hasClass('show')==false) { return $('#save-as-path').val(); };})();";

                while (retval == null)
                {
                    await Task.Delay(500).ContinueWith(_ =>
                    {
                        Action ac = async () => { retval = await ReturnJsFuncStringAsync(x).ConfigureAwait(false); };
                        if (InvokeRequired) Invoke(ac); else ac();
                    }).ConfigureAwait(false);

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
            string x = @"(function(){ setTimeout(function(){ $('#info').text('" + text + "'); $('#info').fadeIn('slow'); setTimeout(function(){$('#info').fadeOut('slow');}," + durationMS + "); }, " + delay + "); })();";
            try
            {
                await ReturnJsFuncStringAsync(x).ConfigureAwait(false);
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
                await ReturnJsFuncStringAsync(x).ConfigureAwait(false);
            }
            catch (Exception xc)
            {
                Console.WriteLine(xc.ToString());
            }
        }
        private async Task<string> ReturnJsFuncStringAsync(string script)
        {
            return await (await chromeBrowser.GetMainFrame().EvaluateScriptAsync(script).ContinueWith(async t =>
            {
                var response = await t.ConfigureAwait(false);
                return response.Success && response.Result != null ? (string)response.Result : null;
            }).ConfigureAwait(false)).ConfigureAwait(false);
        }
        private Task<string> FrameJsGetTextAsync()
        {
            const string x = @"(function()
    					{
                            var x = document.getElementById('slave');
                            return x.value;
    					})();";
            return ReturnJsFuncStringAsync(x);
        }
        private void FrameJsSetText(string text)
        {
            var str1 = "setFrame(" + text + ");" +
               "showtab1();";
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(str1);
        }
        private async Task FrameJsSaveSettingsAsync()
        {
            string x = @"(function(){ return $('#local').val();})();";

            string retval = null;

            async void Ac()
            {
                retval = await ReturnJsFuncStringAsync(x).ConfigureAwait(false);
            }

            if (InvokeRequired) Invoke((Action)Ac); else Ac();
            while (retval == null)
            {
                await Task.Delay(500).ConfigureAwait(false);
            }
            int locint = Convert.ToInt32(retval);
            if (locint == 0)
            {
                Properties.Settings.Default.Local = Convert.ToInt32(local.default_En);
            }
            else
            {
                Properties.Settings.Default.Local = Convert.ToInt32(local.uk_UA);
            }

            Properties.Settings.Default.Save();
        }
        private void FrameJsLocaleUA()
        {
            Invoke(new Action(async () =>
            {
                // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
                await ReturnJsFuncStringAsync(Properties.Resources.localize_UA).ConfigureAwait(false);
            }));
        }
        private void FrameJsFirstLaunch()
        {
            try
            {
                string cul;
                cul = (Thread.CurrentThread.CurrentUICulture.ToString() == "en-150") ? Properties.Resources.easy_start_tour_EN : Properties.Resources.easy_start_tour_UA;

                Invoke(new Action(async () =>
               {
                   await ReturnJsFuncStringAsync(cul).ConfigureAwait(false);
               }));
            }
            catch (NullReferenceException)
            {
                Console.WriteLine(@"Not inited");
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
                        int i = c;
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
};
