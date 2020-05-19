using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CrewChiefV4
{
    class Win32Stuff
    {

        #region Class Variables
        // Window Styles
        [Flags]
        internal enum WindowStyle : uint
        {
            Overlapped = 0x00000000,
            Tiled = 0x00000000,
            MaximizeBox = 00010000,
            Tabstop = 0x00010000,
            Group = 0x00020000,
            MinimizeBox = 0x00020000,
            Sizebox = 0x00040000,
            ThickFrame = 0x00040000,
            SysMenu = 0x00080000,
            HScroll = 0x00100000,
            VScroll = 0x00200000,
            DlgFrame = 0x00400000,
            Border = 0x00800000,
            Caption = DlgFrame | Border,
            OverlappedWindow = Overlapped | Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox,
            TiledWindow = Overlapped | Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox,
            Maximize = 0x01000000,
            ClipChildren = 0x02000000,
            ClipSiblings = 0x04000000,
            Disabled = 0x08000000,
            Visible = 0x10000000,
            Iconic = 0x20000000,
            Minimize = 0x20000000,
            Child = 0x40000000,
            ChildWindow = 0x40000000,
            Popup = 0x80000000,
            PopupWindow = Popup | Border | SysMenu
        }
        public const Int32 CURSOR_SHOWING = 0x00000001;
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public WindowStyle dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler) : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }

        }
        public enum IDC_STANDARD_CURSORS
        {
            IDC_ARROW = 32512,
            IDC_IBEAM = 32513,
            IDC_WAIT = 32514,
            IDC_CROSS = 32515,
            IDC_UPARROW = 32516,
            IDC_SIZE = 32640,
            IDC_ICON = 32641,
            IDC_SIZENWSE = 32642,
            IDC_SIZENESW = 32643,
            IDC_SIZEWE = 32644,
            IDC_SIZENS = 32645,
            IDC_SIZEALL = 32646,
            IDC_NO = 32648,
            IDC_HAND = 32649,
            IDC_APPSTARTING = 32650,
            IDC_HELP = 32651
        }
        #endregion


        #region Class Functions
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", EntryPoint = "DrawIconEx")]
        public static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon,
               int cxWidth, int cyHeight, int istepIfAniCur, IntPtr hbrFlickerFreeDraw,int diFlags);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT lpRect);
        

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);


        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static IntPtr FindWindow(string caption)
        {
            return FindWindow(String.Empty, caption);
        }
        /// <summary>
        ///     Determines the visibility state of the specified window.
        ///     <para>
        ///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633530%28v=vs.85%29.aspx for more
        ///     information. For WS_VISIBLE information go to
        ///     https://msdn.microsoft.com/en-us/library/windows/desktop/ms632600%28v=vs.85%29.aspx
        ///     </para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br />A handle to the window to be tested.</param>
        /// <returns>
        ///     <c>true</c> or the return value is nonzero if the specified window, its parent window, its parent's parent
        ///     window, and so forth, have the WS_VISIBLE style; otherwise, <c>false</c> or the return value is zero.
        /// </returns>
        /// <remarks>
        ///     The visibility state of a window is indicated by the WS_VISIBLE[0x10000000L] style bit. When
        ///     WS_VISIBLE[0x10000000L] is set, the window is displayed and subsequent drawing into it is displayed as long as the
        ///     window has the WS_VISIBLE[0x10000000L] style. Any drawing to a window with the WS_VISIBLE[0x10000000L] style will
        ///     not be displayed if the window is obscured by other windows or is clipped by its parent window.
        /// </remarks>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hWnd);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary> Get the text for the window pointed to by hWnd </summary>
        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        /// <summary> Find all windows that match the given filter </summary>
        /// <param name="filter"> A delegate that returns true for windows
        ///    that should be returned and false for windows that should
        ///    not be returned </param>
        public static List<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary> Find all windows that contain the given title text </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
        public static List<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return GetWindowText(wnd).Contains(titleText);
            });
        }

        /// <summary> Find all windows that contain the given title text </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
        public static IEnumerable<IntPtr> FindWindowsWithSize()
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return IsWindowVisible(wnd);
            });
        }
        // find windows that are visible and have a title
        public static List<IntPtr> FindWindows()
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return IsWindowVisible(wnd) && !string.IsNullOrWhiteSpace(Win32Stuff.GetWindowText(wnd));
            });
        }
        #endregion
    }

}