using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/AscwiitParent")]
    [Tracked]
    public class AscwiitParent : Ascwiit
    {
        public int StProtect;
        public int StWait;
        public AscwiitNest Nest;
        public Vector2 Orig;
        public bool Protective;
        private Vector2 target;
        public AscwiitParent(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            StProtect = StateMachine.AddState("Protect", ProtectUpdate, null, ProtectBegin, ProtectEnd);
            StWait = StateMachine.AddState("Wait", WaitUpdate, null, WaitBegin, WaitEnd);
            StartingState = StWait;
            FleesFromPlayer = false;
        }
        public override void Awake(Scene scene)
        {
            Orig = Position;
            Nest = scene.Tracker.GetEntity<AscwiitNest>();
            IdleHops = false;
            FaceTarget(Nest.Center);
            base.Awake(scene);
        }
        public void FlyToNest()
        {
            Protective = true;
            target = Nest.Center;
            StateMachine.State = StProtect;
        }
        public void FlyToOrig()
        {
            Protective = false;
            target = Orig;
            StateMachine.State = StProtect;
        }
        public void FaceTarget(Vector2 position)
        {
            Facing = position.X < CenterX ? Facings.Left : Facings.Right;
        }
        public int ProtectUpdate()
        {
            Position += (Position - target) * (1f - (float)Math.Pow(0.0000999997764825821, Engine.DeltaTime));
            if (Protective)
            {

            }
            else if(Position.Round() == Orig && target == Orig)
            {
                Position = Orig;
                return StWait;
            }
            return StProtect;
        }
        public void ProtectBegin()
        {
            DisableGravity = true;
            DisableFriction = true;
            IgnoreSolids = true;
            FaceTarget(target);
        }
        public void ProtectEnd()
        {
            IgnoreSolids = false;
            DisableGravity = false;
            DisableFriction = false;
            FaceTarget(Nest.Center);
        }
        public int WaitUpdate()
        {
            IdleUpdate();
            return StWait;
        }
        public void WaitBegin()
        {
            IdleBegin();
            DisableGravity = false;
            DisableFriction = false;
            IgnoreSolids = false;
            IdleHops = false;
            FaceTarget(Nest.Center);
        }
        public void WaitEnd()
        {
            IdleEnd();
        }
    }
}
