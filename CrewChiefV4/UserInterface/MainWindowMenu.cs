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
        private ToolStripMenuItem mExitToolStripMenuItem;
        private ToolStripMenuItem mcCopyConsoleToolStripMenuItem;
        private ToolStripMenuItem mcClearConsoleToolStripMenuItem;
        private ToolStripMenuItem mSaveConsoleToolStripMenuItem;

        public void MenuStrip(Font exemplarFont)
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mcCopyConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mcClearConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mSaveConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cSaveConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cCopyConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cClearConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.mSaveConsoleToolStripMenuItem.Click += new System.EventHandler(this.cSaveConsoleOutputText);

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
            this.consoleContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.cCopyConsoleToolStripMenuItem,
                this.cClearConsoleToolStripMenuItem,
                this.cSaveConsoleToolStripMenuItem});
            this.consoleContextMenuStrip.Name = "consoleContextMenuStrip";
            this.consoleContextMenuStrip.Size = new System.Drawing.Size(271, 144);
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
            this.cSaveConsoleToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.cSaveConsoleToolStripMenuItem.Text = Configuration.getUIString("copy_console_text");
            this.cSaveConsoleToolStripMenuItem.Click += new System.EventHandler(this.cSaveConsoleOutputText);

            this.MainMenuStrip = this.menuStrip1;
            this.Controls.Add(this.menuStrip1);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();

            consoleToolStripMenuItem.Name = "ConsoleToolStripMenuItem";
            consoleToolStripMenuItem.Size = new System.Drawing.Size(62, 22);
            consoleToolStripMenuItem.Text = "Console";

            this.fileToolStripMenuItem.Text = Configuration.getUIString("file_menu");
            this.mExitToolStripMenuItem.Text = Configuration.getUIString("exit_menu_item");
            this.helpToolStripMenuItem.Text = Configuration.getUIString("help_menu");
            this.cClearConsoleToolStripMenuItem.Text = Configuration.getUIString("clear_console");
            this.cCopyConsoleToolStripMenuItem.Text = Configuration.getUIString("copy_console_text");
            this.cSaveConsoleToolStripMenuItem.Text = Configuration.getUIString("save_console_output");

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
            if (consoleTextBox.Text != "")
            {
                System.Windows.Forms.Clipboard.SetText(consoleTextBox.Text);
            }
            else // can't copy "" to clipboard
            {
                System.Windows.Forms.Clipboard.SetText(" ");
            }
        }

        private void cSaveConsoleOutputText(object sender, EventArgs e)
        {
            saveConsoleOutputText();
        }
    }
}
