using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/PrimitiveRival")]
    [Tracked]
    public class PrimitiveRival : VertexPassenger
    {
        public DotX3 Talk;
        public int TimesTalkedTo
        {
            get
            {
                if (Scene is not Level level) return 0;
                return level.Session.GetCounter("TimesTalkedToRandy");
            }
            set
            {
                if (Scene is Level level)
                {
                    level.Session.SetCounter("TimesTalkedToRandy", value);
                }
            }
        }
        public bool IntroCutscenePlayed => SceneAs<Level>().Session.GetFlag("RivalsHaveEntered");
        public PrimitiveRival(EntityData data, Vector2 offset) : base(data.Position + offset, 12, 20, null, Vector2.One, new(-1, 1), 0.9f)
        {
            MinWiggleTime = 1;
            MaxWiggleTime = 2.5f;
            float legsY = 15;

            //HEAD
            AddTriangle(new(6, 0), new(13, 6), new(0, 6), 1, Vector2.One, new(Color.LightGreen, Color.Green, Color.Turquoise));

            //LEGS
            AddTriangle(new(1, 20), new(5, 20), new(3, legsY - 1), 0.1f, Vector2.One, new(Color.Green, Color.DarkGreen, Color.Turquoise));
            AddTriangle(new(11, 20), new(7, 20), new(9, legsY - 1), 0.1f, Vector2.One, new(Color.Green, Color.DarkGreen, Color.Turquoise));

            //BODY
            AddTriangle(new(7, 6), new(12, legsY), new(1, legsY), 1, Vector2.One, new(Color.LightGreen, Color.Green, Color.Turquoise));

            //HAT
            AddQuad(new(2, -8), new(2, 0), new(9, -7), new(9, 0), 0.8f, Vector2.One, new(Color.HotPink, Color.DarkRed, Color.Red));
            //HAT BRIM
            AddTriangle(new(-1, 1), new(5, -2), new(5, 1), 0.8f, Vector2.One, new(Color.HotPink, Color.LightPink, Color.LightPink));
            AddTriangle(new(5, -3), new(11, 1), new(5, 1), 0.8f, Vector2.One, new(Color.HotPink, Color.LightPink, Color.LightPink));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            Talk = new DotX3(Collider, Interact);
            Add(Talk);
            Talk.Enabled = IntroCutscenePlayed && TimesTalkedTo < 1;
            Facing = Facings.Left;
        }
        public void Interact(Player player)
        {
            Scene.Add(new FestivalCutscenes(FestivalCutscenes.Types.Randy1));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
