using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities
{
    [CustomEntity("PuzzleIslandHelper/GrassMaze")]
    [Tracked]
    public class GrassMaze : Entity
    {
        private Image Texture;
        public const int BaseDepth = -13000;
        public static bool Completed
        {
            get
            {
                return PianoModule.Session.GrassMazeCompleted;
            }
            set
            {
                PianoModule.Session.GrassMazeCompleted = value;
            }
        }
        public static bool Reset;
        private CustomTalkComponent Talk;
        private GrassMazeOverlay Maze;
        public GrassMaze(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            PianoModule.Session.GrassMazeFinished = false;
            Reset = false;
            Add(Texture = new Image(GFX.Game["objects/PuzzleIslandHelper/grassMaze/mazeMachine"]));
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Depth = 2;
            Add(Talk = new DotX3(0, 0, Width, Height, Vector2.UnitX * Width / 2, Interact));
            Position += Vector2.UnitY * 8;
        }
        private void Interact(Player player)
        {
            Completed = false;
            Reset = false;
            player.StateMachine.State = Player.StDummy;
            SceneAs<Level>().Add(Maze = new GrassMazeOverlay(SceneAs<Level>().Camera.Position));
        }
        public override void Update()
        {
            base.Update();
            if (Maze is null || Scene is not Level level || level.GetPlayer() is not Player player) return;
            if (Maze.Completed)
            {
                player.StateMachine.State = Player.StNormal;
            }
        }
    }
}