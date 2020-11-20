using CrewChiefV4.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrewChiefV4
{
    public enum GameEnum
    {
        RACE_ROOM, PCARS2, PCARS_64BIT, PCARS_32BIT, PCARS_NETWORK, PCARS2_NETWORK, RF1, ASSETTO_64BIT, ASSETTO_32BIT,
        RF2_64BIT, IRACING, F1_2018, F1_2019, F1_2020, ACC, AMS2, AMS2_NETWORK, PCARS3, RBR, DIRT, DIRT_2, GTR2, UNKNOWN,
        NONE, /* this allows CC macros to run when an unsupported game is being played, it's selectable from the Games list */
        ANY   /* this allows CC macros to be defined that apply to all supported games, it's only selectable from the macro UI */
    }
    public class GameDefinition
    {
        public static GameDefinition pCars64Bit = new GameDefinition(GameEnum.PCARS_64BIT, "pcars_64_bit", "pCARS64",
            "CrewChiefV4.PCars.PCarsSpotterv2", "pcars64_launch_exe", "pcars64_launch_params", "launch_pcars",
            new String[] { "pCARS2", "pCARS2Gld", "pCARS2QA", "pCARS2AVX" }, false);
        public static GameDefinition AMS2 = new GameDefinition(GameEnum.AMS2, "ams2", "AMS2AVX",
            "CrewChiefV4.AMS2.AMS2Spotter", "ams2_launch_exe", "ams2_launch_params", "launch_ams2", new String[] { "AMS2", "AMS2AVX" }, false);
        public static GameDefinition pCars32Bit = new GameDefinition(GameEnum.PCARS_32BIT, "pcars_32_bit", "pCARS",
            "CrewChiefV4.PCars.PCarsSpotterv2", "pcars32_launch_exe", "pcars32_launch_params", "launch_pcars", false);
        // pCars2 defines its own macro manager friendly game name, as these macros can be used by AMS2 and pCars2. Other games
        // just use the game friendly name
        public static GameDefinition pCars2 = new GameDefinition(GameEnum.PCARS2, "pcars_2", "pCARS2AVX",
            "CrewChiefV4.PCars2.PCars2Spotterv2", "pcars2_launch_exe", "pcars2_launch_params", "launch_pcars2", new String[] { "pCARS2", "pCARS2Gld" }, false, "", "pCARS2 / Automobilista 2");
        // pCars3 - uses all the pCars2 classes
        public static GameDefinition pCars3 = new GameDefinition(GameEnum.PCARS3, "pcars_3", "pCARS3",
            "CrewChiefV4.PCars2.PCars2Spotterv2", "pcars3_launch_exe", "pcars3_launch_params", "launch_pcars3", new String[] { "pCARS3", "pCARS3Gld" }, false);
        public static GameDefinition raceRoom = new GameDefinition(GameEnum.RACE_ROOM, "race_room", "RRRE64", "CrewChiefV4.RaceRoom.R3ESpotterv2",
            "r3e_launch_exe", "r3e_launch_params", "launch_raceroom", new String[] { "RRRE" }, false);
        public static GameDefinition pCarsNetwork = new GameDefinition(GameEnum.PCARS_NETWORK, "pcars_udp", null, "CrewChiefV4.PCars.PCarsSpotterv2",
            null, null, null, false);
        public static GameDefinition pCars2Network = new GameDefinition(GameEnum.PCARS2_NETWORK, "pcars2_udp", null, "CrewChiefV4.PCars2.PCars2Spotterv2",
            null, null, null, false);
        public static GameDefinition ams2Network = new GameDefinition(GameEnum.AMS2_NETWORK, "ams2_udp", null, "CrewChiefV4.AMS2.AMS2Spotter",
            null, null, null, false);
        public static GameDefinition rFactor1 = new GameDefinition(GameEnum.RF1, "rfactor1", "rFactor", "CrewChiefV4.rFactor1.RF1Spotter",
            "rf1_launch_exe", "rf1_launch_params", "launch_rfactor1", true, "rFactor");
        public static GameDefinition gameStockCar = new GameDefinition(GameEnum.RF1, "gamestockcar", "GSC", "CrewChiefV4.rFactor1.RF1Spotter",
            "gsc_launch_exe", "gsc_launch_params", "launch_gsc", true);
        public static GameDefinition automobilista = new GameDefinition(GameEnum.RF1, "automobilista", "AMS", "CrewChiefV4.rFactor1.RF1Spotter",
            "ams_launch_exe", "ams_launch_params", "launch_ams", true, "Automobilista");
        public static GameDefinition marcas = new GameDefinition(GameEnum.RF1, "marcas", "MARCAS", "CrewChiefV4.rFactor1.RF1Spotter",
            "marcas_launch_exe", "marcas_launch_params", "launch_marcas", true);
        public static GameDefinition ftruck = new GameDefinition(GameEnum.RF1, "ftruck", "FTRUCK", "CrewChiefV4.rFactor1.RF1Spotter",
            "ftruck_launch_exe", "ftruck_launch_params", "launch_ftruck", true);
        public static GameDefinition assetto64Bit = new GameDefinition(GameEnum.ASSETTO_64BIT, "assetto_64_bit", "acs", "CrewChiefV4.assetto.ACSSpotter",
            "acs_launch_exe", "acs_launch_params", "launch_acs", true, "assettocorsa");
        public static GameDefinition assetto32Bit = new GameDefinition(GameEnum.ASSETTO_32BIT, "assetto_32_bit", "acs_x86", "CrewChiefV4.assetto.ACSSpotter",
            "acs_launch_exe", "acs_launch_params", "launch_acs", true, "assettocorsa");
        public static GameDefinition rfactor2_64bit = new GameDefinition(GameEnum.RF2_64BIT, "rfactor2_64_bit", "rFactor2", "CrewChiefV4.rFactor2.RF2Spotter",
            "rf2_launch_exe", "rf2_launch_params", "launch_rfactor2", true, "rFactor 2");
        public static GameDefinition iracing = new GameDefinition(GameEnum.IRACING, "iracing", "iRacingSim64DX11", "CrewChiefV4.iRacing.iRacingSpotter",
            "iracing_launch_exe", "iracing_launch_params", "launch_iracing", false);
        public static GameDefinition f1_2018 = new GameDefinition(GameEnum.F1_2018, "f1_2018", null, "CrewChiefV4.F1_2018.F12018Spotter",
            "f1_2018_launch_exe", "f1_2018_launch_params", "launch_f1_2018", false);
        public static GameDefinition acc = new GameDefinition(GameEnum.ACC, "acc", "AC2-Win64-Shipping", "CrewChiefV4.ACC.ACCSpotter",
            "acc_launch_exe", "acc_launch_params", "launch_acc", false);
        public static GameDefinition f1_2019 = new GameDefinition(GameEnum.F1_2019, "f1_2019", null, "CrewChiefV4.F1_2019.F12019Spotter",
            "f1_2019_launch_exe", "f1_2019_launch_params", "launch_f1_2019", false);
        public static GameDefinition f1_2020 = new GameDefinition(GameEnum.F1_2020, "f1_2020", null, "CrewChiefV4.F1_2020.F12020Spotter",
            "f1_2020_launch_exe", "f1_2020_launch_params", "launch_f1_2020", false);
        public static GameDefinition rbr = new GameDefinition(GameEnum.RBR, "rbr", "RichardBurnsRally_SSE", null /*spotterName*/,
            "rbr_launch_exe", null /*gameStartCommandOptionsProperty*/, "launch_rbr", true, "RBR", null, CrewChief.RacingType.Rally);
        public static GameDefinition gtr2 = new GameDefinition(GameEnum.GTR2, "gtr2", "GTR2", "CrewChiefV4.GTR2.GTR2Spotter",
            "gtr2_launch_exe", "gtr2_launch_params", "launch_gtr2", true, "GTR2");
        public static GameDefinition dirt = new GameDefinition(GameEnum.DIRT, "dirt", null, null /*spotterName*/,
            "dirt_launch_exe", "dirt2_launch_params" /*gameStartCommandOptionsProperty*/, "launch_dirt", false, "", null, CrewChief.RacingType.Rally);
        public static GameDefinition dirt2 = new GameDefinition(GameEnum.DIRT_2, "dirt2", null, null /*spotterName*/,
            "dirt2_launch_exe", "dirt2_launch_params" /*gameStartCommandOptionsProperty*/, "launch_dirt2", false, "", null, CrewChief.RacingType.Rally);

        public static GameDefinition any = new GameDefinition(GameEnum.ANY, "all_games", null, null, null, null, null, false, "", "Generic (all games)");
        public static GameDefinition none = new GameDefinition(GameEnum.NONE, "unsupported_game", null, null, null, null, null, false, "", "Unsupported games");

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
                                    gameDefinition.friendlyName.Equals(filter) || gameDefinition.lookupName.Equals(filter) || gameDefinition.gameEnum.ToString().Equals(filter)))
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
                                if (filterLower.Contains("pcars2") || filterLower.Contains("pcars_2") || filterLower.Contains("pcars 2") || filterLower.Contains("pcars-2"))
                                {
                                    filtered.Add(GameDefinition.pCars2);
                                }
                                if (filterLower.Contains("ams2") || filterLower.Contains("ams_2") || filterLower.Contains("ams 2") || filterLower.Contains("ams-2"))
                                {
                                    filtered.Add(GameDefinition.AMS2);
                                }
                                else if (filterLower.Contains("pcars_network") || filterLower.Contains("pcars network") || filterLower.Contains("pcars-network"))
                                {
                                    filtered.Add(GameDefinition.pCarsNetwork);
                                }
                                else if (filterLower.Contains("pcars"))
                                {
                                    filtered.Add(GameDefinition.pCars64Bit);
                                }
                                else if (filterLower.Contains("competizione") || filterLower.Contains("acc"))
                                {
                                    filtered.Add(GameDefinition.acc);
                                }
                                else if (filterLower.Contains("corsa") || filterLower.Contains("assetto"))
                                {
                                    filtered.Add(GameDefinition.assetto64Bit);
                                }
                                else if (filterLower.Contains("room") || filterLower.Contains("r3e") || filterLower.Contains("rrre"))
                                {
                                    filtered.Add(GameDefinition.raceRoom);
                                }
                                else if (filterLower.Contains("iracing") || filterLower.Contains("i racing") || filterLower.Contains("i_racing") || filterLower.Contains("i-racing"))
                                {
                                    filtered.Add(GameDefinition.iracing);
                                }
                                else if (filterLower.Contains("2018"))
                                {
                                    filtered.Add(GameDefinition.f1_2018);
                                }
                                else if (filterLower.Contains("2019"))
                                {
                                    filtered.Add(GameDefinition.f1_2019);
                                }
                                else if (filterLower.Contains("2020"))
                                {
                                    filtered.Add(GameDefinition.f1_2020);
                                }
                                else if (filterLower.Contains("rf2") || filterLower.Contains("rf_2") || filterLower.Contains("rf 2") || filterLower.Contains("rf-2") || filterLower.Contains("factor2") || filterLower.Contains("factor 2") || filterLower.Contains("factor_2") || filterLower.Contains("factor-2"))
                                {
                                    filtered.Add(GameDefinition.rfactor2_64bit);
                                }
                                else if (filterLower.Contains("rf") || filterLower.Contains("factor1") || filterLower.Contains("factor 1") || filterLower.Contains("factor_1") || filterLower.Contains("factor-1"))
                                {
                                    filtered.Add(GameDefinition.rFactor1);
                                }
                                else if (filterLower.Contains("ams") || filterLower.Contains("automobilista"))
                                {
                                    filtered.Add(GameDefinition.automobilista);
                                }
                                else if (filterLower.Contains("rbr"))
                                {
                                    filtered.Add(GameDefinition.rbr);
                                }
                                else if (filterLower.Contains("gtr2"))
                                {
                                    filtered.Add(GameDefinition.gtr2);
                                }
                                else if (filterLower.Contains("dirt"))
                                {
                                    filtered.Add(GameDefinition.dirt);
                                }
                                else if (filterLower.Contains("dirt_2"))
                                {
                                    filtered.Add(GameDefinition.dirt2);
                                }
                                else
                                {
                                    Console.WriteLine("Game filter term \"" + filter + "\" not recognised");
                                }
                            }
                        }
                    }
                    return filtered.ToList();
                }
                catch
                { }
            }
            return gameDefinitions;
        }

        public static List<GameDefinition> getAllGameDefinitions(Boolean includeAllSupportedGamesEntry)
        {
            List<GameDefinition> definitions = new List<GameDefinition>();
            definitions.Add(automobilista); definitions.Add(AMS2);
            definitions.Add(gameStockCar); definitions.Add(marcas); definitions.Add(ftruck);
            definitions.Add(pCars2); definitions.Add(pCars3); definitions.Add(pCars64Bit); definitions.Add(pCars32Bit);
            definitions.Add(raceRoom); definitions.Add(pCarsNetwork); 
            
            // TODO: reinstate this when it actually works:
            // definitions.Add(pCars2Network); 
            
            definitions.Add(rFactor1);
            definitions.Add(assetto64Bit); definitions.Add(assetto32Bit); definitions.Add(rfactor2_64bit);
            definitions.Add(iracing);
            definitions.Add(f1_2018);
            definitions.Add(acc);
            definitions.Add(f1_2019);
            definitions.Add(f1_2020);
            definitions.Add(rbr);
            definitions.Add(gtr2);
            definitions.Add(none);
            definitions.Add(dirt);
            definitions.Add(dirt2);
            if (includeAllSupportedGamesEntry) definitions.Add(any);
            return filterAvailableGames(definitions);
        }

        public static GameDefinition getGameDefinitionForFriendlyName(String friendlyName)
        {
            List<GameDefinition> definitions = getAllGameDefinitions(true);
            foreach (GameDefinition def in definitions)
            {
                if (def.friendlyName == friendlyName)
                {
                    return def;
                }
            }
            return null;
        }

        public static GameDefinition getGameDefinitionForEnumName(String enumName)
        {
            List<GameDefinition> definitions = getAllGameDefinitions(false);
            foreach (GameDefinition def in definitions)
            {
                if (def.gameEnum.ToString() == enumName)
                {
                    return def;
                }
            }
            return null;
        }

        public static String[] getGameDefinitionFriendlyNames()
        {
            List<String> names = new List<String>();
            foreach (GameDefinition def in getAllGameDefinitions(false))
            {
                names.Add(def.friendlyName);
            }
            names.Sort();
            return names.ToArray();
        }

        public GameEnum gameEnum;
        public String friendlyName;
        public String macroEditorName;
        public readonly CrewChief.RacingType racingType;
        public String lookupName;
        public String processName;
        public String spotterName;
        public String gameStartCommandProperty;
        public String gameStartCommandOptionsProperty;
        public String gameStartEnabledProperty;
        public String gameInstallDirectory;
        public String[] alternativeProcessNames;
        public Boolean allowsUserCreatedCars;

        public GameDefinition(GameEnum gameEnum, String lookupName, String processName,
            String spotterName, String gameStartCommandProperty, String gameStartCommandOptionsProperty, String gameStartEnabledProperty, Boolean allowsUserCreatedCars,
            String gameInstallDirectory = "", String macroEditorName = null, CrewChief.RacingType racingType = CrewChief.RacingType.Circuit)
        {
            this.gameEnum = gameEnum;
            this.lookupName = lookupName;
            this.friendlyName = Configuration.getUIString(lookupName);
            this.processName = processName;
            this.spotterName = spotterName;
            this.gameStartCommandProperty = gameStartCommandProperty;
            this.gameStartCommandOptionsProperty = gameStartCommandOptionsProperty;
            this.gameStartEnabledProperty = gameStartEnabledProperty;
            this.gameInstallDirectory = gameInstallDirectory;
            this.allowsUserCreatedCars = allowsUserCreatedCars;
            this.macroEditorName = macroEditorName == null ? this.friendlyName : macroEditorName;
            this.racingType = racingType;
        }

        public GameDefinition(GameEnum gameEnum, String lookupName, String processName,
            String spotterName, String gameStartCommandProperty, String gameStartCommandOptionsProperty, String gameStartEnabledProperty, String[] alternativeProcessNames,
            Boolean allowsUserCreatedCars, String gameInstallDirectory = "", String macroEditorName = null, CrewChief.RacingType racingType = CrewChief.RacingType.Circuit)
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
        }

        public bool HasAnyProcessNameAssociated()
        {
            return processName != null
                || (alternativeProcessNames != null && alternativeProcessNames.Length > 0);
        }
    }
}
