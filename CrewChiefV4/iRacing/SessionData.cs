using System;
using System.Collections.Generic;
using System.Linq;
using iRSDKSharp;
namespace CrewChiefV4.iRacing
{
    public class SessionData
    {
        public SessionData()
        {
            this.SessionId = -1;
        }

        public Track Track { get; set; }
        public string EventType { get; set; }
        public string SessionType { get; set; }
        public int SessionId { get; set; }
        public int SubsessionId { get; set; }
        public string SessionTimeString { get; set; }
        public string RaceLaps { get; set; }
        public double RaceTime { get; set; }
        public bool IsHeatRacing { get; set; }
        public string IncidentLimitString { get; set; }
        public int IncidentLimit { get; set; }
        public bool IsTeamRacing { get; set; }
        public int NumCarClasses { get; set; }
        public bool StandingStart { get; set; }
        public bool StartingGridPoleOnLeft { get; set; }
        public string Restarts { get; set; }
        public string CourseCautions { get; set; }
        public string Category { get; set; }
        public bool hasFullCourseCautions { get; set; }
        public string diskTelemetryFile { get; set; }
        public const string sessionInfoYamlPath = "SessionInfo:Sessions:SessionNum:{{{0}}}{1}:";
        
        public void Update(string sessionString, int sessionNumber)
        {
            this.Track = Track.FromSessionInfo(sessionString);            
            this.SubsessionId = Parser.ParseInt(YamlParser.Parse(sessionString, "WeekendInfo:SubSessionID:"));
            this.SessionId = Parser.ParseInt(YamlParser.Parse(sessionString, "WeekendInfo:SessionID:"));            
            this.IsTeamRacing = Parser.ParseInt(YamlParser.Parse(sessionString, "WeekendInfo:TeamRacing:")) == 1;
            this.NumCarClasses = Parser.ParseInt(YamlParser.Parse(sessionString, "WeekendInfo:NumCarClasses:"));
            this.EventType = YamlParser.Parse(sessionString, "WeekendInfo:EventType:");
            this.Category = YamlParser.Parse(sessionString, "WeekendInfo:Category:");
            this.SessionType = YamlParser.Parse(sessionString, string.Format(sessionInfoYamlPath, sessionNumber, "SessionType"));
            this.RaceLaps = YamlParser.Parse(sessionString, string.Format(sessionInfoYamlPath, sessionNumber, "SessionLaps"));
            this.SessionTimeString = YamlParser.Parse(sessionString, string.Format(sessionInfoYamlPath, sessionNumber, "SessionTime"));
            this.IsHeatRacing = Parser.ParseInt(YamlParser.Parse(sessionString, "WeekendInfo:HeatRacing:")) == 1;
            this.RaceTime = Parser.ParseSec(SessionTimeString);            
            this.IncidentLimitString = YamlParser.Parse(sessionString, "WeekendInfo:WeekendOptions:IncidentLimit:");
            this.StandingStart = Parser.ParseInt(YamlParser.Parse(sessionString, "WeekendInfo:WeekendOptions:StandingStart:")) == 1;
            this.StartingGridPoleOnLeft = YamlParser.Parse(sessionString, "WeekendInfo:WeekendOptions:StartingGrid:").Contains("pole on left");
            this.Restarts = YamlParser.Parse(sessionString, "WeekendInfo:WeekendOptions:Restarts:");
            this.CourseCautions = YamlParser.Parse(sessionString, "WeekendInfo:WeekendOptions:CourseCautions:");
            this.hasFullCourseCautions = CourseCautions.Equals("full");
            this.diskTelemetryFile = YamlParser.Parse(sessionString, "WeekendInfo:TelemetryOptions:TelemetryDiskFile:");
            if (IsLimitedIncidents)
            {
                IncidentLimit = Parser.ParseInt(IncidentLimitString);
            }
            else
            {
                IncidentLimit = -1;
            }
        }
        public bool IsLimitedSessionLaps
        {
            get
            {
                if ((Category.ToLower() == "dirtroad" || Category.ToLower() == "dirtoval") && RaceLaps.ToLower() != "unlimited")
                    return true;

                return RaceLaps.ToLower() != "unlimited" && !IsHeatRacing;
            }
        }

        public bool IsLimitedTime
        {
            get
            {
                return SessionTimeString.ToLower() != "unlimited";
            }
        }
        public bool IsLimitedIncidents
        {
            get
            {
                return IncidentLimitString.ToLower() != "unlimited";
            }
        }
    }
}
