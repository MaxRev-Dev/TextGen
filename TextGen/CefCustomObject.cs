using System;
using CefSharp;
using CefSharp.WinForms;

namespace TextGen
{
    internal class CefCustomObject
    {
        private static ChromiumWebBrowser _instanceBrowser;
        private static MainForm _instanceMainForm;
        public CefCustomObject(ChromiumWebBrowser originalBrowser, MainForm mainForm)
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
                Console.WriteLine(@"Browser is not initialized!");
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