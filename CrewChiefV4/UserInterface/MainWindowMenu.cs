using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrewChiefV4
{
    /// <summary>
    /// The idea was to be able to use the VS designer on bits of the main
    /// Window without an enormous diff and messed up layout.  Didn't really
    /// work out but this does allow the menu strip to be edited without
    /// affecting the rest of the main window.
    /// This also provides the console context menu with the same options
    /// as the Console menu item.
    /// It may still be possible to prototype changes using a designer and copy
    /// the code into here.
    /// </summary>
    public partial class MainWindow : Form
    {
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem consoleToolStripMenuItem;
        private ContextMenuStrip consoleContextMenuStrip;
        private ToolStripMenuItem cCopyConsoleToolStripMenuItem;
        private ToolStripMenuItem cClearConsoleToolStripMenuItem;
        private ToolStripMenuItem cSaveConsoleToolStripMenuItem;
        private ToolStripMenuItem cSaveConsoleTextToolStripMenuItem;
        private ToolStripMenuItem cCopySelectedConsoleTextToolStripMenuItem;
        private ToolStripMenuItem cCopyCrewChiefSettingsToolStripMenuItem;
        private ToolStripMenuItem mExitToolStripMenuItem;
        private ToolStripMenuItem mcCopyConsoleToolStripMenuItem;
        private ToolStripMenuItem mcClearConsoleToolStripMenuItem;
        private ToolStripMenuItem mSaveConsoleToolStripMenuItem;
        private ToolStripMenuItem mCopySelectedConsoleTextToolStripMenuItem;
        private ToolStripMenuItem mCopyCrewChiefSettingsToolStripMenuItem;

        public void MenuStrip(Font exemplarFont)
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mcCopyConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mcClearConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mSaveConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mCopySelectedConsoleTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mCopyCrewChiefSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cSaveConsoleTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cSaveConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cCopyConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cClearConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cCopySelectedConsoleTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cSaveConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cCopyCrewChiefSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip1
            //
            this.menuStrip1.BackColor = MainWindow.DefaultBackColor;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.fileToolStripMenuItem,
                this.consoleToolStripMenuItem,
                this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.TabIndex = 503;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Font = exemplarFont;
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            //
            // fileToolStripMenuItem
            //
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mExitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            //
            // mExitToolStripMenuItem
            //
            this.mExitToolStripMenuItem.Name = "mExitToolStripMenuItem";
            this.mExitToolStripMenuItem.ShortcutKeyDisplayString = "Alt+F4";
            this.mExitToolStripMenuItem.ShortcutKeys = (System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4);
            this.mExitToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.mExitToolStripMenuItem.Text = "Exit";
            this.mExitToolStripMenuItem.Click += new System.EventHandler(this.mExitToolStripMenuItem_Click);
            //
            // consoleToolStripMenuItem
            //
            this.consoleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mcCopyConsoleToolStripMenuItem,
                this.mcClearConsoleToolStripMenuItem,
                this.mSaveConsoleToolStripMenuItem});
            this.consoleToolStripMenuItem.Name = "consoleToolStripMenuItem";
            this.consoleToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.consoleToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.consoleToolStripMenuItem.Text = "Console";
            this.consoleToolStripMenuItem.Click += ConsoleToolStripMenuItem_Click;
            //
            // mcCopyConsoleToolStripMenuItem
            //
            this.mcCopyConsoleToolStripMenuItem.Name = "mcCopyConsoleToolStripMenuItem";
            this.mcCopyConsoleToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.mcCopyConsoleToolStripMenuItem.Text = Configuration.getUIString("copy_console_text");
            this.mcCopyConsoleToolStripMenuItem.Click += new System.EventHandler(this.cCopyConsoleToolStripMenuItem_Click);
            //
            // mcClearConsoleToolStripMenuItem
            //
            this.mcClearConsoleToolStripMenuItem.Name = "mcClearConsoleToolStripMenuItem";
            this.mcClearConsoleToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.mcClearConsoleToolStripMenuItem.Text = Configuration.getUIString("clear_console");
            this.mcClearConsoleToolStripMenuItem.Click += new System.EventHandler(this.clearConsole);
            //
            // mSaveConsoleToolStripMenuItem
            //
            this.mSaveConsoleToolStripMenuItem.Name = "mSaveConsoleToolStripMenuItem";
            this.mSaveConsoleToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.mSaveConsoleToolStripMenuItem.Text = Configuration.getUIString("save_console_output");
            this.mSaveConsoleToolStripMenuItem.Click += new System.EventHandler(this.saveConsoleOutputText);
            //
            // mCopySelectedConsoleTextToolStripMenuItem
            //
            this.mCopySelectedConsoleTextToolStripMenuItem.Name = "mCopySelectedConsoleTextToolStripMenuItem";
            this.mCopySelectedConsoleTextToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.mCopySelectedConsoleTextToolStripMenuItem.Text = Configuration.getUIString("copy_selected_text");
            this.mCopySelectedConsoleTextToolStripMenuItem.Click += new System.EventHandler(this.saveSelectedConsoleText_Click);
            //
            // mCopyCrewChiefSettingsToolStripMenuItem
            //
            this.mCopyCrewChiefSettingsToolStripMenuItem.Name = "mCopyCrewChiefSettingsToolStripMenuItem";
            this.mCopyCrewChiefSettingsToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.mCopyCrewChiefSettingsToolStripMenuItem.Text = Configuration.getUIString("copy_crew_chief_settings");
            this.mCopySelectedConsoleTextToolStripMenuItem.Click += new System.EventHandler(this.saveSelectedConsoleText_Click);
            this.mCopyCrewChiefSettingsToolStripMenuItem.Click += new System.EventHandler(this.saveCrewChiefSettings_Click);
            this.mCopyCrewChiefSettingsToolStripMenuItem.ToolTipText = Configuration.getUIString("copy_crew_chief_settings_help");

            //
            // helpToolStripMenuItem
            //
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.ShortcutKeyDisplayString = "F1";
            this.helpToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);

            //
            // consoleContextMenuStrip
            //
            // Items added at runtime
            this.consoleContextMenuStrip.Name = "consoleContextMenuStrip";
            this.consoleContextMenuStrip.Size = new System.Drawing.Size(271, 144);
            this.consoleContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(consoleContextMenuStrip_Opening);

            //
            // cCopyConsoleToolStripMenuItem
            //
            this.cCopyConsoleToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cCopyConsoleToolStripMenuItem.Name = "cCopyConsoleToolStripMenuItem";
            this.cCopyConsoleToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.cCopyConsoleToolStripMenuItem.Text = Configuration.getUIString("copy_console_text");
            this.cCopyConsoleToolStripMenuItem.Click += new System.EventHandler(this.cCopyConsoleToolStripMenuItem_Click);
            //
            // cClearConsoleToolStripMenuItem
            //
            this.cClearConsoleToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cClearConsoleToolStripMenuItem.Name = "cClearConsoleToolStripMenuItem";
            this.cClearConsoleToolStripMenuItem.ShortcutKeys = (Keys.Alt | System.Windows.Forms.Keys.O);
            this.cClearConsoleToolStripMenuItem.ShowShortcutKeys = false;
            this.cClearConsoleToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.cClearConsoleToolStripMenuItem.Text = Configuration.getUIString("clear_console");
            this.cClearConsoleToolStripMenuItem.Click += new System.EventHandler(this.clearConsole);
            //
            // cSaveConsoleToolStripMenuItem
            //
            this.cSaveConsoleToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cSaveConsoleToolStripMenuItem.Name = "cSaveConsoleToolStripMenuItem";
            this.cSaveConsoleToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.cSaveConsoleToolStripMenuItem.Text = Configuration.getUIString("save_console_output");
            this.cSaveConsoleToolStripMenuItem.Click += new System.EventHandler(this.saveConsoleOutputText);
            //
            // cCopySelectedConsoleTextToolStripMenuItem
            //
            this.cCopySelectedConsoleTextToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cCopySelectedConsoleTextToolStripMenuItem.Name = "cCopySelectedConsoleTextToolStripMenuItem";
            this.cCopySelectedConsoleTextToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.cCopySelectedConsoleTextToolStripMenuItem.Text = Configuration.getUIString("copy_selected_text");
            this.cCopySelectedConsoleTextToolStripMenuItem.Click += new System.EventHandler(this.saveSelectedConsoleText_Click);
            //
            // cCopyCrewChiefSettingsToolStripMenuItem
            //
            this.cCopyCrewChiefSettingsToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cCopyCrewChiefSettingsToolStripMenuItem.Name = "cCopyCrewChiefSettingsToolStripMenuItem";
            this.cCopyCrewChiefSettingsToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.cCopyCrewChiefSettingsToolStripMenuItem.Text = Configuration.getUIString("copy_crew_chief_settings");
            this.cCopyCrewChiefSettingsToolStripMenuItem.Click += new System.EventHandler(this.saveCrewChiefSettings_Click);
            this.cCopyCrewChiefSettingsToolStripMenuItem.ToolTipText = Configuration.getUIString("copy_crew_chief_settings_help");

            this.MainMenuStrip = this.menuStrip1;
            this.Controls.Add(this.menuStrip1);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();

            this.consoleToolStripMenuItem.Name = "ConsoleToolStripMenuItem";
            this.consoleToolStripMenuItem.Size = new System.Drawing.Size(62, 22);
            this.consoleToolStripMenuItem.Text = Configuration.getUIString("console_menu");

            this.fileToolStripMenuItem.Text = Configuration.getUIString("file_menu");
            this.mExitToolStripMenuItem.Text = Configuration.getUIString("exit_menu_item");
            this.helpToolStripMenuItem.Text = Configuration.getUIString("help_menu");
            this.cClearConsoleToolStripMenuItem.Text = Configuration.getUIString("clear_console");
            this.cCopyConsoleToolStripMenuItem.Text = Configuration.getUIString("copy_console_text");
            this.cSaveConsoleTextToolStripMenuItem.Text = Configuration.getUIString("save_console_output");
            this.cCopySelectedConsoleTextToolStripMenuItem.Text = Configuration.getUIString("copy_selected_text");
            this.cCopyCrewChiefSettingsToolStripMenuItem.Text = Configuration.getUIString("copy_crew_chief_settings");
            this.consoleTextBox.ContextMenuStrip = this.consoleContextMenuStrip;
        }

        private void mExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new CrewChiefV4.HelpWindow(this);
            form.ShowDialog(this);
        }

        private void cCopyConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(consoleTextBox.Text))
            {
                System.Windows.Forms.Clipboard.SetText(prefixLogfile() + consoleTextBox.Text);
            }
        }

        private void saveConsoleOutputText(object sender, EventArgs e)
        {
            saveConsoleOutputText();
        }

        private void saveSelectedConsoleText_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(consoleTextBox.SelectedText))
            {
                System.Windows.Forms.Clipboard.SetText(prefixLogfile() + consoleTextBox.SelectedText);
            }
        }

        private void saveCrewChiefSettings_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(prefixLogfile());
        }

        /// <summary>
        /// Console context menu entries offered according to Console contents
        /// </summary>
        private void consoleContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            consoleContextMenuStrip.Items.Clear();
            this.cCopySelectedConsoleTextToolStripMenuItem.Enabled = false;
            if (!string.IsNullOrWhiteSpace(consoleTextBox.SelectedText))
            {
                this.cCopySelectedConsoleTextToolStripMenuItem.Enabled = true;
            }
            if (!string.IsNullOrWhiteSpace(consoleTextBox.Text))
            {
                consoleContextMenuStrip.Items.Add(this.cCopyConsoleToolStripMenuItem);
                consoleContextMenuStrip.Items.Add(this.cCopySelectedConsoleTextToolStripMenuItem);
                consoleContextMenuStrip.Items.Add(this.cClearConsoleToolStripMenuItem);
                consoleContextMenuStrip.Items.Add(this.cSaveConsoleToolStripMenuItem);
            }
            consoleContextMenuStrip.Items.Add(this.cCopyCrewChiefSettingsToolStripMenuItem);
            // Always show the context menu
            e.Cancel = false;
        }
        private void ConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mcCopyConsoleToolStripMenuItem.Enabled = false;
            mCopySelectedConsoleTextToolStripMenuItem.Enabled = false;
            mcClearConsoleToolStripMenuItem.Enabled = false;
            mSaveConsoleToolStripMenuItem.Enabled = false;
            this.menuStrip1.SuspendLayout();
            consoleToolStripMenuItem.DropDownItems.Clear();
            if (!string.IsNullOrWhiteSpace(consoleTextBox.SelectedText))
            {
                mCopySelectedConsoleTextToolStripMenuItem.Enabled = true;
            }
            if (!string.IsNullOrWhiteSpace(consoleTextBox.Text))
            {
                mcCopyConsoleToolStripMenuItem.Enabled = true;
                mcClearConsoleToolStripMenuItem.Enabled = true;
                mSaveConsoleToolStripMenuItem.Enabled = true;
            }
            consoleToolStripMenuItem.DropDownItems.Add(this.mcCopyConsoleToolStripMenuItem);
            consoleToolStripMenuItem.DropDownItems.Add(this.mCopySelectedConsoleTextToolStripMenuItem);
            consoleToolStripMenuItem.DropDownItems.Add(this.mcClearConsoleToolStripMenuItem);
            consoleToolStripMenuItem.DropDownItems.Add(this.mSaveConsoleToolStripMenuItem);
            consoleToolStripMenuItem.DropDownItems.Add(this.mCopyCrewChiefSettingsToolStripMenuItem);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
        }
    }
}
