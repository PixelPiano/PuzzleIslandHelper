using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Core;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using IL.Celeste.Mod.Registry.DecalRegistryHandlers;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using VivHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FestivalTrailer")]
    [Tracked]
    public class FestivalTrailer : Entity
    {


        public JumpthruPlatform platform;
        public Vector2 NodePosition;
        private Vector2 orig;
        private float shakeTimer;

        public enum States
        {
            Closed,
            BackOpen,
            BothOpen
        }
        private string path = "objects/PuzzleIslandHelper/trailer/";
        public MTexture ClosedTexture, BackOpenTexture, AllOpenTexture;
        public Image Image;
        public class HelperEntity : Entity
        {
            public FestivalTrailer Trailer;
            public Image SideCoverup;
            public Image BackCoverup;
            public Image Image;
            public float SideAlpha = 1;
            public float Rotation
            {
                set
                {
                    SideCoverup.Rotation = BackCoverup.Rotation = value;
                }
            }
            public Vector2 Scale
            {
                set
                {
                    SideCoverup.Scale = BackCoverup.Scale = value;
                }
            }
            public Vector2 Origin
            {
                set
                {
                    SideCoverup.Origin = BackCoverup.Origin = value;
                }
            }
            public Vector2 ImagePosition
            {
                set
                {
                    SideCoverup.Position = BackCoverup.Position = value;
                }
            }
            public void UpdateImages(float rotation, Vector2 imagePosition, Vector2 scale, Vector2 origin)
            {
                Rotation = rotation;
                ImagePosition = imagePosition;
                Scale = scale;
                Origin = origin;
            }
            public HelperEntity(FestivalTrailer trailer, Image reference) : base(trailer.Position)
            {
                Image = reference;
                Depth = -10000;
                Trailer = trailer;
                SideCoverup = new Image(GFX.Game["objects/PuzzleIslandHelper/trailer/coverupA"]);
                BackCoverup = new Image(GFX.Game["objects/PuzzleIslandHelper/trailer/coverupB"]);
                Add(SideCoverup, BackCoverup);
                SideState(false);
                BackState(false);
                Collider = trailer.Collider;
            }
            public void SideState(bool value)
            {
                SideCoverup.Visible = value;
            }
            public void BackState(bool value)
            {
                BackCoverup.Visible = value;
            }
            public override void Render()
            {
                UpdateImages(Image.Rotation, Image.Position, Image.Scale, Image.Origin);
                base.Render();
            }
            public override void Update()
            {
                Collider = Trailer.Collider;
                /*                float target = CollideCheck<Player>() ? 0 : 1;
                                SideAlpha = Calc.Approach(SideAlpha, target, Engine.DeltaTime);*/
                SideCoverup.Color = Color.White * SideAlpha;
                Position = Trailer.Position;
                UpdateImages(Image.Rotation, Image.Position, Image.Scale, Image.Origin);
                base.Update();
            }
        }
        public States State;
        public HelperEntity Helper;
        public Entity DoorColliderEntity;
        public Solid[] Walls = new Solid[2];
        public FestivalTrailer(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            orig = Position;
            Depth = 3;
            NodePosition = data.NodesOffset(offset)[0];
            ClosedTexture = GFX.Game[path + "closed"];
            BackOpenTexture = GFX.Game[path + "backOpen"];
            AllOpenTexture = GFX.Game[path + "allOpen"];
            Image = new Image(ClosedTexture);
            Add(Image);
            Collider = new Hitbox(Image.Width, Image.Height);
            /*            
                        Add(new DebugComponent(Keys.L, Flip, true));
                        Add(new DebugComponent(Keys.K, Reset, true));
                        Add(new DebugComponent(Keys.J, crashDebug, true));*/
        }
        public void ShakeFor(float time = 0.3f)
        {
            shakeTimer = time;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Walls[0] = new Solid(Position, 8, ClosedTexture.Width - 11, true);
            Walls[1] = new Solid(Position, 4, ClosedTexture.Width - 11, true);
            scene.Add(Walls);
            platform = new JumpthruPlatform(Position + new Vector2(6, 38), 70, "default");
            scene.Add(platform);
            platform.Visible = false;
            platform.Collidable = false;
            DeactivateWalls();
            Helper = new HelperEntity(this, Image);
            Helper.Active = false;
            scene.Add(Helper);
            DoorColliderEntity = new Entity(Position + new Vector2(67, 8));

            DoorColliderEntity.Collider = new Hitbox(7, 35);
            scene.Add(DoorColliderEntity);
            DoorColliderEntity.Collidable = false;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Walls[0].RemoveSelf();
            Walls[1].RemoveSelf();
            Helper.RemoveSelf();
            DoorColliderEntity.RemoveSelf();
        }
        public Vector2 Shake;
        public override void Update()
        {
            if (DoorColliderEntity.Collidable && DoorColliderEntity.CollideCheck<Actor>())
            {
                ForceBackOpen();
            }
            Walls[0].X = X;
            Walls[1].X = Right - Walls[1].Width;
            Walls[0].Y = Walls[1].Y = Y + 5;
            if (shakeTimer > 0)
            {
                shakeTimer -= Engine.DeltaTime;
                Shake = Calc.Random.ShakeVector().XComp();
            }
            else
            {
                Shake = Vector2.Zero;
            }
            Position += Shake;
            Helper.Update();
            base.Update();
            Position -= Shake;
        }
        public void ForceBackOpen(bool shake = true)
        {
            Image.Texture = BackOpenTexture;
            State = States.BackOpen;
            if (shake)
            {
                ShakeFor(0.3f);
            }
            DoorColliderEntity.Collidable = false;
            //todo: play sound
        }
        public void ShutDoor(bool shake = true)
        {
            Image.Texture = ClosedTexture;
            State = States.Closed;
            if (shake)
            {
                ShakeFor(0.3f);
            }
        }
        public void BurstOutFront()
        {
            Image.Texture = AllOpenTexture;
            State = States.BothOpen;
            ShakeFor(0.3f);
            //todo: play sound
            //todo: add particles
        }
        public override void Render()
        {
            Position += Shake;
            Image.DrawSimpleOutline();
            base.Render();
            Position -= Shake;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(NodePosition + Vector2.UnitY * (Image.Height - Image.Width), Image.Height, Image.Width, Color.Magenta);
        }
        public void Flip()
        {
            Add(new Coroutine(flipRoutine()));
        }
        public void PrepareForGettingIn()
        {
            ShutDoor(false);
            ActivatePlatform();
            Helper.SideState(true);
            DoorColliderEntity.Collidable = true;
        }
        public void PrepareForDark()
        {
            ActivatePlatform();
            Helper.SideState(true);
            DoorColliderEntity.Collidable = false;
            ShutDoor(false);
        }
        public void ActivatePlatform()
        {
            platform.Collidable = true;
        }
        public void DeactivatePlatform()
        {
            platform.Collidable = false;
        }
        public void ActivateWalls()
        {
            Walls[0].Collidable = Walls[1].Collidable = true;
        }
        public void DeactivateWalls()
        {
            Walls[0].Collidable = Walls[1].Collidable = false;
        }
        public void Reset()
        {
            DeactivateWalls();
            State = States.Closed;
            Image.Texture = ClosedTexture;
            Image.Rotation = 0;
            Image.CenterOrigin();
            Image.Position = new Vector2(Image.Width / 2, Image.Height / 2);
            Helper.BackState(false);
            Helper.SideState(false);
            platform.Collidable = false;
            Position = orig;
            Collider = new Hitbox(ClosedTexture.Width, ClosedTexture.Height);
        }
        private void crashDebug()
        {
            SetUpCrashed();
            this.Ground();
            Helper.Position = Position;
        }
        public void SetUpCrashed()
        {
            State = States.BothOpen;
            ActivateWalls();
            Level level = Scene as Level;
            Image.Texture = AllOpenTexture;
            Image.Scale = Vector2.One;
            Image.Rotation = 90f.ToRad();
            Image.SetOrigin(0, 0);
            Image.Y = 0;
            Image.X = Image.Height;
            Collider = new Hitbox(Image.Height, Image.Width - 6);
            Position.Y = level.Bounds.Top - Image.Width;
            Position.X = NodePosition.X;
            Helper.SideState(true);
            Helper.BackState(true);
            Helper.Position = Position;
        }
        public void OnImpact()
        {
            ActivateWalls();
        }
        private IEnumerator flipRoutine()
        {
            Level level = Scene as Level;
            float xSpeed = 140f;
            float ySpeed = -380f;
            float yRate = 5f;
            float rot = 20f.ToRad();
            float rotRate = 1f.ToRad();
            float limit = level.Camera.GetBounds().Top - 24;
            while (Bottom > limit)
            {
                Position.Y += ySpeed * Engine.DeltaTime;
                Position.X += xSpeed * Engine.DeltaTime;
                Image.Scale.Y += Engine.DeltaTime * 0.1f;
                if (ySpeed < -160f) ySpeed += yRate;
                Image.Rotation = rot + rotRate;
                rotRate += 7f.ToRad();
                Helper.Position = Position;
                yield return null;
            }
            yield return 0.2f;
            ySpeed = 150f;
            yRate = 5f;
            SetUpCrashed();
            float fromX = Position.X - 40;
            float toX = Position.X;
            float target = this.GroundedPosition().Y;
            float startY = Y;
            float dist = MathHelper.Distance(startY, target);
            while (Y != target)
            {
                Position.X = Calc.LerpClamp(fromX, toX, 1 - ((target - Y) / dist));
                Y = Calc.Approach(Y, target, ySpeed * Engine.DeltaTime);
                ySpeed += yRate;
                Helper.Position = Position;
                yield return null;
            }
            Position.X = toX;
            Y = target;
            Helper.Position = Position;
            OnImpact();
            float mult;
            Vector2 pos = Position;
            for (int i = 0; i < 4; i++)
            {
                mult = 1 - (i / 4) * 0.5f;
                for (int j = -1; j < 2; j += 2)
                {
                    Position.X = pos.X + (Calc.Random.Range(1f, 2) * j * mult);
                    Helper.Position = Position;
                    yield return null;
                }
            }
            Position.X = pos.X;
            Helper.Position = Position;

        }
    }
}