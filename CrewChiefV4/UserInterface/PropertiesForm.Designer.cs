using System;
using System.Windows.Forms;

namespace CrewChiefV4
{
    partial class PropertiesForm
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
            this.components = new System.ComponentModel.Container();
            this.saveButton = new System.Windows.Forms.Button();
            this.propertiesFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.headerTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.gameFilterLabel = new System.Windows.Forms.Label();
            this.filterBox = new System.Windows.Forms.ComboBox();
            this.showCommonCheckbox = new System.Windows.Forms.CheckBox();
            this.categoriesLabel = new System.Windows.Forms.Label();
            this.categoriesBox = new System.Windows.Forms.ComboBox();
            this.buttonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.exitButton = new System.Windows.Forms.Button();
            this.restoreButton = new System.Windows.Forms.Button();
            this.searchBoxTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.mainTableLayoutPanel.SuspendLayout();
            this.headerTableLayoutPanel.SuspendLayout();
            this.buttonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.saveButton.Location = new System.Drawing.Point(3, 3);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(159, 40);
            this.saveButton.TabIndex = 1;
            this.saveButton.Text = "save_and_restart";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // propertiesFlowLayoutPanel
            // 
            this.propertiesFlowLayoutPanel.AutoScroll = true;
            this.propertiesFlowLayoutPanel.AutoSize = true;
            this.propertiesFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.propertiesFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesFlowLayoutPanel.Location = new System.Drawing.Point(3, 31);
            this.propertiesFlowLayoutPanel.Name = "propertiesFlowLayoutPanel";
            this.propertiesFlowLayoutPanel.Size = new System.Drawing.Size(969, 612);
            this.propertiesFlowLayoutPanel.TabIndex = 0;
            this.propertiesFlowLayoutPanel.TabStop = true;
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(781, 3);
            this.searchTextBox.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(189, 20);
            this.searchTextBox.TabIndex = 15;
            this.searchTextBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Controls.Add(this.headerTableLayoutPanel, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.propertiesFlowLayoutPanel, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.buttonsTableLayoutPanel, 0, 2);
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 3;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 88F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(975, 703);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // headerTableLayoutPanel
            // 
            this.headerTableLayoutPanel.ColumnCount = 7;
            this.headerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.headerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.headerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.headerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.headerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.headerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.headerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.headerTableLayoutPanel.Controls.Add(this.gameFilterLabel, 0, 0);
            this.headerTableLayoutPanel.Controls.Add(this.filterBox, 1, 0);
            this.headerTableLayoutPanel.Controls.Add(this.showCommonCheckbox, 2, 0);
            this.headerTableLayoutPanel.Controls.Add(this.categoriesLabel, 3, 0);
            this.headerTableLayoutPanel.Controls.Add(this.categoriesBox, 4, 0);
            this.headerTableLayoutPanel.Controls.Add(this.searchTextBox, 6, 0);
            this.headerTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.headerTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.headerTableLayoutPanel.Name = "headerTableLayoutPanel";
            this.headerTableLayoutPanel.RowCount = 1;
            this.headerTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.headerTableLayoutPanel.Size = new System.Drawing.Size(975, 28);
            this.headerTableLayoutPanel.TabIndex = 0;
            // 
            // gameFilterLabel
            // 
            this.gameFilterLabel.Location = new System.Drawing.Point(3, 0);
            this.gameFilterLabel.Name = "gameFilterLabel";
            this.gameFilterLabel.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.gameFilterLabel.Size = new System.Drawing.Size(60, 23);
            this.gameFilterLabel.TabIndex = 9;
            this.gameFilterLabel.Text = "game_filter_label";
            this.gameFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // filterBox
            // 
            this.filterBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filterBox.Location = new System.Drawing.Point(66, 3);
            this.filterBox.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.filterBox.MaximumSize = new System.Drawing.Size(203, 0);
            this.filterBox.MinimumSize = new System.Drawing.Size(203, 0);
            this.filterBox.Name = "filterBox";
            this.filterBox.Size = new System.Drawing.Size(203, 21);
            this.filterBox.TabIndex = 10;
            this.filterBox.SelectedValueChanged += new System.EventHandler(this.FilterBox_SelectedValueChanged);
            // 
            // showCommonCheckbox
            // 
            this.showCommonCheckbox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.showCommonCheckbox.Checked = true;
            this.showCommonCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showCommonCheckbox.Location = new System.Drawing.Point(286, 5);
            this.showCommonCheckbox.Margin = new System.Windows.Forms.Padding(10, 5, 3, 3);
            this.showCommonCheckbox.Name = "showCommonCheckbox";
            this.showCommonCheckbox.Size = new System.Drawing.Size(101, 20);
            this.showCommonCheckbox.TabIndex = 11;
            this.showCommonCheckbox.Text = "show_common_props_label";
            // 
            // categoriesLabel
            // 
            this.categoriesLabel.Location = new System.Drawing.Point(393, 0);
            this.categoriesLabel.Name = "categoriesLabel";
            this.categoriesLabel.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.categoriesLabel.Size = new System.Drawing.Size(89, 23);
            this.categoriesLabel.TabIndex = 12;
            this.categoriesLabel.Text = "category_filter_label";
            this.categoriesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // categoriesBox
            // 
            this.categoriesBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.categoriesBox.Location = new System.Drawing.Point(485, 3);
            this.categoriesBox.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.categoriesBox.MaximumSize = new System.Drawing.Size(203, 0);
            this.categoriesBox.MinimumSize = new System.Drawing.Size(203, 0);
            this.categoriesBox.Name = "categoriesBox";
            this.categoriesBox.Size = new System.Drawing.Size(203, 21);
            this.categoriesBox.TabIndex = 13;
            // 
            // buttonsTableLayoutPanel
            // 
            this.buttonsTableLayoutPanel.ColumnCount = 4;
            this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49F));
            this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.buttonsTableLayoutPanel.Controls.Add(this.saveButton, 0, 0);
            this.buttonsTableLayoutPanel.Controls.Add(this.exitButton, 1, 0);
            this.buttonsTableLayoutPanel.Controls.Add(this.restoreButton, 3, 0);
            this.buttonsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonsTableLayoutPanel.Location = new System.Drawing.Point(0, 646);
            this.buttonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonsTableLayoutPanel.Name = "buttonsTableLayoutPanel";
            this.buttonsTableLayoutPanel.RowCount = 1;
            this.buttonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.buttonsTableLayoutPanel.Size = new System.Drawing.Size(975, 57);
            this.buttonsTableLayoutPanel.TabIndex = 0;
            // 
            // exitButton
            // 
            this.exitButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.exitButton.Location = new System.Drawing.Point(168, 3);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(159, 40);
            this.exitButton.TabIndex = 0;
            this.exitButton.Text = "exit_without_saving";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // restoreButton
            // 
            this.restoreButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.restoreButton.Location = new System.Drawing.Point(810, 3);
            this.restoreButton.Name = "restoreButton";
            this.restoreButton.Size = new System.Drawing.Size(162, 40);
            this.restoreButton.TabIndex = 2;
            this.restoreButton.Text = "restore_default_settings";
            this.restoreButton.UseVisualStyleBackColor = true;
            this.restoreButton.Click += new System.EventHandler(this.restoreButton_Click);
            // 
            // searchBoxTooltip
            // 
            this.searchBoxTooltip.AutoPopDelay = 5000;
            this.searchBoxTooltip.InitialDelay = 250;
            this.searchBoxTooltip.IsBalloon = true;
            this.searchBoxTooltip.ReshowDelay = 100;
            // 
            // PropertiesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(975, 703);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PropertiesForm";
            this.Text = "properties_form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.properties_FormClosing);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            this.headerTableLayoutPanel.ResumeLayout(false);
            this.headerTableLayoutPanel.PerformLayout();
            this.buttonsTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.FlowLayoutPanel propertiesFlowLayoutPanel;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel headerTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel buttonsTableLayoutPanel;
        private System.Windows.Forms.ToolTip searchBoxTooltip;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button restoreButton;
        private System.Windows.Forms.Label gameFilterLabel;
        private System.Windows.Forms.ComboBox filterBox;
        private System.Windows.Forms.CheckBox showCommonCheckbox;
        private System.Windows.Forms.Label categoriesLabel;
        private System.Windows.Forms.ComboBox categoriesBox;
    }
}