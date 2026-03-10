using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.DalamudServices;
using ECommons.Logging;
using ECommons.Schedulers;
using Splatoon.SplatoonScripting;
using System.Collections.Generic;
using System.Linq;

namespace SplatoonScriptsOfficial.Duties.Dawntrail.AnotherMerchantTale_异闻商客奇谭;

public class Pari_Rotation_Script_火仙女佩莉_四连炎舞 : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [1317];
    public override Metadata? Metadata => new(2, "Poneglyph");

    private List<string> turningVFX = new();
    private List<string> distanceVFX = new();
    private List<TickScheduler> activeSchedulers = new();
    private bool isWaiting = false;
    private bool isTurningOnly = false;

    private const string R1 = "vfx/lockon/eff/m0973_turning_right_3sec_c0e1.avfx";
    private const string R2 = "vfx/lockon/eff/m0973_turning_r_right_3sec_c0e1.avfx";
    private const string L1 = "vfx/lockon/eff/m0973_turning_left_3sec_c0e1.avfx";
    private const string L2 = "vfx/lockon/eff/m0973_turning_r_left_3sec_c0e1.avfx";
    private const string FAR = "vfx/common/eff/m0973_stlpf_c0e1.avfx";
    private const string CLOSE = "vfx/common/eff/m0973_stlpn_c0e1.avfx";

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("RotationOverlay", "{\"Name\":\"RotationOverlay\",\"type\":1,\"radius\":0.0,\"fillIntensity\":0.5,\"overlayBGColor\":3355443200,\"overlayTextColor\":4294967295,\"overlayVOffset\":2.48,\"thicc\":0.0,\"overlayText\":\" \",\"refActorType\":1}");
        Controller.RegisterElementFromCode("DistanceOverlay", "{\"Name\":\"DistanceOverlay\",\"type\":1,\"radius\":0.0,\"fillIntensity\":0.5,\"overlayBGColor\":3355443200,\"overlayTextColor\":4294967295,\"overlayVOffset\":3.0,\"thicc\":0.0,\"overlayText\":\" \",\"refActorType\":1}");
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId is 45467 or 45468 or 47031 or 47032)
        {
            OnReset();
            isWaiting = true;
            isTurningOnly = (castId == 47031 || castId == 47032);
        }
    }

    public override void OnVFXSpawn(uint target, string vfxPath)
    {
        if (!isWaiting) return;

        if (vfxPath is R1 or R2 or L1 or L2)
        {
            turningVFX.Add((vfxPath == R1 || vfxPath == R2) ? "Right" : "Left");
            ProcessRotationStep();
        }

        if (!isTurningOnly && (vfxPath is FAR or CLOSE))
        {
            distanceVFX.Add(vfxPath == FAR ? "Far" : "Close");
            ProcessDistanceStep();
        }
    }

    private void ProcessRotationStep()
    {
        string text = "";
        int count = turningVFX.Count;
        bool isFinal = false;

        if (count == 2)
        {
            text = (turningVFX[0] != turningVFX[1]) ? "停" : "穿";
        }
        else if (count == 3)
        {
            isFinal = true;
            bool firstStepNoMove = turningVFX[0] != turningVFX[1];
            bool secondStepNoMove = turningVFX[1] != turningVFX[2];

            if (firstStepNoMove) text = "停 - 穿 - 穿";
            else if (secondStepNoMove) text = "穿 - 停 - 穿";
            else text = "穿 - 穿 - 停";
        }

        if (!string.IsNullOrEmpty(text)) SetOverlayText("RotationOverlay", text, isFinal);
    }

    private void ProcessDistanceStep()
    {
        string text = "";
        int count = distanceVFX.Count;
        bool isFinal = false;

        if (count == 1) text = distanceVFX[0];
        else if (count == 2) text = $"{distanceVFX[0]} {distanceVFX[1]}";
        else if (count == 3)
        {
            isFinal = true;
            string f = distanceVFX[0];
            string s = distanceVFX[1];
            string t = distanceVFX[2];

            if (f == s) 
                text = (f == "Close") ? "近 近 远 远" : "远 远 近 近";
            else if (f == t) 
                text = (f == "Close") ? "近 远 近 远" : "远 近 远 近";
            else 
                text = (f == "Close") ? "近 远 远 近" : "远 近 近 远";
        }

        if (!string.IsNullOrEmpty(text)) SetOverlayText("DistanceOverlay", text, isFinal);
    }

    private void SetOverlayText(string name, string text, bool startTimer)
    {
        if (Controller.TryGetElementByName(name, out var element))
        {
            element.Enabled = true;
            element.overlayText = text;

            if (startTimer)
            {
                activeSchedulers.Add(new TickScheduler(() => 
                {
                    element.Enabled = false;
                    element.overlayText = " ";
                }, 25000));
            }
        }
    }

    public override void OnReset()
    {
        isWaiting = false;
        turningVFX.Clear();
        distanceVFX.Clear();
        foreach (var s in activeSchedulers) s?.Dispose();
        activeSchedulers.Clear();

        if (Controller.TryGetElementByName("RotationOverlay", out var rot)) { rot.Enabled = false; rot.overlayText = " "; }
        if (Controller.TryGetElementByName("DistanceOverlay", out var dist)) { dist.Enabled = false; dist.overlayText = " "; }
    }
}