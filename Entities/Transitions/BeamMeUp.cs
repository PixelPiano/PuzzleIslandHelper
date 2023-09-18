using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

// PuzzleIslandHelper.TransitionEvent
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Transitions
{
    [CustomEntity("PuzzleIslandHelper/BeamMeUp")]
    public class BeamMeUp : Entity
    {
        private Level level;
        private Player player;
        private Sprite Beam;
        private const string Path = "utils/PuzzleIslandHelper/";
        public float BeamScale = 1;
        private bool InEnd;
        private float Spacing = 16;
        private float SpaceProgress;
        private int LinesAbove;
        private int LinesBelow;
        private float Rate = 2;
        public bool Done;
        private float RemainingHeight;
        private float ScaleY = 1;
        private float LineOpacity;
        private VirtualRenderTarget Target;
        private VirtualRenderTarget PlayerBox;
        private Color RenderColor = Color.White;
        private Rectangle PlayerRect;
        private string RoomName;

        private bool GremlinTime;
        public BeamMeUp(string roomName, bool gremlinTime = false)
        : base(Vector2.Zero)
        {
            RoomName = roomName;
            Beam = new Sprite(GFX.Game, Path);
            Tag |= Tags.Persistent;
            Beam.AddLoop("idle", "beam", 0.1f);
            Beam.Add("intro", "beamIntro", 0.1f, "idle");
            Beam.Add("slice", "introSlice", 0.08f);
            Add(Beam);
            Depth = -20000;
            Beam.Visible = false;
            Collider = new Hitbox(Beam.Width, Beam.Height);
            PlayerRect = new Rectangle(0, 0, (int)Beam.Width, (int)Beam.Height);
            Target = VirtualContent.CreateRenderTarget("DigitalTransitionTarget", 320, 180);

            PlayerBox = VirtualContent.CreateRenderTarget("DigitalTransitionPlayerBox", 320, 180);
            Add(new BeforeRenderHook(BeforeRender));
            GremlinTime = gremlinTime;
        }
        private static void TeleportTo(Scene scene, Player player, string room, Player.IntroTypes introType = Player.IntroTypes.None, Vector2? nearestSpawn = null)
        {
            Level level = scene as Level;
            if (level != null)
            {
                level.OnEndOfFrame += delegate
                {
                    level.TeleportTo(player, room, introType, nearestSpawn);
                };
            }
        }
        private IEnumerator End()
        {
            if (!GremlinTime)
            {
                TeleportTo(level, player, RoomName);
            }
            else
            {
                player.Sprite.Visible = true;
                player.Hair.Visible = true;
                yield return 2f;
                //play "?" animation
                //gremlins appear
                //madeline gets carried away
                //transition
                TeleportTo(level, player, RoomName);
            }
            yield return null;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
            player = level.Tracker.GetEntity<Player>();

            if (player is null)
            {
                RemoveSelf();
            }
            player.StateMachine.State = Player.StDummy;
            player.Sprite.Visible = true;
            player.Hair.Visible = true;
            Beam.JustifyOrigin(0, 0.5f);
            Beam.Position.Y += Beam.Height / 2;
            Position = new Vector2(level.Camera.Left, player.Position.Y - 8 - Beam.Height / 2);
            LinesAbove = (int)((Top - level.Bounds.Top) / Spacing) + 2;
            LinesBelow = (int)((level.Bounds.Bottom - Bottom) / Spacing) + 2;
            Tween ColorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 1);
            ColorTween.OnUpdate = (t) =>
            {
                if (Beam.CurrentAnimationID == "idle")
                {
                    RenderColor = Color.Lerp(Color.Green, Color.DarkGreen, t.Eased);
                }
            };
            Add(ColorTween);
            ColorTween.Start();
            Add(new Coroutine(ScaleRoutine()));
        }
        public override void Update()
        {
            base.Update();
            if (Beam.CurrentAnimationID != "idle")
            {
                RenderColor = Color.White;
            }
            Position = new Vector2(level.Camera.Left, player.Position.Y - 8 - Beam.Height / 2);
            SpaceProgress += Rate;
            SpaceProgress %= Spacing;
            Beam.Scale.Y = ScaleY;
            if (Done && !InEnd)
            {
                Add(new Coroutine(End()));
                InEnd = true;
            }
        }
        private void BeforeRender()
        {
            EasyRendering.DrawToObject(Target, Drawing, level, true);
            if (player is not null)
            {
                player.Sprite.Visible = true;
                player.Hair.Visible = true;
                EasyRendering.DrawToObject(PlayerBox, player.Render, level, true);
                player.Sprite.Visible = false;
                player.Hair.Visible = false;

            }

            RemainingHeight = Beam.Height * (1 - ScaleY);
            PlayerRect.Height = (int)(Beam.Height * ScaleY);
            PlayerRect.Y = (int)(Position.Y - level.Camera.Top + RemainingHeight / 2);
        }
        private void Drawing()
        {
            Draw.Rect(level.Camera.GetBounds(), Color.Lerp(Color.White, Color.Green, 0.4f) * 0.3f * LineOpacity);


            for (int i = 0; i < 2; i++)
            {
                int sign = i == 0 ? 1 : -1;
                float maxAngle = 45f * sign;

                float MiddleX = level.Camera.Position.X + 160;
                int lines = (int)(320 / Spacing);
                float Distance = MathHelper.Distance(Position.X, MiddleX);
                Vector2 orig = Position;
                if (sign == 1)
                {
                    orig.Y += Beam.Height;
                    orig.Y -= RemainingHeight / 2;
                }
                else
                {
                    orig.Y += RemainingHeight / 2;
                }
                for (int j = 0; j < lines; j++)
                {

                    Vector2 start = new Vector2(orig.X + j * Spacing + SpaceProgress * sign, orig.Y);

                    Draw.LineAngle(
                           start: start,
                           angle: Calc.AngleLerp(maxAngle, 90 * sign, MathHelper.Distance(start.X, MiddleX + 160) / Distance),
                           length: level.Bounds.Height,
                           color: Color.White * LineOpacity);
                }
            }
            for (int i = 0; i < LinesAbove; i++)
            {
                Vector2 start = Position;
                Vector2 end = start + Vector2.UnitX * Beam.Width;
                Vector2 offset = Vector2.UnitY * (i * Spacing + SpaceProgress);
                Draw.Line(start - offset, end - offset, Color.White * LineOpacity);
            }
            for (int i = 0; i < LinesBelow; i++)
            {
                Vector2 start = Position + Vector2.UnitY * Beam.Height;
                Vector2 end = start + Vector2.UnitX * Beam.Width;
                Vector2 offset = Vector2.UnitY * (i * Spacing + SpaceProgress);
                Draw.Line(start + offset, end + offset, Color.White * LineOpacity);
            }

            Beam.Render();
        }
        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Draw(Target, level.Camera.Position, RenderColor);
            Draw.SpriteBatch.Draw(PlayerBox, Position + Vector2.UnitY * ((Beam.Height - PlayerRect.Height) / 2), PlayerRect, Color.White);

        }
        private IEnumerator ScaleRoutine()
        {
            Beam.Play("slice");
            while (Beam.CurrentAnimationID == "slice")
            {
                yield return null;
            }
            yield return 0.3f;
            RenderColor = Color.Green;
            Beam.Play("intro");
            Beam.OnLastFrame = (s) =>
            {
                if (s == "intro")
                {
                    RenderColor = Color.LightGreen;
                }
            };
            while (Beam.CurrentAnimationID == "intro")
            {
                yield return null;
            }
            yield return null;
            yield return null;
            RenderColor = Color.White;
            yield return null;
            RenderColor = Color.Green;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                LineOpacity = i;
                yield return null;
            }
            yield return 3;
            for (float i = 1; i > 0; i -= Engine.DeltaTime)
            {
                ScaleY = i;
                LineOpacity = i;
                yield return null;
                yield return null;
            }
            Done = true;
            TransitionManager.Finished = true;
            yield return null;
        }

    }
}
