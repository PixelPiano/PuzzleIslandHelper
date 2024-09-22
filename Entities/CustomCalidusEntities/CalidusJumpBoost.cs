using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities
{

    [CustomEntity("PuzzleIslandHelper/CalidusJumpBoost")]
    [Tracked]
    public class CalidusJumpBoost : Entity
    {
        public EntityID ID;
        public CalidusJumpBoost(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Image i;
            Add(i = new Image(GFX.Game["characters/PuzzleIslandHelper/Calidus/jumpboost"]));
            Collider = new Hitbox(i.Width, i.Height);
            Add(new DotX3(Collider, Interact));
            ID = id;

        }
        public void Interact(Player player)
        {
            if (player is PlayerCalidus pc)
            {
                Scene.Add(new JumpBoostCollect(pc));
                pc.Boosts++;
                (Scene as Level).Session.DoNotLoad.Add(ID);
                RemoveSelf();
            }
        }
    }
    public class JumpBoostCollect : CutsceneEntity
    {
        public PlayerCalidus Player;
        public int prevState;
        public JumpBoostCollect(PlayerCalidus player) : base(false)
        {
            Player = player;
        }

        public override void OnBegin(Level level)
        {
            prevState = Player.State;
            Player.State = PlayerCalidus.DummyState;
            Add(new Coroutine(cutscene()));
        }
        private IEnumerator cutscene()
        {
            yield return Textbox.Say("you got a star :)");
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            Player.State = prevState;
        }
    }
}