//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PersistentTileWarp")]
    [Tracked]
    public class PersistentTileWarp : Entity
    {
        private string RoomName;
        private Player player;
        private bool InRoutine;
        private bool Teleported;
        private bool StartDrawing;
        private float Opacity = 1;
        public VirtualRenderTarget Target;
        private bool IsGlobal;
        public bool Colliding
        {
            get
            {
                return player.Collider.Bounds.Intersects(Collider.Bounds);
            }
        }
        public PersistentTileWarp(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            RoomName = data.Attr("roomName");
            Collider = new Hitbox(data.Width, data.Height);
            TransitionListener Listener = new TransitionListener();
            Add(Listener);

            Listener.OnOutBegin = FadeOut;
            Add(new BeforeRenderHook(BeforeRender));
        }
        private void FadeOut()
        {
            Tween t = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 1);
            t.OnUpdate = (Tween t) =>
            {
                Opacity = 1 - t.Eased;
            };
            t.OnComplete = (Tween t) =>
            {
                Opacity = 0;
                RemoveSelf();
            };
            Add(t);
            t.Start();
        }


        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            player = level.Tracker.GetEntity<Player>();
            Target = VirtualContent.CreateRenderTarget("PersistentTileWarpTarget", level.Bounds.Width, level.Bounds.Height);
        }
        public override void Update()
        {
            base.Update();
            IsGlobal = TagCheck(Tags.Global);
            if (Teleported)
            {
                
                return;
            }
            if (player is not null)
            {
                if (Colliding && !InRoutine)
                {
                    Add(new Coroutine(TeleportRoutine()));
                }
            }

        }
        private IEnumerator TeleportRoutine()
        {
            InRoutine = true;
            PrepTeleport();
            yield return null;
            InstantTeleport(SceneAs<Level>(), player, RoomName);
            Teleported = true;
            SceneAs<Level>().SolidTiles.Visible = false;
            //RemoveTag(Tags.Global);
            player = Scene.Tracker.GetEntity<Player>();
        }

        private void DrawTiles()
        {
            if (Scene is not Level level)
            {
                return;
            }
            level.SolidTiles.Render();
            //Draw.Rect(0,0,level.Bounds.Width,level.Bounds.Height,Color.Red);
        }
        private void BeforeRender()
        {
            if (Scene is not Level level)
            {
                return;
            }
            if (!Teleported)
            {
                Target.DrawToObject(DrawTiles, Matrix.Identity, true);
                //level.SolidTiles.Visible = true;
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level)
            {
                return;
            }
            if (Teleported)
            {
                //level.SolidTiles.Visible = false;
                Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White * Opacity);
            }
        }
        private void PrepTeleport()
        {
            AddTag(Tags.Global);
        }

        public static void InstantTeleport(Scene scene, Player player, string room)
        {
            Level level = scene as Level;
            if (level == null)
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
                level.LoadLevel(Player.IntroTypes.Transition);

                level.Camera.Position = level.LevelOffset + val3;
                level.Add(player);

                player.Position = level.LevelOffset + val2;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

        }
    }
}
