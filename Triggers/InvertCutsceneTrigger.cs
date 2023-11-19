using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/InvertCutsceneTrigger")]
    public class InvertCutsceneTrigger : Trigger
    {
        private GrapherEntity Graph;
        private PuzzleSpotlight Spotlight;
        private Player player;
        private bool InCutscene;
        private Coroutine CoBackground;
        public static bool Collected;
        private float count;
        private Coroutine sizeCoroutine;
        private bool stopSizeRoutine;
        private bool stopBackgroundRoutine;
        private bool shouldRun = true;
        private float targetY;
        private float landing;
        private Level level;
        private float duration = 1;
        private List<Backdrop> backdrops;

        public InvertCutsceneTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {

        }
        private void RegisterFlag()
        {
            SceneAs<Level>().Session.SetFlag("invertOverlay");
            PianoModule.SaveData.HasInvert = true;
        }
        private void BgCleanup(Level level)
        {
            for (int i = 1; i < 7; i++)
            {
                level.Session.SetFlag($"{i}On", false);
                level.Session.SetFlag($"{i}Off", true);
            }
            level.Session.SetFlag("voidOn");
            level.Session.SetFlag("voidOff", false);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = (scene as Level);
            if (PianoModule.SaveData.HasInvert)
            {
                RemoveSelf();
            }
            SetFlag("spotlightFlag");
            player = level.Tracker.GetEntity<Player>();
            level.Add(Graph = new GrapherEntity(player.Position));
            level.Add(Spotlight = new PuzzleSpotlight(player.Position));
            backdrops = level.Background.Backdrops;
            CoBackground = new Coroutine(parallelCutscene());
            BgCleanup(level);
        }
        private void SetFlag(string flag, bool state = true)
        {
            if (level is not null)
            {
                level.Session.SetFlag(flag, state);
            }
        }
        private bool GetFlag(string flag)
        {
            if (level == null) return false;
            return level.Session.GetFlag(flag);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        private IEnumerator FlagSwitch()
        {
            count = 1;
            while (!stopBackgroundRoutine)
            {
                for (int i = 1; i < 7; i++)
                {
                    if (i != count)
                    {
                        SetFlag($"{i}On", false);
                        SetFlag($"{i}Off");
                    }
                }
                SetFlag($"{count}On");
                SetFlag($"{count}Off", false);


                count++;
                count %= 7;
                count = count == 0 ? 1 : count;
                yield return 0.2f;
            }
        }

        private IEnumerator Cutscene(Player player)
        {
            // PLAQUE CHECK
            SetFlag("getInvertDashCheck"); //Target, check for button held

            if (GetFlag("getInvertDashCheck"))
            {
                GrapherEntity.Alpha = 1;
                player.StateMachine.State = 11;
                Audio.Play("event:/PianoBoy/getInvert");

                #region VARIABLE SETUP
                player.DummyGravity = false;
                float fromY = player.Y;
                float fromX = player.X;
                targetY = -1233;
                float targetX = -1295.5f;
                landing = -1184.8f;
                GrapherEntity.size = 0;
                Graph.lineWidth = 0;
                Graph.colorAlpha = 0.3f;
                Graph.timeMod = 3;
                #endregion
                #region SPRITE SETUP
                player.DummyAutoAnimate = false;
                player.Sprite.Play("jumpFast");
                player.Sprite.Rate = 1;
                #endregion
                #region FLOATING SEQUENCE (UP)
                player.Speed = Vector2.Zero;

                for (float p1 = 0; p1 < 1; p1 += Engine.DeltaTime * 0.3f)
                {
                    player.X = fromX + (targetX - fromX) * Ease.SineInOut(p1);
                    player.Y = fromY + (targetY - fromY) * Ease.SineInOut(p1);
                    yield return null;
                }
                #endregion
            }
            player.Sprite.Play("fallFast");
            player.Sprite.Rate = 0;

            SetFlag("liquidFlag");
            #region Invert Effects
            Spotlight.Radius = 0;
            float radiusMax = 30;
            Spotlight.BeamLength = 1;
            Spotlight.BeamWidth = 0;
            Spotlight.State = true;

            for (float i = 0; i < 1; i += Engine.DeltaTime * 0.7f)
            {
                Spotlight.Radius = radiusMax * Ease.QuintIn(i);
                yield return null;
            }

            Add(new Coroutine(introSizeRoutine()));
            Graph.State = true;
            if (!CoBackground.Active)
            {
                Add(CoBackground);

            }
            Add(new Coroutine(FlagSwitch()));
            Spotlight.BeamCount = 8;
            Spotlight.offsetRate = 270;
            Spotlight.BeamLength = 20;
            Spotlight.BeamWidth = 5;
            Spotlight.GapLength = 50;
            Spotlight.hasGaps = true;
            Spotlight.offset = true;
            #endregion
            yield return new SwapImmediately(Body());
            yield return 1;


            for (float i = 0; i < 1; i += Engine.DeltaTime * 0.3f)
            {
                player.Y = targetY - (targetY - landing) * Ease.SineOut(i);
                player.Y--;
                yield return null;
            }

            yield return new SwapImmediately(End());
            RemoveSelf();
        }
        private IEnumerator Body()
        {
            Spotlight.RotateRate = 0;
            float rate;
            float[] max = { 1.8f, 2.4f, 1, 2.4f, 20 };
            float save;


            for (float i = 0; i < max[0]; i += 0.016f)
            {
                yield return null;
            }
            sizeCoroutine = new Coroutine(sineSize(10, 0.5f));
            Add(sizeCoroutine);

            save = Spotlight.RotateRate;

            rate = 0.02f;
            for (float j = 0; j < (max[1] * (100 * rate)); j += rate)
            {
                Spotlight.RotateRate = save + j;
                yield return null;
            }
            save = save + (max[1] * (100 * rate));
            rate = 0.1f;
            Spotlight.RotateRate = save;
            SetFlag("invertAnx");

            float tempLength = Spotlight.BeamLength;
            for (float i = 1; i > 0; i -= Engine.DeltaTime)
            {
                Spotlight.BeamLength = tempLength * Ease.Linear(i);
                yield return null;
            }

            Spotlight.hasGaps = false;
            tempLength = Spotlight.Radius - 5;
            float tempWidth = Spotlight.BeamWidth;
            float tempValue = 0;
            rate = 0.05f;

            for (float i = 0; i < 1; i += Engine.DeltaTime * 0.5f)
            {
                if (tempValue < (max[2] * (100 * rate)))
                {
                    Spotlight.RotateRate = save + (tempValue * Ease.SineIn(i));
                    tempValue += rate;
                }
                Spotlight.BeamLength = tempLength + (320 - tempLength) * Ease.QuintIn(i);
                Spotlight.BeamWidth = tempLength + (1 - tempLength) * Ease.QuintIn(i);
                yield return null;
            }
            float tempRate = Spotlight.RotateRate;
            for (float i = 1; i > 0; i -= Engine.DeltaTime * 0.4f)
            {
                Spotlight.RotateRate = tempRate - (tempRate * Ease.QuintIn(i));
                save = Spotlight.RotateRate;
                yield return null;
            }


            tempRate = Spotlight.RotateRate;
            for (float m = 0; m < 2f; m += Engine.DeltaTime * 0.4f)
            {
                Spotlight.RotateRate = tempRate - Ease.QuintIn(m);
                yield return null;
            }
            save = Spotlight.RotateRate;

            for (float i = 0; i < max[4]; i += rate)
            {
                Spotlight.RotateRate = save - i;
                if (Spotlight.BeamCount < 600)
                {
                    GrapherEntity.size += 0.01f;
                    Spotlight.BeamCount++;
                }
                yield return null;
            }
            save = Spotlight.RotateRate;
            for (float i = 0; i < 4; i += rate)
            {
                Spotlight.RotateRate = save - i;
                Spotlight.BeamCount--;
                GrapherEntity.size -= 2.5f;
                yield return null;
            }
            save = Spotlight.RotateRate;
            for (float i = 0; i < 16; i += rate)
            {
                Spotlight.RotateRate = save - i;
                Spotlight.BeamCount += 7;
                yield return null;
            }
            save = Spotlight.RotateRate;
            float beamSave = Spotlight.BeamCount;
            float sizeSave = GrapherEntity.size;
            float _size = 0;
            for (float i = 1; i < 61; i++)
            {
                Spotlight.RotateRate = save + (i / 4);
                Spotlight.BeamCount -= (int)(beamSave / 60);
                _size = Calc.Approach(_size, sizeSave, sizeSave / 40);
                GrapherEntity.size = sizeSave - _size;
                yield return null;
            }
            float radius = Spotlight.Radius;
            Spotlight.RotateRate = 0;
            Spotlight.offset = false;
            Spotlight.BeamWidth = 1;
            Spotlight.BeamLength = radius - 5;
            Spotlight.BeamCount = 8;
            yield return 0.6f;
            duration = 4;
            player.Sprite.Play("idle_powerUp");
            player.Sprite.Rate = 4;
            for (float i = 0; i < 1; i += Engine.DeltaTime * 0.7f)
            {
                Spotlight.Radius = radius - (radius * Ease.SineIn(i));
                yield return null;
            }
            yield return 1;
            tempLength = Spotlight.BeamLength;
            Add(new Coroutine(endingSizeRoutine()));

            for (float i = 0; i < 1; i += Engine.DeltaTime * 0.7f)
            {
                Spotlight.BeamLength = Spotlight.BeamLength < 0 ? 0 : tempLength - (tempLength * Ease.SineInOut(i));
                GrapherEntity.Alpha = 1 - i;
                yield return null;
            }
            player.Sprite.Play("fallFast");
            player.Sprite.Rate = 1;
            yield return null;
        }
        private IEnumerator End()
        {
            SetFlag("invertAnx", false);
            SetFlag("liquidFlag", false);
            stopBackgroundRoutine = true;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("tired");

            Graph.State = false;
            Spotlight.State = false;
            RegisterFlag();
            Collected = true;
            BgCleanup(level);
            level.Session.SetFlag("voidOn", false);
            level.Session.SetFlag("voidOff", true);
            yield return 3;
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
            player.StateMachine.State = 0;
            yield return null;

        }
        public override void Update()
        {
            base.Update();
            if (level is null)
            {
                return;
            }
            if (InCutscene)
            {
                if (!Collected)
                {
                    if (GetFlag("invertAnx"))
                    {
                        Distort.Anxiety = Calc.Approach(Distort.Anxiety, 0.6f, Engine.DeltaTime);
                    }
                }
                else
                {
                    Distort.Anxiety = Calc.Approach(Distort.Anxiety, 0, Engine.DeltaTime);
                }
            }

        }

        public override void OnEnter(Player player)
        {
            if (!InCutscene && !Collected)
            {
                Add(new Coroutine(Cutscene(player)));
                InCutscene = true;
            }
        }

        public override void OnLeave(Player player)
        {
        }
        private IEnumerator endingSizeRoutine()
        {
            float size = GrapherEntity.size;
            stopSizeRoutine = true;
             for(float i = 0; i < 1; i += Engine.DeltaTime)
            {
                GrapherEntity.size = Calc.Approach(size, 0, size * Ease.SineOut(i));
                yield return null;
            }
            Graph.lineWidth = 0;
            yield return null;
        }
        private IEnumerator introSizeRoutine()
        {
            yield return new SwapImmediately(toLineWidth(2, true, 0.5f));
            yield return new SwapImmediately(toSize(120, true, 0.5f));
        }

        private IEnumerator sineSize(float addBy, float rate)
        {
            while (shouldRun && !stopSizeRoutine)
            {
                for (float p1 = 0.0f; p1 < 1.0f; p1 += Engine.DeltaTime * rate)
                {
                    GrapherEntity.size += addBy * Ease.SineInOut(p1);
                    yield return null;
                }
                for (float p1 = 0.0f; p1 < 1.0f; p1 += Engine.DeltaTime * rate)
                {
                    GrapherEntity.size -= addBy * Ease.SineInOut(p1);
                    yield return null;
                }
            }
        }
        private void setBackdropAlphas(float alpha)
        {
            int count = 0;
            foreach (Backdrop backdrop in backdrops)
            {
                if (backdrop.Tags.Contains("invert"))
                {
                    backdrop.FadeAlphaMultiplier = alpha;
                    count++;
                    if (count == 6)
                    {
                        break;
                    }
                }
            }
        }


        private IEnumerator parallelCutscene()
        {

            while (!stopBackgroundRoutine)
            {
                float startup = 0.3f;
                for (float j = 0.0f; j < 0.3f; j += Engine.DeltaTime / duration)
                {
                    setBackdropAlphas(startup + Ease.SineIn(j));
                    yield return null;
                }
                for (float j = 0.0f; j < 0.3f; j += Engine.DeltaTime / duration)
                {
                    setBackdropAlphas(0.3f - Ease.SineIn(j) + startup);
                    yield return null;
                }
            }

        }

        private IEnumerator toSize(float size, bool ease, float rate)
        {
            float tempSize = GrapherEntity.size;
            bool add = tempSize < size;
            for (float j = 0.0f; j < 1; j += Engine.DeltaTime * rate)
            {
                float value = add ? tempSize + (size - tempSize) : tempSize;
                GrapherEntity.size = (add ? tempSize + (size - tempSize) : tempSize) * (ease ? Ease.SineInOut(j) : Ease.Linear(j));
                if (GrapherEntity.size < size && !add)
                {
                    GrapherEntity.size = size;
                    yield return null;
                    break;
                }
                yield return null;
            }
            yield return null;
        }
        private IEnumerator toLineWidth(float width, bool ease, float rate)
        {
            float tempSize = Graph.lineWidth;
            bool add = tempSize < width;
            for (float j = 0.0f; j < 1; j += Engine.DeltaTime * rate)
            {
                float value = add ? tempSize + (width - tempSize) : tempSize;
                Graph.lineWidth = (add ? tempSize + (width - tempSize) : tempSize) * (ease ? Ease.SineInOut(j) : Ease.Linear(j));
                if (Graph.lineWidth < width && !add)
                {
                    Graph.lineWidth = width;
                    yield return null;
                    break;
                }
                yield return null;
            }
            yield return null;
        }
    }
}
