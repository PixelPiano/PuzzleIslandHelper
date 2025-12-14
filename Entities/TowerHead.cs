using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TowerHead")]
    [Tracked]
    public class TowerHead : Entity
    {
        public class Inside : Entity
        {
            public TowerHead Tower;
            public Image Image;
            public Inside(TowerHead tower, int depth) : base(tower.Position)
            {
                Tower = tower;
                Depth = depth;
                Image = new Image(GFX.Game["objects/PuzzleIslandHelper/towerHead/inside"]);
                Add(Image);
            }
        }
        public class Outside : Entity
        {
            public class OutsideLadder : Ladder
            {
                public Outside Parent;
                public OutsideLadder(Outside parent) : base(parent.Position + Vector2.UnitX * 40, (int)parent.Height, parent.Tower.LadderPath, true, parent.Depth - 1, default, default)
                {
                    Parent = parent;
                }
                public override void Update()
                {
                    base.Update();
                    Depth = Parent.Depth - 1;
                    Alpha = Parent.Alpha;
                }
            }
            public OutsideLadder Ladder;
            public TowerHead Tower;
            public Image Image;
            public int OutsideDepth, InsideDepth;
            public float Alpha = 1;
            public Outside(TowerHead tower, int depthWhenOutside, int depthWhenInside) : base(tower.Position)
            {
                Tower = tower;
                Depth = OutsideDepth = depthWhenOutside;
                InsideDepth = depthWhenInside;
                Image = new Image(GFX.Game["objects/PuzzleIslandHelper/towerHead/outside"]);
                Add(Image);
                Collider = Image.Collider();
                Ladder = new(this);
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                scene.Add(Ladder);
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Ladder?.RemoveSelf();
            }
            public override void Update()
            {
                base.Update();
                Depth = Tower.PlayerInside ? InsideDepth : OutsideDepth;
                Alpha = Calc.Approach(Alpha, Tower.PlayerInside ? 0.2f : 1, Engine.DeltaTime);
                Image.SetColor(Color.White * Alpha);
            }
        }
        public Inside InsideEntity;
        public Outside OutsideEntity;
        public bool PlayerInside;
        public string FlagWhenInside = "";
        public string LadderPath = "objects/PuzzleIslandHelper/ladder";
        public TowerHead(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/towerHead/outside"]);
            Collider = image.Collider();
            FlagWhenInside = data.Attr("flagWhenInside");
            InsideEntity = new Inside(this, data.Int("insideDepth"));
            OutsideEntity = new Outside(this, data.Int("outsideDepth"), -1);
        }
        public override void Update()
        {
            base.Update();
            if (!string.IsNullOrEmpty(FlagWhenInside))
            {
                FlagWhenInside.SetFlag(PlayerInside);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(InsideEntity, OutsideEntity);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            InsideEntity?.RemoveSelf();
            OutsideEntity?.RemoveSelf();
        }
    }
}