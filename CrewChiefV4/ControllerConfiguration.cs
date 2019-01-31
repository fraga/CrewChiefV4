using CrewChiefV4.PCars;
using CrewChiefV4.PCars2;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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

        public static string GetParameterName<T>(T item) where T : class
        {
            if (item == null)
                return string.Empty;

            return item.ToString().TrimStart('{').TrimEnd('}').Split('=')[0].Trim();
        }
        // yuk...
        public Dictionary<String, int> buttonAssignmentIndexes = new Dictionary<String, int>();
        private Thread asyncDisposeThread = null;

        public void Dispose()
        {
            mainWindow = null;
            foreach (ButtonAssignment ba in buttonAssignments)
            {
                if (ba.joystick != null)
                {
                    try
                    {
                        ba.joystick.Unacquire();
                        ba.joystick.Dispose();
                    }
                    catch (Exception) { }
                }
            }
            try
            {
                directInput.Dispose();
            }
            catch (Exception) { }
        }

        public ControllerConfiguration(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            foreach (KeyValuePair<String,String> assignment in assignmentNames)
            {
                addButtonAssignment(assignment.Value);
            }
            controllers = loadControllers();
        }

        public void addCustomController(Guid guid)
        {
            var joystick = new Joystick(directInput, guid);
            String productName = " " + Configuration.getUIString("custom_device");
            try
            {
                productName = ": " + joystick.Properties.ProductName;
            }
            catch (Exception)
            {
            }
            asyncDispose(DeviceType.ControlDevice, joystick);
            controllers.Add(new ControllerData(productName, DeviceType.Joystick, guid));
        }

        public void pollForButtonClicks(Boolean channelOpenIsToggle)
        {
            foreach (KeyValuePair<String, String> assignment in assignmentNames)
            {
                pollForButtonClicks(buttonAssignments[buttonAssignmentIndexes[assignment.Value]]);
            }            
        }

        private void pollForButtonClicks(ButtonAssignment ba)
        {
            if (ba != null && ba.buttonIndex != -1)
            {
                if (ba.joystick != null)
                {
                    try
                    {
                        if (ba.joystick != null)
                        {
                            JoystickState state = ba.joystick.GetCurrentState();
                            if (state != null)
                            {
                                Boolean click = state.Buttons[ba.buttonIndex];
                                if (click)
                                {
                                    ba.hasUnprocessedClick = true;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                else if (ba.controller.guid == UDP_NETWORK_CONTROLLER_GUID)
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
                }
            }
        }

        public Boolean hasOutstandingClick(String action)
        {
            ButtonAssignment ba = buttonAssignments[buttonAssignmentIndexes[action]];
            if (ba.hasUnprocessedClick)
            {
                ba.hasUnprocessedClick = false;
                return true;
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
                    (buttonAssignment.joystick != null || (buttonAssignment.controller != null && buttonAssignment.controller.guid == UDP_NETWORK_CONTROLLER_GUID)) 
                    && buttonAssignment.buttonIndex != -1)
                {
                    return true;
                }
            }
            return false;     
        }
        
        public void saveSettings()
        {
            foreach (ButtonAssignment buttonAssignment in buttonAssignments)
            {
                String actionId = "";
                foreach (KeyValuePair<String, String> assignment in assignmentNames)
                {
                    if (buttonAssignment.action == assignment.Value)
                    {
                        actionId = assignment.Key;
                    }
                }
                if (actionId != "" && buttonAssignment.controller != null && (buttonAssignment.joystick != null || buttonAssignment.controller.guid == UDP_NETWORK_CONTROLLER_GUID) && buttonAssignment.buttonIndex != -1)
                {
                    UserSettings.GetUserSettings().setProperty(actionId + "_button_index", buttonAssignment.buttonIndex);
                    UserSettings.GetUserSettings().setProperty(actionId + "_device_guid", buttonAssignment.controller.guid.ToString());
                }
                else if (actionId != "")
                {
                    UserSettings.GetUserSettings().setProperty(actionId + "_button_index", -1);
                    UserSettings.GetUserSettings().setProperty(actionId + "_device_guid", "");
                }
            }
            UserSettings.GetUserSettings().saveUserSettings();
        }

        public void loadSettings(System.Windows.Forms.Form parent)
        {            
            foreach (KeyValuePair<String, String> assignment in assignmentNames)
            {
                int buttonIndex = UserSettings.GetUserSettings().getInt(assignment.Key + "_button_index");
                String deviceGuid = UserSettings.GetUserSettings().getString(assignment.Key + "_device_guid");
                if (buttonIndex != -1 && deviceGuid.Length > 0)
                {
                    loadAssignment(parent, assignment.Value, buttonIndex, deviceGuid);
                }
            }
        }

        private void loadAssignment(System.Windows.Forms.Form parent, String functionName, int buttonIndex, String deviceGuid)
        {
            if (deviceGuid == UDP_NETWORK_CONTROLLER_GUID.ToString())
            {
                addNetworkControllerToList();
            }
            List<ControllerData> missingControllers = new List<ControllerData>();
            foreach (ControllerData controller in this.controllers)
            {                
                if (controller.guid.ToString() == deviceGuid)
                {
                    buttonAssignments[buttonAssignmentIndexes[functionName]].controller = controller;
                    buttonAssignments[buttonAssignmentIndexes[functionName]].buttonIndex = buttonIndex;
                    if (controller.guid != UDP_NETWORK_CONTROLLER_GUID)
                    {
                        try
                        {
                            var joystick = new Joystick(directInput, controller.guid);
                            // Acquire the joystick
                            joystick.SetCooperativeLevel(parent.Handle, (CooperativeLevel.NonExclusive | CooperativeLevel.Background));
                            joystick.Properties.BufferSize = 128;
                            joystick.Acquire();
                            buttonAssignments[buttonAssignmentIndexes[functionName]].joystick = joystick;
                        }
                        catch (Exception e)
                        {
                            missingControllers.Add(controller);
                            Console.WriteLine("Controller " + controller.deviceName + " is not available: " + e.Message);
                        }
                    }
                }
            }
            Boolean removedMissingController = false;
            foreach (ControllerData controllerData in missingControllers) {
                if (missingControllers.Contains(controllerData))
                {
                    removedMissingController = true;
                    this.controllers.Remove(controllerData);
                }
            }
            if (removedMissingController)
            {
                this.mainWindow.getControllers();
            }
        }

        private void addButtonAssignment(String action)
        {
            buttonAssignmentIndexes.Add(action, buttonAssignmentIndexes.Count());
            buttonAssignments.Add(new ButtonAssignment(action));
        }

        public Boolean isChannelOpen()
        {
            ButtonAssignment ba = buttonAssignments[buttonAssignmentIndexes[CHANNEL_OPEN_FUNCTION]];
            if (ba != null && ba.buttonIndex != -1)
            {
                if (ba.joystick != null)
                {
                    try
                    {
                        return ba.joystick.GetCurrentState().Buttons[ba.buttonIndex];
                    }
                    catch
                    {
                        
                    }
                }
                else if (ba.controller.guid == UDP_NETWORK_CONTROLLER_GUID && CrewChief.gameDefinition.gameEnum == GameEnum.PCARS_NETWORK)
                {
                    return PCarsUDPreader.getButtonState(ba.buttonIndex);
                }
                else if (ba.controller.guid == UDP_NETWORK_CONTROLLER_GUID && CrewChief.gameDefinition.gameEnum == GameEnum.PCARS2_NETWORK)
                {
                    return PCars2UDPreader.getButtonState(ba.buttonIndex);
                }
            }
            return false;
        }

        public List<ControllerData> loadControllers()
        {
            return ControllerData.parse(UserSettings.GetUserSettings().getString(ControllerData.PROPERTY_CONTAINER));
        }
        
        public List<ControllerData> scanControllers(System.Windows.Forms.Form parent)
        {
            List<ControllerData> controllers = new List<ControllerData>(); 
            foreach (DeviceType deviceType in supportedDeviceTypes)
            {
                foreach (var deviceInstance in directInput.GetDevices(deviceType, DeviceEnumerationFlags.AllDevices))
                {
                    Guid joystickGuid = deviceInstance.InstanceGuid;
                    if (joystickGuid != Guid.Empty) 
                    {
                        try
                        {
                            var joystick = new Joystick(directInput, joystickGuid);
                            String productName = "";
                            try
                            {
                                productName = ": " + joystick.Properties.ProductName;
                            }
                            catch (Exception)
                            {
                                // ignore - some devices don't have a product name
                            }
                            asyncDispose(deviceType, joystick);
                            controllers.Add(new ControllerData(productName, deviceType, joystickGuid));
                            ReassignButtonsIfRequired(parent, deviceInstance);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to get device info: " + e.Message);
                        }
                    }
                }
            }
            UnassignButtonsIfRequired();
            String propVal = ControllerData.createPropValue(controllers);
            UserSettings.GetUserSettings().setProperty(ControllerData.PROPERTY_CONTAINER, propVal);
            UserSettings.GetUserSettings().saveUserSettings();
            return controllers;
        }

        private void UnassignButtonsIfRequired()
        {
            foreach (var ba in buttonAssignments.Where(b => b.controller != null && b.joystick != null && !directInput.IsDeviceAttached(b.controller.guid)))
            {
                ba.joystick.Unacquire();
                ba.joystick.Dispose();
                ba.joystick = null;
            }
        }

        private void ReassignButtonsIfRequired(System.Windows.Forms.Form parent, DeviceInstance deviceInstance)
        {
            var joystickGuid = deviceInstance.InstanceGuid;
            foreach (var ba in buttonAssignments.Where(b => b.controller != null && b.controller.guid == joystickGuid && b.buttonIndex != -1))
            {
                var joystick = new Joystick(directInput, ba.controller.guid);
                joystick.SetCooperativeLevel(parent.Handle, (CooperativeLevel.NonExclusive | CooperativeLevel.Background));
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
                ba.joystick = joystick;
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
                    var joystick = new Joystick(directInput, controllerData.guid);
                    // Acquire the joystick
                    joystick.SetCooperativeLevel(parent.Handle, (CooperativeLevel.NonExclusive | CooperativeLevel.Background));
                    joystick.Properties.BufferSize = 128;
                    joystick.Acquire();
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
                                buttonAssignment.joystick = joystick;
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
                    if (!gotAssignment)
                    {
                        joystick.Unacquire();
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
                    ba.joystick = null; // unacquire here?
                    ba.buttonIndex = -1;
                }
            }
        }

        private void asyncDispose(DeviceType deviceType, Joystick joystick)
        {
            ThreadManager.UnregisterTemporaryThread(asyncDisposeThread);
            asyncDisposeThread = new Thread(() =>
            {
                DateTime now = DateTime.UtcNow;
                Thread.CurrentThread.IsBackground = true;
                String name = joystick.Information.InstanceName;
                try
                {                    
                    joystick.Dispose();
                    //Console.WriteLine("Disposed of temporary " + deviceType + " object " + name + " after " + (DateTime.UtcNow - now).TotalSeconds + " seconds");
                }
                catch (Exception e) { 
                    //log and swallow 
                    Console.WriteLine("Failed to dispose of temporary " + deviceType + " object " + name + "after " + (DateTime.UtcNow - now).TotalSeconds + " seconds: " + e.Message);
                }
            });
            asyncDisposeThread.Name = "ControllerConfiguration.asyncDisposeThread";
            ThreadManager.RegisterTemporaryThread(asyncDisposeThread);
            asyncDisposeThread.Start();
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
            public ControllerData controller;
            public int buttonIndex = -1;
            public Joystick joystick;
            public Boolean hasUnprocessedClick = false;
            public ButtonAssignment(String action)
            {
                this.action = action;
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
                if (this.joystick != null)
                {
                    this.joystick.Unacquire();
                    this.joystick.SetNotification(null);
                }
                this.joystick = null;
            }
        }
    }
}
