using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class PortalCutscene : CutsceneEntity
    {

        private bool First;
        private bool inEvent = false;
        private bool inRotateEvent = false;
        private bool EventComplete = false;
        private float particleDistance = 8;
        private bool holdPosition;
        private bool Escaped => PianoModule.Session.Escaped;
        private readonly float[] freezeTimes = new float[] { 1, 1, 0.8f, 0.7f, 0.5f, 0.38f, 0.2f, 0.1f, 0.15f, 0.2f, 2 };
        private List<Entity> blockList;

        private TrianglePortal Portal;
        public PortalCutscene(TrianglePortal portal, bool first) : base()
        {
            Tag = Tags.TransitionUpdate;
            First = first;
            Portal = portal;
        }
        public override void OnBegin(Level level)
        {
            if ((!First && Escaped) || level.GetPlayer() is not Player player) return;
            Add(new Coroutine(Cutscene(player)));
        }

        private IEnumerator Cutscene(Player player)
        {
            if (Escaped)
            {
                yield break;
            }
            inEvent = true;
            Vector2 _position = player.Position;
            player.DummyGravity = true;
            for (float i = 0; i < 1; i += 0.01f)
            {
                player.Speed.Y = 5;
                player.MoveToX(Calc.LerpClamp(_position.X, Center.X, i));
                player.MoveToY(Calc.LerpClamp(_position.Y, Center.Y + 16, i));
                yield return null;
            }
            holdPosition = true;
            Portal.StartRotating();
            yield return 1f;
            while (!Portal.scaleStart)
            {
                yield return null;
            }

            for (float i = 0; i < 1; i += 0.08f)
            {
                player.Sprite.Scale.X = Calc.LerpClamp(1, 2f, i);
                yield return null;
            }
            for (float i = 0; i < 1; i += 0.1f)
            {

                player.Sprite.Scale.X = Calc.LerpClamp(2, 0, i);
                player.Sprite.Scale.Y = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            DigitalEffect.ForceStop = !First;
            player.Visible = false;
            for (int i = 0; i < 16; i++)
            {
                Portal.PoofParticles();
                yield return null;
            }
            yield return First ? 2 : 1;
            if (First)
            {
                yield return GlitchCutscene();
            }
            //teleport to digiC3
            inEvent = false;
            EventComplete = true;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            if ((holdPosition && inEvent) || EventComplete)
            {
                player.MoveToX(Center.X);
                player.MoveToY(Center.Y + 16);
            }
            //fadeLight
            if (Escaped)
            {
                player.Light.Alpha = 1;
                return;
            }
            bool inBounds = player.Position.X < Center.X + Width && player.Position.X > Center.X - Width;
            player.Light.Alpha = inBounds ? Math.Max(0, player.Light.Alpha - Engine.DeltaTime) : Math.Min(1, player.Light.Alpha + Engine.DeltaTime);

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            blockList = scene.Tracker.GetEntities<CustomFlagExitBlock>();
            foreach (CustomFlagExitBlock block in blockList)
            {
                block.forceChange = false;
            }
        }
        #region Coroutines
        private IEnumerator GlitchCutscene()
        {
            //audio glitching here is unintentional but makes the event 100% better because of it
            int index = 0;
            foreach (CustomFlagExitBlock block in blockList)
            {
                Celeste.Freeze(Calc.Random.Range(0.3f, 0.7f));
                block.forceChange = true;
                block.forceState = true;
                yield return index != -1 ? freezeTimes[index] : Calc.Random.Range(0.01f, 0.1f);
                index = index != -1 ? index != freezeTimes.Length - 1 ? index + 1 : -1 : -1;
            }

            float _amount = Glitch.Value;
            Glitch.Value = 1;
            yield return 5;
            Glitch.Value = _amount;
            PianoModule.Session.Escaped = true;
            yield return null;
        }

        public override void OnEnd(Level level)
        {
          
        }
        #endregion
    }
}
