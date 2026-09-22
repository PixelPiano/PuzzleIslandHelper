using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    [Tracked]
    public class SingularityOrb : VertexOrb
    {
        public bool Glitchy;
        public float RadiusOffset, SizeOffset;
        public float Distance;
        public float OrbitOffset;
        private float gravity;
        private float frictionX;
        private float frictionY;
        public float WiggleXAmount = 0.25f;
        public float WiggleYAmount = 0.25f;
        public Vector2 AutoPosition => Entity.Position + Calc.AngleToVector(OrbitAngle + OrbitOffset, Distance);
        public Vector2 PrevPosition;
        public Vector2 Speed;
        public Vector2 WiggleVector;
        public bool AutoOrbit = true;
        public float OrbitAngle;
        public Wiggler WigglerX;
        public Wiggler WigglerY;
        public Wiggler ScaleWiggler;
        public Vector2 ScaleMult = Vector2.One;
        public SingularityOrb(Vector2 offset, float radius, float distance, Color fill, Color edge, Color center) : base(offset, radius, Tower.Portal.MaxOrbCorners, fill, edge, 0)
        {
            Distance = distance;
            CenterColor = center;
            Color = fill;
            Radius = radius;
            Shaker = new BetterShaker(OnShake);
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            ScaleWiggler = Wiggler.Create(0.5f, 4f, (float f) =>
            {
                WiggleVector = Vector2.One * (f * 0.25f);
            });
            WigglerX = Wiggler.Create(0.5f, 4f, (float f) =>
            {
                WiggleVector.X = (f * WiggleXAmount);
            });
            WigglerY = Wiggler.Create(0.5f, 4f, (float f) =>
            {
                WiggleVector.Y = (f * WiggleYAmount);
            });
        }
        public override void Update()
        {
            if (WigglerX.Active) WigglerX.Update();
            if (WigglerY.Active) WigglerY.Update();
            if (ScaleWiggler.Active) ScaleWiggler.Update();

            Position.X += Speed.X * Engine.DeltaTime;
            Position.Y += Speed.Y * Engine.DeltaTime;

            if (frictionX > 0) Speed.X = Calc.Approach(Speed.X, 0, frictionX * Engine.DeltaTime);
            if (frictionY > 0) Speed.Y = Calc.Approach(Speed.Y, 0, frictionY * Engine.DeltaTime);
            if (gravity != 0) Speed.Y = Calc.Approach(Speed.Y, 200 * Math.Sign(gravity), Math.Abs(gravity) * Engine.DeltaTime);
            Vector2 prevScale = Scale;
            Scale = (Scale + WiggleVector) * ScaleMult;
            base.Update();
            Scale = prevScale;
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            WigglerX.Removed(null);
            WigglerY.Removed(null);
            ScaleWiggler.Removed(null);
        }
        public void WiggleX()
        {
            WigglerX.Start();
        }
        public void WiggleY()
        {
            WigglerY.Start();
        }
        public void Wiggle()
        {
            ScaleWiggler.Start();
        }
        public IEnumerator MoveTo(Vector2 position, float time, Ease.Easer ease = null)
        {
            ease ??= Ease.Linear;
            Vector2 p = Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Position = Vector2.Lerp(p, position, ease(i));
                yield return null;
            }
            Position = position;
        }
    }
}