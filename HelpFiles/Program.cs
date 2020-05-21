using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Create Help HTML files from
/// * the menu bar menu.ht
/// * the HTML pages from text (by appending <br> to each line)
/// * the wrap up menu.ml
/// * Output the results into /public from where they will be published
///   in a GitLab page as well as being used by the Crew Chief "Help"
/// </summary>
namespace HelpFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] pageNames, gameNames;
            pageNames = new string []{
                "index",
                "GettingStarted_General",
                "GettingStarted_nAudio",
                //"GettingStarted_GameSpecific",
                "Speech_DriverNames",
                "Speech_Swearing",
                "Speech_TextToSpeech",
                "Speech_PitExitPositionPrediction",
                "Speech_PaceNotes",
                "VoiceRecognition_InstallationTraining",
                "VoiceRecognition_VoiceCommandsAll",
                "VoiceRecognition_VoiceCommandsGrouped",
                "VoiceRecognition_VoiceCommandsCheatSheet",
                "VoiceRecognition_FreeDictationChatMessages",
                "VoiceRecognition_CommandMacros",
                "Overlays_InGame",
                "Overlays_VR",
                //"GameSpecific_ForEachGame",
                "GameSpecific_CommandLineSwitches",
                "Properties_Properties",
                "Properties_Profiles",
                "About_blurb",
                "About_Updating",
                "About_Contact",
                "About_KnownIssues",
                "About_Customising_TrackLandmarks",
                "About_Customising_VoicePacks",
                "About_Customising_NameRequests",
                "About_Credits",
                "About_Donations",
                "About_ChangeLog",
                };
            gameNames = new string[]{
                "AssettoCorsa",
                "AssettoCorsaCompetizione",
                "iRacing",
                "F1_2018",
                "ProjectCars",
                "ProjectCars2",
                "RaceRoomRacingExperience",
                "rFactor",
                "rFactor2"
            };

            foreach (string page in pageNames)
            {
                writeOnePage(page);
            }

            foreach (string page in gameNames)
            {
                writeOnePage($"GettingStarted_GameSpecific_{page}");
                writeOnePage($"GameSpecific_ForEachGame_{page}");
            }

            // Finally copy the css and the images
            foreach (string file in new string[] { "styles.css", "CrewChief.png", "VoiceRecognition_InstallationTraining.png" })
            {
                System.IO.File.Copy($"..\\..\\{file}", $"..\\..\\..\\public\\{file}", true);
            }
        }
        static void writeOnePage(string pageName)
        {
            List<string> lines = System.IO.File.ReadAllLines("..\\..\\menu.ht").ToList();
            List<string> text = System.IO.File.ReadAllLines($"..\\..\\{pageName}.txt").ToList();
            List<string> wrapup = System.IO.File.ReadAllLines("..\\..\\menu.ml").ToList();

            // Add the text to the menu boilerplate
            foreach (string line in text)
            {
                if (pageName.StartsWith("About_ChangeLog") && line.StartsWith("Version"))
                    lines.Add($"<h4>{line}</h4>");  // Version 4.11.1.2 -> <h4>Version 4.11.1.2</h4>
                else if (line.StartsWith("<"))
                    lines.Add($"{line}");   // No <br> on HTML-tagged lines
                else
                    lines.Add($"{line}<br>");
            }

            // Add the wrap up
            foreach (string line in wrapup)
            {
                lines.Add(line);
            }

            // Write the result
            System.IO.File.WriteAllLines($"..\\..\\..\\public\\{pageName}.html", lines);
            Console.WriteLine($"{pageName}.html written");
        }
    }
}
