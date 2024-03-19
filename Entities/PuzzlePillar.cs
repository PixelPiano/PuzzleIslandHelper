using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using System.Collections;
using System.Data;
using System.Runtime.InteropServices.ComTypes;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PuzzlePillar")]
    [Tracked]
    public class PuzzlePillar : ExitBlock
    {
        private string spriteType;
        public bool forceChange = false;
        public bool forceState = false;
        private Sprite sprite;
        private Entity entity;

        public PuzzlePillar(EntityData data, Vector2 offset) : base(data, offset)
        {
            OnDashCollide = OnDash;
            entity = new Entity(Position);
            entity.Depth = Depth - 1;
            spriteType = data.Attr("Texture");
            entity.Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/puzzlePillar/"));
            sprite.AddLoop("idle", "crack" + spriteType, 0.1f);
            sprite.Position.Y += 8;
        }
        private DashCollisionResults OnDash(Player player, Vector2 direction)
        {
            sprite.Play("idle");
            if (!PianoModule.Session.BrokenPillars.Contains(spriteType))
            {
                PianoModule.Session.BrokenPillars.Add(spriteType);
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }

        // In regular C# code we can't just call the parent's base method...
        // but with MonoMod magic we can do it anyway.
        [MonoModLinkTo("Celeste.Solid", "System.Void Update()")]
        public void base_Update()
        {
            base.Update();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(entity);
            if (PianoModule.Session.BrokenPillars.Count == 3)
            {
                RemoveSelf();
            }
            if (PianoModule.Session.BrokenPillars.Contains(spriteType))
            {
                sprite.Play("idle");
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            // get some variables from the parent class.
            DynData<ExitBlock> self = new DynData<ExitBlock>(this);
            tiles = self.Get<TileGrid>("newTiles");
            cutout = self.Get<EffectCutout>("newCutout");
            cutout.Alpha = 1;
        }

        public override void Update()
        {
            base_Update();
            if (PianoModule.Session.BrokenPillars.Count == 3)
            {
                RemoveSelf();
            }
        }
    }
}