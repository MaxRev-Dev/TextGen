using System; 
using System.Threading;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace TextGen
{
    public class TextGen : MainForm
    {
        public HtmlNode NodeAccrd;
        public string[] MainArr;


        public int[] mainques;
        public HtmlNode AccordElem;
        public HtmlNode[] ArchivePanel;
        public HtmlNode acrd;
        public HtmlAgilityPack.HtmlDocument doc;
        public ParserState State = new ParserState();
        public class ParserState
        {
            public volatile int iter, panelID, currentPanel, consIt; 
            public int citr;
            public char[] charsToTrim = { ' ', '/', '{', '}', '~', '%' };
        }
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
            HtmlNode node = new HtmlNode(HtmlNodeType.Element, doc, State.citr++);
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
            CreateHTML_Panel(MainArr[mainques[State.currentPanel]].Trim(State.charsToTrim));
            Console.WriteLine($@"PanelTitle[{(State.currentPanel + 1)}] - OK");
        }
        public void CheckStr()
        {
            string rex = MainArr[State.iter];
            if (rex.Contains("~"))
            {
                CreateHTML_Answer(rex.Trim(State.charsToTrim));
                Console.WriteLine($@"PanelAnswer[{(State.currentPanel + 1)}][{(++State.consIt)}] - OK");

            }
            else if (rex.Contains("%"))
            {
                CreateHTML_Answer("<strong>" + rex.Trim(State.charsToTrim) + "</strong>");
                Console.WriteLine($@"PanelAnswer[{(State.currentPanel + 1)}][{(++State.consIt)}] - OK");

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
                String collapseId = String.Concat("collapse", State.currentPanel);
                // string tb = "\n\t\t\t\t\t\t";
                HtmlNode pandef = new HtmlNode(HtmlNodeType.Element, doc, State.citr++)
                {
                    Name = "div"
                };
                pandef.SetAttributeValue("class", "card");
                pandef.SetAttributeValue("id", $"pan{Convert.ToString(State.currentPanel)}");
                //  acrd.InnerHtml = tb;
                acrd.AppendChild(pandef);

                HtmlNode panelh = new HtmlNode(HtmlNodeType.Element, doc, State.citr++)
                {
                    Name = "div"
                };
                //  panelh.InnerHtml = tb;
                panelh.SetAttributeValue("class", "card-header");
                panelh.SetAttributeValue("role", "tab");
                panelh.SetAttributeValue("id", "heading" + State.currentPanel);

                pandef.AppendChild(panelh);
                HtmlNode panelt = new HtmlNode(HtmlNodeType.Element, doc, State.citr++)
                {
                    Name = "h4"
                };
                //   panelt.InnerHtml = tb;
                panelt.SetAttributeValue("class", "mb-0");

                panelh.AppendChild(panelt);

                HtmlNode panela = new HtmlNode(HtmlNodeType.Element, doc, State.citr++)
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

                HtmlNode _panelColl = new HtmlNode(HtmlNodeType.Element, doc, State.citr++)
                {
                    Name = "div"
                };
                // _panelColl.InnerHtml=tb;
                _panelColl.SetAttributeValue("class", "collapse");
                _panelColl.SetAttributeValue("role", "tabpanel");
                _panelColl.SetAttributeValue("aria-labelledby", "heading" + State.currentPanel);

                _panelColl.SetAttributeValue("id", collapseId);
                HtmlNode panelb = new HtmlNode(HtmlNodeType.Element, doc, State.citr++)
                {
                    Name = "div"
                };
                // panelb.InnerHtml = tb;
                panelb.SetAttributeValue("class", "card-block row");
                _panelColl.AppendChild(panelb);

                pandef.AppendChild(_panelColl);
                ArchivePanel[State.panelID++] = pandef;
                AccordElem.AppendChild(pandef);
                // Thread.Sleep(100);
            }
            catch (Exception x) { MessageBox.Show(x.ToString()); }
        }
        private void CreateHTML_Answer(string answ)
        {
            try
            {
                HtmlNode curpan = doc.GetElementbyId($"pan{Convert.ToString(State.currentPanel)}");
                HtmlNode panelB = curpan.ChildNodes[1].ChildNodes[0];
                HtmlNode panelV = new HtmlNode(HtmlNodeType.Element, doc, State.citr++)
                {
                    Name = "div"
                };
                panelV.SetAttributeValue("class", "col-sm-2");
                panelV.InnerHtml = answ;
                panelB.AppendChild(panelV); 
            }
            catch (Exception x)
            {
                Thread.Sleep(10);
                Console.WriteLine(@"Exception - Not Found A Nest For Answer :" + x);
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
    }
}