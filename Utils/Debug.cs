using System;

namespace Utils
{
    static public class Debug
    {
        static bool DebugMode = false;


        static public void SetDebugMode(bool mode)
        {
            DebugMode = mode;
            Debug.log("Debug mode set to: " + mode.ToString());
        }

        static public void log( string msg)
        {
            if (DebugMode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
        }
        static public void error(string msg, bool condition = true)
        {
            if (condition && DebugMode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
        }
    }
}
