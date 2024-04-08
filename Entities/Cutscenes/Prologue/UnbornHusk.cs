using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using Monocle;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Celeste.Mod.CommunalHelper;
using FrostHelper;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [Tracked]
    public class UnbornHusk : Actor
    {
        public List<GlitchSquare> Squares = new();
        public bool ReadyToBlast;
        public Facings facing;
        public Vector2 Scale = Vector2.One;
        public Vector2 speed;
        public bool enabled;
        public float dashTimer;
        public Vector2 dashDirection;
        public float trailTimerA;
        public float trailTimerB;
        public CustomShaker shaker;
        public PlayerSprite Sprite;
        public PlayerHair Hair;
        public bool Finished;
        private Vector2 playerPos;
        private Player player;
        public DisplacementRenderer.Burst Burst;
        private VirtualRenderTarget Target;
        public bool RenderStuff;
        private float ShakePercent;
        public List<DiamondPulseLines> PulseLines = new();
        public bool DoPulseGlitch;
        public bool WaitingForPolygonScreen;
        public float FloatAmount;
        public enum States
        {
            InPlayer,
            InControl,
            InCutscene
        }
        public States State = States.InPlayer;
        public Vector2 CameraTarget
        {
            get
            {
                Rectangle bounds = (base.Scene as Level).Bounds;
                return (Position + new Vector2(-160f, -90f)).Clamp(bounds.Left, bounds.Top, bounds.Right - 320, bounds.Bottom - 180);
            }
        }
        public bool start;
        public Facings Facing = Facings.Right;
        public Facings LastFacing = Facings.Right;
        public DiamondPulse Pulse;
        public int PulseSize = 30;
        public UnbornHusk(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }
        public UnbornHusk(Vector2 position) : base(position)
        {
            base.Collider = new Hitbox(10f, 10f, -5f, -5f);
            Depth = -100001;
            Add(new MirrorReflection());
            Add(new PlayerCollider(OnPlayer));

            Add(shaker = new CustomShaker(false));
            Add(new Coroutine(IntroSequence()));
            for (int i = 0; i < 25; i++)
            {
                Squares.Add(new GlitchSquare(Width, Height, false));
            }
            Add(Squares.ToArray());
            Sprite = new PlayerSprite(PlayerSpriteMode.Madeline);
            Sprite.Position.Y = Height / 2;
            Sprite.Play(PlayerSprite.Fall);
            Hair = new PlayerHair(Sprite);
            Add(Hair);
            Add(Sprite);
            Hair.Visible = false;
            Sprite.Visible = false;
            Hair.Border = Color.Gray;
            Hair.Color = Color.White;
            Target = VirtualContent.CreateRenderTarget("UnbornHuskTarget", 320, 180);
            Pulse = new DiamondPulse(PulseSize);
            Add(Pulse);
            Pulse.Visible = false;
            Pulse.Scale = Vector2.Zero;
            Add(new BeforeRenderHook(BeforeRender));
            Visible = false;
        }
        public void RenderAllPulses()
        {
            Pulse.Scale = Vector2.One * ShakePercent;
            Pulse.Render();
            foreach (DiamondPulseLines pulse in PulseLines)
            {
                pulse.Render();
            }
        }
        public void RenderAllPulsesForTarget()
        {
            Level level = Scene as Level;
            Vector2 pos = Pulse.RenderPosition;
            Pulse.RenderPosition -= level.Camera.Position;
            Pulse.Scale = Vector2.One * ShakePercent;
            Pulse.Render();
            Pulse.RenderPosition = pos;
            foreach (DiamondPulseLines pulse in PulseLines)
            {
                Vector2 p = pulse.RenderPosition;
                pulse.RenderPosition -= level.Camera.Position;
                pulse.Render();
                pulse.RenderPosition = p;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
        }
        public void UpdateFakePlayer()
        {
            if (speed.X == 0) return;
            Facing = (Facings)Math.Sign(speed.X);

        }
        public void BeforeRender()
        {
            if (Scene is not Level level || !RenderStuff) return;

            Effect effect = ShaderFX.PlayerStatic;
            effect.ApplyScreenSpaceParameters(level);
            Target.DrawThenMask(RenderPlayer, RenderPlayer, Matrix.Identity, effect);
        }

        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;

            player.Position = playerPos + shaker.Value;
            Vector2 pulsePos = player.Center - shaker.Value;
            Pulse.RenderPosition = player.Center - shaker.Value;
            foreach (DiamondPulseLines lines in Components.GetAll<DiamondPulseLines>())
            {
                lines.RenderPosition = pulsePos;
            }
            MoveH(speed.X * Engine.DeltaTime);
            MoveV(speed.Y * Engine.DeltaTime);
            Position = Position.Clamp(level.Bounds.X, level.Bounds.Y, level.Bounds.Right, level.Bounds.Bottom);
            if (State == States.InControl)
            {
                ShiftAreaRenderer.ChangeDepth(false, -100000);
                level.Camera.Position = CameraTarget;
                Vector2 vector2 = Input.Aim.Value.SafeNormalize();
                UpdateFakePlayer();
                speed += vector2 * 600f * Engine.DeltaTime; ;
                float num = speed.Length();
                if (num > 120f)
                {
                    num = Calc.Approach(num, 120f, Engine.DeltaTime * 700f);
                    speed = speed.SafeNormalize(num);
                }

                if (vector2.Y == 0f)
                {
                    speed.Y = Calc.Approach(speed.Y, 0f, 400f * Engine.DeltaTime);
                }

                if (vector2.X == 0f)
                {
                    speed.X = Calc.Approach(speed.X, 0f, 400f * Engine.DeltaTime);
                }

                int num2 = Math.Sign((int)facing);
                int num3 = Math.Sign(speed.X);

                /*            if (Input.Dash.Pressed)
                            {
                                Dash(Input.Aim.Value.EightWayNormal());
                            }*/
            }

        }
        public void RenderPlayer()
        {
            if (Scene is not Level level) return;
            Vector2 prev = Sprite.RenderPosition;
            Sprite.RenderPosition = (prev - level.Camera.Position + Vector2.UnitY * FloatAmount).Floor();
            Hair.MoveHairBy(-level.Camera.Position);
            Sprite.Scale.X = (float)Facing;
            Hair.Facing = Facing;
            Hair.Render();
            Sprite.Render();
            Hair.MoveHairBy(level.Camera.Position);
            Sprite.RenderPosition = prev.Floor();
        }

        public override void Render()
        {
            Level level = Scene as Level;
            RenderSquares(false, level);
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
            base.Render();
            RenderSquares(true, level);
        }
        public void RenderSquares(bool inFront, Level level)
        {
            Draw.SpriteBatch.End();
            foreach (GlitchSquare square in Squares)
            {
                if (square.Visible && square.InFront == inFront)
                {
                    square.DrawSquare(level);
                }
            }
            GameplayRenderer.Begin();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level obj = scene as Level;
            obj.Session.ColorGrade = "PuzzleIslandHelper/prologue";
            obj.ScreenPadding = 32f;
            obj.CanRetry = false;
            while (CollideCheck<Solid>())
            {
                Position.Y -= 8;
                if (!obj.Bounds.Contains(Position.ToPoint()))
                {
                    RemoveSelf();
                }
            }
            if (scene.GetPlayer() is Player player)
            {
                playerPos = player.Position;
                this.player = player;
            }
        }
        public IEnumerator IntroSequence()
        {
            Level level = Scene as Level;
            yield return null;
            Glitch.Value = 0.05f;
            level.Tracker.GetEntity<Player>()?.StartTempleMirrorVoidSleep();
            float limit = 3;
            while (ShakePercent < limit)
            {
                shaker.Interval = Calc.Max(0.1f - (0.05f * ShakePercent), Engine.DeltaTime);
                shaker.MaxShake = Vector2.UnitX * (1 + ShakePercent);
                if (Input.DashPressed)
                {
                    ShakePercent += 0.05f;
                    shaker.On = true;
                    shaker.Timer = shaker.Interval * 2;

                    float pulseAmount = PulseSize * ShakePercent;
                    DiamondPulseLines pulse = new DiamondPulseLines((int)pulseAmount, pulseAmount + 8, 0.25f, 0.1f);
                    Add(pulse);
                    PulseLines.Add(pulse);
                    pulse.Visible = false;
                }
                ShakePercent = Calc.Max(ShakePercent - Engine.DeltaTime / 2, 0);
                yield return null;
            }
            DiamondPulseLines bigPulse = new DiamondPulseLines((int)(PulseSize * limit), 300, 4f, 3f);
            PulseLines.Add(bigPulse);
            Add(bigPulse);
            bigPulse.Visible = false;
            ShakePercent = limit;
            yield return BreakOut();
        }
        private IEnumerator GlitchPulse()
        {
            DoPulseGlitch = true;
            yield return 0.4f;
            DoPulseGlitch = false;
        }
        private IEnumerator BreakOut()
        {
            yield return null;
            RenderStuff = true;
            Visible = true;
            Add(new Coroutine(GlitchPulse()));
            Level level = Scene as Level;
            level.Displacement.AddBurst(level.Camera.Position + new Vector2(160, 90), 2, 0, 64, 1);
            for (int i = 0; i < Squares.Count; i++)
            {
                Squares[i].Visible = true;
            }
            yield return null;
            float y = Position.Y;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                MoveToY(Calc.LerpClamp(y, playerPos.Y - 80f, Ease.CubeOut(i)));
                yield return null;
            }
            y = Position.Y;
            yield return 0.3f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.5f)
            {
                MoveToY(Calc.LerpClamp(y, playerPos.Y - 40f, Ease.SineInOut(i)));
                yield return null;
            }
            yield return 1.5f;
            Vector2 campos = level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.4f)
            {
                level.Camera.Position = Vector2.Lerp(campos, CameraTarget, i);
                yield return null;
            }
            WaitingForPolygonScreen = true;
            yield return null;
        }
        public void OnPlayer(Player player)
        {
            if (State is not States.InCutscene) return;
            if (!player.Dead)
            {
                Engine.TimeRate = 0.25f;
            }
        }
    }
}
