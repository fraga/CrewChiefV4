﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrewChiefV4
{
    public partial class PropertiesForm : Form
    {
        public Boolean hasChanges;
        public Boolean isActiveProfile = true;
        public HashSet<string> updatedPropertiesRequiringRestart = new HashSet<string>();

        System.Windows.Forms.Form parent;

        private Timer searchTimer;
        private readonly string DEFAULT_SEARCH_TEXT = Configuration.getUIString("search_box_default_text");
        private readonly TimeSpan AUTO_SEARCH_DELAY_SPAN = TimeSpan.FromMilliseconds(700);
        private DateTime nextPrefsRefreshAttemptTime = DateTime.MinValue;
        private Label noMatchedLabel = new Label() { Text = Configuration.getUIString("no_matches") };
        private List<string> profileNames = new List<string>();

        public static String listPropPostfix = "_listprop";

        private string searchTextPrev = null;
        private GameEnum gameFilterPrev = GameEnum.UNKNOWN;

        internal enum SpecialFilter
        {
            ALL_PREFERENCES = GameEnum.UNKNOWN + 1,
            COMMON_PREFERENCES,
            UNKNOWN
        }
        private SpecialFilter specialFilterPrev = SpecialFilter.UNKNOWN;
        private bool includeCommonPreferencesPrev = true;

        public enum PropertyCategory
        {
            ALL,  // Don't assign this to properties, this means no filtering applied.
            UI_STARTUP_AND_PATHS,
            AUDIO_VOICE_AND_CONTROLLERS,
            SPOTTER,
            FLAGS_AND_RULES,
            MESSAGE_FREQUENCES,
            FUEL_TEMPS_AND_DAMAGES,
            TIMINGS,
            PIT_STOPS_AND_MULTICLASS,
            INTERNATIONALISATION,
            MISC,  // Implied by default.
            UNKNOWN
        }
        private PropertyCategory categoryFilterPrev = PropertyCategory.UNKNOWN;

        public class ComboBoxItem<T>
        {
            public string Label { get; set; }
            public T Value { get; set; }

            public override string ToString()
            {
                return this.Label != null ? this.Label : string.Empty;
            }
        }
        private void InitializeUiTexts()
        {
            ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));

            this.saveButton.Text = Configuration.getUIString("save_changes");
            this.gameFilterLabel.Text = Configuration.getUIString("game_filter_label");
            this.showCommonCheckbox.Text = Configuration.getUIString("show_common_props_label");
            this.categoriesLabel.Text = Configuration.getUIString("category_filter_label");
            var tooltip = Configuration.getUIString("search_box_tooltip_line1") + Environment.NewLine
                + Configuration.getUIString("search_box_tooltip_line2") + Environment.NewLine
                + Configuration.getUIString("search_box_tooltip_line4") + Environment.NewLine
                + Configuration.getUIString("search_box_tooltip_line5") + Environment.NewLine
                + Configuration.getUIString("search_box_tooltip_line6") + Environment.NewLine
                + Configuration.getUIString("search_box_tooltip_line7") + Environment.NewLine
                + Configuration.getUIString("search_box_tooltip_line8") + Environment.NewLine;
            this.searchBoxTooltip.SetToolTip(this.searchTextBox, tooltip);
            this.exitButton.Text = Configuration.getUIString("exit_without_saving");
            this.restoreButton.Text = Configuration.getUIString("restore_default_settings");
            this.Text = Configuration.getUIString("properties_form");
            userProfileGroupBox.Text = Configuration.getUIString("user_profile")
                + " (" + Configuration.getUIString("active_label") + " "
                + Path.GetFileNameWithoutExtension(UserSettings.GetUserSettings().getString("current_settings_profile")) + ")";
            profilesLabel.Text = Configuration.getUIString("user_profile_label");
            loadProfileButton.Text = Configuration.getUIString("load_profile");
            loadProfileButton.Enabled = false;
            createNewProfileButton.Text = Configuration.getUIString("create_new_profile");
            copySettingsFromCurrentSelectionCheckBox.Text = Configuration.getUIString("copy_settings_from_current");
            activateNewProfileCheckBox.Text = Configuration.getUIString("activate_new_profile");

            List<string> settingsProfileFiles = Directory.GetFiles(UserSettings.userProfilesPath, "*.json", SearchOption.TopDirectoryOnly).ToList();
            foreach (var file in settingsProfileFiles)
            {
                profileNames.Add(Path.GetFileNameWithoutExtension(file));
            }
            foreach (var profile in profileNames)
            {
                profileSelectionComboBox.Items.Add(profile);
            }
            updateSaveButtonText();
        }


        // Note: vast majority of startup time is in ShowDialog.  Looks like pretty much the only way to speed it up is by reducing
        // number of controls or splitting in tabs.
        public PropertiesForm(Form parent)
        {
            StartPosition = FormStartPosition.CenterParent;
            if (MainWindow.forceMinWindowSize)
            {
                this.MinimumSize = new Size(1030, 860);
            }

            hasChanges = false;
            this.parent = parent;

            InitializeComponent();
            InitializeUiTexts();
            if (CrewChief.Debugging)
            {
                this.loadProfileButton.Text = "Activate profile (manual restart required)";
                this.activateNewProfileCheckBox.Text = "Activate new profile (Not possible in debug mode)";
                this.activateNewProfileCheckBox.Enabled = false;
            }

            this.SuspendLayout();
            this.propertiesFlowLayoutPanel.SuspendLayout();

            int widgetCount = 0;
            foreach (SettingsProperty strProp in UserSettings.GetUserSettings().getProperties(typeof(String), null, null))
            {
                if (strProp.Name.EndsWith(PropertiesForm.listPropPostfix) && ListPropertyValues.getListBoxLabels(strProp.Name) != null)
                {
                    this.propertiesFlowLayoutPanel.Controls.Add(new ListPropertyControl(strProp.Name, Configuration.getUIString(strProp.Name) + " " + Configuration.getUIString("text_prop_type"),
                       UserSettings.GetUserSettings().getString(strProp.Name), (String)strProp.DefaultValue,
                       Configuration.getUIString(strProp.Name + "_help"), Configuration.getUIStringStrict(strProp.Name + "_filter"),
                       Configuration.getUIStringStrict(strProp.Name + "_category"), changeRequiresRestart(Configuration.getUIStringStrict(strProp.Name + "_metadata")),
                       Configuration.getUIStringStrict(strProp.Name + "_type"), this));
                }
                else
                {
                    this.propertiesFlowLayoutPanel.Controls.Add(new StringPropertyControl(strProp.Name, Configuration.getUIString(strProp.Name) + " " + Configuration.getUIString("text_prop_type"),
                       UserSettings.GetUserSettings().getString(strProp.Name), (String)strProp.DefaultValue,
                       Configuration.getUIString(strProp.Name + "_help"), Configuration.getUIStringStrict(strProp.Name + "_filter"),
                       Configuration.getUIStringStrict(strProp.Name + "_category"), changeRequiresRestart(Configuration.getUIStringStrict(strProp.Name + "_metadata")), this));
                }
                widgetCount++;
            }
            pad(widgetCount);
            widgetCount = 0;
            foreach (SettingsProperty boolProp in UserSettings.GetUserSettings().getProperties(typeof(Boolean), "enable", null))
            {
                Boolean defaultValue;
                Boolean.TryParse((String)boolProp.DefaultValue, out defaultValue);
                this.propertiesFlowLayoutPanel.Controls.Add(new BooleanPropertyControl(boolProp.Name, Configuration.getUIString(boolProp.Name) + " " + Configuration.getUIString("boolean_prop_type"),
                    UserSettings.GetUserSettings().getBoolean(boolProp.Name), defaultValue,
                    Configuration.getUIString(boolProp.Name + "_help"), Configuration.getUIStringStrict(boolProp.Name + "_filter"),
                    Configuration.getUIStringStrict(boolProp.Name + "_category"), changeRequiresRestart(Configuration.getUIStringStrict(boolProp.Name + "_metadata")), this));
                widgetCount++;
            }
            pad(widgetCount);
            widgetCount = 0;
            foreach (SettingsProperty intProp in UserSettings.GetUserSettings().getProperties(typeof(int), "frequency", null))
            {
                int defaultValue;
                int.TryParse((String)intProp.DefaultValue, out defaultValue);
                this.propertiesFlowLayoutPanel.Controls.Add(new IntPropertyControl(intProp.Name, Configuration.getUIString(intProp.Name) + " " + Configuration.getUIString("integer_prop_type"),
                    UserSettings.GetUserSettings().getInt(intProp.Name), defaultValue,
                    Configuration.getUIString(intProp.Name + "_help"), Configuration.getUIStringStrict(intProp.Name + "_filter"),
                    Configuration.getUIStringStrict(intProp.Name + "_category"), changeRequiresRestart(Configuration.getUIStringStrict(intProp.Name + "_metadata")), this));
                widgetCount++;
            }
            pad(widgetCount);
            widgetCount = 0;
            foreach (SettingsProperty boolProp in UserSettings.GetUserSettings().getProperties(typeof(Boolean), null, "enable"))
            {
                Boolean defaultValue;
                Boolean.TryParse((String)boolProp.DefaultValue, out defaultValue);
                this.propertiesFlowLayoutPanel.Controls.Add(new BooleanPropertyControl(boolProp.Name, Configuration.getUIString(boolProp.Name) + " " + Configuration.getUIString("boolean_prop_type"),
                    UserSettings.GetUserSettings().getBoolean(boolProp.Name), defaultValue,
                    Configuration.getUIString(boolProp.Name + "_help"), Configuration.getUIStringStrict(boolProp.Name + "_filter"),
                    Configuration.getUIStringStrict(boolProp.Name + "_category"), changeRequiresRestart(Configuration.getUIStringStrict(boolProp.Name + "_metadata")), this));
                widgetCount++;
            }
            pad(widgetCount);
            widgetCount = 0;
            foreach (SettingsProperty intProp in UserSettings.GetUserSettings().getProperties(typeof(int), null, "frequency"))
            {
                int defaultValue;
                int.TryParse((String)intProp.DefaultValue, out defaultValue);
                this.propertiesFlowLayoutPanel.Controls.Add(new IntPropertyControl(intProp.Name, Configuration.getUIString(intProp.Name) + " " + Configuration.getUIString("integer_prop_type"),
                    UserSettings.GetUserSettings().getInt(intProp.Name), defaultValue,
                    Configuration.getUIString(intProp.Name + "_help"), Configuration.getUIStringStrict(intProp.Name + "_filter"),
                    Configuration.getUIStringStrict(intProp.Name + "_category"), changeRequiresRestart(Configuration.getUIStringStrict(intProp.Name + "_metadata")), this));
                widgetCount++;
            }
            pad(widgetCount);
            widgetCount = 0;
            foreach (SettingsProperty floatProp in UserSettings.GetUserSettings().getProperties(typeof(float), null, null))
            {
                float defaultValue;
                float.TryParse((String)floatProp.DefaultValue, out defaultValue);
                this.propertiesFlowLayoutPanel.Controls.Add(new FloatPropertyControl(floatProp.Name, Configuration.getUIString(floatProp.Name) + " " + Configuration.getUIString("real_number_prop_type"),
                    UserSettings.GetUserSettings().getFloat(floatProp.Name), defaultValue,
                    Configuration.getUIString(floatProp.Name + "_help"), Configuration.getUIStringStrict(floatProp.Name + "_filter"),
                    Configuration.getUIStringStrict(floatProp.Name + "_category"), changeRequiresRestart(Configuration.getUIStringStrict(floatProp.Name + "_metadata")), this));
                widgetCount++;
            }
            pad(widgetCount);
            widgetCount = 0;

            this.searchTextPrev = DEFAULT_SEARCH_TEXT;
            this.gameFilterPrev = GameEnum.UNKNOWN;
            this.specialFilterPrev = SpecialFilter.UNKNOWN;
            this.categoryFilterPrev = PropertyCategory.ALL;  // Initialize this here, so that initial game filtering works.
            this.includeCommonPreferencesPrev = true;

            this.searchTextBox.Text = DEFAULT_SEARCH_TEXT;
            this.searchTextBox.ForeColor = Color.Gray;
            //this.searchTextBox.GotFocus += SearchTextBox_GotFocus;
            //this.searchTextBox.LostFocus += SearchTextBox_LostFocus;
            //this.searchTextBox.KeyDown += SearchTextBox_KeyDown;
            this.exitButton.Select();

            this.KeyPreview = true;
            this.KeyDown += PropertiesForm_KeyDown;

            this.DoubleBuffered = true;

            // Filtering setup.
            this.filterBox.Items.Clear();
            this.filterBox.Items.Add(new ComboBoxItem<SpecialFilter>()
            {
                Label = Configuration.getUIString("all_preferences_label"),
                Value = SpecialFilter.ALL_PREFERENCES
            });

            this.filterBox.Items.Add(new ComboBoxItem<SpecialFilter>()
            {
                Label = Configuration.getUIString("common_preferences_label"),
                Value = SpecialFilter.COMMON_PREFERENCES
            });

            lock (MainWindow.instanceLock)
            {
                if (MainWindow.instance != null)
                {
                    var currSelectedGameFriendlyName = MainWindow.instance.gameDefinitionList.Text;
                    foreach (var game in MainWindow.instance.gameDefinitionList.Items)
                    {
                        var friendlyGameName = game.ToString();
                        this.filterBox.Items.Add(new ComboBoxItem<GameEnum>()
                        {
                            Label = friendlyGameName,
                            Value = GameDefinition.getGameDefinitionForFriendlyName(friendlyGameName).gameEnum
                        });

                        if (friendlyGameName == currSelectedGameFriendlyName)
                            this.filterBox.SelectedIndex = this.filterBox.Items.Count - 1;
                    }
                }
            }

            // Special case for no game selected.
            if (this.filterBox.SelectedIndex == -1)
            {
                this.filterBox.SelectedIndex = 0;
                // No need to filter.
                this.specialFilterPrev = SpecialFilter.ALL_PREFERENCES;
            }

            // Category filter:
            this.categoriesBox.Items.Clear();
            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("all_categories_label"),
                Value = PropertyCategory.ALL
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("ui_startup_and_paths_category_label"),
                Value = PropertyCategory.UI_STARTUP_AND_PATHS
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("audio_voice_and_controllers_category_label"),
                Value = PropertyCategory.AUDIO_VOICE_AND_CONTROLLERS
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("spotter_category_label"),
                Value = PropertyCategory.SPOTTER
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("flags_and_rules_category_label"),
                Value = PropertyCategory.FLAGS_AND_RULES
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("message_frequences_category_label"),
                Value = PropertyCategory.MESSAGE_FREQUENCES
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("fuel_temps_and_damages_category_label"),
                Value = PropertyCategory.FUEL_TEMPS_AND_DAMAGES
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("timings_category_label"),
                Value = PropertyCategory.TIMINGS
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("pit_stops_and_multiclass_category_label"),
                Value = PropertyCategory.PIT_STOPS_AND_MULTICLASS
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("internationalisation_label"),
                Value = PropertyCategory.INTERNATIONALISATION
            });

            this.categoriesBox.Items.Add(new ComboBoxItem<PropertyCategory>()
            {
                Label = Configuration.getUIString("misc_category_label"),
                Value = PropertyCategory.MISC
            });

            this.categoriesBox.SelectedIndex = 0;

            profileSelectionComboBox.SelectedIndex = profileNames.IndexOf(Path.GetFileNameWithoutExtension(UserSettings.GetUserSettings().getString("current_settings_profile")));

            this.propertiesFlowLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        public bool save()
        {
            Boolean isActiveProfile = profileSelectionComboBox.SelectedItem.ToString().Equals(Path.GetFileNameWithoutExtension(UserSettings.GetUserSettings().getString("current_settings_profile")));
            UserSettings.UserProfileSettings currentSelection = UserSettings.GetUserSettings().loadUserSettings(Path.Combine(UserSettings.userProfilesPath, profileSelectionComboBox.SelectedItem.ToString() + ".json"));
            foreach (var control in this.propertiesFlowLayoutPanel.Controls)
            {
                if (control.GetType() == typeof(StringPropertyControl))
                {
                    StringPropertyControl stringControl = (StringPropertyControl)control;
                    UserSettings.GetUserSettings().setProperty(stringControl.propertyId,
                    stringControl.getValue());
                    currentSelection.userSettings[stringControl.propertyId] = stringControl.getValue();
                }
                else if (control.GetType() == typeof(ListPropertyControl))
                {
                    ListPropertyControl listControl = (ListPropertyControl)control;
                    UserSettings.GetUserSettings().setProperty(listControl.propertyId,
                    listControl.getValue());
                    currentSelection.userSettings[listControl.propertyId] = listControl.getValue();
                }
                else if (control.GetType() == typeof(IntPropertyControl))
                {
                    IntPropertyControl intControl = (IntPropertyControl)control;
                    UserSettings.GetUserSettings().setProperty(intControl.propertyId,
                    intControl.getValue());
                    currentSelection.userSettings[intControl.propertyId] = intControl.getValue();
                }
                if (control.GetType() == typeof(FloatPropertyControl))
                {
                    FloatPropertyControl floatControl = (FloatPropertyControl)control;
                    UserSettings.GetUserSettings().setProperty(floatControl.propertyId,
                    floatControl.getValue());
                    currentSelection.userSettings[floatControl.propertyId] = floatControl.getValue();
                }
                if (control.GetType() == typeof(BooleanPropertyControl))
                {
                    BooleanPropertyControl boolControl = (BooleanPropertyControl)control;
                    UserSettings.GetUserSettings().setProperty(boolControl.propertyId,
                    boolControl.getValue());
                    currentSelection.userSettings[boolControl.propertyId] = boolControl.getValue();
                }
                if (isActiveProfile)
                {
                    UserSettings.GetUserSettings().saveUserSettings();
                }
                else
                {
                    UserSettings.saveUserSettingsFile(currentSelection, profileSelectionComboBox.SelectedItem.ToString() + ".json");
                }
                hasChanges = false;
            }
            return isActiveProfile;
        }

        public void updateSaveButtonText()
        {
            Boolean restart = updatedPropertiesRequiringRestart.Count() > 0;
            if (CrewChief.Debugging)
            {
                saveButton.Text = restart && isActiveProfile ? "Save (manual restart required)" : Configuration.getUIString("save_changes");
                loadProfileButton.Text = restart ? "Activate profile (manual restart required)" : Configuration.getUIString("load_profile");
            }
            else
            {
                saveButton.Text = restart && isActiveProfile ? Configuration.getUIString("save_and_restart") : Configuration.getUIString("save_changes");
                loadProfileButton.Text = restart ? Configuration.getUIString("load_profile_and_restart") : Configuration.getUIString("load_profile");
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Boolean activeProfile = save();
            if (!CrewChief.Debugging && activeProfile && updatedPropertiesRequiringRestart.Count > 0)
            {
                // have to add "multi" to the start args so the app can restart
                List<String> startArgs = new List<string>();
                startArgs.AddRange(Environment.GetCommandLineArgs());
                if (!startArgs.Contains("multi"))
                {
                    startArgs.Add("multi");
                }
                System.Diagnostics.Process.Start(Application.ExecutablePath, String.Join(" ", startArgs.ToArray())); // to start new instance of application
                parent.Close(); //to turn off current app
            }
        }

        private void pad(int widgetCount)
        {
            int paddedWidgetCount = widgetCount;
            while (paddedWidgetCount % 3 > 0)
            {
                paddedWidgetCount++;
            }
            for (int i = 0; i < paddedWidgetCount - widgetCount; i++)
            {
                this.propertiesFlowLayoutPanel.Controls.Add(new Spacer());
            }
        }

        private void properties_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.searchTimer != null)
            {
                this.searchTimer.Stop();
                this.searchTimer = null;
            }

            if (this.hasChanges && this.updatedPropertiesRequiringRestart.Count > 0)
            {
                String warningMessage = Configuration.getUIString("save_prop_changes_warning");
                if (CrewChief.Debugging)
                {
                    warningMessage = "You have unsaved changes. Click 'Yes' to save these changes (you will need to manually restart the application). Click 'No' to discard these changes";
                }
                if (MessageBox.Show(warningMessage, Configuration.getUIString("save_changes_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Boolean activeProfile = save();
                    if (!CrewChief.Debugging && activeProfile)
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
                        parent.Close(); // To turn off current app
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.nextPrefsRefreshAttemptTime = DateTime.UtcNow.Add(AUTO_SEARCH_DELAY_SPAN);

            if (this.searchTextBox.Text == DEFAULT_SEARCH_TEXT)
                return;

            if (this.searchTimer == null)
            {
                this.searchTimer = new Timer();
                this.searchTimer.Interval = 100;
                this.searchTimer.Tick += SearchTimer_Tick;
                this.searchTimer.Start();
            }
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.UtcNow < this.nextPrefsRefreshAttemptTime)
                return;

            var text = this.searchTextBox.Text;
            if (text == DEFAULT_SEARCH_TEXT)
            {
                this.searchTextPrev = text;
                return;
            }

            if (text != this.searchTextPrev)
            {
                // This is the case of clearing previously non-empty search
                if (string.IsNullOrWhiteSpace(text))
                    this.PopulatePrefsFiltered("", this.gameFilterPrev, this.specialFilterPrev, this.includeCommonPreferencesPrev, this.categoryFilterPrev);  // Clear filter out.
                // General case, new filter.
                else if (!string.IsNullOrWhiteSpace(text))
                    this.PopulatePrefsFiltered(text, this.gameFilterPrev, this.specialFilterPrev, this.includeCommonPreferencesPrev, this.categoryFilterPrev);  // Apply new filter.

                this.searchTextPrev = text;
            }
        }

        private void FilterBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var gameFilter = GameEnum.UNKNOWN;
            var specialFilter = SpecialFilter.UNKNOWN;
            if (this.filterBox.SelectedItem is ComboBoxItem<GameEnum>)
            {
                // Game filter selected.
                gameFilter = (this.filterBox.SelectedItem as ComboBoxItem<GameEnum>).Value;
                this.showCommonCheckbox.Enabled = true;
            }
            else
            {
                // Special filter selected.
                specialFilter = (this.filterBox.SelectedItem as ComboBoxItem<SpecialFilter>).Value;
                this.showCommonCheckbox.Enabled = false;
            }

            if ((gameFilter != GameEnum.UNKNOWN && gameFilter != this.gameFilterPrev)
                || (specialFilter != SpecialFilter.UNKNOWN && specialFilter != this.specialFilterPrev))
            {
                this.PopulatePrefsFiltered(this.searchTextPrev == this.DEFAULT_SEARCH_TEXT ? "" : this.searchTextPrev, gameFilter, specialFilter, this.includeCommonPreferencesPrev, this.categoryFilterPrev);

                // Save filter values but keep gameFilter and specialFilter mutually exclusive.
                if (gameFilter != GameEnum.UNKNOWN)
                {
                    this.gameFilterPrev = gameFilter;
                    this.specialFilterPrev = SpecialFilter.UNKNOWN;
                }

                if (specialFilter != SpecialFilter.UNKNOWN)
                {
                    this.specialFilterPrev = specialFilter;
                    this.gameFilterPrev = GameEnum.UNKNOWN;
                }
            }
        }

        private void CategoriesBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var categoryFilter = (this.categoriesBox.SelectedItem as ComboBoxItem<PropertyCategory>).Value;
            if (categoryFilter != this.categoryFilterPrev)
            {
                this.PopulatePrefsFiltered(this.searchTextPrev == this.DEFAULT_SEARCH_TEXT ? "" : this.searchTextPrev, this.gameFilterPrev,
                    this.specialFilterPrev, this.includeCommonPreferencesPrev, categoryFilter);

                this.categoryFilterPrev = categoryFilter;
            }

        }

        private void ShowCommonCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var showCommon = this.showCommonCheckbox.Checked;
            if (showCommon != this.includeCommonPreferencesPrev)
            {
                this.PopulatePrefsFiltered(this.searchTextPrev == this.DEFAULT_SEARCH_TEXT ? "" : this.searchTextPrev, this.gameFilterPrev, this.specialFilterPrev, showCommon, this.categoryFilterPrev);
                this.includeCommonPreferencesPrev = showCommon;
            }
        }


        private void SearchTextBox_GotFocus(object sender, EventArgs e)
        {
            if (this.searchTextBox.Text == DEFAULT_SEARCH_TEXT)
            {
                this.searchTextBox.Text = "";
                this.searchTextBox.ForeColor = Color.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.searchTextBox.Text))
            {
                this.searchTextBox.Text = DEFAULT_SEARCH_TEXT;
                this.searchTextBox.ForeColor = Color.Gray;

                // Not sure why I had this like that, ever.  Keep commented out for now.
                //this.exitButton.Select();
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.searchTextBox.Select();
                this.searchTextBox.Text = "";
                this.exitButton.Select();

                if (!string.IsNullOrWhiteSpace(this.searchTextPrev) && this.searchTextPrev != DEFAULT_SEARCH_TEXT)
                    this.PopulatePrefsFiltered(null, this.gameFilterPrev, this.specialFilterPrev, this.includeCommonPreferencesPrev, this.categoryFilterPrev);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                this.searchTextPrev = this.searchTextBox.Text;
                this.PopulatePrefsFiltered(this.searchTextPrev, this.gameFilterPrev, this.specialFilterPrev, this.includeCommonPreferencesPrev, this.categoryFilterPrev);
            }
        }


        private void PropertiesForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.E)
                this.searchTextBox.Select();
            else if (e.KeyCode == Keys.Escape)
            {
                // Close only if no search is active.
                if (this.searchTextBox.Text == DEFAULT_SEARCH_TEXT)
                    this.Close();
                else
                    this.SearchTextBox_KeyDown(sender, e); // Otherwise, forward.
            }
        }

        private void PopulatePrefsFiltered(string filter, GameEnum gameFilter, SpecialFilter specialFilter, bool includeCommon, PropertyCategory categoryFilter)
        {
            this.SuspendLayout();
            this.propertiesFlowLayoutPanel.SuspendLayout();

            var anyHits = false;
            var filterUpper = string.IsNullOrWhiteSpace(filter) ? filter : filter.ToUpperInvariant();
            foreach (var ctrl in this.propertiesFlowLayoutPanel.Controls)
            {
                if (ctrl is StringPropertyControl)
                {
                    var spc = ctrl as StringPropertyControl;
                    if (spc.filter.Applies(filterUpper, gameFilter, specialFilter, includeCommon, categoryFilter))
                    {
                        spc.Visible = true;
                        anyHits = true;
                    }
                    else
                        spc.Visible = false;
                }
                else if (ctrl is BooleanPropertyControl)
                {
                    var bpc = ctrl as BooleanPropertyControl;
                    if (bpc.filter.Applies(filterUpper, gameFilter, specialFilter, includeCommon, categoryFilter))
                    {
                        bpc.Visible = true;
                        anyHits = true;
                    }
                    else
                        bpc.Visible = false;
                }
                else if (ctrl is IntPropertyControl)
                {
                    var ipc = ctrl as IntPropertyControl;
                    if (ipc.filter.Applies(filterUpper, gameFilter, specialFilter, includeCommon, categoryFilter))
                    {
                        ipc.Visible = true;
                        anyHits = true;
                    }
                    else
                        ipc.Visible = false;
                }
                else if (ctrl is FloatPropertyControl)
                {
                    var fpc = ctrl as FloatPropertyControl;
                    if (fpc.filter.Applies(filterUpper, gameFilter, specialFilter, includeCommon, categoryFilter))
                    {
                        fpc.Visible = true;
                        anyHits = true;
                    }
                    else
                        fpc.Visible = false;
                }
                else if (ctrl is ListPropertyControl)
                {
                    var lpc = ctrl as ListPropertyControl;
                    if (lpc.filter.Applies(filterUpper, gameFilter, specialFilter, includeCommon, categoryFilter))
                    {
                        lpc.Visible = true;
                        anyHits = true;
                    }
                    else
                        lpc.Visible = false;
                }
                else if (ctrl is Spacer)
                {
                    var s = ctrl as Spacer;
                    if (!string.IsNullOrWhiteSpace(filterUpper)
                        || gameFilter != GameEnum.UNKNOWN
                        || specialFilter != SpecialFilter.ALL_PREFERENCES
                        || categoryFilter != PropertyCategory.ALL)
                        s.Visible = false;  // If any filtering is applied, hide splitters.
                    else
                        s.Visible = true;
                }
            }

            if (!anyHits)
                this.propertiesFlowLayoutPanel.Controls.Add(this.noMatchedLabel);
            else
                this.propertiesFlowLayoutPanel.Controls.Remove(this.noMatchedLabel);

            this.propertiesFlowLayoutPanel.ResumeLayout();
            this.ResumeLayout();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void restoreButton_Click(object sender, EventArgs e)
        {
            // Note that even after this said yes, there's still "Save" step.  Maybe dialog isn't necessary.
            var result = MessageBox.Show(Configuration.getUIString("reset_warning_text"), Configuration.getUIString("reset_warning_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
                return;

            foreach (var ctrl in this.propertiesFlowLayoutPanel.Controls)
            {
                if (ctrl is StringPropertyControl)
                {
                    var spc = ctrl as StringPropertyControl;
                    spc.button1_Click(sender, e);
                }
                else if (ctrl is ListPropertyControl)
                {
                    var spc = ctrl as ListPropertyControl;
                    spc.button1_Click(sender, e);
                }
                else if (ctrl is BooleanPropertyControl)
                {
                    var bpc = ctrl as BooleanPropertyControl;
                    bpc.button1_Click(sender, e);
                }
                else if (ctrl is IntPropertyControl)
                {
                    var ipc = ctrl as IntPropertyControl;
                    ipc.button1_Click(sender, e);
                }
                else if (ctrl is FloatPropertyControl)
                {
                    var fpc = ctrl as FloatPropertyControl;
                    fpc.button1_Click(sender, e);
                }
            }
        }

        private void createNewProfileButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = UserSettings.userProfilesPath,
                Title = "Create new profile",
                //CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "json",
                Filter = "Json files (*.json)|*.json",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                if (copySettingsFromCurrentSelectionCheckBox.Checked)
                {
                    UserSettings.UserProfileSettings currentSelection = UserSettings.GetUserSettings().loadUserSettings(Path.Combine(UserSettings.userProfilesPath, profileSelectionComboBox.SelectedItem.ToString() + ".json"));
                    UserSettings.saveUserSettingsFile(currentSelection, Path.GetFileName(saveFileDialog.FileName));
                }
                else
                {
                    UserSettings.UserProfileSettings newUserProfile = new UserSettings.UserProfileSettings();
                    foreach (var ctrl in this.propertiesFlowLayoutPanel.Controls)
                    {
                        if (ctrl is StringPropertyControl)
                        {
                            var spc = ctrl as StringPropertyControl;
                            newUserProfile.userSettings.Add(spc.propertyId, spc.defaultValue);
                        }
                        else if (ctrl is ListPropertyControl)
                        {
                            var lpc = ctrl as ListPropertyControl;
                            newUserProfile.userSettings.Add(lpc.propertyId, ListPropertyValues.getInvariantValueForLabel(lpc.propertyId, lpc.defaultValue));
                        }
                        else if (ctrl is BooleanPropertyControl)
                        {
                            var bpc = ctrl as BooleanPropertyControl;
                            newUserProfile.userSettings.Add(bpc.propertyId, bpc.defaultValue);
                        }
                        else if (ctrl is IntPropertyControl)
                        {
                            var ipc = ctrl as IntPropertyControl;
                            newUserProfile.userSettings.Add(ipc.propertyId, ipc.defaultValue);
                        }
                        else if (ctrl is FloatPropertyControl)
                        {
                            var fpc = ctrl as FloatPropertyControl;
                            newUserProfile.userSettings.Add(fpc.propertyId, fpc.defaultValue);
                        }
                    }
                    UserSettings.saveUserSettingsFile(newUserProfile, Path.GetFileName(saveFileDialog.FileName));
                }
                profileSelectionComboBox.Items.Clear();
                profileNames.Clear();
                List<string> settingsProfileFiles = Directory.GetFiles(UserSettings.userProfilesPath, "*.json", SearchOption.TopDirectoryOnly).ToList();
                foreach (var file in settingsProfileFiles)
                {
                    profileNames.Add(Path.GetFileNameWithoutExtension(file));
                }
                foreach (var profile in profileNames)
                {
                    profileSelectionComboBox.Items.Add(profile);
                }
                profileSelectionComboBox.SelectedIndex = profileNames.IndexOf(Path.GetFileNameWithoutExtension(saveFileDialog.FileName));
                if (activateNewProfileCheckBox.Checked)
                {
                    loadProfileButton_Click(null, null);
                }
                //this.searchTextBox.Text = saveFileDialog1.FileName;
            }
        }

        private void profileSelectionComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            updatedPropertiesRequiringRestart.Clear();
            UserSettings.UserProfileSettings currentSelection = UserSettings.GetUserSettings().loadUserSettings(Path.Combine(UserSettings.userProfilesPath, profileSelectionComboBox.SelectedItem.ToString() + ".json"));
            foreach (var setting in currentSelection.userSettings)
            {
                foreach (var ctrl in this.propertiesFlowLayoutPanel.Controls)
                {
                    if (ctrl is StringPropertyControl)
                    {
                        var spc = ctrl as StringPropertyControl;
                        if (spc.propertyId.Equals(setting.Key))
                        {
                            spc.setValue((String)setting.Value);
                        }
                    }
                    else if (ctrl is BooleanPropertyControl)
                    {
                        var bpc = ctrl as BooleanPropertyControl;
                        if (bpc.propertyId.Equals(setting.Key))
                        {
                            bpc.setValue(Convert.ToBoolean(setting.Value));
                        }
                    }
                    else if (ctrl is IntPropertyControl)
                    {
                        var ipc = ctrl as IntPropertyControl;
                        if (ipc.propertyId.Equals(setting.Key))
                        {
                            ipc.setValue(Convert.ToInt32(setting.Value));
                        }
                    }
                    else if (ctrl is FloatPropertyControl)
                    {
                        var fpc = ctrl as FloatPropertyControl;
                        if (fpc.propertyId.Equals(setting.Key))
                        {
                            fpc.setValue(Convert.ToSingle(setting.Value));
                        }
                    }
                    else if (ctrl is ListPropertyControl)
                    {
                        var lpc = ctrl as ListPropertyControl;
                        if (lpc.propertyId.Equals(setting.Key))
                        {
                            lpc.setValue((String)setting.Value);
                        }
                    }
                }
            }
            updateLabelsAfterChangingProfile();
        }

        private void updateLabelsAfterChangingProfile()
        {
            userProfileSettingsGroupBox.Text = Configuration.getUIString("user_profile_settings") + " - " + profileSelectionComboBox.SelectedItem.ToString();
            isActiveProfile = profileSelectionComboBox.SelectedItem.ToString().Equals(Path.GetFileNameWithoutExtension(UserSettings.GetUserSettings().getString("current_settings_profile")));
            if (!isActiveProfile)
            {
                saveButton.Text = Configuration.getUIString("save_profile_settings");
                loadProfileButton.Enabled = true;
                if (updatedPropertiesRequiringRestart.Count() > 0)
                {
                    this.loadProfileButton.Text = CrewChief.Debugging ? "Activate profile (manual restart required)" :
                        Configuration.getUIString("load_profile_and_restart");
                }
                else
                {
                    this.loadProfileButton.Text = Configuration.getUIString("load_profile");
                }
            }
            else
            {
                loadProfileButton.Enabled = false;
            }
        }

        private void loadProfileButton_Click(object sender, EventArgs e)
        {
            UserSettings.GetUserSettings().setProperty("current_settings_profile", profileSelectionComboBox.SelectedItem.ToString() + ".json");
            UserSettings.GetUserSettings().saveUserSettings();
            if (!CrewChief.Debugging && updatedPropertiesRequiringRestart.Count() > 0)
            {
                // have to add "multi" to the start args so the app can restart
                List<String> startArgs = new List<string>();
                startArgs.AddRange(Environment.GetCommandLineArgs());
                if (!startArgs.Contains("multi"))
                {
                    startArgs.Add("multi");
                }
                System.Diagnostics.Process.Start(Application.ExecutablePath, String.Join(" ", startArgs.ToArray())); // to start new instance of application
                hasChanges = false;
                parent.Close(); //to turn off current app
            }
            else
            {
                this.Text = Configuration.getUIString("properties_form");
                userProfileGroupBox.Text = Configuration.getUIString("user_profile")
                    + " (" + Configuration.getUIString("active_label") + " "
                    + Path.GetFileNameWithoutExtension(UserSettings.GetUserSettings().getString("current_settings_profile")) + ")";
                updateLabelsAfterChangingProfile();
                UserSettings.currentUserProfileFileName = profileSelectionComboBox.SelectedItem.ToString() + ".json";
                foreach (var control in this.propertiesFlowLayoutPanel.Controls)
                {
                    if (control.GetType() == typeof(StringPropertyControl))
                    {
                        StringPropertyControl stringControl = (StringPropertyControl)control;
                        UserSettings.GetUserSettings().setProperty(stringControl.propertyId, stringControl.getValue());
                    }
                    else if (control.GetType() == typeof(ListPropertyControl))
                    {
                        ListPropertyControl listControl = (ListPropertyControl)control;
                        UserSettings.GetUserSettings().setProperty(listControl.propertyId, listControl.getValue());
                    }
                    else if (control.GetType() == typeof(IntPropertyControl))
                    {
                        IntPropertyControl intControl = (IntPropertyControl)control;
                        UserSettings.GetUserSettings().setProperty(intControl.propertyId, intControl.getValue());
                    }
                    if (control.GetType() == typeof(FloatPropertyControl))
                    {
                        FloatPropertyControl floatControl = (FloatPropertyControl)control;
                        UserSettings.GetUserSettings().setProperty(floatControl.propertyId, floatControl.getValue());
                    }
                    if (control.GetType() == typeof(BooleanPropertyControl))
                    {
                        BooleanPropertyControl boolControl = (BooleanPropertyControl)control;
                        UserSettings.GetUserSettings().setProperty(boolControl.propertyId, boolControl.getValue());
                    }
                }
            }
        }

        private Boolean changeRequiresRestart(String metadata)
        {
            if (!string.IsNullOrWhiteSpace(metadata))
            {
                var metadataFlags = metadata.Split(';');
                foreach (var metadataFlag in metadataFlags)
                {
                    if (metadataFlag == "RESTART_REQUIRED")
                        return true;
                }
            }
            return false;
        }
    }
}
