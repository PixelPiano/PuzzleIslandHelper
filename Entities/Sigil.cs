using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    //wip act 1 code puzzle entity
    public class Sigil : Entity
    {
        public Color Color;
        public float AlphaA, AlphaB, AlphaC, AlphaD;
        public float Rotation;
        public int Steps;
        private float[] angleOffsets = [MathHelper.Pi, 0, MathHelper.PiOver2 * 3];
        public FlagList FlagOnComplete;
        public FlagList InPlayFlag;
        public string FailTag;
        public bool OncePerSession;
        private EntityID id;
        public Sigil(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            Rotation = data.Float("rotation");
            Collider = new Hitbox(data.Width, data.Width, -data.Width / 2, -data.Width / 2);
            Add(new DashListener(OnDash));
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.LineAngle(Center, Rotation + angleOffsets[0], Width / 2, Color.Yellow);
            Draw.LineAngle(Center, Rotation + angleOffsets[1], Width / 2, Color.Cyan);
            Draw.LineAngle(Center, Rotation + angleOffsets[2], Width / 2, Color.Magenta);
        }
        public void OnDash(Vector2 direction)
        {
            if (Steps < 4)
            {
                //float desiredAngle = (Rotation + )
            }

        }
    }
}