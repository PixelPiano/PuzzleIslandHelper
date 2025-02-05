using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SecurityCam")]
    [Tracked]
    public class SecurityCam : Entity
    {
        private string path = "objects/PuzzleIslandHelper/camera/";
        public string Name;
        public Sprite Camera;

        public SecurityCam(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -10001;
            Name = data.Attr("name");
            Camera = new Sprite(GFX.Game, path);
            Camera.AddLoop("off", "simpleCam", 0.1f, 0);
            Camera.AddLoop("on", "simpleCam", 1);
            Camera.Play("off");
            Add(Camera);
            Collider = Camera.Collider();
        }
        public override void Update()
        {
            base.Update();
            if (PianoModule.Session.RestoredPower)
            {
                Camera.Play("on");
            }
            else
            {
                Camera.Play("off");
            }
        }
    }
}