using LabApi.Features.Console;

namespace BasePlugin_LabAPI.Features
{
    internal static class Log
    {
        // 只在调试模式下拼接字符串，避免高频事件里白白分配。
        public static void Debug(Func<string> message)
        {
            if (MainClass.Config?.Debug == true)
            {
                Logger.Debug(message());
            }
        }

        public static void Info(string message) => Logger.Info(message);

        public static void Warn(string message) => Logger.Warn(message);

        public static void Error(string message) => Logger.Error(message);
    }
}
