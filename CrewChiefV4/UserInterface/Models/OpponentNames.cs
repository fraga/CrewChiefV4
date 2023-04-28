using CrewChiefV4.UserInterface.VMs;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace CrewChiefV4.UserInterface.Models
{
    /// <summary>
    /// Model module of OpponentNames dialog MVVM
    /// </summary>
    internal class OpponentNames
    {
        private static OpponentNames inst = null;
        private readonly OpponentNames_VM viewModel;
        public OpponentNames(OpponentNames_VM _viewModel)
        {
            viewModel = _viewModel;
            LoadGuessedOpponentNames();
            guessedOpponentNamePairs();
            inst = this;
        }

        private void guessedOpponentNamePairs()
        {
            List<string> availableGuessedOpponentNames = new List<string>();
            foreach (var opponentName in guessedOpponentNames)
            {
                availableGuessedOpponentNames.Add($"{opponentName.Key}:{opponentName.Value}");
            }
            viewModel.fillGuessedOpponentNames(availableGuessedOpponentNames);
        }

        Dictionary<string, string> guessedOpponentNames = new Dictionary<string, string>();
        private void LoadGuessedOpponentNames()
        {
            guessedOpponentNames = DriverNameHelper.getGuessedDriverNames();
        }
        public void PlayOpponentDriverName(string name)
        {
            MyName.PlayDriverName(guessedOpponentNames[name]);
        }
        public static void NewDriverName(string opponentName, string wavFileName)
        {
            inst.guessedOpponentNames[opponentName] = wavFileName;
            inst.guessedOpponentNamePairs();
            DriverNameHelper.writeGuessedDriverNames(inst.guessedOpponentNames);
        }
        public void DeleteOpponentDriverName(string name)
        {
            guessedOpponentNames[name] = null;
            // Now save 
            DriverNameHelper.writeGuessedDriverNames(guessedOpponentNames);
            guessedOpponentNamePairs();
        }
        public void EditGuessedNames()
        {
            DriverNameHelper.EditGuessedNames();
            LoadGuessedOpponentNames();
            guessedOpponentNamePairs();
        }
        public void DeleteGuessedNames()
        {
            guessedOpponentNames.Clear();
            DriverNameHelper.writeGuessedDriverNames(guessedOpponentNames);
            guessedOpponentNamePairs();
        }
        public void EditNames()
        {
            DriverNameHelper.EditNames();
        }
        public void FormClose()
        {
            DriverNameHelper.ReadDriverNameMappings();
        }
    }
}
