using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    public enum MemoryGridCutscenes
    {
        Blocks1,
        Settle,
        Lookout,
        ShakeAgain,
        Blocks2
    }
    [CustomEntity("PuzzleIslandHelper/MemoryGridCutscene")]
    [Tracked]
    public class MemoryGridCutsceneTrigger : Trigger
    {
        public MemoryGridCutscenes Cutscene;
        private string requiredFlag;
        private string flagOnEnd;
        private List<Entity> leftBlocks = new();
        private List<Entity> rightBlocks = new();
        public MemoryGridCutsceneTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Cutscene = data.Enum<MemoryGridCutscenes>("cutscene");
            requiredFlag = data.Attr("requiredFlag");
            flagOnEnd = data.Attr("flagOnEnd");

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
                    leftBlocks.Add(block);
                }
                else
                {
                    rightBlocks.Add(block);
                }
            }
            leftBlocks.OrderByDescending(item => item.Bottom);
            rightBlocks.OrderByDescending(item => item.Bottom);

        }
        public bool CheckFlag(Level level)
        {
            return (string.IsNullOrEmpty(flagOnEnd) || !level.Session.GetFlag(flagOnEnd));// && (string.IsNullOrEmpty(requiredFlag) || level.Session.GetFlag(requiredFlag));
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (CheckFlag(Scene as Level))
            {
                Scene.Add(new MemoryGridCutscene(player, Cutscene, rightBlocks, leftBlocks));
                if (!string.IsNullOrEmpty(flagOnEnd))
                {
                    SceneAs<Level>().Session.SetFlag(flagOnEnd);
                }

            }
        }
        public class MemoryGridCutscene : CutsceneEntity
        {
            private bool cameraSentBack;
            public List<Entity> LeftBlocks = new();
            public List<Entity> RightBlocks = new();
            private MiniTextbox textbox;
            public Player Player;
            public MemoryGridCutscenes Cutscene;

            public MemoryGridCutscene(Player player, MemoryGridCutscenes cutscene, List<Entity> rightBlocks, List<Entity> leftBlocks) : base()
            {
                Player = player;
                Cutscene = cutscene;
                LeftBlocks = leftBlocks;
                RightBlocks = rightBlocks;
                Tag |= Tags.TransitionUpdate | Tags.Persistent;
            }
            public override void OnBegin(Level level)
            {
                switch (Cutscene)
                {
                    case MemoryGridCutscenes.Blocks1:
                        LevelShaker.Intensity = 1;
                        Player.StateMachine.State = Player.StDummy;
                        break;
                    case MemoryGridCutscenes.Settle:
                        LevelShaker.Intensity = 1;
                        break;
                    case MemoryGridCutscenes.Lookout:
                        Player.StateMachine.State = Player.StDummy;
                        LevelShaker.Intensity = 0;
                        break;
                    case MemoryGridCutscenes.ShakeAgain:
                        LevelShaker.Intensity = 0;
                        break;
                    case MemoryGridCutscenes.Blocks2:
                        LevelShaker.Intensity = 1.5f;
                        Player.StateMachine.State = Player.StDummy;
                        break;
                }
                Add(new Coroutine(sequence()));
            }
            private IEnumerator ZoomIn()
            {
                yield return Level.ZoomTo(new Vector2(160, 120), 1.5f, 1);
            }
            private IEnumerator settleCutscene()
            {
                Player.StateMachine.State = Player.StDummy;
                Player.ForceCameraUpdate = true;
                Add(new Coroutine(Player.DummyWalkTo(Level.Marker("camera").X)));
                yield return Textbox.Say("memoryGridSettle", ZoomIn, CameraZoomBack);
            }
            private IEnumerator sequence()
            {
                bool added = false;
                switch (Cutscene)
                {
                    case MemoryGridCutscenes.Blocks1:
                        yield return Textbox.Say("memoryGridTrapped", RunStart, CameraZoomBack, BlockFall1);
                        break;
                    case MemoryGridCutscenes.Settle:
                        float from = LevelShaker.Intensity;
                        Coroutine text = new Coroutine(settleCutscene());
                        for (float i = 0; i < 1; i += Engine.DeltaTime / 2f)
                        {

                            LevelShaker.Intensity = Calc.LerpClamp(from, 0, i);
                            if (i > 0.8f && !added)
                            {
                                Add(text);
                                added = true;
                            }
                            yield return null;
                        }
                        LevelShaker.Intensity = 0;
                        while (!text.Finished) yield return null;
                        break;
                    case MemoryGridCutscenes.Lookout:
                        yield return Player.DummyWalkTo(Level.Marker("player").X);
                        yield return Textbox.Say("memoryGridLookout", Wait2, CameraTo, CameraBack);
                        break;
                    case MemoryGridCutscenes.ShakeAgain:
                        for (float i = 0; i < 1; i += Engine.DeltaTime / 0.6f)
                        {
                            if (i > 0.7f && !added)
                            {
                                Scene.Add(textbox = new MiniTextbox("memoryGridLevelShake"));
                                added = true;
                            }
                            LevelShaker.Intensity = Calc.LerpClamp(0f, 1.5f, Ease.SineIn(i));
                            //rumble volume = LevelShaker.Intensity;
                            yield return null;
                        }
                        yield return null;
                        break;
                    case MemoryGridCutscenes.Blocks2:
                        yield return Textbox.Say("memoryGridBlocks2", RightLeftBlocksFall, LookAtBox);
                        break;
                }
                EndCutscene(Level);
                yield return null;
            }
            private IEnumerator LookAtBox()
            {
                Vector2 position = Level.Marker("box");
                Player.Facing = (Facings)Math.Sign(position.X - Player.Position.X);
                yield return 0.1f;
                yield return CameraTo(position - new Vector2(160, 90), 2, Ease.SineInOut);
                yield return 0.2f;
                yield return null;
            }

            private void groundAllBlocks()
            {
                foreach (var block in LeftBlocks)
                {
                    (block as FallingBlock).Ground();
                }
                foreach (var block in RightBlocks)
                {
                    (block as FallingBlock).Ground();
                }
            }

            public override void OnEnd(Level level)
            {
                if (WasSkipped && textbox != null)
                {
                    textbox.RemoveSelf();
                }
                switch (Cutscene)
                {
                    case MemoryGridCutscenes.Blocks1:
                        LevelShaker.Intensity = 1;
                        Player.StateMachine.State = Player.StNormal;
                        if (WasSkipped)
                        {
                            groundAllBlocks();
                            Player.Position.X = level.Marker("playerRunTo").X + 24;
                            Player.Facing = Facings.Left;
                        }
                        break;
                    case MemoryGridCutscenes.Settle:
                        LevelShaker.Intensity = 0;
                        Player.StateMachine.State = Player.StNormal;
                        foreach (Coroutine c in Components.GetAll<Coroutine>())
                        {
                            c.Cancel();
                        }
                        break;
                    case MemoryGridCutscenes.Lookout:
                        LevelShaker.Intensity = 0;
                        Player.StateMachine.State = Player.StNormal;
                        if (WasSkipped)
                        {
                            Player.Position.X = level.Marker("player").X;
                        }
                        break;
                    case MemoryGridCutscenes.ShakeAgain:
                        LevelShaker.Intensity = 1.5f;
                        if (WasSkipped)
                        {
                            textbox.RemoveSelf();
                        }
                        break;
                    case MemoryGridCutscenes.Blocks2:
                        if (WasSkipped)
                        {
                            groundAllBlocks();
                            Player.Facing = Facings.Left;
                        }
                        LevelShaker.Intensity = 1.5f;
                        Player.StateMachine.State = Player.StNormal;
                        break;
                }
            }

            private void AllBlocksFall(List<Entity> blocks)
            {
                float delay = 0;
                foreach (FallingBlock block in blocks)
                {
                    block.Triggered = true;
                    block.FallDelay = delay;
                    delay += Engine.DeltaTime * 5;
                }
            }
            private bool AllBlocksFallen(List<Entity> blocks)
            {
                foreach (FallingBlock block in blocks)
                {
                    if (!block.OnGround()) return false;
                }
                return true;
            }
            private IEnumerator BlockFall1()
            {
                if (LeftBlocks.Count == 0) yield break;
                Vector2 cameraTo = Level.Camera.Position - Vector2.UnitX * 64;
                Vector2 cameraFrom = Level.Camera.Position;
                Coroutine zoomRoutine = new Coroutine(CameraTo(cameraTo, 1, Ease.CubeOut));
                Add(zoomRoutine);
                yield return 0.7f;
                FallingBlock first = LeftBlocks[0] as FallingBlock;
                AllBlocksFall(LeftBlocks);
                while (!first.Safe)
                {
                    yield return null;
                }
                Player.Jump(false, false);
                Player.Facing = Facings.Left;
                yield return 0.3f;
                yield return Player.DummyWalkTo(Player.Position.X + 24f, true, 1.3f);
                while (!AllBlocksFallen(LeftBlocks)) yield return null;
                yield return 0.2f;
                Add(new Coroutine(CameraTo(cameraFrom, 1, Ease.CubeOut)));
            }
            private IEnumerator RunStart()
            {
                Vector2 to = Level.Marker("playerRunTo");
                Coroutine routine = new Coroutine(Level.ZoomTo(new Vector2(160, 120), 1.5f, 1));
                Add(routine);
                yield return Player.DummyWalkTo(to.X);
                while (!routine.Finished) yield return null;
                yield return null;
            }
            private IEnumerator CameraZoomBack()
            {
                yield return Level.ZoomBack(1);
                yield return null;
            }
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
            private IEnumerator Wait2()
            {
                yield return 2;
            }
            private IEnumerator Wait1()
            {
                yield return 1;
            }
            private IEnumerator RightLeftBlocksFall()
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
        }

    }


    [Tracked]
    public class MemoryGridBoxTouch : CutsceneEntity
    {
        public const string EndFlag = "MemoryGridBoxTouch";
        public Player Player;
        public MemoryGridBox Box;
        private bool stopShaking;
        private bool stopWalkingAround;
        private bool finishedWalking;
        public MemoryGridBoxTouch(Player player, MemoryGridBox box) : base()
        {
            Tag |= Tags.TransitionUpdate;
            Player = player;
            Box = box;
        }
        public override void OnBegin(Level level)
        {
            LevelShaker.Intensity = 1;
            Player.StateMachine.State = Player.StDummy;
            Add(new Coroutine(Sequence(Player)));
        }
        private IEnumerator Sequence(Player player)
        {
            Add(new Coroutine(CameraTo(Level.MarkerCentered("box2"),1, Ease.SineInOut)));
            yield return player.DummyWalkTo(Level.Marker("player").X);
            player.Facing = (Facings)Math.Sign(Box.CenterX - player.CenterX);
            yield return Textbox.Say("memoryGridBoxTouch", ZoomIn, ZoomOut, Wait1, Wait2, WalkToBox, FloatDown, TouchBox, StartShaking, StopShaking, LevelShake, PullIn, EndSequence);
            EndCutscene(Level);

            /*
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
        private IEnumerator EndSequence()
        {
            yield return null;
        }
        private IEnumerator WalkToBox()
        {
            yield return Player.DummyWalkTo(Level.Marker("player").X);
        }
        private IEnumerator ZoomIn()
        {
            yield return Level.ZoomTo(new Vector2(160, 120), 1.5f, 1);
            yield return null;
        }
        private IEnumerator ZoomOut()
        {
            yield return Level.ZoomBack(1);
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
            float toY = Player.Top - 8;
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
            float from = LevelShaker.Intensity;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 3)
            {
                Box.ShakeMult = Calc.LerpClamp(0, 2f, Ease.SineIn(i));
                LevelShaker.Intensity = Calc.LerpClamp(from, 0, Ease.SineIn(i));
                yield return null;
            }
            LevelShaker.Intensity = 0;
            while (!stopShaking) yield return null;
            Box.ShakeMult = 2;
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
