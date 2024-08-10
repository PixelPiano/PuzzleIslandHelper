using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/Blinker")]
    [Tracked]
    public class Blinker : Entity
    {
        public float WaitTime = 0.4f;
        public static int IndexCount;
        public int Index;
        private bool state;
        private float timer;
        public static readonly MTexture On = GFX.Game["objects/PuzzleIslandHelper/blinker/on"];
        public static readonly MTexture Off = GFX.Game["objects/PuzzleIslandHelper/blinker/off"];
        private MTexture Texture;
        public VertexLight Light;
        private bool inGameshow;
        private bool inFreakout;
        public enum Modes
        {
            Static,
            Continuous,
            Oscillate,
            Bugged
        }
        public Modes Mode;
        private Modes prevMode;
        public Blinker(EntityData data, Vector2 offset) : this(data.Position + offset, data.Int("index"), data.Bool("startState"))
        {
            Collider = new Hitbox(On.Width, On.Height);
        }

        public Blinker(Vector2 position, int index, bool startState) : base(position)
        {
            Index = index;
            timer = Index * WaitTime;
            state = startState;
            Texture = state ? On : Off;
            Depth = -10000;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            inGameshow = (scene as Level).Session.Level == "Gameshow";
            if (inGameshow)
            {
                Light = new VertexLight(Color.Orange, 1, 4, 8);
            }
            else
            {
                Light = new VertexLight(Color.Orange, 1, 10, 16);

            }
            Light.Position = new Vector2(On.Width / 2, On.Height / 2);
            Add(Light);
        }
        public void Freakout()
        {
            inFreakout = true;
            Add(new Coroutine(FreakoutRoutine()));
        }
        public IEnumerator FreakoutRoutine()
        {
            int loops = Calc.Random.Range(2, 6);
            float max = Calc.Random.Range(0.1f, 0.4f);
            for (int i = 0; i < loops; i++)
            {
                if (state) TurnOff(false);
                else TurnOn(false);
                yield return Calc.Random.NextFloat(max);
            }
            TurnOff(false);
            yield return 6;
            yield return Calc.Random.Range(0, 1f);
            for (int i = 0; i < loops; i++)
            {
                if (state) TurnOff(false);
                else TurnOn(false);
                yield return Calc.Random.NextFloat(max);
            }
            TurnOn(false);
            inFreakout = false;
            yield return null;
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
            TurnOn();
            prevMode = Mode;
            Mode = Modes.Continuous;
            WaitTime = waitTime;
            timer = Index * WaitTime;
        }
        public void StartOscillate(float waitTime)
        {
            TurnOn();
            prevMode = Mode;
            Mode = Modes.Oscillate;
            WaitTime = waitTime;
            timer = WaitTime * (Index % 2 == 0 ? 2 : 1);
        }
        public override void Update()
        {
            Light.InSolidAlphaMultiplier = inGameshow ? 1 : 0.7f;
            base.Update();
            timer -= Engine.DeltaTime;
            if (Mode == Modes.Static)
            {
                timer = 0;
            }
            else if (timer < 0)
            {
                if (!inFreakout)
                {
                    if (state) TurnOff();
                    else TurnOn();
                }
                else
                {
                    state = !state;
                }

                switch (Mode)
                {
                    case Modes.Continuous:
                        timer = state ? WaitTime : IndexCount * WaitTime;
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
        public void TurnOff(bool changeState = true)
        {
            if(changeState) state = false;
            Texture = Off;
            Light.Visible = false;


        }
        public void TurnOn(bool changeState = true)
        {
            if(changeState) state = true;
            Texture = On;
            Light.Visible = true;
        }
    }
}
