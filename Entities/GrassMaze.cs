using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities
{
    [CustomEntity("PuzzleIslandHelper/GrassMaze")]
    [Tracked]
    public class GrassMaze : Entity
    {
        private Image Texture;
        public const int BaseDepth = -13000;
        public static bool Completed;
        public static bool Reset;
        private CustomTalkComponent Talk;
        public GrassMaze(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            PianoModule.Session.GrassMazeFinished = false;
            Reset = false;
            Add(Texture = new Image(GFX.Game["objects/PuzzleIslandHelper/grassMaze/machine"]));
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
            //SceneAs<Level>().Add(new LGPOverlay(SceneAs<Level>().Camera.Position));
            Add(new Coroutine(WaitForCompleteOrRemoved(player)));
        }
        private IEnumerator WaitForCompleteOrRemoved(Player player)
        {
            while (!Completed && !Reset)
            {
                yield return null;
            }
            player.StateMachine.State = Player.StNormal;
        }
    }
}