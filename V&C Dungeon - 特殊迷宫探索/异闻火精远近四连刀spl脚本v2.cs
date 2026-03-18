using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using ECommons.MathHelpers;
using ECommons.Schedulers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using Splatoon;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using Splatoon.SplatoonScripting.Priority;
using static Splatoon.Splatoon;
using static Dalamud.Bindings.ImGui.ImGui;

namespace SplatoonScriptsOfficial.Duties.Dawntrail.AnotherMerchantTale_异闻商客奇谭;

public class Another_Merchants_Take_B3_Four_Long_Nights_火仙女佩莉_四连炎舞追火 : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => [1317,];
    public override Metadata? Metadata => new(2, "redmoon");

    private enum LeftRight
    {
        Left,
        Right,
    }

    private enum NearFar
    {
        Near,
        Far,
    }

    private bool _activated;
    private int _hitCount;
    private bool _isLocked;
    private bool _isFinal;
    private List<LeftRight> _turnVfx = [];
    private List<(NearFar, int)> _nearFarVfx = [];
    private List<(uint, int)> _fireCrystal = [];

    private Config C => Controller.GetConfig<Config>();

    public override void OnSetup()
    {
        #region Priority List Setup
        if (C.PriorityData.PriorityLists.Count == 0)
        {
            C.PriorityData.PriorityLists.Add(new PriorityList
            {
                IsRole = true,
                List =
                [
                    new JobbedPlayer { Role = RolePosition.T1, },
                    new JobbedPlayer { Role = RolePosition.H1, },
                    new JobbedPlayer { Role = RolePosition.M1, },
                    new JobbedPlayer { Role = RolePosition.R1, },
                ],
            });
        }
        else if (C.PriorityData.PriorityLists[0].List[0].Role == RolePosition.Not_Selected || C.IsIdyl)
        {
            C.PriorityData.PriorityLists[0].IsRole = true;
            C.PriorityData.PriorityLists[0].List[0].Role = RolePosition.T1;
            C.PriorityData.PriorityLists[0].List[2].Role = RolePosition.H1;
            C.PriorityData.PriorityLists[0].List[1].Role = RolePosition.M1;
            C.PriorityData.PriorityLists[0].List[3].Role = RolePosition.R1;
        }
        #endregion

        Controller.RegisterElement("guide", new Element(0)
        {
            radius = 0.35f, thicc = 15f, tether = true,
        });

        Controller.RegisterElementFromCode("lr",
            """{"Name":"","type":3,"refY":-20.0,"radius":20.0,"fillIntensity":0.4,"thicc":6.0,"refActorDataID":19056,"refActorComparisonType":3,"onlyTargetable":true,"IsDead":false}""");
        Controller.RegisterElementFromCode("lrPre",
            """{"Name":"","type":3,"refY":20.0,"radius":20.0,"color":4278255615,"Filled":false,"fillIntensity":0.5,"thicc":8.0,"refActorDataID":19056,"refActorComparisonType":3,"onlyTargetable":true,"IsDead":false}""");
        Controller.RegisterElementFromCode("crystalAoeVertical1",
            """{"Name":"","type":3,"refY":50.0,"offY":-50.0,"radius":5.0,"refActorObjectID":1073757668,"refActorComparisonType":2,"includeRotation":true}""");
        Controller.RegisterElementFromCode("crystalAoeHorizontal1",
            """{"Name":"","type":3,"refY":50.0,"offY":-50.0,"radius":5.0,"refActorObjectID":1073757668,"refActorComparisonType":2,"includeRotation":true,"AdditionalRotation":1.5707964}""");
        Controller.RegisterElementFromCode("crystalAoeVertical2",
            """{"Name":"","type":3,"refY":50.0,"offY":-50.0,"radius":5.0,"refActorObjectID":1073757668,"refActorComparisonType":2,"includeRotation":true}""");
        Controller.RegisterElementFromCode("crystalAoeHorizontal2",
            """{"Name":"","type":3,"refY":50.0,"offY":-50.0,"radius":5.0,"refActorObjectID":1073757668,"refActorComparisonType":2,"includeRotation":true,"AdditionalRotation":1.5707964}""");
    }

    public override unsafe void OnStartingCast(uint sourceId, PacketActorCast* packet)
    {
        if (packet->ActionID is 0xB19C or 0xB7B7) _activated = true;
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (!_activated) return;
        if (set.Action?.RowId is 0xB19D or 0xB19E or 0xB19F or 0xB1A0)
        {
            _hitCount++;
            _isLocked = false;
            if (_hitCount >= 4)
            {
                _isFinal = true;
                OnWormReset();
            }
        }
    }

    public override void OnVFXSpawn(uint target, string vfxPath)
    {
        if (!_activated) return;
        if (vfxPath.Contains("vfx/lockon/eff/m0973_turning_right_3sec_c0e1.avfx")) _turnVfx.Add(LeftRight.Right);
        if (vfxPath.Contains("vfx/lockon/eff/m0973_turning_left_3sec_c0e1.avfx")) _turnVfx.Add(LeftRight.Left);
        if (vfxPath.Contains("vfx/lockon/eff/m0973_turning_r_right_3sec_c0e1.avfx")) _turnVfx.Add(LeftRight.Right);
        if (vfxPath.Contains("vfx/lockon/eff/m0973_turning_r_left_3sec_c0e1.avfx")) _turnVfx.Add(LeftRight.Left);
        if (vfxPath.Contains("vfx/common/eff/m0973_stlpf_c0e1.avfx"))
        {
            if (_nearFarVfx.Any(x => x.Item1 == NearFar.Far))
                _nearFarVfx.Add((NearFar.Far, 1));
            else _nearFarVfx.Add((NearFar.Far, 0));
        }

        if (vfxPath.Contains("vfx/common/eff/m0973_stlpn_c0e1.avfx"))
        {
            if (_nearFarVfx.Any(x => x.Item1 == NearFar.Near))
                _nearFarVfx.Add((NearFar.Near, 1));
            else _nearFarVfx.Add((NearFar.Near, 0));
        }
    }

    public override void OnUpdate()
    {
        if (!_activated) return;
        Controller.Hide();

        if (!Controller.TryGetElementByName("lr", out var lrElement)) return;
        if (!Controller.TryGetElementByName("lrPre", out var lrPreElement)) return;
        if (!Controller.TryGetElementByName("guide", out var guideElement)) return;

        if (!_isFinal)
        {
            if (_turnVfx.Count != 0)
            {
                var turn = _turnVfx[_hitCount];
                if (_hitCount == 0 && turn == LeftRight.Left) lrElement.refY = Math.Abs(lrElement.refY);
                else if (_hitCount == 0 && turn == LeftRight.Right) lrElement.refY = -Math.Abs(lrElement.refY);
                else
                {
                    var turn_prev = _turnVfx[_hitCount - 1];
                    if (turn_prev == turn && !_isLocked) lrElement.refY = -lrElement.refY;
                    else lrElement.refY = lrElement.refY;

                    _isLocked = true;
                }

                if (_hitCount != 3 && _turnVfx.Count > 2)
                {
                    var turn_next = _turnVfx[_hitCount + 1];
                    if (turn_next == turn) lrPreElement.refY = -lrElement.refY;
                    else lrPreElement.refY = lrElement.refY;
                    lrPreElement.Enabled = true;
                }

                lrElement.Enabled = true;
            }

            if (_nearFarVfx.Count != 0)
            {
                var nearFar = _nearFarVfx[_hitCount];
                var center = new Vector3(-760.0f, -54f, -805.0f);
                if (!C.IsIdyl)
                {
                    var index = C.PriorityData.GetOwnIndex(_ => true);
                    if (_hitCount == index)
                    {
                        if (nearFar.Item1 == NearFar.Near) guideElement.SetRefPosition(center);
                        else guideElement.SetRefPosition(center with { X = center.X + 10f, });
                    }
                    else guideElement.SetRefPosition(center with { X = center.X + 5f, });
                }
                else
                {
                    var index = C.PriorityData.GetOwnIndex(_ => true);
                    if (_nearFarVfx[_hitCount].Item1 == NearFar.Near)
                    {
                        if (_nearFarVfx[_hitCount].Item2 == 0 && index == 0) guideElement.SetRefPosition(center);
                        else if (_nearFarVfx[_hitCount].Item2 == 1 && index == 2) guideElement.SetRefPosition(center);
                        else guideElement.SetRefPosition(center with { X = center.X + 5f, });
                    }
                    else // Far
                    {
                        if (_nearFarVfx[_hitCount].Item2 == 0 && index == 1)
                            guideElement.SetRefPosition(center with { X = center.X + 10f, });
                        else if (_nearFarVfx[_hitCount].Item2 == 1 && index == 3)
                            guideElement.SetRefPosition(center with { X = center.X + 10f, });
                        else guideElement.SetRefPosition(center with { X = center.X + 5f, });
                    }
                }

                guideElement.Enabled = true;
            }
        }
        else
        {
            if (!Controller.TryGetElementByName("crystalAoeVertical1", out var cAV1)) return;
            if (!Controller.TryGetElementByName("crystalAoeHorizontal1", out var cAH1)) return;
            if (!Controller.TryGetElementByName("crystalAoeVertical2", out var cAV2)) return;
            if (!Controller.TryGetElementByName("crystalAoeHorizontal2", out var cAH2)) return;

            var crystal = Svc.Objects.Where(x => x.BaseId == 0x4A72).OfType<IBattleNpc>().ToList();
            var visibleCrystal = crystal.Where(x => x.IsCharacterVisible()).ToList();
            if (visibleCrystal.Count != 0)
            {
                // _fireCrystalにまだはいっていないものを探して追加する。
                var fireCrystal = visibleCrystal.Where(x => _fireCrystal.All(y => y.Item1 != x.ObjectId)).ToList();
                foreach (var fire in fireCrystal) _fireCrystal.Add((fire.ObjectId, _fireCrystal.Count / 2));
            }

            if (_turnVfx.Count != 0)
            {
                var turn = _turnVfx[_hitCount];
                if (_hitCount == 0 && turn == LeftRight.Left) lrElement.refY = -Math.Abs(lrElement.refY);
                else if (_hitCount == 0 && turn == LeftRight.Right) lrElement.refY = +Math.Abs(lrElement.refY);
                else
                {
                    var turn_prev = _turnVfx[_hitCount - 1];
                    if (turn_prev == turn && !_isLocked) lrElement.refY = -lrElement.refY;
                    else lrElement.refY = lrElement.refY;

                    _isLocked = true;
                }

                if (_hitCount != 3 && _turnVfx.Count >= 2)
                {
                    var turn_next = _turnVfx[_hitCount + 1];
                    if (turn_next == turn) lrPreElement.refY = -lrElement.refY;
                    else lrPreElement.refY = lrElement.refY;
                    lrPreElement.Enabled = true;
                }

                lrElement.Enabled = true;
            }

            if (_fireCrystal.Count >= 2)
            {
                var fireCrystal = _fireCrystal.Where(x => x.Item2 == _hitCount).ToList();
                if (fireCrystal.Count == 2)
                {
                    cAV1.refActorObjectID = fireCrystal[0].Item1;
                    cAH1.refActorObjectID = fireCrystal[0].Item1;
                    cAV2.refActorObjectID = fireCrystal[1].Item1;
                    cAH2.refActorObjectID = fireCrystal[1].Item1;

                    cAV1.Enabled = true;
                    cAH1.Enabled = true;
                    cAV2.Enabled = true;
                    cAH2.Enabled = true;
                }
            }
        }

        lrElement.Enabled = true;

        guideElement.color = GradientColor.Get(0xFF00FF00.ToVector4(), 0xFF0000FF.ToVector4()).ToUint();
    }

    public override void OnReset()
    {
        _isFinal = false;
        OnWormReset();
    }

    private void OnWormReset()
    {
        _hitCount = 0;
        _isLocked = false;
        _turnVfx.Clear();
        _nearFarVfx.Clear();
        _activated = false;
        Controller.Hide();
    }

    public class PrioriryData4 : PriorityData
    {
        public override int GetNumPlayers() => 4;
    }

    public class Config : IEzConfig
    {
        public bool IsIdyl = false;
        public PrioriryData4 PriorityData = new();
    }

    public override void OnSettingsDraw()
    {
        ImGuiEx.Checkbox("Is Idyl", ref C.IsIdyl);

        C.PriorityData.Draw();

        if (!ImGuiEx.CollapsingHeader("Debug")) return;
        ImGuiEx.Checkbox("Activated", ref _activated);
        ImGuiEx.Checkbox("Is Final", ref _isFinal);
        InputInt("Hit Count", ref _hitCount);
        Separator();
        Text("Turn VFX:");
        foreach (var v in _turnVfx) ImGuiEx.Text(v.ToString());
        Separator();
        Text("Near Far VFX:");
        foreach (var v in _nearFarVfx) ImGuiEx.Text(v.ToString());
        Separator();
        Text("Fire Crystal:");
        foreach (var v in _fireCrystal) ImGuiEx.Text(v.ToString());
        Separator();
        Text("Elements:");
        foreach (var e in Controller.GetRegisteredElements())
            ImGuiEx.Text(
                $"{e.Key}: Enabled={e.Value.Enabled} Pos=({e.Value.refX}, {e.Value.refZ}, {e.Value.refY}) EntityId={e.Value.refActorObjectID:X8}");
    }
}