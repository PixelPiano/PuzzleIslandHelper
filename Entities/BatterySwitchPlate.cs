using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Celeste.Mod.PuzzleIslandHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/SwitchPlate")]
    [Tracked]
    public class BatterySwitchPlate : Entity
    {
        public string BatteryID;
        public bool Activated;
        private bool isEmpty => string.IsNullOrEmpty(BatteryID);
        private Color FillColor = Color.Yellow;
        private Color IndentColor = Color.DarkSlateGray;
        private Vector2[] nodes;
        private int pipeWidth;

        public static int pipeWidthColliderValue = 4;
        public float transportSpeedEnterMultiplier = 0.75f;
        public int pipeColliderWidth = 28;
        public int pipeColliderDepth = 4;

        public Direction startDirection;


        public Direction endDirection;

        public enum Direction
        {
            Up,
            Right,
            Down,
            Left,
            None
        }

        public Vector2 startDirectionVector;
        public Vector2 endDirectionVector;
        public Hitbox startCollider;
        public Hitbox endCollider;
        public List<SwitchPlateTex> textures = new();
        public BloomPoint[] Blooms = new BloomPoint[3];
        public List<Vector2> Positions = new();
        private int currentPosition;
        private bool inRoutine;
        public BatterySwitchPlate(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 2;
            BatteryID = data.Attr("batteryId");
            nodes = data.NodesWithPosition(offset);
            Blooms[0] = new BloomPoint(0.7f, 8);
            Blooms[1] = new BloomPoint(1, 8);
            Blooms[2] = new BloomPoint(0.7f, 8);
            Add(Blooms);
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] -= Vector2.One * 4;
            }
            removeRedundantNodes();
            AddTag(Tags.TransitionUpdate);
        }
        public override void Awake(Scene scene)
        {
            if (nodes.Length - 2 < 0) RemoveSelf();
            pipeWidth = 8;
            pipeColliderWidth = pipeWidth - pipeWidthColliderValue;
            startDirection = GetPipeExitDirection(nodes[0], nodes[1]);
            endDirection = GetPipeExitDirection(nodes[nodes.Length - 1], nodes[nodes.Length - 2]);
            startDirectionVector = GetPipeExitDirectionVector(nodes[0], nodes[1]);
            endDirectionVector = GetPipeExitDirectionVector(nodes[nodes.Length - 1], nodes[nodes.Length - 2]);
            startCollider = getPipeCollider(Vector2.Zero, startDirection, pipeWidth, pipeColliderWidth, pipeColliderDepth);
            endCollider = getPipeCollider(new Vector2(nodes.Last().X - nodes.First().X, nodes.Last().Y - nodes.First().Y), endDirection, pipeWidth, pipeColliderWidth, pipeColliderDepth);
            Collider = new ColliderList(startCollider, endCollider);
            Blooms[0].Visible = Blooms[1].Visible = Blooms[2].Visible = false;
            Add(Blooms);
            addPipeSolids(8);
            if (!isEmpty && PianoModule.Session.DrillBatteryIds.Contains(BatteryID))
            {
                Activate(true);
            }
            base.Awake(scene);
        }
        public void addPipeSolids(int pipeWidth)
        {
            Vector2 startNode = Vector2.Zero;
            bool flag = false;
            int num = 0;
            Vector2[] array = nodes;
            foreach (Vector2 endNode in array)
            {
                if (flag)
                {
                    bool startNodeExit = num == 1;
                    bool endNodeExit = num == nodes.Count() - 1;
                    Vector2 nextNode = nodes.ElementAtOrDefault(num + 1);
                    SwitchPlateTex tex = SwitchPlateTex.FromNodes(startNode, endNode, nextNode, startNodeExit, endNodeExit, pipeWidth,IndentColor, FillColor);
                    textures.Add(tex);
                    foreach (var b in tex.Bolts)
                    {
                        Positions.Add(b.RenderPosition + Vector2.One * 4);
                    }
                    foreach (var c in tex.Corners)
                    {
                        Positions.Add(c.RenderPosition + Vector2.One * 4);
                    }

                    Scene.Add(tex);
                }
                flag = true;
                startNode = endNode;
                num++;
            }
        }
        public static Vector2 GetPipeExitDirectionVector(Vector2 exit, Vector2 previous)
        {
            return (exit - previous).SafeNormalize();
        }
        public Hitbox getPipeCollider(Vector2 position, Direction exitDireciton, int pipeWidth, int pipeColliderWidth, int colliderDepth)
        {
            switch (exitDireciton)
            {
                case Direction.Up:
                    return new Hitbox(pipeColliderWidth, colliderDepth, position.X - (float)(pipeColliderWidth / 2), position.Y - (float)colliderDepth);
                case Direction.Right:
                    if (pipeWidth / 8 % 2 == 1 && nodes.Length > 2)
                    {
                        return new Hitbox(colliderDepth, pipeColliderWidth, position.X - 4f, position.Y - (float)(pipeColliderWidth / 2));
                    }

                    return new Hitbox(colliderDepth, pipeColliderWidth, position.X, position.Y - (float)(pipeColliderWidth / 2));
                case Direction.Down:
                    if (pipeWidth / 8 % 2 == 1 && nodes.Length > 2)
                    {
                        return new Hitbox(pipeColliderWidth, colliderDepth, position.X - (float)(pipeColliderWidth / 2), position.Y - 4f);
                    }

                    return new Hitbox(pipeColliderWidth, colliderDepth, position.X - (float)(pipeColliderWidth / 2), position.Y);
                case Direction.Left:
                    return new Hitbox(colliderDepth, pipeColliderWidth, position.X - (float)colliderDepth, position.Y - (float)(pipeColliderWidth / 2));
                default:
                    return new Hitbox(pipeWidth, pipeWidth, position.X - (float)(pipeWidth / 2), position.Y - (float)(pipeWidth / 2));
            }
        }
        public void removeRedundantNodes()
        {
            List<Vector2> list = new List<Vector2>();
            Vector2 vector = Vector2.Zero;
            Vector2 vector2 = Vector2.Zero;
            bool flag = false;
            Vector2[] array = nodes;
            foreach (Vector2 vector3 in array)
            {
                if (flag)
                {
                    Vector2 vector4 = (vector - vector3).SafeNormalize();
                    if ((double)Math.Abs(vector4.X - vector2.X) > 0.0005 || (double)Math.Abs(vector4.Y - vector2.Y) > 0.0005)
                    {
                        list.Add(vector);
                    }

                    vector2 = vector4;
                }

                flag = true;
                vector = vector3;
            }

            list.Add(nodes.Last());
            nodes = list.ToArray();
        }
        public override void Update()
        {
            base.Update();

            UpdateLightPositions();

            foreach (SwitchPlateTex s in textures)
            {
                s.SpriteColor = FillColor;
                s.IndentColor = IndentColor;
            }
        }
        public void Activate(bool skipToEnd)
        {
            Activated = true;
            
            if (skipToEnd)
            {
                foreach (var t in textures)
                {
                    foreach (SwitchPlateTex.FillAnim anim in t.Bolts)
                    {
                        anim.Finished = true;
                    }
                    foreach (Sprite s in t.Corners)
                    {
                        s.Play("idle");
                    }
                }
            }
            else
            {
                Add(new Coroutine(Routine()));
            }
        }
        public void UpdateLightPositions()
        {
            foreach (BloomPoint bloom in Blooms)
            {
                bloom.Visible = bloom.Position != Vector2.Zero && inRoutine;
            }
            if (!Activated)
            {
                foreach (BloomPoint bloom in Blooms)
                {
                    bloom.Visible = false;
                    bloom.Position = Vector2.Zero;
                }
                return;
            }

            if (currentPosition - 1 > 0)
            {
                Blooms[0].Position = Positions[currentPosition - 1];
            }
            if (currentPosition < Positions.Count)
            {
                Blooms[1].Position = Positions[currentPosition];
            }
            if (currentPosition + 1 < Positions.Count)
            {
                Blooms[2].Position = Positions[currentPosition + 1];
            }
            foreach (BloomPoint bloom in Blooms)
            {
                bloom.Position -= Position;
            }
        }
        public IEnumerator Routine()
        {
            inRoutine = true;
            foreach (var t in textures)
            {
                foreach (SwitchPlateTex.FillAnim anim in t.Bolts)
                {
                    anim.Animate();
                    while (!anim.Finished)
                    {
                        yield return null;
                    }
                    currentPosition++;
                    anim.Stop(true);
                }
                foreach (Sprite s in t.Corners)
                {
                    s.Play("fill");
                    while (s.CurrentAnimationID != "idle")
                    {
                        yield return null;
                    }
                    currentPosition++;
                }
            }
            inRoutine = false;
            yield return null;
        }



        public static Direction GetPipeExitDirection(Vector2 exit, Vector2 previous)
        {
            if (exit.X < previous.X)
            {
                return Direction.Left;
            }

            if (exit.X > previous.X)
            {
                return Direction.Right;
            }

            if (exit.Y < previous.Y)
            {
                return Direction.Up;
            }

            if (exit.Y > previous.Y)
            {
                return Direction.Down;
            }

            return Direction.None;
        }
    }
}