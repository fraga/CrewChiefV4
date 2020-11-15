using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrewChiefV4.UserInterface
{
    public partial class Loading : Form
    {
        public static string splashImageFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\CrewChiefV4\";
        public static string tempSplashImagePath = splashImageFolderPath + "splash_image_tmp.png";
        public static string splashImagePath = splashImageFolderPath + "splash_image.png";
        public Loading()
        {
            ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));

            InitializeComponent();
        }
    }
}
