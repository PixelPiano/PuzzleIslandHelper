using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [Tracked]
    public class CalidusButton : Actor
    {
        public Player Player;
        public Vector2 Speed;
        public VertexPositionColor[] Vertices;
        public Vector2 VertexScale = Vector2.Zero;
        private Color[] colors = new Color[4] { Color.Cyan, Color.Cyan, Color.LightCyan, Color.Cyan };
        public readonly Vector2 ScaleTarget = new Vector2(20, 28);
        public Vector2[] Points = new Vector2[] { new(-0.5f, -1), new(0, -1), new(0, 0), new(0.5f, -1) };
        public int[] indices = new int[] { 0, 1, 2, 1, 3, 2 };
        public Calidus Calidus;
        public bool Loading;
        public Effect Shader;
        private float lineAmount;
        public CalidusButton(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
        public CalidusButton(Vector2 position) : base(position)
        {
            Vertices = new VertexPositionColor[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i] * Collider.Size, 0), Color.White);
            }
            Visible = false;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player = scene.GetPlayer();
            Calidus = scene.Tracker.GetEntity<Calidus>();
        }
        public override void Update()
        {
            base.Update();
            if (!Visible)
            {
                Position = Player.TopCenter + new Vector2(-Width / 2, -Height + 4);
            }
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position = new Vector3(TopCenter + Points[i] * VertexScale, 0);
                Vertices[i].Color = colors[i] * VertexAlpha;
            }
        }
        public void Reveal()
        {
            Visible = true;
        }
        public void Hide()
        {
            Visible = false;
        }
        public float VertexAlpha;
        public void SetUpCalidus()
        {
            Calidus.Position = TopCenter - VertexScale.YComp() - Calidus.Collider.HalfSize;
            Calidus.Visible = Calidus.Active = true;
            Calidus.Update();
        }
        public void SpawnCalidus()
        {
            SetUpCalidus();
            Add(new Coroutine(Sequence()));
        }
        public IEnumerator LoadingRoutine()
        {
            while (Loading)
            {
                for (float i = 0; i < 1 && Loading; i += Engine.DeltaTime)
                {
                    lineAmount = i;
                    yield return null;
                }
                lineAmount = 1;
                for (float i = 0; i < 1 && Loading; i += Engine.DeltaTime)
                {
                    lineAmount = 1 - i;
                    yield return null;
                }
                lineAmount = 0;
            }
        }
        public IEnumerator Sequence()
        {
            bool high = true;
            for (int i = 1; i < 25; i++)
            {
                float wait = i / 10f * Engine.DeltaTime;
                if (high)
                {
                    Calidus.Alpha = i / 15f;
                }
                else
                {
                    Calidus.Alpha = 0;
                }
                yield return wait;
                high = !high;
            }
            Calidus.Alpha = 1;
            Loading = false;
            yield return 0.1f;
        }
        public void FadeOut()
        {
            Add(new Coroutine(FadeOutRoutine()));
        }
        public IEnumerator FadeOutRoutine()
        {
            bool high = false;
            for (int i = 1; i < 16; i++)
            {
                float wait = 0.1f - (i / 15f * 0.1f);
                if (high)
                {
                    VertexAlpha = Calc.Random.Range(0.6f, 0.9f);
                }
                else
                {
                    VertexAlpha = Calc.Random.Range(0.2f, 0.5f);
                }
                yield return wait;
                high = !high;
            }
            VertexAlpha = 0;
            yield return null;
        }
        public IEnumerator VertexFlickerRoutine()
        {
            bool high = true;
            for (int i = 1; i < 16; i++)
            {
                float wait = i / 15f * 0.1f;
                if (high)
                {
                    VertexAlpha = Calc.Random.Range(0.6f, 0.9f);
                }
                else
                {
                    VertexAlpha = Calc.Random.Range(0.2f, 0.5f);
                }
                yield return wait;
                high = !high;
            }
            VertexAlpha = 1;
            yield return null;
        }

        public void Press()
        {
            Loading = true;
            Reveal();
            Add(new Coroutine(VertexFlickerRoutine()));
            Tween.Set(this, Tween.TweenMode.Oneshot, 0.5f, Ease.Linear, t =>
            {
                VertexScale.X = Calc.LerpClamp(0, ScaleTarget.X, Ease.SineIn(t.Eased));
                VertexScale.Y = Calc.LerpClamp(0, ScaleTarget.Y, Ease.CubeIn(t.Eased * 2));
            }, t => SpawnCalidus());
        }
        public void ApplyParameters()
        {
            if (Shader != null)
            {
                Shader.ApplyCameraParams(Scene as Level);
                Shader.Parameters["LineOsc"]?.SetValue(lineAmount);
            }
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Position, 8, 8, Color.Red);
            if (VertexScale != Vector2.Zero)
            {
                Draw.SpriteBatch.End();
                GFX.DrawIndexedVertices(SceneAs<Level>().Camera.Matrix, Vertices, 4, indices, 2);
                GameplayRenderer.Begin();
            }
        }
    }
    [CustomEvent("PuzzleIslandHelper/LookAtCapsule")]
    public class LookAtCapsuleScene : CutsceneEntity
    {
        public LookAtCapsuleScene() : base()
        {

        }
        public override void OnBegin(Level level)
        {
            //"BeginningTalkCutsceneWatched"
            if (level.GetPlayer() is Player player)
            {
                player.DisableMovement();
                player.ForceCameraUpdate = true;
                Add(new Coroutine(cutscene(player)));
            }
        }

        private IEnumerator calidusToMarker(string s, float speed = 1f)
        {
            Calidus c = SceneAs<Level>().Tracker.GetEntity<Calidus>();
            WarpCapsule capsule = SceneAs<Level>().Tracker.GetEntity<WarpCapsule>();
            if (c != null && capsule != null && Marker.TryFind("c" + s, out Vector2 position))
            {
                c.StopFollowing();
                c.LookAt(capsule.Center);
                yield return new SwapImmediately(c.Float(position, speed));
            }
        }
        private IEnumerator calidusTo1a()
        {
            yield return calidusToMarker("1a", 0.5f);
        }
        [Command("add_default_runes", "adds the default set of runes to the rune inventory")]
        public static void AddDefaultRunes()
        {
            WarpCapsule.ObtainedRunes.TryAddRuneRange(WarpCapsule.DefaultRunes);
        }
        private IEnumerator calidusBam()
        {
            yield return 0.8f;
            if (Level.Tracker.GetEntity<Calidus>() is not Calidus c || Level.Tracker.GetEntity<WarpCapsule>() is not WarpCapsule w) yield break;
            Vector2 pos = w.InputMachine.TopLeft + Vector2.One * 8;
            Vector2 from = c.BottomRight;
            yield return PianoUtils.LerpYoyo(Ease.Linear, 0.1f, f => c.BottomRight = Vector2.Lerp(from, pos, f), delegate {/*todo: play sound of calidus ramming into screen*/});
        }
        private IEnumerator screenOn()
        {
            WarpCapsule capsule = Level.Tracker.GetEntity<WarpCapsule>();
            capsule.InputMachine.TurnOn();
            yield return null;
        }
        private IEnumerator calidusTo4()
        {
            yield return calidusToMarker("4");
        }
        private IEnumerator calidusTo1()
        {
            yield return lookAtTerminal();
            yield return calidusToMarker("1");
        }
        private IEnumerator calidusTo2()
        {
            yield return calidusToMarker("2", 0.3f);
            yield return 0.8f;
            Calidus c = Level.Tracker.GetEntity<Calidus>();
            if (c != null)
            {
                c.Emotion(Calidus.Mood.Stern);
            }
            yield return 1f;
        }
        private IEnumerator calidusTo3()
        {
            Calidus c = Level.Tracker.GetEntity<Calidus>();
            WarpCapsule w = Level.Tracker.GetEntity<WarpCapsule>();
            if (c != null && w != null)
            {
                c.LookAt(w);
            }
            yield return calidusToMarker("3");
        }
        private IEnumerator lookatplayer()
        {
            Calidus c = SceneAs<Level>().Tracker.GetEntity<Calidus>();
            if (c != null)
            {
                c.LookAt(Scene.GetPlayer());
            }
            yield return null;
        }
        private IEnumerator prep(Player player)
        {
            if (Marker.TryFind("playerWalkTo2", out var position))
            {
                Coroutine walk = new Coroutine(player.DummyWalkTo(position.X));
                Add(walk);
                while (!walk.Finished)
                {
                    yield return null;
                }
                Add(new Coroutine(Level.ZoomTo(new Vector2(160, 90), 1.5f, 1)));
            }
        }
        private IEnumerator cutscene(Player player)
        {
            Add(new Coroutine(prep(player)));
            Add(new Coroutine(calidusTo1()));
            yield return Textbox.Say("LookAtCapsule", calidusTo1, calidusTo2, calidusTo3, lookatplayer, lookAtTerminal, calidusBam, screenOn, calidusTo4, calidusTo1a);
            yield return Level.ZoomBack(1);
            EndCutscene(Level);
        }
        private IEnumerator lookAtTerminal()
        {
            if (Level.Tracker.GetEntity<Calidus>() is not Calidus c || Level.Tracker.GetEntity<WarpCapsule.Machine>() is not WarpCapsule.Machine w) yield break;
            c.LookAt(w);
        }
        public override void OnEnd(Level level)
        {
            Level.ResetZoom();
            if (level.GetPlayer() is Player player)
            {
                player.EnableMovement();
            }
            Calidus c = SceneAs<Level>().Tracker.GetEntity<Calidus>();
            if (c != null)
            {
                c.StartFollowing();
                c.Normal();
                if (WasSkipped)
                {
                    c.SnapToLeader();
                }
            };
            Level.Tracker.GetEntity<WarpCapsule>()?.InputMachine?.TurnOn();
            AddDefaultRunes();
        }
    }
    public class HairChip
    {
        public static bool Visible = true;
        public static Color PinColor = Color.Yellow;
        public static Vector2 Offset;

        [OnLoad]
        public static void Load()
        {
            On.Celeste.PlayerHair.Render += PlayerHair_Render;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.PlayerHair.Render -= PlayerHair_Render;
        }
        private static void PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            orig(self);
            if (Visible)
            {
                Player player = self.Entity as Player;
                Facings f = player.Facing;
                Vector2 offset = new Vector2((int)f * 3, 4);
                Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, self.Nodes[0] + offset, Draw.Pixel.ClipRect, PinColor, 0f, Vector2.One * 5, self.GetHairScale(0), SpriteEffects.None, 0f);
            }
        }
    }

    [CustomPassengerCutscene("BeginningTalk")]
    [Tracked]
    public class BeginningTalkCutscene : PassengerCutscene
    {
        public CalidusButton Button;
        public VertexPassenger VertexPassenger;
        public Calidus Calidus;
        public BeginningTalkCutscene(Passenger passenger, Player player) : base(passenger, player)
        {
            if (passenger is VertexPassenger)
            {
                VertexPassenger = passenger as VertexPassenger;
            }
            OncePerSession = true;
        }
        public override void OnBegin(Level level)
        {
            if (VertexPassenger == null) return;
            Calidus = level.Tracker.GetEntity<Calidus>();
            if (Calidus == null)
            {
                Calidus = new Calidus(Vector2.Zero, new EntityID(Guid.NewGuid().ToString(), 0));
                level.Add(Calidus);
            }
            Calidus.Visible = Calidus.Active = false;
            Player.DisableMovement();
            Add(new Coroutine(Cutscene()));
        }
        public IEnumerator Cutscene()
        {
            Player.ForceCameraUpdate = false;
            if (Marker.TryFind("cam", out Vector2 pos))
            {
                Add(new Coroutine(Level.ZoomTo(pos - Level.Camera.Position, 1.4f, 1)));
            }
            if (Marker.TryFind("playerWalkTo", out Vector2 pos2))
            {
                yield return Player.DummyWalkTo(pos2.X);
                Player.Facing = Facings.Right;
            }

            yield return Textbox.Say("WarpCapsuleFirstNew", walkToPrimitive, walkToPrimitiveBack, primitiveTurnOn, playerLookAroundLoop, approachChip, turnOnChip, powerDown, pressButton, lookAtPlayer, maddyAngry, lookUpLeft, playerLookRight, lookAtPrimitive, floatToPrimitive, awkwardPause, calidusStutter, calidusCloseEye, calidusYouWhat, Wait1, stopLookLoop, passengerMoveBack);
            Calidus?.StartFollowing();
            yield return Level.ZoomBack(1);
            EndCutscene(Level);
            yield return null;
        }
        private IEnumerator stopLookLoop()
        {
            playerLookingAround = false;
            yield return null;
        }
        private float passengerFrom;
        private IEnumerator approachChip()
        {
            passengerFrom = Passenger.X;
            yield return Passenger.MoveToX(Player.Right, 0.8f);
            yield return null;
        }
        private IEnumerator passengerMoveBack()
        {
            yield return Passenger.MoveToX(passengerFrom, 0.8f);
            yield return null;
        }
        private IEnumerator turnOnChip()
        {
            yield return null;
        }
        private IEnumerator powerDown()
        {
            yield return new SwapImmediately((VertexPassenger as TutorialPassenger).TurnOffRoutine());
            yield return null;
        }
        private IEnumerator calidusCloseEye()
        {
            Calidus.CloseEye();
            yield return null;
        }
        private IEnumerator calidusYouWhat()
        {
            Calidus.Normal();
            Calidus?.LookAt(Player);
            yield return null;
        }
        private IEnumerator calidusStutter()
        {
            Calidus?.Look(Calidus.Looking.Left);
            Add(new Coroutine(Calidus.FloatXNaive(-36, 1.3f)));
            yield return null;
        }
        private IEnumerator awkwardPause()
        {
            Calidus?.Look(Calidus.Looking.UpLeft);
            yield return 2f;
            Calidus?.LookAt(Player);
        }
        private IEnumerator floatToPrimitive()
        {
            float fromY = Calidus.Y;
            float fromX = Calidus.X;
            yield return this.MultiRoutine(
                PianoUtils.Lerp(Ease.CubeOut, 0.7f, f => Calidus.Y = Calc.LerpClamp(fromY, Passenger.Y - 4, f)),
                PianoUtils.Lerp(Ease.SineOut, 0.65f, f => Calidus.X = Calc.LerpClamp(fromX, Passenger.X - Calidus.Width - 24, f)));
        }
        private IEnumerator waitThree()
        {
            yield return 3;
        }
        private IEnumerator walkToPrimitive()
        {
            yield return new SwapImmediately(Player.DummyWalkTo(Passenger.X - 8));
            Player.Face(Passenger);
            yield return 0.6f;
        }
        private IEnumerator walkToPrimitiveBack()
        {
            yield return new SwapImmediately(Player.DummyWalkTo(Passenger.Right + 8));
            Player.Face(Passenger);
            yield return 0.2f;
        }
        public override void OnEnd(Level level)
        {
            if (VertexPassenger is TutorialPassenger t)
            {
                t.TurnOff();
                t.Position.X = t.prevPosition;
            }
            Calidus.Active = Calidus.Visible = true;
            Calidus.StartFollowing(Calidus.Looking.Player);
            Calidus.Normal();
            if (Button is null)
            {
                /*                Button = new CalidusButton(Player.TopCenter + new Vector2(-16, 4))
                                {
                                    Visible = true,
                                    HasGravity = true
                                };
                                Scene.Add(Button);
                                Button.Ground();*/
                Calidus.Position = Player.TopLeft - Vector2.UnitX * 16;
            }
            level.ResetZoom();
            Player.EnableMovement();
            level.Session.SetFlag("BeginningTalkCutsceneWatched");
        }
        private IEnumerator lookAtPrimitive()
        {
            Calidus.LookAt(Passenger);
            yield return null;
        }
        private IEnumerator lookUpLeft()
        {
            Calidus.Look(Calidus.Looking.UpLeft);
            yield return null;
        }
        private IEnumerator maddyAngry()
        {
            yield return null;
        }
        private IEnumerator lookAtPlayer()
        {
            Calidus.LookAt(Player);
            yield return null;
        }
        private void CalidusStartFollowing()
        {
            Calidus.StartFollowing();
        }
        private IEnumerator pressButton()
        {
            /* if (Marker.TryFind("cam", out Vector2 pos))
             {
                 Add(new Coroutine(Level.ZoomAcross(pos - Level.Camera.Position, 1.4f, 1)));
             }
             yield return Player.DummyWalkTo(Button.Right + 2);
             yield return 0.1f;
             yield return Player.Boop(-1);*/
            Player.Duck();
            Button.Loading = true;
            Button.Press();
            //Add(new Coroutine(Player.DummyWalkTo(Player.X + 30, true, 1.1f)));
            while (Button.Loading)
            {
                yield return null;
            }
            Button.FadeOut();
            yield return null;
        }
        private IEnumerator playerLookLeft()
        {
            Player.Facing = Facings.Left;
            yield return null;
        }
        private IEnumerator playerLookRight()
        {
            Player.Facing = Facings.Right;
            yield return null;
        }
        private IEnumerator divide()
        {
            if (Passenger is TutorialPassenger)
            {
                (Passenger as TutorialPassenger).DivideInstruction();
            }
            yield return 1;

            Vector2 pos = Player.TopCenter + Vector2.UnitY * 4;
            Button = new CalidusButton(pos);
            Scene.Add(Button);
            yield return 0.5f;
            if (Passenger is TutorialPassenger)
            {
                (Passenger as TutorialPassenger).DivideInstructionEnd();
            }
            yield return 0.9f;
            Player.Facing = Facings.Left;
        }
        private bool playerLookingAround;
        private IEnumerator playerLookAroundLoop()
        {
            IEnumerator routine()
            {
                playerLookingAround = true;
                while (playerLookingAround)
                {
                    Player.Facing = (Facings)(-(int)Player.Facing);
                    yield return 0.3f;
                }
            }
            Add(new Coroutine(routine()));
            yield return null;
        }
        private IEnumerator primitiveTurnOn()
        {
            if (VertexPassenger is TutorialPassenger)
            {
                yield return Player.DummyWalkTo(Passenger.Left);
                yield return 0.1f;
                yield return Player.Boop();

                IEnumerator routine()
                {
                    yield return 0.3f;
                    if (Marker.TryFind("playerWalkTo", out Vector2 pos2))
                    {
                        yield return Player.DummyWalkTo(pos2.X, true, 1.2f);
                    }
                }
                Add(new Coroutine(routine()));
                yield return new SwapImmediately((VertexPassenger as TutorialPassenger).TurnOnRoutine());
            }
        }

    }
}
