using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrewChiefV4;

namespace CrewChiefV4
{
    public partial class ActionEditor : Form
    {
        private List<ControllerConfiguration.ButtonAssignment> currentAvailableButtonAssignments = new List<ControllerConfiguration.ButtonAssignment>();
        private List<ControllerConfiguration.ButtonAssignment> additionalButtonAssignments = new List<ControllerConfiguration.ButtonAssignment>();
        ControllerConfiguration.ControllerConfigurationData controllerConfigurationData = new ControllerConfiguration.ControllerConfigurationData();
        private Boolean hasChanges = false;
        public ActionEditor()
        {
            InitializeComponent();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.SuspendLayout();
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));            
            labelCurrentActions.Text = Configuration.getUIString("current_available_actions");
            labelAdditionalActions.Text = Configuration.getUIString("additional_available_actions");
            buttonSave.Text = Configuration.getUIString("save_and_restart");
            exitButton.Text = Configuration.getUIString("exit_without_saving");
            buttonReset.Text = Configuration.getUIString("reload_actions");
            reloadData();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private void reloadData()
        {
            controllerConfigurationData = ControllerConfiguration.getControllerConfigurationDataFromFile(ControllerConfiguration.getUserControllerConfigurationDataFileLocation());
            // Need to initialize si we can show correct Ui text to the user
            foreach (ControllerConfiguration.ButtonAssignment ba in controllerConfigurationData.buttonAssignments)
            {
                ba.Initialize();
            }
            refreshLists();
        }

        private void refreshLists()
        {
            listBoxCurrentlyAvailableActions.Items.Clear();
            listBoxAdditionalAvailableActions.Items.Clear();
            currentAvailableButtonAssignments = controllerConfigurationData.buttonAssignments.Where(ba => ba.availableAction).ToList();
            additionalButtonAssignments = controllerConfigurationData.buttonAssignments.Where(ba => !ba.availableAction).ToList();
            foreach (ControllerConfiguration.ButtonAssignment ba in currentAvailableButtonAssignments)
            {
                listBoxCurrentlyAvailableActions.Items.Add(ba.resolvedUiText);
            }
            foreach (ControllerConfiguration.ButtonAssignment ba in additionalButtonAssignments)
            {
                listBoxAdditionalAvailableActions.Items.Add(ba.resolvedUiText);
            }
        }
        private void buttonRemoveAction_Click(object sender, EventArgs e)
        {
            bool hasChangedThisClick = false;
            foreach (var item in listBoxCurrentlyAvailableActions.SelectedItems)
            {
                ControllerConfiguration.ButtonAssignment ba = controllerConfigurationData.buttonAssignments.FirstOrDefault(ba1 => ba1.resolvedUiText == item.ToString());
                if(ba != null)
                {
                    ba.availableAction = false;
                    hasChanges = true;
                    hasChangedThisClick = true;
                }
            }
            if (hasChangedThisClick)
            {
                refreshLists();
            }  
        }

        private void buttonAddAction_Click(object sender, EventArgs e)
        {
            bool hasChangedThisClick = false;
            foreach (var item in listBoxAdditionalAvailableActions.SelectedItems)
            {
                ControllerConfiguration.ButtonAssignment ba = controllerConfigurationData.buttonAssignments.FirstOrDefault(ba1 => ba1.resolvedUiText == item.ToString());
                if (ba != null)
                {
                    ba.availableAction = false;
                    hasChanges = true;
                    hasChangedThisClick = true;
                }
            }
            if (hasChangedThisClick)
            {
                refreshLists();
            }  
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            ControllerConfiguration.saveControllerConfigurationDataFile(controllerConfigurationData);
            hasChanges = false;
            if (!CrewChief.Debugging)
            {
                // have to add "multi" to the start args so the app can restart
                List<String> startArgs = new List<string>();
                startArgs.AddRange(Environment.GetCommandLineArgs());
                if (!startArgs.Contains("multi"))
                {
                    startArgs.Add("multi");
                }
                
                System.Diagnostics.Process.Start(Application.ExecutablePath, String.Join(" ", startArgs.ToArray())); // to start new instance of application
                MainWindow.instance.Close(); //to turn off current app
            }
        }

        private void ActionEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(hasChanges)
            {
                String warningMessage = Configuration.getUIString("save_prop_changes_warning");
                if (CrewChief.Debugging)
                {
                    warningMessage = "You have unsaved changes. Click 'Yes' to save these changes (you will need to manually restart the application). Click 'No' to discard these changes";
                }
                if (MessageBox.Show(warningMessage, Configuration.getUIString("save_changes"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ControllerConfiguration.saveControllerConfigurationDataFile(controllerConfigurationData);
                    if (!CrewChief.Debugging)
                    {
                        // have to add "multi" to the start args so the app can restart
                        List<String> startArgs = new List<string>();
                        startArgs.AddRange(Environment.GetCommandLineArgs());
                        if (!startArgs.Contains("multi"))
                        {
                            startArgs.Add("multi");
                        }
                        if (!startArgs.Contains("app_restart"))
                        {
                            startArgs.Add("app_restart");
                        }
                        System.Diagnostics.Process.Start(Application.ExecutablePath, String.Join(" ", startArgs.ToArray())); // to start new instance of application
                        MainWindow.instance.Close(); //to turn off current app
                    }
                }
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            reloadData();
            hasChanges = false;
        }
    }
}
