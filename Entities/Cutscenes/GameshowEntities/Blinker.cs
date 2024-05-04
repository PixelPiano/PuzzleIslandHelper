using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Threading;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/Blinker")]
    [Tracked]
    public class Blinker : Entity
    {
        public float WaitTime = 0.4f;
        public static int IndexCount;
        public int Index;
        public bool State;
        private float timer;
        public static readonly MTexture On = GFX.Game["objects/PuzzleIslandHelper/blinker/on"];
        public static readonly MTexture Off = GFX.Game["objects/PuzzleIslandHelper/blinker/off"];
        private MTexture Texture => State ? On : Off;
        public enum Modes
        {
            Static,
            Continuous,
            Oscillate
        }
        public Modes Mode;
        private VertexLight Glow;
        public Blinker(EntityData data, Vector2 offset) : this(data.Position + offset, data.Int("index"), data.Bool("startState"))
        {

        }
        public Blinker(Vector2 position, int index, bool startState) : base(position)
        {
            Index = index;
            timer = Index * WaitTime;
            State = startState;
            Depth = -10001;
            Glow = new VertexLight(Color.Orange, 1, 8, 16);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Index > IndexCount)
            {
                IndexCount = Index;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            IndexCount = 0;
        }
        public void StartContinuous(float waitTime)
        {
            Mode = Modes.Continuous;
            WaitTime = waitTime;
            timer = Index * WaitTime;
        }
        public void StartOscillate(float waitTime)
        {
            Mode = Modes.Oscillate;
            WaitTime = waitTime;
            timer = WaitTime * (Index % 2 == 0 ? 2 : 1);
        }
        public override void Update()
        {
            base.Update();
            if (Index > IndexCount)
            {
                IndexCount = Index;
            }
            Glow.Visible = State;
            Glow.InSolidAlphaMultiplier = 1;
            timer -= Engine.DeltaTime;
            if (Mode == Modes.Static)
            {
                timer = 0;
            }
            if (timer < 0)
            {
                State = !State;
                switch (Mode)
                {
                    case Modes.Continuous:
                        timer = State ? WaitTime : IndexCount * WaitTime;
                        break;
                    case Modes.Oscillate:
                        timer = WaitTime;
                        break;
                }
            }


        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position, Color.White);
        }
        public void Blink(float time)
        {
            Add(new Coroutine(BlinkFor(time)));
        }
        public IEnumerator BlinkFor(float time)
        {
            TurnOff();
            yield return time;
            TurnOn();
        }
        public void TurnOff()
        {
            State = false;
        }
        public void TurnOn()
        {
            State = true;
        }
    }
}
