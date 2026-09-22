using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    public static class FreezeTimeBreakExt
    {
        public static void TimelessBreak(this Solid solid, char tileType, Vector2 from, FlagList flagOnBreak = default, bool playSound = true, bool playDebrisSound = true, bool loseCollision = true, bool removeSelf = true)
        {
            if (playSound)
            {
                tileType.BreakSfx(solid.Center);
            }
            solid.SpawnTimelessDebris(from, tileType, playDebrisSound);
            if (loseCollision) solid.Collidable = false;
            flagOnBreak.State = true;
            if (removeSelf)
            {
                if (solid is DashBlock block && block.permanent) block.RemoveAndFlagAsGone();
                else solid.RemoveSelf();
            }
        }
        public static void TimelessBreak(this Solid solid, char tileType, Vector2 from, FlagList flagOnBreak = default, bool playSound = true, bool playDebrisSound = true) => solid.TimelessBreak(tileType, from, flagOnBreak, playSound, playDebrisSound, true, true);
        public static void TimelessBreak(this Solid solid, char tileType, Vector2 from) => solid.TimelessBreak(tileType, from, default, true, true, true, true);
        public static void ShakeFallSfx(this char tileType, Vector2 position)
        {
            if (tileType == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", position);
            }
            else if (tileType == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_shake", position);
            }
            else if (tileType == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_shake", position);
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_shake", position);
            }
        }
        public static void BreakSfx(this char tileType, Vector2 position)
        {
            if (tileType == '1')
            {
                Audio.Play("event:/game/general/wall_break_dirt", position);
            }
            else if (tileType == '3')
            {
                Audio.Play("event:/game/general/wall_break_ice", position);
            }
            else if (tileType == '9')
            {
                Audio.Play("event:/game/general/wall_break_wood", position);
            }
            else
            {
                Audio.Play("event:/game/general/wall_break_stone", position);
            }
        }
        public static void SpawnTimelessDebris(Scene scene, Vector2 position, float width, float height, Vector2 blastFrom, char tileType, bool playSound = true)
        {
            for (int i = 0; i < width / 8f; i++)
            {
                for (int j = 0; j < height / 8f; j++)
                {
                    scene.Add(Engine.Pooler.Create<TimelessDebris>().Init(position + new Vector2(4 + i * 8, 4 + j * 8), tileType, playSound).BlastFrom(blastFrom));
                }
            }
        }
        public static void SpawnTimelessDebris(this Entity entity, Vector2 blastFrom, char tileType, bool playSound = true) => SpawnTimelessDebris(entity.Scene, entity.Position, entity.Width, entity.Height, blastFrom, tileType, playSound);
    }
    [Tracked]
    public class FreezeTimeBreak : CutsceneEntity
    {
        public static FreezeTimeBreak Begin(Solid solid, char tileType, float shakeTime, FlagList flagOnEnd = default, bool loseCollision = true, bool removeSelf = true, Action onEnd = null)
        {
            FreezeTimeBreak cutscene = new(solid, tileType, shakeTime, flagOnEnd, loseCollision, removeSelf);
            solid.Scene.Add(cutscene);
            return cutscene;
        }
        private char tileType;
        private float shakeTime;
        private FlagList flagOnEnd;
        private Solid solid;
        private TimeRateModifier timeModifier;
        private bool removeSelf;
        private bool loseCollision;
        private BetterShaker shaker;
        private Action onEnd;
        public FreezeTimeBreak(Solid solid, char tileType, float shakeTime, FlagList flagOnEnd, bool loseCollision = true, bool removeSelf = true, Action onEnd = null) : base()
        {
            this.onEnd = onEnd;
            this.solid = solid;
            this.tileType = tileType;
            this.shakeTime = shakeTime;
            this.flagOnEnd = flagOnEnd;
            this.removeSelf = removeSelf;
            this.loseCollision = loseCollision;
            solid.Add(shaker = new BetterShaker(solid.OnShake)
            {
                UseRawDeltaTime = true
            });
            Add(timeModifier = new TimeRateModifier(0, false));
        }
        public override void OnBegin(Level level)
        {
            level.DisableMovement();
            Vector2 position = (solid.Center - new Vector2(160, 90)).Clamp(level.Bounds);
            Add(new Coroutine(routine(position)) { UseRawDeltaTime = true });
        }
        public static IEnumerator CameraToRaw(Vector2 target, float duration, Ease.Easer ease = null, float delay = 0f, bool clampToLevel = true)
        {
            if (ease == null)
            {
                ease = Ease.CubeInOut;
            }

            if (delay > 0f)
            {
                yield return delay;
            }

            Level level = Engine.Scene as Level;
            Vector2 from = level.Camera.Position;
            Rectangle bounds = level.Bounds;
            if (clampToLevel)
            {
                target.X = Math.Clamp(target.X, bounds.Left, bounds.Right - 320);
                target.Y = Math.Clamp(target.Y, bounds.Top, bounds.Bottom - 180);
            }
            for (float p = 0f; p < 1f; p += Engine.RawDeltaTime / duration)
            {
                level.Camera.Position = from + (target - from) * ease(p);
                yield return null;
            }

            level.Camera.Position = target;
        }
        private IEnumerator routine(Vector2 camPos)
        {
            Player player = Level.GetPlayer();
            if (player != null)
            {
                player.DisableMovement();
                while (!player.onGround)
                {
                    yield return null;
                }
            }
            Vector2 prev = Level.Camera.Position;

            timeModifier.Enabled = true;
            if (Level.Camera.Position != camPos)
            {
                yield return CameraToRaw(camPos, 1, Ease.CubeOut);
            }
            else
            {
                yield return 0.5f;
            }

            if (shakeTime > 0)
            {
                tileType.ShakeFallSfx(solid.Center);
                shaker.ShakeFor(-1);
                yield return shakeTime;
            }
            solid.TimelessBreak(tileType, solid.Center, flagOnEnd, loseCollision: loseCollision, removeSelf: removeSelf);
            timeModifier.Enabled = false;
            yield return 0.5f;
            yield return CameraToRaw(prev, 1, Ease.CubeOut);
            EndCutscene(Level);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            shaker.RemoveSelf();
            timeModifier.Multiplier = 1; 
            timeModifier.RemoveSelf();
        }
        public override void OnEnd(Level level)
        {
            onEnd?.Invoke();
            shaker.RemoveSelf();
            timeModifier.Multiplier = 1;
            timeModifier.RemoveSelf();
            if (WasSkipped)
            {
                if (solid != null)
                {
                    if (removeSelf)
                    {
                        if (solid is DashBlock dashBlock && dashBlock.permanent)
                        {
                            dashBlock.RemoveAndFlagAsGone();
                        }
                        else
                        {
                            solid.RemoveSelf();
                        }
                    }
                    if (loseCollision)
                    {
                        solid.Collidable = false;
                    }
                }
            }
            level.EnableMovement();
        }
    }
}
