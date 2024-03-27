using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
// PuzzleIslandHelper.SpeedCodePuzzle
//Code is a modified combination of FrostHelper's "Dash Code Trigger" and XaphanHelper's "Custom Collectable Entity"
namespace Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities
{
    [CustomEntity("PuzzleIslandHelper/SpeedCodePuzzle")]
    public class SpeedCodePuzzle : Entity
    {
        private float lerpAmount = 0;
        private float leadLerp = 0.1f;
        private float endLerp = 0.08f;
        private float lerpDelay = 0.2f;
        private float duration;
        private int cycles = 5;
        private int[] placements = new int[] { 0, 0, 0, 0, 0, 0 };
        private float opacity = 1;
        private bool resultsFinished = false;
        private bool inLerpRoutine = false;
        private bool inRoutine = false;
        private bool gotCode = false;
        private bool inOpacityRoutine = false;
        private bool startState;

        private Vector2 startingPosition;
        public EntityID ID;

        private string folder = "objects/PuzzleIslandHelper/speedCodePuzzle/";
        private string[] code = new string[] { "U", "DL", "DL", "UL", "UR", "U" };

        private List<string> currentInputs = new List<string>();
        private Sprite Monitor;
        private MTexture[] results = new MTexture[6];
        private MTexture[] blocks = new MTexture[8];
        private DashListener dashListener;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Monitor.Play("idle");
            startingPosition = new Vector2(Monitor.X + 32, Monitor.Y + 16);
            if (startState)
            {
                InitializeTextures();
            }
        }
        private void InitializeTextures()
        {
            for (int i = 0; i < 8; i++)
            {
                blocks[i] = GFX.Game[folder + StringFromInt(i) + "00"];

            }
            for (int i = 0; i < 6; i++)
            {
                placements[i] = Calc.Random.Range(0, 8);
                results[i] = GFX.Game[folder + StringFromInt(placements[i]) + "00"];
            }
        }
        private string StringFromInt(int a)
        {
            return a == 0 ? "up" : a == 1 ? "upLeft" :
                    a == 2 ? "left" : a == 3 ? "downLeft" :
                    a == 4 ? "down" : a == 5 ? "downRight" :
                    a == 6 ? "right" : a == 7 ? "upRight" : "up";
        }
        private void SetPlacements(int[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                results[i] = blocks[list[i]];
            }
        }
        public SpeedCodePuzzle(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            Add(Monitor = new Sprite(GFX.Game, folder));
            Monitor.AddLoop("idle", "marioxhk_monitor_longer", 0.1f);
            duration = 1 / leadLerp + lerpDelay + 1 / endLerp;
            gotCode = false;
            Add(dashListener = new DashListener());
            dashListener.OnDash = delegate (Vector2 dir)
            {
                string text = "";

                text = dir.Y < 0f ? "U" : dir.Y > 0f ? "D" : "";
                text += dir.X < 0f ? "L" : dir.X > 0f ? "R" : "";
                currentInputs.Add(text);
                Logger.Log(LogLevel.Warn, "PuzzleLog", currentInputs.ToString());
                if (!gotCode)
                {
                    if (currentInputs.Count > code.Length)
                    {
                        currentInputs.RemoveAt(0);
                    }
                    if (currentInputs.Count == code.Length)
                    {
                        bool foo = true;
                        for (int i = 0; i < code.Length; i++)
                        {
                            foo = !currentInputs[i].Equals(code[i]) ? false : foo;
                        }

                        if (foo)
                        {
                            gotCode = true;
                            //DoSomething();
                        }
                    }
                }

            };
            Collider = new Hitbox(Monitor.Width, Monitor.Height);
            Depth = 1;
            startState = data.Bool("startState", true);
        }

        private IEnumerator Randomize()
        {
            inRoutine = true;
            resultsFinished = false;
            for (int i = 0; i < 90; i++)
            {
                for (int j = 0; j < placements.Length; j++)
                {
                    placements[j] = Calc.Random.Range(0, 8);
                }
                SetPlacements(placements);
                code = CreateCode(placements);
                yield return 2f / 60f;
            }
            resultsFinished = true;
            yield return duration / 60;
            inRoutine = false;
        }
        public override void Render()
        {
            base.Render();
            if (Scene as Level == null || SceneAs<Level>().Session.GetFlag("SpeedCodePuzzleCompleted") || !startState)
            {
                return;
            }
            for (int i = 0; i < results.Length; i++)
            {
                results[i].ScaleFix = 1;
                results[i].Draw(Position + startingPosition +
                                new Vector2(i * results[i].Width, 0), Vector2.Zero,
                                Color.Lerp(Color.Green, Color.LightGreen, lerpAmount) * opacity);
            }
        }
        private string[] CreateCode(int[] list)
        {
            string[] output = new string[list.Length];
            int a;
            for (int i = 0; i < list.Length; i++)
            {
                a = list[i];
                output[i] = a == 0 ? "U" : a == 1 ? "UL" : a == 2 ? "L" :
                            a == 3 ? "DL" : a == 4 ? "D" : a == 5 ? "DR" :
                            a == 6 ? "R" : a == 7 ? "UR" : "N/A";
            }
            return output;
        }
        public override void Update()
        {
            base.Update();
            if (startState)
            {
                if (SceneAs<Level>().Session.GetFlag("SpeedCodePuzzleCompleted"))
                {
                    return;
                }
                if (!inRoutine && !inLerpRoutine)
                {
                    Add(new Coroutine(Randomize(), true));
                }
                if (resultsFinished && !inLerpRoutine)
                {
                    Add(new Coroutine(Tick(), true));
                }
                if (gotCode && !inOpacityRoutine)
                {
                    Add(new Coroutine(Fade(), true));
                }
            }
        }
        private IEnumerator Fade()
        {
            inOpacityRoutine = true;
            while (inLerpRoutine || inRoutine) //wait until other routines are finished
            {
                yield return null;
            }

            for (float i = 0; i < 1; i += 0.1f)
            {
                opacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            opacity = 0;
            SceneAs<Level>().Session.SetFlag("SpeedCodePuzzleCompleted", true);
            yield return null;
            inOpacityRoutine = false;
        }
        private IEnumerator Tick()
        {
            inLerpRoutine = true;
            resultsFinished = false;

            for (int j = 0; j < cycles; j++)
            {
                //play sound
                for (float i = 0; i < 1; i += leadLerp)
                {
                    lerpAmount = Calc.LerpClamp(0, 1, i);
                    yield return null;
                }
                yield return lerpDelay;
                for (float i = 0; i < 1; i += endLerp)
                {
                    lerpAmount = Calc.LerpClamp(1, 0, i);
                    yield return null;
                }
            }
            inLerpRoutine = false;
        }
    }
}