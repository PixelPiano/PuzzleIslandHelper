using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Hints
{
    [CustomEvent("PuzzleIslandHelper/MusicNoteEvent")]
    public class MusicHintCutscene : CutsceneEntity
    {
        private Image Front;
        private Image Back;
        private Image Arrow;
        private float arrowAlpha;
        private float eased;
        public MusicHintCutscene() : base()
        {
            Visible = true;
            Tag |= TagsExt.SubHUD;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Front = new Image(GFX.Game["objects/PuzzleIslandHelper/towerNote/flipSideA"]);
            Front.CenterOrigin();
            Front.Position += Front.HalfSize();
            Back = new Image(GFX.Game["objects/PuzzleIslandHelper/towerNote/flipSideB"]);
            Back.CenterOrigin();
            Back.Position += Back.HalfSize();
            Arrow = new Image(GFX.Game["objects/PuzzleIslandHelper/towerNote/arrow"]);
            Add(Front, Back, Arrow);
            Back.Visible = false;

        }
        public override void Update()
        {
            Front.Position = Back.Position = new Vector2(960, 540 + 1620 * (1 - eased));
            Back.Color = Front.Color = Color.White * Math.Min(eased + 0.2f, 1);
            Front.Rotation = Back.Rotation = (1 - eased) * MathHelper.PiOver2;
            Arrow.Color = Color.Lerp(Color.White, Color.Black, (float)(Math.Sin(Scene.TimeActive) + 1) / 2f * 0.6f) * eased * arrowAlpha;
            base.Update();
        }
        public override void Render()
        {
            Draw.Rect(0, 0, 1920, 1080, Color.Black * eased * 0.5f);
            base.Render();
        }
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(routine()));
        }
        private IEnumerator routine()
        {
            Scene.DisableMovement();
            arrowAlpha = 0;
            eased = 0;
            Front.Visible = true;
            Back.Visible = false;
            yield return PianoUtils.Lerp(Ease.SineInOut, 1.4f, f => eased = arrowAlpha = f, true);
            while (!Input.MenuCancel)
            {
                if (Input.MenuConfirm)
                {
                    Image a = Back, b = Front;
                    if (Front.Visible)
                    {
                        a = Front;
                        b = Back;
                    }
                    a.Visible = true;
                    b.Visible = false;
                    yield return PianoUtils.Lerp(Ease.Linear, 0.5f, f => arrowAlpha = a.Scale.X = 1 - f, true);
                    a.Visible = false;
                    b.Visible = true;
                    b.Scale.X = 0;
                    yield return PianoUtils.Lerp(Ease.Linear, 0.5f, f => arrowAlpha = b.Scale.X = f, true);
                }
                yield return null;
            }
            yield return PianoUtils.Lerp(Ease.SineInOut, 1.4f, f => eased = arrowAlpha = 1 - f, true);
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            eased = 0;
            level.EnableMovement();
        }
    }

    [CustomEntity("PuzzleIslandHelper/MusicHint")]
    [Tracked]
    public class MusicHint : Entity
    {
        public static void DrawMusicCircle(Vector2 position, float angle, float radius, Color color, int circleThickness = 1)
        {
            Vector2 center = position + radius * Vector2.One;
            Draw.Circle(center, 1, color, 4);
            for (int i = 1; i < 5; i++)
            {
                Draw.Circle(center, FrequencyData.Interval * i / FrequencyData.Max * radius, color, circleThickness, (int)FrequencyData.Interval * i);
            }
        }
        public static void DrawMusicCircleLineAngle(Vector2 position, float angle, float radius, Color color, int angleLineThickness = 2, int circleThickness = 1)
        {
            DrawMusicCircle(position, angle, radius, color, circleThickness);
            DrawMusicLineAngle(position, angle, radius, color, angleLineThickness);
        }
        public static void DrawMusicLineAngle(Vector2 position, float angle, float radius, Color color, int thickness = 2)
        {
            Vector2 center = position + radius * Vector2.One;
            Draw.LineAngle(center, angle, radius, color, thickness);
        }
        public static void DrawMusicStaff(Vector2 position, float width, float height, Color color, int lineThickness = 1)
        {
            Draw.Line(position, position + Vector2.UnitX * width, color, lineThickness);
            for (int i = 1; i < 5; i++)
            {
                float y = (FrequencyData.Interval * i / FrequencyData.Max * height);
                Draw.Line(position + Vector2.UnitY * y, position + new Vector2(width, y), color, lineThickness);
            }
        }
        public static void DrawMusicStaffWithSides(Vector2 position, float width, float height, Color color, int thickness = 1)
        {
            Draw.Line(position, position + Vector2.UnitX * width, color, thickness);
            for (int i = 1; i < 5; i++)
            {
                float y = (FrequencyData.Interval * i / FrequencyData.Max * height);
                Draw.Line(position + Vector2.UnitY * y, position + new Vector2(position.X, y), color, thickness);
            }
            Draw.Line(position, position + Vector2.UnitY * height, color, thickness);
            Draw.Line(position + Vector2.UnitX * width, position + new Vector2(width, height), color, thickness);
        }
        public static Vector2[] StaffLineStarts(Vector2 position, float width, float height)
        {
            Vector2[] positions = new Vector2[5];
            positions[0] = position;
            for (int i = 1; i < 5; i++)
            {
                positions[i] = position + Vector2.UnitY * (FrequencyData.Interval * i / FrequencyData.Max * height);
            }
            return positions;
        }
        public enum Types
        {
            Staff,
            Circle
        }
        public Types Type;
        public FrequencyOrbComponent[] Orbs = new FrequencyOrbComponent[4];
        public float[] Targets;
        public readonly float Radius;
        private bool orbsCreated;
        public float Angle
        {
            get => angle;
            set
            {
                float prev = angle;
                angle = value;
                if (prev != angle && orbsCreated && Type == Types.Circle)
                {
                    UpdateOrbs();
                }
            }
        }
        private float angle;
        private bool UseIndents;
        private FlagList flagOnFinish;
        private float finishTimer;
        private float finishDuration;
        public bool UseAngle = true;
        public bool UseLines = true;
        public bool UseOrbs = true;
        private bool autoFadeOrbs;
        public MusicHint(Vector2 position, int width, int height, bool useOrbs, bool useIndents, float[] rates, FlagList flagOnEnd, bool autoFadeOrbs = false) : base(position)
        {
            Collider = new Hitbox(width, height);
            UseOrbs = useOrbs;
            UseIndents = useIndents;
            Targets = rates;
            flagOnFinish = flagOnEnd;
            this.autoFadeOrbs = autoFadeOrbs;
            Depth = 10;
        }
        public MusicHint(Vector2 position, float radius, float angleDegrees, bool useOrbs, bool useIndents, float[] rates, FlagList flagOnEnd, bool autoFadeOrbs = false)
            : this(position, (int)radius * 2, (int)radius * 2, useOrbs, useIndents, rates, flagOnEnd, autoFadeOrbs)
        {
            Type = Types.Circle;
            angle = angleDegrees.ToRad();
            Radius = radius;
        }
        public MusicHint(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Depth = 10;
            Targets = [data.Float("rate1", -1), data.Float("rate2", -1), data.Float("rate3", -1), data.Float("rate4", -1)];
            Radius = Math.Min(Width, Height) / 2f;
            Type = data.Bool("unravelled", true) ? Types.Staff : Types.Circle;
            angle = data.Float("ravelledAngle", 180f).ToRad();
            finishTimer = finishDuration = data.Float("finishDuration");
            UseOrbs = data.Bool("visibleOrbs");
            UseIndents = data.Bool("visibleIndents");
            flagOnFinish = data.FlagList("flagOnFinish");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            bool isStaff = Type == Types.Staff;
            float angle = Angle + MathHelper.PiOver4;
            float maxLength = isStaff ? Height : Radius;
            MTexture indentTex = GFX.Game["objects/PuzzleIslandHelper/forkAmp/orb"];
            (Vector2 from, Vector2 to) pair;
            for (int i = 0; i < 4; i++)
            {
                switch (Type)
                {
                    case Types.Staff:
                        pair.from = new Vector2(8 + (Width - 16) / 4 * i, 0);
                        pair.to = new Vector2(pair.from.X, maxLength);
                        break;
                    case Types.Circle:
                    default:
                        pair.from = Vector2.One * Radius;
                        pair.to = pair.from + Calc.AngleToVector(angle, maxLength);
                        angle += MathHelper.PiOver2;
                        break;
                }
                if (Targets[i] >= 0)
                {
                    Image image = new Image(indentTex);
                    image.Visible = UseIndents;
                    image.CenterOrigin();
                    image.Color = Color.Red;
                    image.Position = Vector2.Lerp(pair.from, pair.to, Targets[i] / FrequencyData.Max);
                    Add(image);
                }
                Orbs[i] = new FrequencyOrbComponent(i, pair.from, pair.to, Color.White, Color.Blue, 1, false, autoFadeOrbs)
                {
                    Hidden = !UseOrbs
                };
            }
            orbsCreated = true;
            UpdateOrbs();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(Orbs);
        }
        public override void Update()
        {
            UpdateOrbs();
            base.Update();
            if (ForkAmpSound.GlobalPlaying)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (Targets[i] < 0) continue;
                    if (MathHelper.Distance(FrequencyData.GetRate(Scene, i), Targets[i]) > 5)
                    {
                        finishTimer = finishDuration;
                        return;
                    }
                }
            }
            if (finishTimer > 0)
            {
                finishTimer -= Engine.DeltaTime;
                if (finishTimer <= 0)
                {
                    finishTimer = 0;
                    flagOnFinish.State = true;
                }
            }
        }
        public (Vector2 from, Vector2 to)[] OrbStaffLines(Vector2 position, float width, float height)
        {
            (Vector2 from, Vector2 to)[] pairs = new (Vector2 from, Vector2 to)[4];
            for (int i = 0; i < 4; i++)
            {
                Vector2 f = new Vector2(8 + (width - 16) / 4 * i, height);
                pairs[i] = (position + f.XComp(), position + f);
            }
            return pairs;
        }
        public (Vector2 from, Vector2 to)[] OrbCircleLines(Vector2 position, float radius)
        {
            (Vector2 from, Vector2 to)[] pairs = new (Vector2 from, Vector2 to)[4];
            float angle = this.angle + MathHelper.PiOver4;
            Vector2 f = Vector2.One * radius;
            for (int i = 0; i < 4; i++)
            {
                pairs[i] = (position + f, position + f + Calc.AngleToVector(angle, radius));
                angle -= MathHelper.PiOver2;
            }
            return pairs;
        }
        public virtual void UpdateOrbs()
        {
            switch (Type)
            {
                case Types.Circle:
                    float angle = this.angle + MathHelper.PiOver4;
                    for (int i = 0; i < 4; i++)
                    {
                        Orbs[i].From = Vector2.One * Radius;
                        Orbs[i].To = Vector2.One * Radius + Calc.AngleToVector(angle, Radius);
                        angle += MathHelper.PiOver2;
                    }
                    break;
                case Types.Staff:
                    for (int i = 0; i < 4; i++)
                    {
                        Orbs[i].From = new Vector2(8 + (Width - 16) / 4 * i, 0);
                        Orbs[i].To = new Vector2(Orbs[i].From.X, Height);
                    }
                    break;
            }
            foreach (var o in Orbs)
            {
                if (!UseOrbs)
                {
                    o.Hidden = true;
                }
                o.CanFade = autoFadeOrbs;
            }
        }
        public virtual void DrawStaff(Vector2 position)
        {
            DrawMusicStaff(position, Width, Height, Color.Black);
        }
        public virtual void DrawCircles(Vector2 position)
        {
            DrawMusicCircle(position, Angle, Radius, Color.Black);
        }
        public virtual void DrawAngle(Vector2 position)
        {
            DrawMusicLineAngle(position, Angle, Radius, Color.Black);
        }
        public override void Render()
        {
            Draw.Rect(Collider, Color.DarkGray);
            switch (Type)
            {
                case Types.Staff:
                    if (UseLines) DrawStaff(Position);
                    break;
                case Types.Circle:
                    if (UseLines) DrawCircles(Position);
                    if (UseAngle) DrawAngle(Position);
                    break;
            }
            base.Render();
        }
    }
}
