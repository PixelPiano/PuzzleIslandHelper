using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Hints
{
    [CustomEntity("PuzzleIslandHelper/FrequencyBTutorial")]
    [Tracked(false)]
    public class FrequencyBTutorial : Entity
    {
        private class Hint : MusicHint
        {
            public float MorphPercent = 1;
            public float Rotation = -MathHelper.PiOver2;
            private VirtualRenderTarget target;
            public float anglePercent;
            public Hint(Vector2 position, float radius, float angleDegrees, float[] rates, FlagList flagOnFinish)
                : base(position, radius, angleDegrees, true, false, rates, flagOnFinish)
            {
                target = VirtualContent.CreateRenderTarget("music hint", (int)Width, (int)Height);
                Add(new BeforeRenderHook(() =>
                {
                    target.SetAsTarget(true);
                    Draw.SpriteBatch.Begin();
                    DrawMorphHint(Radius * Vector2.One, Radius, MorphPercent);
                    Vector2 p = Position;
                    Position = Vector2.Zero;
                    Components.Render();
                    Position = p;
                    Draw.SpriteBatch.End();
                }));
            }
            public override void Update()
            {
                base.Update();
            }
            public override void Render()
            {
                Draw.Rect(Collider, Color.DarkGray);
                Draw.SpriteBatch.Draw(target, Center + Vector2.UnitY * (Height / 4) * (MorphPercent), null, Color.White, Rotation + Angle * anglePercent, target.HalfSize(), 1, SpriteEffects.None, 0);
            }
            public struct Involute
            {
                public static float[] Angles;
                public const int PointsFromCenter = 10;
                public const int Points = PointsFromCenter * 2 + 1;
                [OnLoad]
                public static void Load()
                {
                    Angles = new float[Points];
                    for (int i = 0; i < PointsFromCenter * 2; i++)
                    {
                        Angles[i] = -MathHelper.Pi + (MathHelper.TwoPi / (PointsFromCenter * 2) * i);
                    }
                    Angles[^1] = MathHelper.Pi;
                }
                public static Vector2 Equation(float r, float a, float p)
                {
                    float at = a * p;
                    return new(
                        x: (float)(r * (Math.Cos(at - a) + at * Math.Sin(at - a))),
                        y: (float)(r * (Math.Sin(at - a) - at * Math.Cos(at - a))));
                }
                public static Vector2 Point(Vector2 center, float freq, float maxRadius, float morphPercent, float linePercent)
                {
                    int line = (int)(freq / FrequencyData.Interval);
                    float inc = maxRadius / 5f;
                    float lineLength = maxRadius / 4f;
                    center += Vector2.UnitX * (line * inc - lineLength) * morphPercent;
                    float r = Calc.LerpClamp(line * inc, lineLength, morphPercent);
                    return center + Equation(r, Calc.LerpClamp(MathF.PI, -MathF.PI, linePercent), morphPercent);
                }
            }
            public static void DrawBendingLine(Vector2 center, float radius, float p)
            {
                float r = radius;
                Vector2 prev = Involute.Equation(r, Involute.Angles[0], p);
                for (int i = 1; i < Involute.Points; i++)
                {
                    Vector2 next = Involute.Equation(r, Involute.Angles[i], p);
                    Draw.Line(center + prev, center + next, Color.Black, 2);
                    prev = next;
                }
            }
            public static void DrawWalls(Vector2 center, float radius, float p)
            {
                float inc = radius / 5f;
                float lineLength = radius / 4f;
                float closeR = Calc.LerpClamp(1, lineLength, p);
                float farR = Calc.LerpClamp(inc * 4 + 1, lineLength, p);
                Vector2 a1, b1, a2, b2;
                a1 = Involute.Equation(closeR, Involute.Angles[0], p) + center + new Vector2(1 - lineLength, 0) * p;
                b1 = Involute.Equation(farR, Involute.Angles[0], p) + center + new Vector2(radius + 1 - lineLength, 0) * p;
                a2 = Involute.Equation(closeR, Involute.Angles[^1], p) + center + new Vector2(1 - lineLength, 0) * p;
                b2 = Involute.Equation(farR, Involute.Angles[^1], p) + center + new Vector2(radius + 1 - lineLength, 0) * p;
                Draw.Line(a1, b1, Color.Black, 2);
                Draw.Line(a2, b2, Color.Black, 2);
            }
            public static void DrawMorphHint(Vector2 center, float radius, float morphPercent)
            {
                float inc = radius / 5f;
                float r = 1;
                float lineLength = radius / 4f;
                for (int i = 0; i < 5; i++)
                {
                    DrawBendingLine(
                        center: center + new Vector2(r - lineLength, 0) * morphPercent,
                        radius: Calc.LerpClamp(r, lineLength, morphPercent),
                        p: morphPercent);
                    r += inc;
                }
                DrawWalls(center, radius, morphPercent);
            }
            public override void UpdateOrbs()
            {
                for (int i = 0; i < 4; i++)
                {
                    Orbs[i].From = Orbs[i].To = Orbs[i].Position = Vector2.UnitX + Involute.Point(Vector2.One * Radius, Targets[i], Radius, MorphPercent, 0.1f + i * 0.2f);
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                target.Dispose();
            }
            public override void DrawCircles(Vector2 position)
            {
                DrawMusicCircle(position, Angle, Radius, Color.Black);
            }

        }
        private class Light : GraphicsComponent
        {
            public MTexture lightTex, shade, outline, center;
            public bool On => FrequencyData.PuzzleSolved(id);
            private Color onColor, offColor;
            private string id = "";
            public Light(Vector2 position, Color off, Color on, string id) : base(true)
            {
                this.id = id;
                Position = position;
                string path = "objects/PuzzleIslandHelper/musicHint/";
                shade = GFX.Game[path + "lightShade"];
                outline = GFX.Game[path + "lightOutline"];
                lightTex = GFX.Game[path + "lightInside"];
                center = GFX.Game[path + "lightCenter"];
                offColor = off;
                onColor = on;
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                Color = On ? onColor : offColor;
            }
            public override void Render()
            {
                base.Render();
                Vector2 renderPosition = RenderPosition;
                outline.DrawCentered(renderPosition, Color.Black);
                lightTex.DrawCentered(renderPosition, Color);
                if (On)
                {
                    center.DrawCentered(renderPosition, Color);
                }
                shade.DrawCentered(renderPosition, Color.White);
            }
        }
        private class Cutscene : CutsceneEntity
        {
            public FrequencyBTutorial Parent;
            public Cutscene(FrequencyBTutorial parent) : base()
            {
                Parent = parent;
            }
            public override void OnBegin(Level level)
            {
                Add(new Coroutine(routine()));
            }
            private IEnumerator routine()
            {
                Player player = Level.GetPlayer();
                if (player == null) yield break;

                player.DisableMovement();
                Vector2 center = Parent.Center;
                Rectangle bounds = Level.Bounds;
                yield return CameraTo((center - new Vector2(160, 90)).Clamp(bounds), 1, Ease.SineInOut, 0.5f);
                yield return 0.5f;
                yield return Level.ZoomToWorld(center, Parent.Width / (180f * 0.3f), 1f);
                yield return 1;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 4f)
                {
                    Parent.Percent = 1 - Ease.SineInOut(i);
                    yield return null;
                }
                Parent.Percent = 0;
                yield return 0.5f;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 1)
                {
                    Parent.hint.anglePercent = Ease.SineInOut(i);
                    yield return null;
                }
                Parent.hint.anglePercent = 1;
                yield return 1f;
                yield return Level.ZoomBack(1);
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                Revealed.State = true;
                level.EnableMovement();
                if (WasSkipped)
                {
                    Parent.hint.MorphPercent = 0;
                    Parent.hint.anglePercent = 1;
                    Level.ResetZoom();
                }
            }
        }
        public static FlagData Revealed = new FlagData("FrequencyBTutorialRevealed");
        private Hint hint;
        private Light[] lights = new Light[3];
        private Cutscene cutscene;
        private float[] rates;
        private string[] ids;
        private float angleDegrees;
        private bool puzzleSolved;
        private bool wasSolved;
        public float Percent
        {
            get => hint.MorphPercent;
            set => hint.MorphPercent = value;
        }
        private GlobalFrequencyReceiver code;
        public FrequencyBTutorial(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 10;
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            ids = new string[3];
            string[] array = data.Attr("ids").Replace(" ", "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < array.Length && i < 3; i++)
            {
                ids[i] = array[i];
            }
            rates = [data.Float("rateA"), data.Float("rateB"), data.Float("rateC"), data.Float("rateD")];
            angleDegrees = data.Float("angleDegrees");
            Add(code = new GlobalFrequencyReceiver(rates)
            {
                MarginOfError = 0,
                RequiresAudibleSound = true,
                OnFullPower = () =>
                {
                    puzzleSolved = true;
                }
            });
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            float nodeY = Height + 16;
            float nodeXInc = Width / 3f;
            float nodeX = nodeXInc / 2;
            for (int i = 0; i < 3; i++)
            {
                string id = ids[i];
                lights[i] = new Light(new Vector2(nodeX, nodeY), Color.Gray, Color.Lime, id);
                nodeX += nodeXInc;
            }
            Add(lights);

            float radius = Math.Min(Width, Height) / 2;
            hint =
                new Hint(Center - Vector2.One * radius, radius, angleDegrees,
                rates, default);
            scene.Add(hint);
            hint.Depth = Depth - 1;
        }
        public override void Awake(Scene scene)
        {
            hint.UseLines = lights[0].On;
            hint.Orbs[0].Hidden = hint.Orbs[1].Hidden = !lights[1].On;
            hint.Orbs[2].Hidden = hint.Orbs[3].Hidden = !lights[2].On;
            bool puzzleSolved = this.puzzleSolved;
            if (puzzleSolved)
            {
                hint.MorphPercent = 0;
                hint.anglePercent = 1;
            }
            wasSolved = puzzleSolved;
            foreach (var orb in hint.Orbs)
            {
                orb.CanMove = false;
            }
            base.Awake(scene);
        }
        public override void Update()
        {
            hint.UseLines = lights[0].On;
            hint.Orbs[0].Hidden = hint.Orbs[1].Hidden = !lights[1].On;
            hint.Orbs[2].Hidden = hint.Orbs[3].Hidden = !lights[2].On;
            code.Active = lights[0].On && lights[1].On && lights[2].On;
            bool puzzleSolved = this.puzzleSolved;
            if (puzzleSolved)
            {
                if (!wasSolved && cutscene == null)
                {
                    Scene.Add(cutscene = new Cutscene(this));
                }
                else if (!cutscene.Active)
                {
                    hint.MorphPercent = 0;
                    hint.anglePercent = 1;
                }
            }
            wasSolved = puzzleSolved;
            base.Update();
        }
        public override void Render()
        {
            Draw.Rect(Collider, Color.Black);
            base.Render();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            hint.RemoveSelf();
        }

    }
}