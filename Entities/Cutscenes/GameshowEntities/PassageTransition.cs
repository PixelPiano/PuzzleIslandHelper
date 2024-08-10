using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [Tracked]
    public class PassageTransition : CutsceneEntity
    {
        public static bool Transitioning;
        public Player Player;
        public float Alpha;
        public bool HidePlayer;
        public float? NewRoomLighting;
        private List<Entity> entities = new();

        public Entity Holding;

        public Passage Passage;
        public class Door : Entity
        {
            public Sprite Sprite;
            public Image Image;
            private static BlendState Hider = new()
            {
                ColorSourceBlend = Blend.One,
                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.Zero

            };
            private VirtualRenderTarget Target;
            private VirtualRenderTarget Mask;
            public Vector2 HidingPosition;
            public Door(string folderPath) : base()
            {
                Sprite = new Sprite(GFX.Game, folderPath);
                Sprite.AddLoop("inactive", "spin", 0.1f, 0);
                Sprite.AddLoop("active", "spin", 0.1f, 3);
                Sprite.Add("intro", "spin", 0.1f, "active", 1, 2, 3);
                Sprite.Add("outro", "spin", 0.1f, "inactive", 3, 4, 5);
                Sprite.Play("inactive");
                Sprite.Visible = false;
                Add(Sprite);
                Collider = new Hitbox(Sprite.Width, Sprite.Height);
                Add(Image = new Image(GFX.Game[folderPath + "light(front)"]));
                Image.Color = Color.Transparent;
                Depth = 1;
                Target = VirtualContent.CreateRenderTarget("a", 320, 180);
                Mask = VirtualContent.CreateRenderTarget("b", (int)Width, (int)Height);
                Add(new BeforeRenderHook(BeforeRender));
            }
            private void BeforeRender()
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                if (Scene is not Level level) return;
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Mask);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
                Sprite.RenderPosition -= Position;
                Sprite.Render();
                Sprite.RenderPosition += Position;

                Draw.SpriteBatch.End();
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.Camera.Matrix);

                Sprite.Render();

                Draw.SpriteBatch.End();

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Hider, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.Camera.Matrix);
                Draw.SpriteBatch.Draw(Mask, HidingPosition, Color.White);
                Draw.SpriteBatch.End();
            }
            public override void Render()
            {
                base.Render();
                if (Scene is Level level)
                {
                    Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
                }
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
                Mask?.Dispose();
                Target = Mask = null;
            }
        }
        public PassageTransition(Player player, Passage passage) : base()
        {
            Depth = -1000001;
            Passage = passage;
            if (player.Holding != null && player.Holding.Entity != null)
            {
                Holding = player.Holding.Entity;
            }
            AddTag(Tags.Persistent);
            AddTag(Tags.Global);
            AddTag(Tags.TransitionUpdate);
            Player = player;
            Transitioning = true;

        }
        public override void Render()
        {
            base.Render();
            if (Alpha <= 0 || Scene is not Level level) return;

            Draw.Rect(level.Camera.Position, 320, 180, Color.White * Alpha);
        }
        public override void OnBegin(Level level)
        {
            Player.Visible = false;
            Player.StateMachine.State = Player.StDummy;
            Player.MuffleLanding = true;
            Player.ForceCameraUpdate = true;
            Add(new Coroutine(routine()) { UseRawDeltaTime = true });
        }
        public override void Update()
        {
            base.Update();
            if (Scene is Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    if (HidePlayer)
                    {
                        player.Visible = false;
                    }
                }
            }
        }
        public override void OnEnd(Level level)
        {
            if (level.GetPlayer() is Player player)
            {
                player.Visible = true;
                if (!Transitioning)
                {
                    player.StateMachine.State = Passage.EndPlayerState;
                }
                if (entities.Count > 0)
                {
                    level.Remove(entities[0], entities[1]);
                }
                Alpha = 0;
                if (WasSkipped)
                {
                    InstantRelativeTeleport(Scene, Passage.TeleportTo, true);
                }
            }
        }
        public static void InstantRelativeTeleport(Scene scene, string room, bool snapToSpawnPoint, int positionX = 0, int positionY = 0)
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
                //level.Add(player);
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
            };
        }
        private IEnumerator routine()
        {
            if (Scene is Level level)
            {
                for (float i = 0; i < 1; i += Engine.RawDeltaTime / Passage.FadeTime)
                {
                    Alpha = i;
                    yield return null;
                }
                Alpha = 1;
                HidePlayer = true;
                InstantRelativeTeleport(Scene, Passage.TeleportTo, true);
                yield return null;
                Level = Engine.Scene as Level;
                if (NewRoomLighting.HasValue)
                {
                    Level.Lighting.Alpha = NewRoomLighting.Value;
                }
                Player = Level.GetPlayer();
                Level.Camera.Position = Player.CameraTarget;
                Player.ForceCameraUpdate = true;
                Player.StateMachine.State = Player.StDummy;

                Door bg = new Door(Passage.FolderPath);

                Vector2 target = Player.BottomCenter - new Vector2(bg.Width / 2, bg.Height);
                Vector2 from = target + Vector2.UnitY * bg.Height;
                bg.Position = from;
                bg.HidingPosition = from;
                Image lightFg = new Image(GFX.Game[Passage.FolderPath + "light(front)"]);
                Image fgGrad = new Image(GFX.Game[Passage.FolderPath + "lightFg(front)"]);

                Entity fg = new Entity(from);

                fg.Depth = -10000;
                fg.Add(lightFg, fgGrad);
                level.Add(fg, bg);
                entities.Add(fg);
                entities.Add(bg);
                lightFg.Color = fgGrad.Color = Color.Transparent;
                for (float i = 0; i < 1; i += Engine.DeltaTime / Passage.FadeTime)
                {
                    Alpha = 1 - i;
                    yield return null;
                }
                Alpha = 0;

                Vector2 dust = from + Vector2.UnitX * (bg.Width / 2 - 1);
                bool left = false;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    bg.Position = fg.Position = Vector2.Lerp(from, target, Ease.CubeOut(i));
                    bg.Position += Calc.Random.ShakeVector();
                    Dust.Burst(dust, (left ? 165f : 15f).ToRad(), 2);
                    left = !left;
                    yield return null;
                }
                bg.Position = fg.Position = target;
                bg.Sprite.Play("intro");

                while (bg.Sprite.CurrentAnimationID == "intro") yield return null;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
                {
                    lightFg.Color = fgGrad.Color = bg.Image.Color = Passage.LightColor * i;
                    yield return null;
                }

                lightFg.Color = fgGrad.Color = bg.Image.Color = Passage.LightColor;
                HidePlayer = false;
                Player.Visible = true;
                yield return 0.5f;

                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
                {
                    lightFg.Color = Passage.LightColor * (1 - i);
                    yield return null;
                }
                lightFg.Visible = false;
                yield return 0.2f;

                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
                {
                    bg.Image.Color = fgGrad.Color = Passage.LightColor * (1 - i);
                    yield return null;
                }
                bg.Image.Visible = fgGrad.Visible = false;
                yield return 0.1f;

                Player.StateMachine.State = Passage.EndPlayerState;
                Transitioning = false;

                bg.Sprite.Play("outro");

                while (bg.Sprite.CurrentAnimationID == "outro") yield return null;
                for (float i = 0; i < 1; i += Engine.DeltaTime * 3)
                {
                    bg.Position = Vector2.Lerp(target, from, Ease.CubeIn(i));
                    bg.Position += Calc.Random.ShakeVector();
                    Dust.Burst(dust, (left ? 165f : 15f).ToRad(), 4);
                    left = !left;
                    yield return null;
                }

                level.Remove(bg, fg);
            }
            EndCutscene(Engine.Scene as Level);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Transitioning = false;
        }
    }
}
