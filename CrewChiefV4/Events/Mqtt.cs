using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CrewChiefV4.Audio;
using CrewChiefV4.GameState;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrewChiefV4.Events
{
    class Mqtt : AbstractEvent
    {
        private static MqttFactory mqttFactory;
        private static IMqttClient mqttClient;
        private static int CONNECT_DISCONNECT_TIMEOUT = 5000;
        private static int SUBSCRIBE_TIMEOUT = 5000;
        // internal Dictionary<string, string> previousValues = new Dictionary<string, string>();
        private int updateSkipCounter = 0;
        private int updateRateLimit = 0; // setting this to 1 would mean we only update every 2nd tick, ie n+1 ticks
        private string subscribeTopic;
        private string driverName;
        private bool enabled;
        private string server;
        private string topic;
        private string login;
        private string password;
        private int port;
        private List<DataItem> dataItems;

        // a static method that removes invalid characters from a string
        private static string SanitizeForTopic(string input)
        {
            // https://github.com/dotnet/MQTTnet/blob/73d681bc1f978c4e6cf03266fcd1fb4a30a5d205/Source/MQTTnet/Protocol/MqttTopicValidator.cs#L22-L41
            // replace + and # with empty string in input

            return Regex.Replace(input, @"[\+#/]", "");
        }

        public Mqtt(AudioPlayer audioPlayer)
        {
            // TODO how to figure out if we're initialized by "Start" button or by starting the App?
            //      I think we only need to do all the bootstrapping on "Start"
            //      and are events also initialized when a session changes? Like when we go from qualifying to race?
            //      For now just connect everytime, it's not costly, it just opens a TCP connection.
            enabled = UserSettings.GetUserSettings().getBoolean("enable_mqtt_telemetry");

            if (!enabled)
                return;

            driverName = UserSettings.GetUserSettings().getString("mqtt_topic_drivername");
            driverName = Regex.Replace(driverName, @"[^\w\d]", " ");
            driverName = Regex.Replace(driverName, @"\s+", " ");
            driverName = driverName.Trim();
            driverName = SanitizeForTopic(driverName);
            if (driverName == string.Empty)
                driverName = "invalid drivername";

            this.audioPlayer = audioPlayer;
            this.CreateMQTTClient();
        }

        internal void CreateMQTTClient()
        {
            loadConfig();

            if (Mqtt.mqttFactory == null)
            {
                Mqtt.mqttFactory = new MqttFactory();
                Mqtt.mqttClient = Mqtt.mqttFactory.CreateMqttClient();
            }

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(server, port)
                .WithCredentials(login, password)
                .Build();

            Task.Run(() =>
            {
                try
                {
                    if (Mqtt.mqttClient.IsConnected)
                    {
                        Log.Debug("Disconnect from MQTT server");
                        // timeout the disconnect task after 5 seconds
                        if (!Mqtt.mqttClient.DisconnectAsync().Wait(Mqtt.CONNECT_DISCONNECT_TIMEOUT))
                        {
                            Log.Warning($"MQTT disconnect task timed out after {Mqtt.CONNECT_DISCONNECT_TIMEOUT} milliseconds");
                        }
                    }
                    // timeout the connect task after 5 seconds
                    if (Mqtt.mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None).Wait(Mqtt.CONNECT_DISCONNECT_TIMEOUT))
                    {
                        Console.WriteLine($"Connected to MQTT server {server}:{port} as {login}");
                        SubscribeClient();
                    }
                    else
                    {
                        Log.Error($"MQTT connect task timed out after {Mqtt.CONNECT_DISCONNECT_TIMEOUT} milliseconds");
                    }
                }
                catch (AggregateException ex)
                {
                    Log.Error($"MQTT: Failed to connect to the {server}:{port} - " + ex.InnerException.Message);
                }
                if (Mqtt.mqttClient.IsConnected)
                    updateSkipCounter = 0;
            });
        }

        private void playMessage(string text, float distance, int priority)
        {

            var fragment = MessageFragment.Text(text);
            fragment.allowTTS = true;
            string messageName = $"mqtt_response_{text}_{distance}";
            // Console.WriteLine($"MQTT: playMessage {messageName}");
            var message = new QueuedMessage(messageName, 10,
                              messageFragments: MessageContents(fragment),
                              abstractEvent: this, type: SoundType.REGULAR_MESSAGE, priority: priority,
                              triggerFunction: (GameStateData gsd) =>
                                 distance < 0 || Math.Abs(gsd.PositionAndMotionData.DistanceRoundTrack - distance) < 1
                              );
            audioPlayer.playMessage(message);
        }

        private void SubscribeClient()
        {
            // this method is called async in a task
            if (String.IsNullOrEmpty(subscribeTopic))
                return;

            Mqtt.mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                try
                {
                    string response = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    string message = response;
                    float distance = -1;
                    int priority = SoundMetadata.DEFAULT_PRIORITY;
                    if (response.StartsWith("{"))
                    {
                        JObject json = JObject.Parse(response);
                        if (json.ContainsKey("message"))
                        {
                            message = json.GetValue("message").ToString();
                        }
                        if (json.ContainsKey("distance"))
                        {
                            distance = json.GetValue("distance").ToObject<float>();
                        }
                        if (json.ContainsKey("priority"))
                        {
                            priority = json.GetValue("priority").ToObject<int>();
                        }
                    }
                    else
                    {
                        response = Regex.Replace(response, @"[^\w\d\.]", " ");
                        if (response.Length > 256) { response = response.Substring(0, 256); }
                        message = response;
                    }
                    playMessage(message, distance, priority);
                }
                catch (AggregateException ex)
                {
                    Log.Error("Failed to parse response: " + ex.InnerException.Message);
                }
            });

            try
            {
                var responseTopic = subscribeTopic + "/" + driverName;
                // timeout the subscribe task after 1 second
                if (Mqtt.mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(responseTopic).Build()).Wait(Mqtt.SUBSCRIBE_TIMEOUT))
                {
                    Console.WriteLine($"Subscribed to MQTT response topic {responseTopic}");
                }
                else
                {
                    Log.Error($"MQTT subscribe task timed out after {Mqtt.SUBSCRIBE_TIMEOUT} milliseconds");
                }
            }
            catch (AggregateException ex)
            {
                Log.Error("Failed to connect to the server: " + ex.InnerException.Message);
            }
        }

        private void ReconnectClient()
        {
            Task.Run(() =>
            {
                try
                {
                    // timeout the reconnect task after 5 seconds
                    if (Mqtt.mqttClient.ReconnectAsync().Wait(Mqtt.CONNECT_DISCONNECT_TIMEOUT))
                    {
                        Console.WriteLine($"Re-Connected to MQTT server {server}:{port} as {login}");
                        SubscribeClient();
                    }
                    else
                    {
                        Log.Error($"MQTT reconnect task timed out after {Mqtt.CONNECT_DISCONNECT_TIMEOUT} milliseconds");
                    }
                }
                catch (AggregateException ex)
                {
                    Log.Error($"MQTT: Failed to reconnect to the {server}:{port} - " + ex.InnerException.Message);
                }
                if (Mqtt.mqttClient.IsConnected)
                    updateSkipCounter = 0;
            });
        }

        public override void clearState()
        {
            // nothing to clear
            // Console.WriteLine("Clearing state");
        }

        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            if (!enabled)
                return;

            //Reduced update rate
            updateSkipCounter--;
            if (updateSkipCounter > 0)
                return;

            //Avoid issues with disconnected client
            if (!Mqtt.mqttClient.IsConnected)
            {
                updateSkipCounter = 3600; //Timeout of 1min at 60 ticks per second

                //Client reconnect is done in a seperate thread to not lock up the update thread when server is offline
                ReconnectClient();
                return;
            }

            updateSkipCounter = updateRateLimit;

            if (currentGameState != null)
            {
                // TODO: only add values that did not change from previous state

                var payload = new Dictionary<string, object>();
                var telemetry = new Dictionary<string, object>();

                payload["time"] = new DateTimeOffset(currentGameState.Now).ToUnixTimeMilliseconds();
                foreach (DataItem dataItem in dataItems)
                {
                    addValueToTelemetry(telemetry, currentGameState, dataItem.CrewChiefField, dataItem.TelemetryField);
                }

                if (telemetry.Count > 0)
                {
                    payload["telemetry"] = telemetry;

                    string track = "Unknown";
                    if (currentGameState.SessionData.TrackDefinition.name != null)
                        track = SanitizeForTopic(currentGameState.SessionData.TrackDefinition.name);

                    string carModel = "Unknown";
                    if (currentGameState.carName != null)
                        carModel = SanitizeForTopic(currentGameState.carName);

                    string sessionType = SanitizeForTopic(currentGameState.SessionData.SessionType.ToString());

                    string mytopic = topic +
                        "/" + driverName +
                        "/" + new DateTimeOffset(currentGameState.SessionData.SessionStartTime).ToUnixTimeSeconds() +
                        "/" + CrewChief.gameDefinition.friendlyName +
                        "/" + track +
                        "/" + carModel +
                        "/" + sessionType;

                    //Console.WriteLine("Publishing to topic: " + mytopic);

                    var applicationMessage = new MqttApplicationMessageBuilder()
                   .WithTopic(mytopic)
                   .WithPayload(JsonConvert.SerializeObject(payload))
                   .Build();

                    var task = Mqtt.mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                }
            }
        }

        private void addValueToTelemetry(Dictionary<string, object>telemetry, GameStateData gameState, string CCPropName, string TelemetryPropName)
        {
            object value = ReflectionGameStateAccessor.getPropertyValue(gameState, CCPropName);
            if (value != null)
            {
                telemetry[TelemetryPropName] = value;
            }
        }
        
        public void loadConfig()
        {
            JObject config = JObject.Parse(File.ReadAllText(getConfigFileLocation()));
            server = config.GetValue("Server")?.ToString();
            topic = config.GetValue("Topic")?.ToString();
            login = config.GetValue("Login")?.ToString();
            password = config.GetValue("Password")?.ToString();
            port = config.GetValue("Port").ToObject<int>();
            updateRateLimit = config.GetValue("UpdateRateLimit").ToObject<int>();
            subscribeTopic = config.GetValue("SubscribeTopic")?.ToString();
            dataItems = config.GetValue("Channels").ToObject<List<DataItem>>();
        }

        protected static String getMD5HashFromFile(String fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty);
                }
            }
        }
        
        public static String getConfigFileLocation()
        {
            String userConfig = System.IO.Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "mqtt_telemetry.json");
            String defaultConfig = Configuration.getDefaultFileLocation("mqtt_telemetry.json");
            //string oldDefaultConfigMD5 = getMD5HashFromFile(userConfig);
            String oldDefaultConfigMD5 = "32455A215BD00CE14F3F0712C4603550";

            if (File.Exists(userConfig))
            {
                // update the file if it hasnt been modified by the user and its not the new default
                if (getMD5HashFromFile(userConfig) == oldDefaultConfigMD5)
                {
                    Log.Info("Updating unchanged user-configured mqtt_telemetry.json from Documents/CrewChiefV4/ folder");
                    File.Copy(defaultConfig, userConfig, true);
                }
                Log.Info("Loading user-configured mqtt_telemetry.json from Documents/CrewChiefV4/ folder");

                return userConfig;
            }
            // make sure we save a copy to the user config directory
            else if (!File.Exists(userConfig))
            {
                try
                {
                    File.Copy(defaultConfig, userConfig);
                    Log.Info("Loading user-configured mqtt_telemetry.json from Documents/CrewChiefV4/ folder");
                    return userConfig;
                }
                catch (Exception e)
                {
                    Log.Error("Error copying default mqtt_telemetry.json file to user dir : " + e.Message);
                    Log.Error("Loading default mqtt_telemetry.json from installation folder");
                    return defaultConfig;
                }
            }
            else
            {
                Log.Info("Loading default mqtt_telemetry.json from installation folder");
                return defaultConfig;
            }
        }

        public class DataItem
        {
            public string CrewChiefField;
            public string TelemetryField;
        }
    }
}
