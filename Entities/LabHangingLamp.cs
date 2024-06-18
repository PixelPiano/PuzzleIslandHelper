using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    public class LHLData
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Origin;
        public Vector2 CollisionPosition;
        public Vector2 LightPosition;
        public LHLData(Vector2 position, Vector2 collisionPosition, Vector2 origin, float rotation, Vector2 lightPosition)
        {
            Position = position;
            Rotation = rotation;
            Origin = origin;
            CollisionPosition = collisionPosition;
            LightPosition = lightPosition;

        }
    }
    [CustomEntity("PuzzleIslandHelper/LabLightRenderer")]
    [Tracked]
    public class LabLightRenderer : Entity
    {
        public VertexPositionColor[] vertices = new VertexPositionColor[3];
        private Level level;
        public bool Broken;
        public float PowerScalar = 1;
        private static VirtualRenderTarget _Target;
        public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("LampTarget", 320, 180);

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _Target?.Dispose();
            _Target = null;
        }
        public LabLightRenderer(Scene scene) : base(Vector2.Zero)
        {
            level = scene as Level;
            Depth = -8001;
            Add(new BeforeRenderHook(BeforeRender));
            Tag |= Tags.TransitionUpdate;
            Tag |= Tags.Global;
            Add(new CustomBloom(delegate { BloomRender(false); }));
        }
        public override void Update()
        {
            base.Update();
            level = Engine.Scene as Level;
        }
        public LabLightRenderer(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
        }
        public void BloomRender(bool fromRender)
        {
            if (!fromRender)
            {
                return;
            }
            if (vertices.Length > 0 && !Broken)
            {

                foreach (LabHangingLamp lamp in level.Tracker.GetEntities<LabHangingLamp>())
                {

                    if (!lamp.Broken)
                    {
                        if (!fromRender)
                        {
                            lamp.vertices[0].Color = Color.White;
                        }
                        else
                        {
                            Color c = Color.LightYellow;
                            lamp.vertices[0].Color = c;
                        }

                        PowerScalar = PianoModule.Session.RestoredPower && !lamp.FixedOpacity ? 0.5f : 1;
                        lamp.vertices[0].Color *= lamp.Opacity * lamp.FlickerScalar * PowerScalar * (fromRender ? 1 : 0.5f);

                        GFX.DrawVertices(level.Camera.Matrix, lamp.vertices, lamp.vertices.Length);
                    }
                }
            }
        }
        public void BeforeRender()
        {
            if (!PianoModule.Session.RestoredPower)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin();
                BloomRender(true);
                Draw.SpriteBatch.End();
            }
        }
        public override void Render()
        {
            base.Render();
            if (!PianoModule.Session.RestoredPower)
            {
                Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/LabHangingLamp")]
    [Tracked]
    public class LabHangingLamp : Entity
    {
        public bool FixedOpacity;
        public float FlickerScalar = 1;
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
        public Actor Collision;
        public bool Broken;
        public bool Falling;
        private float Degradation;
        private float BreakingPoint = 2;
        private int WearAndTearGrade = 0;
        private float MaxSpeed = 20;
        private float SpeedMult = 1;
        private Vector2 LampSpeed;
        private EntityID id;
        private LHLData LampData;
        private float LastRotation;
        private bool HomeRun;

        public VertexPositionColor[] vertices = new VertexPositionColor[3];
        private Level level;

        public LabHangingLamp(Vector2 position, int length, EntityID id, EntityData data)
        {
            Tag |= Tags.TransitionUpdate;
            this.id = id;
            Opacity = data.Float("alpha", 1);
            FixedOpacity = data.Bool("staticOpacity");
            if (PianoModule.Session.BrokenLamps.Keys.Contains(id))
            {
                LampData = PianoModule.Session.BrokenLamps[id];
                Broken = true;
                Falling = true;
            }
            Position = position + Vector2.UnitX * 4f;
            Length = Math.Max(16, length);
            Depth = 2000;
            MTexture mTexture = GFX.Game["objects/hanginglamp"];
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



            Add(Lamp = new Image(GFX.Game["objects/PuzzleIslandHelper/hangingLamp"]));
            Add(bloom = new BloomPoint(Vector2.UnitY * (Length - 4), 1f * Opacity, 10f));
            Add(light = new VertexLight(Color.White, 0.5f * Opacity, 10, 20));
            if (!Broken)
            {

                Lamp.Origin.X = 4f;
                Lamp.Position.X--;
                Lamp.Origin.Y = -(Length - 8);
            }
            else if (LampData.Position == Vector2.Zero)
            {
                Lamp.Visible = false;
                //bloom.Visible = false;
                light.Visible = false;
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

            Add(sfx = new SoundSource());
            Collider = new Hitbox(8f, Length, -4f);
        }

        public LabHangingLamp(EntityData e, Vector2 position, EntityID id)
            : this(e.Position + position, Math.Max(16, e.Height), id, e)
        {
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
            if (Broken)
            {
                yield break;
            }
            Celeste.Freeze(0.05f);
            Lamp.CenterOrigin();
            Lamp.Position.Y += Length - 8;
            Collision.Position = Lamp.RenderPosition;
            Falling = true;
            Collider.Height -= 8;
            Add(new Coroutine(Flicker(0.1f)));
            while (!Collided)
            {
                LampSpeed.Y = Calc.Min(MaxSpeed, LampSpeed.Y + Engine.DeltaTime * SpeedMult);
                SpeedMult += 0.3f;
                Collision.MoveH(LampSpeed.X, OnCollideH);
                Collision.MoveV(LampSpeed.Y, OnCollideV);
                Lamp.Position = Collision.Position - Position + new Vector2(4, 6);
                HandleVertices();
                //bloom.Position = Collision.Position;
                light.Position = Collision.Position;
                if (HomeRun)
                {
                    level.Camera.Position = Collision.Position - new Vector2(160, 90);
                }
                yield return null;
            }
            Remove(light);
            if (HomeRun)
            {
                Add(new Coroutine(HoldCamera()));
            }
            if (!PianoModule.Session.BrokenLamps.Keys.Contains(id))
            {
                Vector2 abs = Lamp.Position + Position;
                if (abs.X < level.LevelOffset.X || abs.X > level.LevelOffset.X + level.Bounds.Width
                || abs.Y > level.LevelOffset.Y + level.Bounds.Height)
                {
                    Lamp.Visible = false;
                    PianoModule.Session.BrokenLamps.Add(id, new LHLData(Vector2.Zero, Vector2.Zero, Vector2.Zero, 0, Vector2.Zero));
                }
                else
                {
                    PianoModule.Session.BrokenLamps.Add(id, new LHLData(Lamp.Position, Collision.Position, Lamp.Origin, Lamp.Rotation, light.Position));
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
        public override void Update()
        {
            #region Lamp
            base.Update();
            if (Broken)
            {
                Lamp.Texture = GFX.Game["objects/PuzzleIslandHelper/hangingLampBroken"];
                Lamp.Position = Collision.Position - Position + new Vector2(4, 6);
            }
            if (!Falling && !Broken)
            {
                Collision.Position.Y = Position.Y + (Length - 8);
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
                    if (Broken)
                    {
                        continue;
                    }
                    else
                    {
                        Lamp.Rotation = LastRotation;
                        continue;
                    }

                }
                image.Rotation = rotation;
            }

            Vector2 vector = Calc.AngleToVector(rotation + (float)Math.PI / 2f, Length - 4f);
            if (!Falling && !Broken)
            {
                light.Position = vector;
            }
            sfx.Position = vector;
            #endregion
            if (!PianoModule.Session.RestoredPower)
            {
                HandleVertices();
            }
            else
            {
                bloom.Visible = false;
            }
        }
        private void HandleVertices()
        {
            if (Collision is null || Broken)
            {
                return;
            }
            Vector2 CenterPoint;
            if (!Falling)
            {
                CenterPoint = Position + Lamp.Position;
            }
            else
            {
                CenterPoint = Collision.Position + Vector2.One * 4;
            }
            Vector2 a = RotatePoint(Collision.Position + new Vector2(4, 6), CenterPoint, Lamp.Rotation.ToDeg());
            Vector3 TopPoint = new Vector3(a, 0);
            vertices[0] = new VertexPositionColor(TopPoint, Color.White);
            float MaxLength = 80;


            Vector3 LeftPoint = new Vector3(RotatePoint(TopPoint.XY() + Vector2.UnitX * MaxLength, TopPoint.XY(), 120 + Lamp.Rotation.ToDeg()), 0);
            Vector3 RightPoint = new Vector3(RotatePoint(TopPoint.XY() + Vector2.UnitX * MaxLength, TopPoint.XY(), 60 + Lamp.Rotation.ToDeg()), 0);
            vertices[1] = new VertexPositionColor(LeftPoint, Color.Transparent);
            vertices[2] = new VertexPositionColor(RightPoint, Color.Transparent);

        }
        public override void Render()
        {
            foreach (Component component in Components)
            {
                if (component as Image == Lamp)
                {
                    if (Lamp.Visible)
                    {
                        (component as Image)?.DrawOutline();
                    }
                    continue;
                }
                (component as Image)?.DrawOutline();
            }
            base.Render();

        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            Vector2 pos;
            if (!Broken)
            {
                pos = new Vector2(Position.X - 4, Position.Y + Length - 8);
            }
            else
            {
                pos = LampData.CollisionPosition;
            }
            scene.Add(Collision = new Actor(pos));
            Collision.Collider = new Hitbox(8, 8);
            Collision.Add(new StaticMover());
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (PianoUtils.SeekController<LabLightRenderer>(scene) == null)
            {
                scene.Add(new LabLightRenderer(scene));
            }

            HandleVertices();
        }

        private IEnumerator HoldCamera()
        {
            for (int i = 0; i < 300; i++)
            {
                level.Camera.Position = Position + Lamp.Position - new Vector2(160, 90);
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