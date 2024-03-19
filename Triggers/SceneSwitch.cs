
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using static Celeste.Trigger;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    public class AreaFlagData
    {
        public bool IncludeBackend;
        public float Lighting;
        public AreaFlagData(bool includeBackend, float lighting)
        {
            IncludeBackend = includeBackend;
            Lighting = lighting;
        }
    }
    [Tracked]
    public class AreaFlagHelper : Entity
    {
        public static AreaFlagData Backend = new AreaFlagData(false, 0.05f);
        public static AreaFlagData Forest = new AreaFlagData(false, 0);
        public static AreaFlagData Pipes = new AreaFlagData(true, -1);
        public static AreaFlagData Resort = new AreaFlagData(false, -1);
        public static AreaFlagData Lab = new AreaFlagData(true, -1);
        public static Dictionary<string, AreaFlagData> FlagData = new()
        {
            {"InBackend",Backend },
            {"InForest", Forest },
            {"InPipes",Pipes },
            {"InResort",Resort },
            {"InLab",Lab }
        };
        public string Flag;
        public AreaFlagHelper() : base()
        {
            Tag = Tags.Global | Tags.Persistent;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            SetFlags(level, Flag);
        }

        public static void SetLighting(Level level, string key)
        {
            if (!FlagData.ContainsKey(key)) return;
            float prevLight = level.Lighting.Alpha;
            if (FlagData[key].Lighting > -1)
            {
                float num = level.Session.LightingAlphaAdd = prevLight + (FlagData[key].Lighting - prevLight);
                level.Lighting.Alpha = level.BaseLightingAlpha + num;
            }
        }
        public static void SetFlags(Level level, string flag)
        {
            if (!string.IsNullOrEmpty(flag) && FlagData.ContainsKey(flag))
            {
                PianoModule.Session.CurrentAreaFlag = flag;
                foreach (KeyValuePair<string, AreaFlagData> pair in FlagData)
                {
                    level.Session.SetFlag(pair.Key, pair.Key == flag || (pair.Key == "InBackend" && FlagData[flag].IncludeBackend));
                }
            }
        }
    }

    [CustomEntity("PuzzleIslandHelper/SceneSwitch")]
    [Tracked]
    public class SceneSwitch : Trigger
    {
        public enum Areas
        {
            None,
            Backend,
            Forest,
            Pipes,
        }
        public enum Sides
        {
            None,
            Top = 3,
            Bottom = 4,
            Left = 5,
            Right = 6
        }
        public Areas AreaA;
        public Areas AreaB;
        public Areas CurrentArea;
        public Sides SideA;
        public Sides SideB;

        public SceneSwitch(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            AreaA = data.Enum<Areas>("areaA");
            AreaB = data.Enum<Areas>("areaB");
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
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            foreach (AreaFlagHelper helper in level.Tracker.GetEntities<AreaFlagHelper>())
            {
                if (CurrentArea.ToString() == "None") return;
                helper.Flag = "In" + CurrentArea.ToString();
            }
        }
        public void Transition(Areas area)
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
        public void ChangeArea(Areas area)
        {
            if (Scene is not Level level) return;
            Audio.Play("event:/PianoBoy/invertGlitch2");
            CurrentArea = area;
            PianoModule.Session.CurrentBackdropArea = area;
            AreaFlagHelper.SetLighting(level, "In" + area.ToString());
        }
        internal static void Load()
        {
            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
        }
        internal static void Unload()
        {
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
        }

        private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);
            self.Level.Add(new AreaFlagHelper());
        }
    }
}
