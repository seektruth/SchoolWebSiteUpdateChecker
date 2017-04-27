using System;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace SchoolWebsiteUpdateChecker
{
    abstract class UpdateChecker
    {
        public string descrption;
        public string url;
        public string host;
        public string previousFileName;
        public HashSet<Record> notifys;
        public event Action<UpdateChecker, Record> newPosts;
        public abstract LinkedList<Record> parsePage();

        public void check()
        {
            var records = parsePage();
            foreach (var record in records)
            {
                if (notifys.Contains(record))
                {
                    postChanged(this, record);
                }
            }
            notifys = new HashSet<Record>(records);
            updatePreviousFile(records);
        }

        private void readPreviousNotifyFromFile()
        {
            notifys = new HashSet<Record>();
            var s = File.ReadAllText(previousFileName);
            foreach (string t in s.Split('\n'))
            {
                notifys.Add(new Record(t));
            }

        }

        private void updatePreviousFile(LinkedList<Record> notifys)
        {
            File.WriteAllText(previousFileName, String.Join("\n", notifys));
        }

        protected virtual void postChanged(UpdateChecker checker, Record record)
        {
            if (newPosts != null)
            {
                newPosts(checker, record);
            }
        }

        public void loadPrevious()
        {
            if (!File.Exists(previousFileName))
            {
                var records = parsePage();
                updatePreviousFile(records);
            }
            readPreviousNotifyFromFile();
        }
    }

    struct Record
    {
        string notifyName;
        string publishTime;

        public Record(string notifyName, string publishTime)
        {
            this.notifyName = notifyName;
            this.publishTime = publishTime;
        }

        public Record(string n_p)
        {
            var args = n_p.Split();
            notifyName = args[1];
            publishTime = args[0];
        }

        override public string ToString()
        {
            return publishTime + " " + notifyName;
        }
    }

    class BksyChecker: UpdateChecker
    { 
        public BksyChecker()
        {
            url = "http://bksy.zju.edu.cn/redir.php?catalog_id=711393";
            descrption = "本科生院";
            host = "http://bksy.zju.edu.cn/";
            previousFileName = "bksy.json";
            loadPrevious();
        }

        override public LinkedList<Record> parsePage()
        {
            var records = new LinkedList<Record>();
            var httpCLient = new WebClient();
            String html = httpCLient.DownloadString(url);
            var htmlParser = new HtmlDocument();
            htmlParser.LoadHtml(html);
            var articals = htmlParser.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[2]/ul[1]/li");
            foreach (var artical in articals)
            {
                var t = artical.ChildNodes[1];
                var href = t.Attributes["href"].Value;
                var title = t.Attributes["title"].Value;
                records.AddLast(new Record(href, title));
            }
            return records;
        }
    }
}
