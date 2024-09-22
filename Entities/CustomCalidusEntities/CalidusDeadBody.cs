using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using FMOD.Studio;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities
{
    [Tracked]
    public class CalidusDeadBody : PlayerDeadBody
    {
        public PlayerCalidus Player;
        public CalidusSprite Sprite;
        private DeadEye eye;
        public Action OnEnd;
        public CalidusDeadBody(PlayerCalidus calidus, Vector2 direction, Action onEnd) : base(calidus, direction)
        {
            OnEnd = onEnd;
            base.Depth = -1000000;
            Player = calidus;
            facing = Player.Facing;
            Position = Player.Position;
            Remove(hair);
            Remove(sprite);
            Components.RemoveAll<Coroutine>();
            Add(Sprite = Player.sprite);
            Add(light = Player.Light);
            bounce = direction;
            Collider = new Hitbox(8, 8);
            Add(new Coroutine(newDeathRoutine()));
            ActionDelay = 0.1f;
        }
        public override void Update()
        {
            Components.Update();
            if (Input.MenuConfirm.Pressed && !finished)
            {
                End();
            }
        }

        public override void Render()
        {
            if (deathEffect == null)
            {
                if (!eye.Falling)
                {
                    eye.Image.DrawSimpleOutline();
                }
                Sprite.Scale.Y = scale;
                light.Render();
                base.Render();
            }
            else
            {
                deathEffect.Render();
                Sprite.Render();
            }
        }
        public override void Awake(Scene scene)
        {
            if (Components == null)
            {
                return;
            }
            foreach (Component component in Components)
            {
                component.EntityAwake();
            }
            if (!(bounce != Vector2.Zero)) return;

            if (Math.Abs(bounce.X) <= Math.Abs(bounce.Y))
            {
                bounce = Calc.AngleToVector(Calc.AngleApproach(bounce.Angle(), new Vector2(0 - Math.Sign(Player.LastValidXAim), 0f).Angle(), 0.5f), 1f);
            }
            Sprite.Die(bounce);
            DeadEye eye = new DeadEye(this, GFX.Game["characters/PuzzleIslandHelper/Calidus/eyeSurprised00"],
                new Vector2(2, 4) - bounce.Sign() * 4);
            Scene.Add(this.eye = eye);

        }
        public IEnumerator newDeathRoutine()
        {
            Level level = SceneAs<Level>();
            if (bounce != Vector2.Zero)
            {
                Audio.Play("event:/char/madeline/predeath", Position);
                scale = 1.5f;
                Celeste.Freeze(0.05f);
                yield return null;
                Vector2 from = Position;
                Vector2 to = from + bounce * 24f;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.5f, start: true);
                Add(tween);
                tween.OnUpdate = delegate (Tween t)
                {
                    Position = from + (to - from) * t.Eased;
                    scale = 1.5f - t.Eased * 0.5f;
                    Sprite.Rotation = (float)(Math.Floor(t.Eased * 4f) * 6.2831854820251465);
                };
                yield return tween.Duration * 0.75f;
                tween.Stop();
            }
            while (Sprite.DeadBody.CurrentAnimationID == "explodeA")
            {
                yield return null;
            }
            Sprite.EyeSprite.Visible = false;

            level.Displacement.AddBurst(Center, 0.3f, 0f, 80f);
            level.Shake();
            eye.Falling = true;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            Audio.Play(HasGolden ? "event:/new_content/char/madeline/death_golden" : "event:/char/madeline/death", Position);

            CalidusDeathEffect deathEffect = new CalidusDeathEffect(Color.Lime, Collider.HalfSize);
            this.deathEffect = deathEffect;
            deathEffect.OnUpdate = delegate (float f)
            {
                light.Alpha = 1f - f;
            };
            Add(deathEffect);

            Sprite.EyeSprite.Visible = false;
            yield return deathEffect.Duration * 0.65f;
            while (!Sprite.DoneDying)
            {
                yield return null;
            }
            if (ActionDelay > 0f)
            {
                yield return ActionDelay;
            }
            OnEnd?.Invoke();
            End();
        }
        public class DeadEye : Actor
        {
            public Image Image;
            public Vector2 Speed;
            public bool Falling;
            public Entity Track;
            public Vector2 Offset;
            public DeadEye(Entity track, MTexture texture, Vector2 offset) : base(track.Position + offset)
            {
                Depth = -1000001;
                Offset = offset;
                Track = track;
                Add(Image = new Image(texture));
                Collider = new Hitbox(Image.Width - 1, Image.Height - 1);
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Speed.Y = -70f;
            }
            public override void Render()
            {
                if (Falling)
                {
                    Image.DrawSimpleOutline();
                }
                base.Render();
            }
            public override void Update()
            {
                base.Update();
                if (!Falling)
                {
                    Position = Track.Position + Offset;
                }
                else
                {
                    MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                    Speed.Y = Calc.Approach(Speed.Y, 130f, 300f * Engine.DeltaTime);
                }
            }
            public void OnCollideV(CollisionData hit)
            {
                if (hit.Direction.Y < 0)
                {
                    while (CollideCheck<Solid>() && Position.Y < SceneAs<Level>().Bounds.Bottom)
                    {
                        Position.Y++;
                    }
                    Speed.Y = 0;
                }
                if (hit.Direction.Y > 0)
                {
                    if (Speed.Y > 30f)
                    {
                        Speed.Y *= -0.4f;
                    }
                    else
                    {
                        Speed.Y = 0;
                    }
                }
            }
        }
        public class CalidusDeathEffect : DeathEffect
        {
            public const string Path = "characters/PuzzleIslandHelper/Calidus/deathParticle00";
            public const float AdditionalRotation = (float)(Math.PI / 2f);
            public int[] Sequence = new int[8]
            {
                4,7,0,3,2,1,5,6
            };
            private int currentFlashing = -1;
            public CalidusDeathEffect(Color color, Vector2? offset = null) : base(color, offset)
            {
                /* Code is:
                 * Left, UpRight, Right, DownLeft, Down, DownRight, UpLeft, Up
                 */

            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                entity.Add(new Coroutine(sequence(3)));
            }
            private IEnumerator sequence(float waitframes)
            {
                yield return 4 * Engine.DeltaTime;
                for (int i = 0; i < Sequence.Length; i++)
                {
                    currentFlashing = Sequence[i];
                    yield return waitframes * Engine.DeltaTime;
                }
                currentFlashing = -1;
            }
            public override void Render()
            {
                if (Entity != null)
                {
                    Draw(Entity.Position + Position, Color, Percent, currentFlashing);
                }
            }
            public static Vector2 GetVector(int index, float ease)
            {
                return Calc.AngleToVector((index / 8f + (-0.03f + (ease * 0.05f))) * ((float)Math.PI * 2f), Ease.CubeOut(ease) * 24f);
            }
            public static void Draw(Vector2 position, Color color, float ease, int currentlyFlashing = -1)
            {
                Color color2 = ((Math.Floor(ease * 10f) % 2.0 == 0.0) ? color : Color.White);

                MTexture mTexture = GFX.Game[Path];
                float num = ((ease < 0.5f) ? (0.5f + ease) : Ease.CubeOut(1f - (ease - 0.5f) * 2f));
                float rot = AdditionalRotation * ease;
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vector = GetVector(i, ease);
                    mTexture.DrawCentered(position + vector + new Vector2(-1f, 0f), Color.Black, new Vector2(num, num), rot);
                    mTexture.DrawCentered(position + vector + new Vector2(1f, 0f), Color.Black, new Vector2(num, num), rot);
                    mTexture.DrawCentered(position + vector + new Vector2(0f, -1f), Color.Black, new Vector2(num, num), rot);
                    mTexture.DrawCentered(position + vector + new Vector2(0f, 1f), Color.Black, new Vector2(num, num), rot);
                }

                for (int j = 0; j < 8; j++)
                {
                    Vector2 vector2 = GetVector(j, ease);
                    if (currentlyFlashing == j)
                    {
                        mTexture.DrawCentered(position + vector2, Color.Lerp(color, Color.Black, 0.8f), new Vector2(num, num), rot);
                    }
                    else
                    {
                        mTexture.DrawCentered(position + vector2, color2, new Vector2(num, num), rot);
                    }
                }
            }
        }
    }
}
