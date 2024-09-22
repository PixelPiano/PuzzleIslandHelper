using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/PortalNode")]
    [Tracked]
    public class PortalNode : Entity
    {
        public string Flag;
        public Image Image;
        public DotX3 Talk;
        public PortalNode(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Flag = data.Attr("flag");
            Image = new Image(GFX.Game["objects/PuzzleIslandHelper/portalNode/texture"]);
            Add(Image);
            Collider = new Hitbox(Image.Width, Image.Height);
            Add(Talk = new DotX3(Collider, (Player player) => { player.Scene.Add(new PortalNodeCutscene(this)); }));

            Tag |= Tags.TransitionUpdate;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!CalidusCutscene.GetCutsceneFlag(scene, CalidusCutscene.Cutscenes.Second))
            {
                Talk.Enabled = false;
            }
        }
        public void Deactivate()
        {

        }
        public void Activate(bool instant)
        {

        }
        public IEnumerator Routine()
        {
            yield return null;
        }
        public class PortalNodeCutscene : CutsceneEntity
        {
            public PortalNode Node;
            public PortalNodeCutscene(PortalNode parent) : base()
            {
                Node = parent;
            }
            public override void OnBegin(Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    player.StateMachine.State = Player.StDummy;
                }
                Add(new Coroutine(TurnOn()));
            }
            private IEnumerator TurnOn()
            {
                yield return Node.Routine();
            }
            public override void OnEnd(Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    player.StateMachine.State = Player.StNormal;
                }
            }
        }
    }
}