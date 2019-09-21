﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrewChiefV4.Events;
using System.Threading;
using CrewChiefV4.Audio;
using CrewChiefV4.commands;
using System.Windows.Forms;
using System.Diagnostics;
using CrewChiefV4.R3E;
using CrewChiefV4.SRE;
using System.Globalization;

namespace CrewChiefV4
{
    public class SpeechRecogniser : IDisposable
    {
        //private SpeechRecognitionEngine sre;

        private SREWrapper sreWrapper;

        private int nAudioWaveInSampleRate = UserSettings.GetUserSettings().getInt("naudio_wave_in_sample_rate");
        private int nAudioWaveInChannelCount = UserSettings.GetUserSettings().getInt("naudio_wave_in_channel_count");
        private int nAudioWaveInSampleDepth = UserSettings.GetUserSettings().getInt("naudio_wave_in_sample_depth");

        private Boolean identifyOpponentsByPosition = UserSettings.GetUserSettings().getBoolean("sre_enable_opponents_by_position");
        private Boolean identifyOpponentsByName = UserSettings.GetUserSettings().getBoolean("sre_enable_opponents_by_name");
        private Boolean identifyOpponentsByNumber = UserSettings.GetUserSettings().getBoolean("sre_enable_opponents_by_number");

        // used in nAudio mode:
        public static Dictionary<string, Tuple<string, int>> speechRecognitionDevices = new Dictionary<string, Tuple<string, int>>();
        public static int initialSpeechInputDeviceIndex = 0;
        private Boolean useNAudio = UserSettings.GetUserSettings().getBoolean("use_naudio_for_speech_recognition");
        private Boolean disableBehaviorAlteringVoiceCommands = UserSettings.GetUserSettings().getBoolean("disable_behavior_altering_voice_commands");
        private RingBufferStream.RingBufferStream buffer;
        private NAudio.Wave.WaveInEvent waveIn;

        private Thread nAudioAlwaysOnListenerThread = null;
        private bool nAudioAlwaysOnkeepRecording = false;

        private String localeCountryPropertySetting = UserSettings.GetUserSettings().getString("speech_recognition_country");

        private float minimum_name_voice_recognition_confidence = UserSettings.GetUserSettings().getFloat("minimum_name_voice_recognition_confidence");
        private float minimum_trigger_voice_recognition_confidence = UserSettings.GetUserSettings().getFloat("trigger_word_sre_min_confidence");
        private float minimum_voice_recognition_confidence = UserSettings.GetUserSettings().getFloat("minimum_voice_recognition_confidence");
        private Boolean disable_alternative_voice_commands = UserSettings.GetUserSettings().getBoolean("disable_alternative_voice_commands");
        private Boolean enable_iracing_pit_stop_commands = UserSettings.GetUserSettings().getBoolean("enable_iracing_pit_stop_commands");
        private static Boolean use_verbose_responses = UserSettings.GetUserSettings().getBoolean("use_verbose_responses");

        private static String sreConfigLanguageSetting = Configuration.getSpeechRecognitionConfigOption("language");
        private static String sreConfigDefaultLocaleSetting = Configuration.getSpeechRecognitionConfigOption("defaultLocale");

        private int trigger_word_listen_timeout = UserSettings.GetUserSettings().getInt("trigger_word_listen_timeout");

        private static Boolean alarmClockVoiceRecognitionEnabled = UserSettings.GetUserSettings().getBoolean("enable_alarm_clock_voice_recognition");

        public static String[] HOWS_MY_TYRE_WEAR = Configuration.getSpeechRecognitionPhrases("HOWS_MY_TYRE_WEAR");
        public static String[] HOWS_MY_TRANSMISSION = Configuration.getSpeechRecognitionPhrases("HOWS_MY_TRANSMISSION");
        public static String[] HOWS_MY_AERO = Configuration.getSpeechRecognitionPhrases("HOWS_MY_AERO");
        public static String[] HOWS_MY_ENGINE = Configuration.getSpeechRecognitionPhrases("HOWS_MY_ENGINE");
        public static String[] HOWS_MY_SUSPENSION = Configuration.getSpeechRecognitionPhrases("HOWS_MY_SUSPENSION");
        public static String[] HOWS_MY_BRAKES = Configuration.getSpeechRecognitionPhrases("HOWS_MY_BRAKES");
        public static String[] HOWS_MY_FUEL = Configuration.getSpeechRecognitionPhrases("HOWS_MY_FUEL");
        public static String[] HOWS_MY_BATTERY = Configuration.getSpeechRecognitionPhrases("HOWS_MY_BATTERY");
        public static String[] HOWS_MY_PACE = Configuration.getSpeechRecognitionPhrases("HOWS_MY_PACE");
        public static String[] HOWS_MY_SELF_PACE = Configuration.getSpeechRecognitionPhrases("HOWS_MY_SELF_PACE");
        public static String[] HOW_ARE_MY_TYRE_TEMPS = Configuration.getSpeechRecognitionPhrases("HOW_ARE_MY_TYRE_TEMPS");
        public static String[] WHAT_ARE_MY_TYRE_TEMPS = Configuration.getSpeechRecognitionPhrases("WHAT_ARE_MY_TYRE_TEMPS");
        public static String[] HOW_ARE_MY_BRAKE_TEMPS = Configuration.getSpeechRecognitionPhrases("HOW_ARE_MY_BRAKE_TEMPS");
        public static String[] WHAT_ARE_MY_BRAKE_TEMPS = Configuration.getSpeechRecognitionPhrases("WHAT_ARE_MY_BRAKE_TEMPS");
        public static String[] HOW_ARE_MY_ENGINE_TEMPS = Configuration.getSpeechRecognitionPhrases("HOW_ARE_MY_ENGINE_TEMPS");
        public static String[] WHATS_MY_GAP_IN_FRONT = Configuration.getSpeechRecognitionPhrases("WHATS_MY_GAP_IN_FRONT");
        public static String[] WHATS_MY_GAP_BEHIND = Configuration.getSpeechRecognitionPhrases("WHATS_MY_GAP_BEHIND");
        public static String[] WHAT_WAS_MY_LAST_LAP_TIME = Configuration.getSpeechRecognitionPhrases("WHAT_WAS_MY_LAST_LAP_TIME");
        public static String[] WHATS_MY_BEST_LAP_TIME = Configuration.getSpeechRecognitionPhrases("WHATS_MY_BEST_LAP_TIME");
        public static String[] WHATS_THE_FASTEST_LAP_TIME = Configuration.getSpeechRecognitionPhrases("WHATS_THE_FASTEST_LAP_TIME");
        public static String[] WHATS_MY_POSITION = Configuration.getSpeechRecognitionPhrases("WHATS_MY_POSITION");
        public static String[] WHATS_MY_FUEL_LEVEL = Configuration.getSpeechRecognitionPhrases("WHATS_MY_FUEL_LEVEL");
        public static String[] WHATS_MY_FUEL_USAGE = Configuration.getSpeechRecognitionPhrases("WHATS_MY_FUEL_USAGE");
        public static String[] WHATS_MY_IRATING = Configuration.getSpeechRecognitionPhrases("WHATS_MY_IRATING");
        public static String[] WHATS_MY_LICENSE_CLASS = Configuration.getSpeechRecognitionPhrases("WHATS_MY_LICENSE_CLASS");
        public static String[] WHAT_TYRES_AM_I_ON = Configuration.getSpeechRecognitionPhrases("WHAT_TYRES_AM_I_ON");
        public static String[] WHAT_ARE_THE_RELATIVE_TYRE_PERFORMANCES = Configuration.getSpeechRecognitionPhrases("WHAT_ARE_THE_RELATIVE_TYRE_PERFORMANCES");
        public static String[] HOW_LONG_WILL_THESE_TYRES_LAST = Configuration.getSpeechRecognitionPhrases("HOW_LONG_WILL_THESE_TYRES_LAST");
        public static String[] WHATS_PITLANE_SPEED_LIMIT = Configuration.getSpeechRecognitionPhrases("WHATS_PITLANE_SPEED_LIMIT");

        public static String[] HOW_MUCH_FUEL_TO_END_OF_RACE = Configuration.getSpeechRecognitionPhrases("HOW_MUCH_FUEL_TO_END_OF_RACE");
        public static String[] CALCULATE_FUEL_FOR = Configuration.getSpeechRecognitionPhrases("CALCULATE_FUEL_FOR");
        public static String[] LAP = Configuration.getSpeechRecognitionPhrases("LAP");
        public static String[] LAPS = Configuration.getSpeechRecognitionPhrases("LAPS");
        public static String[] MINUTE = Configuration.getSpeechRecognitionPhrases("MINUTE");
        public static String[] MINUTES = Configuration.getSpeechRecognitionPhrases("MINUTES");
        public static String[] HOUR = Configuration.getSpeechRecognitionPhrases("HOUR");
        public static String[] HOURS = Configuration.getSpeechRecognitionPhrases("HOURS");

        public static String[] KEEP_QUIET = Configuration.getSpeechRecognitionPhrases("KEEP_QUIET");
        public static String[] KEEP_ME_INFORMED = Configuration.getSpeechRecognitionPhrases("KEEP_ME_INFORMED");
        public static String[] TELL_ME_THE_GAPS = Configuration.getSpeechRecognitionPhrases("TELL_ME_THE_GAPS");
        public static String[] DONT_TELL_ME_THE_GAPS = Configuration.getSpeechRecognitionPhrases("DONT_TELL_ME_THE_GAPS");
        public static String[] TALK_TO_ME_ANYWHERE = Configuration.getSpeechRecognitionPhrases("TALK_TO_ME_ANYWHERE");
        public static String[] DONT_TALK_IN_THE_CORNERS = Configuration.getSpeechRecognitionPhrases("DONT_TALK_IN_THE_CORNERS");
        public static String[] WHATS_THE_TIME = Configuration.getSpeechRecognitionPhrases("WHATS_THE_TIME");
        public static String[] ENABLE_YELLOW_FLAG_MESSAGES = Configuration.getSpeechRecognitionPhrases("ENABLE_YELLOW_FLAG_MESSAGES");
        public static String[] DISABLE_YELLOW_FLAG_MESSAGES = Configuration.getSpeechRecognitionPhrases("DISABLE_YELLOW_FLAG_MESSAGES");
        public static String[] ENABLE_MANUAL_FORMATION_LAP = Configuration.getSpeechRecognitionPhrases("ENABLE_MANUAL_FORMATION_LAP");
        public static String[] DISABLE_MANUAL_FORMATION_LAP = Configuration.getSpeechRecognitionPhrases("DISABLE_MANUAL_FORMATION_LAP");

        public static String[] WHOS_IN_FRONT_IN_THE_RACE = Configuration.getSpeechRecognitionPhrases("WHOS_IN_FRONT_IN_THE_RACE");
        public static String[] WHOS_BEHIND_IN_THE_RACE = Configuration.getSpeechRecognitionPhrases("WHOS_BEHIND_IN_THE_RACE");
        public static String[] WHOS_IN_FRONT_ON_TRACK = Configuration.getSpeechRecognitionPhrases("WHOS_IN_FRONT_ON_TRACK");
        public static String[] WHOS_BEHIND_ON_TRACK = Configuration.getSpeechRecognitionPhrases("WHOS_BEHIND_ON_TRACK");
        public static String[] WHOS_LEADING = Configuration.getSpeechRecognitionPhrases("WHOS_LEADING");

        public static String[] WHERE_AM_I_FASTER = Configuration.getSpeechRecognitionPhrases("WHERE_AM_I_FASTER");
        public static String[] WHERE_AM_I_SLOWER = Configuration.getSpeechRecognitionPhrases("WHERE_AM_I_SLOWER");

        public static String[] HOW_LONGS_LEFT = Configuration.getSpeechRecognitionPhrases("HOW_LONGS_LEFT");
        public static String[] SPOT = Configuration.getSpeechRecognitionPhrases("SPOT");
        public static String[] DONT_SPOT = Configuration.getSpeechRecognitionPhrases("DONT_SPOT");
        public static String[] REPEAT_LAST_MESSAGE = Configuration.getSpeechRecognitionPhrases("REPEAT_LAST_MESSAGE");
        public static String[] HAVE_I_SERVED_MY_PENALTY = Configuration.getSpeechRecognitionPhrases("HAVE_I_SERVED_MY_PENALTY");
        public static String[] DO_I_HAVE_A_PENALTY = Configuration.getSpeechRecognitionPhrases("DO_I_HAVE_A_PENALTY");
        public static String[] DO_I_STILL_HAVE_A_PENALTY = Configuration.getSpeechRecognitionPhrases("DO_I_STILL_HAVE_A_PENALTY");
        public static String[] DO_I_HAVE_A_MANDATORY_PIT_STOP = Configuration.getSpeechRecognitionPhrases("DO_I_HAVE_A_MANDATORY_PIT_STOP");
        public static String[] WHAT_ARE_MY_SECTOR_TIMES = Configuration.getSpeechRecognitionPhrases("WHAT_ARE_MY_SECTOR_TIMES");
        public static String[] WHATS_MY_LAST_SECTOR_TIME = Configuration.getSpeechRecognitionPhrases("WHATS_MY_LAST_SECTOR_TIME");
        public static String[] WHATS_THE_AIR_TEMP = Configuration.getSpeechRecognitionPhrases("WHATS_THE_AIR_TEMP");
        public static String[] WHATS_THE_TRACK_TEMP = Configuration.getSpeechRecognitionPhrases("WHATS_THE_TRACK_TEMP");
        public static String[] RADIO_CHECK = Configuration.getSpeechRecognitionPhrases("RADIO_CHECK");

        public static String[] IS_MY_PIT_BOX_OCCUPIED = Configuration.getSpeechRecognitionPhrases("IS_MY_PIT_BOX_OCCUPIED");
        public static String[] PLAY_POST_PIT_POSITION_ESTIMATE = Configuration.getSpeechRecognitionPhrases("PLAY_POST_PIT_POSITION_ESTIMATE");
        public static String[] PRACTICE_PIT_STOP = Configuration.getSpeechRecognitionPhrases("PRACTICE_PIT_STOP");

        public static String[] ENABLE_CUT_TRACK_WARNINGS = Configuration.getSpeechRecognitionPhrases("ENABLE_CUT_TRACK_WARNINGS");
        public static String[] DISABLE_CUT_TRACK_WARNINGS = Configuration.getSpeechRecognitionPhrases("DISABLE_CUT_TRACK_WARNINGS");

        public static String[] HOWS_MY_LEFT_FRONT_CAMBER = Configuration.getSpeechRecognitionPhrases("HOWS_MY_LEFT_FRONT_CAMBER");
        public static String[] HOWS_MY_RIGHT_FRONT_CAMBER = Configuration.getSpeechRecognitionPhrases("HOWS_MY_RIGHT_FRONT_CAMBER");
        public static String[] HOWS_MY_LEFT_REAR_CAMBER = Configuration.getSpeechRecognitionPhrases("HOWS_MY_LEFT_REAR_CAMBER");
        public static String[] HOWS_MY_RIGHT_REAR_CAMBER = Configuration.getSpeechRecognitionPhrases("HOWS_MY_RIGHT_REAR_CAMBER");
        public static String[] HOWS_MY_FRONT_CAMBER = Configuration.getSpeechRecognitionPhrases("HOWS_MY_FRONT_CAMBER");
        public static String[] HOWS_MY_REAR_CAMBER = Configuration.getSpeechRecognitionPhrases("HOWS_MY_REAR_CAMBER");
        public static String[] HOW_ARE_MY_TYRE_PRESSURES = Configuration.getSpeechRecognitionPhrases("HOW_ARE_MY_TYRE_PRESSURES");
        public static String[] HOW_ARE_MY_FRONT_TYRE_PRESSURES = Configuration.getSpeechRecognitionPhrases("HOW_ARE_MY_FRONT_TYRE_PRESSURES");
        public static String[] HOW_ARE_MY_REAR_TYRE_PRESSURES = Configuration.getSpeechRecognitionPhrases("HOW_ARE_MY_REAR_TYRE_PRESSURES");
        public static String[] HOWS_MY_LEFT_FRONT_CAMBER_RIGHT_NOW = Configuration.getSpeechRecognitionPhrases("HOWS_MY_LEFT_FRONT_CAMBER_RIGHT_NOW");
        public static String[] HOWS_MY_RIGHT_FRONT_CAMBER_RIGHT_NOW = Configuration.getSpeechRecognitionPhrases("HOWS_MY_RIGHT_FRONT_CAMBER_RIGHT_NOW");
        public static String[] HOWS_MY_LEFT_REAR_CAMBER_RIGHT_NOW = Configuration.getSpeechRecognitionPhrases("HOWS_MY_LEFT_REAR_CAMBER_RIGHT_NOW");
        public static String[] HOWS_MY_RIGHT_REAR_CAMBER_RIGHT_NOW = Configuration.getSpeechRecognitionPhrases("HOWS_MY_RIGHT_REAR_CAMBER_RIGHT_NOW");
        public static String[] HOWS_MY_FRONT_CAMBER_RIGHT_NOW = Configuration.getSpeechRecognitionPhrases("HOWS_MY_FRONT_CAMBER_RIGHT_NOW");
        public static String[] HOWS_MY_REAR_CAMBER_RIGHT_NOW = Configuration.getSpeechRecognitionPhrases("HOWS_MY_REAR_CAMBER_RIGHT_NOW");
        public static String[] HOW_ARE_MY_TYRE_PRESSURES_RIGHT_NOW = Configuration.getSpeechRecognitionPhrases("HOW_ARE_MY_TYRE_PRESSURES_RIGHT_NOW");
        public static String[] HOW_ARE_MY_FRONT_TYRE_PRESSURES_RIGHT_NOW = Configuration.getSpeechRecognitionPhrases("HOW_ARE_MY_FRONT_TYRE_PRESSURES_RIGHT_NOW");
        public static String[] HOW_ARE_MY_REAR_TYRE_PRESSURES_RIGHT_NOW = Configuration.getSpeechRecognitionPhrases("HOW_ARE_MY_REAR_TYRE_PRESSURES_RIGHT_NOW");

        public static String[] STOP_COMPLAINING = Configuration.getSpeechRecognitionPhrases("STOP_COMPLAINING");

        // R3E only for now:
        public static String[] WHAT_ARE_THE_PIT_ACTIONS = Configuration.getSpeechRecognitionPhrases("WHAT_ARE_THE_PIT_ACTIONS");

        public static String ON = Configuration.getSpeechRecognitionConfigOption("ON");
        public static String POSSESSIVE = Configuration.getSpeechRecognitionConfigOption("POSSESSIVE");
        public static String WHERE_IS = Configuration.getSpeechRecognitionConfigOption("WHERE_IS");
        public static String WHERES = Configuration.getSpeechRecognitionConfigOption("WHERES");
        public static String POSITION_LONG = Configuration.getSpeechRecognitionConfigOption("POSITION_LONG");
        public static String POSITION_SHORT = Configuration.getSpeechRecognitionConfigOption("POSITION_SHORT");

        public static String WHOS_IN = Configuration.getSpeechRecognitionConfigOption("WHOS_IN");
        public static String WHATS = Configuration.getSpeechRecognitionConfigOption("WHATS");
        public static String BEST_LAP = Configuration.getSpeechRecognitionConfigOption("BEST_LAP");
        public static String BEST_LAP_TIME = Configuration.getSpeechRecognitionConfigOption("BEST_LAP_TIME");
        public static String LAST_LAP = Configuration.getSpeechRecognitionConfigOption("LAST_LAP");
        public static String LAST_LAP_TIME = Configuration.getSpeechRecognitionConfigOption("LAST_LAP_TIME");
        public static String THE_LEADER = Configuration.getSpeechRecognitionConfigOption("THE_LEADER");
        public static String THE_CAR_AHEAD = Configuration.getSpeechRecognitionConfigOption("THE_CAR_AHEAD");
        public static String THE_CAR_IN_FRONT = Configuration.getSpeechRecognitionConfigOption("THE_CAR_IN_FRONT");
        public static String THE_GUY_AHEAD = Configuration.getSpeechRecognitionConfigOption("THE_GUY_AHEAD");
        public static String THE_GUY_IN_FRONT = Configuration.getSpeechRecognitionConfigOption("THE_GUY_IN_FRONT");
        public static String THE_CAR_BEHIND = Configuration.getSpeechRecognitionConfigOption("THE_CAR_BEHIND");
        public static String THE_GUY_BEHIND = Configuration.getSpeechRecognitionConfigOption("THE_GUY_BEHIND");
        public static String CAR_NUMBER = Configuration.getSpeechRecognitionConfigOption("CAR_NUMBER");

        public static String WHAT_TYRES_IS = Configuration.getSpeechRecognitionConfigOption("WHAT_TYRES_IS");
        public static String WHAT_TYRE_IS = Configuration.getSpeechRecognitionConfigOption("WHAT_TYRE_IS");

        public static String IRATING = Configuration.getSpeechRecognitionConfigOption("IRATING");
        public static String LICENSE_CLASS = Configuration.getSpeechRecognitionConfigOption("LICENSE_CLASS");

        public static String[] PLAY_CORNER_NAMES = Configuration.getSpeechRecognitionPhrases("PLAY_CORNER_NAMES");

        public static String[] DAMAGE_REPORT = Configuration.getSpeechRecognitionPhrases("DAMAGE_REPORT");
        public static String[] CAR_STATUS = Configuration.getSpeechRecognitionPhrases("CAR_STATUS");
        public static String[] SESSION_STATUS = Configuration.getSpeechRecognitionPhrases("SESSION_STATUS");
        public static String[] STATUS = Configuration.getSpeechRecognitionPhrases("STATUS");

        public static String[] START_PACE_NOTES_PLAYBACK = Configuration.getSpeechRecognitionPhrases("START_PACE_NOTES_PLAYBACK");
        public static String[] STOP_PACE_NOTES_PLAYBACK = Configuration.getSpeechRecognitionPhrases("STOP_PACE_NOTES_PLAYBACK");

        // pitstop commands specific to iRacing:
        public static String[] PIT_STOP = Configuration.getSpeechRecognitionPhrases("PIT_STOP");
        public static String[] PIT_STOP_ADD = Configuration.getSpeechRecognitionPhrases("PIT_STOP_ADD");
        public static String[] LITERS = Configuration.getSpeechRecognitionPhrases("LITERS");
        public static String[] GALLONS = Configuration.getSpeechRecognitionPhrases("GALLONS");
        public static String[] PIT_STOP_TEAROFF = Configuration.getSpeechRecognitionPhrases("PIT_STOP_TEAROFF");
        public static String[] PIT_STOP_FAST_REPAIR = Configuration.getSpeechRecognitionPhrases("PIT_STOP_FAST_REPAIR");
        public static String[] PIT_STOP_CLEAR_ALL = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CLEAR_ALL");
        public static String[] PIT_STOP_CLEAR_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CLEAR_TYRES");
        public static String[] PIT_STOP_CLEAR_WIND_SCREEN = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CLEAR_WIND_SCREEN");
        public static String[] PIT_STOP_CLEAR_FAST_REPAIR = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CLEAR_FAST_REPAIR");
        public static String[] PIT_STOP_CLEAR_FUEL = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CLEAR_FUEL");

        public static String[] PIT_STOP_CHANGE_ALL_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_ALL_TYRES");
        public static String[] PIT_STOP_CHANGE_FRONT_LEFT_TYRE = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_FRONT_LEFT_TYRE");
        public static String[] PIT_STOP_CHANGE_FRONT_RIGHT_TYRE = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_FRONT_RIGHT_TYRE");
        public static String[] PIT_STOP_CHANGE_REAR_LEFT_TYRE = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_REAR_LEFT_TYRE");
        public static String[] PIT_STOP_CHANGE_REAR_RIGHT_TYRE = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_REAR_RIGHT_TYRE");

        public static String[] PIT_STOP_CHANGE_TYRE_PRESSURE = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_TYRE_PRESSURE");
        public static String[] PIT_STOP_CHANGE_FRONT_LEFT_TYRE_PRESSURE = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_FRONT_LEFT_TYRE_PRESSURE");
        public static String[] PIT_STOP_CHANGE_FRONT_RIGHT_TYRE_PRESSURE = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_FRONT_RIGHT_TYRE_PRESSURE");
        public static String[] PIT_STOP_CHANGE_REAR_LEFT_TYRE_PRESSURE = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_REAR_LEFT_TYRE_PRESSURE");
        public static String[] PIT_STOP_CHANGE_REAR_RIGHT_TYRE_PRESSURE = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_REAR_RIGHT_TYRE_PRESSURE");

        public static String[] PIT_STOP_CHANGE_LEFT_SIDE_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_LEFT_SIDE_TYRES");
        public static String[] PIT_STOP_CHANGE_RIGHT_SIDE_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_RIGHT_SIDE_TYRES");

        public static String[] PIT_STOP_CHANGE_FRONT_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_FRONT_TYRES");
        public static String[] PIT_STOP_CHANGE_REAR_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_CHANGE_REAR_TYRES");
        public static String[] PIT_STOP_FIX_FRONT_AERO = Configuration.getSpeechRecognitionPhrases("PIT_STOP_FIX_FRONT_AERO");
        public static String[] PIT_STOP_FIX_REAR_AERO = Configuration.getSpeechRecognitionPhrases("PIT_STOP_FIX_REAR_AERO");
        public static String[] PIT_STOP_FIX_ALL_AERO = Configuration.getSpeechRecognitionPhrases("PIT_STOP_FIX_ALL_AERO");
        public static String[] PIT_STOP_FIX_NO_AERO = Configuration.getSpeechRecognitionPhrases("PIT_STOP_FIX_NO_AERO");
        public static String[] PIT_STOP_FIX_SUSPENSION = Configuration.getSpeechRecognitionPhrases("PIT_STOP_FIX_SUSPENSION");
        public static String[] PIT_STOP_DONT_FIX_SUSPENSION = Configuration.getSpeechRecognitionPhrases("PIT_STOP_DONT_FIX_SUSPENSION");
        public static String[] PIT_STOP_SERVE_PENALTY = Configuration.getSpeechRecognitionPhrases("PIT_STOP_SERVE_PENALTY");
        public static String[] PIT_STOP_DONT_SERVE_PENALTY = Configuration.getSpeechRecognitionPhrases("PIT_STOP_DONT_SERVE_PENALTY");
        public static String[] PIT_STOP_REFUEL = Configuration.getSpeechRecognitionPhrases("PIT_STOP_REFUEL");
        public static String[] PIT_STOP_DONT_REFUEL = Configuration.getSpeechRecognitionPhrases("PIT_STOP_DONT_REFUEL");
        public static String[] PIT_STOP_NEXT_TYRE_COMPOUND = Configuration.getSpeechRecognitionPhrases("PIT_STOP_NEXT_TYRE_COMPOUND");
        public static String[] PIT_STOP_SOFT_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_SOFT_TYRES");
        public static String[] PIT_STOP_MEDIUM_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_MEDIUM_TYRES");
        public static String[] PIT_STOP_HARD_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_HARD_TYRES");
        public static String[] PIT_STOP_OPTION_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_OPTION_TYRES");
        public static String[] PIT_STOP_PRIME_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_PRIME_TYRES");
        public static String[] PIT_STOP_ALTERNATE_TYRES = Configuration.getSpeechRecognitionPhrases("PIT_STOP_ALTERNATE_TYRES");

        public static String[] HOW_MANY_INCIDENT_POINTS = Configuration.getSpeechRecognitionPhrases("HOW_MANY_INCIDENT_POINTS");
        public static String[] WHATS_THE_INCIDENT_LIMIT = Configuration.getSpeechRecognitionPhrases("WHATS_THE_INCIDENT_LIMIT");
        public static String[] WHATS_THE_SOF = Configuration.getSpeechRecognitionPhrases("WHATS_THE_SOF");

        public static String[] PIT_STOP_FUEL_TO_THE_END = Configuration.getSpeechRecognitionPhrases("PIT_STOP_FUEL_TO_THE_END");

        public static String[] MORE_INFO = Configuration.getSpeechRecognitionPhrases("MORE_INFO");

        public static String[] I_AM_OK = Configuration.getSpeechRecognitionPhrases("I_AM_OK");

        public static String[] IS_CAR_AHEAD_MY_CLASS = Configuration.getSpeechRecognitionPhrases("IS_CAR_AHEAD_MY_CLASS");
        public static String[] IS_CAR_BEHIND_MY_CLASS = Configuration.getSpeechRecognitionPhrases("IS_CAR_BEHIND_MY_CLASS");
        public static String[] WHAT_CLASS_IS_CAR_AHEAD = Configuration.getSpeechRecognitionPhrases("WHAT_CLASS_IS_CAR_AHEAD");
        public static String[] WHAT_CLASS_IS_CAR_BEHIND = Configuration.getSpeechRecognitionPhrases("WHAT_CLASS_IS_CAR_BEHIND");

        public static String[] SET_ALARM_CLOCK = Configuration.getSpeechRecognitionPhrases("SET_ALARM_CLOCK");
        public static String[] CLEAR_ALARM_CLOCK = Configuration.getSpeechRecognitionPhrases("CLEAR_ALARM_CLOCK");
        public static String[] AM = Configuration.getSpeechRecognitionPhrases("AM");
        public static String[] PM = Configuration.getSpeechRecognitionPhrases("PM");

        private Dictionary<GameEnum, string[]> whatsOpponentChoices = new Dictionary<GameEnum, string[]> {
            { GameEnum.IRACING, new String[] { LAST_LAP, LAST_LAP_TIME, BEST_LAP, BEST_LAP_TIME, IRATING, LICENSE_CLASS } },
            // the array for UNKNOWN is what we'll use if there's no game-specific array
            { GameEnum.UNKNOWN, new String[] { LAST_LAP, LAST_LAP_TIME, BEST_LAP, BEST_LAP_TIME } }
        };

        private String lastRecognisedText = null;

        private CrewChief crewChief;

        public Boolean initialised = false;

        public MainWindow.VoiceOptionEnum voiceOptionEnum;

        private HashSet<string> driverNamesInUse = new HashSet<string>();
        private HashSet<string> carNumbersInUse = new HashSet<string>();

        private List<GrammarWrapper> opponentGrammarList = new List<GrammarWrapper>();
        private List<GrammarWrapper> iracingPitstopGrammarList = new List<GrammarWrapper>();
        private List<GrammarWrapper> r3ePitstopGrammarList = new List<GrammarWrapper>();

        private GrammarWrapper macroGrammar = null;

        private Dictionary<String, ExecutableCommandMacro> macroLookup = new Dictionary<string, ExecutableCommandMacro>();

        private CultureInfo cultureInfo;

        public static Dictionary<String[], String> carNumberToNumber = getCarNumberMappings();

        public static Dictionary<String[], int> numberToNumber = getNumberMappings(1, 199);

        public static Dictionary<String[], int> racePositionNumberToNumber = getNumberMappings(1, 64);

        public static Dictionary<String[], int> hourMappings = getNumberMappings(0, 24);

        public static Dictionary<String[], int> minuteMappings = getNumberMappings(0, 59);

        private ChoicesWrapper digitsChoices;

        private ChoicesWrapper hourChoices;

        public static Boolean waitingForSpeech = false;

        public static Boolean gotRecognitionResult = false;

        // guard against race condition between closing channel and sre_SpeechRecognised event completing
        public static Boolean keepRecognisingInHoldMode = false;

        private SREWrapper triggerSreWrapper;

        // This is the trigger phrase used to activate the 'full' SRE
        private String keyWord = UserSettings.GetUserSettings().getString("trigger_word_for_always_on_sre");

        private EventWaitHandle triggerTimeoutWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private Thread restartWaitTimeoutThreadReference = null;

        public static DateTime recognitionStartedTime = DateTime.MinValue;


        // experimental free-dictation grammar for chat messages
        private Boolean useFreeDictationForChatMessages = UserSettings.GetUserSettings().getBoolean("use_free_dictation_for_chat");
        private static string startChatMacroName = "start chat message";
        private static string endChatMacroName = "end chat message";
        private static string chatContextStart = UserSettings.GetUserSettings().getString("free_dictation_chat_start_word");
        private string chatContextEnd = null;
        private GrammarWrapper chatDictationGrammar;
        private static ExecutableCommandMacro startChatMacro = null;
        private static ExecutableCommandMacro endChatMacro = null;

        static SpeechRecogniser()
        {
            if (UserSettings.GetUserSettings().getBoolean("use_naudio_for_speech_recognition"))
            {
                speechRecognitionDevices.Clear();
                for (int deviceId = 0; deviceId < NAudio.Wave.WaveIn.DeviceCount; deviceId++)
                {
                    // the audio device stuff makes no guarantee as to the presence of sensible device and product guids,
                    // so we have to do the best we can here
                    NAudio.Wave.WaveInCapabilities capabilities = NAudio.Wave.WaveIn.GetCapabilities(deviceId);
                    Boolean hasNameGuid = capabilities.NameGuid != null && !capabilities.NameGuid.Equals(Guid.Empty);
                    Boolean hasProductGuid = capabilities.ProductGuid != null && !capabilities.ProductGuid.Equals(Guid.Empty);
                    String rawName = capabilities.ProductName;
                    String name = rawName;
                    int nameAddition = 0;
                    while (speechRecognitionDevices.Keys.Contains(name))
                    {
                        nameAddition++;
                        name = rawName += "(" + nameAddition + ")";
                    }
                    String guidToUse;
                    if (hasNameGuid)
                    {
                        guidToUse = capabilities.NameGuid.ToString();
                    }
                    else if (hasProductGuid)
                    {
                        guidToUse = capabilities.ProductGuid.ToString() + "_" + name;
                    }
                    else
                    {
                        guidToUse = name;
                    }
                    speechRecognitionDevices.Add(name, new Tuple<string, int>(guidToUse, deviceId));
                }
            }
        }

        // load voice commands for triggering keyboard macros. The String key of the input Dictionary is the
        // command list key in speech_recognition_config.txt. When one of these phrases is heard the map value
        // CommandMacro is executed.
        public void loadMacroVoiceTriggers(Dictionary<string, ExecutableCommandMacro> voiceTriggeredMacros)
        {
            if (!initialised)
            {
                return;
            }
            macroLookup.Clear();
            if (macroGrammar != null && macroGrammar.Loaded())
            {
                sreWrapper.UnloadGrammar(macroGrammar);
            }
            if (voiceTriggeredMacros.Count == 0)
            {
                Console.WriteLine("No macro voice triggers defined for the current game.");
                return;
            }
            ChoicesWrapper macroChoices = SREWrapperFactory.createNewChoicesWrapper();
            foreach (KeyValuePair<String, ExecutableCommandMacro> entry in voiceTriggeredMacros)
            {
                String triggerPhrase = entry.Key;
                ExecutableCommandMacro executableCommandMacro = entry.Value;
                if (executableCommandMacro.macro.intRange != null)
                {
                    foreach (KeyValuePair<String[], int> numberEntry in numberToNumber)
                    {
                        if (numberEntry.Value >= executableCommandMacro.macro.intRange.Item1 && numberEntry.Value <= executableCommandMacro.macro.intRange.Item2)
                        {
                            String thisPhrase = executableCommandMacro.macro.startPhrase + numberEntry.Key[0] + executableCommandMacro.macro.endPhrase;
                            if (!macroLookup.ContainsKey(thisPhrase))
                            {
                                macroLookup.Add(thisPhrase, voiceTriggeredMacros[triggerPhrase]);
                            }
                            macroChoices.Add(thisPhrase);
                        }
                    }
                }
                else
                {
                    // validate?
                    if (!macroLookup.ContainsKey(triggerPhrase))
                    {
                        macroLookup.Add(triggerPhrase, voiceTriggeredMacros[triggerPhrase]);
                    }
                    macroChoices.Add(triggerPhrase);
                }
            }
            GrammarBuilderWrapper macroGrammarBuilder = SREWrapperFactory.createNewGrammarBuilderWrapper();
            macroGrammarBuilder.SetCulture(cultureInfo);
            macroGrammarBuilder.Append(macroChoices);
            macroGrammar = SREWrapperFactory.createNewGrammarWrapper(macroGrammarBuilder);
            sreWrapper.LoadGrammar(macroGrammar);
            Console.WriteLine("Loaded " + voiceTriggeredMacros.Count + " macro voice triggers into the speech recogniser");
        }

        private static Dictionary<String[], int> getNumberMappings(int start, int end)
        {
            Dictionary<String[], int> dict = new Dictionary<string[], int>();
            for (int i = start; i <= end; i++)
            {
                dict.Add(Configuration.getSpeechRecognitionPhrases(i.ToString()), i);
            }
            return dict;
        }


        private static Dictionary<String[], string> getCarNumberMappings()
        {
            Dictionary<String[], string> dict = new Dictionary<string[], string>();
            for (int i = 0; i <= 999; i++)
            {
                dict.Add(getPossibleCarNumberPhrases(i), i.ToString());
            }
            return dict;
        }

        private static string[] getPossibleCarNumberPhrases(int number)
        {
            List<string> phrases = new List<string>();
            string numberStr = number.ToString();
            phrases.AddRange(Configuration.getSpeechRecognitionPhrases(numberStr));
            if (number < 100)
            {
                // add a leading zero if 1 < 100
                numberStr = "0" + numberStr;
                phrases.AddRange(Configuration.getSpeechRecognitionPhrases(numberStr));
                if (number < 10)
                {
                    // add another leading zero if i < 10
                    numberStr = "0" + numberStr;
                    phrases.AddRange(Configuration.getSpeechRecognitionPhrases(numberStr));
                }
            }
            // for numbers >= 100 allow "one three one" and "one thirty one" forms
            if (number >= 100)
            {
                string leadingNumber = numberStr[0].ToString();
                string middleNumber = numberStr[1].ToString();
                string finalNumber = numberStr[2].ToString();
                string leadingNumberPhrase = Configuration.getSpeechRecognitionPhrases(leadingNumber)[0];   //"one" / "two" / etc
                if (middleNumber == "0")
                {
                    // need to add "one oh one", "five zero three", etc
                    string finalNumberPhrase = Configuration.getSpeechRecognitionPhrases(finalNumber)[0];   //"one" / "two" / etc
                    string[] zeroPhrases = Configuration.getSpeechRecognitionPhrases("0");   //"zero", "oh", etc
                    foreach (string zeroPhrase in zeroPhrases)
                    {
                        phrases.Add(leadingNumberPhrase + "-" + zeroPhrase + "-" + finalNumberPhrase);
                    }
                }
                else
                {
                    // need to add "one two three", "four sevent eight", etc
                    string middleNumberPhrase = Configuration.getSpeechRecognitionPhrases(middleNumber)[0];   //"one" / "two" / etc
                    string finalNumberPhrase = Configuration.getSpeechRecognitionPhrases(finalNumber)[0];   //"one" / "two" / etc
                    string combinedFinalNumbersPhrase = Configuration.getSpeechRecognitionPhrases(middleNumber + finalNumber)[0];   //"twenty-one" / "twenty-two" / etc
                    phrases.Add(leadingNumberPhrase + "-" + middleNumberPhrase + "-" + finalNumberPhrase);
                    phrases.Add(leadingNumberPhrase + "-" + combinedFinalNumbersPhrase);
                }
            }
            return phrases.ToArray();
        }

        // if alwaysUseAllPhrases is true, we add all the phrase options to the recogniser even if the disable_alternative_voice_commands option is true.
        // If alwaysUseAllAppends is true we do the same thing with the append options.
        //
        // The generatedGrammars are loaded by this method call, they're only returned to allow us to detect which grammar has been triggered - the
        // opponent grammar processing stuff needs this.
        private List<GrammarWrapper> addCompoundChoices(String[] phrases, Boolean alwaysUseAllPhrases, ChoicesWrapper choices, String[] append, Boolean alwaysUseAllAppends)
        {
            List<GrammarWrapper> generatedGrammars = new List<GrammarWrapper>();
            foreach (string s in phrases)
            {
                if (s == null || s.Trim().Count() == 0)
                {
                    continue;
                }
                GrammarBuilderWrapper gb = SREWrapperFactory.createNewGrammarBuilderWrapper();
                gb.SetCulture(cultureInfo);
                gb.Append(s);
                gb.Append(choices);
                Boolean addAppendChoices = false;
                if (append != null && append.Length > 0)
                {
                    ChoicesWrapper appendChoices = SREWrapperFactory.createNewChoicesWrapper();
                    foreach (string sa in append)
                    {
                        if (sa == null || sa.Trim().Count() == 0)
                        {
                            continue;
                        }
                        addAppendChoices = true;
                        appendChoices.Add(sa.Trim().Trim());
                        if (disable_alternative_voice_commands && !alwaysUseAllAppends)
                        {
                            break;
                        }
                    }
                    if (addAppendChoices)
                    {
                        gb.Append(appendChoices);
                    }
                }
                GrammarWrapper grammar = SREWrapperFactory.createNewGrammarWrapper(gb);
                sreWrapper.LoadGrammar(grammar);
                generatedGrammars.Add(grammar);
                if (disable_alternative_voice_commands && !alwaysUseAllPhrases)
                {
                    break;
                }
            }
            return generatedGrammars;
        }

        // ensure the SRE has stopped waiting for speech, and if we're in trigger-word mode, reset it to the
        // default audio input device
        public void stop()
        {
            if (!initialised)
            {
                return;
            }

            try
            {
                if (sreWrapper != null)
                {
                    sreWrapper.RecognizeAsyncCancel();
                }
                if (voiceOptionEnum == MainWindow.VoiceOptionEnum.TRIGGER_WORD)
                {
                    if (triggerSreWrapper != null)
                    {
                        triggerSreWrapper.RecognizeAsyncCancel();
                    }
                    if (sreWrapper != null)
                    {
                        sreWrapper.SetInputToDefaultAudioDevice();
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error resetting recogniser");
            }
        }

        public void Dispose()
        {
            if (!initialised)
            {
                return;
            }

            if (waveIn != null)
            {
                try
                {
                    waveIn.Dispose();
                }
                catch (Exception) { }
            }
            // VL: do not dispose SRE engines.  It is not clear when, and if ever any stupid outstanding Async call will complete.
            // Outstanding Async calls block Dispose on shutdown.
            //
            // Another option is not to call any Async calls from SpeechRecognizer.stop if MainWindow.instance is null.  However,
            // since we are not continuously re-creating SRE instances, it is safest to simply not Dispose, as it is very unlikely
            // to cause any system wide impact/leak.
            if (sreWrapper != null)
            {
                try
                {
                    sreWrapper.SetInputToNull();
                }
                catch (Exception) { }
                try
                {
                    //sre.Dispose();
                }
                catch (Exception) { }
                sreWrapper = null;
            }
            if (triggerSreWrapper != null)
            {
                try
                {
                    //triggerSre.Dispose();
                }
                catch (Exception) { }
                triggerSreWrapper = null;
            }
            initialised = false;
        }

        public SpeechRecogniser(CrewChief crewChief)
        {
            this.crewChief = crewChief;
            if (minimum_name_voice_recognition_confidence < 0 || minimum_name_voice_recognition_confidence > 1)
            {
                minimum_name_voice_recognition_confidence = 0.4f;
            }
            if (minimum_voice_recognition_confidence < 0 || minimum_voice_recognition_confidence > 1)
            {
                minimum_voice_recognition_confidence = 0.5f;
            }
            if (minimum_trigger_voice_recognition_confidence < 0 || minimum_trigger_voice_recognition_confidence > 1)
            {
                minimum_trigger_voice_recognition_confidence = 0.6f;
            }
        }

        private Tuple<String, String> parseLocalePropertyValue(String value)
        {
            if (value != null && value.Length > 1)
            {
                if (value.Length == 2)
                {
                    return new Tuple<String, String>(value.ToLowerInvariant(), null);
                }
                if (value.Length == 4)
                {
                    return new Tuple<String, String>(value.Substring(0, 2).ToLowerInvariant(), value.Substring(2).ToUpperInvariant());
                }
                if (value.Length == 5)
                {
                    return new Tuple<String, String>(value.Substring(0, 2).ToLowerInvariant(), value.Substring(3).ToUpperInvariant());
                }
            }
            return new Tuple<String, String>(null, null);
        }

        private Boolean initWithLocale()
        {
            Debug.Assert(!initialised);
            if (initialised)
            {
                return false;
            }
            LangCodes langCodes = getLangCodes();
            this.cultureInfo = SREWrapperFactory.GetCultureInfo(langCodes.langAndCountryToUse, langCodes.langToUse, true);

            if (cultureInfo != null)
            {
                Console.WriteLine("Got SRE for " + cultureInfo);
                this.sreWrapper = SREWrapperFactory.createNewSREWrapper(cultureInfo);
                this.triggerSreWrapper = SREWrapperFactory.createNewSREWrapper(cultureInfo);
                return this.sreWrapper != null;
            }
            if (langCodes.countryToUse == null)
            {
                if (langCodes.langToUse == "en")
                {
                    if (MessageBox.Show(Configuration.getUIString("install_any_speechlanguage_popup_text"), Configuration.getUIString("install_speechplatform_popup_title"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=27224");
                    }
                    Console.WriteLine("Unable to initialise speech engine with English voice recognition pack. " +
                    "Check that at least one of MSSpeech_SR_en-GB_TELE.msi, MSSpeech_SR_en-US_TELE.msi, " +
                    "MSSpeech_SR_en-AU_TELE.msi, MSSpeech_SR_en-CA_TELE.msi or MSSpeech_SR_en-IN_TELE.msi are installed." +
                    " It can be downloaded from https://www.microsoft.com/en-us/download/details.aspx?id=27224");
                }
                else
                {
                    if (MessageBox.Show(Configuration.getUIString("install_single_speechlanguage_popup_text_start") + langCodes.langToUse +
                    Configuration.getUIString("install_single_speechlanguage_popup_text_end"),
                    Configuration.getUIString("install_speechplatform_popup_title"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=27224");
                    }
                    Console.WriteLine("Unable to initialise speech engine with '" + langCodes.langToUse + "' voice recognition pack. " +
                    "Check that and appropriate language pack is installed." +
                    " They can be downloaded from https://www.microsoft.com/en-us/download/details.aspx?id=27224");
                }

                return false;
            }
            else
            {
                if (MessageBox.Show(Configuration.getUIString("install_single_speechlanguage_popup_text_start") + langCodes.langAndCountryToUse +
                    Configuration.getUIString("install_single_speechlanguage_popup_text_end"),
                    Configuration.getUIString("install_speechplatform_popup_title"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=27224");
                }
                Console.WriteLine("Unable to initialise speech engine with voice recognition pack for location " + langCodes.langAndCountryToUse +
                    ". Check MSSpeech_SR_" + langCodes.langAndCountryToUse + "_TELE.msi is installed." +
                    " It can be downloaded from https://www.microsoft.com/en-us/download/details.aspx?id=27224");

                return false;
            }
        }

        private void validateAndAdd(String[] speechPhrases, ChoicesWrapper choices)
        {
            if (speechPhrases != null && speechPhrases.Count() > 0)
            {
                Boolean valid = true;
                foreach (String s in speechPhrases)
                {
                    if (s == null || s.Trim().Count() == 0)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    if (disable_alternative_voice_commands)
                    {
                        choices.Add(speechPhrases[0]);
                    }
                    else
                    {
                        choices.Add(speechPhrases);
                    }
                }
            }
        }

        public void initialiseSpeechEngine()
        {
            initialised = false;
            if (useNAudio)
            {
                buffer = new RingBufferStream.RingBufferStream(48000);
                waveIn = new NAudio.Wave.WaveInEvent();
                waveIn.DeviceNumber = SpeechRecogniser.initialSpeechInputDeviceIndex;
            }
            // try to initialize SpeechRecognitionEngine if it trows user is most likely missing SpeechPlatformRuntime.msi from the system
            // catch it and tell user to go download.
            try
            {
                LangCodes langCodes = getLangCodes();
                Console.WriteLine("got language codes data " + langCodes.ToString());
                this.cultureInfo = SREWrapperFactory.GetCultureInfo(langCodes.langAndCountryToUse, langCodes.langToUse, false);
                // if we're using the system SRE, check we have the required language before proceeding
                if (SREWrapperFactory.useSystem && this.cultureInfo == null)
                {
                    // if we have no culture info here we need to fall back to the MS SRE and get the culture again
                    Console.WriteLine("Unable to get language for System SRE with lang " + langCodes.langToUse + " or " + langCodes.langAndCountryToUse +
                        ", will fall back to Microsoft SRE");
                    SREWrapperFactory.useSystem = false;
                    this.cultureInfo = SREWrapperFactory.GetCultureInfo(langCodes.langAndCountryToUse, langCodes.langToUse, false);
                }
                SREWrapperFactory.createNewSREWrapper(this.cultureInfo, true);
            }
            catch (Exception e)
            {
                if (MessageBox.Show(Configuration.getUIString("install_speechplatform_popup_text"), Configuration.getUIString("install_speechplatform_popup_title"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=27225");
                }
                Console.WriteLine("Unable to initialise speech engine. Check that SpeechPlatformRuntime.msi is installed. It can be downloaded from https://www.microsoft.com/en-us/download/details.aspx?id=27225");
                Console.WriteLine("Exception message: " + e.Message + " stack: " + e.StackTrace);
                return;
            }

            //this is not likely to throw but we try to catch it anyways. 
            try
            {
                if (!initWithLocale())
                {
                    return;
                }
                Console.WriteLine("Speech engine initialized successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to initialise speech engine.");
                Console.WriteLine("Exception message: " + e.Message);
            }

            try
            {
                if (useNAudio)
                {
                    waveIn.WaveFormat = new NAudio.Wave.WaveFormat(nAudioWaveInSampleRate, nAudioWaveInChannelCount);
                    waveIn.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(waveIn_DataAvailable);
                    waveIn.NumberOfBuffers = 3;
                }
                else
                {
                    sreWrapper.SetInputToDefaultAudioDevice();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to set default audio device, speech recognition may not function and may crash the app");
                Console.WriteLine("Exception message: " + e.Message);
            }
            try
            {
                if (disable_alternative_voice_commands)
                {
                    Console.WriteLine("*Alternative voice commands are disabled, only the first command from each line in speech_recognition_config.txt will be available*");
                }
                else
                {
                    Console.WriteLine("Loading all voice command alternatives from speech_recognition_config.txt");
                }
                this.digitsChoices = SREWrapperFactory.createNewChoicesWrapper();
                foreach (KeyValuePair<String[], int> entry in numberToNumber)
                {
                    foreach (String numberStr in entry.Key)
                    {
                        digitsChoices.Add(numberStr);
                    }
                }

                ChoicesWrapper staticSpeechChoices = SREWrapperFactory.createNewChoicesWrapper();
                validateAndAdd(HOWS_MY_TYRE_WEAR, staticSpeechChoices);
                validateAndAdd(HOWS_MY_TRANSMISSION, staticSpeechChoices);
                validateAndAdd(HOWS_MY_AERO, staticSpeechChoices);
                validateAndAdd(HOWS_MY_ENGINE, staticSpeechChoices);
                validateAndAdd(HOWS_MY_SUSPENSION, staticSpeechChoices);
                validateAndAdd(HOWS_MY_BRAKES, staticSpeechChoices);
                validateAndAdd(HOWS_MY_FUEL, staticSpeechChoices);
                validateAndAdd(HOWS_MY_BATTERY, staticSpeechChoices);
                validateAndAdd(HOWS_MY_PACE, staticSpeechChoices);
                validateAndAdd(HOWS_MY_SELF_PACE, staticSpeechChoices);
                validateAndAdd(HOW_ARE_MY_TYRE_TEMPS, staticSpeechChoices);
                validateAndAdd(WHAT_ARE_MY_TYRE_TEMPS, staticSpeechChoices);
                validateAndAdd(HOW_ARE_MY_BRAKE_TEMPS, staticSpeechChoices);
                validateAndAdd(WHAT_ARE_MY_BRAKE_TEMPS, staticSpeechChoices);
                validateAndAdd(HOW_ARE_MY_ENGINE_TEMPS, staticSpeechChoices);
                validateAndAdd(WHATS_MY_GAP_IN_FRONT, staticSpeechChoices);
                validateAndAdd(WHATS_MY_GAP_BEHIND, staticSpeechChoices);
                validateAndAdd(WHAT_WAS_MY_LAST_LAP_TIME, staticSpeechChoices);
                validateAndAdd(WHATS_MY_BEST_LAP_TIME, staticSpeechChoices);
                validateAndAdd(WHATS_MY_POSITION, staticSpeechChoices);
                validateAndAdd(WHATS_MY_FUEL_LEVEL, staticSpeechChoices);
                validateAndAdd(WHATS_MY_FUEL_USAGE, staticSpeechChoices);
                validateAndAdd(HOW_MUCH_FUEL_TO_END_OF_RACE, staticSpeechChoices);
                validateAndAdd(WHAT_TYRES_AM_I_ON, staticSpeechChoices);
                validateAndAdd(WHAT_ARE_THE_RELATIVE_TYRE_PERFORMANCES, staticSpeechChoices);
                validateAndAdd(PLAY_CORNER_NAMES, staticSpeechChoices);
                validateAndAdd(HOW_LONG_WILL_THESE_TYRES_LAST, staticSpeechChoices);
                validateAndAdd(WHATS_PITLANE_SPEED_LIMIT, staticSpeechChoices);

                validateAndAdd(DAMAGE_REPORT, staticSpeechChoices);
                validateAndAdd(CAR_STATUS, staticSpeechChoices);
                validateAndAdd(SESSION_STATUS, staticSpeechChoices);
                validateAndAdd(STATUS, staticSpeechChoices);

                validateAndAdd(START_PACE_NOTES_PLAYBACK, staticSpeechChoices);
                validateAndAdd(STOP_PACE_NOTES_PLAYBACK, staticSpeechChoices);

                validateAndAdd(HOWS_MY_LEFT_FRONT_CAMBER, staticSpeechChoices);
                validateAndAdd(HOWS_MY_RIGHT_FRONT_CAMBER, staticSpeechChoices);
                validateAndAdd(HOWS_MY_LEFT_REAR_CAMBER, staticSpeechChoices);
                validateAndAdd(HOWS_MY_RIGHT_REAR_CAMBER, staticSpeechChoices);
                validateAndAdd(HOWS_MY_FRONT_CAMBER, staticSpeechChoices);
                validateAndAdd(HOWS_MY_REAR_CAMBER, staticSpeechChoices);
                validateAndAdd(HOW_ARE_MY_TYRE_PRESSURES, staticSpeechChoices);
                validateAndAdd(HOW_ARE_MY_FRONT_TYRE_PRESSURES, staticSpeechChoices);
                validateAndAdd(HOW_ARE_MY_REAR_TYRE_PRESSURES, staticSpeechChoices);
                validateAndAdd(HOWS_MY_LEFT_FRONT_CAMBER_RIGHT_NOW, staticSpeechChoices);
                validateAndAdd(HOWS_MY_RIGHT_FRONT_CAMBER_RIGHT_NOW, staticSpeechChoices);
                validateAndAdd(HOWS_MY_LEFT_REAR_CAMBER_RIGHT_NOW, staticSpeechChoices);
                validateAndAdd(HOWS_MY_RIGHT_REAR_CAMBER_RIGHT_NOW, staticSpeechChoices);
                validateAndAdd(HOWS_MY_FRONT_CAMBER_RIGHT_NOW, staticSpeechChoices);
                validateAndAdd(HOWS_MY_REAR_CAMBER_RIGHT_NOW, staticSpeechChoices);
                validateAndAdd(HOW_ARE_MY_TYRE_PRESSURES_RIGHT_NOW, staticSpeechChoices);
                validateAndAdd(HOW_ARE_MY_FRONT_TYRE_PRESSURES_RIGHT_NOW, staticSpeechChoices);
                validateAndAdd(HOW_ARE_MY_REAR_TYRE_PRESSURES_RIGHT_NOW, staticSpeechChoices);
                validateAndAdd(WHAT_ARE_THE_PIT_ACTIONS, staticSpeechChoices);

                if (!disableBehaviorAlteringVoiceCommands)
                {
                    validateAndAdd(KEEP_QUIET, staticSpeechChoices);
                    validateAndAdd(KEEP_ME_INFORMED, staticSpeechChoices);
                    validateAndAdd(TELL_ME_THE_GAPS, staticSpeechChoices);
                    validateAndAdd(DONT_TELL_ME_THE_GAPS, staticSpeechChoices);
                    validateAndAdd(ENABLE_YELLOW_FLAG_MESSAGES, staticSpeechChoices);
                    validateAndAdd(DISABLE_YELLOW_FLAG_MESSAGES, staticSpeechChoices);
                    validateAndAdd(ENABLE_MANUAL_FORMATION_LAP, staticSpeechChoices);
                    validateAndAdd(DISABLE_MANUAL_FORMATION_LAP, staticSpeechChoices);
                    validateAndAdd(TALK_TO_ME_ANYWHERE, staticSpeechChoices);
                    validateAndAdd(DONT_TALK_IN_THE_CORNERS, staticSpeechChoices);
                    validateAndAdd(SPOT, staticSpeechChoices);
                    validateAndAdd(DONT_SPOT, staticSpeechChoices);
                    validateAndAdd(ENABLE_CUT_TRACK_WARNINGS, staticSpeechChoices);
                    validateAndAdd(DISABLE_CUT_TRACK_WARNINGS, staticSpeechChoices);
                    validateAndAdd(STOP_COMPLAINING, staticSpeechChoices);
                }

                validateAndAdd(WHATS_THE_FASTEST_LAP_TIME, staticSpeechChoices);

                validateAndAdd(WHERE_AM_I_FASTER, staticSpeechChoices);
                validateAndAdd(WHERE_AM_I_SLOWER, staticSpeechChoices);

                validateAndAdd(HOW_LONGS_LEFT, staticSpeechChoices);
                validateAndAdd(WHATS_THE_TIME, staticSpeechChoices);
                validateAndAdd(REPEAT_LAST_MESSAGE, staticSpeechChoices);
                validateAndAdd(HAVE_I_SERVED_MY_PENALTY, staticSpeechChoices);
                validateAndAdd(DO_I_HAVE_A_PENALTY, staticSpeechChoices);
                validateAndAdd(DO_I_STILL_HAVE_A_PENALTY, staticSpeechChoices);
                validateAndAdd(IS_MY_PIT_BOX_OCCUPIED, staticSpeechChoices);
                validateAndAdd(PLAY_POST_PIT_POSITION_ESTIMATE, staticSpeechChoices);
                validateAndAdd(PRACTICE_PIT_STOP, staticSpeechChoices);
                validateAndAdd(DO_I_HAVE_A_MANDATORY_PIT_STOP, staticSpeechChoices);
                validateAndAdd(WHAT_ARE_MY_SECTOR_TIMES, staticSpeechChoices);
                validateAndAdd(WHATS_MY_LAST_SECTOR_TIME, staticSpeechChoices);
                validateAndAdd(WHATS_THE_AIR_TEMP, staticSpeechChoices);
                validateAndAdd(WHATS_THE_TRACK_TEMP, staticSpeechChoices);
                validateAndAdd(RADIO_CHECK, staticSpeechChoices);

                validateAndAdd(WHOS_IN_FRONT_IN_THE_RACE, staticSpeechChoices);
                validateAndAdd(WHOS_BEHIND_IN_THE_RACE, staticSpeechChoices);
                validateAndAdd(WHOS_IN_FRONT_ON_TRACK, staticSpeechChoices);
                validateAndAdd(WHOS_BEHIND_ON_TRACK, staticSpeechChoices);
                validateAndAdd(WHOS_LEADING, staticSpeechChoices);

                validateAndAdd(WHAT_CLASS_IS_CAR_AHEAD, staticSpeechChoices);
                validateAndAdd(WHAT_CLASS_IS_CAR_BEHIND, staticSpeechChoices);
                validateAndAdd(IS_CAR_AHEAD_MY_CLASS, staticSpeechChoices);
                validateAndAdd(IS_CAR_BEHIND_MY_CLASS, staticSpeechChoices);

                validateAndAdd(MORE_INFO, staticSpeechChoices);

                validateAndAdd(I_AM_OK, staticSpeechChoices);
                if (alarmClockVoiceRecognitionEnabled)
                {
                    validateAndAdd(CLEAR_ALARM_CLOCK, staticSpeechChoices);
                }
                GrammarBuilderWrapper staticGrammarBuilder = SREWrapperFactory.createNewGrammarBuilderWrapper();
                staticGrammarBuilder.SetCulture(cultureInfo);
                staticGrammarBuilder.Append(staticSpeechChoices);
                GrammarWrapper staticGrammar = SREWrapperFactory.createNewGrammarWrapper(staticGrammarBuilder);
                sreWrapper.LoadGrammar(staticGrammar);

                // now the fuel choices
                List<string> fuelTimeChoices = new List<string>();
                if (disable_alternative_voice_commands)
                {
                    fuelTimeChoices.Add(LAPS[0]);
                    fuelTimeChoices.Add(MINUTES[0]);
                    fuelTimeChoices.Add(HOURS[0]);
                }
                else
                {
                    fuelTimeChoices.AddRange(LAPS);
                    fuelTimeChoices.AddRange(MINUTES);
                    fuelTimeChoices.AddRange(HOURS);
                }
                addCompoundChoices(CALCULATE_FUEL_FOR, false, this.digitsChoices, fuelTimeChoices.ToArray(), true);
                if (alarmClockVoiceRecognitionEnabled)
                {
                    this.hourChoices = SREWrapperFactory.createNewChoicesWrapper();
                    foreach (KeyValuePair<String[], int> entry in hourMappings)
                    {
                        foreach (String numberStr in entry.Key)
                        {
                            hourChoices.Add(numberStr);
                        }
                    }
                    List<String> minuteArray = new List<String>();
                    foreach (KeyValuePair<String[], int> entry in minuteMappings)
                    {
                        foreach (String numberStr in entry.Key)
                        {
                            minuteArray.Add(numberStr);
                            foreach (String ams in AM)
                            {
                                minuteArray.Add(numberStr + " " + AM);
                            }
                            foreach (String pms in PM)
                            {
                                minuteArray.Add(numberStr + " " + PM);
                            }
                        }
                    }
                    addCompoundChoices(SET_ALARM_CLOCK, false, this.hourChoices, minuteArray.ToArray(), true);
                }

                if (SREWrapperFactory.useSystem && useFreeDictationForChatMessages)
                {
                    this.chatDictationGrammar = SREWrapperFactory.CreateChatDictationGrammarWrapper();
                    SREWrapperFactory.LoadChatDictationGrammar(this.sreWrapper, this.chatDictationGrammar, chatContextStart, chatContextEnd);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to configure speech engine grammar");
                Console.WriteLine("Exception message: " + e.Message);
                return;
            }
            sreWrapper.SetInitialSilenceTimeout(TimeSpan.Zero);
            try
            {
                if (SREWrapperFactory.useSystem)
                {
                    sreWrapper.AddSpeechRecognizedCallback(new EventHandler<System.Speech.Recognition.SpeechRecognizedEventArgs>(sre_SpeechRecognizedSystem));
                    triggerSreWrapper.AddSpeechRecognizedCallback(new EventHandler<System.Speech.Recognition.SpeechRecognizedEventArgs>(trigger_SpeechRecognizedSystem));
                }
                else
                {
                    sreWrapper.AddSpeechRecognizedCallback(new EventHandler<Microsoft.Speech.Recognition.SpeechRecognizedEventArgs>(sre_SpeechRecognizedMicrosoft));
                    triggerSreWrapper.AddSpeechRecognizedCallback(new EventHandler<Microsoft.Speech.Recognition.SpeechRecognizedEventArgs>(trigger_SpeechRecognizedMicrosoft));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to add event handler to speech engine");
                Console.WriteLine("Exception message: " + e.Message);
                return;
            }
            initialised = true;
        }

        private String[] getWhatsPossessiveChoices()
        {
            return CrewChief.gameDefinition != null && whatsOpponentChoices.ContainsKey(CrewChief.gameDefinition.gameEnum) ?
                whatsOpponentChoices[CrewChief.gameDefinition.gameEnum] : whatsOpponentChoices[GameEnum.UNKNOWN];
        }

        public void addNewOpponentName(String rawDriverName, String carNumberString)
        {
            if (!initialised || (!identifyOpponentsByName && !identifyOpponentsByNumber))
            {
                return;
            }
            try
            {
                if (initialised)
                {
                    String usableName = DriverNameHelper.getUsableDriverName(rawDriverName);
                    Console.WriteLine("Adding new (mid-session joined) opponent name to speech recogniser: " + Environment.NewLine + usableName);
                    // This method is called when a new driver appears mid-session. We need to load the sound file for this new driver
                    // so do it here - nasty nasty hack, need to refactor this. The alternative is to call
                    // SoundCache.loadDriverNameSound in each of mappers when a new driver is added.
                    SoundCache.loadDriverNameSound(usableName);

                    HashSet<string> nameChoices = new HashSet<string>();
                    HashSet<string> namePossessiveChoices = new HashSet<string>();
                    if (identifyOpponentsByName && usableName != null && usableName.Length > 0 && !driverNamesInUse.Contains(rawDriverName))
                    {
                        driverNamesInUse.Add(rawDriverName);
                        nameChoices.Add(usableName);
                        namePossessiveChoices.Add(usableName + POSSESSIVE);
                    }
                    if (identifyOpponentsByNumber && carNumberString != "-1" && carNumberToNumber.ContainsValue(carNumberString))
                    {
                        if (!carNumbersInUse.Contains(carNumberString))
                        {
                            carNumbersInUse.Add(carNumberString);
                            String[] numberOptions = carNumberToNumber.FirstOrDefault(x => x.Value == carNumberString).Key;
                            foreach (String number in numberOptions)
                            {
                                nameChoices.Add(CAR_NUMBER + " " + number);
                                namePossessiveChoices.Add(CAR_NUMBER + " " + number + POSSESSIVE);
                            }
                        }
                        // if the car number has a 0 or 00 in front of it, also listen for the number without the leading zero(s)
                        if (carNumberString.StartsWith("0"))
                        {
                            int parsed;
                            if (int.TryParse(carNumberString, out parsed))
                            {
                                String carNumberStringAlternate = parsed.ToString();
                                if (!carNumbersInUse.Contains(carNumberStringAlternate))
                                {
                                    carNumbersInUse.Add(carNumberStringAlternate);
                                    String[] numberOptionsWithoutLeadingZeros = carNumberToNumber.FirstOrDefault(x => x.Value == parsed.ToString()).Key;
                                    foreach (String number in numberOptionsWithoutLeadingZeros)
                                    {
                                        nameChoices.Add(CAR_NUMBER + " " + number);
                                        namePossessiveChoices.Add(CAR_NUMBER + " " + number + POSSESSIVE);
                                    }
                                }
                            }
                        }
                    }
                    ChoicesWrapper opponentNameChoices = SREWrapperFactory.createNewChoicesWrapper(nameChoices.ToArray<string>());
                    ChoicesWrapper opponentNamePossessiveChoices = SREWrapperFactory.createNewChoicesWrapper(namePossessiveChoices.ToArray<string>());

                    opponentGrammarList.AddRange(addCompoundChoices(new String[] { WHERE_IS, WHERES }, false, opponentNameChoices, null, true));
                    // todo: iracing definitely has no opponent tyre type data, probably more games lack this info
                    if (CrewChief.gameDefinition != null && CrewChief.gameDefinition.gameEnum != GameEnum.IRACING)
                    {
                        opponentGrammarList.AddRange(addCompoundChoices(new String[] { WHAT_TYRE_IS, WHAT_TYRES_IS }, false, opponentNameChoices, new String[] { ON }, true));
                    }
                    opponentGrammarList.AddRange(addCompoundChoices(new String[] { WHATS }, true, opponentNamePossessiveChoices, getWhatsPossessiveChoices(), true));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to add new driver to speech recognition engine - " + e.Message);
            }

        }

        public void addOpponentSpeechRecognition(List<String> names, HashSet<string> carNumbers)
        {
            if (!initialised)
            {
                return;
            }
            driverNamesInUse.Clear();
            carNumbersInUse.Clear();
            foreach (GrammarWrapper opponentGrammar in opponentGrammarList)
            {
                sreWrapper.UnloadGrammar(opponentGrammar);
            }
            opponentGrammarList.Clear();
            ChoicesWrapper opponentChoices = SREWrapperFactory.createNewChoicesWrapper();

            // need choice sets for names, possessive names, positions, possessive positions, and combined:
            ChoicesWrapper opponentNameOrPositionChoices = SREWrapperFactory.createNewChoicesWrapper();
            ChoicesWrapper opponentPositionChoices = SREWrapperFactory.createNewChoicesWrapper();
            ChoicesWrapper opponentNameOrPositionPossessiveChoices = SREWrapperFactory.createNewChoicesWrapper();

            if (identifyOpponentsByName)
            {
                Console.WriteLine("Adding " + names.Count + " new session opponent names to speech recogniser");
                foreach (String name in names)
                {
                    opponentNameOrPositionChoices.Add(name);
                    opponentNameOrPositionPossessiveChoices.Add(name + POSSESSIVE);
                    driverNamesInUse.Add(name);
                }
            }

            if (identifyOpponentsByPosition)
            {
                foreach (KeyValuePair<String[], int> entry in racePositionNumberToNumber)
                {
                    foreach (String numberStr in entry.Key)
                    {
                        opponentNameOrPositionChoices.Add(POSITION_SHORT + " " + numberStr);
                        opponentPositionChoices.Add(POSITION_SHORT + " " + numberStr);
                        opponentNameOrPositionPossessiveChoices.Add(POSITION_SHORT + " " + numberStr + POSSESSIVE);
                        if (!disable_alternative_voice_commands)
                        {
                            opponentNameOrPositionChoices.Add(POSITION_LONG + " " + numberStr);
                            opponentPositionChoices.Add(POSITION_LONG + " " + numberStr);
                            opponentNameOrPositionPossessiveChoices.Add(POSITION_LONG + " " + numberStr + POSSESSIVE);
                        }
                    }
                }
            }
            if (identifyOpponentsByNumber)
            {
                foreach (string carNumberString in carNumbers)
                {
                    if (carNumberString != "-1" && carNumberToNumber.ContainsValue(carNumberString))
                    {
                        if (!carNumbersInUse.Contains(carNumberString))
                        {
                            carNumbersInUse.Add(carNumberString);
                            String[] numberOptions = carNumberToNumber.FirstOrDefault(x => x.Value == carNumberString).Key;
                            foreach (String number in numberOptions)
                            {
                                opponentNameOrPositionChoices.Add(CAR_NUMBER + " " + number);
                                opponentNameOrPositionPossessiveChoices.Add(CAR_NUMBER + " " + number + POSSESSIVE);
                            }
                        }
                        // if the car number has a 0 or 00 in front of it, also listen for the number without the leading zero(s)
                        if (carNumberString.StartsWith("0"))
                        {
                            int parsed;
                            if (int.TryParse(carNumberString, out parsed))
                            {
                                String carNumberStringAlternate = parsed.ToString();
                                if (!carNumbersInUse.Contains(carNumberStringAlternate))
                                {
                                    carNumbersInUse.Add(carNumberStringAlternate);
                                    String[] numberOptionsWithoutLeadingZeros = carNumberToNumber.FirstOrDefault(x => x.Value == carNumberStringAlternate).Key;
                                    foreach (String number in numberOptionsWithoutLeadingZeros)
                                    {
                                        opponentNameOrPositionChoices.Add(CAR_NUMBER + " " + number);
                                        opponentNameOrPositionPossessiveChoices.Add(CAR_NUMBER + " " + number + POSSESSIVE);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            opponentNameOrPositionChoices.Add(THE_CAR_AHEAD);
            opponentNameOrPositionChoices.Add(THE_CAR_BEHIND);
            opponentNameOrPositionChoices.Add(THE_LEADER);
            opponentNameOrPositionPossessiveChoices.Add(THE_CAR_AHEAD);
            opponentNameOrPositionPossessiveChoices.Add(THE_CAR_BEHIND);
            opponentNameOrPositionPossessiveChoices.Add(THE_LEADER);

            if (!disable_alternative_voice_commands)
            {
                opponentNameOrPositionChoices.Add(THE_GUY_AHEAD);
                opponentNameOrPositionChoices.Add(THE_CAR_IN_FRONT);
                opponentNameOrPositionChoices.Add(THE_GUY_IN_FRONT);
                opponentNameOrPositionChoices.Add(THE_GUY_BEHIND);
                opponentNameOrPositionPossessiveChoices.Add(THE_GUY_AHEAD);
                opponentNameOrPositionPossessiveChoices.Add(THE_CAR_IN_FRONT);
                opponentNameOrPositionPossessiveChoices.Add(THE_GUY_IN_FRONT);
                opponentNameOrPositionPossessiveChoices.Add(THE_GUY_BEHIND);
            }

            opponentGrammarList.AddRange(addCompoundChoices(new String[] { WHERE_IS, WHERES }, false, opponentNameOrPositionChoices, null, true));
            if (identifyOpponentsByPosition)
            {
                opponentGrammarList.AddRange(addCompoundChoices(new String[] { WHOS_IN }, false, opponentPositionChoices, null, true));
            }
            if (CrewChief.gameDefinition != null && CrewChief.gameDefinition.gameEnum != GameEnum.IRACING)
            {
                opponentGrammarList.AddRange(addCompoundChoices(new String[] { WHAT_TYRE_IS, WHAT_TYRES_IS }, false, opponentNameOrPositionChoices, new String[] { ON }, true));
            }
            opponentGrammarList.AddRange(addCompoundChoices(new String[] { WHATS }, false, opponentNameOrPositionPossessiveChoices, getWhatsPossessiveChoices(), true));
        }

        public void addR3ESpeechRecogniser()
        {
            if (!initialised)
            {
                return;
            }
            try
            {
                iracingPitstopGrammarList.Clear();
                r3ePitstopGrammarList.Clear();
                ChoicesWrapper r3eChoices = SREWrapperFactory.createNewChoicesWrapper();
                validateAndAdd(PIT_STOP_CHANGE_ALL_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_CHANGE_FRONT_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_CHANGE_REAR_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_CLEAR_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_FIX_FRONT_AERO, r3eChoices);
                validateAndAdd(PIT_STOP_FIX_REAR_AERO, r3eChoices);
                validateAndAdd(PIT_STOP_FIX_ALL_AERO, r3eChoices);
                validateAndAdd(PIT_STOP_FIX_NO_AERO, r3eChoices);
                validateAndAdd(PIT_STOP_FIX_SUSPENSION, r3eChoices);
                validateAndAdd(PIT_STOP_DONT_FIX_SUSPENSION, r3eChoices);
                validateAndAdd(PIT_STOP_SERVE_PENALTY, r3eChoices);
                validateAndAdd(PIT_STOP_DONT_SERVE_PENALTY, r3eChoices);
                validateAndAdd(PIT_STOP_NEXT_TYRE_COMPOUND, r3eChoices);
                validateAndAdd(PIT_STOP_HARD_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_MEDIUM_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_SOFT_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_PRIME_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_ALTERNATE_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_OPTION_TYRES, r3eChoices);
                validateAndAdd(PIT_STOP_DONT_REFUEL, r3eChoices);
                validateAndAdd(PIT_STOP_REFUEL, r3eChoices);

                GrammarBuilderWrapper r3eGrammarBuilder = SREWrapperFactory.createNewGrammarBuilderWrapper(r3eChoices);
                r3eGrammarBuilder.SetCulture(cultureInfo);
                GrammarWrapper r3eGrammar = SREWrapperFactory.createNewGrammarWrapper(r3eGrammarBuilder);
                r3ePitstopGrammarList.Add(r3eGrammar);
                sreWrapper.LoadGrammar(r3eGrammar);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to add R3E pit stop commands to speech recognition engine - " + e.Message);
            }
        }

        public void unloadGameSpecificGrammars()
        {
            foreach (GrammarWrapper iracingGrammar in iracingPitstopGrammarList)
            {
                try
                {
                    sreWrapper.UnloadGrammar(iracingGrammar);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to unload iRacing grammar: " + e.Message);
                }
            }
            foreach (GrammarWrapper r3eGrammar in r3ePitstopGrammarList)
            {
                try
                {
                    sreWrapper.UnloadGrammar(r3eGrammar);
                }
                catch (Exception e)
                {
                    // ignore - we might be switching between games here
                }
            }
        }

        public void addiRacingSpeechRecogniser()
        {
            if (!initialised)
            {
                return;
            }
            try
            {
                r3ePitstopGrammarList.Clear();
                iracingPitstopGrammarList.Clear();
                ChoicesWrapper iRacingChoices = SREWrapperFactory.createNewChoicesWrapper();
                if (enable_iracing_pit_stop_commands)
                {
                    List<string> tyrePressureChangePhrases = new List<string>();
                    if (disable_alternative_voice_commands)
                    {
                        tyrePressureChangePhrases.Add(PIT_STOP_CHANGE_TYRE_PRESSURE[0]);
                        tyrePressureChangePhrases.Add(PIT_STOP_CHANGE_FRONT_LEFT_TYRE_PRESSURE[0]);
                        tyrePressureChangePhrases.Add(PIT_STOP_CHANGE_FRONT_RIGHT_TYRE_PRESSURE[0]);
                        tyrePressureChangePhrases.Add(PIT_STOP_CHANGE_REAR_LEFT_TYRE_PRESSURE[0]);
                        tyrePressureChangePhrases.Add(PIT_STOP_CHANGE_REAR_RIGHT_TYRE_PRESSURE[0]);
                    }
                    else
                    {
                        tyrePressureChangePhrases.AddRange(PIT_STOP_CHANGE_TYRE_PRESSURE);
                        tyrePressureChangePhrases.AddRange(PIT_STOP_CHANGE_FRONT_LEFT_TYRE_PRESSURE);
                        tyrePressureChangePhrases.AddRange(PIT_STOP_CHANGE_FRONT_RIGHT_TYRE_PRESSURE);
                        tyrePressureChangePhrases.AddRange(PIT_STOP_CHANGE_REAR_LEFT_TYRE_PRESSURE);
                        tyrePressureChangePhrases.AddRange(PIT_STOP_CHANGE_REAR_RIGHT_TYRE_PRESSURE);
                    }

                    iracingPitstopGrammarList.AddRange(addCompoundChoices(tyrePressureChangePhrases.ToArray(), true, this.digitsChoices, null, true));
                    List<string> litresAndGallons = new List<string>();
                    litresAndGallons.AddRange(LITERS);
                    litresAndGallons.AddRange(GALLONS);
                    iracingPitstopGrammarList.AddRange(addCompoundChoices(PIT_STOP_ADD, false, this.digitsChoices, litresAndGallons.ToArray(), true));
                    // add the fuel choices with no unit - these use the default / reported unit for fuel
                    iracingPitstopGrammarList.AddRange(addCompoundChoices(PIT_STOP_ADD, false, this.digitsChoices, null, true));

                    validateAndAdd(PIT_STOP_TEAROFF, iRacingChoices);
                    validateAndAdd(PIT_STOP_FAST_REPAIR, iRacingChoices);
                    validateAndAdd(PIT_STOP_CLEAR_ALL, iRacingChoices);
                    validateAndAdd(PIT_STOP_CLEAR_TYRES, iRacingChoices);
                    validateAndAdd(PIT_STOP_CLEAR_WIND_SCREEN, iRacingChoices);
                    validateAndAdd(PIT_STOP_CLEAR_FAST_REPAIR, iRacingChoices);
                    validateAndAdd(PIT_STOP_CLEAR_FUEL, iRacingChoices);
                    validateAndAdd(PIT_STOP_CHANGE_ALL_TYRES, iRacingChoices);
                    validateAndAdd(PIT_STOP_CHANGE_FRONT_LEFT_TYRE, iRacingChoices);
                    validateAndAdd(PIT_STOP_CHANGE_FRONT_RIGHT_TYRE, iRacingChoices);
                    validateAndAdd(PIT_STOP_CHANGE_REAR_LEFT_TYRE, iRacingChoices);
                    validateAndAdd(PIT_STOP_CHANGE_REAR_RIGHT_TYRE, iRacingChoices);
                    validateAndAdd(PIT_STOP_CHANGE_LEFT_SIDE_TYRES, iRacingChoices);
                    validateAndAdd(PIT_STOP_CHANGE_RIGHT_SIDE_TYRES, iRacingChoices);
                }

                validateAndAdd(HOW_MANY_INCIDENT_POINTS, iRacingChoices);
                validateAndAdd(WHATS_THE_INCIDENT_LIMIT, iRacingChoices);
                validateAndAdd(WHATS_MY_IRATING, iRacingChoices);
                validateAndAdd(WHATS_MY_LICENSE_CLASS, iRacingChoices);
                validateAndAdd(PIT_STOP_FUEL_TO_THE_END, iRacingChoices);
                validateAndAdd(WHATS_THE_SOF, iRacingChoices);

                GrammarBuilderWrapper iRacingGrammarBuilder = SREWrapperFactory.createNewGrammarBuilderWrapper(iRacingChoices);
                iRacingGrammarBuilder.SetCulture(cultureInfo);
                GrammarWrapper iRacingGrammar = SREWrapperFactory.createNewGrammarWrapper(iRacingGrammarBuilder);
                iracingPitstopGrammarList.Add(iRacingGrammar);
                sreWrapper.LoadGrammar(iRacingGrammar);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to add iRacing pit stop commands to speech recognition engine - " + e.Message);
            }
        }
        public static Boolean ResultContains(String result, String[] alternatives, Boolean logMatch = true)
        {
            result = result.ToLower();
            foreach (String alternative in alternatives)
            {
                if (result == alternative.ToLower())
                {
                    if (logMatch)
                    {
                        Console.WriteLine("matching entire response " + result);
                    }
                    return true;
                }
            }
            // no result with == so try contains
            foreach (String alternative in alternatives)
            {
                String alternativeLower = alternative.ToLower();
                if (result.Contains(alternativeLower))
                {
                    if (logMatch)
                    {
                        Console.WriteLine("matching partial response " + alternativeLower);
                    }
                    return true;
                }
            }
            return false;
        }

        private Boolean switchFromRegularToTriggerRecogniser()
        {
            if (!initialised)
            {
                return false;
            }

            int attempts = 0;
            Boolean success = false;
            while (!success && attempts < 3)
            {
                attempts++;
                try
                {
                    // the cancel call takes some time to complete but returns immediately, so wait a bit before switching inputs
                    sreWrapper.RecognizeAsyncCancel();
                    Thread.Sleep(5);
                    sreWrapper.SetInputToNull();
                    triggerSreWrapper.SetInputToDefaultAudioDevice();
                    triggerSreWrapper.RecognizeAsync();
                    waitingForSpeech = false;
                    success = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
            if (success)
            {
                if (attempts > 1)
                {
                    Console.WriteLine("Took " + attempts + " attempts to switch from regular to trigger SRE");
                }
            }
            else
            {
                Console.WriteLine("Failed to switch SRE after " + attempts + " attempts");
            }
            return success;
        }

        private Boolean switchFromTriggerToRegularRecogniser()
        {
            if (!initialised)
            {
                return false;
            }

            int attempts = 0;
            Boolean success = false;
            while (!success && attempts < 3)
            {
                attempts++;
                try
                {
                    // the cancel call takes some time to complete but returns immediately, so wait a bit before switching inputs
                    triggerSreWrapper.RecognizeAsyncCancel();
                    Thread.Sleep(5);
                    triggerSreWrapper.SetInputToNull();
                    sreWrapper.SetInputToDefaultAudioDevice();
                    success = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
            if (success)
            {
                if (attempts > 1)
                {
                    Console.WriteLine("Took " + attempts + " attempts to switch from trigger to regular SRE");
                }
                recognitionStartedTime = DateTime.Now;
                recognizeAsync();
                // if we reject messages while we're talking to the chief, attempt to interrupt any sound currently playing
                if (PlaybackModerator.rejectMessagesWhenTalking)
                {
                    SoundCache.InterruptCurrentlyPlayingSound();
                }
                crewChief.audioPlayer.playStartListeningBeep();
            }
            else
            {
                Console.WriteLine("Failed to switch SRE after " + attempts + " attempts");
            }
            return success;
        }

        private void restartWaitTimeoutThread(int timeout)
        {
            triggerTimeoutWaitHandle.Set();
            ThreadManager.UnregisterTemporaryThread(restartWaitTimeoutThreadReference);
            restartWaitTimeoutThreadReference = new Thread(() =>
            {
                triggerTimeoutWaitHandle.Reset();
                Thread.CurrentThread.IsBackground = true;
                Boolean signalled = triggerTimeoutWaitHandle.WaitOne(timeout);
                if (signalled)
                {
                    // thread was stopped so we got some speech or asked the thread to shut down
                    // (no point in logging anything here)
                }
                else
                {
                    // timeout waiting
                    if (waitingForSpeech)
                    {
                        // no result
                        Console.WriteLine("Gave up waiting for voice command, now waiting for trigger word " + keyWord);
                        switchFromRegularToTriggerRecogniser();
                    }
                }
            });
            restartWaitTimeoutThreadReference.Name = "SpeachRecognizer.restartWaitTimeoutThreadReference";
            ThreadManager.RegisterTemporaryThread(restartWaitTimeoutThreadReference);
            restartWaitTimeoutThreadReference.Start();
        }

        void trigger_SpeechRecognizedSystem(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            trigger_SpeechRecognized(sender, e);
        }

        void trigger_SpeechRecognizedMicrosoft(object sender, Microsoft.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            trigger_SpeechRecognized(sender, e);
        }

        void trigger_SpeechRecognized(object sender, object e)
        {
            if (!initialised)
            {
                return;
            }
            float recognitionConfidence = SREWrapperFactory.GetCallbackConfidence(e);
            if (recognitionConfidence > minimum_trigger_voice_recognition_confidence)
            {
                Console.WriteLine("Heard keyword " + keyWord + ", waiting for command confidence " + recognitionConfidence);
                switchFromTriggerToRegularRecogniser();
                restartWaitTimeoutThread(trigger_word_listen_timeout);
            }
            else
            {
                Console.WriteLine("keyword detected but confidence (" + recognitionConfidence + ") too low");
            }
        }

        void sre_SpeechRecognizedMicrosoft(object sender, Microsoft.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            sre_SpeechRecognized(sender, e);
        }

        void sre_SpeechRecognizedSystem(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            sre_SpeechRecognized(sender, e);
        }

        void sre_SpeechRecognized(object sender, object e)
        {
            if (!initialised)
            {
                return;
            }

            // cancel the thread that's waiting for a speech recognised timeout:
            triggerTimeoutWaitHandle.Set();
            SpeechRecogniser.waitingForSpeech = false;
            SpeechRecogniser.gotRecognitionResult = true;
            Boolean youWot = false;
            String recognisedText = SREWrapperFactory.GetCallbackText(e);
            String[] recognisedWords = SREWrapperFactory.GetCallbackWordsList(e);
            float recognitionConfidence = SREWrapperFactory.GetCallbackConfidence(e);
            object recognitionGrammar = SREWrapperFactory.GetCallbackGrammar(e);
            Console.WriteLine("Recognised : " + recognisedText + "  Confidence = " + recognitionConfidence.ToString("0.000") + "  Time Elapsed (ms) = " + (DateTime.Now - SpeechRecogniser.recognitionStartedTime).TotalMilliseconds);
            try
            {
                // special case when we're waiting for a message after a heavy crash:
                if (DamageReporting.waitingForDriverIsOKResponse)
                {
                    DamageReporting damageReportingEvent = (DamageReporting)CrewChief.getEvent("DamageReporting");
                    if (recognitionConfidence > minimum_voice_recognition_confidence && ResultContains(recognisedText, I_AM_OK, false))
                    {
                        damageReportingEvent.cancelWaitingForDriverIsOK(DamageReporting.DriverOKResponseType.CLEARLY_OK);
                    }
                    else
                    {
                        damageReportingEvent.cancelWaitingForDriverIsOK(DamageReporting.DriverOKResponseType.NOT_UNDERSTOOD);
                    }
                }
                else
                {
                    if (useFreeDictationForChatMessages && this.chatDictationGrammar != null && recognitionGrammar == this.chatDictationGrammar.GetInternalGrammar())
                    {
                        Console.WriteLine("chat recognised: " + recognisedText);
                        if (recognisedText.StartsWith(chatContextStart))
                        {
                            string chatText = recognisedText.TrimStart(chatContextStart.ToCharArray()).Trim();
                            getStartChatMacro().execute("", true, false);
                            Console.WriteLine("Sending chat text \"" + chatText + "\"");
                            for (int charIndex = 0; charIndex < chatText.Length; charIndex++)
                            {
                                KeyPresser.KeyCode keyCode;
                                Boolean forcedUpperCase;
                                KeyPresser.parseKeycode(chatText[charIndex].ToString(), true, out keyCode, out forcedUpperCase);
                                // Console.WriteLine("key code = " + keyCode);
                                KeyPresser.SendScanCodeKeyPress(keyCode, forcedUpperCase, 20);
                                Thread.Sleep(20);
                            }
                            getEndChatMacro().execute("", true, false);
                        }
                        else
                        {
                            Console.WriteLine("Chat message doesn't appear to start with context " + chatContextStart + " so will not be executed");
                            crewChief.youWot(true);
                            youWot = true;
                        }
                    }
                    else if (GrammarWrapperListContains(opponentGrammarList, recognitionGrammar))
                    {
                        if (recognitionConfidence > minimum_name_voice_recognition_confidence)
                        {
                            this.lastRecognisedText = recognisedText;
                            CrewChief.getEvent("Opponents").respond(recognisedText);
                        }
                        else
                        {
                            crewChief.youWot(true);
                            youWot = true;
                        }
                    }
                    else if (recognitionConfidence > minimum_voice_recognition_confidence)
                    {
                        if (macroGrammar != null && macroGrammar.GetInternalGrammar() == recognitionGrammar && macroLookup.ContainsKey(recognisedText))
                        {
                            this.lastRecognisedText = recognisedText;
                            macroLookup[recognisedText].execute(recognisedText, false, true);
                        }
                        else if (GrammarWrapperListContains(iracingPitstopGrammarList, recognitionGrammar))
                        {
                            this.lastRecognisedText = recognisedText;
                            CrewChief.getEvent("IRacingBroadcastMessageEvent").respond(recognisedText);
                        }
                        else if (GrammarWrapperListContains(r3ePitstopGrammarList, recognitionGrammar))
                        {
                            this.lastRecognisedText = recognisedText;
                            R3EPitMenuManager.processVoiceCommand(recognisedText, this.crewChief.audioPlayer);
                        }
                        else if (ResultContains(recognisedText, REPEAT_LAST_MESSAGE, false))
                        {
                            crewChief.audioPlayer.repeatLastMessage();
                        }
                        else if (ResultContains(recognisedText, MORE_INFO, false) && this.lastRecognisedText != null && !use_verbose_responses)
                        {
                            AbstractEvent abstractEvent = getEventForSpeech(this.lastRecognisedText);
                            if (abstractEvent != null)
                            {
                                abstractEvent.respondMoreInformation(this.lastRecognisedText, true);
                            }
                        }
                        else
                        {
                            this.lastRecognisedText = recognisedText;
                            AbstractEvent abstractEvent = getEventForSpeech(recognisedText);
                            if (abstractEvent != null)
                            {
                                abstractEvent.respond(recognisedText);

                                if (use_verbose_responses)
                                {
                                    // In verbose mode, always respond with more info.
                                    abstractEvent.respondMoreInformation(this.lastRecognisedText, false);
                                }
                            }
                        }
                        /*if (buttonAssignments.Count > 0)
                        {
                            foreach (var buttonAssignment in buttonAssignments)
                            {
                                if(ResultContains(e.Result.Text, buttonAssignment.action, false))
                                {
                                    this.lastRecognisedText = e.Result.Text;
                                    CrewChief.getEvent(buttonAssignment.eventName).respond(e.Result.Text);
                                }
                            }
                        }*/
                    }
                    else
                    {
                        crewChief.youWot(true);
                        youWot = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Unable to respond - error message: " + exception.Message + " stack " + exception.StackTrace);
            }

            // 'stop' the recogniser if we're ALWAYS_ON (because we restart it below) or TOGGLE
            // (because the user might have forgotten to press the button to close the channel).
            // For HOLD mode, let the recogniser continue listening and executing commands (invoking this 
            // callback again from another thread) until the button is released, which will call
            // RecogniseAsyncCancel
            if (voiceOptionEnum == MainWindow.VoiceOptionEnum.TOGGLE)
            {
                sreWrapper.RecognizeAsyncStop();
                Thread.Sleep(500);
                Console.WriteLine("Stopping speech recognition");
            }
            else if (voiceOptionEnum == MainWindow.VoiceOptionEnum.ALWAYS_ON)
            {
                if (!useNAudio)
                {
                    sreWrapper.RecognizeAsyncStop();
                    Thread.Sleep(500);
                    Console.WriteLine("Restarting speech recognition");
                    recognizeAsync();
                    waitingForSpeech = true;
                }
                else
                {
                    waitingForSpeech = true;
                }
            }
            else if (voiceOptionEnum == MainWindow.VoiceOptionEnum.TRIGGER_WORD)
            {
                if (!youWot)
                {
                    Console.WriteLine("Waiting for trigger word " + keyWord);
                    switchFromRegularToTriggerRecogniser();
                }
                else
                {
                    // wait a little longer here as the "I didn't catch that" takes a second or two to say
                    restartWaitTimeoutThread(trigger_word_listen_timeout + 2000);
                    waitingForSpeech = true;
                }
            }
            else
            {
                // in hold-button mode, we're now waiting-for-speech until we get another result or the button is released
                if (SpeechRecogniser.keepRecognisingInHoldMode)
                {
                    Console.WriteLine("Waiting for more speech");
                    waitingForSpeech = true;
                }
            }
        }

        public void stopTriggerRecogniser()
        {
            if (!initialised)
            {
                return;
            }

            if (triggerSreWrapper != null)
            {
                triggerSreWrapper.RecognizeAsyncCancel();
            }
        }

        public void startContinuousListening()
        {
            if (!initialised)
            {
                return;
            }

            if (voiceOptionEnum == MainWindow.VoiceOptionEnum.TRIGGER_WORD)
            {
                try
                {
                    triggerSreWrapper.UnloadAllGrammars();
                    GrammarBuilderWrapper gb = SREWrapperFactory.createNewGrammarBuilderWrapper();
                    ChoicesWrapper c = SREWrapperFactory.createNewChoicesWrapper();
                    c.Add(keyWord);
                    gb.SetCulture(cultureInfo);
                    gb.Append(c);
                    triggerSreWrapper.LoadGrammar(SREWrapperFactory.createNewGrammarWrapper(gb));
                    sreWrapper.SetInputToNull();
                    triggerSreWrapper.SetInputToDefaultAudioDevice();
                    triggerSreWrapper.RecognizeAsync();
                    Console.WriteLine("waiting for trigger word " + keyWord);
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
            else
            {
                recognizeAsync();
            }
        }

        public void recognizeAsync()
        {
            if (!initialised)
            {
                return;
            }

            Console.WriteLine("Opened channel - waiting for speech");
            SpeechRecogniser.waitingForSpeech = true;
            SpeechRecogniser.gotRecognitionResult = false;
            SpeechRecogniser.keepRecognisingInHoldMode = true;
            try
            {
                if (useNAudio)
                {
                    Console.WriteLine("Getting audio from nAudio input stream");
                    if (MainWindow.voiceOption == MainWindow.VoiceOptionEnum.HOLD)
                    {
                        waveIn.StartRecording();
                    }
                    else if (MainWindow.voiceOption == MainWindow.VoiceOptionEnum.ALWAYS_ON)
                    {
                        nAudioAlwaysOnkeepRecording = true;
                        Debug.Assert(nAudioAlwaysOnListenerThread == null, "nAudio AlwaysOn Listener Thread wasn't shut down correctly.");

                        // This thread is manually synchronized in recongizeAsyncCancel
                        nAudioAlwaysOnListenerThread = new Thread(() =>
                        {
                            waveIn.StartRecording();
                            while (nAudioAlwaysOnkeepRecording
                                && crewChief.running)  // Exit as soon as we begin shutting down.
                            {
                                if (!Utilities.InterruptedSleep(5000 /*totalWaitMillis*/, 1000 /*waitWindowMillis*/, () => nAudioAlwaysOnkeepRecording && crewChief.running /*keepWaitingPredicate*/))
                                {
                                    break;
                                }
                                sreWrapper.SetInputToAudioStream(buffer, nAudioWaveInSampleRate, nAudioWaveInSampleDepth, nAudioWaveInChannelCount); // otherwise input gets unset
                                try
                                {
                                    sreWrapper.RecognizeAsync(); // before this call
                                }
                                catch (Exception e)
                                {
                                    Utilities.ReportException(e, "Exception in SpeechRecognitionEngine.RecognizeAsync.", true /*needReport*/);
                                }
                            }
                            StopNAudioWaveIn();
                        });

                        nAudioAlwaysOnListenerThread.Name = "SpeechRecogniser.nAudioAlwaysOnListenerThread";
                        nAudioAlwaysOnListenerThread.Start();

                    }
                }
                else
                {
                    Console.WriteLine("Getting audio from default device");
                    try
                    {
                        sreWrapper.RecognizeAsync();
                    }
                    catch (Exception e)
                    {
                        Utilities.ReportException(e, "Exception in SpeechRecognitionEngine.RecognizeAsync.", false /*needReport*/);
                        throw e;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to start speech recognition " + e.Message);
            }
        }

        public void recognizeAsyncCancel(Boolean isShuttingDown = false)
        {
            if (!initialised)
            {
                return;
            }

            Console.WriteLine("Cancelling wait for speech");
            SpeechRecogniser.waitingForSpeech = false;
            if (useNAudio)
            {
                if (MainWindow.voiceOption == MainWindow.VoiceOptionEnum.HOLD)
                {
                    SpeechRecogniser.keepRecognisingInHoldMode = false;
                    StopNAudioWaveIn();
                    if (!isShuttingDown)
                    {
                        sreWrapper.SetInputToAudioStream(buffer, nAudioWaveInSampleRate, nAudioWaveInSampleDepth, nAudioWaveInChannelCount); // otherwise input gets unset
                        try
                        {
                            sreWrapper.RecognizeAsync(); // before this call
                        }
                        catch (Exception e)
                        {
                            Utilities.ReportException(e, "Exception in SpeechRecognitionEngine.RecognizeAsync.", true /*needReport*/);
                        }
                    }
                }
                else if (MainWindow.voiceOption == MainWindow.VoiceOptionEnum.ALWAYS_ON)
                {
                    nAudioAlwaysOnkeepRecording = false;
                    sreWrapper.RecognizeAsyncCancel();

                    // Wait for nAudioAlwaysOnListenerThread thread to exit.
                    if (nAudioAlwaysOnListenerThread != null)
                    {
                        if (nAudioAlwaysOnListenerThread.IsAlive)
                        {
                            Console.WriteLine("Waiting for nAudio Always On listener to stop...");
                            if (!nAudioAlwaysOnListenerThread.Join(5000))
                            {
                                var errMsg = "Warning: Timed out waiting for nAudio Always On listener to stop";
                                Console.WriteLine(errMsg);
                                Debug.WriteLine(errMsg);
                            }
                        }
                        nAudioAlwaysOnListenerThread = null;
                        Console.WriteLine("nAudio Always On listener stopped");
                    }
                }
            }
            else
            {
                SpeechRecogniser.keepRecognisingInHoldMode = false;
                sreWrapper.RecognizeAsyncCancel();
            }
        }

        private void StopNAudioWaveIn()
        {
            if (!initialised)
            {
                return;
            }

            int retries = 0;
            Boolean stopped = false;
            while (!stopped && retries < 3)
            {
                try
                {
                    waveIn.StopRecording();
                    stopped = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(50);
                    retries++;
                }
            }
        }

        public void changeInputDevice(int dev)
        {
            if (!initialised)
            {
                return;
            }

            waveIn.DeviceNumber = dev;
        }

        private void waveIn_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            if (!initialised)
            {
                return;
            }

            lock (buffer)
            {
                buffer.Write(e.Buffer, (int)buffer.Position, e.BytesRecorded);
            }
        }

        private AbstractEvent getEventForSpeech(String recognisedSpeech)
        {
            if (!initialised)
            {
                return null;
            }
            if (ResultContains(recognisedSpeech, DONT_SPOT, false))
            {
                crewChief.disableSpotter();
                return null;
            }
            else if (ResultContains(recognisedSpeech, SPOT, false))
            {
                crewChief.enableSpotter();
                return null;
            }
            else
            {
                return getEventForAction(recognisedSpeech);
            }
        }

        public static AbstractEvent getEventForAction(String recognisedSpeech)
        {
            if (ResultContains(recognisedSpeech, RADIO_CHECK, false) ||
                ResultContains(recognisedSpeech, KEEP_QUIET, false) ||
                ResultContains(recognisedSpeech, DONT_TELL_ME_THE_GAPS, false) ||
                ResultContains(recognisedSpeech, TELL_ME_THE_GAPS, false) ||
                ResultContains(recognisedSpeech, ENABLE_YELLOW_FLAG_MESSAGES, false) ||
                ResultContains(recognisedSpeech, DISABLE_YELLOW_FLAG_MESSAGES, false) ||
                ResultContains(recognisedSpeech, ENABLE_CUT_TRACK_WARNINGS, false) ||
                ResultContains(recognisedSpeech, DISABLE_CUT_TRACK_WARNINGS, false) ||
                ResultContains(recognisedSpeech, ENABLE_MANUAL_FORMATION_LAP, false) ||
                ResultContains(recognisedSpeech, DISABLE_MANUAL_FORMATION_LAP, false) ||
                ResultContains(recognisedSpeech, WHATS_THE_TIME, false) ||
                ResultContains(recognisedSpeech, TALK_TO_ME_ANYWHERE, false) ||
                ResultContains(recognisedSpeech, DONT_TALK_IN_THE_CORNERS, false) ||
                ResultContains(recognisedSpeech, KEEP_ME_INFORMED, false) ||
                ResultContains(recognisedSpeech, DAMAGE_REPORT, false) ||
                ResultContains(recognisedSpeech, CAR_STATUS, false) ||
                ResultContains(recognisedSpeech, STATUS, false) ||
                ResultContains(recognisedSpeech, SESSION_STATUS, false) ||
                ResultContains(recognisedSpeech, START_PACE_NOTES_PLAYBACK, false) ||
                ResultContains(recognisedSpeech, STOP_PACE_NOTES_PLAYBACK, false) ||
                ResultContains(recognisedSpeech, PLAY_CORNER_NAMES, false) ||
                ResultContains(recognisedSpeech, STOP_COMPLAINING, false) ||
                ControllerConfiguration.builtInActionMappings.ContainsValue(recognisedSpeech))
            {
                return CrewChief.getEvent("CommonActions");
            }
            else if (ResultContains(recognisedSpeech, HOWS_MY_AERO, false) ||
               ResultContains(recognisedSpeech, HOWS_MY_TRANSMISSION, false) ||
               ResultContains(recognisedSpeech, HOWS_MY_ENGINE, false) ||
               ResultContains(recognisedSpeech, HOWS_MY_SUSPENSION, false) ||
               ResultContains(recognisedSpeech, HOWS_MY_BRAKES, false))
            {
                return CrewChief.getEvent("DamageReporting");
            }

            else if (ResultContains(recognisedSpeech, WHATS_MY_FUEL_LEVEL, false)
                || ResultContains(recognisedSpeech, HOWS_MY_FUEL, false)
                || ResultContains(recognisedSpeech, WHATS_MY_FUEL_USAGE, false)
                || ResultContains(recognisedSpeech, CALCULATE_FUEL_FOR, false)
                || ResultContains(recognisedSpeech, HOW_MUCH_FUEL_TO_END_OF_RACE, false))
            {
                return CrewChief.getEvent("Fuel");
            }
            else if (
                ResultContains(recognisedSpeech, HOWS_MY_BATTERY, false))
            {
                return CrewChief.getEvent("Battery");
            }
            else if (ResultContains(recognisedSpeech, WHATS_MY_GAP_IN_FRONT, false) ||
                ResultContains(recognisedSpeech, WHATS_MY_GAP_BEHIND, false) ||
                ResultContains(recognisedSpeech, WHERE_AM_I_FASTER, false) ||
                ResultContains(recognisedSpeech, WHERE_AM_I_SLOWER, false))
            {
                return CrewChief.getEvent("Timings");
            }
            else if (ResultContains(recognisedSpeech, WHATS_MY_POSITION, false))
            {
                return CrewChief.getEvent("Position");
            }
            else if (ResultContains(recognisedSpeech, WHAT_WAS_MY_LAST_LAP_TIME, false) ||
                ResultContains(recognisedSpeech, WHATS_MY_BEST_LAP_TIME, false) ||
                ResultContains(recognisedSpeech, WHATS_THE_FASTEST_LAP_TIME, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_PACE, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_SELF_PACE, false) ||
                ResultContains(recognisedSpeech, WHAT_ARE_MY_SECTOR_TIMES, false) ||
                ResultContains(recognisedSpeech, WHATS_MY_LAST_SECTOR_TIME, false))
            {
                return CrewChief.getEvent("LapTimes");
            }
            else if (ResultContains(recognisedSpeech, WHAT_ARE_MY_TYRE_TEMPS, false) ||
                ResultContains(recognisedSpeech, HOW_ARE_MY_TYRE_TEMPS, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_TYRE_WEAR, false) ||
                ResultContains(recognisedSpeech, HOW_ARE_MY_BRAKE_TEMPS, false) ||
                ResultContains(recognisedSpeech, WHAT_ARE_MY_BRAKE_TEMPS, false) ||
                ResultContains(recognisedSpeech, WHAT_ARE_THE_RELATIVE_TYRE_PERFORMANCES, false) ||
                ResultContains(recognisedSpeech, HOW_LONG_WILL_THESE_TYRES_LAST, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_LEFT_FRONT_CAMBER, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_RIGHT_FRONT_CAMBER, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_LEFT_REAR_CAMBER, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_RIGHT_REAR_CAMBER, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_FRONT_CAMBER, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_REAR_CAMBER, false) ||
                ResultContains(recognisedSpeech, HOW_ARE_MY_TYRE_PRESSURES, false) ||
                ResultContains(recognisedSpeech, HOW_ARE_MY_FRONT_TYRE_PRESSURES, false) ||
                ResultContains(recognisedSpeech, HOW_ARE_MY_REAR_TYRE_PRESSURES, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_LEFT_FRONT_CAMBER_RIGHT_NOW, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_RIGHT_FRONT_CAMBER_RIGHT_NOW, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_LEFT_REAR_CAMBER_RIGHT_NOW, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_RIGHT_REAR_CAMBER_RIGHT_NOW, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_FRONT_CAMBER_RIGHT_NOW, false) ||
                ResultContains(recognisedSpeech, HOWS_MY_REAR_CAMBER_RIGHT_NOW, false) ||
                ResultContains(recognisedSpeech, HOW_ARE_MY_TYRE_PRESSURES_RIGHT_NOW, false) ||
                ResultContains(recognisedSpeech, HOW_ARE_MY_FRONT_TYRE_PRESSURES_RIGHT_NOW, false) ||
                ResultContains(recognisedSpeech, HOW_ARE_MY_REAR_TYRE_PRESSURES_RIGHT_NOW, false))
            {
                return CrewChief.getEvent("TyreMonitor");
            }
            else if (ResultContains(recognisedSpeech, HOW_LONGS_LEFT, false))
            {
                return CrewChief.getEvent("RaceTime");
            }
            else if (ResultContains(recognisedSpeech, DO_I_STILL_HAVE_A_PENALTY, false) ||
                ResultContains(recognisedSpeech, DO_I_HAVE_A_PENALTY, false) ||
                ResultContains(recognisedSpeech, HAVE_I_SERVED_MY_PENALTY, false))
            {
                return CrewChief.getEvent("Penalties");
            }
            else if (ResultContains(recognisedSpeech, DO_I_HAVE_A_MANDATORY_PIT_STOP, false) ||
                ResultContains(recognisedSpeech, IS_MY_PIT_BOX_OCCUPIED, false) ||
                ResultContains(recognisedSpeech, WHATS_PITLANE_SPEED_LIMIT, false) ||
                ResultContains(recognisedSpeech, WHAT_ARE_THE_PIT_ACTIONS))
            {
                return CrewChief.getEvent("PitStops");
            }
            else if (ResultContains(recognisedSpeech, HOW_ARE_MY_ENGINE_TEMPS, false))
            {
                return CrewChief.getEvent("EngineMonitor");
            }
            else if (ResultContains(recognisedSpeech, WHATS_THE_AIR_TEMP, false) ||
               ResultContains(recognisedSpeech, WHATS_THE_TRACK_TEMP, false))
            {
                return CrewChief.getEvent("ConditionsMonitor");
            }
            else if (ResultContains(recognisedSpeech, WHAT_TYRES_AM_I_ON, false) ||
                ResultContains(recognisedSpeech, WHOS_IN_FRONT_ON_TRACK, false) ||
                ResultContains(recognisedSpeech, WHOS_IN_FRONT_IN_THE_RACE, false) ||
                ResultContains(recognisedSpeech, WHOS_BEHIND_ON_TRACK, false) ||
                ResultContains(recognisedSpeech, WHOS_BEHIND_IN_THE_RACE, false) ||
                ResultContains(recognisedSpeech, WHOS_LEADING, false))
            {
                return CrewChief.getEvent("Opponents");
            }
            else if (ResultContains(recognisedSpeech, IS_CAR_AHEAD_MY_CLASS, false) ||
                ResultContains(recognisedSpeech, IS_CAR_BEHIND_MY_CLASS, false) ||
                ResultContains(recognisedSpeech, WHAT_CLASS_IS_CAR_AHEAD, false) ||
                ResultContains(recognisedSpeech, WHAT_CLASS_IS_CAR_BEHIND, false))
            {
                return CrewChief.getEvent("MulticlassWarnings");
            }
            else if (ResultContains(recognisedSpeech, PRACTICE_PIT_STOP, false) ||
                ResultContains(recognisedSpeech, PLAY_POST_PIT_POSITION_ESTIMATE, false))
            {
                return CrewChief.getEvent("Strategy");
            }
            else if (ResultContains(recognisedSpeech, PIT_STOP_TEAROFF, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_FAST_REPAIR, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CLEAR_ALL, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CLEAR_TYRES, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CLEAR_WIND_SCREEN, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CLEAR_FAST_REPAIR, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CLEAR_FUEL, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CHANGE_ALL_TYRES, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CHANGE_FRONT_LEFT_TYRE, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CHANGE_FRONT_RIGHT_TYRE, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CHANGE_REAR_LEFT_TYRE, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CHANGE_REAR_RIGHT_TYRE, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CHANGE_LEFT_SIDE_TYRES, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_CHANGE_RIGHT_SIDE_TYRES, false) ||
                ResultContains(recognisedSpeech, HOW_MANY_INCIDENT_POINTS, false) ||
                ResultContains(recognisedSpeech, WHATS_THE_INCIDENT_LIMIT, false) ||
                ResultContains(recognisedSpeech, WHATS_MY_IRATING, false) ||
                ResultContains(recognisedSpeech, WHATS_MY_LICENSE_CLASS, false) ||
                ResultContains(recognisedSpeech, PIT_STOP_FUEL_TO_THE_END, false) ||
                ResultContains(recognisedSpeech, WHATS_THE_SOF, false))
            {
                return CrewChief.getEvent("IRacingBroadcastMessageEvent");
            }
            else if (alarmClockVoiceRecognitionEnabled &&
                (ResultContains(recognisedSpeech, SET_ALARM_CLOCK, false) || ResultContains(recognisedSpeech, CLEAR_ALARM_CLOCK, false)))
            {
                return CrewChief.alarmClock;
            }
            return null;
        }

        private Boolean GrammarWrapperListContains(List<GrammarWrapper> grammarWrapperList, object grammar)
        {
            foreach (GrammarWrapper grammarWrapper in grammarWrapperList)
            {
                if (grammarWrapper.GetInternalGrammar() == grammar)
                {
                    return true;
                }
            }
            return false;
        }

        private static ExecutableCommandMacro getStartChatMacro()
        {
            if (SpeechRecogniser.startChatMacro == null)
            {
                MacroManager.macros.TryGetValue(SpeechRecogniser.startChatMacroName, out SpeechRecogniser.startChatMacro);
            }
            return SpeechRecogniser.startChatMacro;
        }

        private static ExecutableCommandMacro getEndChatMacro()
        {
            if (SpeechRecogniser.endChatMacro == null)
            {
                MacroManager.macros.TryGetValue(SpeechRecogniser.endChatMacroName, out SpeechRecogniser.endChatMacro);
            }
            return SpeechRecogniser.endChatMacro;
        }

        private LangCodes getLangCodes()
        {
            LangCodes langCodes = new LangCodes();
            String overrideCountry = null;
            if (localeCountryPropertySetting != null && localeCountryPropertySetting.Length == 2)
            {
                overrideCountry = localeCountryPropertySetting.ToUpper();
            }
            // for backwards compatibility
            Boolean useDefaultLocaleInsteadOfLanguage = sreConfigDefaultLocaleSetting != null && sreConfigDefaultLocaleSetting.Length > 0;

            Tuple<String, String> sreConfigLangAndCountry = parseLocalePropertyValue(useDefaultLocaleInsteadOfLanguage ? sreConfigDefaultLocaleSetting : sreConfigLanguageSetting);
            String sreConfigLang = sreConfigLangAndCountry.Item1;
            String sreConfigCountry = sreConfigLangAndCountry.Item2;

            langCodes.langToUse = sreConfigLang;
            langCodes.countryToUse = overrideCountry != null ? overrideCountry : sreConfigCountry;
            langCodes.langAndCountryToUse = langCodes.countryToUse != null ? langCodes.langToUse + "-" + langCodes.countryToUse : null;

            return langCodes;
        }

        class LangCodes
        {
            public string countryToUse;
            public string langToUse;
            public string langAndCountryToUse;
            public override string ToString()
            {
                return "countryToUse = \"" + countryToUse + "\", langToUse = \"" + langToUse + "\" langAndCountryToUse = \"" + langAndCountryToUse + "\"";
            }
        }
    }
}