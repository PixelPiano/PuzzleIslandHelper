using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FloppyDisk")]
    [Tracked]
    public class FloppyDisk : Entity
    {
        public string Preset;
        public bool Collected
        {
            get
            {
                return PianoModule.Session.CollectedDisks.Contains(this);
            }
        }
        public DotX3 Talk;
        public Image Image;
        public Image Display;
        public FloppyDisk(Vector2 position, string preset, Color color) : base(position)
        {
            Tag |= Tags.TransitionUpdate;
            Preset = preset;
            Image = new Image(GFX.Game["objects/PuzzleIslandHelper/floppy/laying"]);
            Display = new Image(GFX.Game["objects/PuzzleIslandHelper/floppy/front"]);
            Add(Image, Display);
            Display.Visible = false;
            Display.CenterOrigin();
            Collider = new Hitbox(Image.Width, Image.Height);
            Add(Talk = new DotX3(0, 0, Width, Height, Vector2.UnitX * 4, Interact));
            color = Color.Lerp(color, Color.White, 0.2f);
            Image.Color = color;
            Display.Color = color;
        }
        public override void Render()
        {
            Image.DrawSimpleOutline();
            base.Render();
        }
        private void Interact(Player player)
        {
            Add(new Coroutine(Collect(player)));
        }
        private IEnumerator Collect(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            yield return Engine.DeltaTime * 2;
            PianoModule.Session.TryAddDisk(this);
            player.StateMachine.State = Player.StNormal;
            RemoveSelf();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Collected)
            {
                RemoveSelf();
            }
        }
        public override void Update()
        {
            base.Update();
        }

        public FloppyDisk(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("preset"), data.HexColor("color"))
        {

        }
    }
}