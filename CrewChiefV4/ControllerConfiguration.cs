using CrewChiefV4.PCars;
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
namespace CrewChiefV4
{
    public class ControllerConfiguration : IDisposable
    {
        private static Guid UDP_NETWORK_CONTROLLER_GUID = new Guid("2bbfed03-a04f-4408-91cf-e0aa6b20b8ff");

        private MainWindow mainWindow;
        public Boolean listenForAssignment = false;
        DirectInput directInput = new DirectInput();
        DeviceType[] supportedDeviceTypes = new DeviceType[] {DeviceType.Driving, DeviceType.Joystick, DeviceType.Gamepad, 
            DeviceType.Keyboard, DeviceType.ControlDevice, DeviceType.FirstPerson, DeviceType.Flight, 
            DeviceType.Supplemental, DeviceType.Remote};
        public List<ButtonAssignment> buttonAssignments = new List<ButtonAssignment>();
        public List<ControllerData> controllers;

        // keep track of all the Joystick devices we've 'acquired'
        private static Dictionary<Guid, Joystick> activeDevices = new Dictionary<Guid, Joystick>();
        // separate var for added custom controller
        private Guid customControllerGuid = Guid.Empty; 

        // built in controller button functions:
        public static String CHANNEL_OPEN_FUNCTION = "talk_to_crew_chief";
        public static String TOGGLE_RACE_UPDATES_FUNCTION = "toggle_race_updates_on/off";
        public static String TOGGLE_SPOTTER_FUNCTION = "toggle_spotter_on/off";
        public static String TOGGLE_READ_OPPONENT_DELTAS = "toggle_opponent_deltas_on/off_for_each_lap";
        public static String REPEAT_LAST_MESSAGE_BUTTON = "press_to_replay_the_last_message";
        public static String VOLUME_UP = "volume_up";
        public static String VOLUME_DOWN = "volume_down";
        public static String PRINT_TRACK_DATA ="print_track_data";
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

        // these are actions *not* handled by an AbstractEvent instance because of some batshit internal wiring that's impossible to unpick
        private static List<String> specialActions = new List<String>()
        {
            CHANNEL_OPEN_FUNCTION, TOGGLE_SPOTTER_FUNCTION, VOLUME_UP, VOLUME_DOWN
        };

        // this is a map of legacy action name (stuff like "CHANNEL_OPEN_FUNCTION") to new action name (stuff like "talk_to_crew_chief")
        // and is used to move old properties values over to the new JSON format on app start (when there's no user file for mappings)
        public static Dictionary<String, String> builtInActionMappings = new Dictionary<String, String>()
        {
            { GetParameterName(new { CHANNEL_OPEN_FUNCTION }), CHANNEL_OPEN_FUNCTION },
            { GetParameterName(new { TOGGLE_SPOTTER_FUNCTION }), TOGGLE_SPOTTER_FUNCTION },
            { GetParameterName(new { VOLUME_UP }), VOLUME_UP },
            { GetParameterName(new { VOLUME_DOWN }), VOLUME_DOWN },
            { GetParameterName(new { TOGGLE_RACE_UPDATES_FUNCTION }), TOGGLE_RACE_UPDATES_FUNCTION },
            { GetParameterName(new { TOGGLE_READ_OPPONENT_DELTAS }), TOGGLE_READ_OPPONENT_DELTAS },
            { GetParameterName(new { REPEAT_LAST_MESSAGE_BUTTON }), REPEAT_LAST_MESSAGE_BUTTON },
            { GetParameterName(new { PRINT_TRACK_DATA }), PRINT_TRACK_DATA },
            { GetParameterName(new { TOGGLE_YELLOW_FLAG_MESSAGES }), TOGGLE_YELLOW_FLAG_MESSAGES },
            { GetParameterName(new { GET_FUEL_STATUS }), GET_FUEL_STATUS },
            { GetParameterName(new { TOGGLE_MANUAL_FORMATION_LAP }), TOGGLE_MANUAL_FORMATION_LAP },
            { GetParameterName(new { READ_CORNER_NAMES_FOR_LAP }), READ_CORNER_NAMES_FOR_LAP },
            { GetParameterName(new { GET_CAR_STATUS }), GET_CAR_STATUS },
            { GetParameterName(new { GET_STATUS }), GET_STATUS },
            { GetParameterName(new { GET_SESSION_STATUS }), GET_SESSION_STATUS },
            { GetParameterName(new { GET_DAMAGE_REPORT }), GET_DAMAGE_REPORT },
            { GetParameterName(new { TOGGLE_PACE_NOTES_RECORDING }), TOGGLE_PACE_NOTES_RECORDING },
            { GetParameterName(new { TOGGLE_PACE_NOTES_PLAYBACK }), TOGGLE_PACE_NOTES_PLAYBACK },
            { GetParameterName(new { TOGGLE_TRACK_LANDMARKS_RECORDING }), TOGGLE_TRACK_LANDMARKS_RECORDING },
            { GetParameterName(new { TOGGLE_ENABLE_CUT_TRACK_WARNINGS }), TOGGLE_ENABLE_CUT_TRACK_WARNINGS },
            { GetParameterName(new { ADD_TRACK_LANDMARK }), ADD_TRACK_LANDMARK },
            { GetParameterName(new { PIT_PREDICTION }), PIT_PREDICTION },
            { GetParameterName(new { TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS }), TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS }
        };

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
            String path = System.IO.Path.Combine(Environment.GetFolderPath(
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
            if (filename != null)
            {
                try
                {
                    using (StreamReader r = new StreamReader(filename))
                    {
                        string json = r.ReadToEnd();
                        return JsonConvert.DeserializeObject<ControllerConfigurationData>(json);
                    }                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing " + filename + ": " + e.Message);
                }
            }
            return new ControllerConfigurationData();
        }

        private static void saveControllerConfigurationDataFile(ControllerConfigurationData buttonsActions)
        {
            String fileName = "controllerConfigurationData.json";
            String path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4");
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

        public static string GetParameterName<T>(T item) where T : class
        {
            if (item == null)
                return string.Empty;

            return item.ToString().TrimStart('{').TrimEnd('}').Split('=')[0].Trim();
        }

        public void Dispose()
        {
            mainWindow = null;
            unacquireAndDisposeActiveJoysticks();
            try
            {
                directInput.Dispose();
            }
            catch (Exception) { }
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
                    catch (Exception) { }
                    try
                    {
                        joystick.Dispose();
                    }
                    catch (Exception) { }
                }
                activeDevices.Clear();
            }
        }

        public ControllerConfiguration(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            
            // update existing data to use new json - copies assignments from old properties data into new json data, used when there's
            // no user controllers config json file
            if (getUserControllerConfigurationDataFileLocation() == null)
            {
                ControllerConfigurationData defaultData = getControllerConfigurationDataFromFile(getDefaultControllerConfigurationDataFileLocation());
                foreach (KeyValuePair<String, String> assignment in builtInActionMappings)
                {
                    int index = defaultData.buttonAssignments.FindIndex(ind => ind.action.Equals(assignment.Value));
                    if (index != -1)
                    {
                        int buttonIndex = UserSettings.GetUserSettings().getInt(assignment.Key + "_button_index");
                        String deviceGuid = UserSettings.GetUserSettings().getString(assignment.Key + "_device_guid");
                        if (buttonIndex != -1 && deviceGuid.Length > 0)
                        {
                            defaultData.buttonAssignments[index].buttonIndex = buttonIndex;
                            defaultData.buttonAssignments[index].deviceGuid = deviceGuid;
                        }
                    }                                      
                }
                defaultData.devices = ControllerData.parse(UserSettings.GetUserSettings().getString(ControllerData.PROPERTY_CONTAINER));
                saveControllerConfigurationDataFile(defaultData);
            }
            else // app updated add, missing elements ?
            {
                ControllerConfigurationData defaultData = getControllerConfigurationDataFromFile(getDefaultControllerConfigurationDataFileLocation());
                ControllerConfigurationData userData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());

                var missingItems = defaultData.buttonAssignments.Where(ba2 => userData.buttonAssignments.Any(ba1 => ba1.action == ba2.action) == false);
                if(missingItems.ToList().Count > 0)
                {
                    userData.buttonAssignments.AddRange(missingItems);                    
                }
                saveControllerConfigurationDataFile(userData);
            }
                       
            ControllerConfigurationData controllerConfigurationData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());
            // update actions and add assignments            
            buttonAssignments = controllerConfigurationData.buttonAssignments;
            controllers = controllerConfigurationData.devices;
            ButtonAssignment networkAssignment = buttonAssignments.SingleOrDefault(ba => ba.deviceGuid == UDP_NETWORK_CONTROLLER_GUID.ToString());
            if(networkAssignment != null)
            {
                addNetworkControllerToList();
            }            
            foreach (ButtonAssignment assignment in buttonAssignments)
            {
                ControllerData controller = controllers.SingleOrDefault(c => c.guid.ToString() == assignment.deviceGuid);
                assignment.Initialize(controller);
            }
        }

        // just sets the custom controller guid so the scan call will populate it later
        public void addCustomController(Guid guid)
        {
            customControllerGuid = guid;
        }

        public void pollForButtonClicks(Boolean channelOpenIsToggle)
        {
            foreach (var assignment in buttonAssignments)
            {
                pollForButtonClicks(assignment);
            }
        }

        private void pollForButtonClicks(ButtonAssignment ba)
        {
            if (ba != null && ba.buttonIndex != -1 && ba.controller != null && ba.controller.guid != Guid.Empty)
            {
                if (CrewChief.gameDefinition.gameEnum == GameEnum.PCARS_NETWORK)
                {
                    if (PCarsUDPreader.getButtonState(ba.buttonIndex))
                    {
                        ba.hasUnprocessedClick = true;
                    }
                }
                else if (CrewChief.gameDefinition.gameEnum == GameEnum.PCARS2_NETWORK)
                {
                    if (PCars2UDPreader.getButtonState(ba.buttonIndex))
                    {
                        ba.hasUnprocessedClick = true;
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
                                    Boolean click = state.Buttons[ba.buttonIndex];
                                    if (click)
                                    {
                                        ba.hasUnprocessedClick = true;
                                        
                                    }
                                }
                            }
                            catch (Exception)
                            { }
                        }
                    }
                }
            }
        }

        public Boolean hasOutstandingClick(String action = null)
        {
            if (action != null && (action == CHANNEL_OPEN_FUNCTION || 
                action == TOGGLE_SPOTTER_FUNCTION ||
                action == VOLUME_UP || action == VOLUME_DOWN))
            {
                ButtonAssignment ba = buttonAssignments.SingleOrDefault(ba1 => ba1.action == action);
                if (ba != null && ba.hasUnprocessedClick)
                {
                    ba.hasUnprocessedClick = false;
                    return true;
                }
                return false;                
            }
            else
            {
                foreach(var ba in buttonAssignments)
                {
                    if (ba.hasUnprocessedClick && ba.actionEvent != null)
                    {
                        ba.execute();
                        ba.hasUnprocessedClick = false;
                        return true;
                    }                    
                }
            }
            return false;
        }
        
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
            saveControllerConfigurationDataFile(new ControllerConfigurationData() { buttonAssignments = buttonAssignments, devices = controllers });
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
                                return joystick.GetCurrentState().Buttons[ba.buttonIndex];
                            }
                            catch
                            { }
                        }
                    }
                }
            }
            return false;
        }
        
        public void scanControllers(System.Windows.Forms.Form parent)
        {
            int availableCount = 0;
            this.controllers = new List<ControllerData>();

            // this method is called from the UI thread, either by the device-changed event handler or explicitly on app start.
            // The poll for button clicks call is from a different thread and accesses the activeDevices list - potentially concurrently
            lock (activeDevices)
            {
                // dispose all of our active devices:
                unacquireAndDisposeActiveJoysticks();
                // Iterate the list available, as reported by sharpDX
                foreach (DeviceType deviceType in supportedDeviceTypes)
                {
                    foreach (var deviceInstance in directInput.GetDevices(deviceType, DeviceEnumerationFlags.AllDevices))
                    {
                        Guid joystickGuid = deviceInstance.InstanceGuid;
                        if (joystickGuid != Guid.Empty)
                        {
                            try
                            {
                                addControllerFromScan(parent, deviceType, joystickGuid, false);
                                availableCount++;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to get device info: " + e.Message);
                            }
                        }
                    }
                }
                // add the custom device if it's set
                if (customControllerGuid != Guid.Empty)
                {
                    try
                    {
                        addControllerFromScan(parent, DeviceType.Joystick, customControllerGuid, true);
                        availableCount++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to get custom device info: " + e.Message);
                    }
                }
            }
            ControllerConfigurationData controllerConfigurationData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());
            // add controllers not in our saved list
            controllerConfigurationData.devices = controllers;
            saveControllerConfigurationDataFile(controllerConfigurationData);
            Console.WriteLine("Refreshed controllers, there are " + availableCount + " available controllers and " + activeDevices.Count + " active controllers");
        }

        private void addControllerFromScan(System.Windows.Forms.Form parent, DeviceType deviceType, Guid joystickGuid, Boolean isCustomDevice)
        {
            Boolean isMappedToAction = false;
            var joystick = new Joystick(directInput, joystickGuid);
            String productName = isCustomDevice ? Configuration.getUIString("custom_device") : deviceType.ToString();
            try
            {
                productName += ": " + joystick.Properties.ProductName;
            }
            catch (Exception)
            {
                // ignore - some devices don't have a product name
            }
            foreach (var ba in buttonAssignments.Where(b => b.controller != null && b.controller.guid == joystickGuid && b.buttonIndex != -1))
            {
                // if we have a button assigned to this device and it's not active, acquire it here:
                if (!activeDevices.ContainsKey(joystickGuid))
                {
                    joystick.SetCooperativeLevel(parent.Handle, (CooperativeLevel.NonExclusive | CooperativeLevel.Background));
                    joystick.Properties.BufferSize = 128;
                    joystick.Acquire();
                    activeDevices.Add(joystickGuid, joystick);
                }
                isMappedToAction = true;
            }
            controllers.Add(new ControllerData(productName, deviceType, joystickGuid));
            if (!isMappedToAction)
            {
                // we're not using this device so dispose the temporary handle we used to get its name
                try
                {
                    joystick.Dispose();
                }
                catch (Exception) { }
            }
        }
        
        public void addNetworkControllerToList()
        {
            if (!controllers.Contains(networkGamePad))
            {
                controllers.Add(networkGamePad);
            }
        }

        public void removeNetworkControllerFromList()
        {
            controllers.Remove(networkGamePad);
        }

        public Boolean assignButton(System.Windows.Forms.Form parent, int controllerIndex, int actionIndex)
        {
            return getFirstPressedButton(parent, controllers[controllerIndex], buttonAssignments[actionIndex]);
        }

        private Boolean getFirstPressedButton(System.Windows.Forms.Form parent, ControllerData controllerData, ButtonAssignment buttonAssignment)
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
                    if (!activeDevices.TryGetValue(controllerData.guid, out joystick))                    
                    {                        
                        joystick = new Joystick(directInput, controllerData.guid);
                        // Acquire the joystick
                        joystick.SetCooperativeLevel(parent.Handle, (CooperativeLevel.NonExclusive | CooperativeLevel.Background));
                        joystick.Properties.BufferSize = 128;
                        joystick.Acquire();
                        activeDevices.Add(controllerData.guid, joystick);
                    }
                    while (listenForAssignment)
                    {
                        Boolean[] buttons = joystick.GetCurrentState().Buttons;
                        for (int i = 0; i < buttons.Count(); i++)
                        {
                            if (buttons[i])
                            {
                                Console.WriteLine("Got button at index " + i);
                                removeAssignmentsForControllerAndButton(controllerData.guid, i);
                                buttonAssignment.controller = controllerData;
                                buttonAssignment.deviceGuid = controllerData.guid.ToString();
                                buttonAssignment.buttonIndex = i;
                                listenForAssignment = false;
                                gotAssignment = true;
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
            ButtonAssignment()
            {
                // action is either the built-in action name, or the SRE action name
                action = string.Empty;
                deviceGuid = string.Empty; 
                buttonIndex = -1;
                // uiText is optional and will be resolved from the ui_text file or from the SRE config if it's not in the ui_text
                uiText = string.Empty; 
            }
            public String action { get; set; }
            public String deviceGuid { get; set; }
            public int buttonIndex { get; set; }
            
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
            public AbstractEvent actionEvent = null;
            public void Initialize(ControllerData controller)
            {
                this.controller = controller;
                findEvent();
                findUiText();
            }

            private void findEvent()
            {
                if (this.action != null && !specialActions.Contains(this.action))
                {
                    string[] srePhrases = Configuration.getSpeechRecognitionPhrases(this.action);
                    if (srePhrases != null && srePhrases.Length > 0)
                    {
                        this.actionEvent = SpeechRecogniser.getEventForAction(srePhrases[0]);
                        this.resolvedSRECommand = srePhrases[0];
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

            public void execute()
            {
                if (actionEvent != null)
                {
                    actionEvent.respond(resolvedSRECommand);
                }
            }

            public String getInfo()
            {
                if (controller != null && buttonIndex > -1)
                {
                    String name = controller.deviceName == null || controller.deviceName.Length == 0 ? controller.deviceType.ToString() : controller.deviceName;
                    return resolvedUiText + " " + Configuration.getUIString("assigned_to") + " " + name + ", " + Configuration.getUIString("button") + ": " + buttonIndex;
                }
                else
                {
                    return resolvedUiText + " " + Configuration.getUIString("not_assigned");
                }
            }

            public void unassign()
            {
                this.controller = null;
                this.buttonIndex = -1;
                this.deviceGuid = string.Empty;
            }
        }
    }
}
