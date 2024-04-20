using System.Runtime.InteropServices;

namespace GsmCore.Utils;

public static class PathUtil
{
    private static string _appDataPath = "";

    public static string GetAppDataPath()
    {
        if (!string.IsNullOrEmpty(_appDataPath)) return _appDataPath;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "GsmCore");
        }
        else
        {
            _appDataPath = Environment.GetEnvironmentVariable("GSM_DATA_PATH") ??
                           Path.Combine(Environment.GetEnvironmentVariable("HOME")!, ".gsmcore");
        }

        return _appDataPath;
    }
}