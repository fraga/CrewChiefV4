﻿using System.Collections.Generic;

namespace ksBroadcastingNetwork.Structs
{
    public class CarInfo
    {
        public ushort CarIndex { get; }
        public byte CarModelType { get; internal set; }
        public string TeamName { get; internal set; }
        public int RaceNumber { get; internal set; }
        public byte CupCategory { get; internal set; }
        public int CurrentDriverIndex { get; internal set; }
        public ushort Nationality { get; internal set; }

        public IList<DriverInfo> Drivers { get; } = new List<DriverInfo>();

        public CarInfo(ushort carIndex)
        {
            CarIndex = carIndex;
        }

        internal void AddDriver(DriverInfo driverInfo)
        {
            Drivers.Add(driverInfo);
        }
    }
}
