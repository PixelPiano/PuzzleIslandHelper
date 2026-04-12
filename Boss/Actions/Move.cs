using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using static Celeste.Mod.PuzzleIslandHelper.Boss.ActionRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity;

namespace Celeste.Mod.PuzzleIslandHelper.Boss.Actions
{
    public class Move : ActionRegistryHandler
    {
        public override string Name => "move";
        public override bool Dummy => false;
        private string marker;
        private Vector2 away;
        private float delay;
        private float delayTimer;
        private float speed;
        private float lerpTime;
        private float timeLimit;
        private float timeLimitTimer;
        private float lerpTimer;
        private Vector2 _from, _to;
        private Ease.Easer ease;
        private bool finished;
        private bool rubberband;
        private float rubberbandMult;
        private int exitDistance = -1;
        private bool snapOnEnd;
        private bool reverseEase;
        public override void Parse(XmlActionData xml)
        {
            lerpTimer = lerpTimer = xml.Get<float>("lerpTime", -1);
            timeLimit = timeLimitTimer = xml.Get<float>("timeLimit", -1);
            delayTimer = delay = xml.Get<float>("delay", -1);
            marker = xml.GetString("marker", "");
            away = xml.GetVector2("x", "y", Vector2.Zero);
            speed = xml.Get<float>("speed", 0);
            ease = xml.GetEase("ease", Ease.Linear);
            rubberband = xml.GetBool("rubberband", false);
            rubberbandMult = xml.Get<float>("rubberbandMult", 1);
            exitDistance = xml.Get<int>("exitDistance", -1);
            snapOnEnd = xml.GetBool("snap", false);
            reverseEase = xml.GetBool("reverseEase", false);
        }
        private void approachTarget(Vector2 position, Singularity s)
        {
            float easeMult = ease(lerpTimer > 0 ?
                                reverseEase ? 1 - (lerpTime / lerpTimer) : (lerpTime / lerpTimer)
                                : reverseEase ? 1 : 0);
            if (rubberband)
            {
                double factor = RubberbandFactor / rubberbandMult;
                double leftover = 1 - factor;
                RubberbandApproach(s, position, Math.Max(exitDistance, 0), factor + (leftover * easeMult));
            }
            else
            {
                s.Position = Calc.Approach(s.Position, position, speed * easeMult);
            }
            if (exitDistance >= 0 && Vector2.DistanceSquared(s.Position, position) <= exitDistance * exitDistance)
            {
                finished = true;
            }
        }
        public override void Update(Singularity s)
        {
            base.Update(s);
            if (delayTimer > 0)
            {
                delayTimer -= Engine.DeltaTime;
            }
            if (delayTimer > 0) return;
            if (timeLimitTimer > 0)
            {
                timeLimitTimer -= Engine.DeltaTime;
                if (timeLimitTimer <= 0)
                {
                    finished = true;
                }
            }
            if (finished) return;
            approachTarget(_to, s);
            if (lerpTimer > 0)
            {
                lerpTimer -= Engine.DeltaTime;
            }
        }
        public override bool ContinueToNextAction(Singularity s)
        {
            return finished;
        }
        public override void Begin(Singularity s)
        {
            delayTimer = delay;
            lerpTimer = lerpTime;
            timeLimitTimer = timeLimit;
            _from = s.Position;
            _to = !string.IsNullOrEmpty(marker) && Marker.TryFind(marker, out Vector2 position) ? position : _from + away;
            s.Idle();
        }
        public override void End(Singularity s, bool wasSkipped)
        {
            base.End(s, wasSkipped);
            if (snapOnEnd)
            {
                s.Position = _to;
            }
        }
    }
}
