using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CC_log_compare
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<string> testString = new List<string>{
                @"console_2022_05_29-12-52-18.txt",
                @"Profile: defaultSettings.json",
                @"VOICE_OPTION: ALWAYS_ON",
                @"",
                @"Non-default Properties:",
                @"gtr2_install_path: 'D:\games\SteamLibrary\steamapps\common\GTR 2 - FIA GT Racing Game'",
                @"interrupt_setting_listprop: 'IMPORTANT_MESSAGES'",
                @"iracing_launch_exe: 'C:\Progam Files (x86)\iRacing\ui\iRacingUI.exe'",
                @"log_type_verbose: True",
                @"max_complaints_per_session: 50",
                @"minimum_voice_recognition_confidence_system_sre: 0.2",
                @"pcars_enable_rain_prediction: True",
                @"pcars2_launch_exe: 'steam://launch/378860/othervr'",
                @"report_ambient_temp_changes_greater_than: 20",
                @"report_track_temp_changes_greater_than: 20",
                @"rf2_enable_auto_fuel_to_end_of_race: True",
                @"rf2_install_path: 'C:\Program Files (x86)\Steam\steamapps\common\rFactor 2'",
                @"speech_recognition_country: 'GB'",
                @"use_naudio: False",
                @"",
                @"12:46:45.464 : Loading screen opened",
                @"12:46:45.465 : Set rFactor 2 (64 bit) mode from previous launch",
                @"12:46:46.468 : Starting app.  Version: 4.16.2.2",
                @"12:46:48.544 : Using sound pack version 188, driver names version 140 and personalisations version 147"
            };
            var obj = new CC_log_compare(null);
            var result = obj.parseContents(testString);
        }
        [TestMethod]
        public void TestMethod2()
        {
            var obj = new CC_log_compare(@"d:\Tony\repos\CC_log_compare\CC_log_compare\console_2022_06_14-13-13-06.txt");
            var contents = obj.readFile();
            var result = obj.parseContents(contents);
            obj.writeFile(result);
        }
    }
}
