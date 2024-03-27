using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class FloppyHUD : Entity
    {
        private int PerLine = 4;
        private float XSpacing = 20;
        private float YSpacing = 10;
        public bool InControl;
        private float rotation;
        private float yOffset = 320;
        private bool HasChosen;
        private float selectedYOffset;
        private float selectedRotation;
        private float backOpacity;
        private float maxOpacity = 0.7f;
        private int Selected;
        public FloppyDisk SelectedDisk;
        private bool InRoutine;
        public bool LeftEarly;
        public FloppyHUD() : base(Vector2.Zero)
        {
            Tag |= TagsExt.SubHUD;
        }
        private IEnumerator EnterRoutine(Ease.Easer Ease)
        {
            InControl = false;

            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                backOpacity = Calc.LerpClamp(0, maxOpacity, Ease(i));
                rotation = Calc.LerpClamp(90f.ToRad(), 0, Ease(i));
                yOffset = Calc.LerpClamp(160, 0, Ease(i));
                yield return null;
            }
            yOffset = 0;
            backOpacity = maxOpacity;
            rotation = 0;
            InControl = true;
        }
        private IEnumerator LeaveRoutine(Ease.Easer Ease, bool early = false)
        {
            InControl = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                rotation = Calc.LerpClamp(0, 90f.ToRad(), Ease(i));
                yOffset = Calc.LerpClamp(0, 160, Ease(i));
                if (early)
                {
                    backOpacity = Calc.LerpClamp(maxOpacity, 0, Ease(i));
                }
                yield return null;
            }

            yOffset = 180;
            rotation = 90f.ToRad();
            if (!early)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    backOpacity = Calc.LerpClamp(maxOpacity, 0, Ease(i));
                    selectedYOffset = Calc.LerpClamp(0, -160, Ease(i));
                    selectedRotation = Calc.LerpClamp(0, -90f.ToRad(), Ease(i));
                    yield return null;
                }
            }
            backOpacity = 0;
        }
        public override void Render()
        {
            base.Render();
            if (InRoutine)
            {
                if (backOpacity > 0)
                {
                    Draw.Rect(0, 0, 1920, 1080, Color.Black * backOpacity);
                }
                DrawDisks();
            }
        }
        public IEnumerator Sequence()
        {
            float buffer = 0.15f;
            float timer = buffer;
            HasChosen = false;
            SelectedDisk = null;
            Selected = 0;
            InRoutine = true;
            yOffset = 320;
            selectedYOffset = 0;
            rotation = 90f.ToRad();
            selectedRotation = 0;
            yield return EnterRoutine(Ease.SineOut);
            while (!Input.DashPressed)
            {
                if (Input.Jump.Pressed)
                {
                    yield return LeaveRoutine(Ease.SineIn, true);
                    LeftEarly = true;
                    yield break;
                }
                if (timer < buffer)
                {
                    timer += Engine.DeltaTime;
                }
                else if (Input.MoveX != 0 || Input.MoveY != 0)
                {
                    int xmove = Input.MoveX.Value;
                    int ymove = Input.MoveY.Value * PerLine;
                    if (PianoModule.Session.CollectedDisks.Count <= PerLine) ymove = 0;
                    Selected = (Selected + xmove + ymove) % PianoModule.Session.CollectedDisks.Count;
                    if(Selected < 0)
                    {
                        Selected = PianoModule.Session.CollectedDisks.Count - 1;
                    }
                    timer = 0;
                }
                yield return null;
            }
            HasChosen = true;
            SelectedDisk = PianoModule.Session.CollectedDisks[Selected];
            yield return LeaveRoutine(Ease.SineIn);
            InRoutine = false;
        }
        private void DrawDisks()
        {
            List<FloppyDisk> disks = PianoModule.Session.CollectedDisks;
            if (disks.Count == 0)
            {
                return;
            }
            int countX = (int)Calc.Min(disks.Count, PerLine);
            int countY = 0;
            int count = disks.Count;
            while (count >= PerLine)
            {
                count -= PerLine;
                countY++;
            }
            count = disks.Count;
            float width = disks[0].Display.Width / 6;
            float height = disks[0].Display.Height / 6;
            float totalWidth = countX * width;
            float totalHeight = (countY - 1) * height;
            Vector2 pos = new Vector2(160, 90) - new Vector2(totalWidth / 2, totalHeight / 2);
            for (int i = 0; i < count; i++)
            {
                int x = i % PerLine;
                int y = i / PerLine;
                Vector2 destination = pos + new Vector2(x * width + (x - 1) * XSpacing, y * height + (y - 1) * YSpacing);
                bool isSelectedDisk = disks[i] == SelectedDisk;
                disks[i].Display.RenderPosition = (destination + Vector2.UnitY * (isSelectedDisk && HasChosen ? selectedYOffset : yOffset + (yOffset * i * 0.1f))) * 6;
                disks[i].Display.Rotation = isSelectedDisk && HasChosen ? selectedRotation : rotation + (rotation * i * 0.1f);
                if (i == Selected && (InControl || HasChosen))
                {
                    disks[i].Display.DrawOutline(Color.Black, 15);
                }
                disks[i].Display.Render();
            }
        }
    }
}