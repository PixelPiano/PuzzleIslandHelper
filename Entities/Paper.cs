using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Paper")]
    public class Paper : Entity
    {
        private Image image;
        private string path;
        private string DialogID;
        private DotX3 Talk;
        private bool UsesTexture;
        private FlagList VisibleFlag;
        private FlagList InteractableFlag;
        public Paper(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            VisibleFlag = data.FlagList("visibleFlag");
            InteractableFlag = data.FlagList("interactableFlag");
            UsesTexture = data.Bool("usesTexture", true);
            Depth = 9000;
            path = data.Attr("texturePath", "objects/PuzzleIslandHelper/noteSprites/paperA");
            image = new Image(GFX.Game[path]);
            image.Position = data.Nodes[0] - data.Position;
            if (UsesTexture)
            {
                Add(image);
            }
            DialogID = data.Attr("dialogID", "TestDialogue");
            Collider = new Hitbox(data.Width, data.Height);
            Add(Talk = new DotX3(Collider, Interact));
            Talk.SpriteOffset = Vector2.UnitX;
            Talk.VisibleFromDistance = true;
            Talk.AlphaAtDistance = 0.5f;

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Talk.Enabled = InteractableFlag;
            Visible = VisibleFlag;
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = InteractableFlag;
            Visible = VisibleFlag;
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
                if (level.GetPlayer() is not Player player) return;
                player.StateMachine.State = Player.StNormal;
            }
        }
    }
}