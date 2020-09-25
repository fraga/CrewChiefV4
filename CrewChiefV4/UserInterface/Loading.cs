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
        public static string tempSplashImagePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\CrewChiefV4\splash_image_tmp.png";
        public static string splashImagePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\CrewChiefV4\splash_image.png";
        public Loading()
        {
            InitializeComponent();
        }
    }
}
