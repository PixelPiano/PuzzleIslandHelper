using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class DashCodeComponent : Component
    {
        private readonly string[] code;
        private bool gotCode;
        private List<string> currentInputs = new List<string>();
        private DashListener dashListener;
        private Action onCodeGet;
        private bool removeSelf;
        public DashCodeComponent(Action onCodeGet, bool removeSelfOnComplete, string code) :
    base(true, true)
        {
            this.code = code.Replace(" ", "").Split(',').Select(Convert.ToString).ToArray();
            this.onCodeGet = onCodeGet;
            removeSelf = removeSelfOnComplete;
        }
        public void DoAction()
        {
            onCodeGet?.Invoke();
            if (removeSelf)
            {
                RemoveSelf();
            }
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            entity.Add(dashListener = new DashListener());
            dashListener.OnDash = delegate (Vector2 dir)
            {
                string text = "";
                text = dir.Y < 0f ? "U" : dir.Y > 0f ? "D" : "";
                text += dir.X < 0f ? "L" : dir.X > 0f ? "R" : "";
                currentInputs.Add(text);

                if (!gotCode)
                {
                    if (currentInputs.Count > code.Length)
                    {
                        currentInputs.RemoveAt(0);
                    }
                    if (currentInputs.Count == code.Length)
                    {
                        bool isValid = true;
                        for (int i = 0; i < code.Length; i++)
                        {
                            if (!currentInputs[i].Equals(code[i]))
                            {
                                isValid = false;
                            }
                        }

                        if (isValid)
                        {
                            gotCode = true;
                            DoAction();
                        }

                    }
                }
            };
        }
    }
}