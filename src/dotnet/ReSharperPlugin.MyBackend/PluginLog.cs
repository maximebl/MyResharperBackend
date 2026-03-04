using System;
using System.IO;

namespace ReSharperPlugin.MyBackend;

public static class PluginLog
{
    private static readonly string LogFile = @"C:\Temp\MyBackend.log";

    public static void BeginSection(string title)
    {
        var line = $"=== [{DateTime.Now:HH:mm:ss.fff}] {title} ===";
        File.AppendAllText(LogFile, line + Environment.NewLine);
#if RIDER
        LogWindow.AddSection(title);
#endif
    }

    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message.Trim()}";
        File.AppendAllText(LogFile, line + Environment.NewLine);
#if RIDER
        LogWindow.AddEntry(line);
#endif
    }
}
