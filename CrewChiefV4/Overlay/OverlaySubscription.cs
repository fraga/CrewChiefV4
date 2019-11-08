using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.Overlay
{
    public class OverlaySubscription
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string id;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string rawDataFieldName;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string mappedDataFieldName;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string opponentDataFieldname;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string diskDataFieldname;
        [JsonIgnore]
        public string fieldName;
        [JsonIgnore]
        public bool isRawField = true;
        [JsonIgnore]
        public bool isGroup = false;
        [JsonIgnore]
        public bool isDiskData = false;
        [JsonConverter(typeof(StringEnumConverter))]
        public OverlayDataType overlayDataType;
        public string[] labels;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float yMin = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float yMax = 0;
        [JsonConverter(typeof(StringEnumConverter)), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public YAxisScaling yAxisMinScaling = YAxisScaling.AUTO;
        [JsonConverter(typeof(StringEnumConverter)), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public YAxisScaling yAxisMaxScaling = YAxisScaling.AUTO;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] coloursLastLap;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] coloursBestLap;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] coloursOpponentBestLap;
        public string yAxisFormat;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] groupMemberIds;
        public string voiceCommandFragment; // this is just a fragment like "car speed", used as a convenience var where we don't want to define a list of possibilities
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] voiceCommandFragments;

        [JsonConstructor]
        public OverlaySubscription(string id, OverlayDataType overlayDataType, string[] labels, string rawDataFieldName = null, string mappedDataFieldName = null,
            YAxisScaling yAxisMinScaling = YAxisScaling.AUTO, YAxisScaling yAxisMaxScaling = YAxisScaling.AUTO, float yMin = 0, float yMax = 0,
            string[] coloursLastLap = null, string[] coloursBestLap = null, string[] coloursOpponentBestLap = null, string yAxisFormat = null, bool includeOpponentData = false,
            string opponentDataFieldname = null, string[] groupMemberIds = null, string diskDataFieldname = null, string voiceCommandFragment = null, string[] voiceCommandFragments = null)
            : this(id, opponentDataFieldname, overlayDataType, rawDataFieldName, mappedDataFieldName, yAxisMinScaling, yAxisMaxScaling, yMin, yMax, yAxisFormat, groupMemberIds, diskDataFieldname)
        {
            this.labels = labels;
            this.coloursLastLap = coloursLastLap == null ? new string[0] : coloursLastLap;
            this.coloursBestLap = coloursBestLap == null ? new string[0] : coloursBestLap;
            this.coloursOpponentBestLap = coloursOpponentBestLap == null ? new string[0] : coloursOpponentBestLap;
            if (voiceCommandFragments == null || voiceCommandFragments.Count() == 0)
            {
                this.voiceCommandFragments = voiceCommandFragment.Split(':').Select(p => p.Trim()).ToArray();
            }
            else
            {
                this.voiceCommandFragments = voiceCommandFragments;
            }
            this.voiceCommandFragment = this.voiceCommandFragments[0];
        }

        private OverlaySubscription(string id, string opponentDataFieldname, OverlayDataType overlayDataType, string rawDataFieldName,
            string mappedDataFieldName, YAxisScaling yAxisMinScaling, YAxisScaling yAxisMaxScaling, float yMin, float yMax, string yAxisFormat, string[] groupMemberIds, string diskDataFieldname)
        {
            this.id = id;
            this.rawDataFieldName = rawDataFieldName;
            this.mappedDataFieldName = mappedDataFieldName;
            this.diskDataFieldname = diskDataFieldname;
            if (rawDataFieldName != null)
            {
                this.isRawField = true;
                this.fieldName = rawDataFieldName;
            }
            else
            {
                this.isRawField = false;
                this.fieldName = mappedDataFieldName;
            }
            this.overlayDataType = overlayDataType;
            this.yAxisMinScaling = yAxisMinScaling;
            this.yAxisMaxScaling = yAxisMaxScaling;
            this.yMin = yMin;
            this.yMax = yMax;
            this.yAxisFormat = yAxisFormat;
            this.opponentDataFieldname = opponentDataFieldname;
            if (opponentDataFieldname != null)
            {
                OverlayDataSource.mapOpponentData = true;
                OverlayDataSource.opponentDataFields[opponentDataFieldname] = overlayDataType;
            }
            if (groupMemberIds != null)
            {
                this.isGroup = true;
                this.groupMemberIds = groupMemberIds;
            }
            if (diskDataFieldname != null)
            {
                this.fieldName = diskDataFieldname;
                this.isDiskData = true;
            }
        }

        public List<string> getVoiceCommands()
        {
            List<string> commands = new List<string>();
            foreach (string addFragment in SpeechRecogniser.CHART_COMMAND_ADD)
            {
                foreach (string lastLapFragment in SpeechRecogniser.CHART_COMMAND_LAST_LAP)
                {
                    foreach (string singleVoiceCommandFragment in voiceCommandFragments)
                    {
                        commands.Add(addFragment + " " + lastLapFragment + " " + singleVoiceCommandFragment);
                    }
                }
                // special case for last lap - allow a shortened command "show me car speed":
                foreach (string singleVoiceCommandFragment in voiceCommandFragments)
                {
                    commands.Add(addFragment + " " + singleVoiceCommandFragment);
                }

                foreach (string bestLapFragment in SpeechRecogniser.CHART_COMMAND_BEST_LAP)
                {
                    foreach (string singleVoiceCommandFragment in voiceCommandFragments)
                    {
                        commands.Add(addFragment + " " + bestLapFragment + " " + singleVoiceCommandFragment);
                    }
                }
                foreach (string opponentBestLapFragment in SpeechRecogniser.CHART_COMMAND_OPPONENT_BEST_LAP)
                {
                    foreach (string singleVoiceCommandFragment in voiceCommandFragments)
                    {
                        commands.Add(addFragment + " " + opponentBestLapFragment + " " + singleVoiceCommandFragment);
                    }
                }
            }
            foreach (string removeFragment in SpeechRecogniser.CHART_COMMAND_REMOVE)
            {
                foreach (string lastLapFragment in SpeechRecogniser.CHART_COMMAND_LAST_LAP)
                {
                    foreach (string singleVoiceCommandFragment in voiceCommandFragments)
                    {
                        commands.Add(removeFragment + " " + lastLapFragment + " " + singleVoiceCommandFragment);
                    }
                }
                // special case for last lap - allow a shortened command - "chart, remove car speed":
                foreach (string singleVoiceCommandFragment in voiceCommandFragments)
                {
                    commands.Add(removeFragment + " " + singleVoiceCommandFragment);
                }
                foreach (string bestLapFragment in SpeechRecogniser.CHART_COMMAND_BEST_LAP)
                {
                    foreach (string singleVoiceCommandFragment in voiceCommandFragments)
                    {
                        commands.Add(removeFragment + " " + bestLapFragment + " " + singleVoiceCommandFragment);
                    }
                }
                foreach (string opponentBestLapFragment in SpeechRecogniser.CHART_COMMAND_OPPONENT_BEST_LAP)
                {
                    foreach (string singleVoiceCommandFragment in voiceCommandFragments)
                    {
                        commands.Add(removeFragment + " " + opponentBestLapFragment + " " + voiceCommandFragment);
                    }
                }
            }
            return commands;
        }
    }
}
