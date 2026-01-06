using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Glimmer")]
    [Tracked]
    public class GlimmerEntity : Entity
    {
        public Glimmer Glimmer;
        public FlagList Flag;
        public GlimmerEntity(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Flag = data.FlagList("flag");
            Add(Glimmer = new Glimmer(data.HexColor("centerColor", Color.White), data.HexColor("edgeColor", Color.Transparent))
            {
                Size = Math.Max(data.Width, data.Height),
                FadeX = data.Bool("fadeX"),
                FadeY = data.Bool("fadeY"),
                Flashes = data.Bool("flashes"),
                FlashIntensity = data.Float("flashIntensity"),
                FlashDelay = data.Float("flashDelay"),
                FlashAttack = data.Float("flashAttack"),
                FlashSustain = data.Float("flashSustain"),
                FlashRelease = data.Float("flashRelease"),
                FlashWait = data.Float("flashWait"),
                SolidColor = data.Bool("solidColor"),
                LineWidth = data.Int("minLineWidth"),
                LineWidthTarget = data.Int("maxLineWidth"),
                MaxAngle = data.Float("maxAngle", 360),
                MinAngle = data.Float("minAngle"),
                LineOffset = data.Int("lineOffset", 4),
                RotationInterval = data.Float("rotateInterval", Engine.DeltaTime),
                Lines = data.Int("lines", 8),
                FadeWhenBlocked = data.Bool("fadeWhenBlocked"),
                FadeThresh = data.Float("fadeThresh"),
                RotationRate = data.Float("rotateRate"),
                BaseAlpha = data.Float("alpha", 1),
            });
            Tag |= Tags.TransitionUpdate;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Flag)
            {
                Glimmer.Visible = false;
            }
        }
        public override void Update()
        {
            base.Update();
            Glimmer.Visible = Flag;
        }
    }
}
