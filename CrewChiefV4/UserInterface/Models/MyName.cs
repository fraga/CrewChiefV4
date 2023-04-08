using FuzzySharp;
using CrewChiefV4.Audio;
using CrewChiefV4.UserInterface.VMs;
using System.Linq;
using System.IO;
using System.Media;

namespace CrewChiefV4.UserInterface.Models
{
    /// <summary>
    /// Model module of MyName dialog MVVM
    /// </summary>
    internal class MyName
    {
        private readonly MyName_VM viewModel;
        public MyName(MyName_VM _viewModel)
        {
            viewModel = _viewModel;
        }
        /// <summary>
        /// Fuzzy match the name against all the full personalisations and
        /// also the list of opponent names that can have personalisation stubs
        /// applied to them
        /// </summary>
        /// <param name="name">Driver's name</param>
        public void NameEntry(string name)
        {
            // Create 2 lists of candidate names:
            // 1) Full personalisations
            int exactPersonalisationMatch = -1;
            string[] availablePersonalisations = MainWindow.instance.crewChief.audioPlayer.personalisationsArray;
            var matches = Process.ExtractTop(name, availablePersonalisations, limit: 10);
            availablePersonalisations = new string[matches.Count()];
            int index = 0;
            foreach (var match in matches)
            {
                availablePersonalisations[index] = match.Value;
                if (match.Score == 100)
                {
                    exactPersonalisationMatch = index;
                }
                index++;
            }
            // Send it to the VM
            viewModel.fillPersonalisations(availablePersonalisations);
            if (exactPersonalisationMatch != -1)
            {
                viewModel.selectPersonalisation(exactPersonalisationMatch);
            }

            // 2) Just driver names
            int exactDriverNameMatch = -1;
            string[] driverNames = SoundCache.availableDriverNamesForUI.ToArray();
            matches = Process.ExtractTop(name, driverNames, limit: 10);
            driverNames = new string[matches.Count()];
            index = 0;
            foreach (var match in matches)
            {
                driverNames[index] = match.Value;
                if (exactPersonalisationMatch == -1 && match.Score == 100)
                {
                    exactDriverNameMatch = index;
                }
                index++;
            }
            viewModel.fillDriverNames(driverNames);
            if (exactDriverNameMatch != -1)
            {
                viewModel.selectDriverName(exactDriverNameMatch);
            }
        }
        public void PlayRandomPersonalisation(string name)
        {
            DirectoryInfo personalisationsDirectory = new DirectoryInfo(Path.Combine(AudioPlayer.soundFilesPath, "personalisations", name, "prefixes_and_suffixes", "bad_luck"));
            if (personalisationsDirectory.Exists)
            {
                FileInfo[] files = personalisationsDirectory.GetFiles();
                var wavFile = files[Utilities.random.Next(0, files.Length)];
                Log.Info($"Playing {wavFile.FullName}");
                SoundPlayer simpleSound = new SoundPlayer(wavFile.FullName);
                simpleSound.Play();
            }
        }
        public void PlayRandomDriverName(string name)
        {
            //DirectoryInfo soundsDirectory = new DirectoryInfo(AudioPlayer.soundFilesPath);
            DirectoryInfo personalisationsDirectory = new DirectoryInfo(Path.Combine(AudioPlayer.soundFilesPath, "driver_names"));
            //SoundCache.createCompositePrefixesAndSuffixes(soundsDirectory, personalisationsDirectory, name);
            if (personalisationsDirectory.Exists)
            {
                string wavFile = personalisationsDirectory.FullName;
                name += ".wav";
                Log.Info($"Playing {wavFile}\\{name}");
                SoundPlayer simpleSound = new SoundPlayer(Path.Combine(wavFile, name));
                simpleSound.Play();
            }
        }
        public void SelectPersonalisation(string name)
        {
            if (!UserSettings.GetUserSettings().getString("PERSONALISATION_NAME").Equals(name))
            {
                UserSettings.GetUserSettings().setProperty("PERSONALISATION_NAME", name);
                UserSettings.GetUserSettings().saveUserSettings();
                viewModel.doRestart();
                Log.Info($"My name '{name}' selected");
            }
        }
        public void SelectDriverName(string name)
        {
            UserSettings.GetUserSettings().setProperty("allow_composite_personalisations", true);
            SelectPersonalisation(name);
        }
    }
}
