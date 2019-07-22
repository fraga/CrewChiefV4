﻿using CrewChiefV4.ACC.accData;
using ksBroadcastingNetwork;
using ksBroadcastingNetwork.Structs;
using ksBroadcastingTestClient;
using ksBroadcastingTestClient.Broadcasting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;


namespace CrewChiefV4.ACC
{
    public class ACCSharedMemoryReader : GameDataReader
    {
        private MemoryMappedFile memoryMappedPhysicsFile;
        private int sharedmemoryPhysicssize;
        private byte[] sharedMemoryPhysicsReadBuffer;
        private GCHandle handlePhysics;

        private MemoryMappedFile memoryMappedGraphicFile;
        private int sharedmemoryGraphicsize;
        private byte[] sharedMemoryGraphicReadBuffer;
        private GCHandle handleGraphic;

        private MemoryMappedFile memoryMappedStaticFile;
        private int sharedmemoryStaticsize;
        private byte[] sharedMemoryStaticReadBuffer;

        private GCHandle handleStatic;

        private static UdpUpdateViewModel udpUpdateViewModel;

        private Boolean initialised = false;
        private List<ACCStructWrapper> dataToDump;
        private ACCStructWrapper[] dataReadFromFile = null;
        private int dataReadFromFileIndex = 0;
        private String lastReadFileName = null;
        private ACCStructWrapper previousAACStructWrapper; // Used when no data is comming in. EG: game is paused
        private float ackPenalityTime;
        private bool checkedUdpConfig;

        public class ACCStructWrapper
        {
            public long ticksWhenRead;
            public ACCShared data;
        }

        public override void DumpRawGameData()
        {
            if (dumpToFile && dataToDump != null && dataToDump.Count > 0 && filenameToDump != null)
            {
                foreach (ACCStructWrapper wrapper in dataToDump)
                {
                    wrapper.data.accChief.vehicle = getPopulatedDriverDataArray(wrapper.data.accChief.vehicle);
                }
                SerializeObject(dataToDump.ToArray<ACCStructWrapper>(), filenameToDump);
            }
        }

        public override void ResetGameDataFromFile()
        {
            dataReadFromFileIndex = 0;
        }

        public override Object ReadGameDataFromFile(String filename, int pauseBeforeStart)
        {
            if (dataReadFromFile == null || filename != lastReadFileName)
            {
                dataReadFromFileIndex = 0;
                var filePathResolved = Utilities.ResolveDataFile(this.dataFilesPath, filename);
                dataReadFromFile = DeSerializeObject<ACCStructWrapper[]>(filePathResolved);
                lastReadFileName = filename;
                Thread.Sleep(pauseBeforeStart);
            }
            if (dataReadFromFile != null && dataReadFromFile.Length > dataReadFromFileIndex)
            {
                ACCStructWrapper structWrapperData = dataReadFromFile[dataReadFromFileIndex];
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
            if (dumpToFile)
            {
                dataToDump = new List<ACCStructWrapper>();
            }
            lock (this)
            {
                if (!initialised)
                {
                    try
                    {
                        if (!checkedUdpConfig)
                        {
                            checkedUdpConfig = true;
                            var broadcastJsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Assetto Corsa Competizione\Config\broadcasting.json");

                            try
                            {
                                if (!File.Exists(broadcastJsonPath))
                                    throw new ApplicationException();

                                dynamic broadcastingJson = null;

                                // My file was unicode. But it might change depending on what users edit it with. I think one of these should work.
                                try { broadcastingJson = JObject.Parse(File.ReadAllText(broadcastJsonPath, Encoding.Unicode)); } catch { }

                                if (broadcastingJson == null)
                                    try { broadcastingJson = JObject.Parse(File.ReadAllText(broadcastJsonPath, Encoding.UTF8)); } catch { }

                                if (broadcastingJson == null)
                                    try { broadcastingJson = JObject.Parse(File.ReadAllText(broadcastJsonPath, Encoding.ASCII)); } catch { }

                                if (broadcastingJson == null)
                                    throw new ApplicationException();

                                if (broadcastingJson.updListenerPort != 9000 ||
                                    broadcastingJson.connectionPassword != "asd" ||
                                    broadcastingJson.commandPassword != "")
                                    throw new ApplicationException();
                            }
                            catch
                            {
                                Console.WriteLine("****************************************************");
                                Console.WriteLine("The Broadcasting.json file is missing or not configured correctly.");
                                Console.WriteLine($"The file is located at {broadcastJsonPath}.");
                                Console.WriteLine("Crew Chief will not run correctly until the file contains the following (between and including the curly braces):\r\n" + 
                                                  "{\n" +
                                                  "    \"updListenerPort\": 9000,\n" + 
                                                  "    \"connectionPassword\": \"asd\",\n" + 
                                                  "    \"commandPassword\": \"\"\n" + 
                                                  "}\n");
                                Console.WriteLine("****************************************************");
                            }
                        }

                        memoryMappedPhysicsFile = MemoryMappedFile.OpenExisting(accConstant.SharedMemoryNamePhysics);
                        sharedmemoryPhysicssize = Marshal.SizeOf(typeof(SPageFilePhysics));
                        sharedMemoryPhysicsReadBuffer = new byte[sharedmemoryPhysicssize];

                        memoryMappedGraphicFile = MemoryMappedFile.OpenExisting(accConstant.SharedMemoryNameGraphic);
                        sharedmemoryGraphicsize = Marshal.SizeOf(typeof(SPageFileGraphic));
                        sharedMemoryGraphicReadBuffer = new byte[sharedmemoryGraphicsize];

                        memoryMappedStaticFile = MemoryMappedFile.OpenExisting(accConstant.SharedMemoryNameStatic);
                        sharedmemoryStaticsize = Marshal.SizeOf(typeof(SPageFileStatic));
                        sharedMemoryStaticReadBuffer = new byte[sharedmemoryStaticsize];

                        udpUpdateViewModel = new UdpUpdateViewModel(UserSettings.GetUserSettings().getString(accConstant.SettingMachineIpAddress));

                        initialised = true;
                        Console.WriteLine("Initialised Assetto Corsa Competizione shared memory");
                    }
                    catch
                    {
                        initialised = false;
                    }
                }
                return initialised;
            }
        }
        public static String getNameFromBytes(byte[] name)
        {
            return Encoding.Unicode.GetString(name);
        }

        public override Object ReadGameData(Boolean forSpotter)
        {
            lock (this)
            {
                ACCShared accShared = new ACCShared();
                if (!initialised)
                {
                    if (!InitialiseInternal())
                    {
                        throw new GameDataReadException("Failed to initialise shared memory");
                    }
                }
                try
                {
                    using (var sharedMemoryStreamView = memoryMappedStaticFile.CreateViewStream())
                    {
                        BinaryReader _SharedMemoryStream = new BinaryReader(sharedMemoryStreamView);
                        sharedMemoryStaticReadBuffer = _SharedMemoryStream.ReadBytes(sharedmemoryStaticsize);
                        handleStatic = GCHandle.Alloc(sharedMemoryStaticReadBuffer, GCHandleType.Pinned);
                        accShared.accStatic = (SPageFileStatic)Marshal.PtrToStructure(handleStatic.AddrOfPinnedObject(), typeof(SPageFileStatic));
                        handleStatic.Free();
                    }
                    using (var sharedMemoryStreamView = memoryMappedGraphicFile.CreateViewStream())
                    {
                        BinaryReader _SharedMemoryStream = new BinaryReader(sharedMemoryStreamView);
                        sharedMemoryGraphicReadBuffer = _SharedMemoryStream.ReadBytes(sharedmemoryGraphicsize);
                        handleGraphic = GCHandle.Alloc(sharedMemoryGraphicReadBuffer, GCHandleType.Pinned);
                        accShared.accGraphic = (SPageFileGraphic)Marshal.PtrToStructure(handleGraphic.AddrOfPinnedObject(), typeof(SPageFileGraphic));
                        handleGraphic.Free();
                    }

                    using (var sharedMemoryStreamView = memoryMappedPhysicsFile.CreateViewStream())
                    {
                        BinaryReader _SharedMemoryStream = new BinaryReader(sharedMemoryStreamView);
                        sharedMemoryPhysicsReadBuffer = _SharedMemoryStream.ReadBytes(sharedmemoryPhysicssize);
                        handlePhysics = GCHandle.Alloc(sharedMemoryPhysicsReadBuffer, GCHandleType.Pinned);
                        accShared.accPhysics = (SPageFilePhysics)Marshal.PtrToStructure(handlePhysics.AddrOfPinnedObject(), typeof(SPageFilePhysics));
                        handlePhysics.Free();
                    }

                    accShared.accChief = new SPageFileCrewChief();

                    ACCStructWrapper structWrapper = new ACCStructWrapper();
                    structWrapper.ticksWhenRead = DateTime.UtcNow.Ticks;

                    var isFetchingCars = udpUpdateViewModel.BroadcastingVM.Cars.Count == 0;

                    if (!isFetchingCars && accShared.accStatic.numCars != udpUpdateViewModel.BroadcastingVM.Cars.Count)
                    {
                        if (udpUpdateViewModel.BroadcastingVM.Cars.Count != 0)
                        {
                            isFetchingCars = true;
                            System.Diagnostics.Debug.WriteLine("Requesting new car list.");
                            udpUpdateViewModel.ClientPanelVM.RequestEntryList();
                        }
                    }

                    // Send the previous state if the game is paused to prevent bogus track temp and other warnings on unpausing
                    if (isFetchingCars || accShared.accPhysics.airTemp == 0 && accShared.accPhysics.roadTemp == 0 && accShared.accPhysics.fuel == 0 && accShared.accPhysics.heading == 0 && accShared.accPhysics.pitch == 0)
                        return previousAACStructWrapper ?? structWrapper;

                    // Tyre missing data fixups
                    accShared.accPhysics.tyreTempI = accShared.accPhysics.tyreTempM;
                    accShared.accPhysics.tyreTempO = accShared.accPhysics.tyreTempM;

                    structWrapper.data = accShared;

                    if (!forSpotter && dumpToFile && dataToDump != null)
                    {
                        dataToDump.Add(structWrapper);
                    }

                    structWrapper.data.accStatic.isTimedRace = udpUpdateViewModel.SessionInfoVM.RemainingTime.TotalMilliseconds > 0 ? 1 : 0;

                    // New penality?
                    if (structWrapper.data.accGraphic.penaltyTime != ackPenalityTime)
                    {

                        if (structWrapper.data.accGraphic.penaltyTime > ackPenalityTime && structWrapper.data.accGraphic.flag == AC_FLAG_TYPE.AC_NO_FLAG) // Penality flag not supported yet
                            structWrapper.data.accGraphic.flag = AC_FLAG_TYPE.AC_PENALTY_FLAG;

                        ackPenalityTime = structWrapper.data.accGraphic.penaltyTime;
                    }

                    // Populate data from the ACC UDP info. We have to lock it because data can be updated while we read it
                    udpUpdateViewModel.LockForReadingAsync(() =>
                    {
                        BroadcastingEvent[] events = udpUpdateViewModel.BroadcastingVM.EventVM.GetEvents();

                        foreach (var evt in events)
                        {
                        //GreenFlag = 1,
                        //SessionOver = 2,
                        //PenaltyCommMsg = 3,
                        //Accident = 4,
                        //LapCompleted = 5,
                        //BestSessionLap = 6,
                        //BestPersonalLap = 7

                        Console.WriteLine($"Event: {evt.Type.ToString()} - {evt.Msg}");
                        }

                        switch (udpUpdateViewModel.SessionInfoVM.SessionType)
                        {
                            case RaceSessionType.Practice:
                                structWrapper.data.accGraphic.session = AC_SESSION_TYPE.AC_PRACTICE;
                                break;
                            case RaceSessionType.Qualifying:
                                structWrapper.data.accGraphic.session = AC_SESSION_TYPE.AC_QUALIFY;
                                break;
                            case RaceSessionType.Race:
                                structWrapper.data.accGraphic.session = AC_SESSION_TYPE.AC_RACE;
                                break;
                            case RaceSessionType.Hotlap:
                            case RaceSessionType.HotlapSuperpole:
                            case RaceSessionType.Hotstint:
                            case RaceSessionType.Superpole:
                                structWrapper.data.accGraphic.session = AC_SESSION_TYPE.AC_HOTLAP;
                                break;
                            default:
                                structWrapper.data.accGraphic.session = AC_SESSION_TYPE.AC_HOTLAP;
                                break;
                        }

                        switch (udpUpdateViewModel.SessionInfoVM.Phase)
                        {
                            case SessionPhase.SessionOver:
                                structWrapper.data.accChief.SessionPhase = GameState.SessionPhase.Checkered;
                                break;
                            case SessionPhase.PreSession:
                            case SessionPhase.Starting:
                            case SessionPhase.PreFormation:
                                structWrapper.data.accChief.SessionPhase = GameState.SessionPhase.Countdown;
                                break;
                            case SessionPhase.PostSession:
                                structWrapper.data.accChief.SessionPhase = GameState.SessionPhase.Finished;
                                break;
                            case SessionPhase.FormationLap:
                                structWrapper.data.accChief.SessionPhase = GameState.SessionPhase.Formation;
                                break;
                            case SessionPhase.Session:
                                structWrapper.data.accChief.SessionPhase = GameState.SessionPhase.Green;
                                break;
                            default:
                                structWrapper.data.accChief.SessionPhase = GameState.SessionPhase.Unavailable;
                                break;
                        }

                        structWrapper.data.accChief.serverName = ""; // udpUpdateViewModel.BroadcastingVM.EventVM.Evt.;

                    structWrapper.data.accChief.isInternalMemoryModuleLoaded = 1;
                        structWrapper.data.accChief.trackLength = udpUpdateViewModel.BroadcastingVM.TrackVM?.TrackMeters ?? 0;
                        structWrapper.data.accChief.isRaining = udpUpdateViewModel.SessionInfoVM.RainLevel < 0.1 && udpUpdateViewModel.SessionInfoVM.WetnessLevel < 0.1;
                        structWrapper.data.accChief.vehicle = new accVehicleInfo[structWrapper.data.accStatic.numCars];

                        var playerVehicle = getPlayerVehicle(udpUpdateViewModel.BroadcastingVM.Cars, accShared.accStatic);

                        for (var i = 0; i < udpUpdateViewModel.BroadcastingVM.Cars.Count; i++)
                        {
                            var car = udpUpdateViewModel.BroadcastingVM.Cars[i];

                            var currentLap = car.CurrentLap;
                            var lastLap = car.LastLap;
                            var bestLap = car.BestLap;

                            structWrapper.data.accChief.vehicle[i] = new accVehicleInfo
                            {
                                bestLapMS = (bestLap?.IsValid ?? false) ? bestLap.LaptimeMS ?? 0 : 0,
                                carId = car.CarIndex,
                                carLeaderboardPosition = car.Position,
                                carModel = structWrapper.data.accStatic.carModel, //car.CarModelEnum?
                            carRealTimeLeaderboardPosition = car.Position,
                                currentLapInvalid = (currentLap?.IsValid ?? false) ? 0 : 1,
                                currentLapTimeMS = currentLap?.LaptimeMS ?? 0,
                                isPlayerVehicle = (playerVehicle != null && playerVehicle.CarIndex == car.CarIndex) ? 1 : 0,
                                driverName = (playerVehicle.CurrentDriver ?? car.Drivers.First()).DisplayName,
                                isCarInPit = (car.CarLocation == CarLocationEnum.PitEntry || car.CarLocation == CarLocationEnum.PitExit || car.CarLocation == CarLocationEnum.Pitlane) ? 1 : 0,
                                isCarInPitline = (car.CarLocation == CarLocationEnum.Pitlane) ? 1 : 0,
                                isConnected = 1,
                                lapCount = car.Laps,
                                lastLapTimeMS = (lastLap?.IsValid ?? false) ? lastLap.LaptimeMS ?? 0 : 0,
                                speedMS = car.Kmh * 0.277778f,
                                spLineLength = car.SplinePosition,
                                worldPosition = new accVec3 { x = car.WorldX, z = car.WorldY },
                                tyreInflation = i == 0 ? accShared.accPhysics.wheelsPressure : new float[4]
                            };
                        }
                    }).Wait();

                    previousAACStructWrapper = structWrapper;

                    return structWrapper;
                }
                catch (Exception ex)
                {
                    throw new GameDataReadException(ex.Message, ex);
                }
            }
        }

        private CarViewModel getPlayerVehicle(List<CarViewModel> cars, SPageFileStatic accStatic)
        {
            foreach (var car in cars)
            {
                foreach (var driver in car.Drivers)
                {
                    if (accStatic.playerName + " " + accStatic.playerSurname == driver.DisplayName)
                        return car;
                }
            }

            return null;
        }

        private accVehicleInfo[] getPopulatedDriverDataArray(accVehicleInfo[] raw)
        {
            List<accVehicleInfo> populated = new List<accVehicleInfo>();
            foreach (accVehicleInfo rawData in raw)
            {
                if (rawData.carLeaderboardPosition > 0)
                {
                    populated.Add(rawData);
                }
            }
            if (populated.Count == 0)
            {
                populated.Add(raw[0]);
            }
            return populated.ToArray();
        }

        public override void Dispose()
        {
            if (memoryMappedPhysicsFile != null)
            {
                try
                {
                    memoryMappedPhysicsFile.Dispose();
                    memoryMappedPhysicsFile = null;
                }
                catch (Exception) { }
            }
            if (memoryMappedGraphicFile != null)
            {
                try
                {
                    memoryMappedGraphicFile.Dispose();
                    memoryMappedGraphicFile = null;
                }
                catch (Exception) { }
            }
            if (memoryMappedStaticFile != null)
            {
                try
                {
                    memoryMappedStaticFile.Dispose();
                    memoryMappedStaticFile = null;
                }
                catch (Exception) { }
            }
            if (udpUpdateViewModel != null)
            {
                try
                {
                    udpUpdateViewModel.Shutdown();
                }
                catch (Exception) { }
            }
            initialised = false;
        }
    }
}