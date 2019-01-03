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
		public iRacingData( iRacingSDK sdk, bool hasNewSessionData, bool isNewSession)
		{
			if(hasNewSessionData)
			{
				SessionInfo = new SessionInfo(sdk.GetSessionInfoString()).Yaml;
			}
			else
			{
				SessionInfo = "";
			}
			SessionInfoUpdate = sdk.Header.SessionInfoUpdate;
			IsNewSession = isNewSession;
			SessionTime = (System.Double)sdk.GetData("SessionTime");
			SessionTick = (System.Int32)sdk.GetData("SessionTick");
			SessionNum = (System.Int32)sdk.GetData("SessionNum");
			SessionState = (CrewChiefV4.iRacing.SessionStates)sdk.GetData("SessionState");
			SessionUniqueID = (System.Int32)sdk.GetData("SessionUniqueID");
			SessionFlags = (CrewChiefV4.iRacing.SessionFlags)sdk.GetData("SessionFlags");
			SessionTimeRemain = (System.Double)sdk.GetData("SessionTimeRemain");
			SessionLapsRemain = (System.Int32)sdk.GetData("SessionLapsRemain");
			SessionLapsRemainEx = (System.Int32)sdk.GetData("SessionLapsRemainEx");
			RadioTransmitCarIdx = (System.Int32)sdk.GetData("RadioTransmitCarIdx");
			RadioTransmitRadioIdx = (System.Int32)sdk.GetData("RadioTransmitRadioIdx");
			RadioTransmitFrequencyIdx = (System.Int32)sdk.GetData("RadioTransmitFrequencyIdx");
			DisplayUnits = (CrewChiefV4.iRacing.DisplayUnits)sdk.GetData("DisplayUnits");
			DriverMarker = (System.Boolean)sdk.GetData("DriverMarker");
			PushToPass = (System.Boolean)sdk.GetData("PushToPass");
			ManualBoost = (System.Boolean)sdk.GetData("ManualBoost");
			ManualNoBoost = (System.Boolean)sdk.GetData("ManualNoBoost");
			IsOnTrack = (System.Boolean)sdk.GetData("IsOnTrack");
			IsReplayPlaying = (System.Boolean)sdk.GetData("IsReplayPlaying");
			ReplayFrameNum = (System.Int32)sdk.GetData("ReplayFrameNum");
			ReplayFrameNumEnd = (System.Int32)sdk.GetData("ReplayFrameNumEnd");
			IsDiskLoggingEnabled = (System.Boolean)sdk.GetData("IsDiskLoggingEnabled");
			IsDiskLoggingActive = (System.Boolean)sdk.GetData("IsDiskLoggingActive");
			FrameRate = (System.Single)sdk.GetData("FrameRate");
			CpuUsageBG = (System.Single)sdk.GetData("CpuUsageBG");
			PlayerCarPosition = (System.Int32)sdk.GetData("PlayerCarPosition");
			PlayerCarClassPosition = (System.Int32)sdk.GetData("PlayerCarClassPosition");
			PlayerTrackSurface = (CrewChiefV4.iRacing.TrackSurfaces)sdk.GetData("PlayerTrackSurface");
			PlayerTrackSurfaceMaterial = (CrewChiefV4.iRacing.TrackSurfaceMaterial)sdk.GetData("PlayerTrackSurfaceMaterial");
			PlayerCarIdx = (System.Int32)sdk.GetData("PlayerCarIdx");
			PlayerCarTeamIncidentCount = (System.Int32)sdk.GetData("PlayerCarTeamIncidentCount");
			PlayerCarMyIncidentCount = (System.Int32)sdk.GetData("PlayerCarMyIncidentCount");
			PlayerCarDriverIncidentCount = (System.Int32)sdk.GetData("PlayerCarDriverIncidentCount");
			PlayerCarWeightPenalty = (System.Single)sdk.GetData("PlayerCarWeightPenalty");
			PlayerCarTowTime = (System.Single)sdk.GetData("PlayerCarTowTime");
			CarIdxLap = (System.Int32[])sdk.GetData("CarIdxLap");
			CarIdxLapCompleted = (System.Int32[])sdk.GetData("CarIdxLapCompleted");
			CarIdxLapDistPct = (System.Single[])sdk.GetData("CarIdxLapDistPct");
			CarIdxTrackSurface = (CrewChiefV4.iRacing.TrackSurfaces[])sdk.GetData("CarIdxTrackSurface");
			CarIdxTrackSurfaceMaterial = (CrewChiefV4.iRacing.TrackSurfaceMaterial[])sdk.GetData("CarIdxTrackSurfaceMaterial");
			CarIdxOnPitRoad = (System.Boolean[])sdk.GetData("CarIdxOnPitRoad");
			CarIdxPosition = (System.Int32[])sdk.GetData("CarIdxPosition");
			CarIdxClassPosition = (System.Int32[])sdk.GetData("CarIdxClassPosition");
			CarIdxF2Time = (System.Single[])sdk.GetData("CarIdxF2Time");
			CarIdxEstTime = (System.Single[])sdk.GetData("CarIdxEstTime");
			OnPitRoad = (System.Boolean)sdk.GetData("OnPitRoad");
			CarIdxSteer = (System.Single[])sdk.GetData("CarIdxSteer");
			CarIdxRPM = (System.Single[])sdk.GetData("CarIdxRPM");
			CarIdxGear = (System.Int32[])sdk.GetData("CarIdxGear");
			SteeringWheelAngle = (System.Single)sdk.GetData("SteeringWheelAngle");
			Throttle = (System.Single)sdk.GetData("Throttle");
			Brake = (System.Single)sdk.GetData("Brake");
			Clutch = (System.Single)sdk.GetData("Clutch");
			Gear = (System.Int32)sdk.GetData("Gear");
			RPM = (System.Single)sdk.GetData("RPM");
			Lap = (System.Int32)sdk.GetData("Lap");
			LapCompleted = (System.Int32)sdk.GetData("LapCompleted");
			LapDist = (System.Single)sdk.GetData("LapDist");
			LapDistPct = (System.Single)sdk.GetData("LapDistPct");
			RaceLaps = (System.Int32)sdk.GetData("RaceLaps");
			LapBestLap = (System.Int32)sdk.GetData("LapBestLap");
			LapBestLapTime = (System.Single)sdk.GetData("LapBestLapTime");
			LapLastLapTime = (System.Single)sdk.GetData("LapLastLapTime");
			LapCurrentLapTime = (System.Single)sdk.GetData("LapCurrentLapTime");
			LapLasNLapSeq = (System.Int32)sdk.GetData("LapLasNLapSeq");
			LapLastNLapTime = (System.Single)sdk.GetData("LapLastNLapTime");
			LapBestNLapLap = (System.Int32)sdk.GetData("LapBestNLapLap");
			LapBestNLapTime = (System.Single)sdk.GetData("LapBestNLapTime");
			LapDeltaToBestLap = (System.Single)sdk.GetData("LapDeltaToBestLap");
			LapDeltaToBestLap_DD = (System.Single)sdk.GetData("LapDeltaToBestLap_DD");
			LapDeltaToBestLap_OK = (System.Boolean)sdk.GetData("LapDeltaToBestLap_OK");
			LapDeltaToOptimalLap = (System.Single)sdk.GetData("LapDeltaToOptimalLap");
			LapDeltaToOptimalLap_DD = (System.Single)sdk.GetData("LapDeltaToOptimalLap_DD");
			LapDeltaToOptimalLap_OK = (System.Boolean)sdk.GetData("LapDeltaToOptimalLap_OK");
			LapDeltaToSessionBestLap = (System.Single)sdk.GetData("LapDeltaToSessionBestLap");
			LapDeltaToSessionBestLap_DD = (System.Single)sdk.GetData("LapDeltaToSessionBestLap_DD");
			LapDeltaToSessionBestLap_OK = (System.Boolean)sdk.GetData("LapDeltaToSessionBestLap_OK");
			LapDeltaToSessionOptimalLap = (System.Single)sdk.GetData("LapDeltaToSessionOptimalLap");
			LapDeltaToSessionOptimalLap_DD = (System.Single)sdk.GetData("LapDeltaToSessionOptimalLap_DD");
			LapDeltaToSessionOptimalLap_OK = (System.Boolean)sdk.GetData("LapDeltaToSessionOptimalLap_OK");
			LapDeltaToSessionLastlLap = (System.Single)sdk.GetData("LapDeltaToSessionLastlLap");
			LapDeltaToSessionLastlLap_DD = (System.Single)sdk.GetData("LapDeltaToSessionLastlLap_DD");
			LapDeltaToSessionLastlLap_OK = (System.Boolean)sdk.GetData("LapDeltaToSessionLastlLap_OK");
			Speed = (System.Single)sdk.GetData("Speed");
			Yaw = (System.Single)sdk.GetData("Yaw");
			YawNorth = (System.Single)sdk.GetData("YawNorth");
			Pitch = (System.Single)sdk.GetData("Pitch");
			Roll = (System.Single)sdk.GetData("Roll");
			EnterExitReset = (System.Int32)sdk.GetData("EnterExitReset");
			TrackTemp = (System.Single)sdk.GetData("TrackTemp");
			TrackTempCrew = (System.Single)sdk.GetData("TrackTempCrew");
			AirTemp = (System.Single)sdk.GetData("AirTemp");
			WeatherType = (CrewChiefV4.iRacing.WeatherType)sdk.GetData("WeatherType");
			Skies = (CrewChiefV4.iRacing.Skies)sdk.GetData("Skies");
			AirDensity = (System.Single)sdk.GetData("AirDensity");
			AirPressure = (System.Single)sdk.GetData("AirPressure");
			WindVel = (System.Single)sdk.GetData("WindVel");
			WindDir = (System.Single)sdk.GetData("WindDir");
			RelativeHumidity = (System.Single)sdk.GetData("RelativeHumidity");
			FogLevel = (System.Single)sdk.GetData("FogLevel");
			DCLapStatus = (System.Int32)sdk.GetData("DCLapStatus");
			DCDriversSoFar = (System.Int32)sdk.GetData("DCDriversSoFar");
			OkToReloadTextures = (System.Boolean)sdk.GetData("OkToReloadTextures");
			CarLeftRight = (CrewChiefV4.iRacing.CarLeftRight)sdk.GetData("CarLeftRight");
			PitRepairLeft = (System.Single)sdk.GetData("PitRepairLeft");
			PitOptRepairLeft = (System.Single)sdk.GetData("PitOptRepairLeft");
			CamCarIdx = (System.Int32)sdk.GetData("CamCarIdx");
			CamCameraNumber = (System.Int32)sdk.GetData("CamCameraNumber");
			CamGroupNumber = (System.Int32)sdk.GetData("CamGroupNumber");
			CamCameraState = (CrewChiefV4.iRacing.CameraStates)sdk.GetData("CamCameraState");
			IsOnTrackCar = (System.Boolean)sdk.GetData("IsOnTrackCar");
			IsInGarage = (System.Boolean)sdk.GetData("IsInGarage");
			SteeringWheelPctTorque = (System.Single)sdk.GetData("SteeringWheelPctTorque");
			SteeringWheelPctTorqueSign = (System.Single)sdk.GetData("SteeringWheelPctTorqueSign");
			SteeringWheelPctTorqueSignStops = (System.Single)sdk.GetData("SteeringWheelPctTorqueSignStops");
			SteeringWheelPctDamper = (System.Single)sdk.GetData("SteeringWheelPctDamper");
			SteeringWheelAngleMax = (System.Single)sdk.GetData("SteeringWheelAngleMax");
			ShiftIndicatorPct = (System.Single)sdk.GetData("ShiftIndicatorPct");
			ShiftPowerPct = (System.Single)sdk.GetData("ShiftPowerPct");
			ShiftGrindRPM = (System.Single)sdk.GetData("ShiftGrindRPM");
			ThrottleRaw = (System.Single)sdk.GetData("ThrottleRaw");
			BrakeRaw = (System.Single)sdk.GetData("BrakeRaw");
			HandbrakeRaw = (System.Single)sdk.GetData("HandbrakeRaw");
			SteeringWheelPeakForceNm = (System.Single)sdk.GetData("SteeringWheelPeakForceNm");
			EngineWarnings = (CrewChiefV4.iRacing.EngineWarnings)sdk.GetData("EngineWarnings");
			FuelLevel = (System.Single)sdk.GetData("FuelLevel");
			FuelLevelPct = (System.Single)sdk.GetData("FuelLevelPct");
			PitSvFlags = (CrewChiefV4.iRacing.PitServiceFlags)sdk.GetData("PitSvFlags");
			PitSvLFP = (System.Single)sdk.GetData("PitSvLFP");
			PitSvRFP = (System.Single)sdk.GetData("PitSvRFP");
			PitSvLRP = (System.Single)sdk.GetData("PitSvLRP");
			PitSvRRP = (System.Single)sdk.GetData("PitSvRRP");
			PitSvFuel = (System.Single)sdk.GetData("PitSvFuel");
			ReplayPlaySpeed = (System.Int32)sdk.GetData("ReplayPlaySpeed");
			ReplayPlaySlowMotion = (System.Boolean)sdk.GetData("ReplayPlaySlowMotion");
			ReplaySessionTime = (System.Double)sdk.GetData("ReplaySessionTime");
			ReplaySessionNum = (System.Int32)sdk.GetData("ReplaySessionNum");
			TireLF_RumblePitch = (System.Single)sdk.GetData("TireLF_RumblePitch");
			TireRF_RumblePitch = (System.Single)sdk.GetData("TireRF_RumblePitch");
			TireLR_RumblePitch = (System.Single)sdk.GetData("TireLR_RumblePitch");
			TireRR_RumblePitch = (System.Single)sdk.GetData("TireRR_RumblePitch");
			SteeringWheelTorque_ST = (System.Single[])sdk.GetData("SteeringWheelTorque_ST");
			SteeringWheelTorque = (System.Single)sdk.GetData("SteeringWheelTorque");
			VelocityZ_ST = (System.Single[])sdk.GetData("VelocityZ_ST");
			VelocityY_ST = (System.Single[])sdk.GetData("VelocityY_ST");
			VelocityX_ST = (System.Single[])sdk.GetData("VelocityX_ST");
			VelocityZ = (System.Single)sdk.GetData("VelocityZ");
			VelocityY = (System.Single)sdk.GetData("VelocityY");
			VelocityX = (System.Single)sdk.GetData("VelocityX");
			YawRate_ST = (System.Single[])sdk.GetData("YawRate_ST");
			PitchRate_ST = (System.Single[])sdk.GetData("PitchRate_ST");
			RollRate_ST = (System.Single[])sdk.GetData("RollRate_ST");
			YawRate = (System.Single)sdk.GetData("YawRate");
			PitchRate = (System.Single)sdk.GetData("PitchRate");
			RollRate = (System.Single)sdk.GetData("RollRate");
			VertAccel_ST = (System.Single[])sdk.GetData("VertAccel_ST");
			LatAccel_ST = (System.Single[])sdk.GetData("LatAccel_ST");
			LongAccel_ST = (System.Single[])sdk.GetData("LongAccel_ST");
			VertAccel = (System.Single)sdk.GetData("VertAccel");
			LatAccel = (System.Single)sdk.GetData("LatAccel");
			LongAccel = (System.Single)sdk.GetData("LongAccel");
			WaterTemp = (System.Single)sdk.GetData("WaterTemp");
			WaterLevel = (System.Single)sdk.GetData("WaterLevel");
			FuelPress = (System.Single)sdk.GetData("FuelPress");
			FuelUsePerHour = (System.Single)sdk.GetData("FuelUsePerHour");
			OilTemp = (System.Single)sdk.GetData("OilTemp");
			OilPress = (System.Single)sdk.GetData("OilPress");
			OilLevel = (System.Single)sdk.GetData("OilLevel");
			Voltage = (System.Single)sdk.GetData("Voltage");
			ManifoldPress = (System.Single)sdk.GetData("ManifoldPress");
			RRcoldPressure = (System.Single)sdk.GetData("RRcoldPressure");
			RRtempCL = (System.Single)sdk.GetData("RRtempCL");
			RRtempCM = (System.Single)sdk.GetData("RRtempCM");
			RRtempCR = (System.Single)sdk.GetData("RRtempCR");
			RRwearL = (System.Single)sdk.GetData("RRwearL");
			RRwearM = (System.Single)sdk.GetData("RRwearM");
			RRwearR = (System.Single)sdk.GetData("RRwearR");
			LRcoldPressure = (System.Single)sdk.GetData("LRcoldPressure");
			LRtempCL = (System.Single)sdk.GetData("LRtempCL");
			LRtempCM = (System.Single)sdk.GetData("LRtempCM");
			LRtempCR = (System.Single)sdk.GetData("LRtempCR");
			LRwearL = (System.Single)sdk.GetData("LRwearL");
			LRwearM = (System.Single)sdk.GetData("LRwearM");
			LRwearR = (System.Single)sdk.GetData("LRwearR");
			RFcoldPressure = (System.Single)sdk.GetData("RFcoldPressure");
			RFtempCL = (System.Single)sdk.GetData("RFtempCL");
			RFtempCM = (System.Single)sdk.GetData("RFtempCM");
			RFtempCR = (System.Single)sdk.GetData("RFtempCR");
			RFwearL = (System.Single)sdk.GetData("RFwearL");
			RFwearM = (System.Single)sdk.GetData("RFwearM");
			RFwearR = (System.Single)sdk.GetData("RFwearR");
			LFcoldPressure = (System.Single)sdk.GetData("LFcoldPressure");
			LFtempCL = (System.Single)sdk.GetData("LFtempCL");
			LFtempCM = (System.Single)sdk.GetData("LFtempCM");
			LFtempCR = (System.Single)sdk.GetData("LFtempCR");
			LFwearL = (System.Single)sdk.GetData("LFwearL");
			LFwearM = (System.Single)sdk.GetData("LFwearM");
			LFwearR = (System.Single)sdk.GetData("LFwearR");
		}
		public iRacingData() {}
		public System.Boolean IsNewSession;
		public System.Int32 SessionInfoUpdate;
		public System.String SessionInfo;

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
		/// Session ID
		/// <summary>
		public System.Int32 SessionUniqueID;

		/// <summary>
		/// Session flags
		/// <summary>
		public CrewChiefV4.iRacing.SessionFlags SessionFlags;

		/// <summary>
		/// Seconds left till session ends
		/// <summary>
		public System.Double SessionTimeRemain;

		/// <summary>
		/// Old laps left till session ends use SessionLapsRemainEx
		/// <summary>
		public System.Int32 SessionLapsRemain;

		/// <summary>
		/// New improved laps left till session ends
		/// <summary>
		public System.Int32 SessionLapsRemainEx;

		/// <summary>
		/// The car index of the current person speaking on the radio
		/// <summary>
		public System.Int32 RadioTransmitCarIdx;

		/// <summary>
		/// The radio index of the current person speaking on the radio
		/// <summary>
		public System.Int32 RadioTransmitRadioIdx;

		/// <summary>
		/// The frequency index of the current person speaking on the radio
		/// <summary>
		public System.Int32 RadioTransmitFrequencyIdx;

		/// <summary>
		/// Default units for the user interface 0 = english 1 = metric
		/// <summary>
		public CrewChiefV4.iRacing.DisplayUnits DisplayUnits;

		/// <summary>
		/// Driver activated flag
		/// <summary>
		public System.Boolean DriverMarker;

		/// <summary>
		/// Push to pass button state
		/// <summary>
		public System.Boolean PushToPass;

		/// <summary>
		/// Hybrid manual boost state
		/// <summary>
		public System.Boolean ManualBoost;

		/// <summary>
		/// Hybrid manual no boost state
		/// <summary>
		public System.Boolean ManualNoBoost;

		/// <summary>
		/// 1=Car on track physics running with player in car
		/// <summary>
		public System.Boolean IsOnTrack;

		/// <summary>
		/// 0=replay not playing  1=replay playing
		/// <summary>
		public System.Boolean IsReplayPlaying;

		/// <summary>
		/// Integer replay frame number (60 per second)
		/// <summary>
		public System.Int32 ReplayFrameNum;

		/// <summary>
		/// Integer replay frame number from end of tape
		/// <summary>
		public System.Int32 ReplayFrameNumEnd;

		/// <summary>
		/// 0=disk based telemetry turned off  1=turned on
		/// <summary>
		public System.Boolean IsDiskLoggingEnabled;

		/// <summary>
		/// 0=disk based telemetry file not being written  1=being written
		/// <summary>
		public System.Boolean IsDiskLoggingActive;

		/// <summary>
		/// Average frames per second
		/// <summary>
		public System.Single FrameRate;

		/// <summary>
		/// Percent of available tim bg thread took with a 1 sec avg
		/// <summary>
		public System.Single CpuUsageBG;

		/// <summary>
		/// Players position in race
		/// <summary>
		public System.Int32 PlayerCarPosition;

		/// <summary>
		/// Players class position in race
		/// <summary>
		public System.Int32 PlayerCarClassPosition;

		/// <summary>
		/// Players car track surface type
		/// <summary>
		public CrewChiefV4.iRacing.TrackSurfaces PlayerTrackSurface;

		/// <summary>
		/// Players car track surface material type
		/// <summary>
		public CrewChiefV4.iRacing.TrackSurfaceMaterial PlayerTrackSurfaceMaterial;

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
		/// Players weight penalty
		/// <summary>
		public System.Single PlayerCarWeightPenalty;

		/// <summary>
		/// Players car is being towed if time is greater than zero
		/// <summary>
		public System.Single PlayerCarTowTime;

		/// <summary>
		/// Laps started by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Int32[] CarIdxLap;

		/// <summary>
		/// Laps completed by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Int32[] CarIdxLapCompleted;

		/// <summary>
		/// Percentage distance around lap by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Single[] CarIdxLapDistPct;

		/// <summary>
		/// Track surface type by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public CrewChiefV4.iRacing.TrackSurfaces[] CarIdxTrackSurface;

		/// <summary>
		/// Track surface material type by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public CrewChiefV4.iRacing.TrackSurfaceMaterial[] CarIdxTrackSurfaceMaterial;

		/// <summary>
		/// On pit road between the cones by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Boolean[] CarIdxOnPitRoad;

		/// <summary>
		/// Cars position in race by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Int32[] CarIdxPosition;

		/// <summary>
		/// Cars class position in race by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Int32[] CarIdxClassPosition;

		/// <summary>
		/// Race time behind leader or fastest lap time otherwise
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Single[] CarIdxF2Time;

		/// <summary>
		/// Estimated time to reach current location on track
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Single[] CarIdxEstTime;

		/// <summary>
		/// Is the player car on pit road between the cones
		/// <summary>
		public System.Boolean OnPitRoad;

		/// <summary>
		/// Steering wheel angle by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Single[] CarIdxSteer;

		/// <summary>
		/// Engine rpm by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Single[] CarIdxRPM;

		/// <summary>
		/// -1=reverse  0=neutral  1..n=current gear by car index
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public System.Int32[] CarIdxGear;

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
		/// Laps completed count
		/// <summary>
		public System.Int32 LapCompleted;

		/// <summary>
		/// Meters traveled from S/F this lap
		/// <summary>
		public System.Single LapDist;

		/// <summary>
		/// Percentage distance around lap
		/// <summary>
		public System.Single LapDistPct;

		/// <summary>
		/// Laps completed in race
		/// <summary>
		public System.Int32 RaceLaps;

		/// <summary>
		/// Players best lap number
		/// <summary>
		public System.Int32 LapBestLap;

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
		/// Player num consecutive clean laps completed for N average
		/// <summary>
		public System.Int32 LapLasNLapSeq;

		/// <summary>
		/// Player last N average lap time
		/// <summary>
		public System.Single LapLastNLapTime;

		/// <summary>
		/// Player last lap in best N average lap time
		/// <summary>
		public System.Int32 LapBestNLapLap;

		/// <summary>
		/// Player best N average lap time
		/// <summary>
		public System.Single LapBestNLapTime;

		/// <summary>
		/// Delta time for best lap
		/// <summary>
		public System.Single LapDeltaToBestLap;

		/// <summary>
		/// Rate of change of delta time for best lap
		/// <summary>
		public System.Single LapDeltaToBestLap_DD;

		/// <summary>
		/// Delta time for best lap is valid
		/// <summary>
		public System.Boolean LapDeltaToBestLap_OK;

		/// <summary>
		/// Delta time for optimal lap
		/// <summary>
		public System.Single LapDeltaToOptimalLap;

		/// <summary>
		/// Rate of change of delta time for optimal lap
		/// <summary>
		public System.Single LapDeltaToOptimalLap_DD;

		/// <summary>
		/// Delta time for optimal lap is valid
		/// <summary>
		public System.Boolean LapDeltaToOptimalLap_OK;

		/// <summary>
		/// Delta time for session best lap
		/// <summary>
		public System.Single LapDeltaToSessionBestLap;

		/// <summary>
		/// Rate of change of delta time for session best lap
		/// <summary>
		public System.Single LapDeltaToSessionBestLap_DD;

		/// <summary>
		/// Delta time for session best lap is valid
		/// <summary>
		public System.Boolean LapDeltaToSessionBestLap_OK;

		/// <summary>
		/// Delta time for session optimal lap
		/// <summary>
		public System.Single LapDeltaToSessionOptimalLap;

		/// <summary>
		/// Rate of change of delta time for session optimal lap
		/// <summary>
		public System.Single LapDeltaToSessionOptimalLap_DD;

		/// <summary>
		/// Delta time for session optimal lap is valid
		/// <summary>
		public System.Boolean LapDeltaToSessionOptimalLap_OK;

		/// <summary>
		/// Delta time for session last lap
		/// <summary>
		public System.Single LapDeltaToSessionLastlLap;

		/// <summary>
		/// Rate of change of delta time for session last lap
		/// <summary>
		public System.Single LapDeltaToSessionLastlLap_DD;

		/// <summary>
		/// Delta time for session last lap is valid
		/// <summary>
		public System.Boolean LapDeltaToSessionLastlLap_OK;

		/// <summary>
		/// GPS vehicle speed
		/// <summary>
		public System.Single Speed;

		/// <summary>
		/// Yaw orientation
		/// <summary>
		public System.Single Yaw;

		/// <summary>
		/// Yaw orientation relative to north
		/// <summary>
		public System.Single YawNorth;

		/// <summary>
		/// Pitch orientation
		/// <summary>
		public System.Single Pitch;

		/// <summary>
		/// Roll orientation
		/// <summary>
		public System.Single Roll;

		/// <summary>
		/// Indicate action the reset key will take 0 enter 1 exit 2 reset
		/// <summary>
		public System.Int32 EnterExitReset;

		/// <summary>
		/// Deprecated  set to TrackTempCrew
		/// <summary>
		public System.Single TrackTemp;

		/// <summary>
		/// Temperature of track measured by crew around track
		/// <summary>
		public System.Single TrackTempCrew;

		/// <summary>
		/// Temperature of air at start/finish line
		/// <summary>
		public System.Single AirTemp;

		/// <summary>
		/// Weather type (0=constant  1=dynamic)
		/// <summary>
		public CrewChiefV4.iRacing.WeatherType WeatherType;

		/// <summary>
		/// Skies (0=clear/1=p cloudy/2=m cloudy/3=overcast)
		/// <summary>
		public CrewChiefV4.iRacing.Skies Skies;

		/// <summary>
		/// Density of air at start/finish line
		/// <summary>
		public System.Single AirDensity;

		/// <summary>
		/// Pressure of air at start/finish line
		/// <summary>
		public System.Single AirPressure;

		/// <summary>
		/// Wind velocity at start/finish line
		/// <summary>
		public System.Single WindVel;

		/// <summary>
		/// Wind direction at start/finish line
		/// <summary>
		public System.Single WindDir;

		/// <summary>
		/// Relative Humidity
		/// <summary>
		public System.Single RelativeHumidity;

		/// <summary>
		/// Fog level
		/// <summary>
		public System.Single FogLevel;

		/// <summary>
		/// Status of driver change lap requirements
		/// <summary>
		public System.Int32 DCLapStatus;

		/// <summary>
		/// Number of team drivers who have run a stint
		/// <summary>
		public System.Int32 DCDriversSoFar;

		/// <summary>
		/// True if it is ok to reload car textures at this time
		/// <summary>
		public System.Boolean OkToReloadTextures;

		/// <summary>
		/// Notify if car is to the left or right of driver
		/// <summary>
		public CrewChiefV4.iRacing.CarLeftRight CarLeftRight;

		/// <summary>
		/// Time left for mandatory pit repairs if repairs are active
		/// <summary>
		public System.Single PitRepairLeft;

		/// <summary>
		/// Time left for optional repairs if repairs are active
		/// <summary>
		public System.Single PitOptRepairLeft;

		/// <summary>
		/// Active camera's focus car index
		/// <summary>
		public System.Int32 CamCarIdx;

		/// <summary>
		/// Active camera number
		/// <summary>
		public System.Int32 CamCameraNumber;

		/// <summary>
		/// Active camera group number
		/// <summary>
		public System.Int32 CamGroupNumber;

		/// <summary>
		/// State of camera system
		/// <summary>
		public CrewChiefV4.iRacing.CameraStates CamCameraState;

		/// <summary>
		/// 1=Car on track physics running
		/// <summary>
		public System.Boolean IsOnTrackCar;

		/// <summary>
		/// 1=Car in garage physics running
		/// <summary>
		public System.Boolean IsInGarage;

		/// <summary>
		/// Force feedback % max torque on steering shaft unsigned
		/// <summary>
		public System.Single SteeringWheelPctTorque;

		/// <summary>
		/// Force feedback % max torque on steering shaft signed
		/// <summary>
		public System.Single SteeringWheelPctTorqueSign;

		/// <summary>
		/// Force feedback % max torque on steering shaft signed stops
		/// <summary>
		public System.Single SteeringWheelPctTorqueSignStops;

		/// <summary>
		/// Force feedback % max damping
		/// <summary>
		public System.Single SteeringWheelPctDamper;

		/// <summary>
		/// Steering wheel max angle
		/// <summary>
		public System.Single SteeringWheelAngleMax;

		/// <summary>
		/// DEPRECATED use DriverCarSLBlinkRPM instead
		/// <summary>
		public System.Single ShiftIndicatorPct;

		/// <summary>
		/// Friction torque applied to gears when shifting or grinding
		/// <summary>
		public System.Single ShiftPowerPct;

		/// <summary>
		/// RPM of shifter grinding noise
		/// <summary>
		public System.Single ShiftGrindRPM;

		/// <summary>
		/// Raw throttle input 0=off throttle to 1=full throttle
		/// <summary>
		public System.Single ThrottleRaw;

		/// <summary>
		/// Raw brake input 0=brake released to 1=max pedal force
		/// <summary>
		public System.Single BrakeRaw;

		/// <summary>
		/// Raw handbrake input 0=handbrake released to 1=max force
		/// <summary>
		public System.Single HandbrakeRaw;

		/// <summary>
		/// Peak torque mapping to direct input units for FFB
		/// <summary>
		public System.Single SteeringWheelPeakForceNm;

		/// <summary>
		/// Bitfield for warning lights
		/// <summary>
		public CrewChiefV4.iRacing.EngineWarnings EngineWarnings;

		/// <summary>
		/// Liters of fuel remaining
		/// <summary>
		public System.Single FuelLevel;

		/// <summary>
		/// Percent fuel remaining
		/// <summary>
		public System.Single FuelLevelPct;

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
		/// Replay playback speed
		/// <summary>
		public System.Int32 ReplayPlaySpeed;

		/// <summary>
		/// 0=not slow motion  1=replay is in slow motion
		/// <summary>
		public System.Boolean ReplayPlaySlowMotion;

		/// <summary>
		/// Seconds since replay session start
		/// <summary>
		public System.Double ReplaySessionTime;

		/// <summary>
		/// Replay session number
		/// <summary>
		public System.Int32 ReplaySessionNum;

		/// <summary>
		/// Players LF Tire Sound rumblestrip pitch
		/// <summary>
		public System.Single TireLF_RumblePitch;

		/// <summary>
		/// Players RF Tire Sound rumblestrip pitch
		/// <summary>
		public System.Single TireRF_RumblePitch;

		/// <summary>
		/// Players LR Tire Sound rumblestrip pitch
		/// <summary>
		public System.Single TireLR_RumblePitch;

		/// <summary>
		/// Players RR Tire Sound rumblestrip pitch
		/// <summary>
		public System.Single TireRR_RumblePitch;

		/// <summary>
		/// Output torque on steering shaft at 360 Hz
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] SteeringWheelTorque_ST;

		/// <summary>
		/// Output torque on steering shaft
		/// <summary>
		public System.Single SteeringWheelTorque;

		/// <summary>
		/// Z velocity
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] VelocityZ_ST;

		/// <summary>
		/// Y velocity
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] VelocityY_ST;

		/// <summary>
		/// X velocity
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] VelocityX_ST;

		/// <summary>
		/// Z velocity
		/// <summary>
		public System.Single VelocityZ;

		/// <summary>
		/// Y velocity
		/// <summary>
		public System.Single VelocityY;

		/// <summary>
		/// X velocity
		/// <summary>
		public System.Single VelocityX;

		/// <summary>
		/// Yaw rate at 360 Hz
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] YawRate_ST;

		/// <summary>
		/// Pitch rate at 360 Hz
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] PitchRate_ST;

		/// <summary>
		/// Roll rate at 360 Hz
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] RollRate_ST;

		/// <summary>
		/// Yaw rate
		/// <summary>
		public System.Single YawRate;

		/// <summary>
		/// Pitch rate
		/// <summary>
		public System.Single PitchRate;

		/// <summary>
		/// Roll rate
		/// <summary>
		public System.Single RollRate;

		/// <summary>
		/// Vertical acceleration (including gravity) at 360 Hz
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] VertAccel_ST;

		/// <summary>
		/// Lateral acceleration (including gravity) at 360 Hz
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] LatAccel_ST;

		/// <summary>
		/// Longitudinal acceleration (including gravity) at 360 Hz
		/// <summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Single[] LongAccel_ST;

		/// <summary>
		/// Vertical acceleration (including gravity)
		/// <summary>
		public System.Single VertAccel;

		/// <summary>
		/// Lateral acceleration (including gravity)
		/// <summary>
		public System.Single LatAccel;

		/// <summary>
		/// Longitudinal acceleration (including gravity)
		/// <summary>
		public System.Single LongAccel;

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
		/// Engine fuel used instantaneous
		/// <summary>
		public System.Single FuelUsePerHour;

		/// <summary>
		/// Engine oil temperature
		/// <summary>
		public System.Single OilTemp;

		/// <summary>
		/// Engine oil pressure
		/// <summary>
		public System.Single OilPress;

		/// <summary>
		/// Engine oil level
		/// <summary>
		public System.Single OilLevel;

		/// <summary>
		/// Engine voltage
		/// <summary>
		public System.Single Voltage;

		/// <summary>
		/// Engine manifold pressure
		/// <summary>
		public System.Single ManifoldPress;

		/// <summary>
		/// RR tire cold pressure  as set in the garage
		/// <summary>
		public System.Single RRcoldPressure;

		/// <summary>
		/// RR tire left carcass temperature
		/// <summary>
		public System.Single RRtempCL;

		/// <summary>
		/// RR tire middle carcass temperature
		/// <summary>
		public System.Single RRtempCM;

		/// <summary>
		/// RR tire right carcass temperature
		/// <summary>
		public System.Single RRtempCR;

		/// <summary>
		/// RR tire left percent tread remaining
		/// <summary>
		public System.Single RRwearL;

		/// <summary>
		/// RR tire middle percent tread remaining
		/// <summary>
		public System.Single RRwearM;

		/// <summary>
		/// RR tire right percent tread remaining
		/// <summary>
		public System.Single RRwearR;

		/// <summary>
		/// LR tire cold pressure  as set in the garage
		/// <summary>
		public System.Single LRcoldPressure;

		/// <summary>
		/// LR tire left carcass temperature
		/// <summary>
		public System.Single LRtempCL;

		/// <summary>
		/// LR tire middle carcass temperature
		/// <summary>
		public System.Single LRtempCM;

		/// <summary>
		/// LR tire right carcass temperature
		/// <summary>
		public System.Single LRtempCR;

		/// <summary>
		/// LR tire left percent tread remaining
		/// <summary>
		public System.Single LRwearL;

		/// <summary>
		/// LR tire middle percent tread remaining
		/// <summary>
		public System.Single LRwearM;

		/// <summary>
		/// LR tire right percent tread remaining
		/// <summary>
		public System.Single LRwearR;

		/// <summary>
		/// RF tire cold pressure  as set in the garage
		/// <summary>
		public System.Single RFcoldPressure;

		/// <summary>
		/// RF tire left carcass temperature
		/// <summary>
		public System.Single RFtempCL;

		/// <summary>
		/// RF tire middle carcass temperature
		/// <summary>
		public System.Single RFtempCM;

		/// <summary>
		/// RF tire right carcass temperature
		/// <summary>
		public System.Single RFtempCR;

		/// <summary>
		/// RF tire left percent tread remaining
		/// <summary>
		public System.Single RFwearL;

		/// <summary>
		/// RF tire middle percent tread remaining
		/// <summary>
		public System.Single RFwearM;

		/// <summary>
		/// RF tire right percent tread remaining
		/// <summary>
		public System.Single RFwearR;

		/// <summary>
		/// LF tire cold pressure  as set in the garage
		/// <summary>
		public System.Single LFcoldPressure;

		/// <summary>
		/// LF tire left carcass temperature
		/// <summary>
		public System.Single LFtempCL;

		/// <summary>
		/// LF tire middle carcass temperature
		/// <summary>
		public System.Single LFtempCM;

		/// <summary>
		/// LF tire right carcass temperature
		/// <summary>
		public System.Single LFtempCR;

		/// <summary>
		/// LF tire left percent tread remaining
		/// <summary>
		public System.Single LFwearL;

		/// <summary>
		/// LF tire middle percent tread remaining
		/// <summary>
		public System.Single LFwearM;

		/// <summary>
		/// LF tire right percent tread remaining
		/// <summary>
		public System.Single LFwearR;
	}
}
