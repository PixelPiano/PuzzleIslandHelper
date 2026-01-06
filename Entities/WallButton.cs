using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using FrostHelper.ModIntegration;
using Celeste.Mod.Entities;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities //Replace with your mod's namespace
{
    [CustomEntity("PuzzleIslandHelper/WallButton")]
    [Tracked]
    public class WallButton : Entity
    {
        [Command("turn_on_wall_buttons", "turns on every WallButton in the current scene")]
        public static void ActivateAllWallButtons()
        {
            if (Engine.Scene is Level level)
            {
                foreach(WallButton button in level.Tracker.GetEntities<WallButton>())
                {
                    button.Flag.State = true;
                    button.Activate();
                }
            }
            else
            {
                Engine.Commands.Log("Current scene is not a level", Color.Yellow);
            }
        }
        public FlagData Flag;
        public Image Image;
        public bool Persistent;
        private bool added;
        public WallButton(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 10;
            Flag = data.Flag("flag");
            Persistent = data.Bool("persistent");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Image = new Image(GFX.Game["objects/PuzzleIslandHelper/wallbutton00"]);
            if (Flag)
            {
                Image.Color = Color.Lime;
                added = true;
            }
            else
            {
                Image.Color = Color.Red;
            }
            Add(Image);
            Collider = Image.Collider();
            Add(new PlayerCollider(OnPlayer));
        }
        private IEnumerator colorLerp()
        {
            added = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.2f)
            {
                Image.Color = Color.Lerp(Color.Red, Color.White, Ease.SineIn(i));
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                Image.Color = Color.Lerp(Color.White, Color.Lime, Ease.SineInOut(i));
                yield return null;
            }
            Image.Color = Color.Lime;
        }
        public override void Update()
        {
            base.Update();
            if (!added && Flag)
            {
                Activate();
            }

        }
        public void Activate()
        {
            Add(new Coroutine(colorLerp()));
            Pulse.Circle(this, Pulse.Fade.InAndOut, Pulse.Mode.Oneshot, Collider.HalfSize, 0, Width * 2, 0.4f, true, Color.Red, Color.Lime, Ease.CubeIn, Ease.CubeOut);
        }
        private void OnPlayer(Player p)
        {
            if (!added)
            {
                Flag.State = true;
                Activate();
            }
        }
    }

}