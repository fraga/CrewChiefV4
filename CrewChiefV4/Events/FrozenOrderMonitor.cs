/*
 * This monitor announces order information during Full Course Yellow/Yellow (in NASCAR), Rolling start and Formation/standing starts.
 * 
 * Official website: thecrewchief.org 
 * License: MIT
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewChiefV4.RaceRoom.RaceRoomData;
using System.Threading;
using CrewChiefV4.GameState;
using CrewChiefV4.Audio;

namespace CrewChiefV4.Events
{
    class FrozenOrderMonitor : AbstractEvent
    {
        private const int ACTION_STABLE_THRESHOLD = 10;
        private const float DIST_TO_START_TO_ANNOUNCE_POS_REMINDER = 300.0f;

        private bool useDriverName = UserSettings.GetUserSettings().getBoolean("iracing_fcy_formation_use_drivername");
        // Number of updates FO Action and Driver name were the same.
        private int numUpdatesActionSame = 0;
        private Int64 lastActionUpdateTicks = DateTime.MinValue.Ticks;

        // Last FO Action and Driver name announced.
        private FrozenOrderAction currFrozenOrderAction = FrozenOrderAction.None;
        private string currDriverToFollow = null;
        private FrozenOrderColumn currFrozenOrderColumn = FrozenOrderColumn.None;

        // Next FO Action and Driver to be announced if it stays stable for a ACTION_STABLE_THRESHOLD times.
        private FrozenOrderAction newFrozenOrderAction = FrozenOrderAction.None;
        private string newDriverToFollow = null;
        private FrozenOrderColumn newFrozenOrderColumn = FrozenOrderColumn.None;

        private bool formationStandingStartAnnounced = false;
        private bool formationStandingPreStartReminderAnnounced = false;

        private bool scrLastFCYLapLaneAnnounced = false;

        // sounds...
        public const string folderFollow = "frozen_order/follow";
        public const string folderInTheLeftColumn = "frozen_order/in_the_left_column";
        public const string folderInTheRightColumn = "frozen_order/in_the_right_column";
        public const string folderInTheInsideColumn = "frozen_order/in_the_inside_column";
        public const string folderInTheOutsideColumn = "frozen_order/in_the_outside_column";

        // for cases where we have no driver name:
        private const string folderLineUpInLeftColumn = "frozen_order/line_up_in_the_left_column";
        private const string folderLineUpInRightColumn = "frozen_order/line_up_in_the_right_column";
        private const string folderLineUpInInsideColumn = "frozen_order/line_up_in_the_inside_column";
        private const string folderLineUpInOutsideColumn = "frozen_order/line_up_in_the_outside_column";

        public const string folderCatchUpTo = "frozen_order/catch_up_to";    // can we have multiple phrasings of this without needing different structure?
        public const string folderAllow = "frozen_order/allow";
        public const string folderToPass = "frozen_order/to_pass";
        public const string folderTheSafetyCar = "frozen_order/the_safety_car";
        public const string folderThePaceCar = "frozen_order/the_pace_car";
        public const string folderYoureAheadOfAGuyYouShouldBeFollowing = "frozen_order/youre_ahead_of_guy_you_should_follow";
        public const string folderYouNeedToCatchUpToTheGuyAhead = "frozen_order/you_need_to_catch_up_to_the_guy_ahead";
        public const string folderAllowGuyBehindToPass = "frozen_order/allow_guy_behind_to_pass";

        // For car numbers
        public const string folderFollowCarNumber = "frozen_order/follow_car_number";
        public const string folderCatchUpToCarNumber = "frozen_order/catch_up_to_car_number";
        public const string folderAllowCarNumber = "frozen_order/allow_car_number";

        // single file
        public const string folderFCYLineUpSingleFile = "frozen_order/fcy_lineup_single_file";
        public const string folderLineUpSingleFileBehind = "frozen_order/line_up_single_file_behind";
        public const string folderLineUpSingleFileBehindCarNumber = "frozen_order/line_up_single_file_behind_car_number";
        public const string folderLineUpSingleFileBehindSafetyCarEU = "frozen_order/line_up_single_file_behind_safety_car_eu";
        public const string folderLineUpSingleFileBehindSafetyCarUS = "frozen_order/line_up_single_file_behind_safety_car_usa";


        public const string folderWeStartingFromPosition = "frozen_order/were_starting_from_position";
        public const string folderRow = "frozen_order/row";    // "starting from position 4, row 2 in the outside column" - uses column stuff above
        // we'll use the get-ready sound from the LapCounter event here
        public const string folderWereStartingFromPole = "frozen_order/were_starting_from_pole";
        public const string folderSafetyCarSpeedIs = "frozen_order/safety_car_speed_is";
        public const string folderPaceCarSpeedIs = "frozen_order/pace_car_speed_is";
        public const string folderMilesPerHour = "frozen_order/miles_per_hour";
        public const string folderKilometresPerHour = "frozen_order/kilometres_per_hour";
        public const string folderSafetyCarJustLeft = "frozen_order/safety_car_just_left"; // left the pits?
        public const string folderPaceCarJustLeft = "frozen_order/pace_car_just_left"; // left the pits?

        public const string folderSafetyCarIsOut = "frozen_order/safetycar_out_eu"; // left the pits?
        public const string folderPaceCarIsOut = "frozen_order/safetycar_out_usa"; // left the pits?

        public const string folderRollingStartReminder = "frozen_order/thats_a_rolling_start";
        public const string folderStandingStartReminder = "frozen_order/thats_a_standing_start";
        public const string folderStayInPole = "frozen_order/stay_in_pole";
        public const string folderStayInPoleInInsideColumn = "frozen_order/stay_in_pole_in_inside_column";
        public const string folderStayInPoleInOutsideColumn = "frozen_order/stay_in_pole_in_outside_column";
        public const string folderStayInPoleInLeftColumn = "frozen_order/stay_in_pole_in_left_column";
        public const string folderStayInPoleInRightColumn = "frozen_order/stay_in_pole_in_right_column";
        public const string folderMoveToPole = "frozen_order/move_to_pole";
        public const string folderMoveToPoleRow = "frozen_order/move_to_pole_row";
        public const string folderPassThePaceCar = "frozen_order/pass_the_pace_car";
        public const string folderPassTheSafetyCar = "frozen_order/pass_the_safety_car";

        // Validation stuff:
        private const string validateMessageTypeKey = "validateMessageTypeKey";
        private const string validateMessageTypeAction = "validateMessageTypeAction";
        private const string validationActionKey = "validationActionKey";
        private const string validationAssignedPositionKey = "validationAssignedPositionKey";
        private const string validationDriverToFollowKey = "validationDriverToFollowKey";

        private const string doubleZeroKey = "numbers/zerozero";
        private const string zeroKey = "numbers/0";

        public FrozenOrderMonitor(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
        }

        public override List<SessionType> applicableSessionTypes
        {
            get { return new List<SessionType> { SessionType.Race }; }
        }

        public override List<SessionPhase> applicableSessionPhases
        {
            get { return new List<SessionPhase> { SessionPhase.Formation, SessionPhase.FullCourseYellow }; }
        }

        /*
         * IMPORTANT: This method is called twice - when the message becomes due, and immediately before playing it (which may have a 
         * delay caused by the length of the queue at the time). So be *very* careful when checking and updating local state in here.
         */
        public override bool isMessageStillValid(string eventSubType, GameStateData currentGameState, Dictionary<String, Object> validationData)
        {
            if (base.isMessageStillValid(eventSubType, currentGameState, validationData))
            {
                if (currentGameState.PitData.InPitlane)
                    return false;
               
                if (validationData == null)
                    return true;
               
                if ((string)validationData[FrozenOrderMonitor.validateMessageTypeKey] == FrozenOrderMonitor.validateMessageTypeAction)
                {
                    var queuedAction = (FrozenOrderAction)validationData[FrozenOrderMonitor.validationActionKey];
                    var queuedAssignedPosition = (int)validationData[FrozenOrderMonitor.validationAssignedPositionKey];
                    var queuedDriverToFollow = (string)validationData[FrozenOrderMonitor.validationDriverToFollowKey];
                    if (queuedAction == currentGameState.FrozenOrderData.Action
                        && queuedAssignedPosition == currentGameState.FrozenOrderData.AssignedPosition
                        && queuedDriverToFollow == currentGameState.FrozenOrderData.DriverToFollowRaw)
                        return true;
                    else
                    {
                        Console.WriteLine(string.Format("Frozen Order: message invalidated.  Was {0} {1} {2} is {3} {4} {5}", queuedAction, queuedAssignedPosition, queuedDriverToFollow,
                            currentGameState.FrozenOrderData.Action, currentGameState.FrozenOrderData.AssignedPosition, currentGameState.FrozenOrderData.DriverToFollowRaw));
                        return false;
                    }
                }
            }
            return false;
        }

        public override void clearState()
        {
            this.formationStandingStartAnnounced = false;
            this.formationStandingPreStartReminderAnnounced = false;
            this.numUpdatesActionSame = 0;
            this.newFrozenOrderAction = FrozenOrderAction.None;
            this.newDriverToFollow = null;
            this.newFrozenOrderColumn = FrozenOrderColumn.None;
            this.currFrozenOrderAction = FrozenOrderAction.None;
            this.currDriverToFollow = null;
            this.currFrozenOrderColumn = FrozenOrderColumn.None;
            this.scrLastFCYLapLaneAnnounced = false;
            this.lastActionUpdateTicks = DateTime.MinValue.Ticks;
        }
        
        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            var cgs = currentGameState;
            var pgs = previousGameState;
            if (!GlobalBehaviourSettings.enableFrozenOrderMessages
                || cgs.PitData.InPitlane /*|| cgs.SessionData.SessionRunningTime < 10 */
                || GameStateData.onManualFormationLap  // We may want manual formation to phase of FrozenOrder.
                || pgs == null)
                return; // don't process if we're in the pits or just started a session

            var cfod = cgs.FrozenOrderData;
            var pfod = pgs.FrozenOrderData;

            var cfodp = cgs.FrozenOrderData.Phase;
            if (cfodp == FrozenOrderPhase.None)
                return;  // Nothing to do.

            var useAmericanTerms = GlobalBehaviourSettings.useAmericanTerms || GlobalBehaviourSettings.useOvalLogic;
            var useOvalLogic = GlobalBehaviourSettings.useOvalLogic;

            if (pfod.Phase == FrozenOrderPhase.None)
            {
                Console.WriteLine("Frozen Order: New Phase detected: " + cfod.Phase);
                int delay = Utilities.random.Next(0, 3);
                if (cfod.Phase == FrozenOrderPhase.Rolling && CrewChief.gameDefinition.gameEnum != GameEnum.IRACING)
                    audioPlayer.playMessage(new QueuedMessage(folderRollingStartReminder, delay + 4, secondsDelay: delay, abstractEvent: this, priority: 10));
                else if (cfod.Phase == FrozenOrderPhase.FormationStanding)
                    audioPlayer.playMessage(new QueuedMessage(folderStandingStartReminder, delay + 4, secondsDelay: delay, abstractEvent: this, priority: 10));

                // Clear previous state.
                this.clearState();
            }

            // Because FO Action is distance dependent, it tends to fluctuate.
            // We need to detect when it stabilizes (values stay identical for ACTION_STABLE_THRESHOLD times).
            if (cfod.Action == pfod.Action
                && cfod.DriverToFollowRaw == pfod.DriverToFollowRaw
                && cfod.AssignedColumn == pfod.AssignedColumn)
                ++this.numUpdatesActionSame;
            else
            {
                this.numUpdatesActionSame = 0;
                this.lastActionUpdateTicks = cgs.Now.Ticks;
            }

            this.newFrozenOrderAction = cfod.Action;
            this.newDriverToFollow = cfod.DriverToFollowRaw;
            this.newFrozenOrderColumn = cfod.AssignedColumn;

            var isActionUpdateStable = CrewChief.gameDefinition.gameEnum != GameEnum.GTR2
                ? this.numUpdatesActionSame >= FrozenOrderMonitor.ACTION_STABLE_THRESHOLD
                : TimeSpan.FromTicks(cgs.Now.Ticks - this.lastActionUpdateTicks).TotalMilliseconds > 500;  // GTR2 updates at ~2FPS.  So use ticks to detect stability.  Ticks is better in general, but I've no time to test rF2/iR for now.

            // Detect if we should be following SC, as SC has no driver name.
            var shouldFollowSafetyCar = false;
            var driverToFollow = "";
            var carNumber = -1;
            // add leading "zero" to announcment of car number?
            var leadingZeros = false;
            // this will be either "zero" or "zerozero" depending on the number
            var leadingZerosKey = "";
            var useCarNumber = false;
            if (cfodp == FrozenOrderPhase.Rolling || cfodp == FrozenOrderPhase.FullCourseYellow)
            {
                shouldFollowSafetyCar = (cfod.AssignedColumn == FrozenOrderColumn.None && cfod.AssignedPosition == 1)  // Single file order.
                    || (cfod.AssignedColumn != FrozenOrderColumn.None && cfod.AssignedPosition <= 2);  // Double file (grid) order.

                useCarNumber = (CrewChief.gameDefinition.gameEnum == GameEnum.IRACING || (CrewChief.gameDefinition.gameEnum == GameEnum.GTR2 && !cgs.carClass.preferNameForOrderMessages))
                    && !shouldFollowSafetyCar && cfod.CarNumberToFollowRaw != "-1" && Int32.TryParse(cfod.CarNumberToFollowRaw, out carNumber)
                    && !(useDriverName && cfod.DriverToFollowRaw != null && AudioPlayer.canReadName(cfod.DriverToFollowRaw, false));

                if (useCarNumber && cfod.CarNumberToFollowRaw.StartsWith("00"))
                {
                    leadingZerosKey = doubleZeroKey;
                    leadingZeros = true;
                }
                else if (useCarNumber && cfod.CarNumberToFollowRaw.StartsWith("0"))
                {
                    leadingZerosKey = zeroKey;
                    leadingZeros = true;
                }
                driverToFollow = shouldFollowSafetyCar ? (useAmericanTerms ? folderThePaceCar : folderTheSafetyCar) : (useCarNumber ? cfod.CarNumberToFollowRaw : cfod.DriverToFollowRaw);
            }

            if (cfodp == FrozenOrderPhase.Rolling
                && cfod.Action != FrozenOrderAction.None)
            {
                var prevDriverToFollow = this.currDriverToFollow;
                var prevFrozenOrderAction = this.currFrozenOrderAction;

                if (isActionUpdateStable
                    && (this.currFrozenOrderAction != this.newFrozenOrderAction
                        || this.currDriverToFollow != this.newDriverToFollow
                        || this.currFrozenOrderColumn != this.newFrozenOrderColumn))
                {
                    this.currFrozenOrderAction = this.newFrozenOrderAction;
                    this.currDriverToFollow = this.newDriverToFollow;
                    this.currFrozenOrderColumn = this.newFrozenOrderColumn;

                    // canReadDriverToFollow will be true if we're behind the safety car or we can read the driver's name:
                    var canReadDriverToFollow = shouldFollowSafetyCar || useCarNumber || (driverToFollow != null && AudioPlayer.canReadName(driverToFollow));

                    var usableDriverNameToFollow = shouldFollowSafetyCar || useCarNumber ? driverToFollow : (driverToFollow != null ? DriverNameHelper.getUsableDriverName(driverToFollow) : null);

                    // special case for a single leading zero - only play it if we have to - e.g. there is a car using number 023 and one using number 23
                    if (useCarNumber && leadingZeros && leadingZerosKey == zeroKey)
                        leadingZeros = this.ShouldUseLeadingZeros(carNumber, currentGameState.getCarNumbers());
                    
                    var validationData = new Dictionary<string, object>();
                    validationData.Add(FrozenOrderMonitor.validateMessageTypeKey, FrozenOrderMonitor.validateMessageTypeAction);
                    validationData.Add(FrozenOrderMonitor.validationActionKey, cfod.Action);
                    validationData.Add(FrozenOrderMonitor.validationAssignedPositionKey, cfod.AssignedPosition);
                    validationData.Add(FrozenOrderMonitor.validationDriverToFollowKey, cfod.DriverToFollowRaw);

                    if (this.newFrozenOrderAction == FrozenOrderAction.Follow
                        && prevDriverToFollow != this.currDriverToFollow)  // Don't announce Follow messages for the driver that we caught up to or allowed to pass.
                    {
                        if (canReadDriverToFollow)
                        { 
                            // Follow messages are only meaningful if there's name to announce.
                            int delay = Utilities.random.Next(3, 6);
                            if (cfod.AssignedColumn == FrozenOrderColumn.None
                                || Utilities.random.Next(1, 11) > 8)  // Randomly, announce message without coulmn info.
                            {
                                if (!useCarNumber || shouldFollowSafetyCar)
                                    audioPlayer.playMessage(new QueuedMessage(!shouldFollowSafetyCar ? "frozen_order/follow_driver" : "frozen_order/follow_safety_car", delay + 6,
                                        secondsDelay: delay, messageFragments: MessageContents(folderFollow, usableDriverNameToFollow), abstractEvent: this,
                                        validationData: validationData, priority: 10));
                                else
                                    audioPlayer.playMessage(new QueuedMessage("frozen_order/follow_driver", 0,
                                        secondsDelay: 0, messageFragments:
                                        leadingZeros ? MessageContents(folderFollowCarNumber, leadingZerosKey, carNumber) : MessageContents(folderFollowCarNumber, carNumber), abstractEvent: this,
                                        validationData: validationData, priority: 10));
                            }
                            else
                            {
                                string columnName;
                                if (useOvalLogic)
                                    columnName = cfod.AssignedColumn == FrozenOrderColumn.Left ? folderInTheInsideColumn : folderInTheOutsideColumn;
                                else
                                    columnName = cfod.AssignedColumn == FrozenOrderColumn.Left ? folderInTheLeftColumn : folderInTheRightColumn;
                                if (!useCarNumber || shouldFollowSafetyCar)
                                    audioPlayer.playMessage(new QueuedMessage(!shouldFollowSafetyCar ? "frozen_order/follow_driver_in_col" : "frozen_order/follow_safecy_car_in_col", delay + 6,
                                        secondsDelay: delay, messageFragments: MessageContents(folderFollow, usableDriverNameToFollow, columnName), abstractEvent: this,
                                        validationData: validationData, priority: 10));
                                else
                                    audioPlayer.playMessage(new QueuedMessage("frozen_order/follow_driver_in_col", 0,
                                        secondsDelay: 0, messageFragments:
                                        leadingZeros ? MessageContents(folderFollowCarNumber, leadingZerosKey, carNumber, columnName) : MessageContents(folderFollowCarNumber, carNumber, columnName), abstractEvent: this,
                                        validationData: validationData, priority: 10));
                            }
                        }
                    }
                    else if (this.newFrozenOrderAction == FrozenOrderAction.AllowToPass)
                    {
                        // Follow messages are only meaningful if there's name to announce.
                        int delay = Utilities.random.Next(1, 4);
                        if ((canReadDriverToFollow && Utilities.random.Next(0, 11) > 1)   // Randomly, announce message without name.
                            || shouldFollowSafetyCar)  // Unless it is SC
                        {
                            if (!useCarNumber || shouldFollowSafetyCar)
                                audioPlayer.playMessage(new QueuedMessage(!shouldFollowSafetyCar ? "frozen_order/allow_driver_to_pass" : "frozen_order/allow_safety_car_to_pass", delay + 6,
                                    secondsDelay: delay, messageFragments: MessageContents(folderAllow, usableDriverNameToFollow, folderToPass), abstractEvent: this,
                                    validationData: validationData, priority: 10));
                            else
                                audioPlayer.playMessage(new QueuedMessage("frozen_order/allow_driver_to_pass", delay + 6,
                                    secondsDelay: delay, messageFragments:
                                    leadingZeros ? MessageContents(folderAllowCarNumber, leadingZerosKey, carNumber, folderToPass) : MessageContents(folderAllowCarNumber, carNumber, folderToPass), abstractEvent: this,
                                    validationData: validationData, priority: 10));

                        }
                        else
                            audioPlayer.playMessage(new QueuedMessage(folderYoureAheadOfAGuyYouShouldBeFollowing, delay + 6, secondsDelay: delay, abstractEvent: this, 
                                validationData: validationData, priority: 10));
                    }
                    else if (this.newFrozenOrderAction == FrozenOrderAction.CatchUp)
                    {
                        int delay = Utilities.random.Next(1, 4);
                        if ((canReadDriverToFollow && Utilities.random.Next(0, 11) > 1)  // Randomly, announce message without name.
                            || shouldFollowSafetyCar)
                        {
                            if(!useCarNumber || shouldFollowSafetyCar)
                                audioPlayer.playMessage(new QueuedMessage(!shouldFollowSafetyCar ? "frozen_order/catch_up_to_driver" : "frozen_order/catch_up_to_safety_car", delay + 6,
                                    secondsDelay: delay, messageFragments: MessageContents(folderCatchUpTo, usableDriverNameToFollow), abstractEvent: this,
                                    validationData: validationData, priority: 10));
                            else
                                audioPlayer.playMessage(new QueuedMessage("frozen_order/catch_up_to_driver", delay + 6,
                                    secondsDelay: delay, messageFragments:
                                    leadingZeros ? MessageContents(folderCatchUpToCarNumber, leadingZerosKey, carNumber) : MessageContents(folderCatchUpToCarNumber, carNumber), abstractEvent: this,
                                    validationData: validationData, priority: 10));
                        }
                        else
                            audioPlayer.playMessage(new QueuedMessage(folderYouNeedToCatchUpToTheGuyAhead, delay + 6, secondsDelay: delay,abstractEvent: this,
                                validationData: validationData, priority: 10));
                    }
                    else if (this.newFrozenOrderAction == FrozenOrderAction.StayInPole
                        && prevFrozenOrderAction != FrozenOrderAction.MoveToPole)  // No point in nagging user to stay in pole if we previously told them to move there.
                    {
                        int delay = Utilities.random.Next(0, 3);
                        if (cfod.AssignedColumn == FrozenOrderColumn.None
                            || Utilities.random.Next(1, 11) > 8)  // Randomly, announce message without coulmn info.
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/stay_in_pole", delay + 6, secondsDelay: delay,
                                messageFragments: MessageContents(folderStayInPole), abstractEvent: this, validationData: validationData, priority: 10));
                        else
                        {
                            string folderToPlay = null;
                            if (useOvalLogic)
                                folderToPlay = cfod.AssignedColumn == FrozenOrderColumn.Left ? folderStayInPoleInInsideColumn : folderStayInPoleInOutsideColumn;
                            else
                                folderToPlay = cfod.AssignedColumn == FrozenOrderColumn.Left ? folderStayInPoleInLeftColumn : folderStayInPoleInRightColumn;

                            audioPlayer.playMessage(new QueuedMessage("frozen_order/stay_in_pole_in_column", delay + 6, secondsDelay: delay,
                                messageFragments: MessageContents(folderToPlay), abstractEvent: this, validationData: validationData, priority: 10));
                        }
                    }
                    else if (this.newFrozenOrderAction == FrozenOrderAction.MoveToPole)
                    {
                        int delay = Utilities.random.Next(2, 5);
                        if (cfod.AssignedColumn == FrozenOrderColumn.None)
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/move_to_pole", delay + 6, secondsDelay: delay,
                                messageFragments: MessageContents(folderMoveToPole), abstractEvent: this, 
                                validationData: validationData, priority: 10));
                        else
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/move_to_pole_row", delay + 6, secondsDelay: delay,
                                messageFragments: MessageContents(folderMoveToPoleRow), abstractEvent: this,
                                validationData: validationData, priority: 10));
                    }
                }
            }
            else if (cfodp == FrozenOrderPhase.FullCourseYellow && CrewChief.gameDefinition.gameEnum == GameEnum.IRACING)
            {
                var prevDriverToFollow = this.currDriverToFollow;
                var prevFrozenOrderColumn = this.currFrozenOrderColumn;

                var announceSCRLastFCYLapLane = useAmericanTerms
                    && currentGameState.StockCarRulesData.stockCarRulesEnabled
                    && (currentGameState.FlagData.fcyPhase == FullCourseYellowPhase.LAST_LAP_NEXT || currentGameState.FlagData.fcyPhase == FullCourseYellowPhase.LAST_LAP_CURRENT);
                
                this.currFrozenOrderAction = this.newFrozenOrderAction;
                this.currDriverToFollow = this.newDriverToFollow;
                this.currFrozenOrderColumn = this.newFrozenOrderColumn;

                this.scrLastFCYLapLaneAnnounced = announceSCRLastFCYLapLane;

                // canReadDriverToFollow will be true if we're behind the safety car or we can read the driver's name:
                var canReadDriverToFollow = shouldFollowSafetyCar || useCarNumber || (driverToFollow != null && AudioPlayer.canReadName(driverToFollow));

                var usableDriverNameToFollow = shouldFollowSafetyCar || useCarNumber ? driverToFollow : (driverToFollow != null ? DriverNameHelper.getUsableDriverName(driverToFollow) : null);

                // special case for a single leading zero - only play it if we have to - e.g. there is a car using number 023 and one using number 23
                if (useCarNumber && leadingZeros && leadingZerosKey == zeroKey)
                    leadingZeros = this.ShouldUseLeadingZeros(carNumber, currentGameState.getCarNumbers());

                var validationData = new Dictionary<string, object>();
                validationData.Add(FrozenOrderMonitor.validateMessageTypeKey, FrozenOrderMonitor.validateMessageTypeAction);
                validationData.Add(FrozenOrderMonitor.validationActionKey, cfod.Action);
                validationData.Add(FrozenOrderMonitor.validationAssignedPositionKey, cfod.AssignedPosition);
                validationData.Add(FrozenOrderMonitor.validationDriverToFollowKey, cfod.DriverToFollowRaw);

                if ((this.newFrozenOrderAction == FrozenOrderAction.Follow || this.newFrozenOrderAction == FrozenOrderAction.CatchUp) && prevDriverToFollow != this.newDriverToFollow)
                {
                    int delay = Utilities.random.Next(0, 3);
                    if (cgs.SafetyCarData.fcySafetyCarCallsEnabled && !pgs.SafetyCarData.isOnTrack && cgs.SafetyCarData.isOnTrack)
                    {
                        if (shouldFollowSafetyCar)
                        {
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/lineup_single_file_follow_safety_car",
                                delay + 6, secondsDelay: delay, messageFragments: MessageContents(useAmericanTerms ? folderPaceCarIsOut : folderSafetyCarIsOut, useAmericanTerms ? folderLineUpSingleFileBehindSafetyCarUS : folderLineUpSingleFileBehindSafetyCarEU),
                                abstractEvent: this, validationData: validationData, priority: 10));
                        }
                        else if (useCarNumber)
                        {
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/lineup_single_file_follow_car_number",
                                delay + 6, secondsDelay: delay, messageFragments:
                                leadingZeros ? 
                                    MessageContents(useAmericanTerms ? folderPaceCarIsOut : folderSafetyCarIsOut, folderLineUpSingleFileBehindCarNumber, leadingZerosKey, carNumber) :
                                    MessageContents(useAmericanTerms ? folderPaceCarIsOut : folderSafetyCarIsOut, folderLineUpSingleFileBehindCarNumber, carNumber), 
                                abstractEvent: this, validationData: validationData, priority: 10));

                        }
                        else if (canReadDriverToFollow)
                        {
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/lineup_single_file_follow",
                                delay + 6, secondsDelay: delay, messageFragments: MessageContents(useAmericanTerms ? folderPaceCarIsOut : folderSafetyCarIsOut, folderLineUpSingleFileBehind, usableDriverNameToFollow), 
                                abstractEvent: this, validationData: validationData, priority: 10));
                        }
                        else
                            audioPlayer.playMessage(new QueuedMessage(folderPaceCarIsOut, 10, abstractEvent: this));
                    }
                    else if (!cgs.SafetyCarData.fcySafetyCarCallsEnabled && this.newFrozenOrderColumn == FrozenOrderColumn.None 
                        && prevDriverToFollow != this.newDriverToFollow)
                    {
                        if (shouldFollowSafetyCar)
                        {
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/lineup_single_file_follow_safety_car",
                                delay + 6, secondsDelay: delay, messageFragments: MessageContents(useAmericanTerms ? folderLineUpSingleFileBehindSafetyCarUS : folderLineUpSingleFileBehindSafetyCarEU), 
                                abstractEvent: this, validationData: validationData, priority: 10));
                        }
                        else if (useCarNumber)
                        {
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/lineup_single_file_follow_car_number",
                                delay + 6, secondsDelay: delay, messageFragments: 
                                leadingZeros ? MessageContents(folderLineUpSingleFileBehindCarNumber, leadingZerosKey, carNumber) : MessageContents(folderLineUpSingleFileBehindCarNumber, carNumber), 
                                abstractEvent: this, validationData: validationData, priority: 10));

                        }
                        else if (canReadDriverToFollow)
                        {
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/lineup_single_file_follow",
                                delay + 6, secondsDelay: delay, messageFragments: MessageContents(folderLineUpSingleFileBehind, usableDriverNameToFollow), 
                                abstractEvent: this, validationData: validationData, priority: 10));
                        }
                    }
                }
            }
            else if (cfodp == FrozenOrderPhase.FullCourseYellow
                && cfod.Action != FrozenOrderAction.None)
            {
                var prevDriverToFollow = this.currDriverToFollow;
                var prevFrozenOrderColumn = this.currFrozenOrderColumn;

                var announceSCRLastFCYLapLane = useAmericanTerms
                    && currentGameState.StockCarRulesData.stockCarRulesEnabled
                    && (currentGameState.FlagData.fcyPhase == FullCourseYellowPhase.LAST_LAP_NEXT || currentGameState.FlagData.fcyPhase == FullCourseYellowPhase.LAST_LAP_CURRENT);

                if (isActionUpdateStable
                    && (this.currFrozenOrderAction != this.newFrozenOrderAction
                        || this.currDriverToFollow != this.newDriverToFollow
                        || this.currFrozenOrderColumn != this.newFrozenOrderColumn
                        || announceSCRLastFCYLapLane && !this.scrLastFCYLapLaneAnnounced))
                {
                    this.currFrozenOrderAction = this.newFrozenOrderAction;
                    this.currDriverToFollow = this.newDriverToFollow;
                    this.currFrozenOrderColumn = this.newFrozenOrderColumn;

                    this.scrLastFCYLapLaneAnnounced = announceSCRLastFCYLapLane;

                    // canReadDriverToFollow will be true if we're behind the safety car or we can read the driver's name:
                    var canReadDriverToFollow = shouldFollowSafetyCar || useCarNumber || (driverToFollow != null && AudioPlayer.canReadName(driverToFollow));

                    var usableDriverNameToFollow = shouldFollowSafetyCar || useCarNumber ? driverToFollow : (driverToFollow != null ? DriverNameHelper.getUsableDriverName(driverToFollow) : null);

                    // special case for a single leading zero - only play it if we have to - e.g. there is a car using number 023 and one using number 23
                    if (useCarNumber && leadingZeros && leadingZerosKey == zeroKey)
                        leadingZeros = this.ShouldUseLeadingZeros(carNumber, currentGameState.getCarNumbers());

                    var validationData = new Dictionary<string, object>();
                    validationData.Add(FrozenOrderMonitor.validateMessageTypeKey, FrozenOrderMonitor.validateMessageTypeAction);
                    validationData.Add(FrozenOrderMonitor.validationActionKey, cfod.Action);
                    validationData.Add(FrozenOrderMonitor.validationAssignedPositionKey, cfod.AssignedPosition);
                    validationData.Add(FrozenOrderMonitor.validationDriverToFollowKey, cfod.DriverToFollowRaw);

                    if (this.newFrozenOrderAction == FrozenOrderAction.Follow
                        && ((prevDriverToFollow != this.currDriverToFollow)  // Don't announce Follow messages for the driver that we caught up to or allowed to pass.
                            || (prevFrozenOrderColumn != this.currFrozenOrderColumn && announceSCRLastFCYLapLane)))  // But announce for SCR last FCY lap.
                    {
                        string columnName;
                        if (useOvalLogic)
                            columnName = cfod.AssignedColumn == FrozenOrderColumn.Left ? folderInTheInsideColumn : folderInTheOutsideColumn;
                        else
                            columnName = cfod.AssignedColumn == FrozenOrderColumn.Left ? folderInTheLeftColumn : folderInTheRightColumn;

                        int delay = Utilities.random.Next(0, 3);
                        if (canReadDriverToFollow)
                        {
                            if (!useCarNumber || shouldFollowSafetyCar)
                            {
                                if (announceSCRLastFCYLapLane && cfod.AssignedColumn == FrozenOrderColumn.Left)
                                    audioPlayer.playMessage(new QueuedMessage(!shouldFollowSafetyCar ? "frozen_order/follow_driver_in_left" : "frozen_order/follow_safety_car_in_left",
                                        delay + 6, secondsDelay: delay, messageFragments: MessageContents(folderFollow, usableDriverNameToFollow, columnName), abstractEvent: this, validationData: validationData, priority: 10));
                                else if (announceSCRLastFCYLapLane && cfod.AssignedColumn == FrozenOrderColumn.Right)
                                    audioPlayer.playMessage(new QueuedMessage(!shouldFollowSafetyCar ? "frozen_order/follow_driver_in_right" : "frozen_order/follow_safety_car_in_right",
                                        delay + 6, secondsDelay: delay,
                                        messageFragments: MessageContents(folderFollow, usableDriverNameToFollow, columnName),
                                        abstractEvent: this, validationData: validationData, priority: 10));
                                else
                                    audioPlayer.playMessage(new QueuedMessage(!shouldFollowSafetyCar ? "frozen_order/follow_driver" : "frozen_order/follow_safety_car", delay + 6,
                                        secondsDelay: delay, messageFragments: MessageContents(folderFollow, usableDriverNameToFollow), abstractEvent: this, validationData: validationData, priority: 10));
                            }
                            else
                            {
                                if (announceSCRLastFCYLapLane && cfod.AssignedColumn == FrozenOrderColumn.Left)
                                    audioPlayer.playMessage(new QueuedMessage("frozen_order/follow_driver_in_left",
                                        delay + 6, secondsDelay: delay, messageFragments: leadingZeros ? MessageContents(folderFollowCarNumber, leadingZerosKey, carNumber, columnName) : MessageContents(folderFollowCarNumber, carNumber, columnName),
                                        abstractEvent: this, validationData: validationData, priority: 10));
                                else if (announceSCRLastFCYLapLane && cfod.AssignedColumn == FrozenOrderColumn.Right)
                                    audioPlayer.playMessage(new QueuedMessage("frozen_order/follow_driver_in_right",
                                        delay + 6, secondsDelay: delay,
                                        messageFragments: leadingZeros ? MessageContents(folderFollowCarNumber, leadingZerosKey, carNumber, columnName) : MessageContents(folderFollowCarNumber, carNumber, columnName),
                                        abstractEvent: this, validationData: validationData, priority: 10));
                                else
                                    audioPlayer.playMessage(new QueuedMessage("frozen_order/follow_driver", delay + 6,
                                        secondsDelay: delay, messageFragments: leadingZeros ? MessageContents(folderFollowCarNumber, leadingZerosKey, carNumber) : MessageContents(folderFollowCarNumber, carNumber),
                                        abstractEvent: this, validationData: validationData, priority: 10));
                            }
                        }
                        else if (announceSCRLastFCYLapLane && cfod.AssignedColumn == FrozenOrderColumn.Left)
                            audioPlayer.playMessage(new QueuedMessage(columnName, delay + 6, secondsDelay: delay,
                                abstractEvent: this, validationData: validationData, priority: 10));
                        else if (announceSCRLastFCYLapLane && cfod.AssignedColumn == FrozenOrderColumn.Right)
                            audioPlayer.playMessage(new QueuedMessage(columnName, delay + 6, secondsDelay: delay,
                                abstractEvent: this, validationData: validationData, priority: 10));
                    }
                    else if (this.newFrozenOrderAction == FrozenOrderAction.AllowToPass)
                    {
                        int delay = Utilities.random.Next(1, 4);
                        if ((canReadDriverToFollow && Utilities.random.Next(0, 11) > 1) // Randomly, announce message without name.
                            || shouldFollowSafetyCar)
                        {
                            if (!useCarNumber || shouldFollowSafetyCar)
                                audioPlayer.playMessage(new QueuedMessage(!shouldFollowSafetyCar ? "frozen_order/allow_driver_to_pass" : "frozen_order/allow_safety_car_to_pass",
                                    delay + 6, secondsDelay: delay, messageFragments: MessageContents(folderAllow, usableDriverNameToFollow, folderToPass),
                                    abstractEvent: this, validationData: validationData, priority: 10));
                            else
                                audioPlayer.playMessage(new QueuedMessage("frozen_order/allow_driver_to_pass",
                                    delay + 6, secondsDelay: delay, messageFragments:
                                    leadingZeros ? MessageContents(folderAllowCarNumber, leadingZerosKey, carNumber, folderToPass) : MessageContents(folderAllowCarNumber, carNumber, folderToPass),
                                    abstractEvent: this, validationData: validationData, priority: 10));
                        }
                        else
                            audioPlayer.playMessage(new QueuedMessage(folderYoureAheadOfAGuyYouShouldBeFollowing, delay + 6, secondsDelay: delay, abstractEvent: this,
                                validationData: validationData, priority: 10));
                    }
                    else if (this.newFrozenOrderAction == FrozenOrderAction.CatchUp)
                    {
                        int delay = Utilities.random.Next(1, 4);
                        if (canReadDriverToFollow && Utilities.random.Next(0, 11) > 1  // Randomly, announce message without name.
                            || shouldFollowSafetyCar)
                        { 
                            if (!useCarNumber || shouldFollowSafetyCar)
                                audioPlayer.playMessage(new QueuedMessage(!shouldFollowSafetyCar ? "frozen_order/catch_up_to_driver" : "frozen_order/catch_up_to_safety_car",
                                    delay + 6, secondsDelay: delay, messageFragments: MessageContents(folderCatchUpTo, usableDriverNameToFollow),
                                    abstractEvent: this, validationData: validationData, priority: 10));
                            else
                                audioPlayer.playMessage(new QueuedMessage("frozen_order/catch_up_to_driver",
                                    delay + 6, secondsDelay: delay, messageFragments:
                                    leadingZeros ? MessageContents(folderCatchUpToCarNumber, leadingZerosKey, carNumber) : MessageContents(folderCatchUpToCarNumber, carNumber),
                                    abstractEvent: this, validationData: validationData, priority: 10));
                        }
                        else
                            audioPlayer.playMessage(new QueuedMessage(folderYouNeedToCatchUpToTheGuyAhead, delay + 6, secondsDelay: delay, abstractEvent: this,
                                validationData: validationData, priority: 10));
                    }
                    else if (this.newFrozenOrderAction == FrozenOrderAction.PassSafetyCar)
                    {
                        int delay = Utilities.random.Next(1, 4);
                        audioPlayer.playMessage(new QueuedMessage(useAmericanTerms ? folderPassThePaceCar : folderPassTheSafetyCar, delay + 6, secondsDelay: delay, abstractEvent: this,
                            validationData: validationData, priority: 10));
                    }
                }
            }
            else if (cfodp == FrozenOrderPhase.FormationStanding
                && cfod.Action != FrozenOrderAction.None)
            {
                string columnName = null;
                if (cfod.AssignedColumn != FrozenOrderColumn.None)
                {
                    if (useOvalLogic)
                        columnName = cfod.AssignedColumn == FrozenOrderColumn.Left ? folderInTheInsideColumn : folderInTheOutsideColumn;
                    else
                        columnName = cfod.AssignedColumn == FrozenOrderColumn.Left ? folderInTheLeftColumn : folderInTheRightColumn;
                }
                if (!this.formationStandingStartAnnounced && cgs.SessionData.SessionRunningTime > 10)
                {
                    this.formationStandingStartAnnounced = true;
                    var isStartingFromPole = cfod.AssignedPosition == 1;
                    int delay = Utilities.random.Next(0, 3);
                    if (isStartingFromPole)
                    {
                        if (columnName == null)
                            audioPlayer.playMessage(new QueuedMessage(folderWereStartingFromPole, 6, abstractEvent: this));
                        else
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/youre_starting_from_pole_in_column", delay + 6, secondsDelay: delay,
                                    messageFragments: MessageContents(folderWereStartingFromPole, columnName), abstractEvent: this, priority: 10));
                    }
                    else
                    {
                        if (columnName == null)
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/youre_starting_from_pos", delay + 6, secondsDelay: delay,
                                    messageFragments: MessageContents(folderWeStartingFromPosition, cfod.AssignedPosition), abstractEvent: this, priority: 10));
                        else
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/youre_starting_from_pos_row_in_column", delay + 6, secondsDelay: delay, 
                                    messageFragments: MessageContents(folderWeStartingFromPosition, cfod.AssignedPosition, folderRow, cfod.AssignedGridPosition, columnName),
                                    abstractEvent: this, priority: 10));
                    }
                }

                if (!this.formationStandingPreStartReminderAnnounced
                    && cgs.SessionData.SectorNumber == 3
                    && cgs.PositionAndMotionData.DistanceRoundTrack > (cgs.SessionData.TrackDefinition.trackLength - FrozenOrderMonitor.DIST_TO_START_TO_ANNOUNCE_POS_REMINDER))
                {
                    this.formationStandingPreStartReminderAnnounced = true;
                    var isStartingFromPole = cfod.AssignedPosition == 1;
                    int delay = Utilities.random.Next(0, 3);
                    if (isStartingFromPole)
                    {
                        if (columnName == null)
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/get_ready_starting_from_pole", delay + 6, secondsDelay: delay,
                                    messageFragments: MessageContents(LapCounter.folderGetReady, folderWereStartingFromPole), abstractEvent: this, priority: 10));
                        else
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/get_ready_starting_from_pole_in_column", delay + 6, secondsDelay: delay,
                                    messageFragments: MessageContents(LapCounter.folderGetReady, folderWereStartingFromPole, columnName), abstractEvent: this, priority: 10));
                    }
                    else
                    {
                        if (columnName == null)
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/get_ready_youre_starting_from_pos", delay + 6, secondsDelay: delay,
                                    messageFragments: MessageContents(LapCounter.folderGetReady, folderWeStartingFromPosition, cfod.AssignedPosition), abstractEvent: this, priority: 10));
                        else
                            audioPlayer.playMessage(new QueuedMessage("frozen_order/get_ready_youre_starting_from_pos_row_in_column", delay+ 6, secondsDelay: delay,
                                    messageFragments: MessageContents(LapCounter.folderGetReady, folderWeStartingFromPosition, cfod.AssignedPosition, folderRow, cfod.AssignedGridPosition, columnName), 
                                    abstractEvent: this, priority: 10));
                    }
                }
            }

            // Announce SC speed.
            if (pfod.SafetyCarSpeed == -1.0f && cfod.SafetyCarSpeed != -1.0f)
            {
                var kmPerHour = cfod.SafetyCarSpeed * 3.6f;
                var messageFragments = new List<MessageFragment>();
                if (!GlobalBehaviourSettings.useMetric)
                {
                    messageFragments.Add(MessageFragment.Text(FrozenOrderMonitor.folderPaceCarSpeedIs));
                    var milesPerHour = kmPerHour * 0.621371f;
                    messageFragments.Add(MessageFragment.Integer((int)Math.Round(milesPerHour), false));
                    messageFragments.Add(MessageFragment.Text(FrozenOrderMonitor.folderMilesPerHour));
                }
                else
                {
                    messageFragments.Add(MessageFragment.Text(FrozenOrderMonitor.folderSafetyCarSpeedIs));
                    messageFragments.Add(MessageFragment.Integer((int)Math.Round(kmPerHour), false));
                    messageFragments.Add(MessageFragment.Text(FrozenOrderMonitor.folderKilometresPerHour));
                }
                int delay = Utilities.random.Next(10, 16);
                audioPlayer.playMessage(new QueuedMessage("frozen_order/pace_car_speed", delay + 6, secondsDelay: delay, messageFragments: messageFragments, abstractEvent: this, priority: 10));
            }

            // Announce SC left.
            if (pfod.SafetyCarSpeed != -1.0f && cfod.SafetyCarSpeed == -1.0f)
            {
                if (useAmericanTerms)
                    audioPlayer.playMessage(new QueuedMessage(folderPaceCarJustLeft, 10, abstractEvent: this));
                else
                    audioPlayer.playMessage(new QueuedMessage(folderSafetyCarJustLeft, 10, abstractEvent: this));
            }
                        
            if (cgs.SafetyCarData.fcySafetyCarCallsEnabled && (cfodp == FrozenOrderPhase.FullCourseYellow) && 
                pgs.SafetyCarData.isOnTrack && !cgs.SafetyCarData.isOnTrack)
            {
                if (useAmericanTerms)
                    audioPlayer.playMessage(new QueuedMessage(folderPaceCarJustLeft, 10, abstractEvent: this));
                else
                    audioPlayer.playMessage(new QueuedMessage(folderSafetyCarJustLeft, 10, abstractEvent: this));
            }

            // For fast rolling, do nothing for now.
        }

        // double check if we can use leading zero(s)
        private Boolean ShouldUseLeadingZeros(int carNumber, HashSet<string> carNumbers)
        {
            // check to see if we want to add a leading zero - the 'correct' way to announce the car number is to always honour the leading
            // zeros, but in cases where there's only 1 leading zero and the number isn't ambiguous, we override this. This is because
            // "zero twenty three" sounds a bit weird if there's no car number 23 as well. We will always honour numbers with 2
            // leading zeros like 007
            int copiesOfNumber = 0;
            foreach (string carNumberString in carNumbers)
            {
                if (carNumberString != "-1")
                {
                    int parsedNumber = int.Parse(carNumberString);
                    if (parsedNumber == carNumber)
                    {
                        copiesOfNumber++;
                        if (copiesOfNumber > 1)
                        {
                            break;
                        }
                    }
                }
            }
            // only allow the leadingZero to be used if there's >1 copy if this number
            return copiesOfNumber > 1;
        }

    }
}
