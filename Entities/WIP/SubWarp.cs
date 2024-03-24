using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.SubWarp
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/SubWarp")]
    public class SubWarp : Entity
    {
        public static bool Enabled;
        public static Vector2 PositionInLevel;
        private bool AddedState;
        private bool SamePosition;
        private readonly string Room;
        private string TargetID;
        private TalkComponent talk;
        private static float TalkWait;
        private ParticleType Dust = new ParticleType
        {
            Size = 1,
            SizeRange = 5f,
            Direction = -(MathHelper.Pi / 2),
            DirectionRange = MathHelper.TwoPi,
            Color = Color.Black,
            Color2 = Color.Green,
            SpeedMin = 10f,
            SpeedMax = 50f,
            LifeMin = 0.5f,
            LifeMax = 0.7f,
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Linear

        };
        private ParticleSystem system;

        private Sprite sprite;

        public SubWarp(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Depth = 1;
            Tag |= Tags.TransitionUpdate;
            Room = data.Attr("roomName");
            TargetID = data.Attr("targetID");
            SamePosition = data.Bool("keepPosition");
        }
        private void DustParticles()
        {
            Dust.Color2 = Color.Lerp(Color.Green, Color.LightGreen, Calc.Random.Range(0f, 0.4f));
            Dust.Color = Color.Lerp(Color.DarkGreen, Color.Black, Calc.Random.Range(0.7f, 1f));
            system.Emit(Dust, Center + Vector2.UnitX);
        }
        private IEnumerator WaitThenAddTalk(float wait = 0)
        {
            yield return wait;
            Add(talk = new TalkComponent(new Rectangle(0, 0, (int)sprite.Width, (int)sprite.Height), new Vector2(sprite.Width / 2, 0), Interact));
        }
        private void AddComponents(Scene scene)
        {
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/subTeleport/"));
            sprite.AddLoop("idle", "module", 0.1f);
            sprite.Play("idle");

            scene.Add(system = new ParticleSystem(Depth, 50));

            Collider = new Hitbox(sprite.Width, sprite.Height);
            Add(new Coroutine(WaitThenAddTalk(TalkWait)));
            Add(new VertexLight(sprite.Center, Color.Lerp(Color.Green, Color.LightGreen, 0.7f), 1, 20, (int)sprite.Height * 2));
            Add(new BloomPoint(sprite.Center, 1, sprite.Height));
            AddedState = true;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Enabled)
            {
                AddComponents(scene);
            }
        }
        public override void Update()
        {
            base.Update();
            if (Enabled && !AddedState)
            {
                AddComponents(Scene);
            }
            if (Enabled)
            {
                DustParticles();
            }
        }
        private IEnumerator Cutscene(Player player)
        {
            AddTag(Tags.Global);
            string thisLevel = SceneAs<Level>().Session.Level;
            Glitch.Value = 0;
            yield return null;
            player.StateMachine.State = 11;
            Glitch.Value = 0.9f;
            yield return 0.2f;
            if (Scene is not Level level) yield break;
            if (Room != thisLevel)
            {
                int dashes = player.Dashes;
                Console.WriteLine("AAAAAAAAAAAAA");
                TeleportTo(this, level, Room);
                Console.WriteLine("AAAAA");
                yield return null;
                level = SceneAs<Level>();
                TalkWait = 0.8f;
                Console.WriteLine("C");
                //todo: fix this shit
                player = level.Tracker.GetEntity<Player>();
                yield return null;
                TeleportCleanup(level);
                Console.WriteLine("YIPPEE");
                player.Dashes = dashes;
            }

            Glitch.Value = 0.9f;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Glitch.Value = Calc.Approach(Glitch.Value, 0, Glitch.Value * i);
                yield return null;
            }
            player.StateMachine.State = 0;
            Glitch.Value = 0;
            RemoveTag(Tags.Global);
            yield return null;
        }
        private void TeleportCleanup(Scene scene)
        {
            if (scene is not Level level || level.GetPlayer() is not Player player) return;
            if (!string.IsNullOrEmpty(TargetID))
            {
                foreach (FadeWarpTarget target in SceneAs<Level>().Tracker.GetEntities<FadeWarpTarget>())
                {
                    if (TargetID == target.id && !string.IsNullOrEmpty(target.id))
                    {
                        player.Position = target.Position + new Vector2(15, 18);

                        (scene as Level).Camera.Position = player.CameraTarget;
                        break;
                    }
                }
            }
            if (SamePosition)
            {
                player.Position = new Vector2((scene as Level).Bounds.Left, (scene as Level).Bounds.Top) + PositionInLevel;
                (scene as Level).Camera.Position = player.CameraTarget;
            }
            player.StateMachine.State = 0;
        }
        private void Interact(Player player)
        {
            TalkWait = 0;
            Add(new Coroutine(Cutscene(player)));
        }

        public static void TeleportTo(SubWarp warp, Scene scene, string room, Player.IntroTypes introType = Player.IntroTypes.Transition, Vector2? nearestSpawn = null)
        {
            if (scene is not Level level || level.GetPlayer() is not Player player) return;
            if (level != null)
            {
                level.OnEndOfFrame += delegate
                {
                    PositionInLevel = player.Position - new Vector2(level.Bounds.Left, level.Bounds.Top);
                    level.TeleportTo(player, room, introType, warp.SamePosition ? PositionInLevel : nearestSpawn);
                };
            }
        }
    }
}