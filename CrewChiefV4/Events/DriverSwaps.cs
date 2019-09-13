using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrewChiefV4.GameState;

namespace CrewChiefV4.Events
{
    class DriverSwaps : AbstractEvent
    {
        private Boolean playedStintOverThisLap = false;
        private Boolean played15MinutesLeftInStint = false;
        private Boolean played10MinutesLeftInStint = false;
        private Boolean played5MinutesLeftInStint = false;
        private Boolean played2MinutesLeftInStint = false;

        public override void clearState()
        {
            playedStintOverThisLap = false;
            played15MinutesLeftInStint = false;
            played10MinutesLeftInStint = false;
            played5MinutesLeftInStint = false;
            played2MinutesLeftInStint = false;
        }

        protected override void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            if (previousGameState != null && previousGameState.PitData.DriverStintSecondsRemaining < currentGameState.PitData.DriverStintSecondsRemaining)
            {
                Console.WriteLine("Driver stint time remaining has increased, clearing state");
                clearState();
            }
            else if (!currentGameState.PitData.InPitlane && currentGameState.PitData.DriverStintSecondsRemaining > 0)
            {
                if (currentGameState.SessionData.IsNewLap)
                {
                    // check if we'll need to swap at the end of this lap
                    if (currentGameState.PitData.DriverStintSecondsRemaining < currentGameState.SessionData.PlayerLapTimeSessionBest + 30)
                    {
                        playedStintOverThisLap = true;
                        Console.WriteLine("Current stint expiring - pit this lap");
                    }
                    // check if we'll have run out of total stint time at the end of this lap
                    else if (currentGameState.PitData.DriverStintTotalSecondsRemaining < currentGameState.SessionData.PlayerLapTimeSessionBest + 30)
                    {
                        playedStintOverThisLap = true;
                        Console.WriteLine("Total driver seat time expiring - pit this lap");
                    }
                    else
                    {
                        playedStintOverThisLap = false;
                    }
                }
                // checks for intervals
                if (! played15MinutesLeftInStint &&
                    currentGameState.PitData.DriverStintSecondsRemaining < 960 && currentGameState.PitData.DriverStintSecondsRemaining > 930)
                {
                    played15MinutesLeftInStint = true;
                    Console.WriteLine("15 mins left in this stint");
                }
                else if (!played10MinutesLeftInStint &&
                    currentGameState.PitData.DriverStintSecondsRemaining < 600 && currentGameState.PitData.DriverStintSecondsRemaining > 570)
                {
                    played15MinutesLeftInStint = true;
                    played10MinutesLeftInStint = true;
                    Console.WriteLine("10 mins left in this stint");
                }
                else if (!played5MinutesLeftInStint &&
                    currentGameState.PitData.DriverStintSecondsRemaining < 300 && currentGameState.PitData.DriverStintSecondsRemaining > 270)
                {
                    played15MinutesLeftInStint = true;
                    played10MinutesLeftInStint = true;
                    played5MinutesLeftInStint = true;
                    Console.WriteLine("5 mins left in this stint");
                }
                else if (!played2MinutesLeftInStint &&
                    currentGameState.PitData.DriverStintSecondsRemaining < 120 && currentGameState.PitData.DriverStintSecondsRemaining > 110)
                {
                    played15MinutesLeftInStint = true;
                    played10MinutesLeftInStint = true;
                    played5MinutesLeftInStint = true;
                    played2MinutesLeftInStint = true;
                    Console.WriteLine("2 mins left in this stint");
                }
            }
        }

        public override void respond(String voiceMessage)
        {
        }
    }
}
