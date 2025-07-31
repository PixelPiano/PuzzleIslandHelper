using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static MonoMod.InlineRT.MonoModRule;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PowerLine")]
    [Tracked]
    public class PowerLine : Entity
    {
        private FlagList flagData;
        private bool prevFlagState;
        public List<Vector2> Nodes;
        public SnakeLine Line;
        public float Length;
        public int Speed;
        public Color LineColorA
        {
            get => lineColorA;
            set
            {
                lineColorA = value;
                Line.ColorA = value;
            }
        }
        public Color LineColorB
        {
            get => lineColorB;
            set
            {
                lineColorB = value;
                Line.ColorB = value;
            }
        }
        public Color AltColorA
        {
            get => altA;
            set
            {
                altA = value;
                Line.AltColorA = value;
            }
        }
        public Color AltColorB
        {
            get => altB;
            set
            {
                altB = value;
                Line.AltColorB = value;
            }
        }
        private Color lineColorA;
        private Color lineColorB;
        private Color altA, altB;
        private enum flagModes
        {
            Hide,
            AlternateColor
        }
        private flagModes flagMode;
        private float lineStartFade;
        private float lineEndFade;
        private Tween lineTween;
        private float tweenDuration = 1;
        private bool connectEnds;
        private bool setAllFlagsOnStateChange;
        public float AlphaA = 1, AlphaB = 0;
        public bool Backwards;
        public bool ConnectEnds
        {
            get => connectEnds;
            set
            {
                if (Nodes != null)
                {
                    if (!connectEnds && value)
                    {
                        Nodes.Add(Vector2.Zero);
                    }
                    else if (connectEnds && !value && Nodes.Count > 1)
                    {
                        Nodes.RemoveAt(Nodes.Count - 1);
                    }
                }
                connectEnds = value;
            }
        }
        public float Offset;
        public PowerLine(Vector2 position, Vector2[] nodes, int depth = 2, string flag = "", bool invertFlag = false, float lineStartFade = 10, float lineEndFade = 25, float length = 50, int speed = 1,
            Color colorA = default, Color colorB = default, float alphaA = 1, float alphaB = 0, bool backwards = false, float offset = 0, bool connectEnds = false) : base(position)
        {
            flagData = new FlagList(flag, invertFlag);
            Nodes = [.. nodes];
            Depth = depth;
            this.lineStartFade = lineStartFade;
            this.lineEndFade = lineEndFade;
            Length = length;
            Speed = speed;
            lineColorA = colorA;
            lineColorB = colorB;
            AlphaA = alphaA;
            AlphaB = alphaB;
            Backwards = backwards;
            Offset = offset;
            Collider = new Hitbox(4, 4);
            Tag |= Tags.TransitionUpdate;
            removeRedundantNodes();
            ConnectEnds = connectEnds;
        }
        public PowerLine(EntityData data, Vector2 offset) :
            this(data.Position + offset, data.NodesWithPosition(offset - (data.Position + offset)), data.Int("depth", 2),
                data.Attr("flags"), data.Bool("invertFlag"), data.Float("startFade", 10), data.Float("endFade", 25),
                data.Float("length", 50), data.Int("speed", 1), data.HexColor("colorA"), data.HexColor("colorB"),
                data.Float("alphaA"), data.Float("alphaB"), data.Bool("backwards"), data.Float("offset"), data.Bool("connectEnds"))
        {
            flagMode = data.Enum<flagModes>("flagMode");
            setAllFlagsOnStateChange = data.Bool("changeFlags");
            altA = data.HexColor("altColorA");
            altB = data.HexColor("altColorB");
            tweenDuration = data.Float("tweenDuration", 1);
        }
        public void Reverse()
        {
            Nodes.Reverse();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public override void Awake(Scene scene)
        {
            if (Nodes.Count < 2)
            {
                RemoveSelf();
                return;
            }
            Add(Line = new SnakeLine(Vector2.Zero, Nodes, Offset, Length, lineStartFade, lineEndFade, 0, lineColorA, lineColorB, altA, altB)
            {
                WrapAround = true,
                Size = 3,
                Alpha = 1,
                ColorAAlpha = AlphaA,
                ColorBAlpha = AlphaB
            });
            prevFlagState = flagData.State;
            if (prevFlagState)
            {
                Activate(true);
            }
            else
            {
                Deactivate(true);
            }
            base.Awake(scene);
        }
        public override void Update()
        {
            base.Update();
            bool state = flagData.State;
            if (state != prevFlagState)
            {
                if (state)
                {
                    Activate(tweenDuration <= Engine.DeltaTime);
                }
                else
                {
                    Deactivate(tweenDuration <= Engine.DeltaTime);
                }
            }
            prevFlagState = state;
        }

        public void SetFlagState(bool value)
        {
            if (setAllFlagsOnStateChange)
            {
                flagData.State = value;
            }
        }
        public void Activate(bool instant)
        {
            SetFlagState(true);
            Line.Visible = true;
            Line.Active = true;
            lineTween?.RemoveSelf();
            if (instant)
            {
                Line.Alpha = 1;
                Line.Speed = Speed;
                Line.LineLength = Length;
                if (flagMode == flagModes.AlternateColor)
                {
                    Line.ColorLerp = 0;
                }
            }
            else
            {
                float alphaFrom = Line.Alpha;
                float speedFrom = Line.Speed;
                float lengthFrom = Line.LineLength;
                float colorLerpFrom = Line.ColorLerp;
                lineTween = Tween.Set(this, Tween.TweenMode.Oneshot, tweenDuration, Ease.SineIn, t =>
                {
                    Line.Alpha = Calc.LerpClamp(alphaFrom, 1, t.Eased);
                    Line.Speed = Calc.LerpClamp(speedFrom, Speed, t.Eased);
                    Line.LineLength = Calc.LerpClamp(lengthFrom, Length, t.Eased);
                    Line.ColorLerp = Calc.LerpClamp(colorLerpFrom, 0, t.Eased);
                });
            }
        }
        public void Deactivate(bool instant)
        {
            SetFlagState(false);
            lineTween?.RemoveSelf();
            if (instant)
            {
                switch (flagMode)
                {
                    case flagModes.Hide:
                        Line.Visible = Line.Active = false;
                        Line.Alpha = 0;
                        Line.LineLength = 0;
                        Line.Speed = 0;
                        break;
                    case flagModes.AlternateColor:
                        Line.ColorLerp = 0;
                        break;
                }


            }
            else
            {
                float alphaFrom = Line.Alpha;
                float speedFrom = Line.Speed;
                float lengthFrom = Line.LineLength;
                float colorLerp = Line.ColorLerp;
                lineTween = Tween.Set(this, Tween.TweenMode.Oneshot, tweenDuration, Ease.SineInOut, t =>
                {
                    if (flagMode == flagModes.Hide)
                    {
                        Line.Alpha = Calc.LerpClamp(alphaFrom, 0, t.Eased);
                        Line.Speed = Calc.LerpClamp(speedFrom, 0, t.Eased);
                        Line.LineLength = Calc.LerpClamp(lengthFrom, 0, t.Eased);
                    }
                    else
                    {
                        Line.ColorLerp = Calc.LerpClamp(colorLerp, 1, t.Eased);
                    }
                }, t =>
                {
                    if (flagMode == flagModes.Hide)
                    {
                        Line.Visible = Line.Active = false;
                        Line.Alpha = 0;
                        Line.Speed = 0;
                        Line.LineLength = 0;
                    }
                    else
                    {
                        Line.ColorLerp = 1;
                    }
                });
            }
        }
        private void removeRedundantNodes()
        {
            List<Vector2> list = new List<Vector2>();
            Vector2 vector = Vector2.Zero;
            Vector2 vector2 = Vector2.Zero;
            bool flag = false;
            List<Vector2> array = Nodes;
            foreach (Vector2 vector3 in array)
            {
                if (flag)
                {
                    Vector2 vector4 = (vector - vector3).SafeNormalize();
                    if ((double)Math.Abs(vector4.X - vector2.X) > 0.0005 || (double)Math.Abs(vector4.Y - vector2.Y) > 0.0005)
                    {
                        list.Add(vector);
                    }

                    vector2 = vector4;
                }

                flag = true;
                vector = vector3;
            }

            list.Add(Nodes.Last());
            Nodes = list;
        }
    }
}
