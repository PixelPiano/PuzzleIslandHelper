using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Ghost")]
    [Tracked]
    public class Ghost : VertexPassenger
    {
        public bool StartEmpty;
        public float VeilSize;
        public bool DrawCircles;
        public float[] Circles = new float[6];
        public Wiggler Wiggler;
        public SoundSource Scraping;
        public Ghost(EntityData data, Vector2 offset) : this(data.Position + offset, 16, 16, data.Attr("cutsceneID"), Vector2.One, new(-1, 1), 0.95f)
        {
        }
        public Ghost(Vector2 position) : this(position, 16, 16, null, Vector2.One, new(-1, 1), 1f)
        {
            Add(new DebugComponent(Microsoft.Xna.Framework.Input.Keys.O, GlitchCalidus, true));
        }
        public Ghost(Vector2 position, float width, float height, string cutscene, Vector2 scale, Vector2 breathDirection, float breathDuration) : base(position, width, height, cutscene, null, scale, breathDirection, breathDuration)
        {
            MinWiggleTime = 1;
            MaxWiggleTime = 2.5f;
            AddQuad(Vector2.Zero, new(0, 16), new(16, 0), new(16), 1, Vector2.One, new(Color.Red));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
            if (StartEmpty)
            {
                Alpha = 0;
                GravityMult = 0;
                Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t => VeilSize = t.Eased);
            }
        }
        public override void Render()
        {
            Draw.Rect(Center - Vector2.One * VeilSize * 32, 64 * VeilSize, 64 * VeilSize, Color.Green);
            if (DrawCircles)
            {
                for (int i = 0; i < Circles.Length; i++)
                {
                    Draw.Circle(Center, Circles[i], Color.Red, 50);
                }
            }
            base.Render();
        }
        public override void Update()
        {
            base.Update();
            if (onGround) GravityMult = 1;
        }
        public void Roar()
        {

        }
        public void GlitchCalidus()
        {
            ScaleApproach = Vector2.One * 3;
            Circles = new float[6];
            Tween t = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.8f, true);
            t.OnUpdate = t =>
            {
                float space = 0;
                for (int i = 0; i < Circles.Length; i++)
                {
                    Circles[i] = Calc.Max(0, Calc.LerpClamp(-Circles.Length + 1, 320, t.Eased) - space);
                    space += i;
                }
            };
            Add(t);
            Alarm.Set(this, 1, delegate { DrawCircles = false; });
        }
        public void Appear()
        {
            Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.Linear, t => { Alpha = t.Eased; },
                delegate { Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t => VeilSize = 1 - t.Eased, delegate { HasGravity = true; GravityMult = 0.3f; }); });
        }
        public IEnumerator Materialize()
        {
            Appear();
            while (!onGround)
            {
                yield return null;
            }
            yield return 0.2f;
        }
        public void Approach()
        {
            Speed.X = 90f;
        }
    }
}
