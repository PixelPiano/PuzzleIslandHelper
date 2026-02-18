using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    [CustomEntity("PuzzleIslandHelper/FrequencyMonument")]
    [Tracked]
    public class FrequencyMonument : Entity
    {
        public Image Monument;
        public Image CenterImage;
        private float[] rates;
        private FlagList flagOnComplete;
        private FrequencyCodeComponent code;
        public FrequencyMonument(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Add(Monument = new Image(GFX.Game["objects/PuzzleIslandHelper/monument/base"]));
            Add(CenterImage = new Image(GFX.Game[data.Attr("centerTexture","objects/PuzzleIslandHelper/monument/defaultCenter")]));
            CenterImage.CenterOrigin();
            CenterImage.Position = Monument.HalfSize();
            Collider = Monument.Collider();
            rates = [data.Float("rateA"),data.Float("rateB"),data.Float("rateC"),data.Float("rateD")];
            flagOnComplete = data.FlagList("flagOnComplete");
            Add(code = new FrequencyCodeComponent(rates)
            {
                OnFullPower = () =>
                {
                    flagOnComplete.State = true;
                }
            });
        }
    }
}