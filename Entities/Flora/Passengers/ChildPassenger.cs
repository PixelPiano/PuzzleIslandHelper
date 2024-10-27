using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    /*    public class NormalPassenger : Passenger
        {
            public int[] Indices;
            public Vector2[] Points;
            public VertexPositionColor[] Vertices;
            private Vector2 BreathDirection = new Vector2(-1, 1);
            private Vector2 BreathAmount;
            private Vector2[] origOffsets;

            public Tween[] Tweens;
            public ColorShifter[] ColorShifts = new ColorShifter[3];

            public NormalPassenger(EntityData data, Vector2 offset) : base(data.Position + offset, 16, 20, data.Attr("cutsceneID"))
            {
                Points = new Vector2[] { new(0.1f,0.4f), new(0.5f, 0), new(0.9f, 0.28f),
                                         new(0.5f),    new(1, 0.3f), new(1, 0.55f),
                                         new(0.55f, 0.6f), new(0.9f, 0.6f), new(0.8f, 1)};
                Tweens = new Tween[Points.Length];
                origOffsets = new Vector2[Points.Length];
                Vertices = Points.CreateVertices(Collider.Size, out Indices, Color.Lime);
                Position.Y -= 4;
                Add(ColorShifts[0] = new ColorShifter(1f, Ease.Linear, Color.SpringGreen, Color.Lime, Color.LimeGreen));
                Add(ColorShifts[1] = new ColorShifter(0.8f, Ease.Linear, Color.Green, Color.ForestGreen, Color.LawnGreen));
                Add(ColorShifts[2] = new ColorShifter(0.5f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
                Tween breathing = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 1.1f, true);
                breathing.OnUpdate = t => { BreathAmount = BreathDirection * t.Eased; };
                Add(breathing);
            }
            public float GetMultiplier(int index)
            {
                return 1 - (index / 3) * 0.4f;
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                for (int i = 0; i < Points.Length; i++)
                {
                    origOffsets[i] = Vector2.UnitY * Calc.Random.Choose(-1, 1) * Calc.Random.Range(0.5f, 1) * GetMultiplier(i);
                    Tweens[i] = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.QuadInOut, Calc.Random.Range(0.8f, 2), true);
                    Tweens[i].Randomize();
                    Add(Tweens[i]);
                }
            }
            public override void Update()
            {
                base.Update();
                for (int i = 0; i < Points.Length; i++)
                {
                    float multiplier = GetMultiplier(i);
                    Vector2 position = Position + (Points[i] * Collider.Size);
                    Vector2 offset = origOffsets[i] * Tweens[i].Eased * multiplier;
                    Vector2 breath = BreathAmount * multiplier;
                    Vertices[i].Position = new Vector3(position + offset + breath, 0);
                    Vertices[i].Color = ColorShifts[i / 3][i % 3];
                }
            }
            public override void Render()
            {
                Draw.SpriteBatch.End();
                GFX.DrawIndexedVertices(SceneAs<Level>().Camera.Matrix, Vertices, Vertices.Length, Indices, 3);
                GameplayRenderer.Begin();
            }
        }*/
}
