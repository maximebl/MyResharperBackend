using JetBrains.Diagnostics;
using JetBrains.Util;

namespace ReSharperPlugin.MyBackend;

public static class PluginLog
{
    private static readonly ILogger Logger = JetBrains.Util.Logging.Logger.GetLogger("MyBackend");

    public static void BeginSection(string title)
    {
        Logger.Info($"=== {title} ===");
#if RIDER
        LogWindow.AddSection(title);
#endif
    }

    public static void Log(string message)
    {
        Logger.Info(message.Trim());
#if RIDER
        LogWindow.AddEntry(message.Trim());
#endif
    }
}
