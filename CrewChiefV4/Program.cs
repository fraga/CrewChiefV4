using CrewChiefV4.Audio;
using CrewChiefV4.UserInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CrewChiefV4
{
    static class Program
    {
        private static Dictionary<String, IntPtr> processorAffinities = new Dictionary<String, IntPtr> {
            { "cpu1", new IntPtr(0x0001) },
            { "cpu2", new IntPtr(0x0002) },
            { "cpu3", new IntPtr(0x0004) },
            { "cpu4", new IntPtr(0x0008) },
            { "cpu5", new IntPtr(0x0010) },
            { "cpu6", new IntPtr(0x0020) },
            { "cpu7", new IntPtr(0x0040) },
            { "cpu8", new IntPtr(0x0080) }
        };
        public static Loading LoadingScreen;
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

            foreach (var affinity in processorAffinities)
            {
                if (CrewChief.CommandLine.Get(affinity.Key) != null)
                {
                    try
                    {
                        var process = System.Diagnostics.Process.GetCurrentProcess();
                        // Set Core
                        process.ProcessorAffinity = affinity.Value;
                        Console.WriteLine("Set process core affinity to " + affinity.Key);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to set process affinity");
                    }
                }
            }
            MainWindow.soundTestMode = CrewChief.CommandLine.Get("sound_test") != null;
            MainWindow.disableControllerReacquire = CrewChief.CommandLine.Get("nodevicescan") != null;

            // Internal.
            Boolean allowMultipleInst = CrewChief.CommandLine.Get("multi") != null;
            MainWindow.profileMode = CrewChief.CommandLine.Get("profile_mode") != null;

            if (!allowMultipleInst)
            {
                String commandPassed = CrewChief.CommandLine.GetCommandArg();
                if (commandPassed != null)
                {
                    if (CommandManager.ProcesssCommand(commandPassed))
                        return;  // This is execution to perform command, exit.
                }
                try
                {
                    var processes = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location));
                    if (processes.Count() > 1)
                    {
                        var result = MessageBox.Show("Retry to close the other one\nCancel to close this one",
                            "Crew Chief is already running",
                            MessageBoxButtons.RetryCancel);
                        if (result == DialogResult.Cancel)
                        {
                            System.Diagnostics.Process.GetCurrentProcess().Kill();
                        }
                        else // Maybe overkill but may let users kill a stuck process
                        {
                            var startTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
                            foreach (var process in processes)
                            {
                                if (process.StartTime != startTime)
                                {
                                    process.Kill();
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Shouldn't happen but belt and braces...
                    System.Environment.Exit(1);
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool showSplashScreen = false;
            try
            {
                showSplashScreen = UserSettings.GetUserSettings().getBoolean("show_splash_screen");
            }
            catch (Exception)
            {
                // ignore, if we've been unable to load the settings the UserSettings instance should have the 'broken' flag set at this point
            }
            if (showSplashScreen)
            {
                LoadSplashImage();
                LoadingScreen = new Loading();
                // Display form modelessly
                LoadingScreen.StartPosition = FormStartPosition.CenterScreen;
                LoadingScreen.FormBorderStyle = FormBorderStyle.None;
                LoadingScreen.Show();
            }

#if !DEBUG
            try
            {
                SharpDX.Configuration.EnableObjectTracking = true;
                SharpDX.ComObject.LogMemoryLeakWarning = msg => Console.Write(msg);
                
#endif
                MainWindow mw = new MainWindow();
                mw.MenuStrip(mw.exemplarFont); // Add the menu strip to the main window
                Application.Run(mw);
#if !DEBUG
            }
            catch (System.ObjectDisposedException e) 
            {
                // 'Cannot access a disposed object' after doRestart() has closed CC down
                Log.Error("This shouldn't happen");
            }
            catch (Exception e)
            {
                Utilities.ReportException(e, "UNKNOWN EXCEPTION", true);
            }
#endif

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

        // get the latest splash image and prepare it to be used on the next run (not this one)
        private static void LoadSplashImage()
        {
            // download the latest splash image in a thread
            // if our working file exists, move it to be our actual splash image
            try
            {
                if (!Directory.Exists(Loading.splashImageFolderPath))
                {
                    Directory.CreateDirectory(Loading.splashImageFolderPath);
                }
                if (File.Exists(Loading.tempSplashImagePath))
                {
                    if (File.Exists(Loading.splashImagePath))
                    {
                        File.Delete(Loading.splashImagePath);
                    }
                    File.Move(Loading.tempSplashImagePath, Loading.splashImagePath);
                }
            }
            catch (Exception)
            {
                // can't move it but it exists, so nuke it
                try
                {
                    File.Delete(Loading.tempSplashImagePath);
                }
                catch (Exception e) { Log.Exception(e); }
            }
            // refresh the image if we don't have one, and occasionally refresh anyway
            if (!File.Exists(Loading.splashImagePath) || new Random().NextDouble() > 0.9)
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    using (var client = new System.Net.WebClient())
                    {
                        try
                        {
                            client.DownloadFile(@"http://167.235.144.28/CrewChief_splash_image.png", Loading.tempSplashImagePath);
                        }
                        catch (Exception)
                        {
                            // ignore - no splash screen, doesn't matter
                        }
                    }
                }).Start();
            }
        }
    }
}
