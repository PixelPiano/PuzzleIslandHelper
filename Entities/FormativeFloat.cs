using Celeste.Mod.Core;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FestivalFloat")]
    [Tracked]
    public class FestivalFloat : JumpthruPlatform
    {
        public float CubeWidth = 40;
        public float CubeHeight = 40;
        public Color CubeColor;
        public float CubeHoverHeight = 24;
        private float additionalHover;
        public float HoverMult = 1;

        public int WigglerMult = 1;
        public Wiggler Wiggler;

        public enum States
        {
            Empty,
            Jaques,
            Randy,
            Destroyed
        }
        public States State;
        public FestivalFloat(EntityData data, Vector2 offset) : base(data.Position + offset, 80, "default")
        {
            Depth = 2;
            Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 1, true);
            tween.OnUpdate = t => { additionalHover = t.Eased * 4; };
            Wiggler = Wiggler.Create(0.7f, 2f, null, true, false);
            Add(tween, Wiggler);
        }
        public IEnumerator RollTo(float x, float duration)
        {
            float pos = Position.X;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                Position.X = Calc.LerpClamp(pos, x, i);
                yield return null;
            }
        }
        public void CubeGrow()
        {
            Wiggler.StopAndClear();
            Wiggler.Start();
            WigglerMult = 1;
            CubeWidth += 4;
            CubeHeight += 4;
        }
        public void CubeShrink()
        {
            Wiggler.StopAndClear();
            Wiggler.Start();
            WigglerMult = -1;
            CubeWidth = Calc.Max(CubeWidth - 4, 4);
            CubeHeight = Calc.Max(CubeHeight - 4, 4);
        }
        public override void Awake(Scene scene)
        {
            Position = this.Snapped<Solid>(Vector2.UnitY, collider: new Hitbox(Width, 8));
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Position, Width, 8, Color.Lime);
            switch (State)
            {
                case States.Empty:
                    break;
                case States.Jaques:
                    float wiggle = Wiggler.Value * WigglerMult * 4;
                    float cubeWidth = CubeWidth + wiggle;
                    float cubeHeight = CubeHeight + wiggle;
                    Draw.Rect(TopCenter - new Vector2(cubeWidth / 2, (CubeHoverHeight + cubeHeight + additionalHover) * HoverMult), cubeWidth, cubeHeight, Color.White);
                    break;
                case States.Randy:
                    break;
                case States.Destroyed:
                    break;
            }
        }
    }
}