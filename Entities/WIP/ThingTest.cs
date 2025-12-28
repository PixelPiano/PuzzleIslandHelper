using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.BinarySwitch
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP

{
    //[CustomEntity("PuzzleIslandHelper/WipEntity")]
    public class dummyWipEntity : Entity
    {
    }

    public class ThingTest : Entity
    {
        public class Node : GraphicsComponent
        {
            public bool On;
            public void Flip()
            {
                On = !On;
            }
            public Node(Vector2 position) : base(true)
            {
                Position = position;
            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(RenderPosition, 16, 16, On ? Color.Green : Color.Red);
            }

        }
        public bool Interacting;
        public Node[,] Nodes;
        public int SelectX;
        public int SelectY;
        private float inputBuffer;
        private float pressBuffer;
        public ThingTest(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = int.MinValue;
            Nodes = new Node[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Nodes[j, i] = new Node(16 * new Vector2(j, i));
                    Add(Nodes[j, i]);
                }
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Interact(scene.GetPlayer());
        }
        public void Flip(int x, int y)
        {
            if (x > 2 || y > 2 || x < 0 || y < 0) return;
            Nodes[x, y].Flip();
            if (x - 1 >= 0)
            {
                Nodes[x - 1, y].Flip();
            }
            if (x + 1 < 3)
            {
                Nodes[x + 1, y].Flip();
            }
            if (y - 1 >= 0)
            {
                Nodes[x, y - 1].Flip();
            }
            if (y + 1 < 3)
            {
                Nodes[x, y + 1].Flip();
            }
        }
        public void Interact(Player player)
        {
            Interacting = true;
            player.DisableMovement();
        }
        private bool foundInput()
        {
            if (Input.MoveX.Value != 0)
            {
                SelectX = Math.Clamp(SelectX + Input.MoveX.Value, 0, 2);
                return true;
            }
            else if (Input.MoveY.Value != 0)
            {
                SelectY = Math.Clamp(SelectY + Input.MoveY.Value, 0, 2);
                return true;
            }

            return false;
        }
        private bool dashWasPressed;
        private bool foundPress()
        {
            if (Input.Jump.Pressed)
            {
                Interacting = false;
                Scene.GetPlayer().EnableMovement();
                return false;
            }
            if (Input.DashPressed && !dashWasPressed)
            {
                Flip(SelectX, SelectY);
                return true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.L))
            {
                foreach (Node node in Nodes)
                {
                    node.On = false;
                }
                SelectX = 0;
                SelectY = 0;
                return true;
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
            if (Interacting)
            {
                if (inputBuffer > 0)
                {
                    inputBuffer -= Engine.DeltaTime;
                }
                else if (foundInput())
                {
                    inputBuffer = 0.1f;
                }
                foundPress();
            }
            dashWasPressed = Input.DashPressed;
        }
        public override void Render()
        {
            Draw.Rect(Position - Vector2.One * 4, 56, 56, Color.Magenta);
            base.Render();
            if (Interacting)
            {
                Draw.HollowRect(Position + new Vector2(SelectX, SelectY) * 16, 16, 16, Color.White);
            }
        }

    }
}