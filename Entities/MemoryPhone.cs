using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MemoryPhone")]
    [Tracked]
    public class MemoryPhone : Entity
    {
        public bool Receiver;
        public string TeleportTo = "forestMemory";
        public Monocle.Renderer renderer;
        public enum PhoneTypes
        {
            Can,
            Payphone,
            Regular,
            Modern
        }
        public class Cutscene : CutsceneEntity
        {
            public MemoryPhone phone;
            public float BlackoutAmount;
            public Image Circle;
            public Cutscene(MemoryPhone phone) : base()
            {
                this.phone = phone;
                Depth = -100001;
                Tag = Tags.TransitionUpdate | Tags.Global;
                Circle = new Image(GFX.Game["utils/PuzzleIslandHelper/circle"]);
                Circle.CenterOrigin();
                Circle.Position += new Vector2(Circle.Width / 2, Circle.Height / 2);
                Add(Circle);
                Circle.Scale = Vector2.Zero;
                Circle.Color = Color.Black;
            }
            public override void Update()
            {
                base.Update();
                Position = phone.Center;
                Circle.Scale = Vector2.One * BlackoutAmount * 6;
            }
            public override void Render()
            {
                if (BlackoutAmount > 0)
                {
                    base.Render();

                    phone.Render();
                    if (Scene.GetPlayer() is Player player)
                    {
                        player.Render();
                    }
                }
            }

            public override void OnBegin(Level level)
            {
                Add(new Coroutine(cutscene()));
            }

            public override void OnEnd(Level level)
            {

            }
            public IEnumerator cutscene()
            {
                if (Scene is not Level level || level.GetPlayer() is not Player player) yield break;
                player.StateMachine.State = Player.StDummy;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    BlackoutAmount = Calc.LerpClamp(0, 1f, i);
                    yield return null;
                }
                BlackoutAmount = 20;
                Vector2 position = player.Position - phone.Position;
                Facings facing = player.Facing;
                player.ForceCameraUpdate = false;
                InstantRelativeTeleport(Scene, phone.TeleportTo, true);
                yield return null;
                player.Position = phone.Position + position;
                player.Facing = facing;
                Level = Scene as Level;
                yield return 0.7f;
                BlackoutAmount = 1;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    BlackoutAmount = Calc.LerpClamp(1, 0f, i);
                    yield return null;
                }
                BlackoutAmount = 0;
                RemoveSelf();

            }
            public void InstantRelativeTeleport(Scene scene, string room, bool snapToSpawnPoint, int positionX = 0, int positionY = 0)
            {
                Level level = scene as Level;
                Player player = level.GetPlayer();
                if (level == null || player == null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(room))
                {
                    return;
                }
                level.OnEndOfFrame += delegate
                {
                    Vector2 cam = player.Position - level.Camera.Position;
                    Vector2 levelOffset = level.LevelOffset;
                    Vector2 val2 = player.Position - levelOffset;
                    Vector2 val3 = level.Camera.Position - levelOffset;
                    Vector2 offset = new Vector2(positionY, positionX);
                    Facings facing = player.Facing;
                    level.Remove(player);
                    level.UnloadLevel();
                    level.Session.Level = room;
                    Session session = level.Session;
                    Level level2 = level;
                    Rectangle bounds = level.Bounds;
                    float num = bounds.Left;
                    bounds = level.Bounds;
                    session.RespawnPoint = level2.GetSpawnPoint(new Vector2(num, bounds.Top));
                    level.Session.FirstLevel = false;
                    level.LoadLevel(Player.IntroTypes.None);

                    level.Camera.Position = level.LevelOffset + val3 + offset.Floor();
                    if (snapToSpawnPoint && session.RespawnPoint.HasValue)
                    {
                        player.Position = session.RespawnPoint.Value + offset.Floor();
                    }
                    else
                    {
                        player.Position = level.LevelOffset + val2 + offset.Floor();
                    }

                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset + offset.Floor());
                    if (level.Wipe != null)
                    {
                        level.Wipe.Cancel();
                    }
                    Level = level2;
                    phone = level2.Tracker.GetEntity<MemoryPhone>();
                    player.StateMachine.State = Player.StDummy;
                    level.Camera.Position = player.Position - cam;
                };
            }

        }
        public PhoneTypes PhoneType;
        public Image image;
        public MemoryPhone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Receiver = data.Bool("isReceiver");
            image = new Image(GFX.Game["objects/PuzzleIslandHelper/phones/forest/platform"]);
            Add(image);
            if (!Receiver)
            {
                Add(new TalkComponent(new Rectangle(0, 0, (int)image.Width, (int)image.Height), Vector2.Zero, Interact));
            }
        }
        private void Interact(Player player)
        {
            Scene.Add(new Cutscene(this));
        }
    }

}
