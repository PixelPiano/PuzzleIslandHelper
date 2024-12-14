using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    public class ReadGravestoneCutscene(string dialog) : CutsceneEntity()
    {
        public ObsoleteStonePoem Poem = new ObsoleteStonePoem(dialog);

        public override void OnBegin(Level level)
        {
            if (level.GetPlayer() is Player player)
            {
                player.DisableMovement();
            }
            level.Add(Poem);
            Add(new Coroutine(Cutscene()));
        }
        private IEnumerator Cutscene()
        {
            while (Poem.InCutscene)
            {
                yield return null;
            }
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            if (level.GetPlayer() is Player player)
            {
                player.EnableMovement();
            }
            if (WasSkipped)
            {
                Poem.RemoveSelf();
            }
        }
    }
    [Tracked]
    public class ObsoleteStonePoem : Entity
    {
        public bool InCutscene = true;
        public string Dialog;
        public FancyText.Text Text;
        public Image Stone;
        public int EndIndex;
        public float Alpha = 1;
        public float IntroLerp = 0;
        public ObsoleteStonePoem(string dialog) : base()
        {
            Tag |= TagsExt.SubHUD;
            Dialog = dialog;
            Text = FancyText.Parse(Dialog, 100, 40);
            Add(new Coroutine(TextRoutine()));
        }
        private IEnumerator TextRoutine()
        {
            yield return PianoUtils.Lerp(Ease.SineInOut, 1, f => IntroLerp = f);

            foreach (FancyText.Node node in Text.Nodes)
            {
                if (node is FancyText.Char @char)
                {
                    if (char.IsLetterOrDigit((char)@char.Character))
                    {
                        //play sound
                    }
                }
                if (node is FancyText.NewLine @newLine)
                {
                    yield return 1;
                }
                if (node is FancyText.Wait wait)
                {
                    yield return wait.Duration;
                }
                EndIndex++;
                yield return 0.005f;
            }
            while (!Input.DashPressed)
            {
                yield return null;
            }
            yield return PianoUtils.Lerp(Ease.SineIn, 1, f=> Alpha = 1 - f);
            Alpha = 0;
            InCutscene = false;
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            Text.Draw(Vector2.Zero, Vector2.Zero, Vector2.One, 1, 0, EndIndex);
        }
    }
}