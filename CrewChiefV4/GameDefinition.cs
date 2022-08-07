using CrewChiefV4.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrewChiefV4
{
    public enum GameEnum
    {
        RACE_ROOM, PCARS2, PCARS_64BIT, PCARS_32BIT, PCARS_NETWORK, PCARS2_NETWORK, RF1, ASSETTO_64BIT, ASSETTO_64BIT_RALLY, ASSETTO_32BIT,
        RF2_64BIT, IRACING, F1_2018, F1_2019, F1_2020, F1_2021, ACC, AMS2, AMS2_NETWORK, PCARS3, RBR, DIRT, DIRT_2, GTR2, UNKNOWN,
        NONE, /* this allows CC macros to run when an unsupported game is being played, it's selectable from the Games list */
        ANY   /* this allows CC macros to be defined that apply to all supported games, it's only selectable from the macro UI */
    }
    public class GameDefinition
    {
        // Also add any new GameDefinitions to getAllGameDefinitions()
        public static GameDefinition pCars64Bit = new GameDefinition(GameEnum.PCARS_64BIT, "pcars_64_bit", "pCARS64",
            "CrewChiefV4.PCars.PCarsSpotterv2", "pcars64_launch_exe", "pcars64_launch_params", "launch_pcars", false,
            alternativeProcessNames:new String[] { "pCARS", "pCARSGld", "pCARSQA", "pCARSAVX" },
            approxFilterNames: new String[] { "pcars" });
        public static GameDefinition AMS2 = new GameDefinition(GameEnum.AMS2, "ams2", "AMS2AVX",
            "CrewChiefV4.AMS2.AMS2Spotter", "ams2_launch_exe", "ams2_launch_params", "launch_ams2", false,
            alternativeProcessNames:new String[] { "AMS2", "AMS2AVX" },
            approxFilterNames: new String[] { "ams2", "ams_2", "ams 2", "ams-2" });
        public static GameDefinition pCars32Bit = new GameDefinition(GameEnum.PCARS_32BIT, "pcars_32_bit", "pCARS",
            "CrewChiefV4.PCars.PCarsSpotterv2", "pcars32_launch_exe", "pcars32_launch_params", "launch_pcars", false);
        public static GameDefinition pCars2 = new GameDefinition(GameEnum.PCARS2, "pcars_2", "pCARS2AVX",
            "CrewChiefV4.PCars2.PCars2Spotterv2", "pcars2_launch_exe", "pcars2_launch_params", "launch_pcars2", false,
            alternativeProcessNames:new String[] { "pCARS2", "pCARS2Gld" }, macroEditorName:"pCARS2",
            approxFilterNames: new String[] { "pcars2", "pcars_2", "pcars 2", "pcars-2" });

        // pCars3 - uses all the pCars2 classes
        public static GameDefinition pCars3 = new GameDefinition(GameEnum.PCARS3, "pcars_3", "pCARS3",
            "CrewChiefV4.PCars2.PCars2Spotterv2", "pcars3_launch_exe", "pcars3_launch_params", "launch_pcars3", false,
            alternativeProcessNames:new String[] { "pCARS3", "pCARS3Gld" },
            approxFilterNames: new String[] { "pcars3", "pcars_3", "pcars 3", "pcars-3" });
        public static GameDefinition raceRoom = new GameDefinition(GameEnum.RACE_ROOM, "race_room", "RRRE64", "CrewChiefV4.RaceRoom.R3ESpotterv2",
            "r3e_launch_exe", "r3e_launch_params", "launch_raceroom", false,
            alternativeProcessNames:new String[] { "RRRE" },
            approxFilterNames: new String[] { "room", "r3e", "rrre" });
        public static GameDefinition pCarsNetwork = new GameDefinition(GameEnum.PCARS_NETWORK, "pcars_udp", null, "CrewChiefV4.PCars.PCarsSpotterv2",
            null, null, null, false,
            approxFilterNames: new String[] { "pcars_network", "pcars network", "pcars-network" });
        public static GameDefinition pCars2Network = new GameDefinition(GameEnum.PCARS2_NETWORK, "pcars2_udp", null, "CrewChiefV4.PCars2.PCars2Spotterv2",
            null, null, null, false,
            approxFilterNames: new String[] { "pcars2_network", "pcars 2 network", "pcars-2-network" });
        public static GameDefinition ams2Network = new GameDefinition(GameEnum.AMS2_NETWORK, "ams2_udp", null, "CrewChiefV4.AMS2.AMS2Spotter",
            null, null, null, false);
        public static GameDefinition rFactor1 = new GameDefinition(GameEnum.RF1, "rfactor1", "rFactor", "CrewChiefV4.rFactor1.RF1Spotter",
            "rf1_launch_exe", "rf1_launch_params", "launch_rfactor1", true, "rFactor",
            approxFilterNames: new String[] { "rf", "factor1", "factor 1", "factor_1", "factor-1" });

        public static GameDefinition gameStockCar = new GameDefinition(GameEnum.RF1, "gamestockcar", "GSC", "CrewChiefV4.rFactor1.RF1Spotter",
            "gsc_launch_exe", "gsc_launch_params", "launch_gsc", true, commandLineName:"GSC");
        public static GameDefinition automobilista = new GameDefinition(GameEnum.RF1, "automobilista", "AMS", "CrewChiefV4.rFactor1.RF1Spotter",
            "ams_launch_exe", "ams_launch_params", "launch_ams", true, "Automobilista", commandLineName:"AMS",
            approxFilterNames: new String[] { "ams", "automobilista" });
        public static GameDefinition marcas = new GameDefinition(GameEnum.RF1, "marcas", "MARCAS", "CrewChiefV4.rFactor1.RF1Spotter",
            "marcas_launch_exe", "marcas_launch_params", "launch_marcas", true, commandLineName:"MARCAS");
        public static GameDefinition ftruck = new GameDefinition(GameEnum.RF1, "ftruck", "FTRUCK", "CrewChiefV4.rFactor1.RF1Spotter",
            "ftruck_launch_exe", "ftruck_launch_params", "launch_ftruck", true, commandLineName:"FTRUCK");
        public static GameDefinition arcaSimRacing = new GameDefinition(GameEnum.RF1, "asr", "ARCA", "CrewChiefV4.rFactor1.RF1Spotter",
            "asr_launch_exe", gameStartCommandOptionsProperty:null, gameStartEnabledProperty: "launch_asr", allowsUserCreatedCars: true, gameInstallDirectory: "arca", commandLineName: "ASR",
            approxFilterNames: new String[] { "asrx", "asr" });
        public static GameDefinition assetto64Bit = new GameDefinition(GameEnum.ASSETTO_64BIT, "assetto_64_bit", "acs", "CrewChiefV4.assetto.ACSSpotter",
            "acs_launch_exe", "acs_launch_params", "launch_acs", true, "assettocorsa",
            approxFilterNames: new String[] { "assetto", "corsa" });
        public static GameDefinition assetto64BitRallyMode = new GameDefinition(GameEnum.ASSETTO_64BIT_RALLY, "assetto_64_bit_rally_mode", "acs", null,
            "acs_launch_exe", "acs_launch_params", "launch_acs", true, "assettocorsa", racingType: CrewChief.RacingType.Rally,
            approxFilterNames: new String[] { "assetto", "corsa" });
        public static GameDefinition assetto32Bit = new GameDefinition(GameEnum.ASSETTO_32BIT, "assetto_32_bit", "acs_x86", "CrewChiefV4.assetto.ACSSpotter",
            "acs_launch_exe", "acs_launch_params", "launch_acs", true, "assettocorsa");
        public static GameDefinition rfactor2_64bit = new GameDefinition(GameEnum.RF2_64BIT, "rfactor2_64_bit", "rFactor2", "CrewChiefV4.rFactor2.RF2Spotter",
            "rf2_launch_exe", "rf2_launch_params", "launch_rfactor2", true, "rFactor 2", commandLineName:"RF2",
            approxFilterNames: new String[] { "rf2", "rf_2", "rf 2", "factor2", "factor 2", "factor_2", "factor-2" });
        public static GameDefinition iracing = new GameDefinition(GameEnum.IRACING, "iracing", "iRacingSim64DX11", "CrewChiefV4.iRacing.iRacingSpotter",
            "iracing_launch_exe", "iracing_launch_params", "launch_iracing", false,
            approxFilterNames: new String[] { "iracing", "i racing", "i_racing", "i-racing" });
        public static GameDefinition f1_2018 = new GameDefinition(GameEnum.F1_2018, "f1_2018", null, "CrewChiefV4.F1_2018.F12018Spotter",
            "f1_2018_launch_exe", "f1_2018_launch_params", "launch_f1_2018", false,
            approxFilterNames: new String[] { "2018" });
        public static GameDefinition acc = new GameDefinition(GameEnum.ACC, "acc", "AC2-Win64-Shipping", "CrewChiefV4.ACC.ACCSpotter",
            "acc_launch_exe", "acc_launch_params", "launch_acc", false,
            approxFilterNames: new String[] { "competizione", "acc" });
        public static GameDefinition f1_2019 = new GameDefinition(GameEnum.F1_2019, "f1_2019", null, "CrewChiefV4.F1_2019.F12019Spotter",
            "f1_2019_launch_exe", "f1_2019_launch_params", "launch_f1_2019", false,
            approxFilterNames: new String[] { "2019" });
        public static GameDefinition f1_2020 = new GameDefinition(GameEnum.F1_2020, "f1_2020", null, "CrewChiefV4.F1_2020.F12020Spotter",
            "f1_2020_launch_exe", "f1_2020_launch_params", "launch_f1_2020", false,
            approxFilterNames: new String[] { "2020" });
        public static GameDefinition f1_2021 = new GameDefinition(GameEnum.F1_2021, "f1_2021", null, "CrewChiefV4.F1_2021.F12021Spotter",
            "f1_2021_launch_exe", "f1_2021_launch_params", "launch_f1_2021", false,
            approxFilterNames: new String[] { "2021" });
        public static GameDefinition rbr = new GameDefinition(GameEnum.RBR, "rbr", "RichardBurnsRally_SSE", null /*spotterName*/,
            "rbr_launch_exe", null /*gameStartCommandOptionsProperty*/, "launch_rbr", true, "RBR", null, racingType:CrewChief.RacingType.Rally);
        public static GameDefinition gtr2 = new GameDefinition(GameEnum.GTR2, "gtr2", "GTR2", "CrewChiefV4.GTR2.GTR2Spotter",
            "gtr2_launch_exe", null /*gameStartCommandOptionsProperty*/, "launch_gtr2", true, "GTR2",
            approxFilterNames: new String[] { "gtr2", "gtr 2", "gtr_2", "gtr-2" });
        public static GameDefinition dirt = new GameDefinition(GameEnum.DIRT, "dirt", null, null /*spotterName*/,
            "dirt_launch_exe", "dirt2_launch_params" /*gameStartCommandOptionsProperty*/, "launch_dirt", false, "", null, racingType:CrewChief.RacingType.Rally,
            approxFilterNames: new String[] { "dirt" });
        public static GameDefinition dirt2 = new GameDefinition(GameEnum.DIRT_2, "dirt2", null, null /*spotterName*/,
            "dirt2_launch_exe", "dirt2_launch_params" /*gameStartCommandOptionsProperty*/, "launch_dirt2", false, "", null, racingType:CrewChief.RacingType.Rally,
            approxFilterNames: new String[] { "dirt_2" });

        public static GameDefinition any = new GameDefinition(GameEnum.ANY, "all_games", null, null, null, null, null, false, "", macroEditorName:"Generic (all games)");
        public static GameDefinition none = new GameDefinition(GameEnum.NONE, "unsupported_game", null, null, null, null, null, false, "", macroEditorName:"Unsupported games");

        private static string showOnlyTheseGames = UserSettings.GetUserSettings().getString("limit_available_games");

        private static List<GameDefinition> filterAvailableGames(List<GameDefinition> gameDefinitions)
        {
            if (showOnlyTheseGames != null && showOnlyTheseGames.Length > 0)
            {
                try
                {
                    string[] filters = showOnlyTheseGames.Split(',');
                    HashSet<GameDefinition> filtered = new HashSet<GameDefinition>();

                    foreach (string filterItem in filters)
                    {
                        Boolean matched = false;
                        String filter = filterItem.Trim();
                        if (filter.Length > 0)
                        {
                            foreach (GameDefinition gameDefinition in gameDefinitions)
                            {
                                if (filterItem.Length > 0 && (
                                    gameDefinition.friendlyName.Equals(filter) || gameDefinition.lookupName.Equals(filter) || gameDefinition.commandLineName.Equals(filter)))
                                {
                                    filtered.Add(gameDefinition);
                                    matched = true;
                                    break;
                                }
                            }
                            if (!matched)
                            {
                                // no match for this filter, see if we can do an approx match
                                string filterLower = filter.ToLower();
                                foreach (GameDefinition gameDefinition in gameDefinitions)
                                {
                                    if (filterItem.Length > 0 && gameDefinition.alternativeFilterNames != null
                                        && gameDefinition.alternativeFilterNames.Contains(filterLower))
                                    {
                                        filtered.Add(gameDefinition);
                                        matched = true;
                                        Log.Warning($"Limit available games filter '{filter}' should be '{gameDefinition.lookupName}'");
                                        break;
                                    }
                                }
                                if (!matched)
                                {
                                    Log.Error($"Limit available games filter term '{filter}' not recognised");
                                }
                            }
                        }
                    }
                    return filtered.ToList();
                }
                catch (Exception e) { Log.Exception(e); }
            }
            return gameDefinitions;
        }

        public static List<GameDefinition> getAllGameDefinitions()
        {
            List<GameDefinition> definitions = new List<GameDefinition>();
            definitions.Add(automobilista);
            definitions.Add(AMS2);
            definitions.Add(gameStockCar);
            definitions.Add(marcas);
            definitions.Add(ftruck);
            definitions.Add(arcaSimRacing);
            definitions.Add(pCars2);
            definitions.Add(pCars3);
            definitions.Add(pCars64Bit);
            definitions.Add(pCars32Bit);
            definitions.Add(raceRoom);
            definitions.Add(pCarsNetwork);

            // TODO: reinstate this when it actually works:
            // definitions.Add(pCars2Network);

            definitions.Add(rFactor1);
            definitions.Add(assetto64Bit);
            definitions.Add(assetto64BitRallyMode);
            definitions.Add(assetto32Bit);
            definitions.Add(rfactor2_64bit);
            definitions.Add(iracing);
            definitions.Add(acc);
            definitions.Add(f1_2018);
            definitions.Add(f1_2019);
            definitions.Add(f1_2020);
            definitions.Add(f1_2021);
            definitions.Add(rbr);
            definitions.Add(gtr2);
            definitions.Add(none);
            definitions.Add(dirt);
            definitions.Add(dirt2);
            return definitions;
        }
        public static List<GameDefinition> getAllAvailableGameDefinitions(Boolean includeAllSupportedGamesEntry)
        {
            List<GameDefinition> definitions = getAllGameDefinitions();
            if (includeAllSupportedGamesEntry) definitions.Add(any);
            return filterAvailableGames(definitions);
        }

        public static GameDefinition getGameDefinitionForFriendlyName(String friendlyName)
        {
            List<GameDefinition> definitions = getAllAvailableGameDefinitions(true);
            foreach (GameDefinition def in definitions)
            {
                if (def.friendlyName == friendlyName)
                {
                    return def;
                }
            }
            return null;
        }

        public static GameDefinition getGameDefinitionForCommandLineName(String commandLineName)
        {
            List<GameDefinition> definitions = getAllAvailableGameDefinitions(false);
            foreach (GameDefinition def in definitions)
            {
                if (def.commandLineName == commandLineName)
                {
                    return def;
                }
            }
            return null;
        }

        public static String[] getGameDefinitionFriendlyNames()
        {
            List<String> names = new List<String>();
            foreach (GameDefinition def in getAllAvailableGameDefinitions(false))
            {
                names.Add(def.friendlyName);
            }
            names.Sort();
            return names.ToArray();
        }

        public GameEnum gameEnum;
        public String friendlyName;
        public String macroEditorName;
        public readonly CrewChief.RacingType racingType = CrewChief.RacingType.Undefined;
        public String lookupName;
        public String processName;
        public String spotterName;
        public String gameStartCommandProperty;
        public String gameStartCommandOptionsProperty;
        public String gameStartEnabledProperty;
        public String gameInstallDirectory;
        public String[] alternativeProcessNames;
        public String[] alternativeFilterNames;
        public Boolean allowsUserCreatedCars;
        public readonly String commandLineName;

        public GameDefinition(GameEnum gameEnum, String lookupName, String processName,
            String spotterName, String gameStartCommandProperty, String gameStartCommandOptionsProperty,
            String gameStartEnabledProperty, Boolean allowsUserCreatedCars,
            String gameInstallDirectory = "", String[] alternativeProcessNames = null, String[] approxFilterNames = null,
            String macroEditorName = null, CrewChief.RacingType racingType = CrewChief.RacingType.Circuit, String commandLineName = null)
        {
            this.gameEnum = gameEnum;
            this.lookupName = lookupName;
            this.friendlyName = Configuration.getUIString(lookupName);
            this.processName = processName;
            this.spotterName = spotterName;
            this.gameStartCommandProperty = gameStartCommandProperty;
            this.gameStartCommandOptionsProperty = gameStartCommandOptionsProperty;
            this.gameStartEnabledProperty = gameStartEnabledProperty;
            this.alternativeProcessNames = alternativeProcessNames;
            this.gameInstallDirectory = gameInstallDirectory;
            this.allowsUserCreatedCars = allowsUserCreatedCars;
            this.macroEditorName = macroEditorName == null ? this.friendlyName : macroEditorName;
            this.racingType = racingType;
            this.commandLineName = commandLineName == null ? gameEnum.ToString() : commandLineName;
            this.alternativeFilterNames = approxFilterNames;
        }
        /// <summary>
        /// ctor to initialise gameDefinition
        /// </summary>
        public GameDefinition()
        {
            racingType = CrewChief.RacingType.Undefined;
        }
        public bool HasAnyProcessNameAssociated()
        {
            return processName != null
                || (alternativeProcessNames != null && alternativeProcessNames.Length > 0);
        }
    }
}
