using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class LHLData
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Origin;
        public Vector2 ImpactPosition;
        public Vector2 LightPosition;
        public bool Lost;
        public LHLData(Vector2 position, Vector2 collisionPosition, Vector2 origin, float rotation, Vector2 lightPosition)
        {
            Position = position;
            Rotation = rotation;
            Origin = origin;
            ImpactPosition = collisionPosition;
            LightPosition = lightPosition;
        }
        public LHLData()
        {
            Lost = true;
        }
    }
    /*    [CustomEntity("PuzzleIslandHelper/LabLightRenderer")]
        [Tracked]
        public class LabLightRenderer : Entity
        {
            public bool NormalLight => PianoModule.Session.DEBUGBOOL2;
            public float PowerScalar = 1;
            public VirtualRenderTarget Target;

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
                Target = null;
            }
            public LabLightRenderer() : base(Vector2.Zero)
            {
                Target = VirtualContent.CreateRenderTarget("LampTarget", 320, 180);
                Depth = -8001;
                Add(new BeforeRenderHook(BeforeRender));
                Tag |= Tags.TransitionUpdate | Tags.Persistent;
            }
            public LabLightRenderer(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
            }
            public void BloomRender(Level level)
            {
                foreach (LabHangingLamp lamp in level.Tracker.GetEntities<LabHangingLamp>())
                {
                    if (!lamp.Broken)
                    {
                        lamp.vertices[0].Color = Color.LightYellow * lamp.Opacity * lamp.FlickerScalar * PowerScalar;
                        PowerScalar = PianoModule.Session.RestoredPower && !lamp.FixedOpacity ? 0.5f : 0.7f;
                        GFX.DrawVertices(level.Camera.Matrix, lamp.vertices, lamp.vertices.Length);
                    }
                }
            }
            public void BeforeRender()
            {
                if (!PianoModule.Session.RestoredPower && Scene is Level level)
                {
                    Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
                    Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                    Draw.SpriteBatch.Begin();
                    BloomRender(level);
                    Draw.SpriteBatch.End();
                }
            }
            public override void Render()
            {
                base.Render();
                if (!PianoModule.Session.RestoredPower)
                {
                    //Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
                }
            }
        }*/
    [CustomEntity("PuzzleIslandHelper/LabHangingLamp")]
    [Tracked]
    public class LabHangingLamp : Entity
    {
        public bool FixedOpacity;
        public float FlickerScalar = 1;
        public float FlickerScalar2 = 1;
        private bool Collided;
        public float Opacity;
        public readonly int Length;
        private List<Image> images = new List<Image>();
        private BloomPoint bloom;
        private VertexLight light;
        private float speed;
        private float rotation;
        private float soundDelay;
        private SoundSource sfx;
        public Image Lamp;
        public Actor Head;
        public bool Broken;
        public bool Falling;
        private float Degradation;
        private float BreakingPoint = 2;
        public static float AngleOffset = 25f;
        public bool NormalLight => PianoModule.Session.DEBUGBOOL2;
        private int WearAndTearGrade = 0;
        private float MaxSpeed = 20;
        private float SpeedMult = 1;
        private Vector2 LampSpeed;
        private EntityID id;
        private LHLData LampData;
        private float LastRotation;
        private bool HomeRun;
        public bool DependsOnLab = true;
        public FlagList OnFlag;
        private MTexture onTexture => GFX.Game["objects/PuzzleIslandHelper/hangingLamp"];
        private MTexture brokenTexture => GFX.Game["objects/PuzzleIslandHelper/hangingLampBroken"];
        private MTexture flickerTexture => GFX.Game["objects/PuzzleIslandHelper/hangingLampFlicker"];
        private bool disableLight;
        public VertexPositionColor[] vertices = new VertexPositionColor[3];
        public LabHangingLamp(Vector2 position, EntityID id, int length, float alpha, bool staticOpacity, bool dependsOnLab, bool broken, FlagList flag = default) : base(position + Vector2.UnitX * 4)
        {
            Tag |= Tags.TransitionUpdate;
            this.id = id;
            Opacity = alpha;
            FixedOpacity = staticOpacity;
            Broken = broken;
            OnFlag = flag;
            Length = Math.Max(16, length);
            Depth = -1;
            DependsOnLab = dependsOnLab;
            if (PianoModule.Session.BrokenLamps.TryGetValue(id, out LHLData value))
            {
                LampData = value;
                Broken = true;
                Falling = true;
            }
            Add(new Coroutine(randomFlickerRoutine(), false));
        }
        public LabHangingLamp(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, id, Math.Max(16, data.Height), data.Float("alpha", 1), data.Bool("staticOpacity"), data.Bool("dependsOnLab", true), data.Bool("broken"), data.FlagList("onFlag"))
        {
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            MTexture mTexture = GFX.Game["objects/PuzzleIslandHelper/hangingLampChains"];
            Image image;
            for (int i = 0; i < Length - 8; i += 8)
            {
                Add(image = new Image(mTexture.GetSubtexture(0, 8, 8, 8)));
                image.Origin.X = 4f;
                image.Origin.Y = -i;
                images.Add(image);
            }

            Add(image = new Image(mTexture.GetSubtexture(0, 0, 8, 8)));
            image.Origin.X = 4f;
            images.Add(image);


            Add(Lamp = new Image(GFX.Game["objects/PuzzleIslandHelper/hangingLamp"]));
            Add(bloom = new BloomPoint(Vector2.UnitY * (Length - 4), 1f * Opacity, 10f));
            Add(light = new VertexLight(Color.White, 1, 16, 32));


            Add(sfx = new SoundSource());
            Collider = new Hitbox(8f, Length, -4f);
            Vector2 pos = !Broken ? new Vector2(Position.X - 4, Position.Y + Length - 8) : LampData.ImpactPosition;
            scene.Add(Head = new Actor(pos));
            Head.Collider = new Hitbox(8, 8);
            Head.Add(new StaticMover());
            if (!Broken)
            {
                Lamp.Origin.X = 4f;
                Lamp.Position.X--;
                Lamp.Origin.Y = -(Length - 8);
            }
            else if (LampData.Lost)
            {
                Lamp.Visible = bloom.Visible = light.Visible = Head.Visible = Head.Active = false;
            }
            else
            {
                Lamp.Origin = LampData.Origin;
                Lamp.Position = LampData.Position;
                Lamp.Rotation = LampData.Rotation;
                light.Position = LampData.LightPosition;
            }

            if (Lamp.Visible)
            {
                images.Add(Lamp);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            /*            Renderer = PianoUtils.SeekController<LabLightRenderer>(scene);
                        if (Renderer == null)
                        {
                            scene.Add(Renderer = new LabLightRenderer());
                        }*/
            HandleVertices();
            updateLight();
            vertices[0].Color = Color.White;
        }
        public override void Update()
        {
            base.Update();
            updateLight();
            if (Broken)
            {
                Lamp.Texture = brokenTexture;
                Lamp.Position = Head.Position - Position + new Vector2(4, 6);
            }
            else
            {
                Lamp.Texture = disableLight ? flickerTexture : onTexture;
            }
            if (!Falling && !Broken)
            {
                Head.Position.Y = Position.Y + (Length - 8);
            }
            soundDelay -= Engine.DeltaTime;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && Collider.Collide(entity))
            {
                speed = (0f - entity.Speed.X) * 0.005f * ((entity.Y - Y) / Length);
                if (!(Falling || Broken))
                {
                    LampSpeed.X = entity.Speed.X / 100;
                    LampSpeed.Y = entity.Speed.Y / 200;
                }
                if (Math.Abs(speed) < 0.1f)
                {
                    speed = 0f;
                }
                else if (soundDelay <= 0f)
                {
                    sfx.Play("event:/game/02_old_site/lantern_hit");
                    soundDelay = 0.25f;

                    if (!Falling && !Broken)
                    {
                        WearAndTearGrade++;
                        Degradation += (Math.Abs(entity.Speed.X) + Math.Abs(entity.Speed.Y) + WearAndTearGrade) * 0.005f;
                        if (Degradation > BreakingPoint)
                        {
                            Add(new Coroutine(Fall()));
                        }
                    }
                }
            }

            float num = Math.Sign(rotation) == Math.Sign(speed) ? 8f : 6f;
            if (Math.Abs(rotation) < 0.5f)
            {
                num *= 0.5f;
            }
            if (Math.Abs(rotation) < 0.25f)
            {
                num *= 0.5f;
            }

            float value = rotation;
            speed += -Math.Sign(rotation) * num * Engine.DeltaTime;
            rotation += speed * Engine.DeltaTime;
            rotation = Calc.Clamp(rotation, -0.4f, 0.4f);
            if (Math.Abs(rotation) < 0.02f && Math.Abs(speed) < 0.2f)
            {
                rotation = speed = 0f;
            }
            else if (Math.Sign(rotation) != Math.Sign(value) && soundDelay <= 0f && Math.Abs(speed) > 0.5f)
            {
                sfx.Play("event:/game/02_old_site/lantern_hit");
                soundDelay = 0.25f;
            }

            if (!Falling && !Broken)
            {
                LastRotation = rotation;
            }
            else if (Falling)
            {
                LastRotation += 0.1f;
            }

            foreach (Image image in images)
            {
                if (image == Lamp)
                {
                    if (!Broken)
                    {
                        Lamp.Rotation = LastRotation;
                    }
                }
                else
                {
                    image.Rotation = rotation;
                }
            }

            Vector2 vector = Calc.AngleToVector(rotation + (float)Math.PI / 2f, Length - 4f);
            if (!Falling && !Broken)
            {
                light.Position = bloom.Position = vector;
            }
            sfx.Position = vector;

            if ((!DependsOnLab || !PianoModule.Session.RestoredPower) && !disableLight && OnFlag)
            {
                light.Visible = true;
                bloom.Visible = true;
                HandleVertices();
            }
            else
            {
                bloom.Visible = false;
                light.Visible = false;
            }
        }
        public override void Render()
        {
            foreach (Image image in Components.GetAll<Image>())
            {
                if (image != Lamp || Lamp.Visible)
                {
                    image.DrawOutline();
                }
            }
            if (!PianoModule.Session.RestoredPower && !disableLight && !Broken && !NormalLight && Scene is Level level)
            {
                Draw.SpriteBatch.End();
                vertices[0].Color = Color.LightYellow * Opacity * FlickerScalar * FlickerScalar2;
                GFX.DrawVertices(level.Camera.Matrix, vertices, vertices.Length, null, BlendState.Additive);
                GameplayRenderer.Begin();
            }

            base.Render();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            /*            foreach (var v in vertices)
                        {
                            Draw.Rect(v.Position.XY() - Vector2.One, 3, 3, Color.Blue);
                            Draw.Point(v.Position.XY(), Color.Orange);
                            Draw.Line(v.Position.XY(), v.Position.XY() + (Vector2.UnitX * 80).Rotate(Lamp.Rotation + MathHelper.PiOver2), Color.Magenta);
                            Draw.Line(v.Position.XY(), v.Position.XY() + (Vector2.UnitX * 80).Rotate(Lamp.Rotation - MathHelper.PiOver2), Color.Cyan);
                        }*/
        }
        private IEnumerator randomFlickerRoutine()
        {
            NumRange shortRange = new NumRange(Engine.DeltaTime, 9 * Engine.DeltaTime);
            NumRange longRange = new NumRange(0.3f, 3f);
            while (true)
            {
                if (!Falling && !Broken && PianoModule.Session.PowerState == LabPowerState.Barely)
                {
                    if (Calc.Random.Chance(0.1f))
                    {
                        disableLight = false;
                        yield return Calc.Random.Choose(shortRange, longRange).Random();
                        disableLight = true;
                        yield return shortRange.Random();
                    }
                    else
                    {
                        FlickerScalar2 = 1;
                        yield return Calc.Random.Choose(shortRange, longRange).Random();
                        FlickerScalar2 = Calc.Random.Range(0.5f, 1);
                        yield return shortRange.Random();
                    }
                }
                else
                {
                    yield return null;
                }
                disableLight = false;
                FlickerScalar2 = 1;
            }
        }
        private IEnumerator Flicker(float delay)
        {
            while (!Collided)
            {
                FlickerScalar = 0.5f;
                yield return delay;
                if (Collided)
                {
                    yield break;
                }
                FlickerScalar = 0;
                yield return delay;
            }
            FlickerScalar = 0;
            yield return null;
        }
        private IEnumerator Fall()
        {
            if (Scene is not Level level || Broken) yield break;
            Celeste.Freeze(0.05f);
            Lamp.CenterOrigin();
            Lamp.Position.Y += Length - 8;
            Head.Position = Lamp.RenderPosition;
            Falling = true;
            Collider.Height -= 8;
            Add(new Coroutine(Flicker(0.1f)));
            while (!Collided)
            {
                LampSpeed.Y = Calc.Min(MaxSpeed, LampSpeed.Y + Engine.DeltaTime * SpeedMult);
                SpeedMult += 0.3f;
                Head.MoveH(LampSpeed.X, OnCollideH);
                Head.MoveV(LampSpeed.Y, OnCollideV);
                Lamp.Position = Head.Position - Position + new Vector2(4, 6);
                HandleVertices();
                bloom.Position = Head.Position;
                light.Position = Head.Position;
                if (HomeRun)
                {
                    level.Camera.Position = Head.Position - new Vector2(160, 90);
                }
                yield return null;
            }
            light.Visible = false;
            if (HomeRun)
            {
                Add(new Coroutine(HoldCamera()));
            }
            if (!PianoModule.Session.BrokenLamps.ContainsKey(id))
            {
                Vector2 abs = Lamp.Position + Position;
                if (abs.X < level.LevelOffset.X || abs.X > level.LevelOffset.X + level.Bounds.Width
                || abs.Y > level.LevelOffset.Y + level.Bounds.Height)
                {
                    Lamp.Visible = false;
                    PianoModule.Session.BrokenLamps.Add(id, new LHLData());
                    //mark lamp as "lost", as it is out of the level bounds and cannot reenter.
                }
                else
                {
                    PianoModule.Session.BrokenLamps.Add(id, new LHLData(Lamp.Position, Head.Position, Lamp.Origin, Lamp.Rotation, light.Position));
                }
            }
            Broken = true;
            yield return null;
        }
        private void OnCollideH(CollisionData data)
        {
            if (LampSpeed.X > 20)
            {
                Collided = true;
            }
            else
            {
                LampSpeed.X = -LampSpeed.X * 0.7f;
            }
        }
        private void OnCollideV(CollisionData data)
        {
            if (data.Direction.Y == 1)
            {
                Collided = true;
            }
            else
            {
                LampSpeed.Y = 1;
            }
        }
        private void updateLight()
        {
            float scalar = FlickerScalar * FlickerScalar2;
            if (NormalLight)
            {
                light.Alpha = scalar * Opacity;
                light.StartRadius = 16 * scalar;
                light.EndRadius = 32 * scalar;
            }
            else
            {
                light.Alpha = 0.5f * Opacity * scalar;
                light.StartRadius = 10 * scalar;
                light.EndRadius = 20 * scalar;
            }
        }
        private void HandleVertices()
        {
            if (Head is null || Broken || NormalLight) return;
            float rot = Lamp.Rotation;
            float angleOffset = AngleOffset.ToRad();

            Vector2 dist = Vector2.UnitX * 80;
            Vector2 offset = new Vector2(4, 1);
            Vector2 center = !Falling ? Lamp.RenderPosition : Head.Position + offset;
            Vector2 top = RotatePoint(Head.Position + offset, center, rot.ToDeg());
            rot += MathHelper.PiOver2;
            vertices[0].Position = new Vector3(top, 0);
            vertices[1].Position = new Vector3(top + dist.Rotate(rot + angleOffset), 0);//RotatePoint(Vector2.UnitX * 80, top, 120 + deg), 0);
            vertices[2].Position = new Vector3(top + dist.Rotate(rot - angleOffset), 0);//new Vector3(RotatePoint(top + Vector2.UnitX * 80, top, 60 + deg), 0);
        }

        private IEnumerator HoldCamera()
        {
            for (int i = 0; i < 300; i++)
            {
                SceneAs<Level>().Camera.Position = Position + Lamp.Position - new Vector2(160, 90);
                yield return null;
            }
        }
        static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
    }
    public static class VecHelper
    {
        public static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
        public static Vector2 XZ(this Vector3 vector3) => new Vector2(vector3.X, vector3.Z)
        {
        };
        public static Vector2 XY(this Vector3 vector3) => new Vector2(vector3.X, vector3.Y)
        {
        };
        public static Vector2 YZ(this Vector3 vector3) => new Vector2(vector3.Y, vector3.Z)
        {
        };
        public static Vector2 XX(this Vector3 vector3) => new Vector2(vector3.X, vector3.X)
        {
        };
        public static Vector2 YY(this Vector3 vector3) => new Vector2(vector3.Y, vector3.Y)
        {
        };
        public static Vector2 ZZ(this Vector3 vector3) => new Vector2(vector3.Z, vector3.Z)
        {
        };
    }
}