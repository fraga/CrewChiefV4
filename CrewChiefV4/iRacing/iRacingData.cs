using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using iRSDKSharp;
namespace CrewChiefV4.iRacing
{
	[Serializable]
	public class iRacingData
	{
        public iRacingData(iRacingSDK sdk, bool hasNewSessionData, bool isNewSession, int numberOfCarsEnabled, bool is360HzTelemetry)
		{
			if(hasNewSessionData)
			{
				SessionInfo = new SessionInfo(sdk.GetSessionInfoString()).Yaml;
			}
			else
			{
				SessionInfo = "";
			}
            NumberOfCarsEnabled = numberOfCarsEnabled;
            Is360HzTelemetry = is360HzTelemetry;
			SessionInfoUpdate = sdk.Header.SessionInfoUpdate;
			IsNewSession = isNewSession;
			SessionTime = (System.Double)sdk.GetData("SessionTime");
            SessionTick = (System.Int32)sdk.GetData("SessionTick");
            SessionNum = (System.Int32)sdk.GetData("SessionNum");
			SessionState = (CrewChiefV4.iRacing.SessionStates)sdk.GetData("SessionState");
            SessionFlags = (System.UInt32)sdk.GetData("SessionFlags");
			SessionTimeRemain = (System.Double)sdk.GetData("SessionTimeRemain");
			IsOnTrack = (System.Boolean)sdk.GetData("IsOnTrack");
            IsReplayPlaying = (System.Boolean)sdk.GetData("IsReplayPlaying");
            IsDiskLoggingEnabled = (System.Boolean)sdk.GetData("IsDiskLoggingEnabled");
            IsDiskLoggingActive = (System.Boolean)sdk.GetData("IsDiskLoggingActive");
            PlayerCarPosition = (System.Int32)sdk.GetData("PlayerCarPosition");
			PlayerTrackSurface = (CrewChiefV4.iRacing.TrackSurfaces)sdk.GetData("PlayerTrackSurface");
			PlayerCarIdx = (System.Int32)sdk.GetData("PlayerCarIdx");
			PlayerCarTeamIncidentCount = (System.Int32)sdk.GetData("PlayerCarTeamIncidentCount");
			PlayerCarMyIncidentCount = (System.Int32)sdk.GetData("PlayerCarMyIncidentCount");
			PlayerCarDriverIncidentCount = (System.Int32)sdk.GetData("PlayerCarDriverIncidentCount");
			CarIdxLap = (System.Int32[])sdk.GetData("CarIdxLap");
			CarIdxLapCompleted = (System.Int32[])sdk.GetData("CarIdxLapCompleted");
			CarIdxLapDistPct = (System.Single[])sdk.GetData("CarIdxLapDistPct");
			CarIdxTrackSurface = (CrewChiefV4.iRacing.TrackSurfaces[])sdk.GetData("CarIdxTrackSurface");
			CarIdxOnPitRoad = (System.Boolean[])sdk.GetData("CarIdxOnPitRoad");
			CarIdxPosition = (System.Int32[])sdk.GetData("CarIdxPosition");
			CarIdxClassPosition = (System.Int32[])sdk.GetData("CarIdxClassPosition");
			OnPitRoad = (System.Boolean)sdk.GetData("OnPitRoad");
            CarIdxSteer = (System.Single[])sdk.GetData("CarIdxSteer");
            CarIdxRPM = (System.Single[])sdk.GetData("CarIdxRPM");
			CarIdxGear = (System.Int32[])sdk.GetData("CarIdxGear");
            DRS_Status = (DrsStatus)sdk.GetData("DRS_Status");
            SteeringWheelAngle = (System.Single)sdk.GetData("SteeringWheelAngle");
            Throttle = (System.Single)sdk.GetData("Throttle");
			Brake = (System.Single)sdk.GetData("Brake");
			Clutch = (System.Single)sdk.GetData("Clutch");
			Gear = (System.Int32)sdk.GetData("Gear");
            RPM = (System.Single)sdk.GetData("RPM");
            Lap = (System.Int32)sdk.GetData("Lap");
            LapBestLap = (System.Int32)sdk.GetData("LapBestLap");
            LapBestLapTime = (System.Single)sdk.GetData("LapBestLapTime");
            LapLastLapTime = (System.Single)sdk.GetData("LapLastLapTime");
			LapCurrentLapTime = (System.Single)sdk.GetData("LapCurrentLapTime");
			Speed = (System.Single)sdk.GetData("Speed");
			Yaw = (System.Single)sdk.GetData("Yaw");
			Pitch = (System.Single)sdk.GetData("Pitch");
			Roll = (System.Single)sdk.GetData("Roll");
			TrackTempCrew = (System.Single)sdk.GetData("TrackTempCrew");
			AirTemp = (System.Single)sdk.GetData("AirTemp");
			WindVel = (System.Single)sdk.GetData("WindVel");
			CarLeftRight = (CrewChiefV4.iRacing.CarLeftRight)sdk.GetData("CarLeftRight");
            PitsOpen = (System.Boolean)sdk.GetData("PitsOpen");
			IsInGarage = (System.Boolean)sdk.GetData("IsInGarage");
			EngineWarnings = (CrewChiefV4.iRacing.EngineWarnings)sdk.GetData("EngineWarnings");
			FuelLevel = (System.Single)sdk.GetData("FuelLevel");
			WaterTemp = (System.Single)sdk.GetData("WaterTemp");
			WaterLevel = (System.Single)sdk.GetData("WaterLevel");
			FuelPress = (System.Single)sdk.GetData("FuelPress");
			OilTemp = (System.Single)sdk.GetData("OilTemp");
			Voltage = (System.Single)sdk.GetData("Voltage");
			RRcoldPressure = (System.Single)sdk.GetData("RRcoldPressure");
			LRcoldPressure = (System.Single)sdk.GetData("LRcoldPressure");
			RFcoldPressure = (System.Single)sdk.GetData("RFcoldPressure");
			LFcoldPressure = (System.Single)sdk.GetData("LFcoldPressure");
            if (Is360HzTelemetry)
            {
                _VertAccel = (System.Single[])sdk.GetData("VertAccel");
                _LatAccel = (System.Single[])sdk.GetData("LatAccel");
                _LongAccel = (System.Single[])sdk.GetData("LongAccel");

                /*
                _RRshockDefl = (System.Single[])sdk.GetData("RRshockDefl");
                _RRshockVel = (System.Single[])sdk.GetData("RRshockVel");

                _LRshockDefl = (System.Single[])sdk.GetData("LRshockDefl");
                _LRshockVel = (System.Single[])sdk.GetData("LRshockVel");

                _RFshockDefl = (System.Single[])sdk.GetData("RFshockDefl");
                _RFshockVel = (System.Single[])sdk.GetData("RFshockVel");  
                
                _LFshockDefl = (System.Single[])sdk.GetData("LFshockDefl");
                _LFshockVel = (System.Single[])sdk.GetData("LFshockVel");
                */
            }
            else
            {
                _VertAccel = new System.Single[1];
                _VertAccel[0] =  (System.Single)sdk.GetData("VertAccel");
                _LatAccel = new System.Single[1];
                _LatAccel[0] = (System.Single)sdk.GetData("LatAccel");
                _LongAccel = new System.Single[1];
                _LongAccel[0] = (System.Single)sdk.GetData("LongAccel");

                /*
                _RRshockDefl = new System.Single[1];
                _RRshockDefl[0] = (System.Single)sdk.GetData("RRshockDefl");
                _RRshockVel = new System.Single[1];
                _RRshockVel[0] = (System.Single)sdk.GetData("RRshockVel");

                _LRshockDefl = new System.Single[1];
                _LRshockDefl[0] = (System.Single)sdk.GetData("LRshockDefl");
                _LRshockVel = new System.Single[1];
                _LRshockVel[0] = (System.Single)sdk.GetData("LRshockVel");

                _RFshockDefl = new System.Single[1];
                _RFshockDefl[0] = (System.Single)sdk.GetData("RFshockDefl");
                _RFshockVel = new System.Single[1];
                _RFshockVel[0] = (System.Single)sdk.GetData("RFshockVel");

                _LFshockDefl = new System.Single[1];
                _LFshockDefl[0] = (System.Single)sdk.GetData("LFshockDefl");
                _LFshockVel = new System.Single[1];
                _LFshockVel[0] = (System.Single)sdk.GetData("LFshockVel");
                */

            }
            PitSvFlags = (CrewChiefV4.iRacing.PitServiceFlags)sdk.GetData("PitSvFlags");
            PitSvLFP = (System.Single)sdk.GetData("PitSvLFP");
            PitSvRFP = (System.Single)sdk.GetData("PitSvRFP");
            PitSvLRP = (System.Single)sdk.GetData("PitSvLRP");
            PitSvRRP = (System.Single)sdk.GetData("PitSvRRP");
            PitSvFuel = (System.Single)sdk.GetData("PitSvFuel");
            PitRepairLeft = (System.Single)sdk.GetData("PitRepairLeft");
            PitOptRepairLeft = (System.Single)sdk.GetData("PitOptRepairLeft");
            PlayerCarTowTime = (System.Single)sdk.GetData("PlayerCarTowTime");
            PlayerCarInPitStall = (System.Boolean)sdk.GetData("PlayerCarInPitStall");
            PlayerCarPitSvStatus = (CrewChiefV4.iRacing.PitSvStatus)sdk.GetData("PlayerCarPitSvStatus");
        }
		public iRacingData() {}
		public System.Boolean IsNewSession;
		public System.Int32 SessionInfoUpdate;
		public System.String SessionInfo;
        public System.Int32 NumberOfCarsEnabled;
        public System.Boolean Is360HzTelemetry; 
		/// <summary>
		/// Seconds since session start
		/// <summary>
		public System.Double SessionTime;
        
        /// <summary>
        /// Current update number
        /// <summary>
        public System.Int32 SessionTick;

        /// <summary>
        /// Session number
        /// <summary>
        public System.Int32 SessionNum;

		/// <summary>
		/// Session state
		/// <summary>
		public CrewChiefV4.iRacing.SessionStates SessionState;

		/// <summary>
		/// Session flags
		/// <summary>
		public System.UInt32 SessionFlags;

		/// <summary>
		/// Seconds left till session ends
		/// <summary>
		public System.Double SessionTimeRemain;

		/// <summary>
		/// 1=Car on track physics running with player in car
		/// <summary>
		public System.Boolean IsOnTrack;

        /// <summary>
        /// 0=replay not playing  1=replay playing
        /// <summary>
        public System.Boolean IsReplayPlaying;

        /// <summary>
        /// 0=disk based telemetry turned off  1=turned on
        /// <summary>
        public System.Boolean IsDiskLoggingEnabled;

        /// <summary>
        /// 0=disk based telemetry file not being written  1=being written
        /// <summary>
        public System.Boolean IsDiskLoggingActive;
        /// <summary>
        /// Players position in race
        /// <summary>
        public System.Int32 PlayerCarPosition;

		/// <summary>
		/// Players car track surface type
		/// <summary>
		public CrewChiefV4.iRacing.TrackSurfaces PlayerTrackSurface;

		/// <summary>
		/// Players carIdx
		/// <summary>
		public System.Int32 PlayerCarIdx;

		/// <summary>
		/// Players team incident count for this session
		/// <summary>
		public System.Int32 PlayerCarTeamIncidentCount;

		/// <summary>
		/// Players own incident count for this session
		/// <summary>
		public System.Int32 PlayerCarMyIncidentCount;

		/// <summary>
		/// Teams current drivers incident count for this session
		/// <summary>
		public System.Int32 PlayerCarDriverIncidentCount;

		/// <summary>
		/// Laps started by car index
		/// <summary>
		public System.Int32[] CarIdxLap;

		/// <summary>
		/// Laps completed by car index
		/// <summary>
		public System.Int32[] CarIdxLapCompleted;

		/// <summary>
		/// Percentage distance around lap by car index
		/// <summary>
		public System.Single[] CarIdxLapDistPct;

		/// <summary>
		/// Track surface type by car index
		/// <summary>
		public CrewChiefV4.iRacing.TrackSurfaces[] CarIdxTrackSurface;

		/// <summary>
		/// Track surface material type by car index
		/// <summary>
		public CrewChiefV4.iRacing.TrackSurfaceMaterial[] CarIdxTrackSurfaceMaterial;

		/// <summary>
		/// On pit road between the cones by car index
		/// <summary>
		public System.Boolean[] CarIdxOnPitRoad;

		/// <summary>
		/// Cars position in race by car index
		/// <summary>
		public System.Int32[] CarIdxPosition;

		/// <summary>
		/// Cars class position in race by car index
		/// <summary>
		public System.Int32[] CarIdxClassPosition;

		/// <summary>
		/// Is the player car on pit road between the cones
		/// <summary>
		public System.Boolean OnPitRoad;

        /// <summary>
        /// Steering wheel angle by car index
        /// <summary
        public System.Single[] CarIdxSteer;
        /// <summary>
        /// Engine rpm by car index
        /// <summary>
        public System.Single[] CarIdxRPM;

		/// <summary>
		/// -1=reverse  0=neutral  1..n=current gear by car index
		/// <summary>
		public System.Int32[] CarIdxGear;

        /// <summary>
        /// 0=no drs available, 1=drs detected, 2=drs available, 3=drs enabled
        /// <summary>
        public DrsStatus DRS_Status;

        /// <summary>
        /// Steering wheel angle
        /// <summary>
        public System.Single SteeringWheelAngle;
        /// <summary>
        /// 0=off throttle to 1=full throttle
        /// <summary>
        public System.Single Throttle;

		/// <summary>
		/// 0=brake released to 1=max pedal force
		/// <summary>
		public System.Single Brake;

		/// <summary>
		/// 0=disengaged to 1=fully engaged
		/// <summary>
		public System.Single Clutch;

		/// <summary>
		/// -1=reverse  0=neutral  1..n=current gear
		/// <summary>
		public System.Int32 Gear;

        /// <summary>
        /// Engine rpm
        /// <summary>
        public System.Single RPM;

        /// <summary>
        /// Laps started count
        /// <summary>
        public System.Int32 Lap;

        /// <summary>
        /// Players best lap time
        /// <summary>
        public System.Single LapBestLapTime;

		/// <summary>
		/// Players last lap time
		/// <summary>
		public System.Single LapLastLapTime;

        /// <summary>
        /// Players best lap number
        /// <summary>
        public System.Int32 LapBestLap;

        /// <summary>
        /// Estimate of players current lap time as shown in F3 box
        /// <summary>
        public System.Single LapCurrentLapTime;

		/// <summary>
		/// GPS vehicle speed
		/// <summary>
		public System.Single Speed;

		/// <summary>
		/// Yaw orientation
		/// <summary>
		public System.Single Yaw;

		/// <summary>
		/// Pitch orientation
		/// <summary>
		public System.Single Pitch;

		/// <summary>
		/// Roll orientation
		/// <summary>
		public System.Single Roll;

		/// <summary>
		/// Temperature of track measured by crew around track
		/// <summary>
		public System.Single TrackTempCrew;

		/// <summary>
		/// Temperature of air at start/finish line
		/// <summary>
		public System.Single AirTemp;

		/// <summary>
		/// Wind velocity at start/finish line
		/// <summary>
		public System.Single WindVel;

		/// <summary>
		/// Notify if car is to the left or right of driver
		/// <summary>
		public CrewChiefV4.iRacing.CarLeftRight CarLeftRight;

        /// <summary>
        /// True if pit stop is allowed for the current player
        /// <summary>
        public System.Boolean PitsOpen;

		/// <summary>
		/// 1=Car in garage physics running
		/// <summary>
		public System.Boolean IsInGarage;

		/// <summary>
		/// Bitfield for warning lights
		/// <summary>
		public CrewChiefV4.iRacing.EngineWarnings EngineWarnings;

		/// <summary>
		/// Liters of fuel remaining
		/// <summary>
		public System.Single FuelLevel;

		/// <summary>
		/// Engine coolant temp
		/// <summary>
		public System.Single WaterTemp;

		/// <summary>
		/// Engine coolant level
		/// <summary>
		public System.Single WaterLevel;

		/// <summary>
		/// Engine fuel pressure
		/// <summary>
		public System.Single FuelPress;

		/// <summary>
		/// Engine oil temperature
		/// <summary>
		public System.Single OilTemp;

		/// <summary>
		/// Engine voltage
		/// <summary>
		public System.Single Voltage;

		/// <summary>
		/// RR tire cold pressure  as set in the garage
		/// <summary>
		public System.Single RRcoldPressure;

		/// <summary>
		/// LR tire cold pressure  as set in the garage
		/// <summary>
		public System.Single LRcoldPressure;

		/// <summary>
		/// RF tire cold pressure  as set in the garage
		/// <summary>
		public System.Single RFcoldPressure;

		/// <summary>
		/// LF tire cold pressure  as set in the garage
		/// <summary>
		public System.Single LFcoldPressure;

        /// <summary>
        /// Vertical acceleration (including gravity)
        private System.Single[] _VertAccel;
        public System.Single VertAccel
        {
            get
            {
                return _VertAccel == null ? 0 : _VertAccel.Average();
            }                                
        }

        /// <summary>
        /// Lateral acceleration (including gravity)
        /// <summary>
        private System.Single[] _LatAccel;
        public System.Single LatAccel
        {
            get
            {
                return _LatAccel == null ? 0 : _LatAccel.Average();
            }
        }

        /// <summary>
        /// Longitudinal acceleration (including gravity)
        /// <summary>
        private System.Single[] _LongAccel;       
        public System.Single LongAccel
        {
            get
            {
                return _LongAccel == null ? 0 : _LongAccel.Average();
            }
        }

        /// <summary>
        /// Bitfield of pit service checkboxes
        /// <summary>
        public CrewChiefV4.iRacing.PitServiceFlags PitSvFlags;

        /// <summary>
        /// Pit service left front tire pressure
        /// <summary>
        public System.Single PitSvLFP;

        /// <summary>
        /// Pit service right front tire pressure
        /// <summary>
        public System.Single PitSvRFP;

        /// <summary>
        /// Pit service left rear tire pressure
        /// <summary>
        public System.Single PitSvLRP;

        /// <summary>
        /// Pit service right rear tire pressure
        /// <summary>
        public System.Single PitSvRRP;

        /// <summary>
        /// Pit service fuel add amount
        /// <summary>
        public System.Single PitSvFuel;
        
        /// <summary>
        /// Time left for mandatory pit repairs if repairs are active
        /// <summary>
        public System.Single PitRepairLeft;

        /// <summary>
        /// Time left for optional repairs if repairs are active
        /// <summary>
        public System.Single PitOptRepairLeft;

        /// <summary>
        /// Players car is being towed if time is greater than zero
        /// <summary>
        public System.Single PlayerCarTowTime;

        /// <summary>
        /// Players car is properly in there pitstall
        /// <summary>
        public System.Boolean PlayerCarInPitStall;

        /// <summary>
        /// Players car pit service status bits
        /// <summary>
        public CrewChiefV4.iRacing.PitSvStatus PlayerCarPitSvStatus;

        /// <summary>
        /// RR shock deflection
        /// <summary>
        private System.Single[] _RRshockDefl;
        public System.Single RRshockDefl
        {
            get
            {
                return _RRshockDefl == null ? 0 : _RRshockDefl.Average();
            }
        }
        /// <summary>
        /// RR shock velocity
        /// <summary>
        private System.Single[] _RRshockVel;
        public System.Single RRshockVel
        {
            get
            {
                return _RRshockVel == null ? 0 : _RRshockVel.Average();
            }
        }
        /// <summary>
        /// LR shock deflection
        /// <summary>
        private System.Single[] _LRshockDefl;
        public System.Single LRshockDefl
        {
            get
            {
                return _LRshockDefl == null ? 0 : _LRshockDefl.Average();
            }
        }
        /// <summary>
        /// LR shock velocity
        /// <summary>
        private System.Single[] _LRshockVel;
        public System.Single LRshockVel
        {
            get
            {
                return _LRshockVel == null ? 0 : _LRshockVel.Average();
            }
        }
        /// <summary>
        /// RF shock deflection
        /// <summary>
        private System.Single[] _RFshockDefl;
        public System.Single RFshockDefl
        {
            get
            {
                return _RFshockDefl == null ? 0 : _RFshockDefl.Average();
            }
        }
        /// <summary>
        /// RF shock velocity
        /// <summary>
        private System.Single[] _RFshockVel;
        public System.Single RFshockVel
        {
            get
            {
                return _RFshockVel == null ? 0 : _RFshockVel.Average();
            }
        }
        /// <summary>
        /// LF shock deflection
        /// <summary>
        private System.Single[] _LFshockDefl;
        public System.Single LFshockDefl
        {
            get
            {
                return _LFshockDefl == null ? 0 : _LFshockDefl.Average();
            }
        }
        /// <summary>
        /// LF shock velocity
        /// <summary>
        private System.Single[] _LFshockVel;
        public System.Single LFshockVel
        {
            get
            {
                return _LFshockVel == null ? 0 : _LFshockVel.Average();
            }
        }
    }
}
