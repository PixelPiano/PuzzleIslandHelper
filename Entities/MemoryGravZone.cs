using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MemoryGravZone")]
    [Tracked]
    public class MemoryGravZone : Entity
    {
        public string Key;
        public FlagList VisibleFlag;
        public FlagList ActiveFlag;
        public Color Color;
        public float Value;
        public struct MemoryParticle
        {
            public MemoryGravZone Parent;
            public MemoryParticle(MemoryGravZone parent, Vector2 position)
            {
                Parent = parent;
                BasePosition = position;
                Position = position;

            }
            public Vector2 Position;
            public Vector2 CrystalCenter => Parent.Center;
            public Vector2 BasePosition;
            public float Agitation;
            public float ShakeMult;
            private Vector2 shakeVector;
            public Vector2 TargetOffset;
            public float Speed = 1;
            public float CenterLerp;
            public Color RealColor => Color.Lerp(Color1, Color2, ColorLerp);
            public Color Color1 => Parent.Color;
            public Color Color2 => Color.Lerp(Parent.Color, Calc.Random.Choose(Color.White, Color.Black), 0.4f);
            public float ColorLerp;
            public void Update(Scene scene)
            {
                if (scene.OnInterval(0.4f))
                {
                    shakeVector = Calc.Random.ShakeVector();
                }
                if (scene.OnInterval(Calc.Clamp(1 - Agitation, 0, 1) * 2))
                {
                    TargetOffset = Calc.Random.ShakeVector() * 4;
                }
                ColorLerp = (float)Math.Sin(scene.TimeActive * (0.1f + Agitation * 5) + 1) / 2f * Agitation;
                Vector2 basePos = Vector2.Lerp(BasePosition, CrystalCenter, CenterLerp);
                Vector2 target = basePos + TargetOffset * Agitation + shakeVector * Agitation;
                Position += (target - Position) * (1f - (float)Math.Pow(0.0099999997764825821, Speed * Engine.DeltaTime));

            }
        }
        
        public MemoryGravZone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Key = data.Attr("key");
            VisibleFlag = data.FlagList("visibleFlag");
            ActiveFlag = data.FlagList("activeFlag");
            Color = data.HexColor("color");
        }
    }
}
