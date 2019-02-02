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

        public static String CHANNEL_OPEN_FUNCTION = Configuration.getUIString("talk_to_crew_chief");
        public static String TOGGLE_RACE_UPDATES_FUNCTION = Configuration.getUIString("toggle_race_updates_on/off");
        public static String TOGGLE_SPOTTER_FUNCTION = Configuration.getUIString("toggle_spotter_on/off");
        public static String TOGGLE_READ_OPPONENT_DELTAS = Configuration.getUIString("toggle_opponent_deltas_on/off_for_each_lap");
        public static String REPEAT_LAST_MESSAGE_BUTTON = Configuration.getUIString("press_to_replay_the_last_message");
        public static String VOLUME_UP = Configuration.getUIString("volume_up");
        public static String VOLUME_DOWN = Configuration.getUIString("volume_down");
        public static String PRINT_TRACK_DATA = Configuration.getUIString("print_track_data");
        public static String TOGGLE_YELLOW_FLAG_MESSAGES = Configuration.getUIString("toggle_yellow_flag_messages");
        public static String GET_FUEL_STATUS = Configuration.getUIString("get_fuel_status");
        public static String TOGGLE_MANUAL_FORMATION_LAP = Configuration.getUIString("toggle_manual_formation_lap");
        public static String READ_CORNER_NAMES_FOR_LAP = Configuration.getUIString("read_corner_names_for_lap");

        public static String GET_CAR_STATUS = Configuration.getUIString("get_car_status");
        public static String GET_STATUS = Configuration.getUIString("get_status");
        public static String GET_SESSION_STATUS = Configuration.getUIString("get_session_status");
        public static String GET_DAMAGE_REPORT = Configuration.getUIString("get_damage_report");
                
        public static String TOGGLE_PACE_NOTES_RECORDING = Configuration.getUIString("toggle_pace_notes_recording");
        public static String TOGGLE_PACE_NOTES_PLAYBACK = Configuration.getUIString("toggle_pace_notes_playback");

        public static String TOGGLE_TRACK_LANDMARKS_RECORDING = Configuration.getUIString("toggle_track_landmarks_recording");
        public static String TOGGLE_ENABLE_CUT_TRACK_WARNINGS = Configuration.getUIString("toggle_enable_cut_track_warnings");
        
        public static String ADD_TRACK_LANDMARK = Configuration.getUIString("add_track_landmark");

        public static String PIT_PREDICTION = Configuration.getUIString("activate_pit_prediction");

        public static String TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS = Configuration.getUIString("toggle_delay_messages_in_hard_parts");

        private ControllerData networkGamePad = new ControllerData(Configuration.getUIString("udp_network_data_buttons"), DeviceType.Gamepad, UDP_NETWORK_CONTROLLER_GUID);

        // yuk...
        public Dictionary<String, int> buttonAssignmentIndexes = new Dictionary<String, int>();
        private Thread asyncDisposeThread = null;

        private static Dictionary<String, String> assignmentNames = new Dictionary<String, String>()
        {
            { GetParameterName(new { CHANNEL_OPEN_FUNCTION }), CHANNEL_OPEN_FUNCTION},
            { GetParameterName(new { TOGGLE_RACE_UPDATES_FUNCTION }), TOGGLE_RACE_UPDATES_FUNCTION},
            { GetParameterName(new { TOGGLE_SPOTTER_FUNCTION }), TOGGLE_SPOTTER_FUNCTION},
            { GetParameterName(new { TOGGLE_READ_OPPONENT_DELTAS }), TOGGLE_READ_OPPONENT_DELTAS},
            { GetParameterName(new { REPEAT_LAST_MESSAGE_BUTTON }), REPEAT_LAST_MESSAGE_BUTTON},
            { GetParameterName(new { VOLUME_UP }), VOLUME_UP},
            { GetParameterName(new { VOLUME_DOWN }), VOLUME_DOWN},
            { GetParameterName(new { PRINT_TRACK_DATA }), PRINT_TRACK_DATA},
            { GetParameterName(new { TOGGLE_YELLOW_FLAG_MESSAGES }), TOGGLE_YELLOW_FLAG_MESSAGES},
            { GetParameterName(new { GET_FUEL_STATUS }), GET_FUEL_STATUS},
            { GetParameterName(new { TOGGLE_MANUAL_FORMATION_LAP }), TOGGLE_MANUAL_FORMATION_LAP},
            { GetParameterName(new { READ_CORNER_NAMES_FOR_LAP }), READ_CORNER_NAMES_FOR_LAP},            
            { GetParameterName(new { GET_CAR_STATUS }), GET_CAR_STATUS},
            { GetParameterName(new { GET_STATUS }), GET_STATUS},
            { GetParameterName(new { GET_SESSION_STATUS }), GET_SESSION_STATUS},
            { GetParameterName(new { GET_DAMAGE_REPORT }), GET_DAMAGE_REPORT},
            { GetParameterName(new { TOGGLE_PACE_NOTES_RECORDING }), TOGGLE_PACE_NOTES_RECORDING},
            { GetParameterName(new { TOGGLE_PACE_NOTES_PLAYBACK }), TOGGLE_PACE_NOTES_PLAYBACK},
            { GetParameterName(new { TOGGLE_TRACK_LANDMARKS_RECORDING }), TOGGLE_TRACK_LANDMARKS_RECORDING},
            { GetParameterName(new { TOGGLE_ENABLE_CUT_TRACK_WARNINGS }), TOGGLE_ENABLE_CUT_TRACK_WARNINGS},
            { GetParameterName(new { ADD_TRACK_LANDMARK }), ADD_TRACK_LANDMARK},
            { GetParameterName(new { PIT_PREDICTION }), PIT_PREDICTION},
            { GetParameterName(new { TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS }), TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS}
        };
        public class ControllerConfigurationDevice
        {
            public int deviceType { get; set; }
            public String productName { get; set; }
            public String guid { get; set; }
        }
        public class ButtonAssignmentData
        {
            public ButtonAssignmentData()
            {
                action = new string[]{string.Empty};
                uiText = string.Empty;
                eventName = string.Empty;
                buttonIndex = -1;
                deviceGuid = string.Empty;
            }
            public String[] action { get; set; }
            public String uiText { get; set; }
            public String eventName { get; set; }
            public int buttonIndex { get; set; }
            public String deviceGuid { get; set; }
        }
        public class ControllerConfigurationData
        {
            public ControllerConfigurationData()
            {
                devices = new List<ControllerConfigurationDevice>();
                buttonAssignments = new List<ButtonAssignmentData>();
            }
            public List<ControllerConfigurationDevice> devices { get; set; }
            public List<ButtonAssignmentData> buttonAssignments { get; set; }
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
            
            // update existing data to use new json 
            if(getUserControllerConfigurationDataFileLocation() == null)
            {
                ControllerConfigurationData defaultData = getControllerConfigurationDataFromFile(getDefaultControllerConfigurationDataFileLocation());
                foreach (KeyValuePair<String, String> assignment in assignmentNames)
                {
                    int index = defaultData.buttonAssignments.FindIndex(ind => Configuration.getUIString(ind.uiText).Equals(assignment.Value));
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
                for (int i = 0; i < defaultData.buttonAssignments.Count; i++)
                {
                    String uiText = defaultData.buttonAssignments[i].uiText;
                    defaultData.buttonAssignments[i].action = Configuration.getUIStringStrict(uiText) == null ? Configuration.getSpeechRecognitionPhrases(uiText) : new string[] { Configuration.getUIString(uiText) };
                }
                List<ControllerData> currentControllers = ControllerData.parse(UserSettings.GetUserSettings().getString(ControllerData.PROPERTY_CONTAINER));
                foreach (var controller in currentControllers)
                {
                    ControllerConfigurationDevice deviceData = new ControllerConfigurationDevice();
                    deviceData.deviceType = (int)controller.deviceType;
                    deviceData.guid = controller.guid.ToString();
                    deviceData.productName = controller.deviceName;
                    defaultData.devices.Add(deviceData);                    
                }
                saveControllerConfigurationDataFile(defaultData);
            }
            else // app updated add, missing elements ?
            {
                ControllerConfigurationData defaultData = getControllerConfigurationDataFromFile(getDefaultControllerConfigurationDataFileLocation());
                ControllerConfigurationData userData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());

                var missingItems = defaultData.buttonAssignments.Where(ba2 => userData.buttonAssignments.Any(ba1 => ba1.uiText == ba2.uiText) == false)
                    .Select(ba => new ButtonAssignmentData
                    { 
                        uiText = ba.uiText,
                        eventName = ba.eventName,
                        action = Configuration.getUIStringStrict(ba.uiText) == null ? Configuration.getSpeechRecognitionPhrases(ba.uiText) : new string[] { Configuration.getUIString(ba.uiText) }
                    });
                if(missingItems.ToList().Count > 0)
                {
                    userData.buttonAssignments.AddRange(missingItems);                    
                }
                saveControllerConfigurationDataFile(userData);
            }
                       
            ControllerConfigurationData controllerConfigurationData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());
            for  (int i = 0; i < controllerConfigurationData.buttonAssignments.Count; i++)
            {
                String uiText = controllerConfigurationData.buttonAssignments[i].uiText;
                controllerConfigurationData.buttonAssignments[i].action = Configuration.getUIStringStrict(uiText) == null ? Configuration.getSpeechRecognitionPhrases(uiText) : new string[] { Configuration.getUIString(uiText) };
                saveControllerConfigurationDataFile(controllerConfigurationData);
                addButtonAssignment(controllerConfigurationData.buttonAssignments[i].action[0], controllerConfigurationData.buttonAssignments[i].eventName);
            }

            controllers = new List<ControllerData>();
            foreach (var device in controllerConfigurationData.devices)
            {
                controllers.Add(new ControllerData(device.productName, (DeviceType)device.deviceType, new Guid(device.guid)));
            }
            foreach (ButtonAssignmentData assignment in controllerConfigurationData.buttonAssignments)
            {
                if (assignment.buttonIndex != -1 && assignment.deviceGuid.Length > 0)
                {
                    loadAssignment(assignment.action[0], assignment.buttonIndex, assignment.deviceGuid);
                }
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
                pollForButtonClicks(buttonAssignments[buttonAssignmentIndexes[assignment.action]]);
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
                ButtonAssignment ba = buttonAssignments[buttonAssignmentIndexes[action]];
                if (ba.hasUnprocessedClick)
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
                    if (ba.hasUnprocessedClick && ba.eventName != string.Empty)
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
            ControllerConfigurationData controllerData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());
            foreach (ButtonAssignment buttonAssignment in buttonAssignments)
            {
                int index = controllerData.buttonAssignments.FindIndex(ind => ind.action[0].Equals(buttonAssignment.action));
                if (index != -1 && buttonAssignment.controller != null && (buttonAssignment.controller != null || buttonAssignment.controller.guid == UDP_NETWORK_CONTROLLER_GUID) && buttonAssignment.buttonIndex != -1)
                {
                    controllerData.buttonAssignments[index].buttonIndex = buttonAssignment.buttonIndex;
                    controllerData.buttonAssignments[index].deviceGuid = buttonAssignment.controller.guid.ToString();
                }
                else if (index != -1)
                {
                    controllerData.buttonAssignments[index].buttonIndex = -1;
                    controllerData.buttonAssignments[index].deviceGuid = string.Empty;
                }
            }
            saveControllerConfigurationDataFile(controllerData);
        }

        public void loadSettings(System.Windows.Forms.Form parent)
        {
            ControllerConfigurationData controllerData = getControllerConfigurationDataFromFile(getUserControllerConfigurationDataFileLocation());
            foreach (ButtonAssignmentData assignment in controllerData.buttonAssignments)
            {
                int buttonIndex = assignment.buttonIndex;
                String deviceGuid = assignment.deviceGuid;
                if (buttonIndex != -1 && deviceGuid.Length > 0)
                {
                    loadAssignment(assignment.action[0], buttonIndex, deviceGuid);
                }
            }
        }

        private void loadAssignment(String functionName, int buttonIndex, String deviceGuid)
        {
            if (deviceGuid == UDP_NETWORK_CONTROLLER_GUID.ToString())
            {
                addNetworkControllerToList();
            }
            foreach (ControllerData controller in this.controllers)
            {                
                if (controller.guid.ToString() == deviceGuid)
                {
                    buttonAssignments[buttonAssignmentIndexes[functionName]].controller = controller;
                    buttonAssignments[buttonAssignmentIndexes[functionName]].buttonIndex = buttonIndex;                    
                }
            }
        }

        private void addButtonAssignment(String action, String eventName)
        {
            buttonAssignmentIndexes.Add(action, buttonAssignmentIndexes.Count());
            buttonAssignments.Add(new ButtonAssignment(action, eventName));
        }

        public Boolean isChannelOpen()
        {
            ButtonAssignment ba = buttonAssignments[buttonAssignmentIndexes[CHANNEL_OPEN_FUNCTION]];
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
            foreach (var controller in controllers)
            {
                if (!controllerConfigurationData.devices.Exists(c => c.guid == controller.guid.ToString()))
                {
                     ControllerConfigurationDevice deviceData = new ControllerConfigurationDevice();
                     deviceData.deviceType = (int)controller.deviceType;
                     deviceData.guid = controller.guid.ToString();
                     deviceData.productName = controller.deviceName;
                     controllerConfigurationData.devices.Add(deviceData);
                }
            }
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
            foreach (ButtonAssignment ba in buttonAssignments)
            {
                if (ba.controller != null && ba.controller.guid == controllerGuid && ba.buttonIndex == buttonIndex)
                {
                    ba.controller = null;
                    ba.buttonIndex = -1;
                }
            }
        }

        public class ControllerData
        {
            public static String PROPERTY_CONTAINER = "CONTROLLER_DATA";
            public static String definitionSeparator = "CC_CD_SEPARATOR";
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

            public static String createPropValue(List<ControllerData> definitions)
            {
                StringBuilder propVal = new StringBuilder();
                foreach (ControllerData def in definitions)
                {
                    propVal.Append(def.deviceName).Append(elementSeparator).Append(def.deviceType.ToString()).Append(elementSeparator).
                            Append(def.guid.ToString()).Append(definitionSeparator);
                }
                return propVal.ToString();
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
            public String action;
            public String eventName;
            public ControllerData controller;
            public int buttonIndex = -1;
            public Boolean hasUnprocessedClick = false;
            AbstractEvent actionEvent = null;
            public ButtonAssignment(String action, String eventName)
            {
                this.action = action;
                this.eventName = eventName;
                
                if(eventName != string.Empty)
                {
                    actionEvent = CrewChief.getEvent(eventName);
                    
                }
            }
            public void execute()
            {
                if(actionEvent != null)
                {
                    actionEvent.respond(action);
                }
            }
            public String getInfo()
            {
                if (controller != null && buttonIndex > -1)
                {
                    String name = controller.deviceName == null || controller.deviceName.Length == 0 ? controller.deviceType.ToString() : controller.deviceName;
                    return action + " " + Configuration.getUIString("assigned_to") + " " + name + ", " + Configuration.getUIString("button") + ": " + buttonIndex;
                }
                else
                {
                    return action + " " + Configuration.getUIString("not_assigned");
                }
            }

            public void unassign()
            {
                this.controller = null;
                this.buttonIndex = -1;
            }
        }
    }
}
