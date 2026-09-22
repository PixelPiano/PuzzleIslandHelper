using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities.FrequencyData;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    [CustomEntity("PuzzleIslandHelper/MazeFrequencyController")]
    [Tracked]
    public class MazeFrequencyController : Entity
    {
        public float Frequency;
        public bool WarpToCenterCopy;
        public MazeFrequencyController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            Frequency = data.Float("frequency");
            WarpToCenterCopy = data.Bool("warpToCenterClone");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            if (level.Session.GetFlag("MazeComplete")) //if code complete, no need to stick around
            {
                RemoveSelf();
                return;
            }
            if (level.Session.GetSlider("MazeFrequencyNext") == Frequency) //if correct input
            {
                level.Session.SetFlag("MazeFrequencyAdvance"); //tell sender to advance
            }
            if (!level.Session.GetFlag("MazeComplete")) //if code incomplete
            {
                level.Session.SetFlag("WarpToMazeCenter"); //warp back to center
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            (scene as Level).Session.SetFlag("WarpToMazeCenter", false); //reset flag
        }
    }
    [CustomEntity("PuzzleIslandHelper/MazeFrequencySender")]
    [Tracked]
    public class MazeFrequencySender : Entity
    {
        public List<float> Code = [];
        private FrequencyMod mod;
        private EntityID id;
        public MazeFrequencySender(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            string code = data.Attr("code");
            foreach (char c in code)
            {
                switch (char.ToLower(c))
                {
                    case 'l':
                        Code.Add(24);
                        break;
                    case 'r':
                        Code.Add(72);
                        break;
                    case 'u':
                        Code.Add(0);
                        break;
                    case 'd':
                        Code.Add(48);
                        break;
                }
            }

            Add(mod = new FrequencyMod((f, i) =>
            {
                return SceneAs<Level>().Session.GetSlider("MazeFrequencyNext");
            }));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            int counter = level.Session.GetCounter("MazeFrequencyStep"); //current step in the code
            if (level.Session.GetFlag("MazeFrequencyAdvance")) //if the last input was correct
            {
                level.Session.SetFlag("MazeFrequencyAdvance", false);
                if (counter + 1 >= Code.Count) //check if the code is complete
                {
                    level.Session.SetFlag("MazeComplete");
                    level.Session.DoNotLoad.Add(id);
                    RemoveSelf();
                }
                else //advance to the next entry in the code
                {
                    level.Session.IncrementCounter("MazeFrequencyStep");
                    counter++;
                }
            }
            else //if the last input was incorrect
            {
                level.Session.SetCounter("MazeFrequencyStep", 0);
                counter = 0;
            }
            if (counter >= 0 && counter < Code.Count) //store the next correct input
            {
                level.Session.SetSlider("MazeFrequencyNext", Code[counter]);
            }
        }
    }
}