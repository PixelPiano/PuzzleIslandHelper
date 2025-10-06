using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Tutorial")]
    [Tracked]
    public class TutorialPassenger : NormalPassenger
    {

        private bool watched => CutsceneWatched;
        private string id => DataCutsceneID;

        public float NormalBreath;
        public float SavedBreath;
        public float[] Dimming = new float[3];
        public float[] DimmingTimers = new float[3];
        private float min, max;
        private int lastDimmed = -1;
        private Coroutine multRoutine;
        public float prevPosition;
        public TutorialPassenger(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            prevPosition = Position.X;
            Outline = true;
            NormalBreath = BreathDuration;
            Breathes = false;
            MainWiggleMult = 0;
            Dimming = [0.5f, 0.5f, 0.5f];
            min = 0.1f;
            max = 0.6f;
            Add(new TextboxListener("Tutorial", OnChar));
            Add(multRoutine = new Coroutine(false));
        }
        private IEnumerator multRatesTo(float time, float from, float to, float breathFrom, float breathTo)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                BreathDuration = SavedBreath * Calc.LerpClamp(breathFrom, breathTo, i);
                float mult = Calc.LerpClamp(from, to, i);
                foreach (Vertex v in VertexList.Vertices)
                {
                    if (v.Shifter != null)
                    {
                        v.Shifter.Rate = v.Shifter.OriginalRate * mult;
                    }
                }
                yield return null;
            }
            BreathDuration = SavedBreath * breathTo;
        }
        public void DivideInstruction()
        {
            SavedBreath = BreathDuration;
            multRoutine.Replace(multRatesTo(0.8f, 1, 6, 1, 0.1f));
        }
        public void DivideInstructionEnd()
        {
            multRoutine.Replace(multRatesTo(1.6f, 6, 1, 0.1f, 1));
        }
        public void OnChar(FancyText.Portrait portrait, FancyText.Char c)
        {
            if (!c.IsPunctuation)
            {
                int index = Calc.Random.Range(0, 3);
                while (index == lastDimmed)
                {
                    index = Calc.Random.Range(0, 3);
                }
                lastDimmed = index;
                DimmingTimers[index] = Calc.Random.Range(0.05f, 0.1f);
                Dimming[index] = Calc.Random.Range(0.5f, 0.7f);
            }
        }
        public override void EditVertice(int index)
        {
            base.EditVertice(index);
            Color c = Vertices[index].Color;
            Vertices[index].Color = Color.Lerp(c, Color.Black, Dimming[index / 3]);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            /*            if ((scene as Level).Session.GetFlag("BeginningTalkCutsceneWatched"))
                        {
                            TurnOn();
                        }
                        else
                        {
                            foreach (ColorShifter shifter in Shifters)
                            {
                                shifter.Active = false;
                            }
                        }*/
            foreach (Vertex v in VertexList.Vertices)
            {
                if (v.Shifter != null)
                {
                    v.Shifter.Active = false;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                DimmingTimers[i] = Calc.Random.Range(0.1f, 0.4f);
            }
        }
        public override void Update()
        {
            base.Update();
            for (int i = 0; i < 3; i++)
            {
                if (TurnedOn)
                {
                    DimmingTimers[i] -= Engine.DeltaTime;
                    if (DimmingTimers[i] <= 0)
                    {
                        DimmingTimers[i] = 0;
                        Dimming[i] = 0;
                    }
                }
                else if (TurningOn)
                {
                    DimmingTimers[i] -= Engine.DeltaTime;
                    if (DimmingTimers[i] <= 0)
                    {
                        DimmingTimers[i] = Calc.Random.Range(min, max);
                        Dimming[i] = Dimming[i] == 0.7f ? 0 : 0.7f;
                    }
                }
            }
        }
        public bool TurnedOn;
        public bool TurningOn;
        public void TurnOn()
        {
            foreach (Vertex v in VertexList.Vertices)
            {
                if (v.Shifter != null)
                {
                    v.Shifter.Active = true;
                }
            }
            TurnedOn = true;
            TurningOn = false;
            for (int i = 0; i < 3; i++)
            {
                DimmingTimers[i] = 0;
                Dimming[i] = 0;
            }
            BreathDuration = NormalBreath;
            Breathes = true;
            MainWiggleMult = 1;

        }
        public void TurnOff()
        {
            foreach (Vertex v in VertexList.Vertices)
            {
                if (v.Shifter != null)
                {
                    v.Shifter.Active = false;
                }
            }
            TurnedOn = false;
            TurningOn = false;
            for (int i = 0; i < 3; i++)
            {
                DimmingTimers[i] = 0;
                Dimming[i] = 0.7f;
            }
            Breathes = false;
            MainWiggleMult = 1;

        }
        public IEnumerator TurnOnRoutine()
        {
            Breathes = true;
            TurningOn = true;
            foreach (Vertex v in VertexList.Vertices)
            {
                if (v.Shifter != null)
                {
                    v.Shifter.Active = true;
                }
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.6f)
            {
                min = Calc.LerpClamp(0.1f, Engine.DeltaTime, i);
                max = Calc.LerpClamp(0.6f, Engine.DeltaTime * 2, i);
                BreathDuration = Calc.LerpClamp(10, NormalBreath * 0.8f, Ease.SineInOut(i));
                MainWiggleMult = Calc.LerpClamp(0, 1f, Ease.SineInOut(i));
                yield return null;
            }
            for (int i = 0; i < 3; i++)
            {
                Dimming[i] = 0;
            }
            TurnedOn = true;
            TurningOn = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.6f)
            {
                BreathDuration = Calc.LerpClamp(NormalBreath * 0.8f, NormalBreath, Ease.SineInOut(i));
                yield return null;
            }
        }
        public IEnumerator TurnOffRoutine()
        {
            Breathes = true;
            TurnedOn = false;
            TurningOn = true;
           foreach(Vertex v in VertexList.Vertices)
            {
                if(v.Shifter != null)
                {
                    v.Shifter.Active = true;
                }
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.6f)
            {
                min = Calc.LerpClamp(0.1f, Engine.DeltaTime, 1 - i);
                max = Calc.LerpClamp(0.6f, Engine.DeltaTime * 2, 1 - i);
                BreathDuration = Calc.LerpClamp(10, NormalBreath * 0.8f, Ease.SineInOut(1 - i));
                MainWiggleMult = Calc.LerpClamp(0, 1f, Ease.SineInOut(1 - i));
                yield return null;
            }
            for (int i = 0; i < 3; i++)
            {
                Dimming[i] = 0.7f;
            }
            TurnedOn = false;
            TurningOn = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.6f)
            {
                BreathDuration = Calc.LerpClamp(NormalBreath * 0.8f, NormalBreath, Ease.SineInOut(1 - i));
                yield return null;
            }
            foreach (Vertex v in VertexList.Vertices)
            {
                if (v.Shifter != null)
                {
                    v.Shifter.Active = false;
                }
            }
            Breathes = false;
        }
    }

}
