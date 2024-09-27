using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class InGameLog : Component
    {
        public string ValueName;
        private DynamicData dynData;
        public string Text
        {
            get
            {
                if (Function is null)
                {
                    if (string.IsNullOrEmpty(ValueName))
                    {
                        return "null";
                    }
                    else
                    {
                        dynData.TryGet(ValueName, out var value);
                        if (value is not null)
                        {
                            if (value is float)
                            {
                                return string.Format("{0:N2}", value);
                            }
                            return value.ToString();
                        }
                        else
                        {
                            return "null";
                        }
                    }
                }
                else
                {
                    return Function.Invoke();
                }
            }
        }
        private Func<string> Function;
        public override void Added(Entity entity)
        {
            base.Added(entity);
            dynData = DynamicData.For(entity);
        }
        public InGameLog(string nameOfValue) : base(true, true)
        {
            ValueName = nameOfValue;
        }
        public InGameLog(Func<string> function) : base(true, true)
        {
            Function = function;
        }

    }
    [Tracked]
    public class InGameLogRenderer : Entity
    {
        private float space = 10;
        public InGameLogRenderer() : base(Vector2.Zero)
        {
            Tag |= TagsExt.SubHUD | Tags.TransitionUpdate | Tags.Global;
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level)
            {
                return;
            }
            float y = space;
            float x = space;
            if (level.Tracker.Components[typeof(InGameLog)] == null)
            {
                return;
            }
            foreach (InGameLog log in level.Tracker.Components[typeof(InGameLog)])
            {
                Vector2 pos = new Vector2(x, y);
                ActiveFont.DrawOutline(log.Text, pos, Vector2.Zero, Vector2.One, Color.Black, 2, Color.Black);
                ActiveFont.Draw(log.Text, pos, Color.White);
                y += ActiveFont.FontSize.HeightOf(log.Text) + space;
            }
        }
        [OnLoad]
        internal static void Load()
        {
            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
        }
        [OnUnload]
        internal static void Unload()
        {
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
        }
        private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);
            self.Level.Add(new InGameLogRenderer());
        }
    }
}