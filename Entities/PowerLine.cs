using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PowerLine")]
    [Tracked]
    public class PowerLine : Entity
    {
        private FlagData flagData;
        private bool prevFlagState;
        private Vector2[] nodes;
        private SnakeLine line;
        private float lineLength;
        private int lineSpeed;
        private Color lineColorA;
        private Color lineColorB;
        private float lineStartFade;
        private float lineEndFade;
        private Tween lineTween;
        private float stateChangeDuration = 1;
        private List<Image> images = [];
        private bool usesTextures;
        private string texturePath;
        public Model3D Model;
        private VirtualRenderTarget target;
        public PowerLine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            target = VirtualContent.CreateRenderTarget("stkjghfkdsgjhs", 320, 180);
            flagData = data.Flag("flag", "inverted");
            nodes = data.NodesWithPosition(offset - Position);
            Depth = data.Int("depth", 2);
            lineStartFade = data.Float("lineStartFade", 10);
            lineEndFade = data.Float("lineEndFade", 25);
            lineLength = data.Float("lineLength", 50);
            lineSpeed = data.Int("lineSpeed", 1);
            lineColorA = data.HexColor("lineColorA", Color.Green);
            lineColorB = data.HexColor("lineColorB", Color.Transparent);
            stateChangeDuration = data.Float("stateChangeDuration", 1);
            texturePath = data.Attr("texturePath");
            usesTextures = !string.IsNullOrEmpty(texturePath) && GFX.Game.Has(texturePath);
            removeRedundantNodes();
            /*            if (usesTextures)
                        {
                            createTextures();
                        }*/
            Add(line = new SnakeLine(Vector2.Zero, nodes, 0, lineLength, lineStartFade, lineEndFade, 0, lineColorA, lineColorB)
            {
                Alpha = 0,
                Active = false,
                Visible = false,
                WrapAround = true,
                Size = 3
            });
            AddTag(Tags.TransitionUpdate);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            target?.Dispose();
            target = null;
        }
        private void createTextures()
        {
            images = [.. SnakeTexture.Create(GFX.Game[texturePath], nodes)];
            Add([.. images]);
        }
        public override void Awake(Scene scene)
        {
            if (nodes.Length < 2)
            {
                RemoveSelf();
                return;
            }
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
                    Activate(stateChangeDuration <= Engine.DeltaTime);
                }
                else
                {
                    Deactivate(stateChangeDuration <= Engine.DeltaTime);
                }
            }
            prevFlagState = flagData.State;
        }

        public void SetFlagState(bool value)
        {
            flagData.State = value;
        }
        public void Activate(bool instant)
        {
            SetFlagState(true);
            line.Visible = true;
            line.Active = true;
            lineTween?.RemoveSelf();
            if (instant)
            {
                line.Alpha = 1;
                line.Speed = lineSpeed;
                line.LineLength = lineLength;
            }
            else
            {
                float alphaFrom = line.Alpha;
                float speedFrom = line.Speed;
                float lengthFrom = line.LineLength;
                lineTween = Tween.Set(this, Tween.TweenMode.Oneshot, stateChangeDuration, Ease.SineIn, t =>
                {
                    line.Alpha = Calc.LerpClamp(alphaFrom, 1, t.Eased);
                    line.Speed = Calc.LerpClamp(speedFrom, lineSpeed, t.Eased);
                    line.LineLength = Calc.LerpClamp(lengthFrom, lineLength, t.Eased);
                });
            }
        }
        public void Deactivate(bool instant)
        {
            SetFlagState(false);
            lineTween?.RemoveSelf();
            if (instant)
            {
                line.Visible = line.Active = false;
                line.Alpha = 0;
                line.Speed = 0;
                line.LineLength = 0;
            }
            else
            {
                float alphaFrom = line.Alpha;
                float speedFrom = line.Speed;
                float lengthFrom = line.LineLength;
                lineTween = Tween.Set(this, Tween.TweenMode.Oneshot, stateChangeDuration, Ease.SineInOut, t =>
                {
                    line.Alpha = Calc.LerpClamp(alphaFrom, 0, t.Eased);
                    line.Speed = Calc.LerpClamp(speedFrom, 0, t.Eased);
                    line.LineLength = Calc.LerpClamp(lengthFrom, 0, t.Eased);
                }, t =>
                {
                    line.Visible = line.Active = false;
                    line.Alpha = 0;
                    line.Speed = 0;
                    line.LineLength = 0;
                });
            }
        }
        private void removeRedundantNodes()
        {
            List<Vector2> list = new List<Vector2>();
            Vector2 vector = Vector2.Zero;
            Vector2 vector2 = Vector2.Zero;
            bool flag = false;
            Vector2[] array = nodes;
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

            list.Add(nodes.Last());
            nodes = [.. list];
        }
    }
}
