using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/StoneDoor")]
    [Tracked]
    public class StoneDoor : Entity
    {
        private class Void : Entity
        {
            public Image Image;
            public float Alpha = 0;
            public Void(Vector2 position, string path) : base(position)
            {
                Add(Image = new Image(GFX.Game[path + "void"]));
                Collider = Image.Collider();
                Depth = -1;
                Image.Color = Color.Transparent;
            }
            public override void Update()
            {
                base.Update();
                Image.Color = Color.White * Alpha;
            }
        }
        public Image Base;
        public Image Spots;
        public Image SpotsOutline;
        public Image BaseOutline;
        public Image VoidImage;
        private Void theVoid;
        public TalkComponent Talk;
        public DotX3 talk;
        public StoneDoor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            string path = "objects/PuzzleIslandHelper/stoneDoor/";
            Add(
                Base = new Image(GFX.Game[path + "base"]) { Color = data.HexColor("base") },
                BaseOutline = new Image(GFX.Game[path + "outline"]) { Color = data.HexColor("indent") },
                Spots = new Image(GFX.Game[path + "patches"]) { Color = data.HexColor("spots") },
                SpotsOutline = new Image(GFX.Game[path + "patchOutline"]) { Color = data.HexColor("spotsOutline") },
                VoidImage = new Image(GFX.Game[path + "void"])
                );
            Vector2 colliderOffset = new Vector2(16, 12);
            Collider = new Hitbox(VoidImage.Width - 32, VoidImage.Height - 12, colliderOffset.X, colliderOffset.Y);
            theVoid = new Void(Position, path);
            Rectangle r = new Rectangle((int)colliderOffset.X, (int)colliderOffset.Y, (int)Width, (int)Height);
            Add(Talk = new TalkComponent(r, colliderOffset + Vector2.UnitX * Width / 2f, Interact));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(theVoid);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            theVoid.RemoveSelf();
        }
        private class doorCutscene : CutsceneEntity
        {
            public Void theVoid;
            public Player player;
            public doorCutscene(Void theVoid, Player player) : base(true, true)
            {
                this.theVoid = theVoid;
                this.player = player;
            }
            public void Interact(Player player)
            {
                player.DisableMovement();
                Add(new Coroutine(routine(player)));
            }
            private IEnumerator routine(Player player)
            {
                Add(new Coroutine(CameraTo(new Vector2(theVoid.CenterX - 160, Level.Camera.Y), 0.6f, Ease.CubeIn)));
                yield return player.DummyWalkTo(theVoid.CenterX);
                yield return 0.1f;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    theVoid.Alpha = Ease.SineIn(i);
                    yield return null;
                }
                yield return 0.4f;
                EndCutscene(Level);
            }
            public override void OnBegin(Level level)
            {
                player.DisableMovement();
                Add(new Coroutine(routine(player)));
            }

            public override void OnEnd(Level level)
            {
                level.CompleteArea(true, false, true);
            }
        }
        public void Interact(Player player)
        {
            Scene.Add(new doorCutscene(theVoid, player));
        }
    }
}