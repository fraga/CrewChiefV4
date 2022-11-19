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
using System.Windows.Forms;
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
                if (CrewChief.gameDefinition.gameEnum != GameEnum.IRACING)
                {
                    try
                    {
                        parentDir = Path.GetDirectoryName(proc[0].MainModule.FileName);
                    }
                    catch (Win32Exception) { /*Ignore - anti cheat protection?*/ }
                    catch (Exception e) { Log.Exception(e); }
                }
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

        /// <summary>
        /// Launch the game
        /// </summary>
        /// <param name="launchExe"></param>
        /// <param name="launchParams"></param>
        /// <returns>true: the game started
        /// false: there was a problem running the game</returns>
        public static bool runGame(String launchExe, String launchParams)
        {
            string exception;
            string exMsg;
            // Inconsistent handling of spaces in paths
            // ProcessStartInfo() is happy with the escaped " version: "\"c:\pa th\game.exe\""
            // GetDirectoryName() wants "c:\pa th\game.exe"
            // Neither is happy if the user enters "c:\pa th\game.exe" with quotes
            launchExe = launchExe.Trim().Trim('"').Trim('\'').Trim();
            if (launchExe.IsNullOrEmpty())
            {
                // user wants to launch the game but hasn't specified a path. Bloody users.
                Console.WriteLine("Skipping game launch because no path was provided");
                return true;
            }
            try
            {
                Console.WriteLine("Attempting to run game using " + launchExe + " " + launchParams);
                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(launchExe);
                    startInfo.Arguments = launchParams;
                    startInfo.WorkingDirectory = Path.GetDirectoryName(launchExe);
                    process.StartInfo = startInfo;
                    process.Start();
                    return true;
                }
            }
            catch (InvalidOperationException e)
            {
                exception = "InvalidOperationException";
                exMsg = e.Message;
            }
            catch (Exception e)
            {
                exception = "Exception";
                exMsg = e.Message;
            }
            string error = String.IsNullOrEmpty(launchParams) ? 
                $"{exception} starting game with path '{launchExe}' : {exMsg}" : 
                $"{exception} starting game with path '{launchExe}' and params '{launchParams}' : {exMsg}";
            Log.Error(error);
            return false;
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
        /// Restart CC with edited args
        /// </summary>
        /// <returns>true if app restarted</returns>
        public static bool RestartApp(
            bool app_restart = false,
            bool removeSkipUpdates = false,
            bool removeProfile = false,
            bool removeGame = false)
        {
            if (!CrewChief.Debugging)
            {
                var newArgs = RestartAppCommandLine(app_restart,
                                                    removeSkipUpdates,
                                                    removeProfile,
                                                    removeGame);
                System.Diagnostics.Process.Start(    // to start new instance of application
                    System.Windows.Forms.Application.ExecutablePath,
                    String.Join(" ", newArgs.ToArray()));
                return true;
            }
            // If debugging then carry on regardless
            return false;
        }
        /// <summary>
        /// Edit the current command line
        /// </summary>
        /// <param name="app_restart"></param>
        /// <param name="removeSkipUpdates">We're restarting because the 'force update check'</param>
        /// <param name="removeProfile">-profile [profile name]</param>
        /// <param name="removeGame">=game [game name]</param>
        /// <returns></returns>
        // (Extracted so it can be tested)
        internal static List<string> RestartAppCommandLine(
            bool app_restart = false,
            bool removeSkipUpdates = false,
            bool removeProfile = false,
            bool removeGame = false)
        {
            if (app_restart)
            {
                CrewChief.CommandLine.Add("app_restart", "");
            }
            if (removeSkipUpdates)
            {
                CrewChief.CommandLine.Remove("skip_updates");
                CrewChief.CommandLine.Remove("SKIP_UPDATES");
            }
            if (removeProfile)
            {
                CrewChief.CommandLine.Remove("profile");
            }
            if (removeGame)
            {
                CrewChief.CommandLine.Remove("game");
            }
            // Always have to add "-multi" to the start args so the app can restart
            CrewChief.CommandLine.Add("multi", "");

            // Translate the dict back into a command line
            var newArgs = new List<string>();
            foreach (var arg in CrewChief.CommandLine._dict)
            {
                newArgs.Add("-" + arg.Key);
                newArgs.Add(arg.Value);
            }
            return newArgs;
        }
    
        /// <summary>
        /// If 'text' is longer than 'maxLength' insert a newline near
        /// the middle after a word break
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string SplitString(string text, int maxLength)
        {
            if (text.Length <= maxLength)
            {
                return text;
            }
            //Degenerate case with only 1 word
            if (!text.Any(Char.IsWhiteSpace))
            {
                return text;
            }

            int mid = text.Length / 2;
            if (!Char.IsWhiteSpace(text[mid]))
            {
                for (int i = 1; i < mid; i += i)
                {
                    if (Char.IsWhiteSpace(text[mid + i]))
                    {
                        mid = mid + i;
                        break;
                    }
                    if (Char.IsWhiteSpace(text[mid - i]))
                    {
                        mid = mid - i;
                        break;
                    }
                }
            }

            return text.Substring(0, mid)
                   + Environment.NewLine + text.Substring(mid + 1);
        }


    /// <summary>
    /// Read the command line arguments into a dictionary
    /// </summary>
    public class CommandLineParametersReader
    {
        private string[] _args
        {
            get;
        }
        public Dictionary<string, string> _dict
        {
            get;
        }

        private bool CaseSensitive
        {
            get;
        }

        public CommandLineParametersReader(string[] args = null, bool isCaseSensitive = false)
        {
            if (args == null)
            {
                args = Environment.GetCommandLineArgs();
            }
            _args = args;
            CaseSensitive = isCaseSensitive;
            _dict = new Dictionary<string, string>();
            Process();
        }

        // Process Arguments into KeyPairs
        private void Process()
        {
            string currentKey = null;
            foreach (var arg in _args)
            {
                var s = arg.Trim();
                if (s.StartsWith("-"))
                {
                    currentKey = s.Substring(1);
                    if (!CaseSensitive)
                    {
                        currentKey = currentKey.ToLower();
                    }
                    _dict[currentKey] = "";
                }
                else
                {
                    if (currentKey != null)
                    {
                        _dict[currentKey] = s;
                        currentKey = null;
                    }
                }
            }
        }

        // Return the Key with a default value
        public string Get(string key, string defaultvalue = null)
        {
            if (!CaseSensitive)
            {
                key = key.ToLower();
            }
            return _dict.ContainsKey(key) ? _dict[key] : defaultvalue;
        }

        public void Add(string key, string value)
        {
            _dict[key] = value;
        }
        public void Remove(string key)
        {
            if (_dict.ContainsKey(key))
            {
                _dict.Remove(key);
            }
        }
        /// <summary>
        /// Return a -c_[command] argument
        /// </summary>
        /// <returns>
        /// The command or "" if none
        /// </returns>
        public string GetCommandArg()
        {
            string cmd = "";
            foreach (var arg in _dict)
            {
                if (arg.Key.StartsWith("c_"))
                {
                    cmd = "-" + arg.Key;
                }
            }
            return cmd;
        }
    }

        internal static void ReportException(Exception e, string msg, bool needReport)
        {
            String message = needReport ? "Error message copied to clipboard:\n" : "";
            message += e.Message + "Stack trace: " + String.Join(",", e.StackTrace);
            int innerExceptionCount = 0;
            int maxReportableInnerExceptions = 5;   // in case we have a circular set of inner exception references
            Exception innerException = e.InnerException;
            while (innerException != null && innerExceptionCount < maxReportableInnerExceptions)
            {
                message += "\n\nInner exception " + innerExceptionCount + " message: " + e.InnerException.Message +
                    "\nInner exception " + innerExceptionCount + " stack trace: " + String.Join(",", e.InnerException.StackTrace);
                innerException = innerException.InnerException;
                innerExceptionCount++;
            }
            // Write it to the console window if it's live
            Console.WriteLine(
                "==================================================================" + Environment.NewLine
                );
            Console.WriteLine(message);
            Console.WriteLine(
                "==================================================================" + Environment.NewLine
            );

            if (needReport)
            {
                string consoleLogFilename = null;
                try
                {
                    consoleLogFilename = MainWindow.instance.saveConsoleOutputText();
                }
                catch (Exception ex)
                {
                }
                if (consoleLogFilename == null)
                {
                    // Console window not live yet
                    MessageBox.Show("The following text will be COPIED TO THE CLIPBOARD\n"
                        + (needReport ? "Please PASTE the report to the Crew Chief team via the forum or Discord." : "")
                        + "\n\n" + message,
                        "Fatal error",
                        MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show($"The following text should be found in {consoleLogFilename}\n"
                        + "but will be COPIED TO THE CLIPBOARD too\n"
                        + "Please send the log file to the Crew Chief team via the forum or Discord."
                        + "\n\n" + message,
                        "Fatal error",
                        MessageBoxButtons.OK);
                }
                Clipboard.SetText(message);
            }
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
}
