
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    public enum FlagControl
    {
        TurnAllOn,
        TurnAllOff,
        Individual
    }
    public class AreaFlagData
    {
        public bool IncludeBackend;
        public float Lighting;
        public string Name;
        public string Flag => "In" + Name;
        public FlagControl Control;
        public AreaFlagData(string name, FlagControl control, bool includeBackend, float lighting)
        {
            Name = name;
            Control = control;
            IncludeBackend = includeBackend;
            Lighting = lighting;
        }
    }
    public static class AreaFlagHelper
    {

        public static AreaFlagData Backend = new AreaFlagData("Backend", FlagControl.Individual, false, 0.05f);
        public static AreaFlagData Forest = new AreaFlagData("Forest", FlagControl.Individual, false, 0);
        public static AreaFlagData Pipes = new AreaFlagData("Pipes", FlagControl.Individual, true, -1);
        public static AreaFlagData Resort = new AreaFlagData("Resort", FlagControl.Individual, false, -1);
        public static AreaFlagData Lab = new AreaFlagData("Lab", FlagControl.Individual, true, -1);
        public static AreaFlagData Golden = new AreaFlagData("Golden", FlagControl.Individual, false, -1);
        public static AreaFlagData Void = new AreaFlagData("Void", FlagControl.Individual, false, -1);
        public static AreaFlagData None = new AreaFlagData("None", FlagControl.TurnAllOff, false, -1);
        public static List<AreaFlagData> Data = new()
        {
            Backend,Forest,Pipes,Resort,Lab,Golden,Void,None
        };
        public static AreaFlagData GetArea(string name)
        {
            return Data.Find(item => item.Name == name);
        }
        public static void SetLighting(Level level, string key)
        {
            AreaFlagData data = GetArea(key);
            if (data != null)
            {
                float prevLight = level.Lighting.Alpha;
                if (data.Lighting >= 0)
                {
                    float num = level.Session.LightingAlphaAdd = prevLight + (data.Lighting - prevLight);
                    level.Lighting.Alpha = level.BaseLightingAlpha + num;
                }
            }
        }
        public static void SetFlags(Level level, string area)
        {
            AreaFlagData currentArea = GetArea(area);
            string output;
            if (currentArea != null)
            {
                output = "Configuring area flags for Area: " + area;
                foreach (AreaFlagData data2 in Data)
                {
                    output += '\n';
                    bool prevState = level.Session.GetFlag(data2.Flag);
                    bool state = data2.Name == currentArea.Name || (data2.Name == "Backend" && currentArea.IncludeBackend);
                    state = currentArea.Control switch
                    {
                        FlagControl.TurnAllOn => true,
                        FlagControl.TurnAllOff => false,
                        FlagControl.Individual => data2.Name == currentArea.Name || (data2.Name == "Backend" && currentArea.IncludeBackend)
                    };
                    level.Session.SetFlag(data2.Flag, state);
                    output += "Area: " + data2.Name + "\t| PrevState: " + prevState + "\t| NewState: " + state;
                }
            }
            else
            {
                output = "Area \"" + area + "\" does not exist.";
            }
            //Engine.Commands.Log(output);
        }
    }

    [CustomEntity("PuzzleIslandHelper/SceneSwitch")]
    [Tracked]
    public class SceneSwitch : Trigger
    {
        public enum Sides
        {
            None,
            Top = 3,
            Bottom = 4,
            Left = 5,
            Right = 6
        }
        public string AreaA;
        public string AreaB;
        public string CurrentArea;
        public Sides SideA;
        public Sides SideB;

        public SceneSwitch(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            AreaA = data.Attr("areaA");
            AreaB = data.Attr("areaB");
            SideA = data.Enum<Sides>("sideA");
            SideB = data.Enum<Sides>("sideB");
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            
            if (ExitingSide(player, SideA))
            {
                Transition(AreaA);
            }
            else if (ExitingSide(player, SideB))
            {
                Transition(AreaB);
            }
        }
        public void Transition(string area)
        {
            if (area != CurrentArea)
            {
                ChangeArea(area);
            }
        }

        public bool ExitingSide(Player player, Sides side)
        {
            return GetPositionLerp(player, (PositionModes)(int)side) == 0;
        }
        public void ChangeArea(string area)
        {
            if (Scene is not Level level) return;
            Audio.Play("event:/PianoBoy/invertGlitch2");
            CurrentArea = area;
            AreaFlagHelper.SetFlags(level, area);
            AreaFlagHelper.SetLighting(level, area);
        }
    }
}
