using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/GrassMaze")]
    [Tracked]
    public class GrassMaze : Entity
    {
        private Image Texture;
        public const int BaseDepth = -13000;
        public bool Completed
        {
            get
            {
                return  !string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag);
            }
            set
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    SceneAs<Level>().Session.SetFlag(flag, value);
                }
            }
        }
        public static bool Reset;
        public CustomTalkComponent Talk;
        private string flag;
        public GrassMaze(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            PianoModule.Session.GrassMazeFinished = false;
            Reset = false;
            Add(Texture = new Image(GFX.Game["objects/PuzzleIslandHelper/grassMaze/mazeMachine"]));
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Depth = 2;
            Add(Talk = new DotX3(0, 0, Width, Height, Vector2.UnitX * Width / 2, Interact));
            Talk.PlayerMustBeFacing = false;
            Position += Vector2.UnitY * 10;
            flag = data.Attr("flagOnComplete");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Completed)
            {
                Talk.Enabled = false;
            }
        }
        private void Interact(Player player)
        {
            Completed = false;
            Reset = false;
            player.StateMachine.State = Player.StDummy;
            SceneAs<Level>().Add(new GrassMazeOverlay(this, SceneAs<Level>().Camera.Position, flag));
        }
    }
}