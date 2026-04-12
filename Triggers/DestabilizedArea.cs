using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.UI;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    //WIP
    [CustomEntity("PuzzleIslandHelper/DestabilizedArea")]
    [Tracked]
    public class DestabilizedArea : Trigger
    {
        public float Distortion;
        public string StabilizerGroupID;
        public FlagData Flag;
        public bool IsDistorted => !Flag;
        private bool wasDistorted;
        private Tween tween;
        public DestabilizedArea(EntityData data, Vector2 offset) : base(data, offset)
        {
            StabilizerGroupID = data.Attr("stabilizerGroupID");
            Flag = new FlagData("DestabilizedArea:" + StabilizerGroupID);
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (IsDistorted && !player.Dead)
            {
                player.Die(Vector2.Zero);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Distortion = IsDistorted ? 0 : 1;
            wasDistorted = IsDistorted;
        }
        public override void Update()
        {
            base.Update();
            bool distorted = IsDistorted;
            if (distorted != wasDistorted)
            {
                tween?.RemoveSelf();
                float from = Distortion;
                float target = distorted ? 1 : 0;
                tween = Tween.Set(this, Tween.TweenMode.Oneshot, 0.5f, Ease.CubeOut, t =>
                {
                    Distortion = Calc.LerpClamp(from, target, t.Eased);
                }, t =>
                {
                    Distortion = target;
                });
            }
            wasDistorted = distorted;
        }
    }
}
