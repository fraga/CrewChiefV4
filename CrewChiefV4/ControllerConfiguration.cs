﻿using CrewChiefV4.PCars;
using CrewChiefV4.PCars2;
using Newtonsoft.Json;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CrewChiefV4.Events;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Diagnostics;
using CrewChiefV4.commands;
using WindowsInput.Native;

namespace CrewChiefV4
{
    public class ControllerConfiguration : IDisposable
    {
        private static Guid UDP_NETWORK_CONTROLLER_GUID = new Guid("2bbfed03-a04f-4408-91cf-e0aa6b20b8ff");

        private MainWindow mainWindow;
        public Boolean listenForAssignment = false;
        DirectInput directInput = new DirectInput();
        public static DeviceType[] supportedDeviceTypes = new DeviceType[] {DeviceType.Driving, DeviceType.Joystick, DeviceType.Gamepad,
            DeviceType.Keyboard, DeviceType.ControlDevice, DeviceType.FirstPerson, DeviceType.Flight,
            DeviceType.Supplemental, DeviceType.Remote};

        // Note: Below two collections are accessed from the multiple threads, but not yet synchronized.
        public List<ButtonAssignment> buttonAssignments = new List<ButtonAssignment>();
        public List<ControllerData> controllers;

        // Controllers we found during last device scan, not necessarily all connected.
        public List<ControllerData> knownControllers;

        private Dictionary<Guid, bool> knownControllerState = new Dictionary<Guid, bool>();

        private static Boolean usersConfigFileIsBroken = false;

        // keep track of all the Joystick devices we've 'acquired'
        private static Dictionary<Guid, Joystick> activeDevices = new Dictionary<Guid, Joystick>();
        // separate var for added custom controller
        private Guid customControllerGuid = Guid.Empty;

        // built in controller button functions:
        private static String CHANNEL_OPEN_FUNCTION = "talk_to_crew_chief";
        private static String TOGGLE_SPOTTER_FUNCTION = "toggle_spotter_on/off";
        private static String VOLUME_UP = "volume_up";
        private static String VOLUME_DOWN = "volume_down";
        private static String TOGGLE_MUTE = "toggle_mute";
        private static String RESET_VR_VIEW = "reset_vr_view";

        public static String TOGGLE_RACE_UPDATES_FUNCTION = "toggle_race_updates_on/off";
        public static String TOGGLE_READ_OPPONENT_DELTAS = "toggle_opponent_deltas_on/off_for_each_lap";
        public static String REPEAT_LAST_MESSAGE_BUTTON = "press_to_replay_the_last_message";
        public static String PRINT_TRACK_DATA = "print_track_data";
        public static String TOGGLE_YELLOW_FLAG_MESSAGES = "toggle_yellow_flag_messages";
        public static String GET_FUEL_STATUS = "get_fuel_status";
        public static String TOGGLE_MANUAL_FORMATION_LAP = "toggle_manual_formation_lap";
        public static String READ_CORNER_NAMES_FOR_LAP = "read_corner_names_for_lap";

        public static String GET_CAR_STATUS = "get_car_status";
        public static String GET_STATUS = "get_status";
        public static String GET_SESSION_STATUS = "get_session_status";
        public static String GET_DAMAGE_REPORT = "get_damage_report";

        public static String TOGGLE_PACE_NOTES_RECORDING = "toggle_pace_notes_recording";
        public static String TOGGLE_PACE_NOTES_PLAYBACK = "toggle_pace_notes_playback";

        public static String TOGGLE_TRACK_LANDMARKS_RECORDING = "toggle_track_landmarks_recording";
        public static String TOGGLE_ENABLE_CUT_TRACK_WARNINGS = "toggle_enable_cut_track_warnings";

        public static String ADD_TRACK_LANDMARK = "add_track_landmark";

        public static String PIT_PREDICTION = "activate_pit_prediction";

        public static String TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS = "toggle_delay_messages_in_hard_parts";

        private ControllerData networkGamePad = new ControllerData(Configuration.getUIString("udp_network_data_buttons"), DeviceType.Gamepad, UDP_NETWORK_CONTROLLER_GUID);

        #region ConcreteControllerActions
        // these are actions *not* handled by an AbstractEvent instance because of some batshit internal wiring that's impossible to unpick
        private static Dictionary<String, Action> specialActions = new Dictionary<string, Action>()
        {
            { CHANNEL_OPEN_FUNCTION  , channelOpen },
            { TOGGLE_SPOTTER_FUNCTION, toggleSpotter },
            { VOLUME_UP              , volumeUp },
            { VOLUME_DOWN            , volumeDown },
            { TOGGLE_MUTE            , toggleMute },
            { RESET_VR_VIEW          , resetVRview },
        };
        #endregion ConcreteControllerActions

        // this is a map of legacy action name (stuff like "CHANNEL_OPEN_FUNCTION") to new action name (stuff like "talk_to_crew_chief")
        // and is used to move old properties values over to the new JSON format on app start (when there's no user file for mappings)
        public static Dictionary<String, String> builtInActionMappings = new Dictionary<String, String>()
        {
            { nameof(CHANNEL_OPEN_FUNCTION), CHANNEL_OPEN_FUNCTION },
            { nameof(TOGGLE_SPOTTER_FUNCTION), TOGGLE_SPOTTER_FUNCTION },
            { nameof(VOLUME_UP), VOLUME_UP },
            { nameof(VOLUME_DOWN), VOLUME_DOWN },
            { nameof(TOGGLE_MUTE), TOGGLE_MUTE },
            { nameof(TOGGLE_RACE_UPDATES_FUNCTION), TOGGLE_RACE_UPDATES_FUNCTION },
            { nameof(TOGGLE_READ_OPPONENT_DELTAS), TOGGLE_READ_OPPONENT_DELTAS },
            { nameof(REPEAT_LAST_MESSAGE_BUTTON), REPEAT_LAST_MESSAGE_BUTTON },
            { nameof(PRINT_TRACK_DATA), PRINT_TRACK_DATA },
            { nameof(TOGGLE_YELLOW_FLAG_MESSAGES), TOGGLE_YELLOW_FLAG_MESSAGES },
            { nameof(GET_FUEL_STATUS), GET_FUEL_STATUS },
            { nameof(TOGGLE_MANUAL_FORMATION_LAP), TOGGLE_MANUAL_FORMATION_LAP },
            { nameof(READ_CORNER_NAMES_FOR_LAP), READ_CORNER_NAMES_FOR_LAP },
            { nameof(GET_CAR_STATUS), GET_CAR_STATUS },
            { nameof(GET_STATUS), GET_STATUS },
            { nameof(GET_SESSION_STATUS), GET_SESSION_STATUS },
            { nameof(GET_DAMAGE_REPORT), GET_DAMAGE_REPORT },
            { nameof(TOGGLE_PACE_NOTES_RECORDING), TOGGLE_PACE_NOTES_RECORDING },
            { nameof(TOGGLE_PACE_NOTES_PLAYBACK), TOGGLE_PACE_NOTES_PLAYBACK },
            { nameof(TOGGLE_TRACK_LANDMARKS_RECORDING), TOGGLE_TRACK_LANDMARKS_RECORDING },
            { nameof(TOGGLE_ENABLE_CUT_TRACK_WARNINGS), TOGGLE_ENABLE_CUT_TRACK_WARNINGS },
            { nameof(ADD_TRACK_LANDMARK), ADD_TRACK_LANDMARK },
            { nameof(PIT_PREDICTION), PIT_PREDICTION },
            { nameof(TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS), TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS }
        };

        public bool scanInProgress = false;
        private Thread controllerScanThread = null;

        public class ControllerConfigurationData
        {
            public ControllerConfigurationData()
            {
                devices = new List<ControllerData>();
                buttonAssignments = new List<ButtonAssignment>();
            }
            public List<ControllerData> devices { get; set; }
            public List<ButtonAssignment> buttonAssignments { get; set; }
        }

        public void assignButtonEventInstances()
        {
            foreach (ButtonAssignment ba in buttonAssignments)
            {
                ba.findEvent();
            }
        }

        private static String getDefaultControllerConfigurationDataFileLocation()
        {
            String path = Configuration.getDefaultFileLocation("controllerConfigurationData.json");
            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                return null;
            }
        }
        public static String getUserControllerConfigurationDataFileLocation()
        {
            var currProfileName = UserSettings.GetUserSettings().getString("current_settings_profile");
            var path = System.IO.Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments), @"CrewChiefV4\Profiles\ControllerData", currProfileName);

            if (File.Exists(path))
            {
                return path;
            }

            path = System.IO.Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "controllerConfigurationData.json");

            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                return null;
            }
        }

        public static ControllerConfigurationData getControllerConfigurationDataFromFile(String filename)
        {
            if (filename != null && !usersConfigFileIsBroken)
            {
                try
                {
                    using (StreamReader r = new StreamReader(filename))
                    {
                        string json = r.ReadToEnd();
                        ControllerConfigurationData data = JsonConvert.DeserializeObject<ControllerConfigurationData>(json);
                        if (data != null)
                        {
                            usersConfigFileIsBroken = false;
                            return data;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing " + filename + ": " + e.Message);
                    ControllerConfiguration.usersConfigFileIsBroken = true;
                    MessageBox.Show(Configuration.getUIString("controller_mappings_file_error_details_1") + " " + filename + " " +
                            Configuration.getUIString("controller_mappings_file_error_details_2") + " " + e.Message,
                            Configuration.getUIString("controller_mappings_file_error_title"),
                        MessageBoxButtons.OK);
                }
            }
            return new ControllerConfigurationData();
        }

        public static void saveControllerConfigurationDataFile(ControllerConfigurationData buttonsActions)
        {
            if (usersConfigFileIsBroken)
            {
                Console.WriteLine("Unable to update controller bindings because the file isn't valid JSON");
                return;
            }

            var fileName = UserSettings.GetUserSettings().getString("current_settings_profile");

            String path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"CrewChiefV4\Profiles");
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error creating " + path + ": " + e.Message);
                }
            }

            path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"CrewChiefV4\Profiles\ControllerData");
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error creating " + path + ": " + e.Message);
                }
            }

            if (fileName != null)
            {
                try
                {
                    using (StreamWriter file = File.CreateText(System.IO.Path.Combine(path, fileName)))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                        serializer.Serialize(file, buttonsActions);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing " + fileName + ": " + e.Message);
                }
            }
        }

        public void Dispose()
        {
            mainWindow = null;
            unacquireAndDisposeActiveJoysticks();
            try
            {
                directInput.Dispose();
            }
            catch (Exception e) {Log.Exception(e);}
        }

        private void unacquireAndDisposeActiveJoysticks()
        {
            lock (activeDevices)
            {
                foreach (Joystick joystick in activeDevices.Values)
                {
                    try
                    {
                        joystick.Unacquire();
                    }
                    catch (Exception e) {Log.Exception(e);}
                    try
                    {
                        joystick.Dispose();
                    }
                    catch (Exception e) {Log.Exception(e);}
                }
                activeDevices.Clear();
            }
        }

        public ControllerConfiguration(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

        }

        private bool initialized = false;
        public void initialize()
        {
            if (this.initialized
                && !CrewChief.Debugging)  // Allow re-initialization while debugging (Profile activation without restart).
            {
                Debug.Assert(!this.initialized, "This method should be only called once.");
                return;
            }

            // update existing data to use new json - copies assignments from old properties data into new json data, used when there's
            // no user controllers config json file
            if (getUserControllerConfigurationDataFileLocation() == null)
            {
                ControllerConfigurationData oldUserData = new ControllerConfigurationData();
                foreach (KeyValuePair<String, String> assignment in builtInActionMappings)
                {
                    int buttonIndex = UserSettings.GetUserSettings().getInt(assignment.Key + "_button_index");
                    String deviceGuid = UserSettings.GetUserSettings().getString(assignment.Key + "_device_guid");
                    oldUserData.buttonAssignments.Add(new ButtonAssignment() { deviceGuid = deviceGuid, buttonIndex = buttonIndex, action = assignment.Value });
                }
                oldUserData.devices = ControllerData.parse(UserSettings.GetUserSettings().getString(ControllerData.PROPERTY_CONTAINER));
                saveControllerConfigurationDataFile(oldUserData);
            }
            // if there is something in the default data file we want to add it, this is in case we want to add default button actions later on
            ControllerConfigurationData defaultData = getControllerConfigurationDataFromFile(getDefaultControllerConfigurationDataFileLocation());
            ControllerConfigurationData controllerConfigurationData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());

            var missingNewButtonMappings = builtInActionMappings.Where(builtIn => !controllerConfigurationData.buttonAssignments.Any(ba => ba.action == builtIn.Value));

            if (defaultData.buttonAssignments.Count > 0 || missingNewButtonMappings.Count() > 0) // app updated add, missing elements ?
            {
                Boolean save = false;
                var missingItems = defaultData.buttonAssignments.Where(ba2 => !controllerConfigurationData.buttonAssignments.Any(ba1 => ba1.action == ba2.action));
                if (missingItems.ToList().Count > 0)
                {
                    save = true;
                    controllerConfigurationData.buttonAssignments.AddRange(missingItems);
                }
                foreach (KeyValuePair<string, string> missingNewButtonMapping in missingNewButtonMappings)
                {
                    save = true;
                    ButtonAssignment newAssignment = new ButtonAssignment();
                    newAssignment.action = missingNewButtonMapping.Value;
                    controllerConfigurationData.buttonAssignments.Add(newAssignment);
                }
                if (save)
                {
                    saveControllerConfigurationDataFile(controllerConfigurationData);
                }
            }
            // update actions and add assignments
            buttonAssignments = controllerConfigurationData.buttonAssignments.Where(ba => ba.availableAction).ToList();
            controllers = controllerConfigurationData.devices;
            // check if any of the assignments use the network controller, and if so add it to the list
            ButtonAssignment networkAssignment = buttonAssignments.FirstOrDefault(ba => ba.deviceGuid == UDP_NETWORK_CONTROLLER_GUID.ToString());
            if (networkAssignment != null)
            {
                addNetworkControllerToList();
            }
            foreach (ButtonAssignment assignment in buttonAssignments)
            {
                assignment.controller = controllers.FirstOrDefault(c => c.guid.ToString() == assignment.deviceGuid);
                assignment.Initialize();
            }

            this.initialized = true;
        }

        // just sets the custom controller guid so the scan call will populate it later
        public void addCustomController(Guid guid)
        {
            customControllerGuid = guid;
        }

        /// <summary>
        /// For each controller button that is assigned that is pressed set
        /// hasUnprocessedClick which will execute the assigned action when
        /// ExecuteClickedButton() is called or in the case of "specialActions"
        /// when ExecuteSpecialClickedButton() is called
        /// Also handles auto-repeat
        /// </summary>
        public void PollForButtonClicks()
        {
            foreach (var assignment in buttonAssignments)
            {
                var autorepeat = assignment.action == VOLUME_UP || assignment.action == VOLUME_DOWN ? 200 : 1000;
                // 200mS for UP/DOWN, 1 second for all other buttons
                pollForButtonClicks(assignment, autorepeat);
            }
        }

        private void pollForButtonClicks(ButtonAssignment ba, int repeatRate)
        {
            if (ba != null && ba.buttonIndex != -1 && ba.controller != null && ba.controller.guid != Guid.Empty)
            {
                if (ba.controller.guid == UDP_NETWORK_CONTROLLER_GUID)
                {
                    bool udpButtonState = false;
                    if (CrewChief.gameDefinition.gameEnum == GameEnum.PCARS_NETWORK)
                    {
                        udpButtonState = PCarsUDPreader.getButtonState(ba.buttonIndex);
                    }
                    else if (CrewChief.gameDefinition.gameEnum == GameEnum.PCARS2_NETWORK)
                    {
                        udpButtonState = PCars2UDPreader.getButtonState(ba.buttonIndex);
                    }
                    if (udpButtonState)
                    {
                        if (!ba.wasPressedDown)
                        {   // New remote "button" click
                            ba.wasPressedDown = true;
                            ba.hasUnprocessedClick = true;
                            // Set auto-repeat timeout
                            ba.clickTime = DateTime.Now.AddMilliseconds(repeatRate);
                        }
                        else if (ba.clickTime < DateTime.Now)
                        {   // Remote "button" still pressed after "repeatRate" mS
                            ba.wasPressedDown = false;
                            // If still pressed then next time it will auto repeat
                        }
                    }
                    else
                    {
                        ba.wasPressedDown = false;
                    }
                }
                else
                {
                    lock (activeDevices)
                    {
                        Joystick joystick;
                        if (activeDevices.TryGetValue(ba.controller.guid, out joystick))
                        {
                            try
                            {
                                JoystickState state = joystick.GetCurrentState();
                                if (state != null)
                                {
                                    Boolean click = ba.usePovData ? state.PointOfViewControllers[ba.buttonIndex] == ba.povValue : state.Buttons[ba.buttonIndex];
                                    if (click)
                                    {
                                        if (!ba.wasPressedDown)
                                        {   // New button click
                                            ba.wasPressedDown = true;
                                            ba.hasUnprocessedClick = true;
                                            // Set auto-repeat timeout
                                            ba.clickTime = DateTime.Now.AddMilliseconds(repeatRate);
                                        }
                                        else if (ba.clickTime < DateTime.Now)
                                        {   // Button still pressed after "repeatRate" mS
                                            ba.wasPressedDown = false;
                                            // If still pressed then next time it will auto repeat
                                        }
                                    }
                                    else
                                    {
                                        ba.wasPressedDown = false;
                                    }
                                }
                            }
                            catch (Exception e) { Log.Exception(e); }
                        }
                    }
                }
            }
        }

        #region ConcreteControllerActions
        /// <summary>
        /// These functions connect to their equivalents in MainWindow
        /// I suspect there's a neater answer...
        /// </summary>
        static void channelOpen()
        {
            MainWindow.instance.channelOpen();
        }
        static void toggleSpotter()
        {
            MainWindow.instance.toggleSpotter();
        }
        static void volumeUp()
        {
            MainWindow.instance.volumeUp();
        }
        static void volumeDown()
        {
            MainWindow.instance.volumeDown();
        }
        static void toggleMute()
        {
            MainWindow.instance.toggleMute();
        }

        /// <summary>
        /// Send Numpad 5 to games that have CC VR DLLs
        /// </summary>
        static void resetVRview()
        {
            if (CrewChief.gameDefinition.friendlyName.Equals("Automobilista") ||
                CrewChief.gameDefinition.friendlyName.Equals("GTR 2"))
            {
                string str = UserSettings.GetUserSettings().getString("reset_vr_view_control");
                if (KeyPresser.parseKeycode(str, out Tuple<VirtualKeyCode?, VirtualKeyCode> keyCode))
                {
                    KeyPresser.SendKeyPress(keyCode);
                    Log.Commentary("Reset VR view");
                }
                else
                {
                    Log.Error($"reset_vr_view_control: invalid keycode '{str}'");
                }
            }
            else
            {
                Log.Verbose($"{CrewChief.gameDefinition.friendlyName} doesn't use 'Reset VR view'");
            }
        }

        /// <summary>
        /// Check if the button assigned to a "special" action has been pressed
        /// Used for actions that do NOT have an AbstractEvent instance
        /// </summary>
        /// <returns>
        /// True: a special action's button was pressed.
        /// </returns>
        public Boolean ExecuteSpecialClickedButton()
        {
            foreach (var action in specialActions.Keys)
            {
                ButtonAssignment ba = buttonAssignments.SingleOrDefault(ba1 => ba1.action == action);
                if (ba != null && ba.hasUnprocessedClick)
                {
                    Log.Verbose($"{ba.action} clicked");
                    specialActions[action].Invoke();
                    ba.hasUnprocessedClick = false;
                    return true;
                }
            }
            return false;
        }
        #endregion ConcreteControllerActions
        /// <summary>
        /// Execute the assigned action for any button that is pressed
        /// Used for actions that do have an AbstractEvent instance
        /// </summary>
        /// <returns>
        /// True: an assigned button's action was executed
        /// </returns>
        public Boolean ExecuteClickedButton()
        {
            foreach (var ba in buttonAssignments)
            {
                if (ba.hasUnprocessedClick && (ba.actionEvent != null || ba.executableCommandMacro != null))
                {
                    string actionName = ba.actionEvent != null ? ba.actionEvent.ToString() : ba.executableCommandMacro.macro.name;
                    Log.Verbose($"\"{actionName}\" executing");
                    bool allowedToRun = ba.execute();
                    // if we're executing a macro, report when we're done
                    if (ba.executableCommandMacro != null)
                    {
                        Log.Verbose(allowedToRun ? "macro complete" : "macro rejected");
                    }
                    ba.hasUnprocessedClick = false;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether a button is assigned for "talk to crew chief"
        /// </summary>
        /// <returns>
        /// True: a button is assigned
        /// </returns>
        public Boolean listenForChannelOpen()
        {
            foreach (ButtonAssignment buttonAssignment in buttonAssignments)
            {
                if (buttonAssignment.action == CHANNEL_OPEN_FUNCTION && buttonAssignment.buttonIndex != -1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether to listen for controller button presses
        /// </summary>
        /// <param name="channelOpenIsToggle">Set if voice command is triggered
        /// by toggling a button</param>
        /// <returns>
        /// True: listen for controller buttons
        /// </returns>
        public Boolean listenForButtons(Boolean channelOpenIsToggle)
        {
            foreach (ButtonAssignment buttonAssignment in buttonAssignments)
            {
                if ((channelOpenIsToggle || buttonAssignment.action != CHANNEL_OPEN_FUNCTION) &&
                    buttonAssignment.controller != null && buttonAssignment.buttonIndex != -1)
                {
                    return true;
                }
            }
            return false;
        }

        public void saveSettings()
        {
            ControllerConfigurationData controllerConfigurationData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());

            // User controller assignment profile contains hidden actions, so we need to merge assigments.
            foreach (var userButtonAssignment in controllerConfigurationData.buttonAssignments)
            {
                if (userButtonAssignment.availableAction)
                {
                    foreach (var currButtonAssignment in this.buttonAssignments)
                    {
                        if (userButtonAssignment.action == currButtonAssignment.action)
                        {
                            userButtonAssignment.deviceGuid = currButtonAssignment.deviceGuid;
                            userButtonAssignment.buttonIndex = currButtonAssignment.buttonIndex;
                            userButtonAssignment.action = currButtonAssignment.action;
                            userButtonAssignment.povValue = currButtonAssignment.povValue;
                            userButtonAssignment.usePovData = currButtonAssignment.usePovData;
                        }
                    }
                }
            }

            saveControllerConfigurationDataFile(controllerConfigurationData);
        }

        public Boolean isChannelOpen()
        {
            ButtonAssignment ba = buttonAssignments.SingleOrDefault(ba1 => ba1.action == CHANNEL_OPEN_FUNCTION);
            if (ba != null && ba.buttonIndex != -1 && ba.controller != null && ba.controller.guid != Guid.Empty)
            {
                if (ba.controller.guid == UDP_NETWORK_CONTROLLER_GUID)
                {
                    return CrewChief.gameDefinition.gameEnum == GameEnum.PCARS_NETWORK ?
                        PCarsUDPreader.getButtonState(ba.buttonIndex) : PCars2UDPreader.getButtonState(ba.buttonIndex);
                }
                else
                {
                    lock (activeDevices)
                    {
                        Joystick joystick;
                        if (activeDevices.TryGetValue(ba.controller.guid, out joystick))
                        {
                            try
                            {
                                return ba.usePovData ? joystick.GetCurrentState().PointOfViewControllers[ba.buttonIndex] == ba.povValue : joystick.GetCurrentState().Buttons[ba.buttonIndex];
                            }
                            catch (Exception e) { Log.Exception(e); }
                        }
                    }
                }
            }
            return false;
        }

        private List<DeviceInstance> getDevices()
        {
            List<DeviceInstance> instancesToReturn = new List<DeviceInstance>();
            try
            {
                // iterate the received devices list explicitly so we can track what's going on
                IList<DeviceInstance> instances = directInput.GetDevices();
                for (int i = 0; i < instances.Count(); i++)
                {
                    DeviceInstance instance = instances[i];
                    if (!supportedDeviceTypes.Contains(instance.Type))
                    {
                        continue;
                    }

                    Console.WriteLine("Adding \"" + instance.Type + "\" device instance " + (i + 1) + " of " + instances.Count + " (\"" + instance.InstanceName + "\")");
                    instancesToReturn.Add(instance);
                }
            }
            catch (Exception e) {Log.Exception(e);}
            return instancesToReturn;
        }

        public void scanControllers()
        {
            Console.WriteLine("Re-scanning controllers...");

            int availableCount = 0;

            // This method is called from the controller refresh thread, either by the device-changed event handler or explicitly on app start.
            // The poll for button clicks call is from a helper thread and accesses the activeDevices list - potentially concurrently

            var scanCancelled = false;
            // Iterate the list available, as reported by sharpDX
            ThreadManager.UnregisterTemporaryThread(this.controllerScanThread);
            this.controllerScanThread = new Thread(() =>
            {
                try
                {
                    lock (activeDevices)
                    {
                        this.controllers = new List<ControllerData>();
                        this.knownControllers.Clear();
                        this.knownControllerState = new Dictionary<Guid, bool>();

                        // dispose all of our active devices:
                        unacquireAndDisposeActiveJoysticks();

                        foreach (var deviceInstance in this.getDevices())
                        {
                            Guid joystickGuid = deviceInstance.InstanceGuid;
                            if (joystickGuid != Guid.Empty)
                            {
                                try
                                {
                                    addController(deviceInstance.InstanceName, deviceInstance.Type, joystickGuid, false, false);
                                    availableCount++;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Failed to get device info: " + e.Message);
                                }
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    scanCancelled = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error looking for controllers: " + e.StackTrace);
                }
            });
            this.controllerScanThread.Start();
            this.controllerScanThread.Name = "ControllerConfiguration.scanControllers";
            ThreadManager.RegisterTemporaryThread(this.controllerScanThread);

            controllerScanThread.Join(1000);
            while (controllerScanThread.IsAlive)
            {
                Thread.Sleep(5000);
                Console.WriteLine("Re-scanning controller devices (this may take a while depending on your configuration)...");
            }

            if (scanCancelled)
            {
                Console.WriteLine("Controller scan cancelled.");
                // On failure, try re-acquire.
                this.reacquireControllers();
                return;
            }
            else
            {
                Console.WriteLine("Controller scan finished.");
            }

            lock (activeDevices)
            {
                // add the custom device if it's set
                if (customControllerGuid != Guid.Empty)
                {
                    try
                    {
                        addController(null, DeviceType.Joystick, customControllerGuid, true, false);
                        availableCount++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to get custom device info: " + e.Message);
                    }
                }
                ControllerConfigurationData controllerConfigurationData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());
                this.controllers = this.controllers.OrderBy(ctrl => ctrl.deviceName).ToList();
                controllerConfigurationData.devices = this.controllers;
                saveControllerConfigurationDataFile(controllerConfigurationData);
                foreach (ButtonAssignment assignment in buttonAssignments.Where(ba => ba.controller == null && ba.buttonIndex != -1 && !string.IsNullOrEmpty(ba.deviceGuid)))
                {
                    addControllerObjectToButtonAssignment(assignment);
                }
            }
            Console.WriteLine("Re-scanned controllers, there are " + availableCount + " available controllers and " + activeDevices.Count + " active controllers");
        }

        public void addControllerObjectToButtonAssignment(ButtonAssignment buttonAssignment)
        {
            buttonAssignment.controller = controllers.FirstOrDefault(c => c.guid.ToString() == buttonAssignment.deviceGuid);
        }

        public void addControllerIfNecessary(string deviceName, string guid)
        {
            Guid deviceGuid;
            if (Guid.TryParse(guid, out deviceGuid))
            {
                if (!activeDevices.ContainsKey(deviceGuid))
                {
                    // don't really care what the device type is here, we're just ensuring the macro-assigned device is active
                    addController(deviceName, DeviceType.Device, deviceGuid, false, true);
                }
            }
        }

        private void addController(string deviceName, DeviceType deviceType, Guid joystickGuid, Boolean isCustomDevice, bool addForMacroSupport)
        {
            lock (activeDevices)
            {
                Boolean isMappedToAction = false;
                Joystick joystick;
                try
                {
                    joystick = new Joystick(directInput, joystickGuid);

                    var previouslyConnected = false;
                    if (!this.knownControllerState.TryGetValue(joystickGuid, out previouslyConnected))
                    {
                        this.knownControllerState.Add(joystickGuid, true /*connected*/);
                    }

                    if (!previouslyConnected)
                    {
                        this.knownControllerState[joystickGuid] = true;  // Connected
                        Console.WriteLine("Device Connected - " + (string.IsNullOrWhiteSpace(deviceName) ? "" : (" Name: \"" + deviceName + "\"    ")) + "GUID: \"" + joystickGuid + "\"");
                    }
                }
                catch (Exception e)
                {
                    var previouslyConnected = true;
                    if (!this.knownControllerState.TryGetValue(joystickGuid, out previouslyConnected))
                    {
                        this.knownControllerState.Add(joystickGuid, false /*connected*/);
                    }

                    if (previouslyConnected)
                    {
                        this.knownControllerState[joystickGuid] = false;  // Connected
                        Console.WriteLine("Device Disconnected - " + (string.IsNullOrWhiteSpace(deviceName) ? "" : (" Name: \"" + deviceName + "\"    ")) + "GUID: \"" + joystickGuid + "\"");
                    }

                    Debug.WriteLine("Unable to create a Joystick device with GUID " + joystickGuid + (string.IsNullOrWhiteSpace(deviceName) ? "" : (" name: " + deviceName)) + ": " + e.Message);
                    return;
                }
                String productName = isCustomDevice ? Configuration.getUIString("custom_device") : deviceType.ToString();
                try
                {
                    productName += ": " + joystick.Properties.ProductName;
                }
                catch (Exception)
                {
                    // ignore - some devices don't have a product name
                }
                if (addForMacroSupport)
                {
                    // when adding for macro support we always want to ensure this device is active because at this point we know
                    // there's an active macro using it
                    if (!activeDevices.ContainsKey(joystickGuid))
                    {
                        joystick.SetCooperativeLevel(mainWindow.Handle, (CooperativeLevel.NonExclusive | CooperativeLevel.Background));
                        joystick.Properties.BufferSize = 128;
                        joystick.Acquire();
                        activeDevices.Add(joystickGuid, joystick);
                    }
                    isMappedToAction = true;
                }
                else
                {
                    foreach (var ba in buttonAssignments.Where(b => b.controller != null && b.controller.guid == joystickGuid && b.buttonIndex != -1))
                    {
                        // if we have a button assigned to this device and it's not active, acquire it here:
                        if (!activeDevices.ContainsKey(joystickGuid))
                        {
                            joystick.SetCooperativeLevel(mainWindow.Handle, (CooperativeLevel.NonExclusive | CooperativeLevel.Background));
                            joystick.Properties.BufferSize = 128;
                            joystick.Acquire();
                            activeDevices.Add(joystickGuid, joystick);
                        }
                        isMappedToAction = true;
                    }
                    controllers.Add(new ControllerData(productName, deviceType, joystickGuid));
                }
                if (!isMappedToAction)
                {
                    // we're not using this device so dispose the temporary handle we used to get its name
                    try
                    {
                        joystick.Dispose();
                    }
                    catch (Exception e) {Log.Exception(e);}
                }
            }
        }

        public void reacquireControllers()
        {
            Debug.Assert(MainWindow.instance != null && !MainWindow.instance.InvokeRequired);

            // This method is called from the UI thread, either by the device-changed event handler or explicitly on app start.
            // The poll for button clicks call is from a helper thread and accesses the activeDevices list - potentially concurrently
            lock (activeDevices)
            {
                this.controllers = new List<ControllerData>();
                this.unacquireAndDisposeActiveJoysticks();
                ControllerConfigurationData controllerConfigurationData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());
                var assignedDevices = new HashSet<Guid>();

                this.knownControllers = controllerConfigurationData.devices.OrderBy(d => d.deviceName).ToList();

                // add the custom device if it's set
                if (customControllerGuid != Guid.Empty)
                {
                    try
                    {
                        addController(null, DeviceType.Joystick, customControllerGuid, true, false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to get custom device info: " + e.Message);
                    }
                }

                // Update assignments.
                controllerConfigurationData.devices.ForEach(controller => addController(controller.deviceName, controller.deviceType, controller.guid, false, false));
                foreach (ButtonAssignment assignment in buttonAssignments.Where(ba => ba.controller == null && ba.buttonIndex != -1 && !string.IsNullOrEmpty(ba.deviceGuid)))
                {
                    assignment.controller = controllers.FirstOrDefault(c => c.guid.ToString() == assignment.deviceGuid);
                }
                this.controllers = this.controllers.OrderBy(ctrl => ctrl.deviceName).ToList();

            }
        }

        public void addNetworkControllerToList()
        {
            if (controllers != null && !controllers.Contains(networkGamePad))
            {
                this.controllers.Add(networkGamePad);
                this.controllers = this.controllers.OrderBy(ctrl => ctrl.deviceName).ToList();
            }
        }

        public void removeNetworkControllerFromList()
        {
            if (controllers != null)
            {
                this.controllers.Remove(networkGamePad);
                this.controllers = this.controllers.OrderBy(ctrl => ctrl.deviceName).ToList();
            }
        }

        public Boolean assignButton(System.Windows.Forms.Form parent, int controllerIndex, int actionIndex)
        {
            return controllerIndex != -1 && controllerIndex < controllers.Count // Make sure device is connected.
                && getFirstReleasedButton(parent, controllers[controllerIndex], buttonAssignments[actionIndex]);
        }

        public Boolean assignButton(System.Windows.Forms.Form parent, int controllerIndex, ButtonAssignment buttonAssignment)
        {
            return controllerIndex != -1 && controllerIndex < controllers.Count // Make sure device is connected.
                && getFirstReleasedButton(parent, controllers[controllerIndex], buttonAssignment);
        }

        private Boolean getFirstReleasedButton(System.Windows.Forms.Form parent, ControllerData controllerData, ButtonAssignment buttonAssignment)
        {
            Boolean gotAssignment = false;
            if (controllerData.guid == UDP_NETWORK_CONTROLLER_GUID)
            {
                int assignedButton;
                if (CrewChief.gameDefinition.gameEnum == GameEnum.PCARS_NETWORK)
                {
                    PCarsUDPreader gameDataReader = (PCarsUDPreader)GameStateReaderFactory.getInstance().getGameStateReader(GameDefinition.pCarsNetwork);
                    assignedButton = gameDataReader.getButtonIndexForAssignment();
                }
                else
                {
                    PCars2UDPreader gameDataReader = (PCars2UDPreader)GameStateReaderFactory.getInstance().getGameStateReader(GameDefinition.pCars2Network);
                    assignedButton = gameDataReader.getButtonIndexForAssignment();
                }
                if (assignedButton != -1)
                {
                    removeAssignmentsForControllerAndButton(controllerData.guid, assignedButton);
                    buttonAssignment.controller = controllerData;
                    buttonAssignment.deviceGuid = controllerData.guid.ToString();
                    buttonAssignment.buttonIndex = assignedButton;
                    listenForAssignment = false;
                    gotAssignment = true;
                }
            }
            else
            {
                listenForAssignment = true;
                // Instantiate the joystick
                try
                {
                    Joystick joystick;
                    lock (activeDevices)
                    {
                        if (!activeDevices.TryGetValue(controllerData.guid, out joystick))
                        {
                            joystick = new Joystick(directInput, controllerData.guid);
                            // Acquire the joystick
                            joystick.SetCooperativeLevel(mainWindow.Handle, (CooperativeLevel.NonExclusive | CooperativeLevel.Background));
                            joystick.Properties.BufferSize = 128;
                            joystick.Acquire();
                            activeDevices.Add(controllerData.guid, joystick);
                        }
                    }
                    var pressedButtons = new List<int>();
                    var pressedPovIndexes = new List<int>();
                    var pressedPovValues = new List<int>();
                    bool allowPov = UserSettings.GetUserSettings().getBoolean("enable_controller_pov_switches");
                    while (listenForAssignment)
                    {
                        Boolean[] buttons = joystick.GetCurrentState().Buttons;
                        int[] pov = joystick.GetCurrentState().PointOfViewControllers;

                        // Collect currently pressed buttons:
                        for (int buttonIndex = 0; buttonIndex < buttons.Count(); ++buttonIndex)
                        {
                            if (buttons[buttonIndex]
                                && !pressedButtons.Contains(buttonIndex))
                            {
                                Console.WriteLine("Button pressed at index: " + buttonIndex);
                                pressedButtons.Add(buttonIndex);
                            }
                        }
                        if (allowPov)
                        {
                            for (int povIndex = 0; povIndex < pov.Count(); ++povIndex)
                            {
                                if (pov[povIndex] != -1
                                    && !pressedPovIndexes.Contains(povIndex))
                                {
                                    Console.WriteLine("PoV pressed at index: " + povIndex + " with raw value " + pov[povIndex]);
                                    pressedPovIndexes.Add(povIndex);
                                    pressedPovValues.Add(pov[povIndex]);
                                }
                            }
                        }
                        // See if any of the buttons got released:
                        foreach (var previouslyPressedButton in pressedButtons)
                        {
                            if (!gotAssignment
                                && !buttons[previouslyPressedButton])
                            {
                                Console.WriteLine("Button released at index: " + previouslyPressedButton);
                                removeAssignmentsForControllerAndButton(controllerData.guid, previouslyPressedButton);
                                buttonAssignment.controller = controllerData;
                                buttonAssignment.deviceGuid = controllerData.guid.ToString();
                                buttonAssignment.buttonIndex = previouslyPressedButton;
                                buttonAssignment.usePovData = false;
                                buttonAssignment.findEvent();
                                listenForAssignment = false;

                                gotAssignment = true;
                            }
                        }
                        if (allowPov)
                        {
                            foreach (var previouslyPressedPov in pressedPovIndexes)
                            {
                                if (!gotAssignment
                                    && pov[previouslyPressedPov] == -1)
                                {
                                    Console.WriteLine("Pov released at index: " + previouslyPressedPov);
                                    removeAssignmentsForControllerAndButton(controllerData.guid, previouslyPressedPov);
                                    buttonAssignment.controller = controllerData;
                                    buttonAssignment.deviceGuid = controllerData.guid.ToString();
                                    buttonAssignment.buttonIndex = previouslyPressedPov;
                                    buttonAssignment.usePovData = true;
                                    buttonAssignment.povValue = pressedPovValues[previouslyPressedPov];
                                    buttonAssignment.findEvent();
                                    listenForAssignment = false;

                                    gotAssignment = true;
                                }
                            }
                        }

                        if (!gotAssignment)
                        {
                            Thread.Sleep(20);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to acquire device " + controllerData.deviceName + " error: " + e.Message);
                    listenForAssignment = false;
                    gotAssignment = false;
                }
            }
            return gotAssignment;
        }

        private void removeAssignmentsForControllerAndButton(Guid controllerGuid, int buttonIndex)
        {
            foreach (ButtonAssignment ba in buttonAssignments.Where(ba => ba.controller != null && ba.controller.guid == controllerGuid && ba.buttonIndex == buttonIndex))
            {
                ba.unassign();
            }
        }

        public class ControllerData
        {
            [JsonIgnore]
            public static String PROPERTY_CONTAINER = "CONTROLLER_DATA";
            [JsonIgnore]
            public static String definitionSeparator = "CC_CD_SEPARATOR";
            [JsonIgnore]
            public static String elementSeparator = "CC_CE_SEPARATOR";

            public String deviceName;
            public DeviceType deviceType;
            public Guid guid;

            public static List<ControllerData> parse(String propValue)
            {
                List<ControllerData> definitionsList = new List<ControllerData>();

                if (propValue != null && propValue.Length > 0)
                {
                    String[] definitions = propValue.Split(new string[] { definitionSeparator }, StringSplitOptions.None);
                    foreach (String definition in definitions)
                    {
                        if (definition != null && definition.Length > 0)
                        {
                            try
                            {
                                String[] elements = definition.Split(new string[] { elementSeparator }, StringSplitOptions.None);
                                if (elements.Length == 3)
                                {
                                    definitionsList.Add(new ControllerData(elements[0], (DeviceType)System.Enum.Parse(typeof(DeviceType), elements[1]), new Guid(elements[2])));
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
                return definitionsList;
            }
            public ControllerData(String deviceName, DeviceType deviceType, Guid guid)
            {
                this.deviceName = deviceName;
                this.deviceType = deviceType;
                this.guid = guid;
            }
        }

        public class ButtonAssignment
        {
            public ButtonAssignment()
            {
                // action is the built-in action name, the SRE action name (key in the SRE config file) or one of the SRE
                // values from the SRE config file (e.g "get_session_status", "SESSION_STATUS", or "session status" will all do the same thing)
                action = string.Empty;
                deviceGuid = string.Empty;
                buttonIndex = -1;
                // uiText is optional and will be resolved from the ui_text file or from the SRE config if it's not in the ui_text
                uiText = null;
                availableAction = true;
            }
            public String action { get; set; }
            public String deviceGuid { get; set; }
            public int buttonIndex { get; set; }
            public bool usePovData { get; set; }
            public int povValue { get; set; }
            public bool opponentDataCommand { get; set; }
            public bool availableAction { get; set; }
            // used to override the default ui text for an action - is optional and generally not used much
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public String uiText { get; set; }

            // the ui text value that's been resolved for this action
            [JsonIgnore]
            public String resolvedUiText;
            // the first element in the SRE text list for this command
            [JsonIgnore]
            public String resolvedSRECommand;
            [JsonIgnore]
            public ControllerData controller;
            [JsonIgnore]
            public Boolean hasUnprocessedClick = false;
            [JsonIgnore]
            public Boolean wasPressedDown = false;
            [JsonIgnore]
            public DateTime clickTime = DateTime.MinValue;
            [JsonIgnore]
            public AbstractEvent actionEvent = null;

            [JsonIgnore]
            public ExecutableCommandMacro executableCommandMacro;
            public void Initialize()
            {
                findEvent();
                findUiText();
            }

            public void findEvent()
            {
                if (this.executableCommandMacro == null && this.action != null && !specialActions.ContainsKey(this.action))
                {
                    string[] srePhrases = Configuration.getSpeechRecognitionPhrases(this.action);
                    if (srePhrases != null && srePhrases.Length > 0)
                    {
                        this.actionEvent = SpeechRecogniser.getEventForAction(srePhrases[0]);
                        this.resolvedSRECommand = srePhrases[0];
                    }
                    else if (opponentDataCommand)
                    {
                        this.actionEvent = CrewChief.getEvent("Opponents");
                        this.resolvedSRECommand = this.action;
                    }
                    else
                    {
                        // final possibility, the action value is an actual SRE command
                        this.actionEvent = SpeechRecogniser.getEventForAction(action);
                        if (this.actionEvent != null)
                        {
                            this.resolvedSRECommand = this.action;
                        }
                        else
                        {
                            Console.WriteLine("No SRE key or value item for action " + action);
                        }
                    }
                }
            }

            private void findUiText()
            {
                if (string.IsNullOrEmpty(this.uiText))
                {
                    // no override for uitext so work out what it should be
                    this.resolvedUiText = Configuration.getUIStringStrict(action);
                    if (string.IsNullOrEmpty(this.resolvedUiText))
                    {
                        // nothing in the ui_text, use the resolved SRE command
                        this.resolvedUiText = resolvedSRECommand;
                    }
                    if (string.IsNullOrEmpty(this.resolvedUiText))
                    {
                        // if we get no hits at this point we'll use the action for the UI text
                        this.resolvedUiText = action;
                    }
                }
                else
                {
                    this.resolvedUiText = this.uiText;
                }
            }

            public bool execute()
            {
                if (actionEvent != null)
                {
                    actionEvent.respond(resolvedSRECommand);
                    return true;
                }
                if (executableCommandMacro != null)
                {
                    return executableCommandMacro.execute("", false, true);
                }
                return false;
            }

            public String getInfo()
            {
                if (controller != null && buttonIndex > -1)
                {
                    String name = controller.deviceName == null || controller.deviceName.Length == 0 ? controller.deviceType.ToString() : controller.deviceName;
                    string buttonName = usePovData ? Configuration.getUIString("POV") + " " + buttonIndex + " (" + getTextForPovValue() + ")"
                        : Configuration.getUIString("button") + ": " + buttonIndex;
                    return resolvedUiText + " " + Configuration.getUIString("assigned_to") + " " + name + ", " + buttonName;
                }
                else
                {
                    return resolvedUiText + " " + Configuration.getUIString("not_assigned");
                }
            }

            private string getTextForPovValue()
            {
                if (povValue.ToString().StartsWith("0"))
                {
                    return Configuration.getUIString("up");
                }
                if (povValue.ToString().StartsWith("180"))
                {
                    return Configuration.getUIString("down");
                }
                if (povValue.ToString().StartsWith("270"))
                {
                    return Configuration.getUIString("left");
                }
                if (povValue.ToString().StartsWith("90"))
                {
                    return Configuration.getUIString("right");
                }
                return povValue.ToString();
            }

            public void unassign()
            {
                this.controller = null;
                this.buttonIndex = -1;
                this.deviceGuid = string.Empty;
            }
        }

        public void cancelScan()
        {
            if (this.controllerScanThread != null)
            {
                this.controllerScanThread.Abort();
            }
        }
    }
}
