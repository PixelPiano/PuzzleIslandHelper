using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    public class UnlockStairsCutscene : CutsceneEntity
    {
        private Stairs stairs;
        private StairLock stairLock;
        private Player player;
        private Coroutine cameraToCoroutine;
        private bool dialogue;
        private Column column;
        private float columnStartY;
        private Vector2 camTarget;
        private int newFloor;

        public LevelShakeModifier ShakeMod;
        private bool? origDisableScreenShake;
        private List<SoundSource> sfxs = [];

        public UnlockStairsCutscene(Player player) : base()
        {
            this.player = player;
            dialogue = true;
        }
        public UnlockStairsCutscene(StairLock stairLock, Stairs stairs, Player player) : base()
        {
            this.stairLock = stairLock;
            this.stairs = stairs;
            this.player = player;
            Add(cameraToCoroutine = new Coroutine(false));
            Add(new PostUpdateHook(postUpdate));
        }
        public UnlockStairsCutscene(Column column, int index, Player player) : base()
        {
            newFloor = index;
            this.column = column;
            columnStartY = column.Y;
            this.player = player;
            stairLock = column.Locks[index];
            stairs = column.Stairs[index];
            Add(cameraToCoroutine = new Coroutine(false));
            Add(new PostUpdateHook(postUpdate));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(ShakeMod = new(0));
            ShakeMod.Type = LevelShakeModifier.Types.AddToValue;
            ShakeMod.Active = false;
            ShakeMod.SkipIfInactive = true;
        }
        public override void OnBegin(Level level)
        {
            earthquake = !level.Session.GetFlag("TowerEarthquake");
            origDisableScreenShake = Settings.Instance.DisableScreenShake;
            Settings.Instance.DisableScreenShake = false;
            player.StateMachine.State = Player.StDummy;
            ShakeMod.Mult = 0;
            ShakeMod.Active = true;
            Add(new Coroutine(dialogue ? dialogueRoutine() : column == null ? unmanagedRoutine() : managedRoutine()));
        }
        private IEnumerator dialogueRoutine()
        {
            yield return Textbox.Say("you don't have a key for this.");
            EndCutscene(Level);
        }
        private float cameraLerp;
        private Vector2 cameraAnchor, cameraOffset;
        private Vector2 cameraOrigin;
        private Vector2 lockOrig;
        private bool anchorCameraToLock;

        private void postUpdate()
        {
            if (anchorCameraToLock)
            {
                cameraOffset.Y = (stairLock.Y - lockOrig.Y) + stairs.StairsExtendCameraOffsetY;
                Level.Camera.Position = Vector2.Lerp(cameraOrigin, cameraAnchor + cameraOffset, cameraLerp);
            }
        }
        private IEnumerator managedRoutine()
        {
            stairLock.Unlock(true, false);
            yield return 1.1f;
            column.ShakeFor(-1);
            yield return 0.2f;
            yield return player.DummyRunTo(column.Right + 24);
            player.Face(column);
            yield return 0.2f;

            if (earthquake)
            {
                column.StopShaking();
                yield return 1.4f;
                yield return player.DummyWalkTo(column.Right + 16);
                column.ShakeFor(-1);
                Camera camera = SceneAs<Level>().Camera;
                Vector2 position = new Vector2(camera.Right + 20, camera.Top + 90);
                ShakeMod.ShakeFor(-1);
                ShakeMod.Mult = 1f;
                player.Face(column);
                player.DummyAutoAnimate = false;
                player.Sprite.Play(PlayerSprite.SitDown);
                player.Sprite.Rate = 6;
                yield return 2;
                yield return emitSounds(position);
                yield return 0.6f;
            }
            cameraOrigin = Level.Camera.Position;
            cameraAnchor = new Vector2(Math.Max(Level.Bounds.X, stairs.CenterX - 160), Level.Camera.Y);
            cameraOffset = default;
            anchorCameraToLock = true;
            lockOrig = stairLock.Position;
            lerpTween();
            yield return 1;
            if (!earthquake)
            {
                ShakeMod.ShakeFor(-1);
                ShakeMod.Mult = 1;
            }
            yield return column.MoveRoutine(column.Y - column.FloorHeight, 40f, newFloor);
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                ShakeMod.Mult = 1 - i;
                yield return null;
            }
            ShakeMod.Mult = 0;
            ShakeMod.Active = false;
            ShakeMod.StopShaking();
            column.StopShaking();
            yield return 0.7f;
            stairs.ExtendStairs(0.5f, 0.2f, 0.07f);
            while (stairs.Animating) yield return null;
            yield return 0.7f;
            anchorCameraToLock = false;
            yield return CameraTo(player.CameraTarget, 1, Ease.CubeInOut);
            if (earthquake)
            {
                yield return 1;
                player.Sprite.Rate = 1;
                player.Sprite.Reverse(PlayerSprite.SitDown, true);
                yield return player.Sprite.PlayUtil();
                player.DummyAutoAnimate = true;
            }
            EndCutscene(Level);
        }
        private void lerpTween()
        {
            Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.CubeInOut, t =>
            {
                cameraLerp = t.Eased;
            }, t =>
            {
                cameraLerp = 1;
            });
        }
        private IEnumerator unmanagedRoutine()
        {
            stairLock.Unlock(false);
            yield return 1.1f;
            Level.Shake(-1);
            yield return 0.6f;
            Rectangle bounds = Level.Bounds;
            Vector2 cameraTo = new Vector2(Math.Max(bounds.X, stairs.CenterX - 160), Math.Max(bounds.Y, stairs.CenterY - 90));
            cameraToCoroutine.Replace(CameraTo(cameraTo, 1, Ease.CubeOut));
            yield return 0.5f;
            stairs.ExtendStairs(0.5f, 0.2f, 0.07f);
            while (stairs.Animating) yield return null;
            yield return 0.1f;
            Level.StopShake();
            yield return 0.3f;
            cameraToCoroutine.Cancel();
            yield return CameraTo(player.CameraTarget, 1, Ease.CubeOut);
            EndCutscene(Level);
        }

        public override void OnEnd(Level level)
        {
            ResetScreenShakeSetting();
            anchorCameraToLock = false;
            if (WasSkipped)
            {
                if (!dialogue)
                {
                    if (!stairLock.Unlocked)
                    {
                        stairLock.Unlock(false, true);
                    }
                }
            }
            if (!dialogue)
            {
                if (column != null)
                {
                    if (earthquake)
                    {
                        level.Session.SetFlag("TowerEarthquake");
                        player.Sprite.Rate = 1;
                        player.DummyAutoAnimate = true;
                        foreach (SoundSource s in sfxs)
                        {
                            s.Stop();
                        }
                        sfxs.Clear();
                        player.X = column.Right + 16;
                    }
                    else if (!dialogue)
                    {
                        player.X = column.Right + 24;
                    }
                    column.StopShaking();
                    player.Collidable = false;
                    column.MoveToY(columnStartY - column.FloorHeight);
                    player.Collidable = true;
                    foreach (var sl in column.Locks)
                    {
                        sl.UpdateTalkBounds();
                    }
                    column.AssignStairPriorities(newFloor);
                }
                if (stairs != null)
                {
                    stairs.Enabled = true;
                    stairs.SnapStairs(true);
                    stairs.Platform.Collidable = true;
                    stairs.Collidable = true;
                }
                level.Camera.Position = player.CameraTarget;
            }
            player.StateMachine.State = Player.StNormal;

        }
        private IEnumerator emitSounds(Vector2 position)
        {
            if (!SceneAs<Level>().Session.GetFlag("brokeDumbBlock"))
            {
                SoundSource s = new SoundSource(position, "event:/game/general/wall_break_stone");
                sfxs.Add(s); //dashBlock breaks
                Add(s);
                yield return 0.5f;
            }
            SoundSource s2 = new SoundSource(position, "event:/game/general/fallblock_shake");
            sfxs.Add(s2);//ledge shakes
            Add(s2);
            yield return 0.5f;
            for (int i = 0; i < 5; i++)
            {
                //ledge snaps
                SoundSource debris = new SoundSource(position, "event:/game/general/debris_stone");
                debris.Param("debris-velocity", 80);
                sfxs.Add(debris);
                Add(debris);
                yield return Calc.Random.NextFloat() * 8 * Engine.DeltaTime;
            }
            yield return 0.7f;
            SoundSource s4 = new SoundSource(position + Vector2.UnitY * 48, "event:/game/general/fallblock_impact");
            sfxs.Add(s4);
            Add(s4);
        }
        private bool earthquake;
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            ResetScreenShakeSetting();
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            ResetScreenShakeSetting();
        }
        public void ResetScreenShakeSetting()
        {
            if (origDisableScreenShake.HasValue)
            {
                Settings.Instance.DisableScreenShake = origDisableScreenShake.Value;
                origDisableScreenShake = null;
            }
            ShakeMod.Active = false;
            ShakeMod.Mult = 0;
            ShakeMod.StopShaking();
        }
    }
}