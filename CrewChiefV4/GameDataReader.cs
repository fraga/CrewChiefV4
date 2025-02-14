﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO.Compression;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CrewChiefV4
{
    public abstract class GameDataReader
    {
        protected String filenameToDump;

        public Boolean dumpToFile = false;

        protected abstract Boolean InitialiseInternal();

        public abstract Object ReadGameData(Boolean forSpotter);

        public abstract void Dispose();

        public abstract void DumpRawGameData();

        /// <summary>
        /// Kinda enumerator, returns an item of game data each time it's called
        /// </summary>
        /// <param name="filename">(which EVERY game handler processes into a full path)</param>
        /// <param name="pauseBeforeStart">mS to sleep the first time it's called</param>
        /// <returns>raw game data</returns>
        public abstract Object ReadGameDataFromFile(String filename, int pauseBeforeStart);

        public abstract void ResetGameDataFromFile();

        protected String dataFilesPath;

        private Boolean wroteConsoleDebugLog = false;

        public GameDataReader()
        {
            if (CrewChief.UseDebugFilePaths)
            {
                dataFilesPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), @"..\", @"..\dataFiles\");
            }
            else
            {
                dataFilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "debugLogs");
            }
            try
            {
                System.IO.Directory.CreateDirectory(dataFilesPath);
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to create folder for data file, no session record will be available");
                dataFilesPath = null;
            }
        }

        // NOTE: InitialiseInternal must be synchronized internally.
        public Boolean Initialise()
        {
            if (CrewChief.gameDefinition.gameEnum != GameEnum.NONE)
            {
                Console.WriteLine("Initialising...");
            }
            Boolean initialised = InitialiseInternal();
            if (dataFilesPath == null)
            {
                // We can't dump to file if there's no valid path.
                dumpToFile = false;
            }
            if (initialised && dumpToFile)
            {
                filenameToDump = dataFilesPath + "\\" + CrewChief.gameDefinition.gameEnum.ToString() + "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".cct";
                Console.WriteLine("Session recording will be dumped to file: " + filenameToDump);
            }
            return initialised;
        }
        protected void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }
            try
            {
                if (!MainWindow.shouldSaveTrace)
                    return;

                Console.WriteLine("About to dump game data - this may take a while");
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                {
                    using (GZipStream zipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
                    {
                        using (StreamWriter sw = new StreamWriter(zipStream, Encoding.UTF8))
                        {
                            using (JsonWriter writer = new JsonTextWriter(sw))
                            {
                                serializer.Serialize(writer, serializableObject);
                            }
                        }
                    }
                }
                if (!wroteConsoleDebugLog) //prevent more then one console dump in case of split traces
                {
                    lock (MainWindow.instanceLock)
                    {
                        if (MainWindow.instance != null)
                        {
                            File.WriteAllText(Path.ChangeExtension(fileName, "txt"), MainWindow.instance.consoleWriter.enable ?
                                MainWindow.instance.consoleTextBox.Text : MainWindow.instance.consoleWriter.builder.ToString());
                            wroteConsoleDebugLog = true;
                        }
                    }
                }


                Console.WriteLine("Done writing session data log to: " + fileName);
                Console.WriteLine("PLEASE RESTART THE APPLICATION BEFORE ATTEMPTING TO RECORD ANOTHER SESSION");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to write raw game data: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
            }
        }

        protected T DeSerializeObject<T>(string fileName)
        {
            Console.WriteLine("About to load recorded game data from file " + fileName + " - this may take a while");
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);
            try
            {
                if (Path.GetExtension(fileName) == ".gz")
                {
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        using(GZipStream zipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                        {
                            Type outType = typeof(T);
                            XmlSerializer serializer = new XmlSerializer(outType);
                            using (XmlReader xmlReader = new XmlTextReader(zipStream))
                            {
                                objectOut = (T)serializer.Deserialize(xmlReader);
                            }
                        }
                    }
                }
                else if (Path.GetExtension(fileName) == ".cct")
                {
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        using (GZipStream zipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                        {
                            using (StreamReader reader = new StreamReader(zipStream))
                            {
                                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                                {
                                    JsonSerializer ser = new JsonSerializer();
                                    objectOut = ser.Deserialize<T>(jsonReader);
                                }
                            }
                        }
                    }
                }
                else // assume xml
                {
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        Type outType = typeof(T);
                        XmlSerializer serializer = new XmlSerializer(outType);
                        using (XmlReader reader = new XmlTextReader(fileStream))
                        {
                            objectOut = (T)serializer.Deserialize(reader);
                        }
                    }                    
                }
                Console.WriteLine("Done reading session data from: " + fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to read raw game data: " + ex.Message);
            }
            return objectOut;
        }

        public virtual Boolean hasNewSpotterData()
        {
            return true;
        }

        public virtual Object getLatestGameData()
        {
            return null;
        }

        public virtual void stop()
        {
            // no op - only implemented by UDP reader
        }

        // NOTE: This needs to be synchronized, because disconnection happens from CrewChief.Run and MainWindow.Dispose.
        // Does not apply to network data feeds.
        public virtual void DisconnectFromProcess()
        {
            // Is called when game process exits or Stop button is pressed and run loop terminates.
            // Can be used to release resources.

            // no op - only implemented for rF2.
        }
    }
}
