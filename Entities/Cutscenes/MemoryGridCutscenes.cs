using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [CustomEvent("PuzzleIslandHelper/MemoryGridBlocks1")]
    [Tracked]
    public class MemoryGridBlocks1 : CutsceneEntity
    {
        public const string EndFlag = "MemGridBlocks1";
        public Player Player;
        public List<Entity> Blocks;
        public MemoryGridBlocks1(EventTrigger trigger, Player player, string eventID) : base()
        {
            Tag |= Tags.TransitionUpdate;
            Player = player;
        }
        public override void OnBegin(Level level)
        {
            if (!CanContinue()) return;
            LevelShaker.Intensity = 1;
            Player.StateMachine.State = Player.StDummy;
            Add(new Coroutine(Sequence()));
        }
        public bool CanContinue()
        {
            return !Level.Session.GetFlag(EndFlag);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Blocks = scene.Tracker.GetEntities<FallingBlock>().OrderByDescending(item => item.Bottom).ToList();
        }

        public override void OnEnd(Level level)
        {
            LevelShaker.Intensity = 0.1f;
            Player.StateMachine.State = Player.StNormal;
            if (WasSkipped)
            {
                foreach (var block in Blocks)
                {
                    (block as FallingBlock).Ground();
                }
                Player.Position.X = level.Marker("playerWalkBack").X;
                Player.Facing = Facings.Left;
            }
            level.Session.SetFlag(EndFlag);
        }
        private void AllBlocksFall()
        {
            float delay = 0;
            foreach (FallingBlock block in Blocks)
            {

                block.Triggered = true;
                block.FallDelay = delay;
                delay += 0.1f;
            }
        }
        private bool AllBlocksFallen()
        {
            foreach (FallingBlock block in Blocks)
            {
                if (!block.OnGround()) return false;
            }
            return true;
        }
        private IEnumerator BlockFall()
        {
            if (Blocks.Count == 0) yield break;
            Vector2 cameraTo = new Vector2(Level.Marker("playerPull").X + 40, Level.Camera.Position.Y);
            Coroutine zoomRoutine = new Coroutine(CameraTo(cameraTo, 1, Ease.SineIn));
            Add(zoomRoutine);
            yield return 0.7f;
            FallingBlock first = Blocks[0] as FallingBlock;
            AllBlocksFall();
            while (!first.Safe)
            {
                yield return null;
            }
            Player.Jump(false, false);
            Player.Facing = Facings.Left;
            yield return 0.3f;
            yield return Player.DummyWalkTo(Player.Position.X + 24f, true, 1.3f);
            while (!AllBlocksFallen()) yield return null;
            yield return 0.2f;
            Add(new Coroutine(RunToRock()));
        }
        
        private IEnumerator RunStart()
        {
            Vector2 to = Level.Marker("playerRunTo");
            Coroutine routine = new Coroutine(Level.ZoomTo(new Vector2(160, 120),1.5f, 1));
            Add(routine);
            yield return Player.DummyWalkTo(to.X);
            while(!routine.Finished) yield return null;
            yield return null;
        }
        private IEnumerator RunToRock()
        {
            float x = Blocks.OrderByDescending(block => block.Right).First().Right + 1;
            yield return Player.DummyWalkTo(x, false, 1.4f);
        }
        private IEnumerator TryPullRock()
        {
            yield return null;
        }
        private IEnumerator Wait1()
        {
            yield return 1;
        }
        private IEnumerator Wait2()
        {
            yield return 2;
        }
        private IEnumerator CameraZoom()
        {
            yield return null;
        }
        private IEnumerator CameraZoomBack()
        {
            yield return null;
        }
        private IEnumerator FallBack()
        {
            Level.Flash(Color.White);
            for (int i = 0; i < 10; i++)
            {
                Player.MoveH(3);
                yield return null;
            }
            yield return null;
        }
        private IEnumerator RumbleWeaken()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2f)
            {
                LevelShaker.Intensity = Calc.LerpClamp(1, 0.1f, Ease.SineOut(i));
                yield return null;
            }
            yield return null;
        }
        private IEnumerator Sequence()
        {
            yield return Textbox.Say("memoryGridTrapped", RunStart, CameraZoom, CameraZoomBack, BlockFall, RunToRock, TryPullRock, Wait1, Wait2, FallBack, RumbleWeaken);
            EndCutscene(Level);
        }
    }


    [CustomEvent("PuzzleIslandHelper/MemoryGridLookout")]
    [Tracked]
    public class MemoryGridLookout : CutsceneEntity
    {
        public const string EndFlag = "MemGridLookout";
        public Player Player;
        public MemoryGridLookout(EventTrigger trigger, Player player, string eventID) : base()
        {
            Tag |= Tags.TransitionUpdate;
            Player = player;
        }
        public override void OnBegin(Level level)
        {
            if (!CanContinue()) return;
            LevelShaker.Intensity = 0.1f;
            Player.StateMachine.State = Player.StDummy;
            Add(new Coroutine(Sequence(Player)));
        }
        public bool CanContinue()
        {
            return !Level.Session.GetFlag(EndFlag);
        }
        public override void OnEnd(Level level)
        {
            LevelShaker.Intensity = 0.1f;
            Player.StateMachine.State = Player.StNormal;
            if (WasSkipped)
            {
                Player.Position.X = level.Marker("player").X;
            }
            level.Session.SetFlag(EndFlag);
        }
        private bool cameraSentBack;
        private IEnumerator CameraTo()
        {
            Vector2 fromCamera = Level.Camera.Position;
            Vector2 pos = new Vector2(Level.Marker("look").X, fromCamera.Y);
            Coroutine routine = new Coroutine(CameraTo(pos, 4, Ease.SineIn));
            Add(routine);
            while (!cameraSentBack) yield return null;
            yield return CameraTo(fromCamera, 1, Ease.CubeIn);
            yield return null;
        }
        private IEnumerator CameraBack()
        {
            cameraSentBack = true;
            yield return null;
        }
        private IEnumerator Wait()
        {
            yield return 2;
        }
        private IEnumerator Sequence(Player player)
        {
            yield return player.DummyWalkTo(Level.Marker("player").X);
            yield return Textbox.Say("memoryGridLookout", Wait, CameraTo, CameraBack);
            EndCutscene(Level);
        }
    }

    [CustomEvent("PuzzleIslandHelper/MemoryGridLevelShake")]
    [Tracked]
    public class MemoryGridLevelShake : CutsceneEntity
    {
        public const string EndFlag = "MemGridLevelShake";
        private MiniTextbox textbox;
        private Player player;
        public MemoryGridLevelShake(EventTrigger trigger, Player player, string eventID) : base()
        {
            this.player = player;
        }
        public override void OnBegin(Level level)
        {
            if (!CanContinue()) return;
            LevelShaker.Intensity = 0f;
            Add(new Coroutine(cutscene()));
        }
        public bool CanContinue()
        {
            return !Level.Session.GetFlag(EndFlag) && Level.Session.GetFlag(MemoryGridLookout.EndFlag);
        }
        private IEnumerator cutscene()
        {
            bool jumped = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.6f)
            {
                if (i > 0.7f && !jumped)
                {
                    player.Jump(false, false);
                    Scene.Add(textbox = new MiniTextbox("memoryGridLevelShake"));
                    jumped = true;
                }
                LevelShaker.Intensity = Calc.LerpClamp(0f, 1, Ease.SineIn(i));
                //rumble volume = LevelShaker.Intensity;
                yield return null;
            }

            yield return null;
        }
        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                textbox.RemoveSelf();
                LevelShaker.Intensity = 1;
                //rumble volume = LevelShaker.Intensity
            }
            level.Session.SetFlag(EndFlag);
        }
    }
    [CustomEvent("PuzzleIslandHelper/MemoryGridReturn")]
    [Tracked]
    public class MemoryGridBlocksReturn : CutsceneEntity
    {
        public const string EndFlag = "MemGridBlocksReturn";
        public Player Player;
        private MiniTextbox textbox;
        public MemoryGridBlocksReturn(EventTrigger trigger, Player player, string eventID) : base()
        {
            Player = player;
        }
        public override void OnEnd(Level level)
        {
            level.Session.SetFlag(EndFlag);
        }
        private IEnumerator cutscene(Level level)
        {
            LevelShaker.Intensity = 1;
            Scene.Add(textbox = new MiniTextbox("memoryGridBlocksReturn"));
            while (textbox.Scene != null) yield return null;
            yield return null;
            EndCutscene(level);
        }
        public override void OnBegin(Level level)
        {
            if (!CanContinue()) return;
            Add(new Coroutine(cutscene(level)));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            textbox?.RemoveSelf();
        }
        public bool CanContinue()
        {
            return !Level.Session.GetFlag(EndFlag) && Level.Session.GetFlag(MemoryGridLevelShake.EndFlag);
        }


    }

    [CustomEvent("PuzzleIslandHelper/MemoryGridBlocks2")]
    [Tracked]
    public class MemoryGridBlocks2 : CutsceneEntity
    {
        public const string EndFlag = "MemGridBlocks2";
        public List<FallingBlock> LeftBlocks = new();
        public List<FallingBlock> RightBlocks = new();
        public Player Player;
        public MemoryGridBlocks2(EventTrigger trigger, Player player, string eventID) : base()
        {
            Player = player;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            List<Entity> blocks = scene.Tracker.GetEntities<FallingBlock>().OrderByDescending(item => item.Bottom).ToList();
            Level level = scene as Level;
            Vector2 levelCenter = level.Bounds.Center();
            foreach (FallingBlock block in blocks)
            {
                if (block.CenterX < levelCenter.X)
                {
                    LeftBlocks.Add(block);
                }
                else
                {
                    RightBlocks.Add(block);
                }
            }
            LeftBlocks.OrderByDescending(item => item.Bottom);
            RightBlocks.OrderByDescending(item => item.Bottom);
        }
        public override void OnEnd(Level level)
        {
            LevelShaker.Intensity = 1;
            Player.StateMachine.State = Player.StNormal;
            if (WasSkipped)
            {
                foreach (var block in LeftBlocks)
                {
                    block.Ground();
                }
                foreach (var block in RightBlocks)
                {
                    block.Ground();
                }
                Player.Facing = Facings.Left;
            }
            level.Session.SetFlag(EndFlag);
        }

        private void AllBlocksFall(List<FallingBlock> blocks)
        {
            float delay = 0;
            foreach (FallingBlock block in blocks)
            {
                block.Triggered = true;
                block.FallDelay = delay;
                delay += Engine.DeltaTime;
            }
        }
        private bool AllBlocksFallen(List<FallingBlock> blocks)
        {
            foreach (FallingBlock block in blocks)
            {
                if (!block.OnGround()) return false;
            }
            return true;
        }
        private IEnumerator BlocksFall()
        {
            Vector2 prev = Level.Camera.Position;
            yield return CameraTo(Level.Marker("cameraRight") - new Vector2(160, 90), 0.8f, Ease.CubeOut);
            Player.StateMachine.State = Player.StDummy;
            yield return null;
            AllBlocksFall(RightBlocks);
            while (!AllBlocksFallen(RightBlocks)) yield return null;
            Player.Facing = Facings.Right;
            yield return null;
            yield return CameraTo(Level.Marker("cameraLeft") - new Vector2(160, 90), 1f, Ease.CubeOut);
            yield return null;
            AllBlocksFall(LeftBlocks);
            while (!AllBlocksFallen(LeftBlocks)) yield return null;
            Player.Facing = Facings.Left;
            yield return CameraTo(prev, 0.6f, Ease.CubeIn);
            yield return null;
        }
        private IEnumerator cutscene()
        {
            yield return Textbox.Say("memoryGridBlocks2", BlocksFall);
            EndCutscene(Level);
        }
        public override void OnBegin(Level level)
        {
            if (!CanContinue()) return;
            LevelShaker.Intensity = 1;
            Add(new Coroutine(cutscene()));
        }
        public bool CanContinue()
        {
            return !Level.Session.GetFlag(EndFlag) && Level.Session.GetFlag(MemoryGridBlocksReturn.EndFlag);
        }

    }

    [Tracked]
    public class MemoryGridBoxTouch : CutsceneEntity
    {
        public const string EndFlag = "MemoryGridBoxTouch";
        public Player Player;
        public MemoryGridBox Box;
        private bool stopShaking;
        public MemoryGridBoxTouch(Player player, MemoryGridBox box) : base()
        {
            Tag |= Tags.TransitionUpdate;
            Player = player;
            Box = box;
        }
        public override void OnBegin(Level level)
        {
            if (!level.Session.GetFlag(MemoryGridBlocks2.EndFlag)) return;
            LevelShaker.Intensity = 1;
            Player.StateMachine.State = Player.StDummy;
            Add(new Coroutine(Sequence(Player)));
        }
        public bool CanContinue()
        {
            return !Level.Session.GetFlag(EndFlag) && Level.Session.GetFlag(MemoryGridBlocks2.EndFlag);
        }
        private IEnumerator Sequence(Player player)
        {
            yield return player.DummyWalkTo(Level.Marker("player").X);
            player.Facing = (Facings)Math.Sign(Box.CenterX - player.CenterX);
            yield return Textbox.Say("memoryGridBoxTouch", Wait1, Wait2, ZoomIn, ZoomOut, FloatDown, ReachOut, TouchBox, EmitPulse, StartShaking, StopShaking, LevelShake, PullIn);
            EndCutscene(Level);

            /* Level starts shaking
             * Maddy runs back to entrance
             * still blocked
             * maddy runs back to middle room
             * both entrances get blocked
             * maddy panics
             * maddy touches box
             * box starts shaking, level stops shaking
             * pause
             * level starts shaking violently
             * box pulls maddy in
             * trippy effects
             */
        }
        public override void OnEnd(Level level)
        {
            Player.StateMachine.State = Player.StNormal;
            if (WasSkipped)
            {
                Player.Position.X = level.Marker("player").X;
            }
        }

        private IEnumerator ZoomIn()
        {
            yield return null;
        }
        private IEnumerator ZoomOut()
        {
            yield return null;
        }
        private IEnumerator Wait1()
        {
            yield return 2;
        }
        private IEnumerator Wait2()
        {
            yield return 2;
        }
        private IEnumerator FloatDown()
        {
            float toY = Player.Y;
            float fromY = Box.Y;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)
            {
                Box.Y = Calc.LerpClamp(fromY, toY, Ease.SineInOut(i));
                yield return null;
            }
            yield return null;
        }
        private IEnumerator StartShaking()
        {
            Box.ShakeMult = 0;
            Box.ForceShake = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 3)
            {
                Box.ShakeMult = Calc.LerpClamp(0, 1f, Ease.SineIn(i));
                LevelShaker.Intensity = 1 - Box.ShakeMult * 0.9f;
                yield return null;
            }
            LevelShaker.Intensity = 0;
            while (!stopShaking) yield return null;
            Box.ShakeMult = 1;
            Box.ForceShake = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                LevelShaker.Intensity = Ease.SineInOut(i);
                yield return null;
            }
            LevelShaker.Intensity = 1;
            yield return null;
        }
        private IEnumerator StopShaking()
        {
            stopShaking = true;
            yield return null;
        }
        private IEnumerator EmitPulse()
        {
            yield return null;
        }
        private IEnumerator LevelShake()
        {
            yield return null;
        }
        private IEnumerator ReachOut()
        {
            yield return null;
        }
        private IEnumerator TouchBox()
        {
            yield return null;
        }
        private IEnumerator PullIn()
        {
            yield return null;
        }
    }

}
