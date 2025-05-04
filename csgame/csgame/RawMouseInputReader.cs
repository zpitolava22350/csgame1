using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace csgame {
    public static class RawMouseInputReader {
        // Constants for raw input
        private const int RIM_TYPEMOUSE = 0;
        private const uint RID_INPUT = 0x10000003;  // from winuser.h
        private const int WM_INPUT = 0x00FF;
        private const int GWL_WNDPROC = -4;

        // P/Invoke declarations
        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICE {
            public ushort UsagePage;
            public ushort Usage;
            public RawInputDeviceFlags Flags;
            public IntPtr Target;
        }
        [Flags]
        private enum RawInputDeviceFlags: uint {
            None = 0,
            NoLegacy = 0x30, // RIDEV_NOLEGACY: ignore legacy mouse msgs
                             // (Other flags like InputSink can be defined if needed)
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(
            [In] RAWINPUTDEVICE[] pRawInputDevices,
            uint uiNumDevices,
            uint cbSize);

        [DllImport("user32.dll")]
        private static extern int GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader);

        // Raw input data structures
        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTHEADER {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct RAWMOUSE {
            public ushort usFlags;
            public uint ulButtons;      // union: contains flags + data
            public uint ulRawButtons;
            public int lLastX;          // Raw movement in X
            public int lLastY;          // Raw movement in Y
            public uint ulExtraInformation;
        }

        // Delegates and handles for subclassing WndProc
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private static IntPtr _oldWndProc = IntPtr.Zero;
        private static WndProcDelegate _wndProcDelegate = WndProc; // keep reference alive

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(
            IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Define the delegate type for the callback
        public delegate void MouseMovedHandler(int deltaX, int deltaY);

        // Store the callback in a static field
        private static MouseMovedHandler? _callback;

        public static void SetCallback(MouseMovedHandler callback) {
            _callback = callback;
        }

        /// <summary>
        /// Call once to start receiving raw mouse input. Pass in the Win32 window handle of the MonoGame window.
        /// (For example, call RawMouseInputReader.Initialize(this.Window.Handle) in Game1.Initialize().)
        /// </summary>
        public static void Initialize(IntPtr hWnd) {
            // Register the mouse for raw input (Generic desktop mouse, ignore legacy msgs)
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
            rid[0].UsagePage = 0x01;   // Generic desktop
            rid[0].Usage = 0x02;   // Mouse
            rid[0].Flags = RawInputDeviceFlags.NoLegacy;
            rid[0].Target = hWnd;
            Debug.WriteLine($"Window Handle: {hWnd}");
            bool success = RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0]));
            if (!success) {
                Debug.WriteLine("Failed to register raw input device.");
                int error = Marshal.GetLastWin32Error();
                Debug.WriteLine("RegisterRawInputDevices failed with error code: " + error);
            }

            // Subclass the window to intercept WM_INPUT
            IntPtr newWndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
            _oldWndProc = SetWindowLongPtr(hWnd, GWL_WNDPROC, newWndProcPtr);
        }

        // Our window procedure to catch WM_INPUT
        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) {
            if (msg == WM_INPUT) {
                // First call to GetRawInputData to get buffer size
                uint dataSize = 0;
                GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref dataSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));
                if (dataSize > 0) {
                    IntPtr buffer = Marshal.AllocHGlobal((int)dataSize);
                    try {
                        // Second call to get the actual raw input data
                        if (GetRawInputData(lParam, RID_INPUT, buffer, ref dataSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) == (int)dataSize) {
                            // Marshal the header
                            RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(buffer);
                            if (header.dwType == RIM_TYPEMOUSE) {
                                // Marshal the RAWMOUSE (located just after header)
                                IntPtr ptr = IntPtr.Add(buffer, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
                                RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(ptr);
                                _callback.Invoke(mouse.lLastX, mouse.lLastY);
                            }
                        }
                    } finally {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
            }
            // Call original WndProc for other messages
            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
        }
    }
}
