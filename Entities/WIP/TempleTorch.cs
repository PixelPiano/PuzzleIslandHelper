using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/TempleTorch")]
    [Tracked]

    public class TempleTorch : Entity
    {
        public VertexLight Light;
        public BloomPoint Bloom;
        public Sprite Sprite;
        public Image Base;
        public bool On;
        private string flag;
        private bool isEmpty;
        private float startFade;
        private float endFade;
        public TempleTorch(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 10000;
            startFade = 64;
            endFade = 120;
            On = data.Bool("lit");
            flag = data.Attr("flag");
            isEmpty = string.IsNullOrEmpty(flag);
            Add(Base = new Image(GFX.Game["objects/PuzzleIslandHelper/templeTorch/base"]));
            Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/templeTorch/"));

            Sprite.AddLoop("idle", "flame", 0.1f);
            Sprite.Add("ignite", "ignite", 0.1f, "idle");
            Base.Position.Y = Sprite.Height - Base.Height;
            Add(Light = new VertexLight(Color.White, 0.9f, (int)startFade, (int)endFade));
            Light.InSolidAlphaMultiplier = 0.9f;
            Add(Bloom = new BloomPoint(0.5f, 16));
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
            Light.Visible = false;
            Sprite.Visible = false;
            Bloom.Visible = false;
            Light.Position = Bloom.Position = Collider.HalfSize;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (On)
            {
                Light.Visible = true;
                Bloom.Visible = true;
                Sprite.Visible = true;
                Sprite.Play("idle");
            }
        }
        public override void Update()
        {
            base.Update();
            if (isEmpty) return;
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                if (!On) StartLight();
            }
            else if (On)
            {
                StopLight();
            }
        }
        public void StopLight()
        {
            On = false;
            Sprite.Visible = false;
            Components.RemoveAll<Tween>();
            float alphaFrom = Light.Alpha;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 1, true);
            tween.OnUpdate = t =>
            {
                Light.Alpha = Calc.LerpClamp(alphaFrom, 0, t.Eased);
            };
            tween.OnComplete = t =>
            {
                Light.Visible = false;
                Light.Alpha = 1;
            };
            Add(tween);
        }
        public void StartLight()
        {
            Add(new Coroutine(turnOn()));
        }
        public void SetAt(float lerp)
        {
            Light.Alpha = Calc.LerpClamp(0.3f, 1, lerp);
            Light.StartRadius = startFade + (1f - lerp) * 32f;
            Light.EndRadius = endFade + (1f - lerp) * 32f;
        }
        private IEnumerator turnOn()
        {
            Light.Alpha = 0;
            Sprite.Visible = Light.Visible = Bloom.Visible = true;
            Sprite.Play("ignite");
            yield return null;
            Components.RemoveAll<Tween>();
            On = true;
            //TODO: Make SHEEEN sound for flame igniting
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.6f)
            {
                SetAt(Calc.LerpClamp(0.5f, 1, Ease.BackOut(i)));
                yield return null;
            }
        }
    }
}