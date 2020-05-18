using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace CrewChiefV4
{
    public partial class HelpWindow : Form
    {
        private WebBrowser webBrowser1;

        public HelpWindow(System.Windows.Forms.Form parent)
        {
            StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();
            string p = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            //this.Text = p;
            //Uri uri = new Uri(p+@"\..\..\..\Public\index.html");
            //For now at least load the help directly from the Internet
            //so it can be changed without having to update CC
            Uri uri = new Uri(@"https://tonywhitley.github.io/Help/index.html");
            webBrowser1.Navigate(uri);
            webBrowser1.Navigating += webBrowser1_Navigating;
        }

        public void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (!e.Url.AbsolutePath.StartsWith("/Help/") &&
                !e.Url.AbsolutePath.StartsWith("/Public/") &&
                e.Url.Scheme != "file")
            {   // Browser loading external page (e.g. Paypal)
                //cancel the current event
                e.Cancel = true;

                //this opens the URL in the user's default browser
                Process.Start(e.Url.ToString());
            }
            // else loading another page of CC Help
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(735, 634);
            this.webBrowser1.TabIndex = 0;
            // 
            // HelpWindow
            // 
            this.ClientSize = new System.Drawing.Size(735, 634);
            this.Controls.Add(this.webBrowser1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HelpWindow";
            this.Text = "Crew Chief Help";
            this.ResumeLayout(false);

        }

        /*private void HelpWindow_Load(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            StreamReader reader = new StreamReader(assembly.GetManifestResourceStream("README.HTML"));
            webBrowser1.DocumentText = reader.ReadToEnd();
        }*/
    }
}
