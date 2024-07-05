using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/CenterTriangle")]
    public class CenterTriangle : Entity
    {
        public static MTexture Texture = GFX.Game["objects/PuzzleIslandHelper/polygonScreen/center"];
        public Image Triangle;
        public float ShineAmount;
        public float Alpha;
        public float LineAlpha;
        public Color LineColor;
        private int outlineOffset = 1;
        public Color Color;
        public List<Shard> Shards = new();
        public bool Shattering;
        public class Shard : Image
        {
            public bool Shattering;
            public float Angle;
            public float Speed;
            private int XScaleDir, YScaleDir, RotationDir;
            private float XScaleTime, YScaleTime, RotationRate;
            private float xEase, yEase;
            private List<Tween> tweens = new();
            private Vector2 halfSize;
            private Vector2 shardOffset;
            private float ColorLerpAmount;

            public Shard(MTexture texture, float angle, Vector2 centerTriOffset, Vector2 shardOffset) : base(texture)
            {
                Position = centerTriOffset;
                this.shardOffset = shardOffset;
                XScaleDir = Calc.Random.Choose(-1, 1);
                YScaleDir = Calc.Random.Choose(-1, 1);
                RotationDir = Calc.Random.Choose(-1, 1);
                XScaleTime = Calc.Random.Range(0.8f, 1.5f);
                YScaleTime = Calc.Random.Range(0.8f, 1.5f);
                RotationRate = Calc.Random.Range(2f, 10f).ToRad();
                Speed = Calc.Random.Range(2f, 6f);
                Angle = angle;
                CenterOrigin();
                halfSize = new Vector2(Width / 2, Height / 2);
            }
            public override void Render()
            {
                if (Shattering)
                {
                    Vector2 prevPosition = Position;
                    Color prevColor = Color;
                    Color = Color.Lerp(Color, Color.PaleGoldenrod, ColorLerpAmount);
                    Position += shardOffset + halfSize;
                    DrawOutline(Color.PaleGoldenrod);
                    base.Render();
                    Position = prevPosition;
                    Color = prevColor;
                }
            }
            public void Shatter()
            {
                if (Entity != null)
                {
                    Shattering = true;
                    Tween xScale = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.Linear, XScaleTime, true);
                    Tween yScale = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.Linear, YScaleTime, true);

                    xScale.OnUpdate = (Tween t) => { xEase = t.Eased; };
                    yScale.OnUpdate = (Tween t) => { yEase = t.Eased; };

                    Entity.Add(xScale, yScale);
                    tweens.Add(xScale);
                    tweens.Add(yScale);
                }
            }
            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                foreach (Tween t in tweens)
                {
                    entity.Remove(t);
                }
                tweens.Clear();
            }
            public override void Update()
            {
                base.Update();
                if (Shattering)
                {
                    Position += Calc.AngleToVector(Angle, Speed);
                    Scale.X = XScaleDir + (xEase * Math.Sign(XScaleDir) * -2);
                    Scale.Y = YScaleDir + (yEase * Math.Sign(YScaleDir) * -2);
                    Rotation = (Rotation + RotationDir * RotationRate) % MathHelper.TwoPi;
                    ColorLerpAmount = 1 - Scale.X;
                }
            }

        }
        private Vector2[] ShardOffsets = new Vector2[]
        {
            new(0,0), new(0,0), new(0,0), new(0,0), new(0,0), new(0,0), new(0,0), new(0,0), new(0,0), new(0,0), new(0,0),
        };
        private float[] ShardAngles = new float[]
        {
            100, 20, 10,0,0,0,0,0,0,0,0,0,0,0
        };
        public CenterTriangle() : base(Vector2.Zero)
        {
            Depth = -10002;
            Add(Triangle = new Image(Texture));
            Triangle.Position = (new Vector2(320, 180) / 2) - (new Vector2(Triangle.Width, Triangle.Height) / 2);
            MTexture[] array = GFX.Game.GetAtlasSubtextures("objects/PuzzleIslandHelper/polygonScreen/centerShards").ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                Shard shard = new Shard(array[i], ShardAngles[i].ToRad(), Position, ShardOffsets[i]);
                Shards.Add(shard);
                Add(shard);
            }
        }
        public CenterTriangle(EntityData data, Vector2 offset) : this()
        {

        }
        public void Shatter()
        {
            Shattering = true;
            foreach (Shard s in Shards)
            {
                s.Shatter();
            }
        }
        public override void Update()
        {
            base.Update();
            foreach (Shard s in Shards)
            {
                s.Color = Color;
            }
        }
        public override void Render()
        {
            Vector2 camPos = SceneAs<Level>().Camera.Position;
            Position += camPos;
            if (Shattering)
            {
                foreach (Shard s in Shards)
                {
                    s.Render();
                }
            }
            else
            {
                if (LineAlpha > 0)
                {
                    Triangle.DrawOutline(LineColor * LineAlpha, outlineOffset);
                }
                Triangle.Color = Color * Alpha;
            }
            Position -= camPos;
        }
        public void IntoPolyscreen()
        {
            Add(new Coroutine(ShineIn()));
        }
        public IEnumerator ShineIn()
        {
            ShineAmount = 1;
            yield return null;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Alpha = i;
                yield return null;
            }
            Alpha = 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                ShineAmount = 1 - i;
                yield return null;
            }
            ShineAmount = 0;
        }
        public void FadeConnectBegin(float time)
        {
            Add(new Coroutine(PrimitaveLineFade(time)));
        }
        private IEnumerator PrimitaveLineFade(float time)
        {
            LineColor = Color.White;
            for (float i = 0; i < 1; i += Engine.DeltaTime / (time * 0.7f))
            {
                LineAlpha = Ease.SineInOut(i);
                yield return null;
            }
            LineAlpha = 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime / (time - (time * 0.7f)))
            {
                LineColor = Color.Lerp(Color.White, Color.PaleGoldenrod, Ease.SineInOut(i));
                yield return null;
            }
        }
    }
}
