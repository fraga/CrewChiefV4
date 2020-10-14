﻿using CrewChiefV4.GameState;
using CrewChiefV4.PCars;
using CrewChiefV4.RaceRoom;
using CrewChiefV4.rFactor1;
using CrewChiefV4.assetto;
using System;
using CrewChiefV4.rFactor2;
using CrewChiefV4.iRacing;
using CrewChiefV4.PCars2;
using CrewChiefV4.F1_2018;
using CrewChiefV4.ACC;
using CrewChiefV4.F1_2019;
using CrewChiefV4.AMS2;
using CrewChiefV4.F1_2020;
using CrewChiefV4.RBR;
using CrewChiefV4.GTR2;
using CrewChiefV4.Dirt;

namespace CrewChiefV4
{
    class GameStateReaderFactory
    {
        private static GameStateReaderFactory INSTANCE = new GameStateReaderFactory();

        // the Reader objects may be used by other Threads, so the factory must cache them and return the same instance
        // when called.
        private PCarsUDPreader pcarsUDPreader;
        private PCars2UDPreader pcars2UDPreader;
        private PCarsSharedMemoryReader pcarsSharedMemoryReader;
        private PCars2SharedMemoryReader pcars2SharedMemoryReader;
        private R3ESharedMemoryReader r3eSharedMemoryReader;
        private RF1SharedMemoryReader rf1SharedMemoryReader;
        private RF2SharedMemoryReader rf2SharedMemoryReader;
        private ACSSharedMemoryReader ascSharedMemoryReader;
        private iRacingSharedMemoryReader iracingSharedMemoryReader;
        private F12018UDPreader f12018UDPReader;
        private ACCSharedMemoryReader accSharedMemoryReader;
        private F12019UDPreader f12019UDPReader;        
        private AMS2UDPreader ams2UDPReader;
        private AMS2SharedMemoryReader ams2SharedMemoryReader;
        private F12020UDPreader f12020UDPReader;
        private RBRSharedMemoryReader rbrSharedMemoryReader;
        private GTR2SharedMemoryReader gtr2SharedMemoryReader;
        private DirtUDPreader dirtUDPMemoryReader;

        public static GameStateReaderFactory getInstance()
        {
            return INSTANCE;
        }

        public GameDataReader getGameStateReader(GameDefinition gameDefinition)
        {
            lock (this)
            {
                switch (gameDefinition.gameEnum)
                {
                    case GameEnum.PCARS_NETWORK:
                        if (pcarsUDPreader == null)
                        {
                            pcarsUDPreader = new PCarsUDPreader();
                        }
                        return pcarsUDPreader;
                    case GameEnum.PCARS2_NETWORK:
                        if (pcars2UDPreader == null)
                        {
                            pcars2UDPreader = new PCars2UDPreader();
                        }
                        return pcars2UDPreader;
                    case GameEnum.PCARS_32BIT:
                    case GameEnum.PCARS_64BIT:                    
                        if (pcarsSharedMemoryReader == null)
                        {
                            pcarsSharedMemoryReader = new PCarsSharedMemoryReader();
                        }
                        return pcarsSharedMemoryReader;
                    case GameEnum.PCARS2:
                    case GameEnum.PCARS3:
                        if (pcars2SharedMemoryReader == null)
                        {
                            pcars2SharedMemoryReader = new PCars2SharedMemoryReader();
                        }
                        return pcars2SharedMemoryReader;
                    case GameEnum.RACE_ROOM:
                        if (r3eSharedMemoryReader == null)
                        {
                            r3eSharedMemoryReader = new R3ESharedMemoryReader();
                        }
                        return r3eSharedMemoryReader;
                    case GameEnum.RF1:
                        if (rf1SharedMemoryReader == null)
                        {
                            rf1SharedMemoryReader = new RF1SharedMemoryReader();
                        }
                        return rf1SharedMemoryReader;
                    case GameEnum.ASSETTO_64BIT:
                    case GameEnum.ASSETTO_32BIT:
                        if (ascSharedMemoryReader == null)
                        {
                            ascSharedMemoryReader = new ACSSharedMemoryReader();
                        }
                        return ascSharedMemoryReader;
                    case GameEnum.RF2_64BIT:
                        if (rf2SharedMemoryReader == null)
                        {
                            rf2SharedMemoryReader = new RF2SharedMemoryReader();
                        }
                        return rf2SharedMemoryReader;
                    case GameEnum.IRACING:
                        if (iracingSharedMemoryReader == null)
                        {
                            iracingSharedMemoryReader = new iRacingSharedMemoryReader();
                        }
                        return iracingSharedMemoryReader;
                    case GameEnum.F1_2018:
                        if (f12018UDPReader == null)
                        {
                            f12018UDPReader = new F12018UDPreader();
                        }
                        return f12018UDPReader;
                    case GameEnum.ACC:
                        if (accSharedMemoryReader == null)
                        {
                            accSharedMemoryReader = new ACCSharedMemoryReader();
                        }
                        return accSharedMemoryReader;
                    case GameEnum.F1_2019:
                        if (f12019UDPReader == null)
                        {
                            f12019UDPReader = new F12019UDPreader();
                        }
                        return f12019UDPReader;
                    case GameEnum.F1_2020:
                        if (f12020UDPReader == null)
                        {
                            f12020UDPReader = new F12020UDPreader();
                        }
                        return f12020UDPReader;
                    case GameEnum.AMS2:
                        if (ams2SharedMemoryReader == null)
                        {
                            ams2SharedMemoryReader = new AMS2SharedMemoryReader();
                        }
                        return ams2SharedMemoryReader;
                    case GameEnum.AMS2_NETWORK:
                        if (ams2UDPReader == null)
                        {
                            ams2UDPReader = new AMS2UDPreader();
                        }
                        return ams2UDPReader;
                    case GameEnum.RBR:
                        if (rbrSharedMemoryReader == null)
                        {
                            rbrSharedMemoryReader = new RBRSharedMemoryReader();
                        }
                        return rbrSharedMemoryReader;
                    case GameEnum.GTR2:
                        if (gtr2SharedMemoryReader == null)
                        {
                            gtr2SharedMemoryReader = new GTR2SharedMemoryReader();
                        }
                        return gtr2SharedMemoryReader;
                    case GameEnum.DIRT:
                    case GameEnum.DIRT_2:
                        if (dirtUDPMemoryReader == null)
                        {
                            dirtUDPMemoryReader = new DirtUDPreader();
                        }
                        return dirtUDPMemoryReader;
                    default:
                        return new DummyGameDataReader();
                }
            }
            return null;
        }

        public GameStateMapper getGameStateMapper(GameDefinition gameDefinition)
        {
            switch (gameDefinition.gameEnum)
            {
                case GameEnum.PCARS_NETWORK:
                case GameEnum.PCARS_32BIT:
                case GameEnum.PCARS_64BIT:
                    return new PCarsGameStateMapper();
                case GameEnum.PCARS2_NETWORK:
                case GameEnum.PCARS2:
                case GameEnum.PCARS3:
                    return new PCars2GameStateMapper();
                case GameEnum.RACE_ROOM:
                    return new R3EGameStateMapper();
                case GameEnum.RF1:
                    return new RF1GameStateMapper();
                case GameEnum.ASSETTO_64BIT:
                case GameEnum.ASSETTO_32BIT:
                    return new ACSGameStateMapper();
                case GameEnum.RF2_64BIT:
                    return new RF2GameStateMapper();
                case GameEnum.IRACING:
                    return new iRacingGameStateMapper();
                case GameEnum.F1_2018:
                    return new F12018GameStateMapper();
                case GameEnum.ACC:
                    return new ACCGameStateMapper();
                case GameEnum.F1_2019:
                    return new F12019GameStateMapper();
                case GameEnum.F1_2020:
                    return new F12020GameStateMapper();
                case GameEnum.AMS2:
                case GameEnum.AMS2_NETWORK:
                    return new AMS2GameStateMapper();
                case GameEnum.RBR:
                    return new RBRGameStateMapper();
                case GameEnum.GTR2:
                    return new GTR2GameStateMapper();
                case GameEnum.DIRT:
                case GameEnum.DIRT_2:
                    return new DirtGameStateMapper();
                default:
                    Console.WriteLine("No mapper is defined for GameDefinition " + gameDefinition.friendlyName);
                    return new DummyGameStateMapper();
            }
        }
    }
}
