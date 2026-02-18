using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Hints
{
    [CustomEntity("PuzzleIslandHelper/FallenOrbsPuzzle")]
    [Tracked(false)]
    public class FallenOrbsPuzzle : Entity
    {
        public static FlagData Revealed = new FlagData("FallenOrbsPuzzleRevealed");
        private class fallingOrb : Actor
        {
            public FrequencyOrbComponent Orb;
            private int index;
            public float FallSpeed;
            public bool Falling;
            private float bounceMult = 1;
            public Vector2 orig;
            private bool simulatedWhenAdded;
            private bool revealed;
            public bool Simulated
            {
                get => SceneAs<Level>().Session.GetFlag("FallenOrb:" + index + ":Simulated");
                set => SceneAs<Level>().Session.SetFlag("FallenOrb:" + index + ":Simulated", value);
            }
            public float DeadOffset
            {
                get
                {
                    Level level = Scene as Level;
                    string path = "FallenOrb:" + index + ':';
                    return level.Session.GetSlider(path + "Y:");
                }
                set
                {
                    Level level = Scene as Level;
                    string path = "FallenOrb:" + index + ':';
                    level.Session.SetSlider(path + "Y:", value);
                }
            }
            public fallingOrb(Vector2 position, int index, bool revealed) : base(position)
            {
                this.revealed = revealed;
                orig = position;
                this.index = index;
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Orb = new FrequencyOrbComponent("objects/PuzzleIslandHelper/forkAmp/orbCracked", index, Vector2.Zero, Vector2.Zero, Color.White, Color.Blue, 1, false)
                {
                    CanFade = false,
                    Alpha = 1,
                    CanMove = false
                };
                Add(Orb);
                Collider = new Hitbox(Orb.Width, Orb.Height, -Orb.Width / 2, -Orb.Height / 2);
                if (Simulated)
                {
                    Position.Y += DeadOffset;
                    if (Y > (scene as Level).Bounds.Bottom)
                    {
                        RemoveSelf();
                    }
                    simulatedWhenAdded = true;
                }
            }
            public bool HasFallen;
            public override void Render()
            {
                Orb.Render();
            }
            public void StartFalling()
            {
                HasFallen = true;
                Falling = true;
            }
            public void StopFalling()
            {
                Falling = false;
                FallSpeed = 0;
            }
            public void Simulate()
            {
                if (Simulated) return;
                Vector2 prevPosition = Position;
                Position = orig;
                bounceMult = 1;
                FallSpeed = 0;
                StartFalling();
                Level level = SceneAs<Level>();
                Falling = true;
                while (Y > level.Bounds.Bottom && Falling)
                {
                    if (Falling)
                    {
                        FallUpdate();
                    }
                }
                DeadOffset = orig.Y - Position.Y;

                Position = prevPosition;
                bounceMult = 1;
                FallSpeed = 0;
                Simulated = true;
                if (Y > level.Bounds.Bottom)
                {
                    RemoveSelf();
                }
            }
            public override void Update()
            {
                base.Update();
                if (!simulatedWhenAdded)
                {
                    if (Falling)
                    {
                        FallUpdate();
                    }
                }
            }
            public void FallUpdate()
            {
                FallSpeed = Calc.Approach(FallSpeed, 130f, 900f * Engine.DeltaTime);
                MoveV(FallSpeed * Engine.DeltaTime, OnCollideV);

            }
            public void OnCollideV(CollisionData data)
            {
                if (FallSpeed > 20)
                {
                    FallSpeed *= -0.6f * bounceMult;
                    bounceMult = Calc.Approach(bounceMult, 0.05f, 0.1f);
                }
                else
                {
                    FallSpeed = 0;
                    Falling = false;
                }
            }
        }
        private class hint : MusicHint
        {
            public hint(Vector2 position, float radius, float angleDegrees, float rateA, float rateB, float rateC, float rateD, FlagList flagOnFinish) : base(position, radius, angleDegrees, false, false, [rateA, rateB, rateC, rateD], flagOnFinish)
            {
                Visible = false;
                Active = false;
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Orbs = null;
            }
        }
        [Tracked]
        private class doorEntity : Entity
        {
            public doorEntity(Vector2 position, float width, float height) : base(position)
            {
                Collider = new Hitbox(width, height);
            }
            public override void Render()
            {
                base.Render();
                if (Height > 0)
                {
                    Draw.Rect(Collider, Color.Gray);
                }
            }
        }
        private doorEntity door;
        private hint musicHint;
        private float[] rates;
        private fallingOrb[] orbs = new fallingOrb[4];
        private DotX3 talk;
        public float RevealAmount
        {
            get => revealamount;
            set
            {
                revealamount = value;
                door.Collider.Height = (1 - value) * Height;
            }
        }
        private float revealamount;
        public FallenOrbsPuzzle(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 10;
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            rates = [data.Float("rateA"), data.Float("rateB"), data.Float("rateC"), data.Float("rateD")];
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            float bottom = Bottom;
            while (level.Bounds.Bottom > bottom && !Scene.CollideCheck<Solid>(new Vector2(X, bottom++))) { }
            talk = new DotX3(0, 0, Width, Height + bottom - Bottom, Collider.HalfSize, p =>
            {
                Add(new Coroutine(routine(p)));
            });
            Add(talk);
            float radius = Math.Min(Width, Height) / 2;
            musicHint = new hint(Center - Vector2.One * radius, radius, 45f, rates[0], rates[1], rates[2], rates[3], default);
            scene.Add(musicHint);
            musicHint.Depth = 9;
            musicHint.Active = Revealed;
            float angle = musicHint.Angle + MathHelper.PiOver2;
            float maxLength = musicHint.Radius;
            for (int i = 0; i < 4; i++)
            {
                Vector2 from = Center;
                Vector2 to = from + Calc.AngleToVector(angle, maxLength);
                angle += MathHelper.PiOver2;
                float lerp = rates[i] / FrequencyData.Max;
                Vector2 position = Vector2.Lerp(from, to, lerp);
                orbs[i] = new fallingOrb(position, i, Revealed);
                orbs[i].Depth = 8;
            }
            scene.Add(orbs);

            door = new doorEntity(Position, Width, Height);
            scene.Add(door);
            door.Depth = 7;
            if (Revealed)
            {
                RevealAmount = 1;
                musicHint.Visible = true;
            }
        }
        public override void Update()
        {
            base.Update();
            talk.Enabled = !Revealed;
        }
        //if the player skips this by mistake, they can't see it again. therefore don't use a cutscene entity for this
        public IEnumerator routine(Player player)
        {
            Level level = SceneAs<Level>();
            player.DisableMovement();
            musicHint.Active = true;
            musicHint.Visible = true;

            foreach (var o in orbs)
            {
                o.Simulated = false;
                o.Simulate();
            }
            yield return CutsceneEntity.CameraTo((Center - new Vector2(180, 90)).Clamp(level.Bounds), 1, Ease.CubeOut);
            yield return 1f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
            {
                RevealAmount = i;
                for (int j = 0; j < 4; j++)
                {
                    var o = orbs[j];
                    if (!o.HasFallen && o.Bottom < door.Top)
                    {
                        o.StartFalling();
                    }
                }
                yield return null;
            }
            foreach (var o in orbs)
            {
                if (!o.HasFallen)
                {
                    o.StartFalling();
                }
            }
            yield return 1;
            player.EnableMovement();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            musicHint.RemoveSelf();
            door.RemoveSelf();
            orbs.RemoveSelves();
        }

        public override void Render()
        {
            Draw.Rect(Collider, Color.Black);
        }
    }
}