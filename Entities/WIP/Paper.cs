using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/Paper")]
    public class Paper : Entity
    {
        private Image image;
        private string path;
        private string DialogID;
        public Paper(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Depth = 1;
            path = data.Attr("texturePath", "objects/PuzzleIslandHelper/noteSprites/paperA");
            image = new Image(GFX.Game[path]);
            image.Position = data.Nodes[0] - data.Position;
            Add(image);
            DialogID = data.Attr("dialogID", "TestDialogue");
            Collider = new Hitbox(data.Width, data.Height);
            Add(new DotX3(Collider, Interact));
        }
        private void Interact(Player player)
        {
            Scene.Add(new PaperDialogue(DialogID));
        }
        public class PaperDialogue : CutsceneEntity
        {
            public string DialogID;
            public PaperDialogue(string dialogID) : base()
            {
                DialogID = dialogID;
            }
            public override void OnBegin(Level level)
            {
                if (level.GetPlayer() is not Player player) return;
                player.StateMachine.State = Player.StDummy;
                Add(new Coroutine(Cutscene()));
            }
            private IEnumerator Cutscene()
            {
                yield return Textbox.Say(DialogID);
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                if(level.GetPlayer() is not Player player) return;
                player.StateMachine.State = Player.StNormal;
            }
        }
    }
}