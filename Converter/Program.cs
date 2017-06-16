using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;

namespace Translator
{
    public class Program
    {        
        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

[STAThread]   
        public static int Main(string[] args)
        {
            int result = 0;
            bool startUI = false;
            
#if !DEBUG
            try
            {
#endif // !DEBUG
                if (!args.Contains("-g"))
                {
                    // console version
                    IntPtr ptr = GetForegroundWindow();
                    int  u;
                    GetWindowThreadProcessId(ptr, out u);
                    Process process = Process.GetProcessById(u);
                    bool attachToWindow = process.ProcessName == "cmd";

                    if (attachToWindow)
                    {
                        //Is the uppermost window a cmd process?
                        // attach to existing cmd
                        AttachConsole(process.Id);    
                    }
                    else
                    {
                        // this is explorer or not cmd console, e.g. NC, FarManager, etc.
                        // create new console window
                        AllocConsole();
                    }

                    TranslatorTool program = new TranslatorTool();

                    try
                    {
                        program.Run(args, out startUI);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                
#if DEBUG
                    Console.WriteLine();
                    Console.WriteLine(ResStr.MSG_PRESS_ANY_KEY_TO_CONTINUE);

                    if (!attachToWindow && !startUI)
                    {
                        Console.ReadKey();
                    }
#endif

                   FreeConsole();
                }

                if (startUI || args.Contains("-g"))
                {
                    // UI version
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                } 
#if !DEBUG
            }
            catch (Exception e)
            {
                if (startUI || args.Contains("-g"))
                {
                    MessageBox.Show(String.Format(ResStr.WARN_INTERNAL_ERROR, "\n" + e.Message + "\n"));
                }
                Console.WriteLine(ResStr.MSG_INTERNAL_ERROR, e.Message);

                result = -1;
            }
#endif // !DEBUG

            return result;
        }

        
    }
}
