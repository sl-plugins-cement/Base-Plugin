using HintServiceMeow.Core.Enum;
using PlayerRoles;
using System.ComponentModel;

namespace BasePlugin_LabAPI.Configs
{
    public sealed class InfiniteAmmoConfig
    {
        [Description("是否启用 [无限弹药] 功能")]
        public bool Enabled { get; set; } = true;

        [Description("只对这些角色生效，留空为所有角色")]
        public List<RoleTypeId> Roles { get; set; } = new();

        [Description("是否禁止退弹（长按换弹键）。开启无限弹药后退弹可以凭空产出弹药，默认禁止")]
        public bool BlockUnload { get; set; } = true;
    }

    public sealed class PortableKeycardConfig
    {
        [Description("是否启用 [便携钥匙] 功能")]
        public bool Enabled { get; set; } = true;

        [Description("手持钥匙卡时是否只用手上这张卡（原版行为），不再检查背包里的其他钥匙卡")]
        public bool HeldKeycardOnly { get; set; } = true;

        [Description("是否对门生效")]
        public bool Doors { get; set; } = true;

        [Description("是否对储物柜（武器柜、SCP 收容柜等）生效")]
        public bool Lockers { get; set; } = true;

        [Description("是否对发电机生效")]
        public bool Generators { get; set; } = true;

        [Description("是否对地表核弹按钮盖生效")]
        public bool WarheadButton { get; set; } = true;
    }

    public sealed class Scp207Config
    {
        [Description("是否启用 [有益可乐] 功能")]
        public bool Enabled { get; set; } = true;

        [Description("SCP-207 伤害倍率：0 为完全不掉血，0.5 为原版的一半，1 为原版")]
        public decimal DamageMultiplier { get; set; } = 0;
    }

    public sealed class KillFeedConfig
    {
        [Description("是否启用 [击杀播报] 功能")]
        public bool Enabled { get; set; } = true;

        [Description("击杀播报的文字。可用占位符：{attacker} 击杀者名字、{victim} 死者名字、{attacker_role} / {victim_role} 角色名、{attacker_color} / {victim_color} 角色颜色")]
        public string Format { get; set; } = "<color={attacker_color}><b>{attacker}</b></color> <color=#98F5FF>击杀了</color> <color={victim_color}><b>{victim}</b></color>";

        [Description("是否播报非玩家造成的死亡（摔死、电网、核弹、自杀等），使用下方的文字")]
        public bool ShowOtherDeaths { get; set; } = false;

        [Description("非玩家死亡的播报文字，占位符同上（没有 attacker 相关占位符）")]
        public string OtherDeathFormat { get; set; } = "<color={victim_color}><b>{victim}</b></color> <color=#98F5FF>死亡</color>";

        [Description("SCP-3114 击杀时是否把名字显示为 SCP-3114（避免暴露伪装者身份）")]
        public bool HideScp3114Name { get; set; } = true;

        [Description("同时显示的最多条数，新的在最上面")]
        public int MaxLines { get; set; } = 4;

        [Description("每条播报显示的秒数")]
        public decimal LineDuration { get; set; } = 5;

        [Description("播报位置 Y 坐标（0 为屏幕最上方，1080 为最下方）")]
        public decimal YCoordinate { get; set; } = 180;

        [Description("播报位置 X 坐标（相对屏幕中心，正数向右）")]
        public decimal XCoordinate { get; set; } = 0;

        [Description("对齐方式：Left、Center、Right")]
        public HintAlignment Alignment { get; set; } = HintAlignment.Center;

        [Description("文字大小")]
        public int FontSize { get; set; } = 30;
    }
}
