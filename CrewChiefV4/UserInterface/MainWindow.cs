﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using AutoUpdaterDotNET;
using System.Net;
using System.Xml.Linq;
using System.IO.Compression;
using CrewChiefV4.Audio;
using CrewChiefV4.UserInterface;
using System.Diagnostics;
using CrewChiefV4.commands;
using CrewChiefV4.GameState;
using CrewChiefV4.Events;

namespace CrewChiefV4
{
    public partial class MainWindow : Form
    {
        // used when retrying downloads:
        private Boolean usingRetryAddressForSoundPack = false;
        private Boolean usingRetryAddressForDriverNames = false;
        private Boolean usingRetryAddressForPersonalisations = false;

        private Boolean willNeedAnotherSoundPackDownload = false;
        private Boolean willNeedAnotherPersonalisationsDownload = false;
        private Boolean willNeedAnotherDrivernamesDownload = false;

        private String driverNamesTempFileName = "temp_driver_names.zip";
        private String drivernamesDownloadURL;

        private String soundPackTempFileName = "temp_sound_pack.zip";
        private String soundPackDownloadURL;

        private String personalisationsTempFileName = "temp_personalisations.zip";
        private String personalisationsDownloadURL;

        private Boolean isDownloadingDriverNames = false;
        private Boolean isDownloadingSoundPack = false;
        private Boolean isDownloadingPersonalisations = false;
        private Boolean newSoundPackAvailable = false;
        private Boolean newDriverNamesAvailable = false;
        private Boolean newPersonalisationsAvailable = false;

        public struct ControllerUiEntry
        {
            public string uiText;
            public bool isConnected;

            public ControllerUiEntry(string uiText, bool isConnected)
            {
                this.uiText = uiText;
                this.isConnected = isConnected;
            }

            public override string ToString()
            {
                return uiText;
            }
        }

        // Shared with worker thread.  This should be disposed after root threads stopped, in GlobalResources.Dispose.
        private ControllerConfiguration controllerConfiguration;

        public CrewChief crewChief;

        private Boolean isAssigningButton = false;

        private bool _IsAppRunning;

        private Boolean runListenForChannelOpenThread = false;

        private Boolean runListenForButtonPressesThread = false;

        private TimeSpan buttonCheckInterval = TimeSpan.FromMilliseconds(100);

        public static VoiceOptionEnum voiceOption;

        // the new update stuff all hosted on the CrewChief website
        // this is the physical file:
        // private static String autoUpdateXMLURL1 = "http://thecrewchief.org/downloads/auto_update_data_primary.xml";
        // this is the file accessed via the PHP download script:
        private static String autoUpdateXMLURL1 = "http://thecrewchief.org/downloads.php?do=downloadxml";

        // the legacy update stuff hosted on GoogleDrive with downloads on the isnais ftp server
        private static String autoUpdateXMLURL2 = "https://drive.google.com/uc?export=download&id=0B4KQS820QNFbWWFjaDAzRldMNUE";

        private Boolean preferAlternativeDownloadSite = UserSettings.GetUserSettings().getBoolean("prefer_alternative_download_site");
        private Boolean minimizeToTray = UserSettings.GetUserSettings().getBoolean("minimize_to_tray");
        private Boolean rejectMessagesWhenTalking = UserSettings.GetUserSettings().getBoolean("reject_message_when_talking");
        public static Boolean forceMinWindowSize = UserSettings.GetUserSettings().getBoolean("force_min_window_size");
        private readonly int holdButtonPollFrequency = UserSettings.GetUserSettings().getInt("hold_button_poll_frequency");
        private readonly int sreWaitTime = UserSettings.GetUserSettings().getInt("sre_wait_time");

        public ControlWriter consoleWriter = null;

        public static float currentMessageVolume = -1;
        private NotifyIcon notificationTrayIcon;
        private ToolStripItem contextMenuStartItem;
        private ToolStripItem contextMenuStopItem;
        private ToolStripMenuItem contextMenuGamesMenu;
        private ToolStripItem contextMenuPreferencesItem;

        // instance 
        public static MainWindow instance = null;
        public static object instanceLock = new object();

        // True, while we are in a constructor.
        private bool constructingWindow = false;

        public static bool autoScrollConsole = true;
        private Thread youWotThread = null;
        private Thread assignButtonThread = null;
        public bool formClosed = false;

        public static bool soundTestMode = false;
        public static bool shouldSaveTrace = false;

        private AutoResetEvent consoleUpdateThreadWakeUpEvent = new AutoResetEvent(false);
        private bool consoleUpdateThreadRunning = false;

        private AutoResetEvent controllerRescanThreadWakeUpEvent = new AutoResetEvent(false);
        private bool controllerRescanThreadRunning = false;

        // This lock must be held while we are updating controller devices or updating assignments.
        private object controllerWriteLock = new object();

        public static bool disableControllerReacquire = false;

        private Boolean isMuted = false;
        private float messageVolumeToRestore = -1;

        private const int WM_DEVICECHANGE = 0x219;
        private const int DBT_DEVNODES_CHANGED = 0x0007;

        public void killChief()
        {
            crewChief.stop();
        }

        protected override void WndProc(ref Message m)
        {
            if (!MainWindow.disableControllerReacquire && this.controllerRescanThreadRunning)
            {
                if (m.Msg == WM_DEVICECHANGE)
                {
                    if ((int)m.WParam == DBT_DEVNODES_CHANGED)
                    {
                        if (!this.controllerConfiguration.scanInProgress)
                        {
                            this.scanControllers.Enabled = false;
#if DEBUG
                            var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
                            this.reacquireControllerList();
#if DEBUG
                            watch.Stop();
                            Debug.WriteLine("Controller re-acquisition took: " + watch.ElapsedTicks * 1000 / System.Diagnostics.Stopwatch.Frequency + "ms to shutdown");
#endif
                            if (!this.IsAppRunning)
                            {
                                this.scanControllers.Enabled = true;
                            }
                        }
                    }
                }
            }
            base.WndProc(ref m);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Set up console update thread.  We need this because we call Console.WriteLine from random threads.
            ThreadStart ts = consoleUpdateThreadWorker;
            var consoleUpdateThread = new Thread(ts);
            consoleUpdateThread.Name = "MainWindow.consoleUpdateThreadWorker";
            consoleUpdateThreadRunning = true;
            ThreadManager.RegisterResourceThread(consoleUpdateThread);
            consoleUpdateThread.Start();

            // Set up Controller Rescan thread
            ts = controllerRescanThreadWorker;
            var controllerRescanThread = new Thread(ts);
            controllerRescanThread.Name = "MainWindow.controllerRescanThreadWorker";
            controllerRescanThreadRunning = true;
            ThreadManager.RegisterResourceThread(controllerRescanThread);
            controllerRescanThread.Start();

            // Run immediately if requested.
            // Note that it is not safe to run immidiately from the constructor, becasue form handle
            // is created on a message pump, at undefined moment, which prevents Invoke from
            // working while constructor is running.
            Debug.Assert(this.IsHandleCreated);
            this.controllersList.DrawItem += this.ControllersList_DrawItem;
            this.controllersList.MeasureItem += this.ControllersList_MeasureItem;
            this.controllersList.DrawMode = DrawMode.OwnerDrawVariable;
            this.reacquireControllerList();

            if (UserSettings.GetUserSettings().getBoolean("run_immediately") &&
                GameDefinition.getGameDefinitionForFriendlyName(gameDefinitionList.Text) != null)
            {
                doStartAppStuff();

                // Will wait for threads to start, possible file load and enable the button.
                ThreadManager.DoWatchStartup(crewChief);
            }

            if (forceMinWindowSize)
            {
                this.MinimumSize = new System.Drawing.Size(1160, 730);
            }

            // Some update test code - uncomment this to allow the app to process an update .zip file in the root of the sound pack
            /*
            ZipFile.ExtractToDirectory(AudioPlayer.soundFilesPath + @"\" + soundPackTempFileName, AudioPlayer.soundFilesPath + @"\sounds_temp");
            UpdateHelper.ProcessFileUpdates(AudioPlayer.soundFilesPath + @"\sounds_temp");
            UpdateHelper.MoveDirectory(AudioPlayer.soundFilesPath + @"\sounds_temp", AudioPlayer.soundFilesPath);                   
            */

            // do the auto updating stuff in a separate Thread
            if (!CrewChief.Debugging ||
                SoundPackVersionsHelper.currentSoundPackVersion <= 0 || SoundPackVersionsHelper.currentPersonalisationsVersion <= 0 || SoundPackVersionsHelper.currentDriverNamesVersion <= 0)
            {
                var checkForUpdatesThread = new Thread(() =>
                {
                    try
                    {
                        Console.WriteLine("Checking for updates");
                        String firstUpdate = preferAlternativeDownloadSite ? autoUpdateXMLURL2 : autoUpdateXMLURL1;
                        String secondUpdate = preferAlternativeDownloadSite ? autoUpdateXMLURL1 : autoUpdateXMLURL2;

                        Thread.CurrentThread.IsBackground = true;
                        // now the sound packs
                        downloadSoundPackButton.Text = Configuration.getUIString("checking_sound_pack_version");
                        downloadDriverNamesButton.Text = Configuration.getUIString("checking_driver_names_version");
                        downloadPersonalisationsButton.Text = Configuration.getUIString("checking_personalisations_version");

                        String[] commandLineArgs = Environment.GetCommandLineArgs();
                        Boolean appRestarted = false;
                        Boolean skipUpdates = false;
                        if (commandLineArgs != null)
                        {
                            foreach (String arg in commandLineArgs)
                            {
                                if ("SKIP_UPDATES".Equals(arg))
                                {
                                    Console.WriteLine("Skipping application update check. To enable this check, run the app *without* the SKIP_UPDATES command line argument");
                                    skipUpdates = true;
                                    continue;
                                }
                                if ("app_restart".Equals(arg))
                                {
                                    appRestarted = true;
                                    continue;
                                }
                                if(appRestarted && skipUpdates)
                                {
                                    break;
                                }
                            }
                        }
                        Boolean gotUpdateData = false;
                        try
                        {
                            if (!skipUpdates)
                            {
                                AutoUpdater.Start(firstUpdate);
                            }
                            if (firstUpdate == autoUpdateXMLURL1 && CrewChief.gameDefinition != null)
                            {
                                firstUpdate += "&lastplayed=" + (appRestarted ? "app_restart" : CrewChief.gameDefinition.gameEnum.ToString());
                            }
                            string xml = new WebClient().DownloadString(firstUpdate);
                            gotUpdateData = SoundPackVersionsHelper.parseUpdateData(xml);
                        }
                        catch (Exception) { }
                        if (gotUpdateData)
                        {
                            Console.WriteLine("Got update data from primary URL: " + firstUpdate.Substring(0, 24));
                        }
                        else
                        {
                            Console.WriteLine("Unable to get update data with primary URL, trying secondary");
                            try
                            {
                                if (formClosed)
                                {
                                    return;                                
                                }
                                if (!skipUpdates)
                                {
                                    AutoUpdater.Start(secondUpdate);
                                }
                                if (secondUpdate == autoUpdateXMLURL1 && CrewChief.gameDefinition != null)
                                {
                                    secondUpdate += "&lastplayed=" + (appRestarted ? "app_restart" : CrewChief.gameDefinition.gameEnum.ToString());
                                }
                                string xml = new WebClient().DownloadString(secondUpdate);
                                gotUpdateData = SoundPackVersionsHelper.parseUpdateData(xml);
                            }
                            catch (Exception) { }
                        }
                        if (formClosed)
                        {
                            return;
                        }
                        if (gotUpdateData)
                        {
                            downloadSoundPackButton.Enabled = false;
                            downloadSoundPackButton.BackColor = Color.LightGray;
                            downloadSoundPackButton.Text = Configuration.getUIString("sound_pack_is_up_to_date");
                            if (SoundPackVersionsHelper.latestSoundPackVersion == -1 && SoundPackVersionsHelper.currentSoundPackVersion == -1)
                            {
                                downloadSoundPackButton.Text = Configuration.getUIString("no_sound_pack_detected_unable_to_locate_update");
                            }
                            else if (SoundPackVersionsHelper.latestSoundPackVersion > SoundPackVersionsHelper.currentSoundPackVersion &&
                                SoundPackVersionsHelper.voiceMessageUpdatePacks.Count > 0)
                            {
                                SoundPackVersionsHelper.SoundPackData soundPackUpdateData = SoundPackVersionsHelper.voiceMessageUpdatePacks[0];
                                foreach (SoundPackVersionsHelper.SoundPackData soundPack in SoundPackVersionsHelper.voiceMessageUpdatePacks)
                                {
                                    if (SoundPackVersionsHelper.currentSoundPackVersion < soundPack.upgradeFromVersion)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        soundPackUpdateData = soundPack;
                                    }
                                }
                                soundPackDownloadURL = soundPackUpdateData.url;
                                if (soundPackDownloadURL != null)
                                {
                                    Console.WriteLine("Current sound pack version " + SoundPackVersionsHelper.currentSoundPackVersion + " is out of date, next update is " + soundPackUpdateData.url);
                                    willNeedAnotherSoundPackDownload = soundPackUpdateData.willRequireAnotherUpdate;
                                    downloadSoundPackButton.Text = Configuration.getUIString(SoundPackVersionsHelper.latestSoundPackVersion == -1 ?
                                        "no_sound_pack_detected_press_to_download" : "updated_sound_pack_available_press_to_download");
                                    if (!IsAppRunning)
                                    {
                                        downloadSoundPackButton.Enabled = true;
                                    }
                                    downloadSoundPackButton.BackColor = Color.LightGreen;
                                    newSoundPackAvailable = true;
                                }
                            }

                            downloadPersonalisationsButton.Enabled = false;
                            downloadPersonalisationsButton.BackColor = Color.LightGray;
                            downloadPersonalisationsButton.Text = Configuration.getUIString("personalisations_are_up_to_date");
                            if (SoundPackVersionsHelper.latestPersonalisationsVersion == -1 && SoundPackVersionsHelper.currentPersonalisationsVersion == -1)
                            {
                                downloadPersonalisationsButton.Text = Configuration.getUIString("no_personalisations_detected_unable_to_locate_update");
                            }
                            else if (SoundPackVersionsHelper.latestPersonalisationsVersion > SoundPackVersionsHelper.currentPersonalisationsVersion &&
                                SoundPackVersionsHelper.personalisationUpdatePacks.Count > 0)
                            {
                                SoundPackVersionsHelper.SoundPackData personalisationPackUpdateData = SoundPackVersionsHelper.personalisationUpdatePacks[0];
                                foreach (SoundPackVersionsHelper.SoundPackData personalisationPack in SoundPackVersionsHelper.personalisationUpdatePacks)
                                {
                                    if (SoundPackVersionsHelper.currentPersonalisationsVersion < personalisationPack.upgradeFromVersion)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        personalisationPackUpdateData = personalisationPack;
                                    }
                                }
                                personalisationsDownloadURL = personalisationPackUpdateData.url;
                                if (personalisationsDownloadURL != null)
                                {
                                    Console.WriteLine("Current personalisations pack version " + SoundPackVersionsHelper.currentPersonalisationsVersion + " is out of date, next update is " + personalisationPackUpdateData.url);
                                    willNeedAnotherPersonalisationsDownload = personalisationPackUpdateData.willRequireAnotherUpdate;
                                    downloadPersonalisationsButton.Text = Configuration.getUIString(SoundPackVersionsHelper.latestPersonalisationsVersion == -1 ?
                                        "no_personalisations_detected_press_to_download" : "updated_personalisations_available_press_to_download");
                                    if (!IsAppRunning)
                                    {
                                        downloadPersonalisationsButton.Enabled = true;
                                    }
                                    downloadPersonalisationsButton.BackColor = Color.LightGreen;
                                    newPersonalisationsAvailable = true;
                                }
                            }

                            downloadDriverNamesButton.Text = Configuration.getUIString("driver_names_are_up_to_date");
                            downloadDriverNamesButton.Enabled = false;
                            downloadDriverNamesButton.BackColor = Color.LightGray;
                            if (SoundPackVersionsHelper.latestDriverNamesVersion == -1 && SoundPackVersionsHelper.currentDriverNamesVersion == -1)
                            {
                                downloadDriverNamesButton.Text = Configuration.getUIString("no_driver_names_detected_unable_to_locate_update");
                            }
                            else if (SoundPackVersionsHelper.latestDriverNamesVersion > SoundPackVersionsHelper.currentDriverNamesVersion &&
                                SoundPackVersionsHelper.drivernamesUpdatePacks.Count > 0)
                            {
                                SoundPackVersionsHelper.SoundPackData drivernamesPackUpdateData = SoundPackVersionsHelper.drivernamesUpdatePacks[0];
                                foreach (SoundPackVersionsHelper.SoundPackData drivernamesPack in SoundPackVersionsHelper.drivernamesUpdatePacks)
                                {
                                    if (SoundPackVersionsHelper.currentDriverNamesVersion < drivernamesPack.upgradeFromVersion)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        drivernamesPackUpdateData = drivernamesPack;
                                    }
                                }
                                drivernamesDownloadURL = drivernamesPackUpdateData.url;
                                if (drivernamesDownloadURL != null)
                                {
                                    Console.WriteLine("Current driver names pack version " + SoundPackVersionsHelper.currentDriverNamesVersion + " is out of date, next update is " + drivernamesPackUpdateData.url);
                                    willNeedAnotherDrivernamesDownload = drivernamesPackUpdateData.willRequireAnotherUpdate;
                                    downloadDriverNamesButton.Text = Configuration.getUIString(SoundPackVersionsHelper.latestDriverNamesVersion == -1 ?
                                        "no_driver_names_detected_press_to_download" : "updated_driver_names_available_press_to_download");
                                    if (!IsAppRunning)
                                    {
                                        downloadDriverNamesButton.Enabled = true;
                                    }
                                    downloadDriverNamesButton.BackColor = Color.LightGreen;
                                    newDriverNamesAvailable = true;
                                }
                            }

                            if (newSoundPackAvailable || newPersonalisationsAvailable || newDriverNamesAvailable)
                            {
                                // Ok, we have something available for download (any of the buttons is green).
                                // Restore CC once so that user gets higher chance of noticing.
                                // I am not sure what is the best approach, we could also have text in the context menu,
                                // but I definitely dislike Balloons and other distracting methods.  But, basically if we choose
                                // to do anything, do it here.
                                // This has limitation if, say, we have sound pack available, and at next startup we have driver pack
                                // available, one property is not enough.  But this is ultra rare and not worth complications.

                                if (!UserSettings.GetUserSettings().getBoolean("update_notify_attempted"))
                                {
                                    // Do this once per update availability.
                                    UserSettings.GetUserSettings().setProperty("update_notify_attempted", true);
                                    UserSettings.GetUserSettings().saveUserSettings();

                                    // Slight race with minimize on startup :D
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        this.RestoreFromTray();
                                    });
                                }
                            }
                            else
                            {
                                // If there are no updates available, clear the update_notify_attempted flag if it is set.
                                if (UserSettings.GetUserSettings().getBoolean("update_notify_attempted"))
                                {
                                    UserSettings.GetUserSettings().setProperty("update_notify_attempted", false);
                                    UserSettings.GetUserSettings().saveUserSettings();
                                }
                            }

                            Console.WriteLine("Check for updates completed");
                        }
                        else
                        {
                            Console.WriteLine("Unable to get update data");
                        }
                    }
                    catch (Exception)
                    {
                        // This can throw on form close.
                    }
                });
                checkForUpdatesThread.Name = "MainWindow.checkForUpdatesThread";
                ThreadManager.RegisterResourceThread(checkForUpdatesThread);
                checkForUpdatesThread.Start();
            }
            else
            {
                Console.WriteLine("Skipping update check in debug mode");
            }

            if (UserSettings.GetUserSettings().getBoolean("minimize_on_startup"))
            {
                if (this.minimizeToTray)
                    this.HideToTray();
                else
                    this.WindowState = FormWindowState.Minimized;
            }
        }

        private void ControllersList_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= this.controllersList.Items.Count)
            {
                // WTF?
                return;
            }
            var entry = (MainWindow.ControllerUiEntry)this.controllersList.Items[e.Index];

            // Measure the string.
            var txtSize = e.Graphics.MeasureString(entry.uiText, this.Font);

            // Set the required size.
            e.ItemHeight = (int)txtSize.Height;
            e.ItemWidth = (int)txtSize.Width;
        }

        private void ControllersList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= this.controllersList.Items.Count)
            {
                // WTF?
                return;
            }

            // Draw the background of the ListBox control for each item.
            e.DrawBackground();

            var entry = (MainWindow.ControllerUiEntry)this.controllersList.Items[e.Index];

            var brush = entry.isConnected ? Brushes.Black : Brushes.Gray;
            if (e.State.HasFlag(DrawItemState.Selected))
            {
                brush = Brushes.White;
            }

            SizeF txt_size = e.Graphics.MeasureString(entry.uiText, e.Font);

            e.Graphics.DrawString(this.controllersList.Items[e.Index].ToString(),
                e.Font, brush, e.Bounds, StringFormat.GenericDefault);

            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }

        private void HideToTray()
        {
            if (!this.minimizeToTray)
                return;

            this.ShowInTaskbar = false;
            this.Hide();
            this.notificationTrayIcon.Visible = true;

            // Do not mess with WindowState here, causes weirdest problems.
        }

        private void RestoreFromTray()
        {
            if (!this.minimizeToTray)
                return;

            this.ShowInTaskbar = true;
            this.notificationTrayIcon.Visible = false;
            this.Show();

            // This is necessary to bring window to the foreground.  Why ffs BringToFront doesn't work is beyound me.
            this.WindowState = FormWindowState.Normal;
        }

        /*
         changes the current message playback volume. If saveChange is true the change is written to the properties file,
         if updateSlider is true the slider is moved to reflect the new volume
         */
        public void updateMessagesVolume(float messagesVolume, Boolean saveChange, Boolean updateSlider, bool forceUpdate = false)
        {
            if (messagesVolume < 0)
            {
                currentMessageVolume = 0;
            }
            else if (messagesVolume > 1)
            {
                currentMessageVolume = 1;
            }
            else
            {
                currentMessageVolume = messagesVolume;
            }
            // no point in setting output channel volume with nAudio - the sound file volumes are scaled separately
            if (!UserSettings.GetUserSettings().getBoolean("use_naudio")
                || forceUpdate)
            {
                setOuputChannelVolume(currentMessageVolume);
            }
            if (saveChange)
            {
                UserSettings.GetUserSettings().setProperty("messages_volume", currentMessageVolume);
                UserSettings.GetUserSettings().saveUserSettings();
            }
            if (updateSlider)
            {
                messagesVolumeSlider.Value = (int)(currentMessageVolume * 100f);
            }
        }

        private void messagesVolumeSlider_Scroll(object sender, EventArgs e)
        {
            float volFloat = (float)messagesVolumeSlider.Value / 100;
            // no point in setting output channel volume with nAudio - the sound file volumes are scaled separately
            if (!UserSettings.GetUserSettings().getBoolean("use_naudio"))
            {
                setOuputChannelVolume(volFloat);
            }
            currentMessageVolume = volFloat;
            UserSettings.GetUserSettings().setProperty("messages_volume", volFloat);
            UserSettings.GetUserSettings().saveUserSettings();
        }

        // update the output channel volume - not used for nAudio
        private void setOuputChannelVolume(float vol)
        {
            int NewVolume = (int)(((float)ushort.MaxValue) * vol);
            // Set the same volume for both the left and the right channels
            uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
            // Set the volume
            NativeMethods.waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);            
        }

        private void backgroundVolumeSlider_Scroll(object sender, EventArgs e)
        {
            float volFloat = (float)backgroundVolumeSlider.Value / 100;
            UserSettings.GetUserSettings().setProperty("background_volume", volFloat);
            UserSettings.GetUserSettings().saveUserSettings();
        }

        public bool IsAppRunning
        {
            get
            {
                return _IsAppRunning;
            }
            set
            {
                _IsAppRunning = value;
                startApplicationButton.Text = _IsAppRunning 
                    ? !this.recordSession.Checked ? Configuration.getUIString("stop") : Configuration.getUIString("stop_and_save")
                    : Configuration.getUIString("start_application");
                downloadDriverNamesButton.Enabled = !value && newDriverNamesAvailable;
                downloadSoundPackButton.Enabled = !value && newSoundPackAvailable;
                downloadPersonalisationsButton.Enabled = !value && newPersonalisationsAvailable;
            }
        }

        private void setSelectedGameType()
        {
            String[] commandLineArgs = Environment.GetCommandLineArgs();
            Boolean setFromCommandLine = false;
            if (commandLineArgs != null)
            {
                foreach (String arg in commandLineArgs)
                {
                    if (arg.Equals(GameDefinition.raceRoom.gameEnum.ToString()))
                    {
                        Console.WriteLine("Set Raceroom mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.raceRoom.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals(GameDefinition.pCars2.gameEnum.ToString()))
                    {
                        Console.WriteLine("Set PCars 2 mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.pCars2.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals(GameDefinition.pCars32Bit.gameEnum.ToString()))
                    {
                        Console.WriteLine("Set PCars 32bit mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.pCars32Bit.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals(GameDefinition.pCars64Bit.gameEnum.ToString()))
                    {
                        Console.WriteLine("Set PCars 64bit mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.pCars64Bit.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }

                    else if (arg.Equals(GameDefinition.pCarsNetwork.gameEnum.ToString()))
                    {
                        Console.WriteLine("Set PCars network mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.pCarsNetwork.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals(GameDefinition.pCars2Network.gameEnum.ToString()))
                    {
                        Console.WriteLine("Set PCars2 network mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.pCars2Network.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals(GameDefinition.assetto64Bit.gameEnum.ToString()))
                    {
                        Console.WriteLine("Set Assetto Corsa mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.assetto64Bit.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals(GameDefinition.assetto32Bit.gameEnum.ToString()))
                    {
                        Console.WriteLine("Set Assetto Corsa mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.assetto32Bit.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals(GameDefinition.iracing.gameEnum.ToString()))
                    {
                        Console.WriteLine("Set iRacing mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.iracing.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    // special cases for RF1 versions
                    else if (arg.Equals("AMS"))
                    {
                        Console.WriteLine("Set Autombilista mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.automobilista.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals("FTRUCK"))
                    {
                        Console.WriteLine("Set FTruck mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.ftruck.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals("RF1"))
                    {
                        Console.WriteLine("Set RF1 mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.rFactor1.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals("MARCAS"))
                    {
                        Console.WriteLine("Set Copa de Marcas mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.marcas.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals("GSC"))
                    {
                        Console.WriteLine("Set GSC mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.gameStockCar.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals("RF2") || arg.Equals("RF2_64BIT"))
                    {
                        Console.WriteLine("Set RF2 mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.rfactor2_64bit.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                    else if (arg.Equals("ACC"))
                    {
                        Console.WriteLine("Set Assetto Corsa Competizione mode from command line");
                        this.gameDefinitionList.Text = GameDefinition.acc.friendlyName;
                        setFromCommandLine = true;
                        break;
                    }
                }
            }
            if (!setFromCommandLine)
            {
                String lastDef = UserSettings.GetUserSettings().getString("last_game_definition");
                if (lastDef != null && lastDef.Length > 0)
                {
                    try
                    {
                        GameDefinition gameDefinition = GameDefinition.getGameDefinitionForEnumName(lastDef);
                        if (gameDefinition != null)
                        {
                            Console.WriteLine("Set " + gameDefinition.friendlyName + " mode from previous launch");
                            this.gameDefinitionList.Text = gameDefinition.friendlyName;
                        }
                    }
                    catch (Exception)
                    {
                        //ignore, just don't set the value in the list
                    }
                }
            }
            if (this.gameDefinitionList.Text.Length > 0)
            {
                try
                {
                    CrewChief.gameDefinition = GameDefinition.getGameDefinitionForFriendlyName(this.gameDefinitionList.Text);
                }
                catch (Exception) { }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!MainWindow.shouldSaveTrace
                && this.recordSession.Checked)
            {
                // Message box with y/n to save?
                var dialogResult = MessageBox.Show("A trace was enabled, would you like to save this trace?", "Save trace?", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, (MessageBoxOptions)0x40000 /*MB_TOPMOST*/);

                if (dialogResult == DialogResult.Yes)
                    MainWindow.shouldSaveTrace = true;
            }

            base.OnFormClosing(e);
            MacroManager.stop();
            saveConsoleOutputText();

            consoleUpdateThreadRunning = false;
            consoleUpdateThreadWakeUpEvent.Set();

            controllerRescanThreadRunning = false;
            this.controllerConfiguration.cancelScan();
            controllerRescanThreadWakeUpEvent.Set();

            lock (consoleWriter)
            {
                consoleWriter.Dispose();
            }
        }

        private void SetupNotificationTrayIcon()
        {
            Debug.Assert(notificationTrayIcon == null, "Supposed to be called once");

            notificationTrayIcon = new NotifyIcon();

            // Load the icon.
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            notificationTrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

            notificationTrayIcon.DoubleClick += NotifyIcon_DoubleClick;

            // Setup the context menu.
            var cms = new ContextMenuStrip();

            // Restore item.
            var cmi = cms.Items.Add(Configuration.getUIString("restore_context_menu"));
            cmi.Click += NotifyIcon_DoubleClick;

            // Start/Stop items.
            cms.Items.Add(new ToolStripSeparator());
            contextMenuStartItem = cms.Items.Add(Configuration.getUIString("start_application"), null, this.startApplicationButton_Click);
            contextMenuStopItem = cms.Items.Add(!this.recordSession.Checked ? Configuration.getUIString("stop") : Configuration.getUIString("stop_and_save"), null, this.startApplicationButton_Click);
            cms.Items.Add(new ToolStripSeparator());

            // Form Game context submenu.
            cmi = cms.Items.Add(Configuration.getUIString("game"));
            contextMenuGamesMenu = cmi as ToolStripMenuItem;
            foreach (var game in this.gameDefinitionList.Items)
            {
                var ddi = contextMenuGamesMenu.DropDownItems.Add(game.ToString());
                ddi.Click += (sender, e) =>
                {
                    var gameSelected = sender as ToolStripMenuItem;
                    if (gameSelected == null)
                        return;

                    this.gameDefinitionList.Text = gameSelected.Text;
                };
            }

            contextMenuGamesMenu.DropDownOpening += (sender, e) =>
            {
                var currGameFriendlyName = this.gameDefinitionList.Text;
                foreach (var game in contextMenuGamesMenu.DropDownItems)
                {
                    var tsmi = game as ToolStripMenuItem;
                    tsmi.Checked = tsmi.Text == currGameFriendlyName;
                }
            };

            // Preferences and Close items
            contextMenuPreferencesItem = cms.Items.Add(Configuration.getUIString("properties"), null, this.editPropertiesButtonClicked);
            cms.Items.Add(new ToolStripSeparator());
            cmi = cms.Items.Add(Configuration.getUIString("close_context_menu"));
            cmi.Click += (sender, e) =>
            {
                this.notificationTrayIcon.Visible = false;
                this.Close();
            };

            cms.Opening += (sender, e) =>
            {
                this.contextMenuStartItem.Enabled = !this._IsAppRunning;
                this.contextMenuStopItem.Enabled = this._IsAppRunning;

                this.contextMenuStartItem.Text = string.IsNullOrWhiteSpace(this.gameDefinitionList.Text)
                    ? Configuration.getUIString("start_application")
                    : string.Format(Configuration.getUIString("start_context_menu"), this.gameDefinitionList.Text);

                // Only allow game selection if we're in a Stopped state.
                foreach (var game in this.contextMenuGamesMenu.DropDownItems)
                    (game as ToolStripMenuItem).Enabled = !this._IsAppRunning;
            };


            notificationTrayIcon.ContextMenuStrip = cms;
            notificationTrayIcon.Text = Configuration.getUIString("idling_context_menu");
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.RestoreFromTray();
        }

        private void InitializeUiTexts()
        {
            
            this.startApplicationButton.Text = Configuration.getUIString("start_application");
            this.scanControllers.Text = Configuration.getUIString("scan_for_controllers");
            this.assignButtonToAction.Text = Configuration.getUIString("assign_control");
            this.deleteAssigmentButton.Text = Configuration.getUIString("delete_assignment");
            this.buttonEditCommandMacros.Text = Configuration.getUIString("edit_macro_commands");
            this.label1.Text = Configuration.getUIString("available_controllers");
            this.label2.Text = Configuration.getUIString("available_actions");
            this.propertiesButton.Text = Configuration.getUIString("properties");
            this.helpButton.Text = Configuration.getUIString("help");
            this.aboutButton.Text = Configuration.getUIString("about");            
            this.groupBox1.Text = Configuration.getUIString("voice_recognition_mode");
            voiceRecognitionToolTip.SetToolTip(this.groupBox1, Configuration.getUIString("voice_recognition_mode_help"));             
            this.alwaysOnButton.Text = Configuration.getUIString("always_on");
            voiceRecognitionAlwaysOnToolTip.SetToolTip(this.alwaysOnButton, Configuration.getUIString("voice_recognition_always_on_help"));
            this.toggleButton.Text = Configuration.getUIString("toggle_button");
            voiceRecognitionToggleButtonToolTip.SetToolTip(this.toggleButton, Configuration.getUIString("voice_recognition_toggle_button_help"));
            this.holdButton.Text = Configuration.getUIString("hold_button");
            voiceRecognitionHoldButtonToolTip.SetToolTip(this.holdButton, Configuration.getUIString("voice_recognition_hold_button_help"));
            this.voiceDisableButton.Text = Configuration.getUIString("disabled");
            voiceRecognitionDisabledToolTip.SetToolTip(this.voiceDisableButton, Configuration.getUIString("voice_recognition_disabled_help"));
            this.triggerWordButton.Text = Configuration.getUIString("trigger_word") + " (\"" + UserSettings.GetUserSettings().getString("trigger_word_for_always_on_sre") + "\")";
            voiceRecognitionTriggerWordToolTip.SetToolTip(this.triggerWordButton, Configuration.getUIString("voice_recognition_trigger_word_help"));
            this.button2.Text = Configuration.getUIString("clear_console");
            this.label3.Text = Configuration.getUIString("messages_volume");
            this.label4.Text = Configuration.getUIString("background_volume");
            this.label5.Text = Configuration.getUIString("game");
            this.filenameLabel.Text = "File &name to run";
            this.app_version.Text = Configuration.getUIString("app_version");
            this.forceVersionCheckButton.Text = Configuration.getUIString("check_for_updates");
            this.downloadSoundPackButton.Text = Configuration.getUIString("sound_pack_is_up_to_date");
            this.downloadDriverNamesButton.Text = Configuration.getUIString("driver_names_are_up_to_date");
            this.downloadPersonalisationsButton.Text = Configuration.getUIString("personalisations_are_up_to_date");
            this.personalisationLabel.Text = Configuration.getUIString("personalisation_label");
            this.myNameBoxTooltip.SetToolTip(this.personalisationLabel, Configuration.getUIString("personalisation_tooltip"));
            this.myNameBoxTooltip.SetToolTip(this.personalisationBox, Configuration.getUIString("personalisation_tooltip"));
            this.chiefNameLabel.Text = Configuration.getUIString("chief_name_label");
            this.chiefNameBoxTooltip.SetToolTip(this.chiefNameLabel, Configuration.getUIString("chief_name_tooltip"));            
            this.chiefNameBoxTooltip.SetToolTip(this.chiefNameBox, Configuration.getUIString("chief_name_tooltip"));
            this.spotterNameLabel.Text = Configuration.getUIString("spotter_name_label");
            this.spotterNameBoxTooltip.SetToolTip(this.spotterNameLabel, Configuration.getUIString("spotter_name_tooltip"));
            this.spotterNameBoxTooltip.SetToolTip(this.spotterNameBox, Configuration.getUIString("spotter_name_tooltip"));
            this.donateLink.Text = Configuration.getUIString("donate_link_text");
            this.speechRecognitionDeviceLabel.Text = Configuration.getUIString("speech_recognition_device_label");
            this.messagesAudioDeviceLabel.Text = Configuration.getUIString("messages_audio_device_label");
            this.backgroundAudioDeviceLabel.Text = Configuration.getUIString("background_audio_device_label");
            this.AddRemoveActions.Text = Configuration.getUIString("add_remove_actions");
            
            this.gameDefinitionList.Items.Clear();
            this.gameDefinitionList.Items.AddRange(GameDefinition.getGameDefinitionFriendlyNames());
            if (MainWindow.soundTestMode)
            {
                this.SuspendLayout();
                this.consoleTextBoxBackgroundPanel.Size = new System.Drawing.Size(793, 285);
                this.consoleTextBox.Size = new System.Drawing.Size(793, 285);
                this.ResumeLayout(false);
            }
        }
        public MainWindow()
        {
            lock (MainWindow.instanceLock)
            {
                MainWindow.instance = this;
            }

            this.constructingWindow = true;

            InitializeComponent();
            InitializeUiTexts();
            
            this.SuspendLayout();

            SetupNotificationTrayIcon();

            if (CrewChief.Debugging)
            {
                // Restore last saved trace file name.
                filenameTextbox.Text = UserSettings.GetUserSettings().getString("last_trace_file_name");
                filenameTextbox.TextChanged += MainWindow_TextChanged;
            }

            CheckForIllegalCrossThreadCalls = false;
            consoleTextBox.WordWrap = false;
            consoleWriter = new ControlWriter(consoleTextBox, consoleUpdateThreadWakeUpEvent);
            consoleTextBox.KeyDown += TextBoxConsole_KeyDown;

            Console.SetOut(consoleWriter);

            // if we can't init the UserSettings the app will basically be fucked. So try to nuke the Britton_IT_Ltd directory from
            // orbit (it's the only way to be sure) then restart the app. This shit is comically flakey but what else can we do here?
            if (UserSettings.GetUserSettings().initFailed)
            {
                Console.WriteLine("Unable to upgrade properties from previous version, settings will be reset to default");
                try
                {
                    UserSettings.ForceablyDeleteConfigDirectory();
                    // note we can't load these from the UI settings because loading stuff will be broken at this point
                    doRestart("Failed to load user settings, app must be restarted to try again.", "Failed to load user settings");
                }
                catch (Exception)
                {
                    // oh dear, now we are in a pickle.
                    Console.WriteLine("Unable to remove broken app settings file\n Please exit the app and manually delete folder " + UserSettings.userConfigFolder);
                }
            }

            setSelectedGameType();

            this.app_version.Text = Configuration.getUIString("version") + ": " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("Starting app.  " + this.app_version.Text);
            this.filenameLabel.Visible = CrewChief.Debugging;
            this.filenameTextbox.Visible = CrewChief.Debugging;
            this.playbackInterval.Visible = CrewChief.Debugging;
           
            String[] commandLineArgs = Environment.GetCommandLineArgs();
            if (MainWindow.soundTestMode)
            {
                Console.WriteLine("Sound-test enabled");
                this.buttonSmokeTest.Visible = true;
                this.smokeTestTextBox.Visible = true;
            }
            if (CrewChief.Debugging)
            {
                this.recordSession.Visible = true;
            }
            else
            {
                this.recordSession.Visible = false;
                if (commandLineArgs != null)
                {
                    foreach (String arg in commandLineArgs)
                    {
                        if (arg.Equals("DEBUG"))
                        {
                            Console.WriteLine("Dump-to-file enabled");
                            this.recordSession.Visible = true;
                            this.recordSession.Checked = true;
                            break;
                        }
                        if (arg.Equals("DEBUG_WITH_PLAYBACK"))
                        {
                            Console.WriteLine("Dump-to-file and playback controls enabled");
                            this.recordSession.Visible = true;
                            this.recordSession.Checked = false;
                            this.playbackInterval.Visible = true;
                            this.filenameLabel.Visible = true;
                            this.filenameTextbox.Visible = true;
                            break;
                        }
                    }
                }
            }

            if (!UserSettings.GetUserSettings().getBoolean("enable_console_logging"))
            {
                Console.WriteLine("Console logging has been disabled ('enable_console_logging' property)");
            }
            consoleWriter.enable = UserSettings.GetUserSettings().getBoolean("enable_console_logging");

            if (UserSettings.GetUserSettings().getBoolean("use_naudio"))
            {
                this.messagesAudioDeviceBox.Enabled = true;
                this.messagesAudioDeviceBox.Visible = true;
                this.messagesAudioDeviceLabel.Visible = true;
                this.messagesAudioDeviceBox.Items.AddRange(AudioPlayer.playbackDevices.Keys.ToArray());
                // only register the value changed listener after loading the available values
                String messagesPlaybackGuid = UserSettings.GetUserSettings().getString("NAUDIO_DEVICE_GUID_MESSAGES");
                Boolean foundMessagesDeviceGuid = false;
                if (messagesPlaybackGuid != null)
                {
                    foreach (KeyValuePair<string, Tuple<string, int>> entry in AudioPlayer.playbackDevices)
                    {
                        if (messagesPlaybackGuid.Equals(entry.Value.Item1))
                        {
                            this.messagesAudioDeviceBox.Text = entry.Key;
                            AudioPlayer.naudioMessagesPlaybackDeviceId = entry.Value.Item2;
                            foundMessagesDeviceGuid = true;
                            break;
                        }
                    }
                }
                if (!foundMessagesDeviceGuid && AudioPlayer.playbackDevices.Count > 0)
                {
                    this.messagesAudioDeviceBox.Text = AudioPlayer.playbackDevices.First().Key;
                }
                this.messagesAudioDeviceBox.SelectedValueChanged += new System.EventHandler(this.messagesAudioDeviceSelected);

                this.backgroundAudioDeviceBox.Enabled = true;
                this.backgroundAudioDeviceBox.Visible = true;
                this.backgroundAudioDeviceLabel.Visible = true;
                this.backgroundAudioDeviceBox.Items.AddRange(AudioPlayer.playbackDevices.Keys.ToArray());
                String backgroundPlaybackGuid = UserSettings.GetUserSettings().getString("NAUDIO_DEVICE_GUID_BACKGROUND");
                // only register the value changed listener after loading the available values
                Boolean foundBackgroundDeviceGuid = false;
                if (backgroundPlaybackGuid != null)
                {
                    foreach (KeyValuePair<string, Tuple<string, int>> entry in AudioPlayer.playbackDevices)
                    {
                        if (backgroundPlaybackGuid.Equals(entry.Value.Item1))
                        {
                            this.backgroundAudioDeviceBox.Text = entry.Key;
                            AudioPlayer.naudioBackgroundPlaybackDeviceId = entry.Value.Item2;
                            foundBackgroundDeviceGuid = true;
                            break;
                        }
                    }
                }
                if (!foundBackgroundDeviceGuid && AudioPlayer.playbackDevices.Count > 0)
                {
                    this.backgroundAudioDeviceBox.Text = AudioPlayer.playbackDevices.First().Key;
                }
                this.messagesAudioDeviceBox.SelectedValueChanged += new System.EventHandler(this.messagesAudioDeviceSelected);
                this.backgroundAudioDeviceBox.SelectedValueChanged += new System.EventHandler(this.backgroundAudioDeviceSelected);
            }

            if (UserSettings.GetUserSettings().getBoolean("use_naudio_for_speech_recognition"))
            {
                this.speechRecognitionDeviceBox.Enabled = true;
                this.speechRecognitionDeviceBox.Visible = true;
                this.speechRecognitionDeviceLabel.Visible = true;
                this.speechRecognitionDeviceBox.Items.AddRange(SpeechRecogniser.speechRecognitionDevices.Keys.ToArray());
                // only register the value changed listener after loading the available values
                String speechRecognitionDeviceGuid = UserSettings.GetUserSettings().getString("NAUDIO_RECORDING_DEVICE_GUID");
                Boolean foundspeechRecognitionDeviceGuid = false;
                if (speechRecognitionDeviceGuid != null)
                {
                    foreach (KeyValuePair<string, Tuple<string, int>> entry in SpeechRecogniser.speechRecognitionDevices)
                    {
                        if (speechRecognitionDeviceGuid.Equals(entry.Value.Item1))
                        {
                            this.speechRecognitionDeviceBox.Text = entry.Key;
                            SpeechRecogniser.initialSpeechInputDeviceIndex = entry.Value.Item2;
                            foundspeechRecognitionDeviceGuid = true;
                            break;
                        }
                    }
                }
                if (!foundspeechRecognitionDeviceGuid && SpeechRecogniser.speechRecognitionDevices.Count > 0)
                {
                    this.speechRecognitionDeviceBox.Text = SpeechRecogniser.speechRecognitionDevices.First().Key;
                }
                this.speechRecognitionDeviceBox.SelectedValueChanged += new System.EventHandler(this.speechRecognitionDeviceSelected);
            }

            // NOTE: if you ever move this construction, please make sure controller rescan thread is not running yet.
            Debug.Assert(!this.controllerRescanThreadRunning);
            controllerConfiguration = new ControllerConfiguration(this);

            // NOTE: important to keep this instantiation here to avoid race between DirectInput and WMP initialization.
            crewChief = new CrewChief(controllerConfiguration);

            controllerConfiguration.initialize();
            GlobalResources.controllerConfiguration = controllerConfiguration;

            this.personalisationBox.Items.AddRange(this.crewChief.audioPlayer.personalisationsArray);
            this.chiefNameBox.Items.AddRange(AudioPlayer.availableChiefVoices.ToArray());
            this.spotterNameBox.Items.AddRange(NoisyCartesianCoordinateSpotter.availableSpotters.ToArray());
            if (crewChief.audioPlayer.selectedPersonalisation == null || crewChief.audioPlayer.selectedPersonalisation.Length == 0 ||
                crewChief.audioPlayer.selectedPersonalisation.Equals(AudioPlayer.NO_PERSONALISATION_SELECTED) ||
                !this.crewChief.audioPlayer.personalisationsArray.Contains(crewChief.audioPlayer.selectedPersonalisation))
            {
                this.personalisationBox.Text = AudioPlayer.NO_PERSONALISATION_SELECTED;
            }
            else
            {
                this.personalisationBox.Text = crewChief.audioPlayer.selectedPersonalisation;
            }

            String savedChief = UserSettings.GetUserSettings().getString("chief_name");
            if (!String.IsNullOrWhiteSpace(savedChief) && AudioPlayer.availableChiefVoices.Contains(savedChief))
            {
                this.chiefNameBox.Text = savedChief;
            }
            else
            {
                this.chiefNameBox.Text = AudioPlayer.defaultChiefId;
            }

            String savedSpotter = UserSettings.GetUserSettings().getString("spotter_name");
            if (!String.IsNullOrWhiteSpace(savedSpotter) && NoisyCartesianCoordinateSpotter.availableSpotters.Contains(savedSpotter))
            {
                this.spotterNameBox.Text = savedSpotter;
            }
            else
            {
                this.spotterNameBox.Text = NoisyCartesianCoordinateSpotter.defaultSpotterId;
            }
            // only register the value changed listener after loading the saved values
            this.personalisationBox.SelectedValueChanged += new System.EventHandler(this.personalisationSelected);
            this.chiefNameBox.SelectedValueChanged += new System.EventHandler(this.chiefNameSelected);
            this.spotterNameBox.SelectedValueChanged += new System.EventHandler(this.spotterNameSelected);

            float messagesVolume = UserSettings.GetUserSettings().getFloat("messages_volume");
            float backgroundVolume = UserSettings.GetUserSettings().getFloat("background_volume");
            updateMessagesVolume(messagesVolume, false, true);
            backgroundVolumeSlider.Value = (int)(backgroundVolume * 100f);

            Console.WriteLine("Loading controller settings");
            String customDeviceGuid = UserSettings.GetUserSettings().getString("custom_device_guid");
            if (customDeviceGuid != null && customDeviceGuid.Length > 0)
            {
                try
                {
                    Guid guid;
                    if (Guid.TryParse(customDeviceGuid, out guid))
                    {
                        controllerConfiguration.addCustomController(guid);
                    }
                    else
                    {
                        Console.WriteLine("Failed to add custom device, unable to process GUID");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to add custom device, message: " + e.Message);
                }
            }
            Console.WriteLine("Load controller settings complete");
            voiceOption = getVoiceOptionEnum(UserSettings.GetUserSettings().getString("VOICE_OPTION"));
            if (voiceOption == VoiceOptionEnum.DISABLED)
            {
                this.voiceDisableButton.Checked = true;
            }
            else if (voiceOption == VoiceOptionEnum.ALWAYS_ON)
            {
                this.alwaysOnButton.Checked = true;
            }
            else if (voiceOption == VoiceOptionEnum.HOLD)
            {
                this.holdButton.Checked = true;
            }
            else if (voiceOption == VoiceOptionEnum.TOGGLE)
            {
                this.toggleButton.Checked = true;
            }
            else if (voiceOption == VoiceOptionEnum.TRIGGER_WORD)
            {
                this.triggerWordButton.Checked = true;
            }

            // don't allow trigger word or always on if using nAudio
            if (UserSettings.GetUserSettings().getBoolean("use_naudio_for_speech_recognition"))
            {
                if (voiceOption == VoiceOptionEnum.TOGGLE || voiceOption == VoiceOptionEnum.TRIGGER_WORD)
                {
                    Console.WriteLine("Voice option " + voiceOption.ToString() + " not compatible with nAudio input");
                    this.voiceDisableButton.Checked = true;
                }
                this.triggerWordButton.Enabled = false;
                this.toggleButton.Enabled = false;
            }
            this.assignButtonToAction.Enabled = false;
            this.deleteAssigmentButton.Enabled = false;

            this.ResumeLayout();

            this.Resize += MainWindow_Resize;
            this.KeyPreview = true;
            this.KeyDown += MainWindow_KeyDown;

            this.constructingWindow = false;
        }

        private void consoleUpdateThreadWorker()
        {
            while (consoleUpdateThreadRunning)
            {
                consoleUpdateThreadWakeUpEvent.WaitOne();
                if (!consoleUpdateThreadRunning)
                {
                    Debug.WriteLine("Exiting console update thread.");
                    return;
                }

                if (!consoleWriter.enable)
                {
                    Debug.WriteLine("Exiting console update thread, console output disabled.");
                    return;
                }

                Debug.Assert(consoleTextBox.InvokeRequired);

                string messages = null;
                // Pick up the new messages.
                lock (ControlWriter.controlWriterLock)
                {
                    messages = consoleWriter.newMessagesBuilder.ToString();
                    consoleWriter.newMessagesBuilder.Clear();
                }

                if (MainWindow.instance != null
                    && consoleTextBox != null
                    && !consoleTextBox.IsDisposed
                    && !string.IsNullOrWhiteSpace(messages)
                    && consoleUpdateThreadRunning)
                {
                    try
                    {
                        consoleTextBox.Invoke((MethodInvoker)delegate
                        {
                            if (MainWindow.instance != null
                                && consoleTextBox != null
                                && !consoleTextBox.IsDisposed)
                            {
                                try
                                {
                                    consoleTextBox.AppendText(messages);
                                    if (MainWindow.autoScrollConsole)
                                    {
                                        consoleTextBox.ScrollToCaret();
                                    }
                                }
                                catch (Exception)
                                {
                                    // swallow - nothing to log it to
                                }
                            }
                        });
                    }
                    catch (Exception)
                    {
                        // Possible shutdown.
                    }
                }
            }
        }

        private void controllerRescanThreadWorker()
        {
            while (controllerRescanThreadRunning && !disableControllerReacquire)
            {
                controllerRescanThreadWakeUpEvent.WaitOne();
                if (!controllerRescanThreadRunning)
                {
                    Debug.WriteLine("Exiting controller rescan thread.");
                    return;
                }

                if (MainWindow.instance != null)
                {
                    try
                    {
                        lock (this.controllerWriteLock)
                        {
                            this.refreshControllerList();
                        }
                    }
                    catch (Exception)
                    {
                        // Possible shutdown.
                    }
                }
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.F1)
            {
                this.helpButtonClicked(sender, e);
                e.Handled = true;
            }
        }

        private void MainWindow_TextChanged(object sender, EventArgs e)
        {
            UserSettings.GetUserSettings().setProperty("last_trace_file_name", filenameTextbox.Text);

            // It's awful to save on each character entered, but alternatives are far hairier, so let it be (debug only stuff anyway).
            UserSettings.GetUserSettings().saveUserSettings();
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.HideToTray();
        }

        private void TextBoxConsole_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                consoleTextBox.SelectAll();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                consoleTextBox.DeselectAll();
            }
        }

        private void listenForChannelOpen()
        {
            Boolean channelOpen = false;
            if (crewChief.speechRecogniser != null && crewChief.speechRecogniser.initialised && voiceOption == VoiceOptionEnum.HOLD)
            {
                Console.WriteLine("Running speech recognition in 'hold button' mode");
                crewChief.speechRecogniser.voiceOptionEnum = VoiceOptionEnum.HOLD;
                while (runListenForChannelOpenThread)
                {
                    Thread.Sleep(this.holdButtonPollFrequency);
                    if (!channelOpen && controllerConfiguration.isChannelOpen())
                    {
                        channelOpen = true;

                        // for pace notes recording, start SRE *after* the beep. For voice commands, start SRE *before* the beep
                        if (DriverTrainingService.isRecordingPaceNotes)
                        {
                            if (CrewChief.distanceRoundTrack > 0)
                            {
                                crewChief.audioPlayer.playStartListeningBeep();
                                Console.WriteLine("Recording pace note...");
                                DriverTrainingService.startRecordingMessage((int)CrewChief.distanceRoundTrack);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Listening for voice command...");
                            crewChief.speechRecogniser.recognizeAsync();
                            crewChief.audioPlayer.playStartListeningBeep();
                        }
                        if (rejectMessagesWhenTalking)
                        {
                            // Wait for button press bleep.  Ideally, we need to know when intro beep is done playing to mute.
                            Thread.Sleep(200);
                            muteVolumes();
                        }
                    }
                    else if (channelOpen && !controllerConfiguration.isChannelOpen())
                    {
                        if (rejectMessagesWhenTalking)
                        {
                            // Drop any outstanding messages queued while user was talking, this should prevent weird half phrases.
                            crewChief.audioPlayer.purgeQueues();
                            // unmute
                            unmuteVolumes();

                            PlaybackModerator.rejectMessagesWhenTalking = false;  // Hack to let this through moderator.
                            crewChief.audioPlayer.playChiefEndSpeakingBeep();
                            PlaybackModerator.rejectMessagesWhenTalking = true;
                        }

                        if (DriverTrainingService.isRecordingPaceNotes)
                        {
                            Console.WriteLine("Saving recorded pace note");
                            DriverTrainingService.stopRecordingMessage();
                        }
                        else
                        {
                            Console.WriteLine("Invoking speech recognition...");
                            SpeechRecogniser.recognitionStartedTime = DateTime.Now;
                            crewChief.speechRecogniser.recognizeAsyncCancel();
                            if (youWotThread == null
                                || !youWotThread.IsAlive)
                            {
                                ThreadManager.UnregisterTemporaryThread(youWotThread);
                                youWotThread = new Thread(() =>
                                {
                                    Utilities.InterruptedSleep(totalWaitMillis: this.sreWaitTime, waitWindowMillis: 50, keepWaitingPredicate: () => crewChief.running);
                                    if (!channelOpen && !SpeechRecogniser.gotRecognitionResult)
                                    {
                                        crewChief.youWot(false);
                                    }
                                });
                                youWotThread.Name = "MainWindow.youWotThread";
                                ThreadManager.RegisterTemporaryThread(youWotThread);
                                youWotThread.Start();
                            }
                            else
                            {
                                Console.WriteLine("Skipping new instance of youWot thread because previous is still running.");
                            }
                        }
                        channelOpen = false;
                    }
                }
            }
        }

        private void listenForButtons()
        {
            DateTime lastButtoncheck = DateTime.UtcNow;
            if (crewChief.speechRecogniser.initialised && voiceOption == VoiceOptionEnum.TOGGLE)
            {
                Console.WriteLine("Running speech recognition in 'toggle button' mode");
            }
            while (runListenForButtonPressesThread)
            {
                Thread.Sleep(100);
                DateTime now = DateTime.UtcNow;
                controllerConfiguration.pollForButtonClicks(voiceOption == VoiceOptionEnum.TOGGLE);
                int nextPollWait = 0;
                if (now > lastButtoncheck.Add(buttonCheckInterval))
                {
                    lastButtoncheck = now;
                    if (controllerConfiguration.hasOutstandingClick(ControllerConfiguration.VOLUME_UP))
                    {
                        if (currentMessageVolume == -1)
                        {
                            Console.WriteLine("Initial volume not set, ignoring");
                        }
                        else if (currentMessageVolume >= 1)
                        {
                            Console.WriteLine("Volume at max");
                        }
                        else
                        {
                            Console.WriteLine("Increasing volume");
                            updateMessagesVolume(currentMessageVolume + 0.05f, true, true);
                        }
                        nextPollWait = 200;
                    }
                    else if (controllerConfiguration.hasOutstandingClick(ControllerConfiguration.VOLUME_DOWN))
                    {
                        if (currentMessageVolume == -1)
                        {
                            Console.WriteLine("Initial volume not set, ignoring");
                        }
                        else if (currentMessageVolume <= 0)
                        {
                            Console.WriteLine("Volume at min");
                        }
                        else
                        {
                            Console.WriteLine("Decreasing volume");
                            updateMessagesVolume(currentMessageVolume - 0.05f, true, true);
                        }
                        nextPollWait = 200;
                    }
                    else if (crewChief.speechRecogniser != null && crewChief.speechRecogniser.initialised && voiceOption == VoiceOptionEnum.TOGGLE &&
                             controllerConfiguration.hasOutstandingClick(ControllerConfiguration.CHANNEL_OPEN_FUNCTION))
                    {
                        // JB: no idea why we're setting this enum option here. Will leave it in just in case
                        crewChief.speechRecogniser.voiceOptionEnum = VoiceOptionEnum.TOGGLE;
                        if (SpeechRecogniser.waitingForSpeech)
                        {
                            Console.WriteLine("Cancelling...");
                            SpeechRecogniser.waitingForSpeech = false;
                            SpeechRecogniser.recognitionStartedTime = DateTime.Now;
                            crewChief.speechRecogniser.recognizeAsyncCancel();
                            nextPollWait = 1000;
                        }
                        else
                        {
                            Console.WriteLine("Listening...");
                            crewChief.speechRecogniser.recognizeAsync();
                            crewChief.audioPlayer.playStartListeningBeep();
                        }
                    }
                    else if (controllerConfiguration.hasOutstandingClick(ControllerConfiguration.TOGGLE_SPOTTER_FUNCTION))
                    {
                        Console.WriteLine("Toggling spotter mode");
                        crewChief.toggleSpotterMode();
                        nextPollWait = 1000;
                    }
                    else if (controllerConfiguration.hasOutstandingClick(ControllerConfiguration.TOGGLE_MUTE))
                    {
                        if (!isMuted)
                        {
                            muteVolumes();
                        }
                        else
                        {
                            unmuteVolumes();
                        }
                        isMuted = !isMuted;
                        nextPollWait = 200;
                    }
                    else if (controllerConfiguration.hasOutstandingClick())
                    {
                        //Console.WriteLine("Toggling keep quiet mode");
                        //crewChief.toggleKeepQuietMode();
                        nextPollWait = 1000;
                    }
                }
                Thread.Sleep(nextPollWait);
            }
        }

        private void unmuteVolumes()
        {
            crewChief.audioPlayer.muteBackgroundPlayer(false);
            updateMessagesVolume(messageVolumeToRestore, saveChange: false, updateSlider: false, forceUpdate: true);
            messagesVolumeSlider.Enabled = true;
            backgroundVolumeSlider.Enabled = true;
        }

        private void muteVolumes()
        {
            // save the volume level to restore later
            messageVolumeToRestore = currentMessageVolume;
            updateMessagesVolume(0, saveChange: false, updateSlider: false, forceUpdate: true);
            crewChief.audioPlayer.muteBackgroundPlayer(true);
            messagesVolumeSlider.Enabled = false;
            backgroundVolumeSlider.Enabled = false;
        }

        private void startApplicationButton_Click(object sender, EventArgs e)
        {
            MainWindow.shouldSaveTrace = IsAppRunning;

            doStartAppStuff();

            if (IsAppRunning)
            {
                ThreadManager.DoWatchStartup(crewChief);
            }
            else
            {
                ThreadManager.DoWatchStop(crewChief);
            }
        }

        private void uiSyncAppStart()
        {
            this.runListenForButtonPressesThread = controllerConfiguration.listenForButtons(voiceOption == VoiceOptionEnum.TOGGLE);
            this.assignButtonToAction.Enabled = false;
            this.deleteAssigmentButton.Enabled = false;
            this.groupBox1.Enabled = false;
            this.propertiesButton.Enabled = false;
            this.personalisationBox.Enabled = false;
            this.chiefNameBox.Enabled = false;
            this.spotterNameBox.Enabled = false;
            this.recordSession.Enabled = false;
            this.gameDefinitionList.Enabled = false;
            this.contextMenuPreferencesItem.Enabled = false;
            this.notificationTrayIcon.Text = string.Format(Configuration.getUIString("running_context_menu"), this.gameDefinitionList.Text);
            this.scanControllers.Enabled = false;
        }

        public void uiSyncAppStop()
        {
            this.deleteAssigmentButton.Enabled = this.buttonActionSelect.SelectedIndex > -1 &&
                this.controllerConfiguration.buttonAssignments[this.buttonActionSelect.SelectedIndex].controller != null;

            this.assignButtonToAction.Enabled = this.buttonActionSelect.SelectedIndex > -1 && this.controllersList.SelectedIndex > -1 && ((MainWindow.ControllerUiEntry)this.controllersList.Items[this.controllersList.SelectedIndex]).isConnected;
            this.propertiesButton.Enabled = true;
            this.groupBox1.Enabled = true;
            this.personalisationBox.Enabled = true;
            this.chiefNameBox.Enabled = true;
            this.spotterNameBox.Enabled = true;
            this.recordSession.Enabled = true;
            this.gameDefinitionList.Enabled = true;
            this.contextMenuPreferencesItem.Enabled = true;
            this.notificationTrayIcon.Text = Configuration.getUIString("idling_context_menu");
            this.scanControllers.Enabled = true;
        }

        private void doStartAppStuff()
        {
            IsAppRunning = !IsAppRunning;
            if (_IsAppRunning)
            {
                startApplicationButton.Enabled = false;
                uiSyncAppStart();
                
                // Don't disable auto scroll in Debug builds.
#if !DEBUG
                Console.WriteLine("Pausing console scrolling");
                MainWindow.autoScrollConsole = false;
#endif
                GameDefinition gameDefinition = GameDefinition.getGameDefinitionForFriendlyName(gameDefinitionList.Text);
                if (gameDefinition != null)
                {
                    crewChief.setGameDefinition(gameDefinition);
                    MacroManager.initialise(crewChief.audioPlayer, crewChief.speechRecogniser);
                    CarData.loadCarClassData();
                    TrackData.loadTrackLandmarksData();
                    ThreadStart crewChiefWork = runApp;
                    Thread crewChiefThread = new Thread(crewChiefWork);
                    crewChiefThread.Name = "MainWindow.runApp";
                    ThreadManager.RegisterRootThread(crewChiefThread);

                    // this call is not part of the standard AutoUpdater API - I added a 'stopped' flag to prevent the auto updater timer
                    // or other Threads firing when the game is running. It's not needed 99% of the time, it just stops that edge case where
                    // the AutoUpdater triggers and steals focus while the player is racing
                    AutoUpdater.Stop();

                    crewChief.onRestart();
                    crewChiefThread.Start();
                    runListenForChannelOpenThread = controllerConfiguration.listenForChannelOpen()
                        && voiceOption == VoiceOptionEnum.HOLD && crewChief.speechRecogniser != null && crewChief.speechRecogniser.initialised;
                    if (runListenForChannelOpenThread && voiceOption == VoiceOptionEnum.HOLD && crewChief.speechRecogniser != null && crewChief.speechRecogniser.initialised)
                    {
                        Console.WriteLine("Listening on default audio input device");
                        ThreadStart channelOpenButtonListenerWork = listenForChannelOpen;
                        Thread channelOpenButtonListenerThread = new Thread(channelOpenButtonListenerWork);

                        channelOpenButtonListenerThread.Name = "MainWindow.listenForChannelOpen";
                        ThreadManager.RegisterRootThread(channelOpenButtonListenerThread);

                        channelOpenButtonListenerThread.Start();
                    }
                    else if ((voiceOption == VoiceOptionEnum.ALWAYS_ON || voiceOption == VoiceOptionEnum.TRIGGER_WORD) && 
                        crewChief.speechRecogniser != null && crewChief.speechRecogniser.initialised)
                    {
                        Console.WriteLine("Running speech recognition in 'always on' mode");
                        crewChief.speechRecogniser.voiceOptionEnum = voiceOption;
                        crewChief.speechRecogniser.startContinuousListening();
                    }
                    if (runListenForButtonPressesThread)
                    {
                        Console.WriteLine("Listening for buttons");
                        ThreadStart buttonPressesListenerWork = listenForButtons;
                        Thread buttonPressesListenerThread = new Thread(buttonPressesListenerWork);

                        buttonPressesListenerThread.Name = "MainWindow.listenForButtons";
                        ThreadManager.RegisterRootThread(buttonPressesListenerThread);

                        buttonPressesListenerThread.Start();
                    }
                    
                }
                else
                {
                    MessageBox.Show(Configuration.getUIString("please_choose_a_game_option"), Configuration.getUIString("no_game_selected"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    IsAppRunning = false;
                    return;
                }
            }
            else
            {
                startApplicationButton.Enabled = false;
                Console.WriteLine("Resuming console scrolling");
                MainWindow.autoScrollConsole = true;
                MacroManager.stop();
                if ((voiceOption == VoiceOptionEnum.ALWAYS_ON || voiceOption == VoiceOptionEnum.TOGGLE) && crewChief.speechRecogniser != null && crewChief.speechRecogniser.initialised)
                {
                    Console.WriteLine("Stopping listening...");
                    try
                    {
                        SpeechRecogniser.waitingForSpeech = false;
                        crewChief.speechRecogniser.recognizeAsyncCancel();
                        crewChief.speechRecogniser.stopTriggerRecogniser();
                    }
                    catch (Exception) { }
                }
                stopApp();
                Console.WriteLine("Application stopped");
                DriverTrainingService.completeRecordingPaceNotes();
                DriverTrainingService.stopPlayingPaceNotes();
            }
        }


        // called from the close callback on the main form
        private void stopApp(object sender, FormClosedEventArgs e)
        {
            lock (MainWindow.instanceLock)
            {
                MainWindow.instance = null;
                formClosed = true;
            }

            // Shutdown long running threads:

            // SoundCache spawns a Thread to lazy-load the sound data. Cancel this:
            SoundCache.cancelLazyLoading = true;
            
            // Make sure we quit button assignment listener.
            controllerConfiguration.listenForAssignment = false;

            stopApp();
        }

        private void runApp()
        {
            String filenameToRun = null;
            Boolean record = false;
            if (!String.IsNullOrWhiteSpace(filenameTextbox.Text))
            {
                filenameToRun = filenameTextbox.Text;
                if (this.playbackInterval.Text.Length > 0)
                {
                    CrewChief.playbackIntervalMilliseconds = int.Parse(playbackInterval.Text);
                }
            }
            if (recordSession.Checked)
            {
                record = true;
            }
            if (!crewChief.Run(filenameToRun, record))
            {
                this.deleteAssigmentButton.Enabled = this.buttonActionSelect.SelectedIndex > -1 &&
                    this.controllerConfiguration.buttonAssignments[this.buttonActionSelect.SelectedIndex].controller != null;
                this.assignButtonToAction.Enabled = this.buttonActionSelect.SelectedIndex > -1 && this.controllersList.SelectedIndex > -1 && ((MainWindow.ControllerUiEntry)this.controllersList.Items[this.controllersList.SelectedIndex]).isConnected;
                stopApp();
                this.propertiesButton.Enabled = true;
                this.personalisationBox.Enabled = true;
                IsAppRunning = false;
            }
        }

        private void stopApp()
        {
            if (isMuted)
            {
                unmuteVolumes();
            }
            runListenForChannelOpenThread = false;
            runListenForButtonPressesThread = false;
            crewChief.stop();
        }

        private void playbackIntervalChanged(object sender, EventArgs e)
        {
            if (this.playbackInterval.Text.Length > 0)
            {
                try
                {
                    CrewChief.playbackIntervalMilliseconds = int.Parse(playbackInterval.Text);
                }
                catch (Exception)
                {
                    // swallow - not much we can do here
                }
            }
            else
            {
                CrewChief.playbackIntervalMilliseconds = 0;
            }
        }

        private void buttonActionSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.deleteAssigmentButton.Enabled = this.buttonActionSelect.SelectedIndex > -1 && !crewChief.running;
            this.assignButtonToAction.Enabled = this.buttonActionSelect.SelectedIndex > -1 && this.controllersList.SelectedIndex > -1 && !crewChief.running && ((MainWindow.ControllerUiEntry)this.controllersList.Items[this.controllersList.SelectedIndex]).isConnected;
        }

        private void controllersList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.deleteAssigmentButton.Enabled = this.buttonActionSelect.SelectedIndex > -1 && !crewChief.running;
            this.assignButtonToAction.Enabled = this.buttonActionSelect.SelectedIndex > -1 && this.controllersList.SelectedIndex > -1 && !crewChief.running && ((MainWindow.ControllerUiEntry)this.controllersList.Items[this.controllersList.SelectedIndex]).isConnected;
        }

        public void updateControllersUi()
        {
            Debug.Assert(controllerConfiguration.knownControllers != null);
            Debug.Assert(!this.InvokeRequired);

            this.controllersList.Items.Clear();

            // First, add active controllers to the list:
            // NOTE: it is important that connected controllers go first, because their index in the UI list is used to access controllerConfiguration.controllers list.
            foreach (ControllerConfiguration.ControllerData configData in controllerConfiguration.controllers)
            {
                this.controllersList.Items.Add(new MainWindow.ControllerUiEntry(configData.deviceName, isConnected: true));
            }

            // Now, add grayed out (inactive) controllers
            foreach (ControllerConfiguration.ControllerData configData in controllerConfiguration.knownControllers)
            {
                if (controllerConfiguration.controllers.Exists(c => c.guid == configData.guid))
                {
                    continue;
                }
                this.controllersList.Items.Add(new MainWindow.ControllerUiEntry(configData.deviceName, isConnected: false));
            }
        }

        private void updateActions()
        {
            Debug.Assert(!this.InvokeRequired);
            this.buttonActionSelect.Items.Clear();
            foreach (ControllerConfiguration.ButtonAssignment assignment in controllerConfiguration.buttonAssignments)
            {
                this.buttonActionSelect.Items.Add(assignment.getInfo());
            }
        }

        private void assignButtonToActionClick(object sender, EventArgs e)
        {
            if (!isAssigningButton)
            {
                if (this.controllersList.SelectedIndex >= 0 && this.buttonActionSelect.SelectedIndex >= 0)
                {
                    isAssigningButton = true;
                    this.assignButtonToAction.Text = Configuration.getUIString("waiting_for_button_click_to_cancel");
                    ThreadStart assignButtonWork = assignButton;
                    ThreadManager.UnregisterTemporaryThread(assignButtonThread);
                    assignButtonThread = new Thread(assignButtonWork);
                    assignButtonThread.Name = "MainWindow.assignButtonThread";
                    ThreadManager.RegisterTemporaryThread(assignButtonThread);
                    assignButtonThread.Start();
                }
            }
            else
            {
                isAssigningButton = false;
                controllerConfiguration.listenForAssignment = false;
                this.assignButtonToAction.Text = Configuration.getUIString("assign");
            }
        }

        private bool initialiseSpeechEngine()
        {
            try
            {
                if (crewChief.speechRecogniser != null && !crewChief.speechRecogniser.initialised)
                {
                    crewChief.speechRecogniser.initialiseSpeechEngine();
                    Console.WriteLine("Attempted to initialise speech engine - success = " + crewChief.speechRecogniser.initialised);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to create speech engine, error message: " + e.Message);
                runListenForChannelOpenThread = false;
            }
            //make sure we disable everything that might have been enabled in case speech engine fails
            if (!crewChief.speechRecogniser.initialised)
            {
                
                voiceDisableButton.Checked = true;
                runListenForChannelOpenThread = false;
                runListenForButtonPressesThread = controllerConfiguration.listenForButtons(false);
                voiceOption = VoiceOptionEnum.DISABLED;

                // Turns out saving prefs takes 5% of main thread time on startup, so don't do it
                // as we just read this from prefs.
                if (!this.constructingWindow)
                {
                    UserSettings.GetUserSettings().setProperty("VOICE_OPTION", getVoiceOptionString());
                    UserSettings.GetUserSettings().saveUserSettings();
                }
            }
            return crewChief.speechRecogniser.initialised;
        }

        private void assignButton()
        {
            lock (this.controllerWriteLock)
            {
                if (controllerConfiguration.assignButton(this, this.controllersList.SelectedIndex, this.buttonActionSelect.SelectedIndex))
                {
                    isAssigningButton = false;
                    controllerConfiguration.saveSettings();
                    runListenForChannelOpenThread = controllerConfiguration.listenForChannelOpen() && voiceOption != VoiceOptionEnum.DISABLED;
                    if (runListenForChannelOpenThread)
                    {
                        if (initialiseSpeechEngine())
                        {
                            runListenForButtonPressesThread = controllerConfiguration.listenForButtons(voiceOption == VoiceOptionEnum.TOGGLE);
                        }
                    }
                }
                else
                {
                    isAssigningButton = false;
                }
            }

            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (MainWindow.instance != null)
                    {
                        this.updateActions();
                        this.assignButtonToAction.Text = Configuration.getUIString("assign");
                    }
                });
            }
            catch (Exception)
            {
                // Shutdown.
            }
        }

        private void deleteAssignmentButtonClicked(object sender, EventArgs e)
        {
            if (this.buttonActionSelect.SelectedIndex >= 0)
            {
                this.controllerConfiguration.buttonAssignments[this.buttonActionSelect.SelectedIndex].unassign();
                updateActions();
                runListenForChannelOpenThread = controllerConfiguration.listenForChannelOpen();
                runListenForButtonPressesThread = controllerConfiguration.listenForButtons(voiceOption == VoiceOptionEnum.TOGGLE);
            }
            controllerConfiguration.saveSettings();
        }
        
        private void editPropertiesButtonClicked(object sender, EventArgs e)
        {
            // If minized to tray, hide tray icon while properties dialog is shown,
            // and it again when dialog is gone.  The goal is to prevent weird scenarios while
            // option dialog is visible.
            var minimizedToTray = this.notificationTrayIcon.Visible;
            if (minimizedToTray)
                this.notificationTrayIcon.Visible = false;

            try
            {
                var form = new PropertiesForm(this);
                form.ShowDialog(this);
            }
            finally
            {
                if (minimizedToTray)
                    this.notificationTrayIcon.Visible = true;
            }
        }

        private void helpButtonClicked(object sender, EventArgs e)
        {
            var form = new ShowHelp(this);
            form.ShowDialog(this);
        }

        private void aboutButtonClicked(object sender, EventArgs e)
        {
            var form = new ShowAbout(this);
            form.ShowDialog(this);
        }

        private void forceVersionCheckButtonClicked(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree("Software\\Britton IT Ltd");
            }
            catch
            {
            }
            doRestart(Configuration.getUIString("the_application_must_be_restarted_to_check_for_updates"), Configuration.getUIString("check_for_updates"), true);
        }

        private void saveConsoleOutputText()
        {
            try
            {
                if (consoleTextBox.Text.Length > 0) 
                {
                    String filename = "console_" + DateTime.Now.ToString("yyyy_MM_dd-HH-mm-ss") + ".txt";
                    String path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "debugLogs");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    foreach (var fi in new DirectoryInfo(path).GetFiles().Where(f => f.Extension == ".txt").OrderByDescending(x => x.LastWriteTime).Skip(25))
                    {
                        fi.Delete();
                    }                        
                    path = System.IO.Path.Combine(path, filename);
                    File.WriteAllText(path, consoleTextBox.Text);
                    Console.WriteLine("Console output saved to " + path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to save console output, message = " + ex.Message);
            }
        }

        private void reacquireControllerList()
        {
            Debug.Assert(!this.InvokeRequired);
            this.controllerConfiguration.reacquireControllers();

            if (MainWindow.instance != null)
            {
                if (this.gameDefinitionList.Text.Equals(GameDefinition.pCarsNetwork.friendlyName) || this.gameDefinitionList.Text.Equals(GameDefinition.pCars2Network.friendlyName))
                {
                    controllerConfiguration.addNetworkControllerToList();
                }

                this.updateControllersUi();

                runListenForChannelOpenThread = controllerConfiguration.listenForChannelOpen()
                    && voiceOption == VoiceOptionEnum.HOLD && crewChief.speechRecogniser != null && crewChief.speechRecogniser.initialised;

                updateActions();
            }
        }

        private void refreshControllerList()
        {
            Debug.Assert(this.InvokeRequired);
            this.controllerConfiguration.scanControllers();

            try
            {
                // VL: I can't come up with a deadlock scenario, but if this dealocks we could move it out of this.controllerWriteLock.
                this.Invoke((MethodInvoker)delegate
                {
                    if (MainWindow.instance != null)
                    {
                        if (this.gameDefinitionList.Text.Equals(GameDefinition.pCarsNetwork.friendlyName) || this.gameDefinitionList.Text.Equals(GameDefinition.pCars2Network.friendlyName))
                        {
                            controllerConfiguration.addNetworkControllerToList();
                        }

                        this.updateControllersUi();

                        runListenForChannelOpenThread = controllerConfiguration.listenForChannelOpen()
                            && voiceOption == VoiceOptionEnum.HOLD && crewChief.speechRecogniser != null && crewChief.speechRecogniser.initialised;

                        updateActions();

                        this.controllerConfiguration.scanInProgress = false;
                        this.scanControllers.Text = Configuration.getUIString("scan_for_controllers");
                        this.scanControllers.Enabled = true;
                    }
                });
            }
            catch (Exception)
            {
                // Shutdown.
            }
        }

        private void voiceDisableButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                runListenForChannelOpenThread = false;
                runListenForButtonPressesThread = controllerConfiguration.listenForButtons(false);
                voiceOption = VoiceOptionEnum.DISABLED;
                // Turns out saving prefs takes 5% of main thread time on startup, so don't do it
                // as we just read this from prefs.
                if (!this.constructingWindow)
                {
                    UserSettings.GetUserSettings().setProperty("VOICE_OPTION", getVoiceOptionString());
                    UserSettings.GetUserSettings().saveUserSettings();
                }
            }
        }

        private void triggerWordButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                try
                {
                    if (initialiseSpeechEngine())
                    {
                        runListenForChannelOpenThread = false;
                        runListenForButtonPressesThread = controllerConfiguration.listenForButtons(false);
                        crewChief.speechRecogniser.voiceOptionEnum = VoiceOptionEnum.TRIGGER_WORD;
                        voiceOption = VoiceOptionEnum.TRIGGER_WORD;
                        UserSettings.GetUserSettings().setProperty("VOICE_OPTION", getVoiceOptionString());
                        UserSettings.GetUserSettings().saveUserSettings();
                    }
                    else
                    {
                        ((RadioButton)sender).Checked = false;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to initialise speech engine, message = " + ex.Message);
                }
            }
        }

        private void holdButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                try
                {
                    if(initialiseSpeechEngine())
                    {
                        runListenForButtonPressesThread = controllerConfiguration.listenForButtons(false);
                        crewChief.speechRecogniser.voiceOptionEnum = VoiceOptionEnum.HOLD;
                        voiceOption = VoiceOptionEnum.HOLD;
                        runListenForChannelOpenThread = true;
                        UserSettings.GetUserSettings().setProperty("VOICE_OPTION", getVoiceOptionString());
                        UserSettings.GetUserSettings().saveUserSettings();
                    }
                    else
                    {
                        ((RadioButton)sender).Checked = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to initialise speech engine, message = " + ex.Message);
                }
            }
        }
        private void toggleButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                try
                {
                    if(initialiseSpeechEngine())
                    {
                        runListenForButtonPressesThread = true;
                        runListenForChannelOpenThread = false;
                        crewChief.speechRecogniser.voiceOptionEnum = VoiceOptionEnum.TOGGLE;
                        voiceOption = VoiceOptionEnum.TOGGLE;
                        UserSettings.GetUserSettings().setProperty("VOICE_OPTION", getVoiceOptionString());
                        UserSettings.GetUserSettings().saveUserSettings();
                    }
                    else
                    {
                        ((RadioButton)sender).Checked = false;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to initialise speech engine, message = " + ex.Message);
                }
            }
        }
        private void alwaysOnButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                try
                {
                    if(initialiseSpeechEngine())
                    {
                        runListenForChannelOpenThread = false;
                        runListenForButtonPressesThread = controllerConfiguration.listenForButtons(false);
                        crewChief.speechRecogniser.voiceOptionEnum = VoiceOptionEnum.ALWAYS_ON;
                        voiceOption = VoiceOptionEnum.ALWAYS_ON;
                        UserSettings.GetUserSettings().setProperty("VOICE_OPTION", getVoiceOptionString());
                        UserSettings.GetUserSettings().saveUserSettings();
                    }
                    else
                    {
                        ((RadioButton)sender).Checked = false;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to initialise speech engine, message = " + ex.Message);
                }
            }
        }

        private void personalisationSelected(object sender, EventArgs e)
        {
            if (!UserSettings.GetUserSettings().getString("PERSONALISATION_NAME").Equals(this.personalisationBox.Text))
            {
                UserSettings.GetUserSettings().setProperty("PERSONALISATION_NAME", this.personalisationBox.Text);
                UserSettings.GetUserSettings().saveUserSettings();
                doRestart(Configuration.getUIString("the_application_must_be_restarted_to_load_the_new_sounds"), Configuration.getUIString("load_new_sounds"));
            }
        }

        private void messagesAudioDeviceSelected(object sender, EventArgs e)
        {
            Tuple<string, int> device = null;
            if (AudioPlayer.playbackDevices.TryGetValue(this.messagesAudioDeviceBox.Text, out device))
            {
                int deviceId = device.Item2;
                AudioPlayer.naudioMessagesPlaybackDeviceId = deviceId;
                UserSettings.GetUserSettings().setProperty("NAUDIO_DEVICE_GUID_MESSAGES",
                    AudioPlayer.playbackDevices[this.messagesAudioDeviceBox.Text].Item1);
                UserSettings.GetUserSettings().saveUserSettings();
            }
        }

        private void speechRecognitionDeviceSelected(object sender, EventArgs e)
        {
            Tuple<string, int> device = null;
            if (SpeechRecogniser.speechRecognitionDevices.TryGetValue(this.speechRecognitionDeviceBox.Text, out device))
            {
                int deviceId = device.Item2;
                crewChief.speechRecogniser.changeInputDevice(deviceId);
                UserSettings.GetUserSettings().setProperty("NAUDIO_RECORDING_DEVICE_GUID",
                    SpeechRecogniser.speechRecognitionDevices[this.speechRecognitionDeviceBox.Text].Item1);
                UserSettings.GetUserSettings().saveUserSettings();
            }
        }

        private void backgroundAudioDeviceSelected(object sender, EventArgs e)
        {
            Tuple<string, int> device = null;
            if (AudioPlayer.playbackDevices.TryGetValue(this.backgroundAudioDeviceBox.Text, out device))
            {
                int deviceId = device.Item2; 
                AudioPlayer.naudioBackgroundPlaybackDeviceId = deviceId;
                UserSettings.GetUserSettings().setProperty("NAUDIO_DEVICE_GUID_BACKGROUND",
                    AudioPlayer.playbackDevices[this.backgroundAudioDeviceBox.Text].Item1);
                UserSettings.GetUserSettings().saveUserSettings();
            }
        }

        private void chiefNameSelected(object sender, EventArgs e)
        {
            if (!UserSettings.GetUserSettings().getString("chief_name").Equals(this.chiefNameBox.Text))
            {
                UserSettings.GetUserSettings().setProperty("chief_name", this.chiefNameBox.Text);
                UserSettings.GetUserSettings().saveUserSettings();
                doRestart(Configuration.getUIString("the_application_must_be_restarted_to_load_the_new_sounds"), Configuration.getUIString("load_new_sounds"));
            }
        }

        private void spotterNameSelected(object sender, EventArgs e)
        {
            if (!UserSettings.GetUserSettings().getString("spotter_name").Equals(this.spotterNameBox.Text))
            {
                UserSettings.GetUserSettings().setProperty("spotter_name", this.spotterNameBox.Text);
                UserSettings.GetUserSettings().saveUserSettings();
                doRestart(Configuration.getUIString("the_application_must_be_restarted_to_load_the_new_sounds"), Configuration.getUIString("load_new_sounds"));
            }
        }

        private VoiceOptionEnum getVoiceOptionEnum(String enumStr)
        {
            VoiceOptionEnum enumVal = VoiceOptionEnum.DISABLED;
            if (enumStr != null && enumStr.Length > 0)
            {
                enumVal = (VoiceOptionEnum)VoiceOptionEnum.Parse(typeof(VoiceOptionEnum), enumStr, true);
            }
            return enumVal;
        }

        private String getVoiceOptionString()
        {
            return voiceOption.ToString();
        }

        public enum VoiceOptionEnum
        {
            DISABLED, HOLD, TOGGLE, ALWAYS_ON, TRIGGER_WORD
        }

        private void clearConsole(object sender, EventArgs e)
        {
            if (!consoleTextBox.IsDisposed)
            {
                try
                {
                    lock (this)
                    {
                        consoleTextBox.Text = "";
                        consoleWriter.builder.Clear();
                    }
                }
                catch (Exception)
                {
                    // swallow - nothing to log it to
                }
            }
        }

        private void populateControlListUI()
        {
            if (controllerConfiguration != null)
            {
                if (this.gameDefinitionList.Text.Equals(GameDefinition.pCarsNetwork.friendlyName) || this.gameDefinitionList.Text.Equals(GameDefinition.pCars2Network.friendlyName))
                {
                    controllerConfiguration.addNetworkControllerToList();
                }
                else
                {
                    controllerConfiguration.removeNetworkControllerFromList();
                }
                updateControllersUi();
            }
        }
        private void updateSelectedGameDefinition(object sender, EventArgs e)
        {
            if (this.gameDefinitionList.Text.Length > 0)
            {
                try
                {
                    CrewChief.gameDefinition = GameDefinition.getGameDefinitionForFriendlyName(this.gameDefinitionList.Text);
                }
                catch (Exception) { }
            }
            populateControlListUI();
        }

        private enum DownloadType
        {
            DRIVER_NAMES, SOUND_PACK, PERSONALISATIONS
        }

        private void startDownload(DownloadType downloadType)
        {
            // Strictly speaking, it is not ok to dispose object before Async calls are complete.  However, due to
            // legacy reasons, Dispose on WebClient does not interfere with Async call completion.  Correct pattern
            // is to CancelAsync on form close, and dispose in callbacks or on form close. But code as is works too,
            // by luck, so just add a formClosed check in callbacks.  That's safe, because they're invoked on the UI
            // thread.
            using (WebClient wc = new WebClient())
            {
                if (downloadType == DownloadType.SOUND_PACK)
                {
                    isDownloadingSoundPack = true;
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(soundpack_DownloadProgressChanged);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(soundpack_DownloadFileCompleted);
                    try
                    {
                        File.Delete(AudioPlayer.soundFilesPathNoChiefOverride + @"\" + soundPackTempFileName);
                    }
                    catch (Exception) { }
                    wc.DownloadFileAsync(new Uri(soundPackDownloadURL), AudioPlayer.soundFilesPathNoChiefOverride + @"\" + soundPackTempFileName);
                }
                else if (downloadType == DownloadType.DRIVER_NAMES)
                {
                    isDownloadingDriverNames = true;
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(drivernames_DownloadProgressChanged);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(drivernames_DownloadFileCompleted);
                    try
                    {
                        File.Delete(AudioPlayer.soundFilesPathNoChiefOverride + @"\" + driverNamesTempFileName);
                    }
                    catch (Exception) { }
                    wc.DownloadFileAsync(new Uri(drivernamesDownloadURL), AudioPlayer.soundFilesPathNoChiefOverride + @"\" + driverNamesTempFileName);
                }
                else if (downloadType == DownloadType.PERSONALISATIONS)
                {
                    isDownloadingPersonalisations = true;
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(personalisations_DownloadProgressChanged);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(personalisations_DownloadFileCompleted);
                    try
                    {
                        File.Delete(AudioPlayer.soundFilesPathNoChiefOverride + @"\" + personalisationsTempFileName);
                    }
                    catch (Exception) { }
                    wc.DownloadFileAsync(new Uri(personalisationsDownloadURL), AudioPlayer.soundFilesPathNoChiefOverride + @"\" + personalisationsTempFileName);
                }
            }
        }

        void soundpack_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (formClosed)
            {
                return;
            }
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            if (percentage > 0)
            {
                soundPackProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            }
        }

        void drivernames_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (formClosed)
            {
                return;
            }
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            if (percentage > 0)
            {
                driverNamesProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            }
        }
        void personalisations_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (formClosed)
            {
                return;
            }
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            if (percentage > 0)
            {
                personalisationsProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            }
        }

        void soundpack_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (formClosed)
            {
                return;
            }
            if (e.Error == null && !e.Cancelled)
            {
                String extractingButtonText = Configuration.getUIString("extracting_sound_pack");
                downloadSoundPackButton.Text = extractingButtonText;
                var extractSoundPackThread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    Boolean success = false;
                    Thread progressThread = null;
                    try
                    {
                        if (Directory.Exists(AudioPlayer.soundFilesPathNoChiefOverride + @"\sounds_temp"))
                        {
                            Directory.Delete(AudioPlayer.soundFilesPathNoChiefOverride + @"\sounds_temp", true);
                        }
                        if (formClosed)
                        {
                            return;
                        }
                        progressThread = createProgressThread(downloadSoundPackButton, extractingButtonText);
                        progressThread.Start();
                        ZipFile.ExtractToDirectory(AudioPlayer.soundFilesPathNoChiefOverride + @"\" + soundPackTempFileName, AudioPlayer.soundFilesPathNoChiefOverride + @"\sounds_temp");
                        // It's important to note that the order of these two calls must *not* matter. If it does, the update process results will be inconsistent.
                        // The update pack can contain file rename instructions and file delete instructions but it can *never* contain obsolete files (or files
                        // with old names). As long as this is the case, it shouldn't matter what order we do these in...
                        UpdateHelper.ProcessFileUpdates(AudioPlayer.soundFilesPathNoChiefOverride + @"\sounds_temp");

                        // If we made it here, block the shutdown to complete the move.
                        lock (MainWindow.instanceLock)
                        {
                            if (MainWindow.instance != null)
                            {
                                UpdateHelper.MoveDirectory(AudioPlayer.soundFilesPathNoChiefOverride + @"\sounds_temp", AudioPlayer.soundFilesPathNoChiefOverride);
                                success = true;
                            }
                        }
                    }
                    catch (Exception) { }
                    finally
                    {
                        if (progressThread != null)
                        {
                            progressThread.Abort();
                            Thread.Sleep(100);
                            downloadSoundPackButton.Text = Configuration.getUIString("sound_pack_is_up_to_date");
                        }
                        if (success)
                        {
                            try
                            {
                                File.Delete(AudioPlayer.soundFilesPathNoChiefOverride + @"\" + soundPackTempFileName);
                            }
                            catch (Exception) { }
                        }
                        soundPackProgressBar.Value = 0;
                        isDownloadingSoundPack = false;
                        if (success && !isDownloadingDriverNames && !isDownloadingPersonalisations)
                        {
                            doRestart(Configuration.getUIString(willNeedAnotherSoundPackDownload ? "the_application_must_be_restarted_to_load_the_new_sounds_need_another_restart" :
                                "the_application_must_be_restarted_to_load_the_new_sounds"), Configuration.getUIString("load_new_sounds"));
                        }
                    }
                    if (!success)
                    {
                        soundPackUpdateFailed(false);
                    }
                });
                extractSoundPackThread.Name = "MainWindow.extractSoundPackThread";
                ThreadManager.RegisterResourceThread(extractSoundPackThread);
                extractSoundPackThread.Start();
            }
            else
            {
                soundPackUpdateFailed(e.Cancelled);
            }
        }

        void drivernames_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (formClosed)
            {
                return;
            }
            if (e.Error == null && !e.Cancelled)
            {
                String extractingButtonText = Configuration.getUIString("extracting_driver_names");
                downloadDriverNamesButton.Text = extractingButtonText;
                var extractDriverNamesThread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    Boolean success = false;
                    Thread progressThread = null;
                    try
                    {
                        if (Directory.Exists(AudioPlayer.soundFilesPathNoChiefOverride + @"\driver_names_temp"))
                        {
                            Directory.Delete(AudioPlayer.soundFilesPathNoChiefOverride + @"\driver_names_temp", true);
                        }
                        if (formClosed)
                        {
                            return;
                        }
                        progressThread = createProgressThread(downloadDriverNamesButton, extractingButtonText);
                        progressThread.Start(); 
                        ZipFile.ExtractToDirectory(AudioPlayer.soundFilesPathNoChiefOverride + @"\" + driverNamesTempFileName, AudioPlayer.soundFilesPathNoChiefOverride + @"\driver_names_temp", Encoding.UTF8);

                        // If we made it here, block the shutdown to complete the move.
                        lock (MainWindow.instanceLock)
                        {
                            if (MainWindow.instance != null)
                            {
                                UpdateHelper.MoveDirectory(AudioPlayer.soundFilesPathNoChiefOverride + @"\driver_names_temp", AudioPlayer.soundFilesPathNoChiefOverride);
                                success = true;
                            }
                        }
                    }
                    catch (Exception) { }
                    finally
                    {
                        if (progressThread != null)
                        {
                            progressThread.Abort();
                            Thread.Sleep(100);
                            downloadDriverNamesButton.Text = Configuration.getUIString("driver_names_are_up_to_date");
                        }
                        if (success)
                        {
                            try
                            {
                                File.Delete(AudioPlayer.soundFilesPathNoChiefOverride + @"\" + driverNamesTempFileName);
                            }
                            catch (Exception) { }
                        }
                        driverNamesProgressBar.Value = 0;
                        isDownloadingDriverNames = false;
                        if (success && !isDownloadingSoundPack && !isDownloadingPersonalisations)
                        {
                            doRestart(Configuration.getUIString(willNeedAnotherDrivernamesDownload ? "the_application_must_be_restarted_to_load_the_new_sounds_need_another_restart" :
                                "the_application_must_be_restarted_to_load_the_new_sounds"), Configuration.getUIString("load_new_sounds"));
                        }
                    }
                    if (!success)
                    {
                        driverNamesUpdateFailed(false);
                    }
                });
                extractDriverNamesThread.Name = "MainWindow.extractDriverNamesThread";
                ThreadManager.RegisterResourceThread(extractDriverNamesThread);
                extractDriverNamesThread.Start();
            }
            else
            {
                driverNamesUpdateFailed(e.Cancelled);
            }
        }

        void personalisations_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (formClosed)
            {
                return;
            }
            if (e.Error == null && !e.Cancelled)
            {
                String extractingButtonText = Configuration.getUIString("extracting_personalisations");
                downloadPersonalisationsButton.Text = extractingButtonText;
                var extractPersonalizationsThread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    Boolean success = false;
                    Thread progressThread = null;
                    try
                    {
                        if (e.Error == null && !e.Cancelled)
                        {

                            if (Directory.Exists(AudioPlayer.soundFilesPathNoChiefOverride + @"\personalisations_temp"))
                            {
                                Directory.Delete(AudioPlayer.soundFilesPathNoChiefOverride + @"\personalisations_temp", true);
                            }
                            if (formClosed)
                            {
                                return;
                            }
                            progressThread = createProgressThread(downloadPersonalisationsButton, extractingButtonText);
                            progressThread.Start();
                            ZipFile.ExtractToDirectory(AudioPlayer.soundFilesPathNoChiefOverride + @"\" + personalisationsTempFileName, AudioPlayer.soundFilesPathNoChiefOverride + @"\personalisations_temp", Encoding.UTF8);
                        
                            // If we made it here, block the shutdown to complete the move.
                            lock (MainWindow.instanceLock)
                            {
                                if (MainWindow.instance != null)
                                {
                                    UpdateHelper.MoveDirectory(AudioPlayer.soundFilesPathNoChiefOverride + @"\personalisations_temp", AudioPlayer.soundFilesPathNoChiefOverride + @"\personalisations");
                                    success = true;
                                }
                            }
                        }
                    }
                    catch (Exception e2)
                    {
                        Console.WriteLine("Error extracting, " + e2.Message);
                    }
                    finally
                    {
                        if (progressThread != null)
                        {
                            progressThread.Abort();
                            Thread.Sleep(100);
                            downloadPersonalisationsButton.Text = Configuration.getUIString("personalisations_are_up_to_date");
                        }
                        if (success)
                        {
                            try
                            {
                                File.Delete(AudioPlayer.soundFilesPathNoChiefOverride + @"\" + personalisationsTempFileName);
                            }
                            catch (Exception) { }
                        }
                        personalisationsProgressBar.Value = 0;
                        isDownloadingPersonalisations = false;
                        if (success && !isDownloadingSoundPack && !isDownloadingDriverNames)
                        {
                            doRestart(Configuration.getUIString(willNeedAnotherPersonalisationsDownload ? "the_application_must_be_restarted_to_load_the_new_sounds_need_another_restart" :
                                "the_application_must_be_restarted_to_load_the_new_sounds"), Configuration.getUIString("load_new_sounds"));
                        }
                    }
                    if (!success)
                    {
                        personalisationsUpdateFailed(false);
                    }
                });
                extractPersonalizationsThread.Name = "MainWindow.extractPersonalizationsThread";
                ThreadManager.RegisterResourceThread(extractPersonalizationsThread);
                extractPersonalizationsThread.Start();
            }
            else
            {
                personalisationsUpdateFailed(e.Cancelled);
            }
        }

        // 'ticks' the button so the user knows something's happening
        private Thread createProgressThread(Button button, String text)
        {
            // This thread is managed by sound file extractor threads.
            return new Thread(() =>
            {
                Boolean cancelled = false;
                try
                {
                    while (!cancelled)
                    {
                        lock (MainWindow.instanceLock)
                        {
                            if (MainWindow.instance != null)
                            {
                                button.Text = text + ".";
                                Thread.Sleep(300);
                            }
                        }

                        lock (MainWindow.instanceLock)
                        {
                            if (MainWindow.instance != null)
                            {
                                button.Text = text + "..";
                                Thread.Sleep(300);
                            }
                        }

                        lock (MainWindow.instanceLock)
                        {
                            if (MainWindow.instance != null)
                            {
                                button.Text = text + "...";
                                Thread.Sleep(300);
                            }
                        }

                        lock (MainWindow.instanceLock)
                        {
                            if (MainWindow.instance != null)
                            {
                                button.Text = text;
                                Thread.Sleep(300);
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    cancelled = true;
                    Thread.ResetAbort();
                }
            });
        }
        private void driverNamesUpdateFailed(Boolean cancelled)
        {
            if (!cancelled && !usingRetryAddressForDriverNames && SoundPackVersionsHelper.retryReplace != null && SoundPackVersionsHelper.retryReplaceWith != null)
            {
                Console.WriteLine("Unable to get driver names from " + SoundPackVersionsHelper.retryReplace + " will try from " + SoundPackVersionsHelper.retryReplaceWith);
                usingRetryAddressForDriverNames = true;
                drivernamesDownloadURL = drivernamesDownloadURL.Replace(SoundPackVersionsHelper.retryReplace, SoundPackVersionsHelper.retryReplaceWith);
                startDownload(DownloadType.DRIVER_NAMES);
            }
            else
            {
                startApplicationButton.Enabled = !isDownloadingSoundPack && !isDownloadingPersonalisations;
                if (SoundPackVersionsHelper.currentDriverNamesVersion == -1)
                {
                    downloadDriverNamesButton.Text = Configuration.getUIString("no_driver_names_detected_press_to_download");
                }
                else
                {
                    downloadDriverNamesButton.Text = Configuration.getUIString("updated_driver_names_available_press_to_download");
                }
                if (!IsAppRunning)
                {
                    downloadDriverNamesButton.Enabled = true;
                }
                if (!cancelled)
                {
                    MessageBox.Show(Configuration.getUIString("error_downloading_driver_names"), Configuration.getUIString("unable_to_download_driver_names"),
                        MessageBoxButtons.OK);
                }
            }
        }

        private void soundPackUpdateFailed(Boolean cancelled)
        {
            if (!cancelled && !usingRetryAddressForSoundPack && SoundPackVersionsHelper.retryReplace != null && SoundPackVersionsHelper.retryReplaceWith != null)
            {
                Console.WriteLine("Unable to get sound pack from " + SoundPackVersionsHelper.retryReplace + " will try from " + SoundPackVersionsHelper.retryReplaceWith);
                usingRetryAddressForSoundPack = true;
                soundPackDownloadURL = soundPackDownloadURL.Replace(SoundPackVersionsHelper.retryReplace, SoundPackVersionsHelper.retryReplaceWith);
                startDownload(DownloadType.SOUND_PACK);
            }
            else
            {
                startApplicationButton.Enabled = !isDownloadingDriverNames && !isDownloadingPersonalisations;
                if (SoundPackVersionsHelper.currentSoundPackVersion == -1)
                {
                    downloadSoundPackButton.Text = Configuration.getUIString("no_sound_pack_detected_press_to_download");
                }
                else
                {
                    downloadSoundPackButton.Text = Configuration.getUIString("updated_sound_pack_available_press_to_download");
                }
                if (!IsAppRunning)
                {
                    downloadSoundPackButton.Enabled = true;
                }
                if (!cancelled)
                {
                    MessageBox.Show(Configuration.getUIString("error_downloading_sound_pack"), Configuration.getUIString("unable_to_download_sound_pack"),
                        MessageBoxButtons.OK);
                }
            }
        }

        private void personalisationsUpdateFailed(Boolean cancelled)
        {
            if (!cancelled && !usingRetryAddressForPersonalisations && SoundPackVersionsHelper.retryReplace != null && SoundPackVersionsHelper.retryReplaceWith != null)
            {
            if (!cancelled && !usingRetryAddressForPersonalisations && SoundPackVersionsHelper.retryReplace != null && SoundPackVersionsHelper.retryReplaceWith != null)
                Console.WriteLine("Unable to get personalisations from " + SoundPackVersionsHelper.retryReplace + " will try from " + SoundPackVersionsHelper.retryReplaceWith);
                usingRetryAddressForPersonalisations = true;
                personalisationsDownloadURL = personalisationsDownloadURL.Replace(SoundPackVersionsHelper.retryReplace, SoundPackVersionsHelper.retryReplaceWith);
                startDownload(DownloadType.PERSONALISATIONS);
            }
            else
            {
                startApplicationButton.Enabled = !isDownloadingSoundPack && !isDownloadingDriverNames;
                if (SoundPackVersionsHelper.currentPersonalisationsVersion == -1)
                {
                    downloadPersonalisationsButton.Text = Configuration.getUIString("no_personalisations_detected_press_to_download");
                }
                else
                {
                    downloadPersonalisationsButton.Text = Configuration.getUIString("updated_personalisations_available_press_to_download");
                }
                if (!IsAppRunning)
                {
                    downloadPersonalisationsButton.Enabled = true;
                }
                if (!cancelled)
                {
                    MessageBox.Show(Configuration.getUIString("error_downloading_personalisations"), Configuration.getUIString("unable_to_download_personalisations"),
                        MessageBoxButtons.OK);
                }
            }
        }
        
        private void doRestart(String warningMessage, String warningTitle, Boolean removeSkipUpdates = false)
        {
            if (CrewChief.Debugging)
            {
                warningMessage = "The app must be restarted manually";
            }

            // Make app visible first.
            this.RestoreFromTray();

            if (MessageBox.Show(warningMessage, warningTitle,
                CrewChief.Debugging ? MessageBoxButtons.OK : MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                if (!CrewChief.Debugging)
                {
                    // have to add "multi" to the start args so the app can restart
                    List<String> startArgs = new List<string>();
                    foreach (String startArg in Environment.GetCommandLineArgs())
                    {
                        // if we're restarting because the 'force update check' was clicked, remove the SKIP_UPDATES arg
                        if (removeSkipUpdates && "SKIP_UPDATES".Equals(startArg))
                        {
                            continue;
                        }
                        startArgs.Add(startArg);
                    }
                    if (!startArgs.Contains("multi"))
                    {
                        startArgs.Add("multi");
                    }
                    if (!startArgs.Contains("app_restart"))
                    {
                        startArgs.Add("app_restart");
                    }
                    System.Diagnostics.Process.Start(Application.ExecutablePath, String.Join(" ", startArgs.ToArray())); // to start new instance of application
                    this.Close(); //to turn off current app
                }
            }
        }

        private void downloadSoundPackButtonPress(object sender, EventArgs e)
        {
            if (AudioPlayer.soundPackLanguage == null)
            {
                DialogResult dialogResult = MessageBox.Show(Configuration.getUIString("unknown_sound_pack_language_text"),
                    Configuration.getUIString("unknown_sound_pack_language_title"), MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    startApplicationButton.Enabled = false;
                    downloadSoundPackButton.Text = Configuration.getUIString("downloading_sound_pack");
                    downloadSoundPackButton.Enabled = false;
                    startDownload(DownloadType.SOUND_PACK);
                }
                else if (dialogResult == DialogResult.No)
                {
                }
            }
            else
            {
                startApplicationButton.Enabled = false;
                downloadSoundPackButton.Text = Configuration.getUIString("downloading_sound_pack");
                downloadSoundPackButton.Enabled = false;
                startDownload(DownloadType.SOUND_PACK);
            }
        }

        private void downloadDriverNamesButtonPress(object sender, EventArgs e)
        {
            if (AudioPlayer.soundPackLanguage == null)
            {
                DialogResult dialogResult = MessageBox.Show(Configuration.getUIString("unknown_driver_names_language_text"),
                    Configuration.getUIString("unknown_driver_names_language_title"), MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    startApplicationButton.Enabled = false;
                    downloadDriverNamesButton.Text = Configuration.getUIString("downloading_driver_names");
                    downloadDriverNamesButton.Enabled = false;
                    startDownload(DownloadType.DRIVER_NAMES);
                }
                else if (dialogResult == DialogResult.No)
                {
                }
            }
            else
            {
                startApplicationButton.Enabled = false;
                downloadDriverNamesButton.Text = Configuration.getUIString("downloading_driver_names");
                downloadDriverNamesButton.Enabled = false;
                startDownload(DownloadType.DRIVER_NAMES);
            }
        }

        private void downloadPersonalisationsButtonPress(object sender, EventArgs e)
        {
            if (AudioPlayer.soundPackLanguage == null)
            {
                DialogResult dialogResult = MessageBox.Show(Configuration.getUIString("unknown_personalisations_language_text"),
                    Configuration.getUIString("unknown_personalisations_language_title"), MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    startApplicationButton.Enabled = false;
                    downloadPersonalisationsButton.Text = Configuration.getUIString("downloading_personalisations");
                    downloadPersonalisationsButton.Enabled = false;
                    startDownload(DownloadType.PERSONALISATIONS);
                }
                else if (dialogResult == DialogResult.No)
                {
                }
            }
            else
            {
                startApplicationButton.Enabled = false;
                downloadPersonalisationsButton.Text = Configuration.getUIString("downloading_personalisations");
                downloadPersonalisationsButton.Enabled = false;
                startDownload(DownloadType.PERSONALISATIONS);
            }
        }

        private void internetPanHandler(object sender, EventArgs e)
        {
            Process.Start("http://thecrewchief.org/misc.php?do=donate");
        }

        private void playSmokeTestSounds(object sender, EventArgs e)
        {
            if (crewChief.audioPlayer != null)
            {
                new SmokeTest(crewChief.audioPlayer).soundTestPlay(this.smokeTestTextBox.Lines);
            }
        }

        private void ScanControllers_Click(object sender, System.EventArgs e)
        {
            if (!this.controllerConfiguration.scanInProgress)
            {
                this.controllerConfiguration.scanInProgress = true;
                this.controllerRescanThreadWakeUpEvent.Set();
                this.scanControllers.Text = Configuration.getUIString("cancel_scan");
            }
            else
            {
                this.scanControllers.Enabled = false;
                this.controllerConfiguration.cancelScan();
            }
        }
        private void editCommandMacroButtonClicked(object sender, EventArgs e)
        {
            var form = new MacroEditor();
            form.ShowDialog(this);
        }

        private void AddRemoveActions_Click(object sender, EventArgs e)
        {
            var form = new ActionEditor();
            form.ShowDialog(this);
        }
    }


    public class ControlWriter : TextWriter
    {
        private int repetitionCount = 0;
        private String previousMessage = null;

        public Boolean enable = true;
        public StringBuilder builder = new StringBuilder();

        public StringBuilder newMessagesBuilder = new StringBuilder();
        public static object controlWriterLock = new object();
        private AutoResetEvent consoleUpdateThreadWakeUpEvent = null;
        public ControlWriter(RichTextBox textbox, AutoResetEvent consoleUpdateThreadWakeUpEvent)
        {
            this.consoleUpdateThreadWakeUpEvent = consoleUpdateThreadWakeUpEvent;
        }

        public override void WriteLine(string value)
        {
            if (MainWindow.instance != null && (enable || MainWindow.instance.recordSession.Checked))
            {
                if (value == previousMessage)
                {
                    repetitionCount++;
                }
                else
                {
                    if (repetitionCount > 0 && repetitionCount < 20)
                    {
                        writeMessage("Skipped " + repetitionCount + " copies of previous message\n");
                    }
                    else if (repetitionCount >= 20 && MainWindow.instance.crewChief.mapped)
                    {
                        writeMessage("++++++++++++ Skipped " + repetitionCount + " copies of previous message. Please report this error to the CC dev team ++++++++++++\n");
                    }
                    repetitionCount = 0;
                    previousMessage = value;
                    Boolean gotDateStamp = false;
                    StringBuilder sb = new StringBuilder();
                    DateTime now = DateTime.Now;
                    if (CrewChief.loadDataFromFile)
                    {
                        if (CrewChief.currentGameState != null)
                        {
                            if (CrewChief.currentGameState.CurrentTimeStr == null || CrewChief.currentGameState.CurrentTimeStr == "")
                            {
                                CrewChief.currentGameState.CurrentTimeStr = GameStateData.CurrentTime.ToString("HH:mm:ss.fff");
                            }
                            sb.Append(now.ToString("HH:mm:ss.fff")).Append(" (").Append(CrewChief.currentGameState.CurrentTimeStr).Append(")");
                            gotDateStamp = true;
                        }
                    }
                    if (!gotDateStamp)
                    {
                        sb.Append(now.ToString("HH:mm:ss.fff"));
                    }
                    sb.Append(" : ").Append(value).AppendLine();
                    writeMessage(sb.ToString());
                }
            }
        }

        private void writeMessage(String message)
        {
            if (enable)
            {
                lock (ControlWriter.controlWriterLock)
                {
                    newMessagesBuilder.Append(message);
                }
                consoleUpdateThreadWakeUpEvent.Set();
            }
            else
            {
                lock (ControlWriter.controlWriterLock)
                {
                    builder.Append(message);
                }
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

    static class NativeMethods
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
    }
}
