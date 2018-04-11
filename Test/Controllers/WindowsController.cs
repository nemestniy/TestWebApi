using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace Test.Controllers
{
    public class WindowsController : Controller
    {

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible ( IntPtr hWnd );
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private struct SIZE
        {
            public int width;
            public int height;
        }



        public string Index(string name)
        {
            Process[] list = Process.GetProcesses();
            var json = new List<Window>();
            if (name == null)
            {
                foreach (Process proc in list)
                {
                    IntPtr ptr = FindWindow(null, proc.MainWindowTitle);
                    RECT rect = new RECT();
                    SIZE size = new SIZE();
                    if (proc.MainWindowTitle != "" && IsWindowVisible(ptr) && proc.MainWindowHandle!=IntPtr.Zero) {
                        GetWindowRect(ptr, ref rect);
                        size = GetWindowSize(rect);
                        json.Add(new Window{ name = proc.MainWindowTitle, x = rect.Left, y=rect.Top, width=size.width, height =size.height });
                    }
                }
                string jsonString = JsonConvert.SerializeObject(json, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented });  
                return jsonString;
            }
            else
            {
                RECT rect = new RECT();
                SIZE size = new SIZE();
                IntPtr window = FindWindow(null, name);
                if (IsWindowVisible(window) && window != IntPtr.Zero && GetWindowText(window) != null)
                {
                    GetWindowRect(window, ref rect);
                    size = GetWindowSize(rect);
                    json.Add(new Window { name = GetWindowText(window), x = rect.Left, y = rect.Top, width = size.width, height = size.height });
                    string jsonString = JsonConvert.SerializeObject(json, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented });
                    return jsonString;
                }
                else
                    return "404 - error" + "\n" + "Window wasn't founded";
            }
                
        }

        private static SIZE GetWindowSize(RECT rect)
        {
            SIZE size = new SIZE();
            size.height = rect.Bottom - rect.Top;
            size.width = rect.Right - rect.Left;
            return size;
        }

        string GetWindowText(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd) + 1;
            StringBuilder sb = new StringBuilder(len);
            len = GetWindowText(hWnd, sb, len);
            return sb.ToString(0, len);
        }
    }
}