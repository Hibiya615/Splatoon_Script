using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.DalamudServices.Legacy;
using ECommons.GameHelpers;
using ECommons.ImGuiMethods;
using ECommons.Throttlers;
using ImGuiNET;
using Splatoon.SplatoonScripting;

namespace SplatoonScriptsOfficial.Duties.Dawntrail.光暗未来绝境战_The_Futures_Rewritten;

public class P2_自动选中水晶_AutoTargetCrystal : SplatoonScript
{
    public enum TargetType
    {
        仅光水晶,
        仅大水晶,
        全部
    }

    public override HashSet<uint>? ValidTerritories => [1238];
    public override Metadata? Metadata => new(2, "Garume");

    private IEnumerable<IBattleNpc> LightCrystals => Svc.Objects.Where(x => x.DataId == 0x45A3).OfType<IBattleNpc>();
    private IBattleNpc? IceCrystal => Svc.Objects.FirstOrDefault(x => x.DataId == 0x45A5) as IBattleNpc;

    public Config C => Controller.GetConfig<Config>();

    public override void OnSettingsDraw()
    {
        ImGuiEx.EnumCombo("目标类型", ref C.TargetType);
        if (ImGuiEx.CollapsingHeader("Debug"))
        {
            ImGui.Text("光水晶");
            foreach (var crystal in LightCrystals) ImGui.Text(crystal.Name.ToString());
        }
    }

    public override void OnUpdate()
    {
        if (EzThrottler.Throttle("AutoTargetCrystal", 200)) SetNearTarget();
    }

    private void SetNearTarget()
    {
        switch (C.TargetType)
        {
            case TargetType.仅光水晶:
            {
                if (LightCrystals.Where(x => x.CurrentHp != 0)
                        .MinBy(x => Vector3.Distance(x.Position, Player.Position)) is { } target)
                    Svc.Targets.SetTarget(target);
                return;
            }
            case TargetType.仅大水晶:
            {
                if (!LightCrystals.Any(x => x.CurrentHp != 0) && IceCrystal is { } ice) Svc.Targets.SetTarget(ice);
                return;
            }
            case TargetType.全部:
            {
                if (LightCrystals.Where(x => x.CurrentHp != 0)
                        .MinBy(x => Vector3.Distance(x.Position, Player.Position)) is { } target)
                    Svc.Targets.SetTarget(target);
                else if (IceCrystal is { } ice) Svc.Targets.SetTarget(ice);
                return;
            }
        }
    }

    public class Config : IEzConfig
    {
        public TargetType TargetType = TargetType.全部;
    }
}