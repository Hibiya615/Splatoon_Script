using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Splatoon;
using Splatoon.SplatoonScripting;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SplatoonScriptsOfficial.Duties.Dawntrail.The.Arcadion.阿卡狄亚登天斗技场;

public sealed class M5S_放纵劲舞_Debuff指路_Lets_Dance_NavigateStandPosition : SplatoonScript
{
    private enum State
    {
        None,
        Casting,
        End
    }

    private const ushort AlphaDebuff = 0x116E;
    private const ushort BetaDebuff = 0x116F;
    private Vector3[] Positions = [new Vector3(100, 0, 106), new Vector3(100, 0, 102), new Vector3(100, 0, 98), new Vector3(100, 0, 94)];
    private Vector3[] ReversePositions => Positions.Reverse().ToArray();

    private State _state = State.None;
    public override HashSet<uint>? ValidTerritories => [1257];

    public override Metadata? Metadata => new(1, "Garume, NightmareXIV,南雲鉄虎本地化");

    private Config C => Controller.GetConfig<Config>();

    public override void OnSetup()
    {
        var element = new Element(0)
        {
            radius = 1.5f,
            thicc = 3f,
            tether = true,
        };
        Controller.RegisterElement("Bait", element);
    }

    public override void OnSettingsDraw()
    {
        ImGuiEx.RadioButtonBool("从南开始 短→长", "从北开始 短→长", ref C.IsReversed);
        ImGui.ColorEdit4("Color1", ref C.BaitColor1, ImGuiColorEditFlags.NoInputs);
        ImGui.ColorEdit4("Color2", ref C.BaitColor2, ImGuiColorEditFlags.NoInputs);
        ImGui.Text($"当前状态: {_state}");
    }

    public override void OnUpdate()
    {
        if(_state == State.Casting)
        {
            Controller.GetRegisteredElements().Each(x => x.Value.color = GradientColor.Get(C.BaitColor1, C.BaitColor2).ToUint());
            if(!Player.Status.Any(s => s.StatusId is AlphaDebuff or BetaDebuff) || Svc.Objects.OfType<IPlayerCharacter>().All(x => !x.StatusList.Any(s => s.StatusId is AlphaDebuff or BetaDebuff)))
            {
                _state = State.End;
            }
        }
        else
        {
            Controller.GetRegisteredElements().Each(x => x.Value.Enabled = false);
        }
    }

    public override void OnReset()
    {
        _state = State.None;
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if(castId == 42858 && _state == State.None)
        {
            _state = State.Casting;
            var remainingTime = -1f;
            if(Player.Status.Any(x => x.StatusId == AlphaDebuff))
                remainingTime = Player.Status.First(x => x.StatusId == AlphaDebuff).RemainingTime;
            else if(Player.Status.Any(x => x.StatusId == BetaDebuff))
                remainingTime = Player.Status.First(x => x.StatusId == BetaDebuff).RemainingTime;

            PluginLog.Warning($"Remaining time: {remainingTime}");
            if(remainingTime == -1f)
            {
                PluginLog.Warning($"Player is dead");
                return;
            }

            if(!Controller.TryGetElementByName("Bait", out var baitElement))
                return;

            var positions = C.IsReversed ? ReversePositions : Positions;

            switch(remainingTime)
            {
                case > 20:
                    baitElement.SetOffPosition(positions[0]);
                    baitElement.overlayText = "23s";
                    break;
                case > 15:
                    baitElement.SetOffPosition(positions[1]);
                    baitElement.overlayText = "18s";
                    break;
                case > 10:
                    baitElement.SetOffPosition(positions[2]);
                    baitElement.overlayText = "13s";
                    break;
                case > 5:
                    baitElement.SetOffPosition(positions[3]);
                    baitElement.overlayText = "8s";
                    break;
            }

            baitElement.Enabled = true;
        }
    }

    private class Config : IEzConfig
    {
        public Vector4 BaitColor1 = 0xFFFF00FF.ToVector4();
        public Vector4 BaitColor2 = 0xFFFFFF00.ToVector4();
        public bool IsReversed = false;
    }
}