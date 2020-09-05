using CrewChiefV4.Audio;
using CrewChiefV4.GameState;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace CrewChiefV4
{
    /// <summary>
    /// value object for message expectations
    /// </summary>
    public class ExpectedMessage
    {
        string[] messageNames;
        public int minCount;
        public int maxCount;

        // expect a specific message to be played the range of times - note that the DELAYED_ and COMPOUND_ prefixes are also considered here
        public ExpectedMessage(string messageName, int minCount, int maxCount)
        {
            this.messageNames = new string[] { messageName };
            this.minCount = minCount;
            this.maxCount = maxCount;
        }

        // expect one of the messageNames to be played the range of times - note that the DELAYED_ and COMPOUND_ prefixes are also considered here
        public ExpectedMessage(string[] messageNames, int minCount, int maxCount)
        {
            this.messageNames = messageNames;
            this.minCount = minCount;
            this.maxCount = maxCount;            
        }
        // expect a specific message to be played the exact number of times - note that the DELAYED_ and COMPOUND_ prefixes are also considered here
        public ExpectedMessage(string messageName, int exactCount)
        {
            this.messageNames = new string[] { messageName };
            this.minCount = exactCount;
            this.maxCount = exactCount;
        }

        // expect one of the messageNames to be played the exact number of times - note that the DELAYED_ and COMPOUND_ prefixes are also considered here
        public ExpectedMessage(string[] messageNames, int exactCount)
        {
            this.messageNames = messageNames;
            this.minCount = exactCount;
            this.maxCount = exactCount;
        }
        override public string ToString()
        {
            return string.Join(", ", messageNames) + " expected >= " + minCount + " and <= " + maxCount;
        }
        // check that this expectation is met - i.e. a message was queued with one of the expected names >= min and <= max times
        public int getMatchCount()
        {
            int matchCount = 0;
            foreach (KeyValuePair<string, int> entry in Utilities.queuedMessageIds)
            {
                foreach (string messageName in messageNames)
                {
                    if (messageName == entry.Key || ("DELAYED_" + messageName) == entry.Key || ("COMPOUND_" + messageName) == entry.Key)
                    {
                        matchCount += entry.Value;
                    }
                }
            }
            return matchCount;
        }
    }

    public static class Utilities
    {
        public static Boolean includesRaceSession = false;

        public static Dictionary<string, int> queuedMessageIds = new Dictionary<string, int>();

        // some noddy hard-coded expectations for race session trace playback
        // TODO make this something that can be saved with the trace so each trace can define its own set of expectations

        // TODO: move the hard-coded Strings to a messageNames class and reference these in all the events instead of using random
        // magic Strings everywhere
        private static ExpectedMessage[] defaultExpectedMessagesForRaceSessions = new ExpectedMessage[]
        {        
            new ExpectedMessage("lap_counter/get_ready", 1),
            new ExpectedMessage("lap_counter/green_green_green", 1),
            new ExpectedMessage("position", 1, 1000), // expect at least *some* position messages
            new ExpectedMessage(new string[] {"Timings/gap_behind", "Timings/gap_in_front"}, 1, 1000), // expect at least *some* gap messages
            new ExpectedMessage(new string[] {"fuel/half_distance_good_fuel", "fuel/half_distance_low_fuel"}, 0, 1),    // won't always get this, but should never have > 1
            new ExpectedMessage(new string[] {"lap_counter/two_to_go", "lap_counter/two_to_go_top_three", "lap_counter/two_to_go_leading", 
                "race_time/five_minutes_left_podium", "race_time/five_minutes_left_leading", "race_time/five_minutes_left"}, 1),    // should always get 1 2-to-go or 5-mins-to-go
            new ExpectedMessage(new string[] {"lap_counter/last_lap", "lap_counter/white_flag_last_lap", "lap_counter/last_lap_leading", 
                "lap_counter/last_lap_top_three", "race_time/last_lap", "race_time/last_lap_leading", "race_time/last_lap_top_three"}, 1),  // should always get 1 last-lap
            new ExpectedMessage("SESSION_END", 1)   // should always get 1 session end
        };

        public static Random random = new Random();

        private static WebSocketServer ccDataWebSocketServer;

        private static WebSocketServer gameDataWebSocketServer;

        private static object websocketServerLock = new object();

        public static AudioPlayer audioPlayer;

        private static int ccDataWebsocketPort = UserSettings.GetUserSettings().getInt("websocket_port");

        private static int gameDataWebsocketPort = UserSettings.GetUserSettings().getInt("game_data_websocket_port");

        public static GameDataReader gameDataReader;

        public static GameDataSerializer gameDataSerializer;
        
        public static void checkPlaybackCounts()
        {
            if (includesRaceSession)
            {
                Console.WriteLine("Playback counts: \n" + string.Join("\n", queuedMessageIds.Select(x => x.Key + " : " + x.Value)));
                checkMessageCounts();
            }
            else
            {
                Console.WriteLine("Skipping expectations as we've not had a race session");
            }
        }

        private static Boolean checkMessageCounts()
        {
            Boolean pass = true;
            foreach (ExpectedMessage expected in defaultExpectedMessagesForRaceSessions)
            {
                int matchCount = expected.getMatchCount();
                if (matchCount < expected.minCount || matchCount > expected.maxCount)
                {
                    Console.WriteLine("***** match count check failed " + expected.ToString() + " got " + matchCount + " matches");
                    pass = false;
                }
            }
            if (pass)
            {
                Console.WriteLine("Message expectations passed");
            }
            else
            {
                Console.WriteLine("Message expectations failed");
            }
            return pass;
        }

        public static void startCCDataWebsocketServer(AudioPlayer audioPlayer)
        {
            Utilities.audioPlayer = audioPlayer;
            stopCCWebsocketServer();
            try
            {
                lock (websocketServerLock)
                {
                    ccDataWebSocketServer = new WebSocketServer(ccDataWebsocketPort);
                    ccDataWebSocketServer.AddWebSocketService<WebsocketData>("/crewchief");
                    ccDataWebSocketServer.Start();
                    Console.WriteLine("Successfully started Crew Chief data WebSocket server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to start Crew Chief websocket: " + e.Message + ", " + e.StackTrace);
            }
        }

        public static void startGameDataWebsocketServer(String endpoint, GameDataReader gameDataReader, GameDataSerializer serializer)
        {
            stopGameDataWebsocketServer();
            GameDataWebsocketData.init(gameDataReader, serializer);
            try
            {
                lock (websocketServerLock)
                {
                    gameDataWebSocketServer = new WebSocketServer(gameDataWebsocketPort);
                    gameDataWebSocketServer.AddWebSocketService<GameDataWebsocketData>(endpoint);
                    gameDataWebSocketServer.Start();
                    Console.WriteLine("Successfully started game data WebSocket server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to start game data websocket: " + e.Message + ", " + e.StackTrace);
            }
        }

        public static void stopWebsocketServers()
        {
            stopCCWebsocketServer();
            stopGameDataWebsocketServer();
        }

        private static void stopCCWebsocketServer()
        {
            try
            {
                lock (websocketServerLock)
                {
                    if (ccDataWebSocketServer != null)
                    {
                        ccDataWebSocketServer.Stop();
                        ccDataWebSocketServer = null;
                        Console.WriteLine("Stopped CC data WebSocket server");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to stop CC data websocket: " + e.Message + ", " + e.StackTrace);
            }
        }

        private static void stopGameDataWebsocketServer() 
        {
            GameDataWebsocketData.reset();
            try
            {
                lock (websocketServerLock)
                {
                    if (gameDataWebSocketServer != null)
                    {
                        gameDataWebSocketServer.Stop();
                        gameDataWebSocketServer = null;
                        Console.WriteLine("Stopped game data WebSocket server");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to stop game data websocket: " + e.Message + ", " + e.StackTrace);
            }
        }

        public static bool IsGameRunning(String processName, String[] alternateProcessNames, out String parentDir)
        {
            parentDir = null;

            var proc = Process.GetProcessesByName(processName);
            if (proc.Length > 0)
            {
                try
                {
                    parentDir = Path.GetDirectoryName(proc[0].MainModule.FileName);
                }
                catch (Exception) { }

                return true;
            }
            else if (alternateProcessNames != null && alternateProcessNames.Length > 0)
            {
                foreach (String alternateProcessName in alternateProcessNames)
                {
                    if (Process.GetProcessesByName(alternateProcessName).Length > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void runGame(String launchExe, String launchParams)
        {
            try
            {
                Console.WriteLine("Attempting to run game using " + launchExe + " " + launchParams);
                if (launchExe.Contains(" "))
                {
                    if (!launchExe.StartsWith("\""))
                    {
                        launchExe = "\"" + launchExe;
                    }
                    if (!launchExe.EndsWith("\""))
                    {
                        launchExe = launchExe + "\"";
                    }
                }
                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(launchExe);
                    startInfo.Arguments = launchParams;
                    process.StartInfo = startInfo;
                    process.Start();
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("InvalidOperationException starting game: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception starting game: " + e.Message);
            }
        }

        /*
         * For tyre life estimates we want to know how long the tyres will last, so we're asking for a time prediction
         * given a wear amount (100% wear). So y_data is the y-axis which may be time points (session running time) or
         * number of sectors since session start incrementing +1 for each sector. When we change tyres we clear these
         * data sets but the y-axis time / sector counts will start at however long into the session (time or total 
         * sectors) we are.
         * x_data is the tyre wear at that y point (a percentage).
         * the x_point is the point you want to predict the life - wear amount. So we pass 100% in here to give us
         * a time / sector count estimate.
         * order is the polynomial fit order - 1 for linear, 2 for quadratic etc. > 3 does not give a suitable
         * curve and will produce nonsense. Use 2 for tyre wear.
         */
        public static double getYEstimate(double[] x_data, double[] y_data, double x_point, int order)
        {
            // get the polynomial from the Numerics library:
            double[] curveParams = Fit.Polynomial(x_data, y_data, order);

            // solve for x_point:
            double y_point = 0;
            for (int power = 0; power < curveParams.Length; power++)
            {
                if (power == 0)
                {
                    y_point = y_point + curveParams[power];
                }
                else if (power == 1)
                {
                    y_point = y_point + curveParams[power] * x_point;
                }
                else
                {
                    y_point = y_point + curveParams[power] * Math.Pow(x_point, power);
                }
            }
            return y_point;
        }

        public static void TraceEventClass(GameStateData gsd)
        {
            if (gsd == null || gsd.carClass == null)
                return;

            var eventCarClasses = new Dictionary<string, CarData.CarClassEnum>();
            eventCarClasses.Add(gsd.carClass.getClassIdentifier(), gsd.carClass.carClassEnum);

            if (gsd.OpponentData != null)
            {
                foreach (var opponent in gsd.OpponentData)
                {
                    if (opponent.Value.CarClass != null
                        && !eventCarClasses.ContainsKey(opponent.Value.CarClass.getClassIdentifier()))
                    {
                        eventCarClasses.Add(opponent.Value.CarClass.getClassIdentifier(), opponent.Value.CarClass.carClassEnum);
                    }
                }
            }

            if (eventCarClasses.Count == 1)
                Console.WriteLine("Single-Class event:\"" + eventCarClasses.Keys.First() + "\" "
                    + Utilities.GetCarClassMappingHint(eventCarClasses.Values.First()));
            else
            {
                Console.WriteLine("Multi-Class event:");
                foreach (var carClass in eventCarClasses)
                {
                    Console.WriteLine("\t\"" + carClass.Key + "\" "
                        + Utilities.GetCarClassMappingHint(carClass.Value));
                }
                if (!GameStateData.Multiclass)
                {
                    Console.WriteLine("Insufficient car class data, so dropping back to single class racing");
                }
            }
        }

        private static string GetCarClassMappingHint(CarData.CarClassEnum cce)
        {
            if (cce == CarData.CarClassEnum.UNKNOWN_RACE)
                return "(unmapped)";
            else if (cce == CarData.CarClassEnum.USER_CREATED)
                return "(user defined)";

            return "(built-in)";
        }

        public static string ResolveDataFile(string dataFilesPath, string fileNameToResolve)
        {
            // Search in dataFiles:
            if (Directory.Exists(dataFilesPath))
            {
                var resolvedFilePaths = Directory.GetFiles(dataFilesPath, fileNameToResolve, SearchOption.AllDirectories);
                if (resolvedFilePaths.Length > 0)
                    return resolvedFilePaths[0];
            }

            // Search documents debugLogs:
            var resolvedFileUserPaths = Directory.GetFiles(System.IO.Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments), @"CrewChiefV4\debugLogs"), fileNameToResolve, SearchOption.AllDirectories);

            if (resolvedFileUserPaths.Length > 0)
                return resolvedFileUserPaths[0];

            Console.WriteLine("Failed to resolve trace file full path: " + fileNameToResolve);
            return null;
        }


        public static Tuple<int, int> WholeAndFractionalPart(float realNumber, int fractions = 1)
        {
            // get the whole and fractional part (yeah, I know this is shit)
            var str = realNumber.ToString();
            int pointPosition = str.IndexOf('.');
            int wholePart = 0;
            int fractionalPart = 0;
            if (pointPosition > 0)
            {
                wholePart = int.Parse(str.Substring(0, pointPosition));
                fractionalPart = int.Parse(str.Substring(pointPosition + 1, fractions).ToString());
            }
            else
            {
                wholePart = (int)realNumber;
            }

            return new Tuple<int, int>(wholePart, fractionalPart);
        }

        /// <summary>
        /// Restart CC with new args
        /// </summary>
        /// <param name="newArgs"></param>
        /// <param name="removeSkipUpdates"></param>
        /// <returns>true if app restarted</returns>
        public static bool RestartApp(List<String> newArgs=null, bool removeSkipUpdates = false)
        {
            if (!CrewChief.Debugging)
            {
                List<String> startArgs = new List<string>();
                foreach (String startArg in Environment.GetCommandLineArgs())
                {
                    // if we're restarting because the 'force update check'
                    // was clicked, remove the '-skip_updates' arg
                    if (removeSkipUpdates && 
                        ("-skip_updates".Equals(startArg, StringComparison.InvariantCultureIgnoreCase)
                        || "SKIP_UPDATES".Equals(startArg)))
                    {
                        continue;
                    }
                    startArgs.Add(startArg);
                }

                // Always have to add "-multi" to the start args so the app can restart
                if (newArgs == null)
                    newArgs = new List<string>();
                newArgs.Add("-multi");
                foreach (string arg in newArgs)
                {
                    if (!startArgs.Contains(arg))
                    {
                        startArgs.Add(arg);
                    }
                }
                System.Diagnostics.Process.Start(    // to start new instance of application
                    System.Windows.Forms.Application.ExecutablePath,
                    String.Join(" ", startArgs.ToArray()));
                return true;
            }
            // If debugging then carry on regardless
            return false;
        }

        internal static void ReportException(Exception e, string msg, bool needReport)
        {
            Console.WriteLine(
                Environment.NewLine + "==================================================================" + Environment.NewLine
                + (needReport ? ("PLEASE REPORT THIS ERROR TO CC DEV TEAM." + Environment.NewLine) : "")
                + "Error message: " + msg + Environment.NewLine
                + e.ToString() + Environment.NewLine
                + e.Message + Environment.NewLine
                + e.StackTrace + Environment.NewLine
            );

            if (e.InnerException != null)
            {
                Console.WriteLine(
                    "Inner exception: " + e.InnerException.ToString() + Environment.NewLine
                    + e.InnerException.Message + Environment.NewLine
                    + e.InnerException.StackTrace + Environment.NewLine
                );
            }

            Console.WriteLine(
                "==================================================================" + Environment.NewLine
            );
        }

        internal static bool InterruptedSleep(int totalWaitMillis, int waitWindowMillis, Func<bool> keepWaitingPredicate)
        {
            Debug.Assert(totalWaitMillis > 0 && waitWindowMillis > 0);
            var waitSoFar = 0;
            while (waitSoFar < totalWaitMillis)
            {
                if (!keepWaitingPredicate())
                    return false;

                Thread.Sleep(waitWindowMillis);
                waitSoFar += waitWindowMillis;
            }

            return true;
        }

        internal static bool IsFlagOn<E, F>(E value, F flag)
        {
            return (Convert.ToInt32(value) & Convert.ToInt32(flag)) != 0;
        }

        internal static bool IsFlagOff<E, F>(E value, F flag)
        {
            return !Utilities.IsFlagOn(value, flag);
        }

        internal static bool TryBackupBrokenFile(string filePath, string backupExt, string msg)
        {
            try
            {
                var brokenFilePath = Path.ChangeExtension(filePath, backupExt);
                Console.WriteLine($"{msg} - renaming \"{filePath}\" to \"{brokenFilePath}\"");
                File.Delete(brokenFilePath);
                File.Move(filePath, brokenFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File.Move failed for {filePath} exception: {ex.Message}");
                return false;
            }

            return true;
        }

        internal static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
        public static int SizeOf<T>()
        {
            return Marshal.SizeOf(typeof(T));
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern long GetTickCount64();

        public static IEnumerable<Enum> GetEnumFlags(Enum input)
        {
            foreach (Enum value in Enum.GetValues(input.GetType()))
            {
                if (input.HasFlag(value))
                    yield return value;
            }
        }

        public static string GetFileContentsJsonWithComment(string fullFilePath)
        {
            var jsonString = new StringBuilder();
            StreamReader file = null;
            try
            {
                file = new StreamReader(fullFilePath);
                String line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.Trim().StartsWith("#"))
                    {
                        jsonString.AppendLine(line);
                    }
                }
                return jsonString.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file " + fullFilePath + ": " + e.Message);
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
            return null;
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key,
            string defaultValue, StringBuilder value, int size, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(string section, string key,
            string value, string filePath);

        public static string ReadIniValue(string section, string key, string filePath, string defaultValue = "")
        {
            var value = new StringBuilder(512);
            GetPrivateProfileString(section, key, defaultValue, value, value.Capacity, filePath);
            return value.ToString();
        }

        public static bool WriteIniValue(string section, string key, string value, string filePath)
        {
            bool result = WritePrivateProfileString(section, key, value, filePath);
            return result;
        }
    }

    public class WebsocketData : WebSocketBehavior
    {
        private String channelOpenStringResponse = "{\"channelOpen\": true}";
        private String channelClosedStringResponse = "{\"channelOpen\": false}";
        protected override void OnMessage(MessageEventArgs e)
        {
            Send(Utilities.audioPlayer.isChannelOpen() ? channelOpenStringResponse : channelClosedStringResponse);
        }
    }

    public class GameDataWebsocketData : WebSocketBehavior
    {
        private static GameDataReader gameDataReader;
        private static GameDataSerializer gameDataSerializer;

        public static void init(GameDataReader gameDataReader, GameDataSerializer gameDataSerializer)
        {
            GameDataWebsocketData.gameDataReader = gameDataReader;
            GameDataWebsocketData.gameDataSerializer = gameDataSerializer;
        }

        public static void reset()
        {
            GameDataWebsocketData.gameDataReader = null;
            GameDataWebsocketData.gameDataSerializer = null;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Send(GameDataWebsocketData.gameDataSerializer.Serialize(GameDataWebsocketData.gameDataReader.getLatestGameData(), e.Data));
        }
    }

    // stackoverflow...
    public static class Extensions
    {

        /*public static int IndexOfMin<T>(this IList<T> list) where T : IComparable
        {
            if (list == null)
                throw new ArgumentNullException("list");

            IEnumerator<T> enumerator = list.GetEnumerator();
            bool isEmptyList = !enumerator.MoveNext();

            if (isEmptyList)
                throw new ArgumentOutOfRangeException("list", "list is empty");

            int minOffset = 0;
            T minValue = enumerator.Current;
            for (int i = 1; enumerator.MoveNext(); ++i)
            {
                if (enumerator.Current.CompareTo(minValue) >= 0)
                    continue;

                minValue = enumerator.Current;
                minOffset = i;
            }

            return minOffset;
        }*/
        public static int IndexOfMin<T>(this IEnumerable<T> source, IComparer<T> comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (comparer == null)
                comparer = Comparer<T>.Default;

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return -1;    // or maybe throw InvalidOperationException

                int minIndex = 0;
                T minValue = enumerator.Current;

                int index = 0;
                while (enumerator.MoveNext())
                {
                    index++;
                    if (comparer.Compare(enumerator.Current, minValue) < 0)
                    {
                        minIndex = index;
                        minValue = enumerator.Current;
                    }
                }
                return minIndex;
            }
        }
    }
}
