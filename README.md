# BasePlugin - 基础插件

基于 LabAPI 编写的 SCP:秘密实验室 专用服务器插件，提供服务器常用的基础功能，全部可在一个配置文件里开关和调整。

## 功能

| 功能 | 配置分区 | 说明 |
| --- | --- | --- |
| 角色属性 | `roles` | 按角色设置出生时的最大血量、血量、AHP、休谟护盾（含回复速度与冷却）、出生物品、出生弹药和效果。SCP 与人类角色都可以，人类也能拥有休谟护盾 |
| 背包上限 | `inventory_limits` | 覆盖原版的物品类别上限与弹药上限（客户端背包界面会同步显示），并可限制单种物品的携带数量（例如最多 1 个 Micro-HID） |
| 无限弹药 | `unlimited_ammo` | 换弹时自动补足本次换弹所需的备弹，可限定角色；没用完的补给会收回，退弹默认禁止，避免凭空产出弹药 |
| 便携钥匙 | `portable_keycard` | 钥匙卡放在背包里即可开门、开储物柜、解锁发电机与核弹按钮盖，权限判断与手持钥匙卡完全一致；锁住的门仍然打不开 |
| 有益可乐 | `scp207` | 按倍率调整 SCP-207 的伤害，0 为完全不掉血 |
| 击杀播报 | `kill_feed` | 有人被击杀时向所有玩家滚动显示，最近几条叠在一起，新的在最上面；文字、位置、大小、条数、时长均可配置 |

## 安装

1. 把 `BasePlugin-LabAPI.dll` 放进 `%appdata%\SCP Secret Laboratory\LabAPI\plugins\<端口>`（或 `global`）。
2. 安装 [HintServiceMeow](https://github.com/MeowServer/HintServiceMeow)（LabAPI 版），击杀播报和提示文字依赖它。
3. 启动服务器，配置文件会生成在 `%appdata%\SCP Secret Laboratory\LabAPI\configs\<端口>\BasePlugin\config.yml`，每一项都带中文注释。

从 1.x 升级时，旧的 `infinite_ammo`、`portable_key_card`、`current_key_card_high`、`beneficial_s_c_p207`、`kill_broadcast` 开关会自动迁移到新的分区。

## 配置示例

```yaml
roles:
  enabled: true
  settings:
    ClassD:
      max_health: 120          # 最大血量，-1 为保持原版
      artificial_health: 20    # 出生 AHP
      max_hume_shield: 15      # 人类也可以有休谟护盾
      items: [Flashlight]      # 额外的出生物品
    Scp173:
      max_health: 5000
      max_hume_shield: 1000
      hume_shield_regen_rate: 30
    NtfCaptain:
      effects:
        - name: MovementBoost
          intensity: 10
          duration: 0          # 0 为永久

inventory_limits:
  enabled: true
  category_limits: { Medical: 4, Grenade: 3 }   # 类别上限，0-8
  ammo_limits: { Ammo9x19: 120 }                # 弹药上限，0 为不限
  item_limits: { MicroHID: 1, SCP207: 2 }       # 单种物品上限
```

- 角色名使用 `RoleTypeId`（`ClassD`、`Scientist`、`FacilityGuard`、`NtfPrivate`、`NtfSergeant`、`NtfSpecialist`、`NtfCaptain`、`ChaosConscript`、`ChaosRifleman`、`ChaosMarauder`、`ChaosRepressor`、`Tutorial`、`Scp049`、`Scp0492`、`Scp079`、`Scp096`、`Scp106`、`Scp173`、`Scp939`、`Scp3114` 等），未列出的角色保持原版。
- 出生物品与弹药只在角色获得原版出生装备时生效（回合开始、刷新、撤离等）；血量、护盾、AHP 和效果在每次成为该角色时生效。
- `category_limits` 与 `ammo_limits` 会覆盖 `config_gameplay.txt` 里的 `limit_category_*` / `limit_ammo*`，护甲的加成仍叠加在上面。降低上限不会没收玩家已经携带的物品。
- 休谟护盾的回复速度与冷却只对原版就会回复护盾的 SCP 生效；其他角色设置的护盾不会自动回复。

## 命令

| 命令 | 权限 | 说明 |
| --- | --- | --- |
| `bp reload`（`baseplugin reload`） | `ServerConfigs` | 重新读取 `config.yml` 并立即生效，无需重启服务器。配置有误时会在控制台列出警告 |

## 构建

项目默认从 `..\Resources\Managed` 引用游戏程序集、从 `..\Resources\HintServiceMeow-LabAPI.dll` 引用 HintServiceMeow，也可以用
`dotnet build -c Release -p:SCP_SL_MANAGED=<服务器的 SCPSL_Data\Managed 目录> -p:HintServiceMeowDll=<HintServiceMeow.dll 路径>` 指定。
