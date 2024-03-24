using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/PlaceholderSecret")]
    [Tracked]
    public class PlaceholderSecret : Entity
    {
        public DotX3 Talk;
        public Image Image;
        private EntityID id;
        public PlaceholderSecret(Vector2 position) : base(position)
        {
            Tag |= Tags.TransitionUpdate;
            Image = new Image(GFX.Game["objects/PuzzleIslandHelper/tempSecret/gem"]);
            Add(Image);

            Collider = new Hitbox(Image.Width, Image.Height);
            Add(Talk = new DotX3(0, 0, Width, Height, Vector2.UnitX * 4, Interact));

        }
        public override void Render()
        {
            Image.DrawSimpleOutline();
            base.Render();
        }
        private void Interact(Player player)
        {
            Add(new Coroutine(Collect(player)));
        }
        private IEnumerator Collect(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            yield return Engine.DeltaTime * 2;
            player.StateMachine.State = Player.StNormal;
            SceneAs<Level>().Session.DoNotLoad.Add(id);
            RemoveSelf();
        }
        public PlaceholderSecret(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset)
        {
            this.id = id;
        }
    }
}