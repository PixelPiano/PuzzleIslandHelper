using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/AscwiitNest")]
    [Tracked]
    public class AscwiitNest : Entity
    {
        public int FloorElevation;
        public int FloorShrink;
        public float ProtectRadius;
        public FlagList ProtectFlag;
        public FlagList FlagOnGift;
        public int TotalBabies;
        public Ascwiit[] Babies;
        public Image Nest;
        public JumpThru Platform;
        public bool colliding;
        public AscwiitNest(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -10;
            Add(Nest = new Image(GFX.Game["objects/PuzzleIslandHelper/birdNest"]));
            Collider = new Circle(ProtectRadius = data.Float("protectRadius", 16));
            ProtectFlag = data.FlagList("protectFlag");
            FlagOnGift = data.FlagList("flagOnGift");
            TotalBabies = data.Int("babies", 4);
            FloorElevation = data.Int("floorElevation", 2);
            FloorShrink = data.Int("floorShrink");
            Add(new PlayerCollider(OnPlayer));
            Add(new PostUpdateHook(() => colliding = false));
            Tag |= Tags.TransitionUpdate;
        }
        public void OnPlayer(Player player)
        {
            colliding = true;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Scene.Add(Platform = new JumpThru(Position + new Vector2(FloorShrink, Nest.Height - FloorElevation), (int)Nest.Width - FloorShrink * 2, true));
            Babies = new Ascwiit[TotalBabies];
            for (int i = 0; i < TotalBabies; i++)
            {
                Vector2 position = Platform.Position - Vector2.UnitY * 4 + Vector2.UnitX * Calc.Random.Range(0, Platform.Width - 4);
                Ascwiit bird = new Ascwiit(position, Ascwiit.StIdle, 0.5f);
                bird.IgnoreJumpThrus = false;
                bird.AvoidNoHopZones = true;
                bird.FleesFromPlayer = false;
                bird.IdleHops = true;
                Babies[i] = bird;
                scene.Add(bird);
            }
        }
        public override void Update()
        {
            base.Update();
            if (!colliding)
            {
                foreach (var b in Babies)
                {
                    b.Scared = false;
                }
            }
        }

    }
}
