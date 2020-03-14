using CrewChiefV4.Events;
using GameOverlay.Drawing;
using GameOverlay.PInvoke;
using GameOverlay.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace CrewChiefV4.Overlay
{
    public class SubtitleOverlay
    {
        #region overlay settings
        public class SubtitleOverlaySettings : OverlaySettings
        {
            public int maxSubtitlehistory = 4;
            public int windowWidth = 600;
            public new int fontSize = 16;
            public new bool fontBold = true;
            public new int windowX = (Screen.PrimaryScreen.Bounds.Width / 2) - 350;
            public new int windowY = 50;
            public new string activeColorScheme = "Transparent";

        }
        #endregion
        private readonly GraphicsWindow overlayWindow;
        private Font font;
        private Font fontBold;
        private Font fontControls;
        private SolidBrush fontBrush;
        private SolidBrush backgroundBrush;
        private SolidBrush transparentBrush;
        public static SubtitleOverlaySettings settings = null;
        public static ColorScheme colorScheme = null;
        public static ColorScheme defaultColorScheme = null;
        public static ColorScheme colorSchemeTransparent = null;

        public static bool shown = UserSettings.GetUserSettings().getBoolean("enable_subtitle_overlay");
        private Boolean cleared = true;
        public bool inputsEnabled = false;
        public bool keepWindowActiveBackUpOnClose = false;
        public int windowHeightBackUpOnClose = -1;

        Dictionary<string, OverlayElement> overlayElements = new Dictionary<string, OverlayElement>();
        static readonly string overlayFileName = "subtitle_overlay.json";

        private string tileBarName = "overlay_titlebar";
        private string subtitleOverlayName = "subtitle_overlay";
        private string displayModeBoxName = "display_mode";

        private OverlayElement titleBar = null;
        private OverlayElement subtitleElement = null;
        private OverlayElement displayModeControlBox = null;

        private int maxDisplayLines = 0;
        float messuredFontHeight = 0;
        float messuredFontWidth = 0;
        int maxCharInSubtitleString = 0;
        private float subtitleTextBoxHeight = 0;
        public SubtitleOverlay()
        {
            // initialize a new Graphics object
            // GraphicsWindow will do the remaining initialization            
            settings = OverlaySettings.loadOverlaySetttings<SubtitleOverlaySettings>(overlayFileName);
            if (settings.colorSchemes == null || settings.colorSchemes.Count == 0)
            {
                settings.colorSchemes = new List<ColorScheme>() { OverlaySettings.defaultCrewChiefColorScheme, OverlaySettings.windowsGrayColorScheme, OverlaySettings.transparentColorScheme };
            }
            colorScheme = settings.colorSchemes.FirstOrDefault(s => s.name == settings.activeColorScheme);
            defaultColorScheme = OverlaySettings.defaultCrewChiefColorScheme;
            if (colorScheme == null)
            {
                colorScheme = defaultColorScheme;
            }
            var graphics = new Graphics
            {
                PerPrimitiveAntiAliasing = true,
                TextAntiAliasing = settings.textAntiAliasing,
                UseMultiThreadedFactories = false,
                VSync = settings.vSync,
                WindowHandle = IntPtr.Zero
            };

            // it is important to set the window to visible (and topmost) if you want to see it!
            overlayWindow = new GraphicsWindow(graphics)
            {
                IsTopmost = true,
                IsVisible = true,
                IsAppWindow = UserSettings.GetUserSettings().getBoolean("make_overlay_app_window"),
                FPS = settings.windowFPS,
                X = settings.windowX,
                Y = settings.windowY,
                Width = settings.windowWidth,
                Height = settings.windowHeight,
                Title = "CrewChief Subtitles",
                ClassName = "CrewChief_Subtitles",
            };

            overlayWindow.SetupGraphics += overlayWindow_SetupGraphics;
            overlayWindow.DestroyGraphics += overlayWindow_DestroyGraphics;
            overlayWindow.DrawGraphics += overlayWindow_DrawGraphics;

        }
        public void Dispose()
        {
            // you do not need to dispose the Graphics surface
            overlayWindow.Dispose();
        }

        public void Run()
        {
            // creates the window and setups the graphics
            overlayWindow.StartThread();
        }
        private void overlayWindow_SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            var gfx = e.Graphics;

            // creates a simple font with no additional style
            try
            {
                font = gfx.CreateFont(settings.fontName, settings.fontSize, settings.fontBold, settings.fontItalic);
                if (font == null)
                {
                    font = gfx.CreateFont("Arial", 12);
                }
                fontBold = gfx.CreateFont(settings.fontName, settings.fontSize, true, false);
                fontControls = gfx.CreateFont(settings.fontName, 12, false, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing " + settings.fontName + ": " + ex.Message);
            }
            fontBrush = gfx.CreateSolidBrush(colorScheme.fontColor);
            backgroundBrush = gfx.CreateSolidBrush(colorScheme.backgroundColor);
            transparentBrush = gfx.CreateSolidBrush(Color.Transparent);
            maxDisplayLines = settings.maxSubtitlehistory == -1 || settings.maxSubtitlehistory > 10 ? 10 : settings.maxSubtitlehistory;
            messuredFontHeight = font.MeasureString("H", font.FontSize).Height;
            messuredFontWidth = font.MeasureString("H", font.FontSize).Width;
            subtitleTextBoxHeight = messuredFontHeight * (maxDisplayLines);
            maxCharInSubtitleString = (int)Math.Floor((double)settings.windowWidth / ((double)messuredFontWidth) * 1.5);
            

            titleBar = overlayElements[tileBarName] = new OverlayHeader(gfx, "CrewChief Subtitles", fontBold, new Rect(0, 0, overlayWindow.Width, 20), defaultColorScheme, overlayWindow, OnEnableUserInput, OnButtonClosed, OnSavePosition, initialEnabled: true);
            titleBar.AddChildElement(new ElementCheckBox(gfx, "Enable Input", fontControls, new Rect(135, 3, 14, 14), defaultColorScheme, initialEnabled: true));
            titleBar.AddChildElement(new ElementButton(gfx, "ButtonClose", font, new Rect(overlayWindow.Width - 18, 3, 14, 14), defaultColorScheme));
            titleBar.AddChildElement(new ElementButton(gfx, "Save window position", fontControls, new Rect(overlayWindow.Width - 160, 3, 130, 14), defaultColorScheme));

            subtitleElement = overlayElements[subtitleOverlayName] = new ElementTextBox(gfx, subtitleOverlayName, font, new Rect(0, 20, overlayWindow.Width, subtitleTextBoxHeight),
            colorScheme, OnPhraseUpdate, initialEnableState: true);

            overlayWindow.Resize(settings.windowWidth, (int)subtitleElement.rectangle.Bottom);

            foreach (var element in overlayElements)
            {
                element.Value.initialize();
            }
            overlayWindow.OnWindowMessage += overlayWindow_OnWindowMessage;
        }

        private void overlayWindow_OnWindowMessage(object sender, OverlayWindowsMessage e)
        {
            try
            {                
                foreach (var element in overlayElements)
                {
                    element.Value.OnWindowMessage(e.WindowMessage, e.wParam, e.lParam);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Swollow
            }

        }
        bool shiftKeyReleased = false;
        private void overlayWindow_DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            if (Control.ModifierKeys != (Keys.Alt | Keys.Control))
            {
                shiftKeyReleased = true;
            }
            if (Control.ModifierKeys == (Keys.Alt | Keys.Control) && shiftKeyReleased)
            {
                titleBar.elementEnabled = !titleBar.elementEnabled;
                shiftKeyReleased = false;
                if(inputsEnabled && titleBar.elementEnabled)
                {
                    overlayWindow.ActivateWindow();
                }
                else
                {
                    overlayWindow.DeActivateWindow();
                }
            }
            if (!shown)
            {
                if (!cleared)
                {
                    // if we call this.overlayWindow.Hide() here, this method won't be called again so we can't
                    // unhide in this callback
                    keepWindowActiveBackUpOnClose = inputsEnabled;
                    windowHeightBackUpOnClose = overlayWindow.Height;
                    e.Graphics.ClearScene(Color.Transparent);
                    overlayWindow.Resize(0, 0);
                    cleared = true;
                }
                return;
            }
            else
            {
                if (windowHeightBackUpOnClose != -1)
                {
                    overlayWindow.Resize(settings.windowWidth, windowHeightBackUpOnClose);
                    windowHeightBackUpOnClose = -1;
                }
            }
            cleared = false;
            var gfx = e.Graphics;
            int thisFrameWindowHeight = 0;
            if (keepWindowActiveBackUpOnClose)
            {
                inputsEnabled = keepWindowActiveBackUpOnClose;
                overlayWindow.ActivateWindow();
                keepWindowActiveBackUpOnClose = false;
            }

            gfx.ClearScene(Color.Transparent);

            titleBar.updateInputs(overlayWindow.X, overlayWindow.Y, inputsEnabled);
            if(!titleBar.elementEnabled)
            {
                subtitleElement.rectangle.Y = 0;
            }                
            else
            {
                subtitleElement.rectangle.Y = titleBar.rectangle.Bottom;                
            }

            thisFrameWindowHeight += (int)subtitleElement.rectangle.Bottom;
            if (thisFrameWindowHeight != overlayWindow.Height)
            {
                overlayWindow.Resize(settings.windowWidth, thisFrameWindowHeight);
            }
            foreach (var elements in overlayElements)
            {
                elements.Value.drawElement();
            }
        }

        private void overlayWindow_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
            // you may want to dispose any brushes, fonts or images
        }
        // 
        IEnumerable<string> SplitToLines(string stringToSplit, int maximumLineLength)
        {
            var words = stringToSplit.Split(' ').Concat(new[] { "" });
            return
                words
                    .Skip(1)
                    .Aggregate(
                        words.Take(1).ToList(),
                        (a, w) =>
                        {
                            var last = a.Last();
                            while (last.Length > maximumLineLength)
                            {
                                a[a.Count() - 1] = last.Substring(0, maximumLineLength);
                                last = last.Substring(maximumLineLength);
                                a.Add(last);
                            }
                            var test = last + " " + w;
                            if (test.Length > maximumLineLength)
                            {
                                a.Add(w);
                            }
                            else
                            {
                                a[a.Count() - 1] = test;
                            }
                            return a;
                        });
        }
        private void OnPhraseUpdate(object sender, OverlayElementDrawUpdate e)
        {
            var lineOffsetY = e.rect.Top;
            lock(Audio.SubtitleManager.phraseBuffer)
            {
                if(Audio.SubtitleManager.phraseBuffer.Size < 1)
                {
                    string subtitle = "Use CTRL + ALT to show/hide the title bar";
                    if (subtitle.Length > maxCharInSubtitleString)
                    {
                        var phraseLines = SplitToLines(subtitle, maxCharInSubtitleString);
                        foreach (var line in phraseLines)
                        {
                            e.graphics.DrawText(font, fontBrush, e.rect.Left, lineOffsetY, line);
                            lineOffsetY += messuredFontHeight;
                        }
                    }
                    else
                    {
                        e.graphics.DrawText(font, fontBrush, e.rect.Left, lineOffsetY, subtitle);
                        lineOffsetY += messuredFontHeight;
                    }
                    return;
                }
                var phrases = Audio.SubtitleManager.phraseBuffer.ToArray();
                for (int i = 0; i < phrases.Length && i < maxDisplayLines && lineOffsetY < overlayWindow.Height; i++)
                {
                    string subtitle = phrases[i].voiceName + ": " + Audio.SubtitleManager.FirstLetterToUpper(phrases[i].phrase);
                    if (subtitle.Length > maxCharInSubtitleString)
                    {
                        var phraseLines = SplitToLines(subtitle, maxCharInSubtitleString);
                        foreach (var line in phraseLines)
                        {
                            e.graphics.DrawText(font, fontBrush, e.rect.Left, lineOffsetY, line);
                            lineOffsetY += messuredFontHeight;
                        }
                    }
                    else
                    {
                        e.graphics.DrawText(font, fontBrush, e.rect.Left, lineOffsetY, subtitle);
                        lineOffsetY += messuredFontHeight;
                    }
                }
            }
        }
        private void OnButtonClosed(object sender, OverlayElementClicked e)
        {
            this.overlayWindow.DeActivateWindow();
            shown = false;
        }
        // enable user to move the window
        private void OnEnableUserInput(object sender, OverlayElementClicked e)
        {
            inputsEnabled = e.enabled;
            if (inputsEnabled)
            {
                this.overlayWindow.ActivateWindow();
            }
            else
            {
                this.overlayWindow.DeActivateWindow();
            }
        }
        private void OnSavePosition(object sender, OverlayElementClicked e)
        {
            settings.windowX = overlayWindow.X;
            settings.windowY = overlayWindow.Y;
            OverlaySettings.saveOverlaySetttings(overlayFileName, settings);
        }
    }
}
