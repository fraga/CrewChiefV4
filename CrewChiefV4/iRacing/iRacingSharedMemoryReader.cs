﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRSDKSharp;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
namespace CrewChiefV4.iRacing
{
    public class iRacingSharedMemoryReader : GameDataReader
    {
        private iRacingSDK sdk = null;
        private Sim sim = null;
        
        private Boolean initialised = false;
        private List<iRacingStructDumpWrapper> dataToDump;
        private iRacingStructDumpWrapper[] dataReadFromFile = null;
        private int dataReadFromFileIndex = 0;
        private String lastReadFileName = null;
        private int numberOfCarsEnabled = 0;
        private bool is360HzTelemetry = false;
        int lastUpdate = -1;
        private int _DriverId = -1;
        public int DriverId { get { return _DriverId; } }
        private static bool enableDiskTelemetry = UserSettings.GetUserSettings().getBoolean("iracing_enable_disk_based_telemetry");
        public iRacingSharedMemoryReader()
        {
            string dataFilesPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "iRacing", "app.ini");
            if(System.IO.File.Exists(dataFilesPath))
            {
                string serverTransmitMaxCars = PluginInstaller.ReadValue("Graphics", "serverTransmitMaxCars", dataFilesPath, "0");
                Int32.TryParse(serverTransmitMaxCars.Substring(0, 2), out numberOfCarsEnabled);
                Console.WriteLine("serverTransmitMaxCars = " + numberOfCarsEnabled);
                string irsdkLog360Hz = PluginInstaller.ReadValue("Misc", "irsdkLog360Hz", dataFilesPath, "0");
                int Is360HzTelemetry = 0;
                Int32.TryParse(irsdkLog360Hz.Substring(0, 1), out Is360HzTelemetry);
                if (Is360HzTelemetry == 1)
                    is360HzTelemetry = true;
                Console.WriteLine("is360HzTelemetry = " + is360HzTelemetry);
            }
        }

        private object TryGetSessionNum()
        {
            try
            {
                var sessionnum = sdk.GetData("SessionNum");
                return sessionnum;
            }
            catch
            {
                return null;
            }
        }
        public class iRacingStructWrapper
        {
            public long ticksWhenRead;
            public Sim data;

        }
        public class iRacingStructDumpWrapper
        {
            public long ticksWhenRead;
            public iRacingData data;
        }
        public override void DumpRawGameData()
        {            
            if (dumpToFile && dataToDump != null && dataToDump.Count > 0 && filenameToDump != null)
            {                
                bool firstSession = true;
                List<iRacingStructDumpWrapper> currentSession = new List<iRacingStructDumpWrapper>();
                string sessionType = YamlParser.Parse(dataToDump[0].data.SessionInfo, string.Format(SessionData.sessionInfoYamlPath, dataToDump[0].data.SessionNum, "SessionType"));
                string track = YamlParser.Parse(dataToDump[0].data.SessionInfo, "WeekendInfo:TrackName:");
                string filename = System.IO.Path.GetFileNameWithoutExtension(filenameToDump);
                string directory = System.IO.Path.GetDirectoryName(filenameToDump);
                string extension = System.IO.Path.GetExtension(filenameToDump);
                string filenameToDumpRenamed = System.IO.Path.Combine(directory, filename + "-" + track + "-" + sessionType) + extension;
                                
                foreach (iRacingStructDumpWrapper wr in dataToDump)
                {
                    if (firstSession || !wr.data.IsNewSession)
                    {
                        firstSession = false;
                        currentSession.Add(wr);
                        continue;
                    }
                    else
                    {
                        SerializeObject(currentSession.ToArray<iRacingStructDumpWrapper>(), filenameToDumpRenamed);
                        currentSession.Clear();
                        currentSession.Add(wr);
                        track = YamlParser.Parse(wr.data.SessionInfo, "WeekendInfo:TrackName:");
                        sessionType = YamlParser.Parse(wr.data.SessionInfo, string.Format(SessionData.sessionInfoYamlPath, wr.data.SessionNum, "SessionType"));
                        filenameToDumpRenamed = System.IO.Path.Combine(directory, filename + "-" + track + "-" + sessionType) + extension;
                    }
                }
                SerializeObject(currentSession.ToArray<iRacingStructDumpWrapper>(), filenameToDumpRenamed);
            }
        }

        public void SplitTraceData(String newFilename)
        {
            if (dataToDump != null && dataToDump.Count > 0)
            {
                bool firstSession = true;
                List<iRacingStructDumpWrapper> currentSession = new List<iRacingStructDumpWrapper>();
                string sessionType = YamlParser.Parse(dataToDump[0].data.SessionInfo, string.Format(SessionData.sessionInfoYamlPath, dataToDump[0].data.SessionNum, "SessionType"));
                string track = YamlParser.Parse(dataToDump[0].data.SessionInfo, "WeekendInfo:TrackName:");
                string filename = System.IO.Path.GetFileNameWithoutExtension(newFilename);
                string directory = System.IO.Path.GetDirectoryName(newFilename);
                string extension = System.IO.Path.GetExtension(newFilename);
                string filenameToDumpRenamed = System.IO.Path.Combine(directory, filename + "-" + track + "-" + sessionType) + extension;

                foreach (iRacingStructDumpWrapper wr in dataToDump)
                {
                    if (firstSession || !wr.data.IsNewSession)
                    {
                        firstSession = false;
                        currentSession.Add(wr);
                        continue;
                    }
                    else
                    {
                        SerializeObject(currentSession.ToArray<iRacingStructDumpWrapper>(), filenameToDumpRenamed);
                        currentSession.Clear();
                        currentSession.Add(wr);
                        track = YamlParser.Parse(wr.data.SessionInfo, "WeekendInfo:TrackName:");
                        sessionType = YamlParser.Parse(wr.data.SessionInfo, string.Format(SessionData.sessionInfoYamlPath, wr.data.SessionNum, "SessionType"));
                        filenameToDumpRenamed = System.IO.Path.Combine(directory, filename + "-" + track + "-" + sessionType) + extension;
                    }
                }
                SerializeObject(currentSession.ToArray<iRacingStructDumpWrapper>(), filenameToDumpRenamed);
            }
        }
        public override void ResetGameDataFromFile()
        {
            dataReadFromFileIndex = 0;
        }

        public override Object ReadGameDataFromFile(String filename, int pauseBeforeStart)
        {
            if(sim == null)
            {
                sim = new Sim();
            }
            if (dataReadFromFile == null || filename != lastReadFileName)
            {
                dataReadFromFileIndex = 0;
                var filePathResolved = Utilities.ResolveDataFile(this.dataFilesPath, filename);
                dataReadFromFile = DeSerializeObject<iRacingStructDumpWrapper[]>(filePathResolved);
                //dataToDump = dataReadFromFile.ToList<iRacingStructDumpWrapper>();
                //SplitTraceData(filePathResolved);
                lastReadFileName = filename;
                Thread.Sleep(pauseBeforeStart);
            }
            if (dataReadFromFile != null && dataReadFromFile.Length > dataReadFromFileIndex)
            {
                bool IsNewSession = false;
                iRacingStructDumpWrapper structDumpWrapperData = dataReadFromFile[dataReadFromFileIndex];
                if (structDumpWrapperData.data.SessionInfoUpdate != lastUpdate && structDumpWrapperData.data.SessionInfo.Length > 0)
                {
                    IsNewSession = sim.SdkOnSessionInfoUpdated(structDumpWrapperData.data.SessionInfo, structDumpWrapperData.data.SessionNum, structDumpWrapperData.data.PlayerCarIdx, structDumpWrapperData.data.SessionTimeRemain, structDumpWrapperData.data.SessionTime);
                    lastUpdate = structDumpWrapperData.data.SessionInfoUpdate;
                }
                /*if (IsNewSession)
                {
                    Console.WriteLine(structDumpWrapperData.data.SessionInfo);
                }*/
                sim.SdkOnTelemetryUpdated(structDumpWrapperData.data);
                iRacingStructWrapper structWrapperData = new iRacingStructWrapper() { data = sim, ticksWhenRead = structDumpWrapperData.ticksWhenRead };
                structWrapperData.data.Telemetry.IsNewSession = IsNewSession;
                dataReadFromFileIndex++;
                return structWrapperData;
            }
            else
            {
                return null;
            }
        }

        protected override Boolean InitialiseInternal()
        {
            lock (this)
            {
                if (!initialised)
                {
                    try
                    {
                        if (sdk == null)
                        {
                            sdk = new iRacingSDK();
                        }

                        sdk.Shutdown();                       
                        if (!sdk.IsInitialized)
                        {
                            sdk.Startup();
                        }
                        if (sdk.IsConnected())
                        {
                            initialised = true;
                            if (dumpToFile)
                            {
                                dataToDump = new List<iRacingStructDumpWrapper>();
                            }
                            int attempts = 0;
                            const int maxAttempts = 99;
                            //sdk.Generate_iRacingData_cs();
                            object sessionnum = this.TryGetSessionNum();
                            while (sessionnum == null && attempts <= maxAttempts)
                            {
                                attempts++;
                                sessionnum = this.TryGetSessionNum();
                            }
                            if (attempts >= maxAttempts)
                            {
                                Console.WriteLine("Session num too many attempts");
                            }

                            Console.WriteLine("Initialised iRacing shared memory");
                        }
                    }
                    catch (Exception)
                    {
                        initialised = false;
                    }
                }
                return initialised;
            }
        }
        
        public override Object ReadGameData(Boolean forSpotter)
        {
            lock (this)
            {

                if (!initialised)
                {
                    if (!InitialiseInternal())
                    {
                        throw new GameDataReadException("Failed to initialise shared memory");
                    }
                }
                try
                {

                    if (sdk.IsConnected())
                    {
                        if(sim == null)
                        {
                            sim = new Sim();
                        }
                        if (forSpotter)
                        {
                            return (int)sdk.GetData("CarLeftRight");
                        }

                        _DriverId = (int)sdk.GetData("PlayerCarIdx");

                        int newUpdate = sdk.Header.SessionInfoUpdate;
                        bool hasNewSessionData = false;
                        bool isNewSession = false;
                        if (newUpdate != lastUpdate)
                        {
                            var sessionNum = TryGetSessionNum();
                            if(sessionNum != null)
                            {
                                string sessionInfoUnFiltred = sdk.GetSessionInfoString();
                                if(sessionInfoUnFiltred == null)
                                {
                                    return null;
                                }
                                System.Double SessionTimeRemain = (System.Double)sdk.GetData("SessionTimeRemain");
                                System.Double SessionTime = (System.Double)sdk.GetData("SessionTime");
                                string sessionInfoFiltred = new SessionInfo(sessionInfoUnFiltred).Yaml;
                                isNewSession = sim.SdkOnSessionInfoUpdated(sessionInfoFiltred, (int)sessionNum, DriverId, SessionTimeRemain, SessionTime);
                                lastUpdate = newUpdate;
                                hasNewSessionData = true;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        iRacingData irData = new iRacingData(sdk, hasNewSessionData && dumpToFile, isNewSession, numberOfCarsEnabled, is360HzTelemetry);

                        sim.SdkOnTelemetryUpdated(irData);

                        iRacingStructWrapper structWrapper = new iRacingStructWrapper();
                        structWrapper.ticksWhenRead = DateTime.UtcNow.Ticks;
                        structWrapper.data = sim;

                        if (dumpToFile && dataToDump != null)
                        {
                            dataToDump.Add(new iRacingStructDumpWrapper() { ticksWhenRead = structWrapper.ticksWhenRead, data = irData });
                        }
                        
                        return structWrapper;
                    }
                    else
                    {
                        return null;
                    }                    
                }
                catch (Exception ex)
                {
                    throw new GameDataReadException(ex.Message, ex);
                }
            }
        }
        public override void DisconnectFromProcess()
        {
            this.Dispose();
        }
        public override void Dispose()
        {
            lock (this)
            {
                if (sdk != null)
                {
                    sdk.Shutdown();
                    sdk = null;
                }
                if(sim != null)
                {
                    sim = null;
                }
                
                if (initialised)
                {
                    lastUpdate = -1;
                    _DriverId = -1;
                    initialised = false;
                    Console.WriteLine("Disconnected from iRacing Shared Memory");
                }
            }
        }
    }
}
