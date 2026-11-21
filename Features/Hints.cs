using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using LabApi.Features.Wrappers;

namespace BasePlugin_LabAPI.Features
{
    internal static class Hints
    {
        /// <summary>
        /// 在准星下方给单个玩家显示一条短提示。
        /// </summary>
        public static void Show(Player player, string text, float duration)
        {
            if (!player.IsPlayer || !player.IsReady || string.IsNullOrEmpty(text))
            {
                return;
            }

            Hint hint = new()
            {
                Text = text,
                FontSize = 26,
                YCoordinate = 760f,
                Alignment = HintAlignment.Center,
            };

            PlayerDisplay.Get(player).ShowHint(hint, duration);
        }

        /// <summary>
        /// 玩家名字可能带富文本标签，原样拼进提示会改变排版，所以禁止解析。
        /// </summary>
        public static string Plain(string text) => "<noparse>" + text.Replace("</noparse>", string.Empty) + "</noparse>";
    }
}
