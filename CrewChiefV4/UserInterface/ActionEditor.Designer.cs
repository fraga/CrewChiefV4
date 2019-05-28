namespace CrewChiefV4
{
    partial class ActionEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxCurrentlyAvailableActions = new System.Windows.Forms.ListBox();
            this.listBoxAdditionalAvailableActions = new System.Windows.Forms.ListBox();
            this.buttonRemoveAction = new System.Windows.Forms.Button();
            this.buttonAddAction = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.current_available_actions = new System.Windows.Forms.Label();
            this.additional_available_actions = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBoxCurrentlyAvailableActions
            // 
            this.listBoxCurrentlyAvailableActions.FormattingEnabled = true;
            this.listBoxCurrentlyAvailableActions.Location = new System.Drawing.Point(337, 20);
            this.listBoxCurrentlyAvailableActions.Name = "listBoxCurrentlyAvailableActions";
            this.listBoxCurrentlyAvailableActions.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxCurrentlyAvailableActions.Size = new System.Drawing.Size(272, 134);
            this.listBoxCurrentlyAvailableActions.TabIndex = 0;
            // 
            // listBoxAdditionalAvailableActions
            // 
            this.listBoxAdditionalAvailableActions.FormattingEnabled = true;
            this.listBoxAdditionalAvailableActions.Location = new System.Drawing.Point(8, 20);
            this.listBoxAdditionalAvailableActions.Name = "listBoxAdditionalAvailableActions";
            this.listBoxAdditionalAvailableActions.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxAdditionalAvailableActions.Size = new System.Drawing.Size(272, 134);
            this.listBoxAdditionalAvailableActions.TabIndex = 1;
            // 
            // buttonRemoveAction
            // 
            this.buttonRemoveAction.Location = new System.Drawing.Point(286, 95);
            this.buttonRemoveAction.Name = "buttonRemoveAction";
            this.buttonRemoveAction.Size = new System.Drawing.Size(45, 25);
            this.buttonRemoveAction.TabIndex = 2;
            this.buttonRemoveAction.Text = "<<";
            this.buttonRemoveAction.UseVisualStyleBackColor = true;
            this.buttonRemoveAction.Click += new System.EventHandler(this.buttonRemoveAction_Click);
            // 
            // buttonAddAction
            // 
            this.buttonAddAction.Location = new System.Drawing.Point(286, 54);
            this.buttonAddAction.Name = "buttonAddAction";
            this.buttonAddAction.Size = new System.Drawing.Size(45, 23);
            this.buttonAddAction.TabIndex = 3;
            this.buttonAddAction.Text = ">>";
            this.buttonAddAction.UseVisualStyleBackColor = true;
            this.buttonAddAction.Click += new System.EventHandler(this.buttonAddAction_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(8, 161);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(133, 35);
            this.buttonSave.TabIndex = 4;
            this.buttonSave.Text = "save_and_restart";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(147, 161);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(133, 35);
            this.exitButton.TabIndex = 5;
            this.exitButton.Text = "exit_without_saving";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // current_available_actions
            // 
            this.current_available_actions.AutoSize = true;
            this.current_available_actions.Location = new System.Drawing.Point(334, 4);
            this.current_available_actions.Name = "current_available_actions";
            this.current_available_actions.Size = new System.Drawing.Size(128, 13);
            this.current_available_actions.TabIndex = 6;
            this.current_available_actions.Text = "current_available_actions";
            // 
            // additional_available_actions
            // 
            this.additional_available_actions.AutoSize = true;
            this.additional_available_actions.Location = new System.Drawing.Point(5, 4);
            this.additional_available_actions.Name = "additional_available_actions";
            this.additional_available_actions.Size = new System.Drawing.Size(140, 13);
            this.additional_available_actions.TabIndex = 7;
            this.additional_available_actions.Text = "additional_available_actions";
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(476, 161);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(133, 35);
            this.buttonReset.TabIndex = 8;
            this.buttonReset.Text = "reload_actions";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // ActionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 208);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.additional_available_actions);
            this.Controls.Add(this.current_available_actions);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonAddAction);
            this.Controls.Add(this.buttonRemoveAction);
            this.Controls.Add(this.listBoxAdditionalAvailableActions);
            this.Controls.Add(this.listBoxCurrentlyAvailableActions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ActionEditor";
            this.Text = "Action Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ActionEditor_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxCurrentlyAvailableActions;
        private System.Windows.Forms.ListBox listBoxAdditionalAvailableActions;
        private System.Windows.Forms.Button buttonRemoveAction;
        private System.Windows.Forms.Button buttonAddAction;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Label current_available_actions;
        private System.Windows.Forms.Label additional_available_actions;
        private System.Windows.Forms.Button buttonReset;
    }
}