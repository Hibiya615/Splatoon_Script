using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using Splatoon.SplatoonScripting;
using System.Collections.Generic;
using System.Linq;

namespace SplatoonScriptsOfficial.Duties.Dawntrail.DeepDungeon.深层迷宫;
public class 卓异的悲寂通用_目标选择器_Quantum_Target_Enforcer : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [1290, 1311, 1333];
    public override Metadata? Metadata => new(7, "Poneglyph, NightmareXIV, Redmoon, 南雲鉄虎(汉化)");

    private static class Buffs
    {
        public const uint DarkBuff = 4559;
        public const uint LightBuff = 4560;
    }

    private static class Enemies
    {
        public const uint EminentGrief = 14037; // 光BOSS
        public const uint DevouredEater = 14038; // 暗BOSS
        public const uint VodorigaMinion = 14039; // 仆从小怪，优先击杀
        public const uint MagicCircle = 14042; // 魔法阵，需要分配转火
        public const uint FireEnemy = 14041; // 火人，不需要输出，通过喂到光地板处理
    }

    private bool Throttle() => EzThrottler.Throttle($"{InternalData.FullName}_SetTarget", 250);

    public override void OnUpdate()
    {

        // 非战斗状态下、玩家死亡时、切换至屏幕外时 取消工作

        if(!Controller.InCombat) return;
        if(Player.Object == null || Player.Object.IsDead) return;
        if(!GenericHelpers.IsScreenReady()) return;
        
        if(Svc.Targets.SoftTarget != null)
        {
            FrameThrottler.Throttle("SoftTargetThrottle", 10);
        }
        if(!FrameThrottler.Check("SoftTargetThrottle")) return;

        // 检查用户设置

        if(C.NoSwitchOffPlayers && Svc.Targets.Target is IPlayerCharacter)
        {
            return;
        }

        if(C.NoSwitchMagicCircle && Svc.Targets.Target is ICharacter npc && npc.NameId == Enemies.MagicCircle)
        {
            return;
        }

        if(C.NoSwitchFireEnemy && Svc.Targets.Target is ICharacter npc2 && npc2.NameId == Enemies.FireEnemy)
        {
            return;
        }

        var player = Player.Object;
        if(player == null) return;

        // 优先处理仆从小怪

        var vodoriga = Svc.Objects.OfType<IBattleNpc>()
            .FirstOrDefault(x => x.IsTargetable && !x.IsDead && x.NameId == Enemies.VodorigaMinion);

        if(vodoriga != null)
        {
            if(C.TargetVodoriga)
            {
                EnforceTarget(vodoriga);
            }
            return;
        }

        //检查玩家Buff
        var hasDark = player.StatusList.Any(x => x.StatusId == Buffs.DarkBuff);
        var hasLight = player.StatusList.Any(x => x.StatusId == Buffs.LightBuff);


        // 验证当前目标
        if(Svc.Targets.Target is IBattleNpc currentTarget)
        {

            bool isCurrentlyValidTarget =
                currentTarget.NameId == Enemies.EminentGrief ||
                currentTarget.NameId == Enemies.DevouredEater;

            if(!isCurrentlyValidTarget)
            {
                return;
            }
        }

        IBattleNpc? target = null;


        // 根据状态选择目标

        if(hasLight)
        {
            target = Svc.Objects.OfType<IBattleNpc>()
                .FirstOrDefault(x => x.IsTargetable && !x.IsDead && x.NameId == Enemies.EminentGrief);
        }
        else if(hasDark)
        {
            target = Svc.Objects.OfType<IBattleNpc>()
                .FirstOrDefault(x => x.IsTargetable && !x.IsDead && x.NameId == Enemies.DevouredEater);
        }

        if(target != null)
        {
            EnforceTarget(target);
        }
    }


    // Imgui界面绘制

    public override void OnSettingsDraw()
    {
        ImGui.Checkbox("自动选中仆从小怪 / Target Vodoriga", ref C.TargetVodoriga);
                ImGui.TextWrapped("当仆从小怪出现时，将优先自动选中，并强制锁定目标\n近战需考虑GCD丢失，DPS需注意能力技热风\n ");
        ImGui.Checkbox("选中队友时，不自动选中光暗BOSS / Don't switch off players", ref C.NoSwitchOffPlayers);
                ImGui.TextWrapped("[TN推荐启用] 当选中玩家时，不自动选中光暗BOSS，以免给不到队友 \n[DPS推荐关闭] 当选中玩家时，仍然自动选中光暗BOSS，防止选到队友\n ");
        ImGui.Checkbox("选中魔法阵时，不自动切换目标 / Don't switch off Magic Circle", ref C.NoSwitchMagicCircle);
                ImGui.TextWrapped("[推荐启用] 当选中魔法阵时，不自动选中光暗BOSS，防止自动切换魔法阵外的目标 \n[关闭] 当选中魔法阵时，仍然根据踩地板变换自动选中光暗BOSS\n ");
        ImGui.Checkbox("选中火人时，不自动切换目标 / Don't switch off Fire Enemy", ref C.NoSwitchFireEnemy);
                ImGui.TextWrapped("[启用] 当选中火人时，不自动选中光暗BOSS \n[推荐关闭] 当选中火人时，仍然根据踩地板变换自动选中光暗BOSS");
    }

    private void EnforceTarget(IBattleNpc target)
    {
        if(!Throttle()) return;
        if(target == null || !target.IsTargetable || target.IsDead) return;
        if(Svc.Targets.Target != target)
        {
            Svc.Targets.Target = target;
        }
    }

    // 默认用户设置

    Config C => Controller.GetConfig<Config>();
    public class Config : IEzConfig
    {
        public bool TargetVodoriga = true;
        public bool NoSwitchOffPlayers = true;
        public bool NoSwitchMagicCircle = true;
        public bool NoSwitchFireEnemy = false;
    }
}