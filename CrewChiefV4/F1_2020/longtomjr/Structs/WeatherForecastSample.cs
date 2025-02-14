﻿
namespace F12020UdpNet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WeatherForecastSample
    {
        public byte m_sessionType;                     // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P, 5 = Q1
                                                       // 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ, 10 = R, 11 = R
                                                       // 12 = Time Trial
        public byte m_timeOffset;                      // Time in minutes the forecast is for
        public byte m_weather;                         // Weather - 0 = clear, 1 = light cloud, 2 = overcast
                                                       // 3 = light rain, 4 = heavy rain, 5 = storm
        public byte m_trackTemperature;                // Track temp. in degrees celsius
        public byte m_airTemperature;                  // Air temp. in degrees celsius
    }
}
