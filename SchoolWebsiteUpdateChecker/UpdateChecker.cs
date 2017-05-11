using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Threading;

namespace SchoolWebsiteUpdateChecker
{
    abstract class UpdateChecker
    {
        public string descrption;
        public string url;
        public string previousFileName;
        public HashSet<string> notifys;
        public event Action<UpdateChecker, string> newPosts;
        public event Action<UpdateChecker> netWorkError;
        public event Action<UpdateChecker> noUpdate;
        public abstract List<string> parsePage();
        public MenuItem connnectItem;

        public UpdateChecker(MenuItem item)
        {
            this.connnectItem = item;
        }

        public void check()
        {
            var records = parsePage();
            bool hasUpdate = false;
            if (records == null)
            {
                return;
            }
            foreach (var record in records)
            {
                if (!notifys.Contains(record))
                {
                    hasUpdate = true;
                    postChanged(this, record);
                    Thread.Sleep(1000);
                }
            }
            if (!hasUpdate)
            {
                everyThingUpdate();
                Thread.Sleep(1000);
            }
            notifys = new HashSet<string>(records);
            updatePreviousFile(records);
        }

        private void readPreviousNotifyFromFile()
        {
            notifys = new HashSet<string>();
            var s = File.ReadAllText(previousFileName);
            foreach (string t in s.Split('\n'))
            {
                notifys.Add(t);
            }

        }

        private void updatePreviousFile(List<string> notifys)
        {
            var content = notifys != null ? String.Join("\n", notifys) : "";
            File.WriteAllText(previousFileName, content);
        }

        protected virtual void postChanged(UpdateChecker checker, string record)
        {
            newPosts?.Invoke(checker, record);
        }

        protected virtual void NetWorkError()
        {
            netWorkError?.Invoke(this);
        }

        protected virtual void everyThingUpdate()
        {
            noUpdate?.Invoke(this);
        }

        public void loadPrevious()
        {
            if (!File.Exists(previousFileName))
            {
                File.WriteAllText(previousFileName, "");
            }
            readPreviousNotifyFromFile();
        }
    }


    class Cc98Checker: UpdateChecker
    {
        public Cc98Checker(MenuItem item): base(item)
        {
            url = "http://www.cc98.org/hottopic.asp";
            descrption = "cc98";
            previousFileName = "cc98.json";
            loadPrevious();
        }

        public override List<string> parsePage()
        {
            string html;
            var records = new List<string>();
            var httpCLient = new WebClient();
            try
            {
                var bs = httpCLient.DownloadData(url);
                html = Encoding.UTF8.GetString(bs);
            }
            catch (WebException)
            {
                NetWorkError();
                return null;
            }
            
            var htmlParser = new HtmlAgilityPack.HtmlDocument();
            htmlParser.LoadHtml(html);
            var articals = htmlParser.DocumentNode.SelectNodes("/html[1]/body[1]/table[4]/tr");
            articals.RemoveAt(0);
            foreach(var artical in articals)
            {
                var t = artical.ChildNodes[1].ChildNodes[0].ChildNodes[0].InnerText;
                t = t.Replace("\r", "");
                records.Add(t);
            }
            return records;
        }
    }

    class JwwChecker: UpdateChecker
    {
        public JwwChecker(MenuItem item) : base(item)
        {
            url = "http://10.202.78.12/jwggcx.aspx?type=1";
            descrption = "教务网";
            previousFileName = "jww.json";
            loadPrevious();
        }

        public override List<string> parsePage()
        {
            string html;
            var httpCLient = new WebClient();
            var records = new List<string>();
            try
            {
                html = httpCLient.DownloadString(url);
            }
            catch (WebException)
            {
                NetWorkError();
                return null;
            }
            var htmlParser = new HtmlAgilityPack.HtmlDocument();
            htmlParser.LoadHtml(html);
            var articals = htmlParser.DocumentNode.SelectNodes("/html[1]/body[1]/div[2]/div[3]/span[1]/fieldset[1]/table[1]/tr");
            articals.Remove(21);
            articals.Remove(0);
            foreach(var artical in articals)
            {
                var t = artical.ChildNodes[1].InnerText;
                t = t.Replace("\n", "");
                records.Add(t);
            }
            return records;
        }
    }

    class JyChecker: UpdateChecker
    {

        public JyChecker(MenuItem item) : base(item)
        {
            url = "http://cspo.zju.edu.cn/redir.php?catalog_id=20";
            descrption = "计算机学院";
            previousFileName = "jy.json";
            loadPrevious();
        }

        public override List<string> parsePage()
        {
            string html;
            var records = new List<string>();
            var httpCLient = new WebClient();
            try
            {
                html = httpCLient.DownloadString(url);
            }
            catch (WebException)
            {
                NetWorkError();
                return null;
            }
            var htmlParser = new HtmlAgilityPack.HtmlDocument();
            htmlParser.LoadHtml(html);
            var articals = htmlParser.DocumentNode.SelectNodes("/html[1]/body[1]/div[2]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[2]/table[2]/tr[1]/td[1]/table[1]/tr");
            articals.RemoveAt(59);
            foreach (var artical in articals)
            {
                if(artical.InnerText != "\r\n            \r\n                      " && artical.InnerText != "\r\n    即时更新\r\n\t")
                {
                    var t = artical.InnerText;
                    t = t.Replace("\n", "");
                    records.Add(t);
                }
            }
            return records;
        }
    }

    class BksyChecker: UpdateChecker
    { 
        public BksyChecker(MenuItem item) : base(item)
        {
            url = "http://bksy.zju.edu.cn/redir.php?catalog_id=711393";
            descrption = "本科生院";
            previousFileName = "bksy.json";
            loadPrevious();
        }

        override public List<string> parsePage()
        {
            string html;
            var records = new List<string>();
            var httpCLient = new WebClient();
            try
            {
                html = httpCLient.DownloadString(url);
            }
            catch (WebException)
            {
                NetWorkError();
                return null;
            }            
            var htmlParser = new HtmlAgilityPack.HtmlDocument();
            htmlParser.LoadHtml(html);
            var articals = htmlParser.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[2]/ul[1]/li");
            foreach (var artical in articals)
            {
                var t = artical.ChildNodes[1];
                var title = t.Attributes["title"].Value;
                title = title.Replace("\n", "");
                records.Add(title);
            }
            return records;
        }
    }
}
