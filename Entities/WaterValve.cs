using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/WaterValve")]
    [Tracked]
    public class WaterValve : Entity
    {
        private string flag;
        private bool InRoutine;
        private float MachineOffsetLerp;

        private Sprite Wheel;
        public WaterValve(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 9001;
            flag = data.Attr("flag");
            Wheel = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/valve/wheel/");
            Wheel.AddLoop("idle", "spin", 0.1f, 0);
            Wheel.Add("settle", "spin", 0.1f, "idle", 7, 8);
            Wheel.AddLoop("spinLoop", "spin", 0.1f, 3, 4, 5, 6);
            Wheel.Add("spinStart", "spin", 0.1f, "spinLoop", 0, 1, 2);
            Add(Wheel);
            Wheel.Position += new Vector2(7, 2);
            Wheel.Play("idle");
            Collider = new Hitbox(Wheel.Width, Wheel.Height, Wheel.X, Wheel.Y);
            Add(new PlayerCollider(OnPlayer));
        }
        private void OnPlayer(Player player)
        {
            if (Math.Abs(player.Speed.X) >= 160f && !InRoutine)
            {
                Add(new Coroutine(SpinWheel(Math.Sign(player.Speed.X))));
            }
        }
        private void StartSpin(bool forwards)
        {
            Wheel.Play("spinStart");
        }
        private IEnumerator ShakeMachine()
        {
            float amount = 1;
            bool state = false;
            float wait = 0.05f;
            Vector2 pos = Position;
            while (Wheel.CurrentAnimationID != "idle")
            {
                Position.X = pos.X + (state ? amount * MachineOffsetLerp : -amount * MachineOffsetLerp);
                state = !state;
                if (MachineOffsetLerp < 1)
                {
                    MachineOffsetLerp += Engine.DeltaTime * 7;
                }
                yield return wait;
            }
            Position = pos;
            MachineOffsetLerp = 0;
            yield return null;
        }
        private IEnumerator SpinWheel(int direction)
        {
            if (direction == 0)
            {
                yield break;
            }
            InRoutine = true;
            Add(new Coroutine(ShakeMachine()));
            StartSpin(direction > 0);
            yield return 1;
            Wheel.Play("settle");
            while (Wheel.CurrentAnimationID != "idle")
            {
                yield return null;
                //TODO play sound here or something
            }
            if (PianoModule.Session.GetPipeState() is > 1 and < 4)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    SceneAs<Level>().Session.SetFlag(flag);
                }
            }
            yield return null;
            InRoutine = false;
        }
        public override void Render()
        {
            GFX.Game["objects/PuzzleIslandHelper/valve/pipes"].Draw(Position - Vector2.UnitX * 15);
            GFX.Game["objects/PuzzleIslandHelper/valve/machine"].Draw(Position);
            Wheel.DrawSimpleOutline();
            base.Render();

        }
    }
}