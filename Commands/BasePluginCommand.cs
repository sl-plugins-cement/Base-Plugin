using CommandSystem;
using RemoteAdmin;

namespace BasePlugin_LabAPI.Commands
{
    /// <summary>
    /// RA / 服务器控制台命令：bp reload 重新读取 config.yml，无需重启服务器。
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class BasePluginCommand : ICommand
    {
        public string Command => "baseplugin";

        public string[] Aliases { get; } = { "bp" };

        public string Description => "BasePlugin：bp reload 重新加载配置";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(PlayerPermissions.ServerConfigs, out response))
            {
                return false;
            }

            if (arguments.Count == 0 || !arguments.At(0).Equals("reload", StringComparison.OrdinalIgnoreCase))
            {
                response = "用法：bp reload";
                return false;
            }

            MainClass? plugin = MainClass.Instance;
            if (plugin == null)
            {
                response = "BasePlugin 未启用";
                return false;
            }

            bool ok = plugin.TryReloadConfig(out response);
            return ok;
        }
    }
}
