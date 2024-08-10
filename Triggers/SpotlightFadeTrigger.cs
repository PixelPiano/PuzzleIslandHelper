
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/SpotlightFadeTrigger")]
    public class SpotlightFadeTrigger : Trigger
    {
        private Color colorFrom;
        private Color colorTo;
        private PositionModes positionMode;
        private float alphaFrom;
        private float startFadeFrom;
        private float endFadeFrom;
        private readonly float alphaTo;
        private readonly float startFadeTo;
        private readonly float endFadeTo;
        private readonly bool affectColor;

        private bool enteredOnce;
        public SpotlightFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            colorFrom = data.HexColor("colorFrom", Color.White);
            colorTo = data.HexColor("colorTo", Color.White);
            alphaFrom = data.Float("alphaFrom", 1f);
            alphaTo = data.Float("alphaTo", 1f);
            startFadeFrom = data.Int("startFadeFrom", 32);
            startFadeTo = data.Int("startFadeTo", 32);
            endFadeFrom = data.Int("endFadeFrom", 64);
            endFadeTo = data.Int("endFadeTo", 64);
            affectColor = data.Bool("affectColor");
            positionMode = data.Enum<PositionModes>("positionMode");
        }
        public void SetSpotlight(Player player, float amount)
        {
            VertexLight light = player.Light;
            if (affectColor) light.Color = Color.Lerp(colorFrom, colorTo, amount);
            light.Alpha = Calc.LerpClamp(alphaFrom, alphaTo, amount);
            light.StartRadius = Calc.LerpClamp(startFadeFrom, startFadeTo, amount);
            light.EndRadius = Calc.LerpClamp(endFadeFrom, endFadeTo, amount);
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            SetSpotlight(player, GetPositionLerp(player, positionMode));
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            SetSpotlight(player, GetPositionLerp(player, positionMode));
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (enteredOnce) return;
            if (endFadeFrom < 0) endFadeFrom = player.Light.EndRadius;
            if (startFadeFrom < 0) startFadeFrom = player.Light.StartRadius;
            if (alphaFrom < 0) alphaFrom = player.Light.Alpha;
            enteredOnce = true;
        }
    }
}
