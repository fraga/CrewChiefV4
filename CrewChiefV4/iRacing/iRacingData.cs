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
			SessionNum = (System.Int32)sdk.GetData("SessionNum");
			SessionState = (CrewChiefV4.iRacing.SessionStates)sdk.GetData("SessionState");
            SessionFlags = (System.UInt32)sdk.GetData("SessionFlags");
			SessionTimeRemain = (System.Double)sdk.GetData("SessionTimeRemain");
			IsOnTrack = (System.Boolean)sdk.GetData("IsOnTrack");
			IsReplayPlaying = (System.Boolean)sdk.GetData("IsReplayPlaying");
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
			CarIdxRPM = (System.Single[])sdk.GetData("CarIdxRPM");
			CarIdxGear = (System.Int32[])sdk.GetData("CarIdxGear");
			Throttle = (System.Single)sdk.GetData("Throttle");
			Brake = (System.Single)sdk.GetData("Brake");
			Clutch = (System.Single)sdk.GetData("Clutch");
			Gear = (System.Int32)sdk.GetData("Gear");
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
            }
            else
            {
                _VertAccel = new System.Single[1];
                _VertAccel[0] =  (System.Single)sdk.GetData("VertAccel");
                _LatAccel = new System.Single[1];
                _LatAccel[0] = (System.Single)sdk.GetData("LatAccel");
                _LongAccel = new System.Single[1];
                _LongAccel[0] = (System.Single)sdk.GetData("LongAccel");
            }

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
		/// Engine rpm by car index
		/// <summary>
		public System.Single[] CarIdxRPM;

		/// <summary>
		/// -1=reverse  0=neutral  1..n=current gear by car index
		/// <summary>
		public System.Int32[] CarIdxGear;

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
		/// Players best lap time
		/// <summary>
		public System.Single LapBestLapTime;

		/// <summary>
		/// Players last lap time
		/// <summary>
		public System.Single LapLastLapTime;

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


	}
}
