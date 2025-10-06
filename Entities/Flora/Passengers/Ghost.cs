using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Ghost")]
    [Tracked]
    public class Ghost : VertexPassenger
    {
        private const int armLength = 12;
        public Group[] Groups = new Group[3];
        public Ghost(EntityData data, Vector2 offset, EntityID id) : this(data, offset, id, armLength * 3, armLength * 3, Vector2.One, new(-1, 1), 0.95f)
        {
        }
        public Ghost(EntityData data, Vector2 offset, EntityID id, float width, float height, Vector2 scale, Vector2 breathDirection, float breathDuration) : base(data, offset, id, width, height, scale, breathDirection, breathDuration)
        {

            HasGravity = false;
            MinWiggleTime = 1;
            MaxWiggleTime = 2.5f;
            for (int i = 0; i < Groups.Length; i++)
            {
                Groups[i] = new Group()
                {
                    RotationRate = 1f.ToRad(),
                };
            }

            /*            
             *            PointData[] ct = [new(Collider.BottomLeft, Color.Lime), new(Collider.TopCenter, Color.Lime), new(Collider.BottomRight, Color.Lime), new(Collider.BottomLeft, Color.Lime)];
             *            for (int i = 1; i < ct.Length; i++)
                        {
                            Vector2 center = getSideTriCenter(ct[i - 1].Point, ct[i].Point, armLength, out float angle);
                            AddEquilateral(new PointData()
                            {
                                Point = center,
                                DefaultColor = Color.Lime,
                                Group = Groups[i - 1],
                                Shifter = null,
                            }, armLength, angle);
                        }
                        AddTriangle(ct[0], ct[1], ct[2]);*/

            Vector2 bodyTop = Collider.TopCenter;
            Vector2 bodyLeft = Collider.Center + new Vector2(-Width / 4f, Height / 8f);
            Vector2 bodyRight = Collider.Center + new Vector2(Width / 4f, Height / 8f);
            Vector2 bodyBottom = Collider.BottomCenter;
            AddTriangle(bodyTop, bodyLeft, bodyRight, 1, -Vector2.UnitY);
            AddTriangle(bodyLeft, bodyRight, bodyBottom, 0, Vector2.Zero);
            PointData leftCenter = new PointData()
            {
                Point = bodyLeft,
                WiggleMult = 0,
                WiggleDir = Vector2.Zero,
                DefaultColor = Color.Lime
            };
            PointData rightCenter = new PointData()
            {
                Point = bodyRight,
                WiggleMult = 0,
                WiggleDir = Vector2.Zero,
                DefaultColor = Color.Lime
            };
            PointData circleCenter = new PointData()
            {
                Point = bodyBottom,
                WiggleMult = 0,
                WiggleDir = Vector2.Zero,
                DefaultColor = Color.DarkGreen
            };
            AddEquilateral(leftCenter, Width / 4f, 45f.ToRad());
            AddEquilateral(rightCenter, Width / 4f, 45f.ToRad());
            AddCircle(circleCenter, (bodyRight.X - bodyLeft.X) / 4, 0, 8);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            /*            foreach (var v in lines)
                        {
                            Draw.Line(v.Item1 + Position, v.Item2 + Position, Color.Orange);
                        }*/
        }
        private Vector2 getSideTriCenter(Vector2 a, Vector2 b, float radius, out float angle)
        {
            Vector2 centerLine = Vector2.Lerp(a, b, 0.5f);
            angle = Vector2.Normalize(a - b).Perpendicular().Angle();
            return centerLine + Calc.AngleToVector(angle, radius);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
        public override void Render()
        {
            base.Render();
        }

    }
}
