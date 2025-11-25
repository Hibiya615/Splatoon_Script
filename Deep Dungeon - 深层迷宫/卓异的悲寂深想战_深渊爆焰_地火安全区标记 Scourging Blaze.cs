using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ECommons.Schedulers;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatoonScriptsOfficial.Duties.Dawntrail.DeepDungeon.深层迷宫;
public class 卓异的悲寂深想战_深渊爆焰_地火安全区标记_Scourging_Blaze : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [1311];

    public override Metadata Metadata => new(4, "damolitionn, 南雲鉄虎(汉化)");

    private uint NSFirst = 44798;  // 南北
    private uint EWFirst = 44797;  // 东西

    private uint Crystal = 2014832; // 水晶DataID

    private bool isNSFirst = false; //判断水晶模式
    private bool isEWFirst = false;

    private bool isIn12Lane = false; // 水晶通道判断
    private bool isIn34Lane = false;

    private int castCounter = 0;

    private DateTime? castStartTime = null;


    // 注册安全区
    
    public override void OnSetup()
    {
        //1 右上 Safe
        Controller.RegisterElementFromCode("1", "{\"Name\":\"1\",\"enabled\": false,\"refX\":-594.13434,\"refY\":-313.40497,\"refZ\":-1.2874603E-05,\"radius\":1.5,\"color\":4278255360,\"Filled\":false,\"fillIntensity\":0.15,\"overlayBGColor\":2097152000,\"overlayTextColor\":4278255360,\"overlayFScale\":1.2,\"overlayText\":\"安全\"}");
        //2 右下 Safe
        Controller.RegisterElementFromCode("2", "{\"Name\":\"2\",\"enabled\": false,\"refX\":-594.2086,\"refY\":-286.23376,\"refZ\":-3.3378597E-06,\"radius\":1.5,\"color\":4278255360,\"Filled\":false,\"fillIntensity\":0.15,\"overlayBGColor\":2097152000,\"overlayTextColor\":4278255360,\"overlayFScale\":1.2,\"overlayText\":\"安全\"}");
        //3 左下 Safe
        Controller.RegisterElementFromCode("3", "{\"Name\":\"3\",\"enabled\": false,\"refX\":-605.5338,\"refY\":-286.20038,\"refZ\":-3.3378597E-06,\"radius\":1.5,\"color\":4278255360,\"Filled\":false,\"fillIntensity\":0.15,\"overlayBGColor\":2097152000,\"overlayTextColor\":4278255360,\"overlayFScale\":1.2,\"overlayText\":\"安全\"}");
        //4 左上 Safe
        Controller.RegisterElementFromCode("4", "{\"Name\":\"4\",\"enabled\": false,\"refX\":-605.6291,\"refY\":-313.52478,\"refZ\":-1.430511E-06,\"radius\":1.5,\"color\":4278255360,\"Filled\":false,\"fillIntensity\":0.15,\"overlayBGColor\":2097152000,\"overlayTextColor\":4278255360,\"overlayFScale\":1.2,\"overlayText\":\"安全\"}");
    }


    // BOSS开始读条触发

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == NSFirst)
        {
            castCounter++;
            isNSFirst = true;
            isEWFirst = false;
            castStartTime = DateTime.Now;
            isIn12Lane = false;
            isIn34Lane = false;
        }
        if (castId == EWFirst)
        {
            castCounter++;
            isEWFirst = true;
            isNSFirst = false;
            castStartTime = DateTime.Now;
            isIn12Lane = false;
            isIn34Lane = false;
        }
    }

    public override void OnUpdate()
    {
        // 南北安全 S Safe
        if (isNSFirst)
        {
            // 第一阶段：读条后11-14秒检测水晶位置 First Set
            if (castStartTime.HasValue && (DateTime.Now - castStartTime.Value).TotalSeconds >= 11 && (DateTime.Now - castStartTime.Value).TotalSeconds <= 14)
            {
                var positions = new List<(float x, float y, float z)>
                {
                    (-594f, 0, -300f), // 1/2通道检测点 Lane
                    (-606f, 0, -300f)  // 3/4通道检测点 Lane
                };

                var crystals = Svc.Objects  // 获取场景中所有水晶物体
                    .OfType<IGameObject>()
                    .Where(obj => obj.BaseId == Crystal);

                if (!isIn12Lane) // 检测水晶是否在1/2通道
                {
                    isIn12Lane = crystals.Any(obj =>
                        Math.Abs(obj.Position.X - positions[0].x) < 1.0f &&
                        Math.Abs(obj.Position.Y - positions[0].y) < 1.0f &&
                        Math.Abs(obj.Position.Z - positions[0].z) < 1.0f);
                }

                if (!isIn34Lane) // 检测水晶是否在3/4通道
                {
                    isIn34Lane = crystals.Any(obj =>
                        Math.Abs(obj.Position.X - positions[1].x) < 1.0f &&
                        Math.Abs(obj.Position.Y - positions[1].y) < 1.0f &&
                        Math.Abs(obj.Position.Z - positions[1].z) < 1.0f);
                }
            }
        }

        // 东西安全 E Safe
        if (isEWFirst)
        {
            // 第一阶段：读条后11-14秒检测水晶位置 First Set
            if (castStartTime.HasValue && (DateTime.Now - castStartTime.Value).TotalSeconds >= 11 && (DateTime.Now - castStartTime.Value).TotalSeconds <= 14)
            {
                var positions = new List<(float x, float y, float z)>
                {
                    (-594f, 0, -300f), // 1/2通道 Lane
                    (-606f, 0, -300f)  // 3/4通道 Lane
                };

                var crystals = Svc.Objects // 获取场景中所有水晶物体
                    .OfType<IGameObject>()
                    .Where(obj => obj.BaseId == Crystal);

                if (!isIn12Lane)
                {
                    isIn12Lane = crystals.Any(obj =>
                        Math.Abs(obj.Position.X - positions[0].x) < 1.0f &&
                        Math.Abs(obj.Position.Y - positions[0].y) < 1.0f &&
                        Math.Abs(obj.Position.Z - positions[0].z) < 1.0f);
                }

                if (!isIn34Lane)
                {
                    isIn34Lane = crystals.Any(obj =>
                        Math.Abs(obj.Position.X - positions[1].x) < 1.0f &&
                        Math.Abs(obj.Position.Y - positions[1].y) < 1.0f &&
                        Math.Abs(obj.Position.Z - positions[1].z) < 1.0f);
                }
            }
        }


        // 第二阶段：根据检测结果显示安全区域

        if (castStartTime.HasValue && 
            ((castCounter != 4 && (DateTime.Now - castStartTime.Value).TotalSeconds >= 11 && (DateTime.Now - castStartTime.Value).TotalSeconds <= 43) ||
             (castCounter == 4 && (DateTime.Now - castStartTime.Value).TotalSeconds >= 11 && (DateTime.Now - castStartTime.Value).TotalSeconds <= 69)))
        {
            if (isIn12Lane) // 水晶在1/2通道情况
            {
                if (isNSFirst) // 南北：安全区在3
                {
                    if (Controller.TryGetElementByName("3", out var threeSafe))
                    {
                        threeSafe.Enabled = true;
                    }
                }
                if (isEWFirst) // 东西：安全区在1
                {
                    if (Controller.TryGetElementByName("1", out var oneSafe))
                    {
                        oneSafe.Enabled = true;
                    }

                }
            }
            if (isIn34Lane) // 水晶在3/4通道情况
            {
                if (isNSFirst) // 南北：安全区在2
                {
                    if (Controller.TryGetElementByName("2", out var twoSafe))
                    {
                        twoSafe.Enabled = true;
                    }
                }
                if (isEWFirst) // 东西：安全区在4
                {
                    if (Controller.TryGetElementByName("4", out var fourSafe))
                    {
                        fourSafe.Enabled = true;
                    }
                }
            }
        }

        if (castStartTime.HasValue && (DateTime.Now - castStartTime.Value).TotalSeconds > 43)
        {
            Reset();  // 机制结束重置
        }
    }


    // 重置脚本

    public override void OnReset()
    {
        Reset();
        castCounter = 0;
    }

    private void Reset()
    {
        Controller.GetRegisteredElements().Each(x => x.Value.Enabled = false);
        isNSFirst = false;
        isEWFirst = false;
        isIn12Lane = false;
        isIn34Lane = false;
        castStartTime = null;
    }
}
