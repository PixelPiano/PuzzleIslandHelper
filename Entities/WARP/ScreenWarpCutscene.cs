using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    public class ScreenWarpCutscene : CutsceneEntity
    {
        public static ScreenWarpCutscene Create(Scene scene, string room, Player player, bool setPlayerState = true, Action onEnd = null)
        {
            ScreenWarpCutscene cutscene = new ScreenWarpCutscene(room, player, setPlayerState, onEnd);
            scene.Add(cutscene);
            return cutscene;
        }
        private string Room;
        private Player Player;
        private bool setNormalState;
        private bool teleported;
        private Vector2 newPosition;
        private Action onEnd;
        public ScreenWarpCutscene(string room, Player player, bool setPlayerState = true, Action onEnd = null) : base()
        {
            Depth = int.MinValue;
            Room = room;
            Player = player;
            setNormalState = setPlayerState;
            this.onEnd = onEnd;
        }
        public override void Render()
        {
            base.Render();
            if (scale > 0)
            {
                MTexture tex = GFX.Game["objects/PuzzleIslandHelper/circle"];
                tex.DrawCentered(Level.Camera.Position + new Vector2(160, 90), Color.White, scale);
            }
        }
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Routine(Player)));
        }
        public override void OnEnd(Level level)
        {
            if (!teleported)
            {
                InstantTeleport(level, Player, (l, p) =>
                {
                    if (setNormalState)
                    {
                        Player.StateMachine.State = Player.StNormal;
                    }
                    onEnd?.Invoke();
                });
                scale = 0;
            }
            else
            {
                if (setNormalState)
                {
                    Player.StateMachine.State = Player.StNormal;
                }
                onEnd?.Invoke();
            }
            
        }
        private void TeleportCleanUp(Level level, Player player)
        {

            teleported = true;
            Level = Engine.Scene as Level;
            Player = Level.GetPlayer();
            makeDummy(Player);
            Level.Camera.Position = Player.CameraTarget;
            Level.Camera.Position.Clamp(Level.Bounds);
            if (Marker.TryFind("center", out Vector2 position))
            {
                Player.Position = position;
            }
        }
        private void makeDummy(Player player)
        {
            player.DisableMovement();
            player.Speed = Vector2.Zero;
            player.LiftSpeed = Vector2.Zero;
            player.DummyGravity = false;
            player.DummyFriction = false;
            player.DummyAutoAnimate = false;
            player.MuffleLanding = true;
        }
        public void InstantTeleport(Level level, Player player, Action<Level, Player> onEnd = null)
        {
            if (string.IsNullOrEmpty(Room)) return;
            level.OnEndOfFrame += delegate
            {
                FirfilStorage.Release(false);
                Vector2 levelOffset = level.LevelOffset;
                Vector2 playerPosInLevel = player.Position - level.LevelOffset;
                Vector2 camPos = level.Camera.Position - newPosition;
                float flash = level.flash;
                Color flashColor = level.flashColor;
                bool flashDraw = level.flashDrawPlayer;
                bool doFlash = level.doFlash;
                float zoom = level.Zoom;
                float zoomTarget = level.ZoomTarget;
                Facings facing = player.Facing;
                level.Remove(player);
                level.UnloadLevel();

                level.Session.Level = Room;
                Session session = level.Session;
                Level level2 = level;
                Rectangle bounds = level.Bounds;
                float left = bounds.Left;
                bounds = level.Bounds;
                session.RespawnPoint = level2.GetSpawnPoint(new Vector2(left, bounds.Top));
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.None);


                level.Zoom = zoom;
                level.ZoomTarget = zoomTarget;
                level.flash = flash;
                level.flashColor = flashColor;
                level.doFlash = doFlash;
                level.flashDrawPlayer = flashDraw;
                player.Position = level.LevelOffset + playerPosInLevel;
                level.Camera.Position = level.Tracker.GetEntity<WarpCapsule>() is WarpCapsule r ? r.Position + camPos : level.LevelOffset + camPos;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                level.Wipe?.Cancel();

                onEnd?.Invoke(level, player);
            };
        }
        private float scale;
        private IEnumerator Routine(Player player)
        {

            Player = player;
            makeDummy(player);
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                scale = Calc.LerpClamp(0, 320, Ease.CubeIn(i));
                yield return null;
            }
            AddTag(Tags.Global); //make this cutscene global so it doesn't get removed once the player is teleported
            InstantTeleport(Level, player, TeleportCleanUp); //teleport the player
                                                             //once teleported, set Parent to the receiving machine and regrab all entities needed for the cutscene from the new scene.
            yield return 1.5f;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                scale = Calc.LerpClamp(320, 0, Ease.CubeIn(i));
                yield return null;
            }
            yield return 0.4f;
            player.DummyGravity = true;
            player.DummyAutoAnimate = true;
            EndCutscene(Level);
        }
    }
}
