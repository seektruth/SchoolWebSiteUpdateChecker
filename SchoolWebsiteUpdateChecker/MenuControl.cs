using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace SchoolWebsiteUpdateChecker
{
    class MenuController
    {
        public NotifyIcon notifyIcon;
        public ContextMenu contextMenu;
        public MenuItem exitItem;
        public MenuItem startOnBoot;
        public MenuItem checkNow;
        public MenuItem ListenWhat;
        public MenuItem ListenBksy;
        public MenuItem ListenCC98;
        public MenuItem ListenJww;
        public MenuItem ListenJy;
        private UpdateChecker[] checkers;
        public int timeInterval = 1000 * 60 * 10;
        public string currentUrl;
        public string defaultConfig = "startup=false\nJww=true\nBksy=true\nCC98=true\nJy=true";
        public string configFileName = "config.txt";
        Thread monitor;

        public MenuController()
        {
            initNotifyIcon();
            LoadConfig();
            checkers = new UpdateChecker[] { new BksyChecker(this.ListenBksy),new JwwChecker(this.ListenJww),
                                            new Cc98Checker(this.ListenCC98),new JyChecker(this.ListenJy) };
            foreach (var checker in checkers)
            {
                checker.newPosts += ShowNewPost;
                checker.netWorkError += ShowNetWorkError;
                checker.noUpdate += ShowNoUpdate;
            }
            monitor = new Thread(this.Monitor);
            monitor.Start();
        }

        private void initNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            contextMenu = new ContextMenu(new MenuItem[]
            {
                this.ListenWhat = new MenuItem("选择检查的网站",new MenuItem[]{
                    this.ListenCC98 = new MenuItem("CC98",new EventHandler(ItemClicked)),
                    this.ListenJww = new MenuItem("教务网",new EventHandler(ItemClicked)),
                    this.ListenJy = new MenuItem("计算机学院",new EventHandler(ItemClicked)),
                    this.ListenBksy = new MenuItem("本科生院",new EventHandler(ItemClicked))
                    }),
                this.checkNow = new MenuItem("现在检查", new EventHandler(this.Check)),
                this.startOnBoot = new MenuItem("开机启动", new EventHandler(this.StartOnBoot)),
                this.exitItem = new MenuItem("退出", new EventHandler(this.exitClick))
            });
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Icon = new Icon("app.ico");
            notifyIcon.Text = "校园网站监听器";
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipClicked += visitWeb;
        }

        public void LoadConfig()
        {
            if (!File.Exists(configFileName))
            {
                File.WriteAllText("config.txt", defaultConfig);
            }
            var config = File.ReadAllText(configFileName);
            var d = new Dictionary<string, string>();
            foreach(var c in config.Split('\n'))
            {
                var t = c.Split('=');
                d.Add(t[0], t[1]);
            }
            startOnBoot.Checked = d["startup"] == "true";
            ListenBksy.Checked = d["Bksy"] == "true";
            ListenCC98.Checked = d["CC98"] == "true";
            ListenJww.Checked = d["Jww"] == "true";
            ListenJy.Checked = d["Jy"] == "true";
        }

        public void UpdateConfig()
        {
            var config = $"startup={startOnBoot.Checked}\n" +
                $"Bksy={ListenBksy.Checked}\n" +
                $"Jww={ListenJww.Checked}\n" +
                $"Jy={ListenJy.Checked}\n" +
                $"CC98={ListenCC98.Checked}";
            File.WriteAllText(configFileName, config);
        }

        public void ItemClicked(object sender, EventArgs e)
        {
            var t = (MenuItem)sender;
            t.Checked = !t.Checked;
        }

        public void Monitor()
        {
            Thread.Sleep(1000 * 600);
            Check();
        }

        public void Check()
        {
           foreach(var check in checkers)
            {
                if (check.connnectItem.Checked)
                {
                    check.check();
                }
            }
        }

        public void Check(object sender, EventArgs e)
        {
            Check();
        }

        public void visitWeb(object sender, EventArgs e)
        {
            Console.WriteLine(currentUrl);
            System.Diagnostics.Process.Start(currentUrl);
        }

        public void ShowNewPost(UpdateChecker checker, string record)
        {
            notifyIcon.BalloonTipTitle = $"{checker.descrption}有更新";
            notifyIcon.BalloonTipText = record;
            currentUrl = checker.url;
            notifyIcon.ShowBalloonTip(1000);
        }

        public void ShowNetWorkError(UpdateChecker checker)
        {
            notifyIcon.BalloonTipTitle = $"无法访问{checker.descrption}";
            notifyIcon.BalloonTipText = $"网络错误，无法访问{checker.descrption}";
            currentUrl = checker.url;
            notifyIcon.ShowBalloonTip(1000);
        }

        public void ShowNoUpdate(UpdateChecker checker)
        {
            notifyIcon.BalloonTipTitle = $"{checker.descrption}";
            notifyIcon.BalloonTipText = $"{checker.descrption}, everything update";
            currentUrl = checker.url;
            notifyIcon.ShowBalloonTip(1000);
        }

        public void exitClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            monitor.Abort();
            UpdateConfig();
            Application.Exit();
        }

        public void StartOnBoot(object sender, EventArgs e)
        {
            startOnBoot.Checked = !startOnBoot.Checked;
            SetAutoStart(startOnBoot.Checked);
        }

        public static void SetAutoStart(bool enabled, RegistryHive hive = RegistryHive.CurrentUser)
        {
            string ExecutablePath = Assembly.GetEntryAssembly().Location;
            string Entry = "网站更新监听器" + Application.StartupPath.GetHashCode();

            var Key = RegistryKey.OpenBaseKey(hive,
                        Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32)
                    .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (enabled)
            {
                Key.SetValue(Entry, ExecutablePath);
            }
            else
            {
                Key.DeleteValue(Entry);
            }
        }
    }
}
