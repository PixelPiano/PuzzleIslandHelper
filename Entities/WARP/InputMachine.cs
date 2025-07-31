using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    [Tracked]
    public class InputMachine : Entity
    {
        public class MachineNode : GraphicsComponent
        {
            public InputMachine Parent => Entity as InputMachine;
            public int Index;
            public static readonly Vector2[] Offsets =
            {
                    new Vector2(4, 2), new Vector2(8, 2), new Vector2(12, 2),
                    new Vector2(2, 5), new Vector2(6, 5), new Vector2(10, 5), new Vector2(14, 5),
                    new Vector2(4, 8), new Vector2(8, 8), new Vector2(12, 8)
                };
            private Vector2 offset;
            public Rectangle Bounds;
            public bool Usable => WARPData.Inv.HasNode((WARPData.NodeTypes)Index);
            public MachineNode(int index, Rectangle bounds) : base(true)
            {
                Index = index;
                offset = bounds.Location.ToVector2() + Offsets[index];
            }
            public override void Update()
            {
                base.Update();
            }
            public override void Render()
            {
                base.Render();
                if (Usable)
                {
                    Draw.Point(RenderPosition + offset, Color.Red);
                }
            }
        }
        public static MTexture Texture => GFX.Game[WARPData.DefaultPath + "control"];
        public static MTexture FilledTex => GFX.Game[WARPData.DefaultPath + "controlFilled"];
        public Image Image;
        public Image Screen;
        public WarpCapsule Parent;
        public DotX3 Talk;
        public UI UI;
        public List<MachineNode> Nodes = [];
        public bool Blocked;
        public bool PlayerHasAnyNodes => WARPData.Inv.IsObtained.Any(item => item == true);
        public InputMachine(WarpCapsule parent, Vector2 position) : base(position)
        {
            Depth = 10;
            Parent = parent;
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Add(Screen = new Image(FilledTex, true));
            Add(Image = new Image(Texture, true));
            Talk = new DotX3(Collider, Interact);
            Add(Talk);
            Add(new BathroomStallComponent(null, Block, Unblock));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Rectangle bounds = new Rectangle(0, 0, 13, 7);
            for (int i = 0; i < 10; i++)
            {
                MachineNode n = new MachineNode(i, bounds);
                Nodes.Add(n);
                Add(n);
            }
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = Parent.Accessible && !Blocked && PlayerHasAnyNodes;
        }
        public override void Render()
        {
            Image.DrawSimpleOutline();
            base.Render();
        }
        public void Interact(Player player)
        {
            Add(new Coroutine(Sequence(player)));
        }
        public void Block()
        {
            Blocked = true;
        }
        public void Unblock()
        {
            Alarm.Set(this, 0.3f, delegate { Blocked = false; });
        }
        public IEnumerator Sequence(Player player)
        {
            player.DisableMovement();
            Scene.Add(UI = new UI(Parent));
            while (!UI.Finished)
            {
                yield return null;
            }
            if (Parent.UsesRune)
            {
                WARPData.ObtainedRunes.Add(Parent.OwnWarpRune);
            }
            player.EnableMovement();
        }
        /*public IEnumerator origCutscene(Player player)
        {
            Level level = Scene as Level;
            player.StateMachine.State = Player.StDummy;
            if (PianoModule.Session.TimesUsedCapsuleWarp < 1 && Marker.TryFind("isStartingWarpRoom", out _))
            {
                yield return Textbox.Say("capsuleWelcome", pressButton);
                player.StateMachine.State = Player.StNormal;
                yield break;
            }
            float width = 150;
            float height = 80;
            FakeTerminal t = new FakeTerminal(level.Camera.Position + new Vector2(160, 90) - new Vector2(width / 2, height / 2), width, height);
            Scene.Add(t);
            while (t.TransitionAmount < 1)
            {
                yield return null;
            }
            WarpProgram program = new WarpProgram(Parent, t);
            Scene.Add(program);

            while (t.TransitionAmount > 0)
            {
                yield return null;
            }
            to = from;
            from = level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeOut(i));
                yield return null;
            }
            yield return 0.1f;
            player.StateMachine.State = Player.StNormal;
            yield return null;
        }*/
    }
}
