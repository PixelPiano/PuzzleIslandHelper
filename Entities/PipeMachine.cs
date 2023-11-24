using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PipeMachine")]
    [Tracked]
    public class PipeMachine : Entity
    {
        private string flag;
        private bool used;
        private bool InRoutine;
        private float MachineOffsetLerp;
        public PipeMachine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 9001;
            flag = data.Attr("flag");

        }
        private IEnumerator ShakeMachine()
        {
            float amount = 1;
            bool state = false;
            float wait = 0.05f;
            Vector2 pos = Position;
            while (true)
            {
                Position.X = pos.X + (state ? amount * MachineOffsetLerp : -amount * MachineOffsetLerp);
                state = !state;
                if (MachineOffsetLerp < 1)
                {
                    MachineOffsetLerp += Engine.DeltaTime * 7;
                }
                yield return wait;
            }
        }
     
        public override void Render()
        {
            GFX.Game["objects/PuzzleIslandHelper/valve/pipes"].Draw(Position - Vector2.UnitX * 15);
            GFX.Game["objects/PuzzleIslandHelper/valve/machine"].Draw(Position);
            base.Render();

        }
    }
}