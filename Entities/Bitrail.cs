using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/Bitrail")]
    [Tracked]
    public class Bitrail : Entity
    {
        private Vector2[] nodes;
        private const int pipeWidth = 8;

        public static int pipeWidthColliderValue = 4;
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
        public List<BitrailTex> textures = new();
        private Vector2 travelPosition;
        public Bitrail(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 2;
            nodes = data.NodesWithPosition(offset);
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] -= Vector2.One * 4;
            }
            travelPosition = Position;
            RemoveRedundantNodes();
            AddTag(Tags.TransitionUpdate);
            Add(new Coroutine(Travel(70 * Engine.DeltaTime)));
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(travelPosition, 8, 8, Color.Green);
        }
        public IEnumerator Travel(float speed)
        {
            while (true)
            {
                travelPosition = Position;
                for (int i = 0; i < nodes.Length; i++)
                {
                    Vector2 to = nodes[i];
                    while (travelPosition != to)
                    {
                        travelPosition = Calc.Approach(travelPosition, to, speed);
                        yield return null;
                    }
                }
            }
        }

        public override void Awake(Scene scene)
        {
            if (nodes.Length - 2 < 0) RemoveSelf();
            pipeColliderWidth = pipeWidth - pipeWidthColliderValue;
            startDirection = GetPipeExitDirection(nodes[0], nodes[1]);
            endDirection = GetPipeExitDirection(nodes[nodes.Length - 1], nodes[nodes.Length - 2]);
            startDirectionVector = GetPipeExitDirectionVector(nodes[0], nodes[1]);
            endDirectionVector = GetPipeExitDirectionVector(nodes[nodes.Length - 1], nodes[nodes.Length - 2]);
            startCollider = getPipeCollider(Vector2.Zero, startDirection, pipeWidth, pipeColliderWidth, pipeColliderDepth);
            endCollider = getPipeCollider(new Vector2(nodes.Last().X - nodes.First().X, nodes.Last().Y - nodes.First().Y), endDirection, pipeWidth, pipeColliderWidth, pipeColliderDepth);
            Collider = new ColliderList(startCollider, endCollider);
            addRailSolids(8);
            base.Awake(scene);
        }
        public void addRailSolids(int pipeWidth)
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
                    BitrailTex tex = BitrailTex.FromNodes(startNode, endNode, nextNode, startNodeExit, endNodeExit, pipeWidth);
                    textures.Add(tex);
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
        private void RemoveRedundantNodes()
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

        public static Direction GetPipeExitDirection(Vector2 exit, Vector2 previous)
        {
            if (exit.X < previous.X) return Direction.Left;
            if (exit.X > previous.X) return Direction.Right;
            if (exit.Y < previous.Y) return Direction.Up;
            if (exit.Y > previous.Y) return Direction.Down;
            return Direction.None;
        }
    }
}