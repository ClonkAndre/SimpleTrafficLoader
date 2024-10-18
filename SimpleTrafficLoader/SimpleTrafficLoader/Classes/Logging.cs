using IVSDKDotNet;

namespace SimpleTrafficLoader.Classes
{
    internal class Logging
    {

#if DEBUG
        public static bool EnableDebugLogging = true;
#else
        public static bool EnableDebugLogging = false;
#endif

        public static void Log(string str, params object[] args)
        {
            IVGame.Console.Print(string.Format("[SimpleTrafficLoader] {0}", string.Format(str, args)));
        }
        public static void LogWarning(string str, params object[] args)
        {
            IVGame.Console.PrintWarning(string.Format("[SimpleTrafficLoader] {0}", string.Format(str, args)));
        }
        public static void LogError(string str, params object[] args)
        {
            IVGame.Console.PrintError(string.Format("[SimpleTrafficLoader] {0}", string.Format(str, args)));
        }

        public static void LogDebug(string str, params object[] args)
        {
#if DEBUG
            if (EnableDebugLogging)
                IVGame.Console.Print(string.Format("[SimpleTrafficLoader] [DEBUG] {0}", string.Format(str, args)));
#endif
        }

    }
}
