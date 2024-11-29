using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.PuzzleIslandHelper.Helpers.BitrailHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class Balloon : Entity
    {
        public Entity Track;
        public float Length;
        public Vector2 Offset;
        public Color BalloonColor, StringColor;
        public Vector2 prev;
        public float connectSpeed;
        public float connectXOffset;
        public Vector2 connect;
        public float approachMult = 0;
        public float WindMult = 1;
        private float random;
        public float speed = 0;
        public Vector2 Max;
        public static MTexture Tex => GFX.Game["objects/PuzzleIslandHelper/festival/balloonFill"];
        public Balloon(Entity track, float length, Color balloonColor, Color stringColor) : base(track.Position)
        {
            Track = track;
            Length = length;
            BalloonColor = balloonColor;
            StringColor = stringColor;
            connect = Position - Vector2.UnitY;
            Max = Calc.AngleToVector(-135f.ToRad(), Length);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            random = Calc.Random.Range(0f, 100f);
        }
        public override void Update()
        {
            float sin = (float)Math.Sin(Scene.TimeActive + random) * WindMult;
            Vector2 target = Position + new Vector2(sin * 3f, -Length);
            approachMult = Math.Abs(connect.X - Position.X) / Length + ((sin + 1) / 2) * 0.2f;
            Position = Track.Position + Offset;

            float len = (Position - connect).Length();
            if (len > Length)
            {
                float boost = Math.Abs(Position.X - prev.X);
                if (boost > 2)
                {
                    speed = Calc.Approach(speed, 120, 50f * Engine.DeltaTime);
                }
                else
                {
                    speed = Calc.Approach(speed, 30, 70f * Engine.DeltaTime);
                }
                connect = Calc.Approach(connect, target, (speed + boost) * Engine.DeltaTime * approachMult);
               
                connect = Calc.Clamp(connect, target.X + Max.X, target.Y, target.X - Max.X, Position.Y);
            }
            connect = connect + (target - connect) * (1f - (float)Math.Pow(0.7999975574f,Engine.DeltaTime));
            base.Update();
            prev = Position;


        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Position, 4, 4, Color.Orange);
            Draw.HollowRect(connect, 4, 4, Color.Cyan);
        }
        public override void Render()
        {
            Position = Track.Position + Offset;
            MTexture tex = Tex;
            Vector2 origin = new Vector2(0.5f, 1) * tex.Size();
            float angle = (Position - connect).Angle() - 90f.ToRad();
            GFX.Game["objects/PuzzleIslandHelper/festival/balloonShine"].Draw(connect, origin, Color.White, 1, angle);
            Draw.Line(Position, connect, StringColor);
            tex.Draw(connect, origin, BalloonColor, 1, angle);

            base.Render();
        }
    }
}
