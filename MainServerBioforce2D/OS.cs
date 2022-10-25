using System.Runtime.InteropServices;

namespace MainServerBioforce2D
{
    internal class OS
    {
        internal enum Type
        {
            Windows,
            Linux,
            Mac,
            none = int.MaxValue
        }

        static Type CurrentType = Type.none;
        internal static Type GetOS()
        {
            if (CurrentType != Type.none)
                return CurrentType;


            if (IsWindows())
                CurrentType = Type.Windows;
            else if (IsLinux())
                CurrentType = Type.Linux;
            else if (IsMacOS())
                CurrentType = Type.Mac;
            else
                throw new System.Exception("Operating System not identified by OS.");

            return CurrentType;
        }

        internal const string LinuxAppExtension = "x86_64";
        internal const string WindowsAppExtension = ".exe";
        internal const string MacAppExtension = ".app";

        internal static string GetAppExtension()
        {
            switch (GetOS())
            {
                case Type.Windows:
                    return WindowsAppExtension;

                case Type.Linux:
                    return LinuxAppExtension;

                case Type.Mac:
                    return MacAppExtension;

                default:
                    return null;
            }
        }

        private static bool IsOS(OSPlatform os) =>
            RuntimeInformation.IsOSPlatform(os);
        private static bool IsWindows() =>
            IsOS(OSPlatform.Windows);
        private static bool IsLinux() =>
            IsOS(OSPlatform.Linux);
        private static bool IsMacOS() =>
            IsOS(OSPlatform.OSX);

    }
}
