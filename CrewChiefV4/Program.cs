﻿using CrewChiefV4.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrewChiefV4
{
    static class Program
    {
        private static Dictionary<String, IntPtr> processorAffinities = new Dictionary<String, IntPtr> {
            { "-cpu1", new IntPtr(0x0001) },
            { "-cpu2", new IntPtr(0x0002) },
            { "-cpu3", new IntPtr(0x0004) },
            { "-cpu4", new IntPtr(0x0008) },
            { "-cpu5", new IntPtr(0x0010) },
            { "-cpu6", new IntPtr(0x0020) },
            { "-cpu7", new IntPtr(0x0040) },
            { "-cpu8", new IntPtr(0x0080) }
        };
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set Invariant Culture for all threads as default.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Set Invariant Culture for current thead.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            String[] commandLineArgs = Environment.GetCommandLineArgs();
            Boolean allowMultipleInst = false;
            String commandPassed = null;
            if (commandLineArgs != null)
            {
                var argIdx = 0;
                foreach (String commandLineArg in commandLineArgs)
                {
                    IntPtr pArg = IntPtr.Zero;
                    if (processorAffinities.TryGetValue(commandLineArg.ToLowerInvariant(), out pArg))
                    {
                        try
                        {
                            var process = System.Diagnostics.Process.GetCurrentProcess();
                            // Set Core 
                            process.ProcessorAffinity = pArg;
                            Console.WriteLine("Set process core affinity to " + commandLineArg);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Failed to set process affinity");
                        }
                    }
                    if (commandLineArg.Equals("-sound_test", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MainWindow.soundTestMode = true;
                    }
                    if (commandLineArg.Equals("-nodevicescan", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MainWindow.disableControllerReacquire = true;
                    }
                    if (commandLineArg.StartsWith("-c_", StringComparison.InvariantCultureIgnoreCase))
                    {
                        commandPassed = commandLineArg;
                    }
                    // Internal.
                    if (commandLineArg.Equals("-multi", StringComparison.InvariantCultureIgnoreCase))
                    {
                        allowMultipleInst = true;
                    }
                    if (commandLineArg.Equals("-profile_mode", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MainWindow.profileMode = true;
                    }

                    ++argIdx;
                }
                if (!allowMultipleInst)
                {
                    if (!string.IsNullOrWhiteSpace(commandPassed))
                    {
                        if (CommandManager.ProcesssCommand(commandPassed))
                            return;  // This is execution to perform command, exit.
                    }
                    try
                    {
                        if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                        {
                            System.Diagnostics.Process.GetCurrentProcess().Kill();
                        }
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());

            var watch = System.Diagnostics.Stopwatch.StartNew();
            ThreadManager.WaitForRootThreadsShutdown();
            watch.Stop();

            Debug.WriteLine("Root threads took: " + watch.ElapsedTicks * 1000 / System.Diagnostics.Stopwatch.Frequency + "ms to shutdown");

            watch = System.Diagnostics.Stopwatch.StartNew();
            ThreadManager.WaitForTemporaryThreadsShutdown();
            watch.Stop();
            Debug.WriteLine("Temporary threads took: " + watch.ElapsedTicks * 1000 / System.Diagnostics.Stopwatch.Frequency + "ms to shutdown");

            watch = System.Diagnostics.Stopwatch.StartNew();
            ThreadManager.WaitForResourceThreadsShutdown();
            watch.Stop();
            Debug.WriteLine("Resource threads took: " + watch.ElapsedTicks * 1000 / System.Diagnostics.Stopwatch.Frequency + "ms to shutdown");

            watch = System.Diagnostics.Stopwatch.StartNew();
            GlobalResources.Dispose();
            watch.Stop();
            Debug.WriteLine("Resource Disposal took: " + watch.ElapsedTicks * 1000 / System.Diagnostics.Stopwatch.Frequency + "ms");

            if (AudioPlayer.playWithNAudio)
                Debug.Assert(SoundCache.activeSoundPlayerObjects == 0);
        }
    }
}
