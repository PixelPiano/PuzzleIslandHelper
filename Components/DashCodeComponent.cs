using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    public class RuleCodeComponent : Component
    {
        public class DirectionMap
        {
            public static DirectionMap Default = new DirectionMap(0, 1, 2, 3, 4, 5, 6, 7);
            public static DirectionMap ReverseDefault = new DirectionMap(0, 7, 6, 5, 4, 3, 2, 1);
            public Dictionary<string, int> map = [];
            public DirectionMap(int up, int upRight, int right, int downRight, int down, int downLeft, int left, int upLeft)
            {
                map = new()
                {
                    {"U",up},
                    {"UR",upRight },
                    {"R",right },
                    {"DR",downRight},
                    {"D",down },
                    {"DL",downLeft },
                    {"L",left },
                    {"UL",upLeft }
                };
            }
        }
        public DirectionMap Map = DirectionMap.Default;
        public bool removeOnComplete;
        public RuleCodeComponent(DirectionMap map, bool removeSelfOnComplete) : base(true, true)
        {
            Map = map;
            removeOnComplete = removeSelfOnComplete;
        }
        public RuleCodeComponent(bool removeOnComplete) : this(DirectionMap.Default, removeOnComplete)
        {

        }
    }
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