using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP.PianoEntities
{
    [CustomEntity("PuzzleIslandHelper/Piano")]
    [Tracked]
    public class Piano : Entity
    {
        public Sprite PianoSprite;
        public static readonly string[] Scale = { "a", "a#", "b", "c", "c#", "d", "d#", "e", "f", "f#", "g", "g#" };
        public TalkComponent Talk;
        public PianoContent Content;
        public Piano(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            PianoSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/organ/");
            PianoSprite.AddLoop("idle", "chomp", 0.1f, 2);
            PianoSprite.AddLoop("chomp", "chomp", 0.1f);
            Add(PianoSprite);
            PianoSprite.Play("idle");
            Collider = new Hitbox(PianoSprite.Width, PianoSprite.Height);
            Add(Talk = new TalkComponent(new Rectangle(0, 0, (int)Collider.Width, (int)Collider.Height), Collider.HalfSize, Interact));
        }
        private void Interact(Player player)
        {
            if (Scene is not Level level)
            {
                return;
            }
            Content = new PianoContent();
            level.Add(Content);
            player.StateMachine.State = Player.StDummy;

        }
        private void Dispose(Scene scene)
        {
            if (Content is not null)
            {
                scene.Remove(Content);
            }
            Content = null;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose(scene);
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose(scene);
        }
    }
}