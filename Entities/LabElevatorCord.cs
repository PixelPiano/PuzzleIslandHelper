using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabElevatorCord")]
    [Tracked]
    public class LabElevatorCord : Entity
    {
        public int Direction = 1;
        public int Offset;
        public int Rate = 1;
        public int FrontOffset;
        public int BackOffset;
        public Image FrontNode;
        public Image BackNode;
        public bool UsesNodes;
        public List<Sprite> Fronts = [], Backs = [];
        public MTexture[] Frames;
        
        public LabElevatorCord(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            
            Direction = Math.Sign(data.Int("yDirection", 1));
            Rate = data.Int("rate");
            Offset = data.Int("offset");
            Collider = new Hitbox(8, data.Height);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Frames = GFX.Game.GetAtlasSubtextures("objects/PuzzleIslandHelper/labElevator/cords").ToArray();
        }
        public void Advance(int offset)
        {
            Offset += offset;
            if (FrontNode.Visible)
            {

            }
            FrontOffset += offset;
            BackOffset += offset;
        }

    }
}
