using CrewChiefV4.ACC.accData;
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

        private DateTime nextScheduleRequestCars = DateTime.MinValue;

        private SPageFileCrewChief mostRecentUDPData = null;

        private Dictionary<int, int> lapsCompletedByCarId = new Dictionary<int, int>();
        private Dictionary<int, float> splinePositionByCarId = new Dictionary<int, float>();
        private HashSet<int> carIdsWaitingForSplineReset = new HashSet<int>();
        private HashSet<int> carIdsWithForcedLapIncrement = new HashSet<int>();

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
                    DateTime now = DateTime.UtcNow;
                    structWrapper.ticksWhenRead = now.Ticks;
                    Boolean isFetchingCars = false;
                    
                    if (now > nextScheduleRequestCars && !forSpotter)
                    {
                        isFetchingCars = true;
                        System.Diagnostics.Debug.WriteLine("Requesting new car list.");
                    }
                    if (isFetchingCars)
                    {
                        nextScheduleRequestCars = now.AddSeconds(10);
                        udpUpdateViewModel.ClientPanelVM.RequestEntryList();
                    }

                    // Send the previous state if the game is paused to prevent bogus track temp and other warnings on unpausing
                    if (isFetchingCars || (accShared.accPhysics.airTemp == 0 && accShared.accPhysics.roadTemp == 0 && 
                        accShared.accPhysics.fuel == 0 && accShared.accPhysics.heading == 0 && accShared.accPhysics.pitch == 0))
                        return previousAACStructWrapper ?? structWrapper;

                    structWrapper.data = accShared;
                    
                    structWrapper.data.accStatic.SET_FROM_UDP_isTimedRace = udpUpdateViewModel.SessionInfoVM.RemainingTime.TotalMilliseconds > 0 ? 1 : 0;

                    // New penality?
                    if (structWrapper.data.accGraphic.penaltyTime != ackPenalityTime)
                    {

                        if (structWrapper.data.accGraphic.penaltyTime > ackPenalityTime && structWrapper.data.accGraphic.flag == AC_FLAG_TYPE.AC_NO_FLAG) // Penality flag not supported yet
                            structWrapper.data.accGraphic.flag = AC_FLAG_TYPE.AC_PENALTY_FLAG;

                        ackPenalityTime = structWrapper.data.accGraphic.penaltyTime;
                    }

                    if (forSpotter && this.mostRecentUDPData != null && this.mostRecentUDPData.vehicle.Length > 0)
                    {
                        for (int i = 0; i < this.mostRecentUDPData.vehicle.Length; i++)
                        {
                            int indexInCoordsArray = Array.IndexOf(structWrapper.data.accGraphic.carIDs, this.mostRecentUDPData.vehicle[i].carId);
                            if (indexInCoordsArray > -1 && indexInCoordsArray < structWrapper.data.accGraphic.carCoordinates.Length)
                            {
                                this.mostRecentUDPData.vehicle[i].worldPosition = structWrapper.data.accGraphic.carCoordinates[indexInCoordsArray];
                            }
                        }
                        structWrapper.data.accChief = this.mostRecentUDPData;
                    }
                    else
                    {
                        // Populate data from the ACC UDP info. We have to lock it because data can be updated while we read it
                        udpUpdateViewModel.LockForReadingAsync(() =>
                        {
                            BroadcastingEvent[] events = udpUpdateViewModel.BroadcastingVM.EventVM.GetEvents();

                            //foreach (var evt in events)
                            //{
                            //GreenFlag = 1,
                            //SessionOver = 2,
                            //PenaltyCommMsg = 3,
                            //Accident = 4,
                            //LapCompleted = 5,
                            //BestSessionLap = 6,
                            //BestPersonalLap = 7

                            //Console.WriteLine($"Event: {evt.Type.ToString()} - {evt.Msg}");
                            //}

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
                            structWrapper.data.accChief.rainLevel = udpUpdateViewModel.SessionInfoVM.RainLevel;
                            structWrapper.data.accChief.cloudCoverPercent = udpUpdateViewModel.SessionInfoVM.CloudCoverPercent;

                            // until we check that a driver's carId is also in the accGraphic.carIDs array, we don't know how long this list will be:
                            LinkedList<accVehicleInfo> activeVehicles = new LinkedList<accVehicleInfo>();
                            structWrapper.data.accChief.vehicle = new accVehicleInfo[udpUpdateViewModel.BroadcastingVM.Cars.Count];

                            List<float> distancesTravelled = new List<float>();
                            // get the player vehicle first and put this at the front of the list
                            var playerVehicle = getPlayerVehicle(udpUpdateViewModel.BroadcastingVM.Cars, accShared.accGraphic.playerCarID,
                                accShared.accStatic, accShared.accGraphic.position);
                            if (playerVehicle != null)
                            {
                                activeVehicles.AddFirst(createCar(1, playerVehicle, structWrapper.data.accGraphic.carIDs, structWrapper.data.accGraphic.carCoordinates));
                                distancesTravelled.Add(playerVehicle.Laps + playerVehicle.SplinePosition);

                                // only add a car to our data set if it exists in the UDP data and the shared memory car IDs array
                                foreach (CarViewModel car in udpUpdateViewModel.BroadcastingVM.Cars)
                                {
                                    if (car != playerVehicle && structWrapper.data.accGraphic.carIDs.Contains(car.CarIndex))
                                    {
                                        activeVehicles.AddLast(createCar(0, car, structWrapper.data.accGraphic.carIDs, structWrapper.data.accGraphic.carCoordinates));
                                        distancesTravelled.Add(car.Laps + car.SplinePosition);
                                    }
                                }
                                // now set the accVehicle array from our list of vehicles that we've deemed to be 'active'
                                structWrapper.data.accChief.vehicle = activeVehicles.ToArray();
                                List<float> sortedDistances = new List<float>(distancesTravelled);
                                sortedDistances.Sort();
                                sortedDistances.Reverse();
                                for (var i=0; i < distancesTravelled.Count; i++)
                                {
                                    int positionFromSpline = sortedDistances.IndexOf(distancesTravelled[i]) + 1;
                                    structWrapper.data.accChief.vehicle[i].carRealTimeLeaderboardPosition = positionFromSpline;
                                }
                                // save the populated driver data so we can reuse it when reading for the spotter
                                this.mostRecentUDPData = structWrapper.data.accChief;
                            }
                        }).Wait();
                    }

                    if (!forSpotter && dumpToFile && dataToDump != null)
                    {
                        dataToDump.Add(structWrapper);
                    }

                    previousAACStructWrapper = structWrapper;

                    return structWrapper;
                }
                catch (AggregateException e1)
                {
                    Console.WriteLine("Inner exception:" + e1.Message + " " + e1.InnerException.StackTrace);
                    throw new GameDataReadException(e1.InnerException.Message, e1.InnerException);
                }
                catch (Exception e2)
                {
                    throw new GameDataReadException(e2.Message, e2);
                }
            }
        }

        public string getCarModel(int carModelEnum)
        {
            // 4 classes in ACC - GT4 (class enum 50 - 61), Porsche Cup (class enum 9) and Huracan Super Trofeo (class enum 18). Everything else is GT3
            if (carModelEnum == 9)
            {
                return "porsche_911_cup";
            }
            else if (carModelEnum == 18)
            {
                return "ks_lamborghini_huracan_st"; // this puts the ST in the GTE class
            }
            else if (carModelEnum >= 50 && carModelEnum <= 61)
            {
                return "GT4";
            }
            return "GT3";
        }

        private accVehicleInfo createCar(int carIsPlayerVehicle, CarViewModel car, int[] carIds, accVec3[] carPositions)
        {
            var currentLap = car.CurrentLap;
            var lastLap = car.LastLap;
            var bestLap = car.BestLap;

            // we only ever add the player to position 0:
            string carDriverName;
            if (car.CurrentDriver != null)
            {
                carDriverName = car.CurrentDriver.DisplayName;
            }
            else
            {
                carDriverName = car.Drivers.Count > 0 ? car.Drivers.First().DisplayName : "";
            }

            // get the position in the CarIDs array
            float x_coord = 0;
            float z_coord = 0;
            int indexInCoordsArray = Array.IndexOf(carIds, car.CarIndex);
            if (indexInCoordsArray > -1 && indexInCoordsArray < carPositions.Length)
            {
                accVec3 carPosition = carPositions[indexInCoordsArray];
                x_coord = carPosition.x;
                z_coord = carPosition.z;
            }
            int correctedLapCount = correctLapCount(car.Laps, car.SplinePosition, car.CarIndex);
            return new accVehicleInfo
            {
                bestLapMS = (bestLap?.IsValid ?? false) ? bestLap.LaptimeMS ?? 0 : 0,
                carId = car.CarIndex,
                carLeaderboardPosition = car.Position,
                carModel = getCarModel(car.CarModelEnum),
                carRealTimeLeaderboardPosition = car.Position,  /* don't be tempted to use TrackPosition here, it's always zero */    
                currentLapInvalid = (currentLap?.IsValid ?? false) ? 0 : 1,
                currentLapTimeMS = currentLap?.LaptimeMS ?? 0,
                isPlayerVehicle = carIsPlayerVehicle,
                driverName = carDriverName,
                isCarInPit = (car.CarLocation == CarLocationEnum.PitEntry || car.CarLocation == CarLocationEnum.PitExit || car.CarLocation == CarLocationEnum.Pitlane) ? 1 : 0,
                isCarInPitline = (car.CarLocation == CarLocationEnum.Pitlane) ? 1 : 0,
                isConnected = 1,
                lapCount = correctedLapCount,
                lastLapTimeMS = (lastLap?.IsValid ?? false) ? lastLap.LaptimeMS ?? 0 : 0,
                speedMS = car.Kmh * 0.277778f,
                spLineLength = car.SplinePosition,
                worldPosition = new accVec3 { x = x_coord, z = z_coord },
                raceNumber = car.RaceNumber
            };            
        }
        
        private int correctLapCount(int lapCountFromData, float splineFromData, int carId)
        {
            int correctedLapCount = lapCountFromData;
            // 2 cases: one where the spline position is small (<0.1) and one where it's large (> 0.9). This is a bit arbitrary but it should be
            // enough to catch the 2 possible cases
            int previousLapsCompleted;
            float previousSplinePosition;
            // only proceed if we have data and we've completed 1 or more laps. Can't apply this correction
            // on lap 1 because we don't know the difference between crossing the startline at the race start and crossing it after lap 1
            if (lapCountFromData > 0 && lapsCompletedByCarId.TryGetValue(carId, out previousLapsCompleted) && splinePositionByCarId.TryGetValue(carId, out previousSplinePosition))
            {
                if (splineFromData > 0.9 && lapCountFromData > previousLapsCompleted && !carIdsWaitingForSplineReset.Contains(carId))
                {
                    // we've not yet crossed the line according to the spline position but the lap count has been incremented so we're waiting for the spline position to reset
                    carIdsWaitingForSplineReset.Add(carId);
                    carIdsWithForcedLapIncrement.Remove(carId);
                    correctedLapCount = previousLapsCompleted;
                    Console.WriteLine("Car " + carId + " lapcount increment blocked, waiting for spline to reset from " + splineFromData);
                }
                else if (splineFromData < 0.1 && splineFromData > 0)
                {
                    if (carIdsWithForcedLapIncrement.Contains(carId))
                    {
                        // we're in the early part of the lap and we've already established that the game was late sending the lap increment so use
                        // the corrected value we saved
                        correctedLapCount = lapsCompletedByCarId[carId];
                    }
                    else if (previousSplinePosition > 0.9)
                    {
                        // we've just crossed the line, see if we're waiting to increment or if we're late incrementing
                        if (carIdsWaitingForSplineReset.Contains(carId))
                        {
                            // we can now allow the game's updated lap count to be used
                            carIdsWaitingForSplineReset.Remove(carId);
                            carIdsWithForcedLapIncrement.Remove(carId);
                            lapsCompletedByCarId[carId] = lapCountFromData;
                            Console.WriteLine("Car " + carId + " lapcount incremented after spline to reset");
                        }
                        else if (lapCountFromData == previousLapsCompleted && !carIdsWithForcedLapIncrement.Contains(carId))
                        {
                            // we've just crossed the line, our lap count should have been incremented but hasn't
                            correctedLapCount = lapCountFromData + 1;
                            carIdsWaitingForSplineReset.Remove(carId);
                            carIdsWithForcedLapIncrement.Add(carId);
                            lapsCompletedByCarId[carId] = correctedLapCount;
                            Console.WriteLine("Car " + carId + " spline reset but lapcount hasn't incremented, forcing increment");
                        }
                    }
                }
                else
                {
                    // we're not in the interesting area of the lap so reset all the cached stuff
                    carIdsWaitingForSplineReset.Remove(carId);
                    carIdsWithForcedLapIncrement.Remove(carId);
                    lapsCompletedByCarId[carId] = lapCountFromData;
                }
            }
            else
            {
                lapsCompletedByCarId[carId] = lapCountFromData;
            }
            splinePositionByCarId[carId] = splineFromData;
            return correctedLapCount;
        }

        private CarViewModel getPlayerVehicle(List<CarViewModel> cars, int carId, SPageFileStatic accStatic, int positionFromSharedMem)
        {
            foreach (var car in cars)
            {
                if (car.CarIndex == carId)
                {
                    return car;
                }
            }
            int positionDiff = int.MaxValue;
            CarViewModel bestMatch = null;
            foreach (var car in cars)
            {
                foreach (var driver in car.Drivers)
                {
                    if (accStatic.playerName + " " + accStatic.playerSurname == driver.DisplayName)
                    {
                        // check the positions match
                        if (positionFromSharedMem == car.Position)
                        {
                            // yay, this is probably the player. Probably :(
                            return car;
                        }
                        else 
                        {
                            // the names match but the position doesn't so we need to check the rest of the array
                            int thisPositionDiff = Math.Abs(positionFromSharedMem - car.Position);
                            if (thisPositionDiff < positionDiff)
                            {
                                positionDiff = thisPositionDiff;
                                bestMatch = car;
                            }
                        }
                    }
                }
            }

            return bestMatch;
        }

        private accVehicleInfo[] getPopulatedDriverDataArray(accVehicleInfo[] raw)
        {
            List<accVehicleInfo> populated = new List<accVehicleInfo>();
            if (raw != null && raw.Count() > 0)
            {
                foreach (accVehicleInfo rawData in raw)
                {
                    if (rawData.carLeaderboardPosition > 0 || rawData.isPlayerVehicle == 1)
                    {
                        populated.Add(rawData);
                    }
                }
                if (populated.Count == 0)
                {
                    populated.Add(raw[0]);
                }
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
                catch (Exception e) {Log.Exception(e);}
            }
            if (memoryMappedGraphicFile != null)
            {
                try
                {
                    memoryMappedGraphicFile.Dispose();
                    memoryMappedGraphicFile = null;
                }
                catch (Exception e) {Log.Exception(e);}
            }
            if (memoryMappedStaticFile != null)
            {
                try
                {
                    memoryMappedStaticFile.Dispose();
                    memoryMappedStaticFile = null;
                }
                catch (Exception e) {Log.Exception(e);}
            }
            if (udpUpdateViewModel != null)
            {
                try
                {
                    udpUpdateViewModel.Shutdown();
                }
                catch (Exception e) {Log.Exception(e);}
            }
            initialised = false;
        }
    }
}