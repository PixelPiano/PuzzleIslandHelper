using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class DuplicateCopy : CutsceneEntity
    {
        public DuelView DuelView;
        public ShaderOverlay Shader;
        public ShaderOverlay Glitchy;
        private static VirtualRenderTarget _Target;
        private static VirtualRenderTarget _Target2;
        public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("DuplicateTarget", 320, 180);

        public static VirtualRenderTarget Target2 => _Target2 ??= VirtualContent.CreateRenderTarget("DuplicateTarget2", 320, 180);

        private static VirtualRenderTarget _Target3;
        public static VirtualRenderTarget Target3 => _Target3 ??= VirtualContent.CreateRenderTarget("DuplicateTarget3", 320, 180);
        private static VirtualRenderTarget _Screenshot;
        public static VirtualRenderTarget Screenshot => _Screenshot ??= VirtualContent.CreateRenderTarget("DuplicateScreenshot", 320, 180);

        public bool Drawn;
        public bool Teleporting;
        public bool DrawOnce;
        private bool DrawFirstRoom;
        private bool FirstRoomDrawn;

        private bool DrawSecondRoom;
        private bool SecondRoomDrawn;

        private float spawnOne;
        private float spawnTwo;

        private bool Split;
        public DuplicateCopy() : base()
        {
            Add(new BeforeRenderHook(BeforeRender));
            Depth = int.MinValue;
            Tag |= Tags.Global | Tags.Persistent | Tags.TransitionUpdate;
            Shader = new ShaderOverlay("PuzzleIslandHelper/Shaders/fuzzyNoise");
            DuelView = new DuelView();
        }
        private void BeforeRender()
        {
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            ;
            if (Teleporting && !DrawOnce)
            {
                if (DuelView is not null)
                {
                    DuelView.ForceLevelRender = false;
                }
                Screenshot.DrawToObject(DrawLevel, Matrix.Identity, false);
                if (DuelView is not null)
                {
                    DuelView.ForceLevelRender = true;
                }
                DrawOnce = true;
            }
            if (DrawFirstRoom && !FirstRoomDrawn)
            {
                GameplayBuffers.TempA.DrawToObject(DrawLevel, Matrix.Identity, true);
                DrawFirstRoom = false;
                FirstRoomDrawn = true;
            }
            if (DrawSecondRoom && !SecondRoomDrawn)
            {
                GameplayBuffers.TempB.DrawToObject(DrawGameplay, Matrix.Identity, true);
                DrawSecondRoom = false;
                SecondRoomDrawn = true;
            }
            if (FirstRoomDrawn)
            {
                if (Split)
                {
                    float spawnx = spawnOne;
                    Rectangle rect = new Rectangle(160 - DuelView.MaxSize - (int)DuelView.Offset.X, (int)DuelView.Offset.Y, DuelView.MaxSize, DuelView.MaxSize);
                    Rectangle rect2 = new Rectangle((int)spawnx - DuelView.MaxSize / 2, (int)DuelView.Offset.Y, DuelView.MaxSize, DuelView.MaxSize);

                    ShaderOverlay.Apply(GameplayBuffers.TempA, Target, rect, rect2, null, true);
                }
                else
                {
                    ShaderOverlay.Apply(GameplayBuffers.TempA, Target, null, true);
                }
            }
            if (SecondRoomDrawn)
            {
                ShaderOverlay.Apply(GameplayBuffers.TempB, Target2, null, true);
            }
            if (DrawOnce)
            {
                ShaderOverlay.Apply(Screenshot, Target3, null, true);
                DrawOnce = false;
            }
        }
        public override void Render()
        {
            base.Render();
            if (Drawn)
            {
                if (Split)
                {
                    Rectangle rect = new Rectangle(160 - DuelView.MaxSize - (int)DuelView.Offset.X, (int)DuelView.Offset.Y, DuelView.MaxSize, DuelView.MaxSize);
                    Rectangle rect2 = new Rectangle(160 + (int)DuelView.Offset.X, (int)DuelView.Offset.Y, DuelView.MaxSize, DuelView.MaxSize);
                    Vector2 offsetA = new Vector2(rect.X, rect.Y);
                    Vector2 offsetB = new Vector2(rect2.X, rect2.Y);
                    Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position + offsetA, rect, Color.White);
                    Draw.SpriteBatch.Draw(Target2, SceneAs<Level>().Camera.Position + offsetB, rect2, Color.White);
                }
                else
                {
                    Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White);
                    Draw.SpriteBatch.Draw(Target2, SceneAs<Level>().Camera.Position, Color.White);
                }
            }
            if (Teleporting && DrawOnce)
            {
                Draw.SpriteBatch.Draw(Target3, SceneAs<Level>().Camera.Position, Color.White);
            }

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _Target?.Dispose();
            _Target = null;
            _Screenshot?.Dispose();
            _Screenshot = null;
            _Target2?.Dispose();
            _Target2 = null;
            _Target3?.Dispose();
            _Target3 = null;
        }
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene()));
        }

        public override void OnEnd(Level level)
        {

        }
        private IEnumerator Cutscene()
        {
            yield return MultiTeleportGlitch(0.05f, 4);
            yield return CleanUpBeams(2);
            yield return TeleportAndDraw();

        }
        private IEnumerator TeleportAndDraw()
        {
            yield return null;
            string[] rooms = new string[2];
            int index = 0;
            foreach (BeamMeUp b in Level.Tracker.GetEntities<BeamMeUp>())
            {
                if (index >= 2) break;
                rooms[index] = b.RoomName;
                index++;
            }
            DuelView.Tag |= Tags.Global | Tags.Persistent | Tags.TransitionUpdate;
            Teleporting = true;
            yield return null;
            DuelView.ForceLevelRender = false;
            float wait = 0.5f;
            string firstRoom = Level.Session.Level;
            InstantTeleportToSpawn(Level, rooms[0], true, DuelView.MaxSize);
            yield return wait;
            Level = SceneAs<Level>();
            spawnOne = Level.GetPlayer().Position.X - Level.Camera.Position.X;
            DrawFirstRoom = true;
            yield return wait;
            InstantTeleportToSpawn(Level, rooms[1], true, DuelView.MaxSize);
            yield return wait;
            Level = SceneAs<Level>();
            spawnTwo = Level.GetPlayer().Position.X - Level.Camera.Position.X;
            DrawSecondRoom = true;
            yield return wait;
            InstantTeleportToSpawn(Level, firstRoom, true);
            Level = SceneAs<Level>();
            Drawn = true;
            Teleporting = false;
            yield return null;
            Level.Remove(Level.Tracker.GetEntity<Player>());
            DuelView.ForceLevelRender = false;
        }

        private void DrawLevel()
        {
            Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, Color.White);
            Player player = SceneAs<Level>().GetPlayer();
            if (player is null) return;
            player.Render();
        }
        private void DrawGameplay()
        {
            Draw.SpriteBatch.Draw(GameplayBuffers.Gameplay, Vector2.Zero, Color.White);
            Player player = SceneAs<Level>().GetPlayer();
            if (player is null) return;
            player.Render();
        }
        private IEnumerator MultiTeleportGlitch(float deviation, float maxTime)
        {
            Level.Add(Shader);
            Shader.ForceLevelRender = true;
            /*            Tween alphaTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, maxTime / 1.5f);
                        alphaTween.OnUpdate = (Tween t) =>
                        {
                            Shader.Alpha = t.Eased;
                        };
                        Add(alphaTween);
                        alphaTween.start();
                        float longWait;
                        float shortWait;
                        float midAmp = 0;
                        for (int i = 0; i < 10; i++)
                        {
                            Shader.Amplitude = Calc.Random.Range(Calc.Max(0, midAmp - deviation), Calc.Min(1, midAmp + deviation));
                            shortWait = Calc.Random.Range(0.1f, 0.3f);
                            longWait = Calc.Random.Range(0.5f, 0.8f);
                            yield return (Calc.Random.Range(0, 2) == 0 ? shortWait : longWait);
                            midAmp += Engine.DeltaTime / maxTime;
                        }*/
            DuelView = new DuelView();
            Level.Add(DuelView);
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {

                Shader.Amplitude = Calc.LerpClamp(Shader.Amplitude, 0, i);
                yield return null;
            }
            Shader.ForceLevelRender = false;
            DuelView.ForceLevelRender = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                DuelView.Amplitude = Calc.LerpClamp(0, 1, Ease.SineIn(i));
                DuelView.Alpha = Ease.SineIn(i);
                yield return null;
            }
        }
        private IEnumerator CleanUpBeams(float duration)
        {
            for (int i = 0; i < duration; i++)
            {
                foreach (BeamMeUp b in Level.Tracker.GetEntities<BeamMeUp>())
                {
                    if (!b.Faulty)
                    {
                        b.Alpha = i / duration;
                    }
                }
                yield return null;
            }
        }
        public static void InstantTeleportToSpawn(Level level, string room, bool cameraToSpawn = false, int size = 0)
        {
            Player player = level.GetPlayer();
            if (level == null || player is null)
            {
                return;
            }
            if (string.IsNullOrEmpty(room))
            {
                return;
            }
            level.OnEndOfFrame += delegate
            {
                Vector2 levelOffset = level.LevelOffset;
                Vector2 val2 = player.Position - level.LevelOffset;
                Vector2 val3 = level.Camera.Position - level.LevelOffset;
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
                Vector2 val4 = level.DefaultSpawnPoint - level.LevelOffset - val2;
                if (!cameraToSpawn) level.Camera.Position = level.LevelOffset + val3 + val4;
                level.Add(player);
                player.Position = session.RespawnPoint.HasValue ? session.RespawnPoint.Value : level.DefaultSpawnPoint;
                if (cameraToSpawn) level.Camera.Position = player.Center - Vector2.One * size / 2;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset + val4);
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }


            };
        }
    }
}
