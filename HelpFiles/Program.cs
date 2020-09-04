//#define TextHelpFiles
//#define FirstHTMLfiles

using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack; // https://html-agility-pack.net/

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
                "About_Updating",
                "About_Contact",
                "About_KnownIssues",
                "About_Customising_TrackLandmarks",
                "About_Customising_VoicePacks",
                "About_Customising_NameRequests",
                "About_Customising_CarClasses",
                "About_Credits",
                "About_Donations",
                "About_ChangeLog",
                "About_Licenses"
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
                "rFactor2",
                "RichardBurnsRally"
            };

            writeOnePage("index");

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
            foreach (string file in new string[] { "styles.css",
                "CrewChief.png",
                "VoiceRecognition_InstallationTraining.png",
                "engineer_edited.ico",
                "engineer_edited_transparent.png"
            })
            {
                System.IO.File.Copy($"..\\..\\{file}", $"..\\..\\..\\public\\{file}", true);
            }
        }

#if TextHelpFiles
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
                else // replace leading spaces with &nbsp; then copy the rest plus <br>
                {   // (Neater using regex substitution but tricky to work out)
                    string _newLine = String.Empty;
                    for (var i = 0; i < line.Length; i++)
                    {
                        if (line[i] == ' ')
                        {
                            _newLine += "&nbsp;";
                        }
                        else
                        {
                            _newLine += line.Substring(i);
                            break;
                        }
                    }
                    lines.Add($"{_newLine}<br>");
                }
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
#endif

        /// <summary>
        /// Insert the content in the HTML file between the menu and
        /// the footer and save the result in \public
        /// The content section is marked <div> </div>
        /// </summary>
        /// <param name="pageName"></param>
        static void writeOnePage(string pageName)
        {
            var templateDoc = new HtmlDocument();
            templateDoc.Load("..\\..\\menu.html");
            var node = templateDoc.DocumentNode.SelectSingleNode("//div");
            var nodes = templateDoc.DocumentNode.SelectNodes("//div");
            var oldChild = nodes[1];

            var contentDoc = new HtmlDocument();
            contentDoc.Load($"..\\..\\{pageName}.html");
            var insert = contentDoc.DocumentNode.SelectSingleNode("//div").InnerHtml;
            oldChild.InnerHtml = insert;
            templateDoc.Save($"..\\..\\..\\public\\{pageName}.html");
        }

#if FirstHTMLfiles
        /// <summary>
        /// ONLY USED ONCE TO CREATE *.HTML
        /// Create a replacement for the .txt file by inserting the HTML
        /// text section (<div>) from the original help file into a blank HTML
        /// </summary>
        /// <param name="pageName"></param>
        static void writeOnePage(string pageName)
        {
            var templateDoc = new HtmlDocument();
            templateDoc.Load("..\\..\\blank.html");
            var node = templateDoc.DocumentNode.SelectSingleNode("//div");
            var nodes = templateDoc.DocumentNode.SelectNodes("//div");
            var oldChild = nodes[1];

            var contentDoc = new HtmlDocument();
            contentDoc.Load($"..\\..\\..\\public\\{pageName}.html");
            var insert = contentDoc.DocumentNode.SelectSingleNode("//div").InnerHtml;
            oldChild.InnerHtml = insert;
            templateDoc.Save($"..\\..\\{pageName}.html");
        }
#endif
    }
}
