using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrewChiefV4.VirtualReality
{
    /// <summary>
    /// Configuration for the VR Settings Form
    /// </summary>
    public class VROverlayConfiguration
    {
        public string HighlightColor { get; set; } = "#FFFF00"; //yellow

        public List<VROverlaySettings.HotKeyMapping> HotKeys { get; set; } = new List<VROverlaySettings.HotKeyMapping>();
        public List<VROverlayWindow> Windows { get; set; } = new List<VROverlayWindow>();

        /// <summary>
        /// File location where this instance was loaded
        /// </summary>
        [JsonIgnore]
        public FileInfo FileInfo { get; set; }

        static readonly DirectoryInfo MyDocuments;
        static readonly FileInfo OldSettingsFile;
        static readonly FileInfo DefaultFile;

        static VROverlayConfiguration()
        {
            MyDocuments = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4"));
            OldSettingsFile = new FileInfo(Path.Combine(MyDocuments.FullName, "vr_overlay_windows.json"));
            DefaultFile = new FileInfo(Path.Combine(MyDocuments.FullName, "CrewChiefV4.vrconfig.json"));
        }        

        private void Save(FileInfo file)
        {
            file.Directory.TryCreate(Console.WriteLine);
            
            try
            {
                this.SerializeJson(file.FullName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error parsing {file.FullName}: {e.Message}");
            }
        }

        /// <summary>
        /// Save the settings file to the Default Location
        /// </summary>
        public void Save()
        {
            Save(FileInfo);
        }

        /// <summary>
        /// Load the settings file from the default location
        /// </summary>
        /// <returns></returns>
        public static VROverlayConfiguration FromFile()
        {
            return FromFile(DefaultFile);
        }


        /// <summary>
        /// Helper function to handle migration from old file type
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static VROverlayConfiguration FromFile(FileInfo fileInfo)
        {
            if (OldSettingsFile.Exists)
            {
                // migrate the old setting file to the new format
                var oldWindows = OldSettingsFile.TryDeserializeJson<List<VROverlayWindow>>(Console.WriteLine);
                if (oldWindows == null)
                {
                    oldWindows = new List<VROverlayWindow>();
                }
                new VROverlayConfiguration
                {
                    Windows = oldWindows,
                    HotKeys = VROverlaySettings.HotKeyMapping.Default().ToList()
                }
                .Save(fileInfo);
                OldSettingsFile.Delete();
            }
            if (!fileInfo.Exists)
            {
                new VROverlayConfiguration
                {
                    Windows = new List<VROverlayWindow>(),
                    HotKeys = VROverlaySettings.HotKeyMapping.Default().ToList()
                }
                .Save(fileInfo);
            }

           var config = fileInfo.TryDeserializeJson<VROverlayConfiguration>(Console.WriteLine);
            
            if (config.HotKeys == null)
            {
                config.HotKeys = new List<VROverlaySettings.HotKeyMapping>();
            }
            if (config.Windows == null)
            {
                config.Windows = new List<VROverlayWindow>();
            }
            // remove any hotkeys with an unrecognized action
            foreach (VROverlaySettings.HotKeyMapping hotKeyMapping in config.HotKeys.Where(m => !m.IsValid()).ToList())
            {
                Console.WriteLine($"Unknown hotkey action '{hotKeyMapping.Id}', Hotkey will be ignored");
                config.HotKeys.Remove(hotKeyMapping);
            }

            config.FileInfo = fileInfo;
            return config;
        }
    }
}
