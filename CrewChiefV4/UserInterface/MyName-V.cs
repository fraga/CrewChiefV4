using CrewChiefV4.UserInterface.Models;
using CrewChiefV4.UserInterface.VMs;

using System;
using System.Windows.Forms;

namespace CrewChiefV4.UserInterface
{
    /// <summary>
    /// View module of MyName dialog MVVM
    /// </summary>
    public partial class MyName_V : Form
    {
        private readonly MyName_VM vm;
        private readonly MyName model;
        private readonly MainWindow mwi;
        public MyName_V(MainWindow _mwi, string oldName)
        {
            mwi = _mwi;
            InitializeComponent();
            vm = new MyName_VM(this);
            model = new MyName(vm);
            labelEnterYourName.Text = Configuration.getUIString("enter_your_name");
            textBoxMyName.Text = oldName;
            labelFullPersonalisation.Text = Configuration.getUIString("full_personalisation");
            labelDriverName.Text = Configuration.getUIString("driver_name");
            labelOtherDriverName.Text = Configuration.getUIString("other_driver_name");
            //buttonPlayName.Text = Configuration.getUIString("play_name_sample");
            buttonNameSelect.Text = Configuration.getUIString("select");
            if (textBoxMyName.Text.Length > 0)
            {
                // Run the model
                model.NameEntry(textBoxMyName.Text);
            }
        }

        private void textBoxMyName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && textBoxMyName.Text.Length > 0)
            {
                // Run the model
                model.NameEntry(textBoxMyName.Text);
                e.SuppressKeyPress = true;  // Prevent the error beep
            }
        }
        public void fillPersonalisations(string[] names)
        {
            listBoxPersonalisations.Enabled = true;
            listBoxPersonalisations.Items.Clear();
            foreach (var name in names)
            {
                listBoxPersonalisations.Items.Add(name);
            }
        }
        public void selectPersonalisation(int index)
        {
            listBoxPersonalisations.SelectedIndex = index;
        }
        public void fillDriverNames(string[] names)
        {
            listBoxDriverNames.Enabled = true;
            listBoxDriverNames.Items.Clear();
            foreach (var name in names)
            {
                listBoxDriverNames.Items.Add(name);
            }
            // May not be any other names
            listBoxOtherDriverNames.Items.Clear();
            listBoxOtherDriverNames.Enabled = false;
        }
        public void selectDriverName(int index)
        {
            listBoxDriverNames.SelectedIndex = index;
        }
        public void fillOtherDriverNames(string[] names)
        {
            listBoxOtherDriverNames.Enabled = true;
            listBoxOtherDriverNames.Items.Clear();
            foreach (var name in names)
            {
                listBoxOtherDriverNames.Items.Add(name);
            }
        }
        public void doRestart()
        {
            mwi.doRestart(Configuration.getUIString("the_application_must_be_restarted_to_load_the_new_sounds"),
                Configuration.getUIString("load_new_sounds"));
        }

        private void buttonPlayName_Click(object sender, EventArgs e)
        {
            if (listBoxPersonalisations.SelectedIndex != -1)
            {
                model.PlayRandomPersonalisation(listBoxPersonalisations.SelectedItem.ToString());
            }
            if (listBoxDriverNames.SelectedIndex != -1)
            {
                model.PlayRandomDriverName(listBoxDriverNames.SelectedItem.ToString());
            }
            if (listBoxOtherDriverNames.SelectedIndex != -1)
            {
                model.PlayRandomDriverName(listBoxOtherDriverNames.SelectedItem.ToString());
            }
        }

        private void buttonNameSelect_Click(object sender, EventArgs e)
        {
            string name = null;
            if (listBoxPersonalisations.SelectedIndex != -1)
            {
                name = listBoxPersonalisations.SelectedItem.ToString();
                model.SelectPersonalisation(name);
            }
            else if (listBoxDriverNames.SelectedIndex != -1)
            {
                name = listBoxDriverNames.SelectedItem.ToString();
                model.SelectDriverName(name);
            }
            else if (listBoxOtherDriverNames.SelectedIndex != -1)
            {
                name = listBoxOtherDriverNames.SelectedItem.ToString();
                model.SelectDriverName(name);
            }
            mwi.SetButtonMyNameText();
            this.Close();
        }

        private void listBoxPersonalisations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxPersonalisations.SelectedIndex != -1)
            {   // listBoxDriverNames cleared this one
                listBoxDriverNames.SelectedIndex = -1;
                listBoxOtherDriverNames.SelectedIndex = -1;
            }

            buttonPlayName.Enabled = true;
            buttonNameSelect.Enabled = true;
        }

        private void listBoxDriverNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDriverNames.SelectedIndex != -1)
            {   // listBoxPersonalisations cleared this one
                listBoxPersonalisations.SelectedIndex = -1;
                listBoxOtherDriverNames.SelectedIndex = -1;
            }
            buttonPlayName.Enabled = true;
            buttonNameSelect.Enabled = true;
        }

        private void listBoxOtherDriverNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxOtherDriverNames.SelectedIndex != -1)
            {   // listBoxPersonalisations cleared this one
                listBoxPersonalisations.SelectedIndex = -1;
                listBoxDriverNames.SelectedIndex = -1;
            }
            buttonPlayName.Enabled = true;
            buttonNameSelect.Enabled = true;
        }

        private void listBoxPersonalisations_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonPlayName_Click(sender, e);
        }

        private void listBoxDriverNames_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonPlayName_Click(sender, e);
        }

        private void listBoxOtherDriverNames_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonPlayName_Click(sender, e);
        }
    }
}
