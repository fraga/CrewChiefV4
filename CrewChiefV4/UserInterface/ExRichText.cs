using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CrewChiefV4
{
    public class ExRichText : RichTextBox
    {
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                try
                {
                    Utilities.LoadLibrary("MsftEdit.dll"); // Available since XP SP1
                    cp.ClassName = "RichEdit50W";
                }
                catch { /* Windows XP without any Service Pack.*/ }
                return cp;
            }
        }
    }
}