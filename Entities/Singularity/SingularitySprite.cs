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
using System.Reflection.Metadata;
using static Celeste.Mod.PuzzleIslandHelper.Components.VertexOrb;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    public class SingularitySprite : GraphicsComponent
    {
        public Matrix RotationMatrix;
        public SingularityOrb R, G, B;
        public float OrbitRate = 1;
        public float Orbit;
        public float OrbitRateMult = 1;
        public bool AutoHandleOrbit = true;
        public bool AutoHandlePositions = true;
        public float[] DistanceOffsets = new float[3];
        private float Radius;
        public float Distance;
        private BetterShaker shaker;
        public Vector2 shake;
        public List<SingularityOrb> ActiveOrbs = [];
        public List<SingularityOrb> Orbs = [];
        private List<VertexOrb.AfterImage> AfterImages = [];
        public bool IncludeRed;
        public SingularitySprite(float radius, bool includeRed) : base(true)
        {
            IncludeRed = includeRed;
            Radius = radius;
            Distance = Radius / 2;
            shaker = new BetterShaker(OnShake);
        }
        public virtual void CreateAfterImage(float fillMult, float edgeMult, float scaleSpeed = 0, Vector2 speed = default)
        {
            foreach (var o in Orbs)
            {
                VertexOrb.AfterImage afterImage = new(o, 1, 1, scaleSpeed, AfterImages);
                afterImage.FillAlpha *= fillMult;
                afterImage.EdgeAlpha *= edgeMult;
                afterImage.Speed = speed;
                Entity.Add(afterImage);
            }
        }
        private void OnShake(Vector2 amount)
        {
            shake += amount;
        }
        public void ShakeFor(float time = -1)
        {
            shaker.ShakeFor(time);
        }
        public void StopShaking()
        {
            shaker.StopShaking();
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            shaker.Entity = entity;
            R = new SingularityOrb(Vector2.Zero, Radius, Distance, OrbColors.RFill, OrbColors.REdge, OrbColors.RCenter);
            G = new SingularityOrb(Vector2.Zero, Radius, Distance, OrbColors.GFill, OrbColors.GEdge, OrbColors.GCenter);
            B = new SingularityOrb(Vector2.Zero, Radius, Distance, OrbColors.BFill, OrbColors.BEdge, OrbColors.BCenter);
            Orbs = [R, G, B];
            foreach (var orb in Orbs)
            {
                orb.Entity = Entity;
                orb.Added(entity);
            }
            if (IncludeRed) ActiveOrbs.Add(R);
            ActiveOrbs.Add(G);
            ActiveOrbs.Add(B);
        }
        public Vector2 GetOrbPosition(int i)
        {
            return Position + shake + ActiveOrbs[i].AutoPosition;
        }
        public override void Update()
        {
            base.Update();
            shaker.Update();
            Vector2 offset = Position + shake;
            float inc = MathHelper.TwoPi / ActiveOrbs.Count;
            int count = 0;
            int distanceIndex = IncludeRed ? 0 : 1;
            Orbit = (Orbit + (OrbitRate * Engine.DeltaTime * OrbitRateMult)) % MathHelper.TwoPi;
            foreach (var orb in ActiveOrbs)
            {
                if (orb.Active)
                {
                    orb.ScaleMult = Scale;
                    orb.Radius = (Entity as SingularityActor).Radius;
                    orb.Distance = Distance + DistanceOffsets[distanceIndex];
                    orb.AutoOrbit = !AutoHandleOrbit;
                    if (AutoHandleOrbit)
                    {
                        orb.UpdateOrbit(Orbit, count * inc);
                    }
                    if (AutoHandlePositions)
                    {
                        orb.RenderPosition = orb.AutoPosition;
                    }
                    orb.Position += offset;
                    orb.Update();
                    orb.Position -= offset;
                    count++;
                }
                distanceIndex++;
            }
        }
        public override void Render()
        {
            base.Render();
            GameplayRenderer.End();
            PianoUtils.DrawUserPrimitives<VertexPositionColor>(SceneAs<Level>().Camera.Matrix, ForEachPass);
            GameplayRenderer.Begin();
        }

        public void ForEachPass(EffectPass pass)
        {
            foreach (var a in AfterImages)
            {
                a.DirectRenderVertices();
            }
            if (R.Visible && IncludeRed) R.DirectRenderVertices();
            if (G.Visible) G.DirectRenderVertices();
            if (B.Visible) B.DirectRenderVertices();
        }
    }
}