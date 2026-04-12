using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TAS;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    [CustomEntity("PuzzleIslandHelper/FrequencyMonument")]
    [Tracked]
    public class FrequencyMonument : Entity
    {
        public string MonumentComponentID;
        public Image Monument;
        public Image CenterImage;
        private float[] rates;
        public FlagList CompletedFlag;
        private GlobalFrequencyReceiver code;
        public FrequencyMonument(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            MonumentComponentID = data.Attr("componentID");
            Depth = 1;
            Add(Monument = new Image(GFX.Game["objects/PuzzleIslandHelper/monument/base"]));
            Add(CenterImage = new Image(GFX.Game[data.Attr("centerTexture", "objects/PuzzleIslandHelper/monument/defaultCenter")]));
            CenterImage.CenterOrigin();
            CenterImage.Position = Monument.HalfSize();
            Collider = Monument.Collider();
            rates = [data.Float("rateA"), data.Float("rateB"), data.Float("rateC"), data.Float("rateD")];
            CompletedFlag = data.FlagList("flagOnComplete");
            Add(code = new GlobalFrequencyReceiver(rates)
            {
                OnFullPower = () =>
                {
                    if (!CompletedFlag)
                    {
                        if (ForkAmpUI.UIActive)
                        {
                            ForkAmpUI.ForceOff = true;
                            ForkAmpUI.EndingSequence = MonumentManager.FadeInComponents(this, SceneAs<Level>().Tracker.GetEntity<MonumentManager>());
                            ForkAmpUI.OnEndCallback = () =>
                            {
                                SceneAs<Level>().ResetZoom();
                                MonumentManager.EnableAll(Scene, this);
                                ForkAmpUI.OnEndCallback = null;
                                ForkAmpUI.EndingSequence = null;
                            };
                        }
                        else
                        {
                            MonumentManager.EnableAll(Scene, this);
                            ForkAmpUI.OnEndCallback = null;
                            ForkAmpUI.EndingSequence = null;
                        }
                    }
                }
            });
        }
    }
}