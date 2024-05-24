using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/Host")]
    [Tracked]
    public class Host : Entity
    {
        public Sprite Machine;
        public Sprite Flair;
        public Sprite Shine;
        public Image Cart;
        public Image Bag;
        public Sprite Hat;
        public Image Style;
        private Vector2 beamStart;
        private Vector2 beamEnd;
        private float beamAmount;
        private bool drawBeam;
        private float beamAlpha;
        private int beamThickness = 6;
        private float inBeamAlpha;
        private float throwTime = 1.2f;
        public VertexLight Light;
        private Vector2 returnTo;
        public bool DestroyedCurtains;
        private bool hatRotato;
        public Image Inside;
        public bool HoldingCharge;
        public float SteamAmount;
        private float xShake;
        public Vector2 LastBagPosition;
        private MTexture cartFrame => GFX.Game["objects/PuzzleIslandHelper/gameshow/host/cart0" + ((int)Position.X % 4)];

        public Host(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            string path = "objects/PuzzleIslandHelper/gameshow/host/";
            Depth = 2;

            Add(Flair = new Sprite(GFX.Game, path));
            Flair.AddLoop("idle", "flair", 0.1f);
            Flair.Play("idle");
            Flair.Position.X = -Flair.Width + 1;

            Add(Inside = new Image(GFX.Game[path + "inside"]));
            Inside.Color = Color.Black;
            Add(Machine = new Sprite(GFX.Game, path));
            Machine.AddLoop("idle", "machine", 0.1f);
            Machine.Add("steamIn", "door", 0.1f, "open", 0, 1, 2, 3, 4);
            Machine.AddLoop("openIdle", "door", 0.1f, 7);
            Machine.Add("open", "door", 0.07f, "openIdle", 5, 6);
            Machine.AddLoop("holdCharge", "charge", 0.1f, 5, 6, 7, 8);
            Machine.Add("charge", "charge", 0.07f, "holdCharge");
            Machine.Add("steamOut", "door", 0.1f, "idle", 0, 1, 2, 3, 4);
            Machine.Add("close", "door", 0.1f, "steamOut", 8);

            Machine.Play("idle");
            Machine.OnChange = (string s1, string s2) =>
            {
                HoldingCharge = false;
                if (s2 == "open")
                {
                    Shine.Visible = true;
                }
                if (s2 == "holdCharge")
                {
                    HoldingCharge = true;
                }
            };
            Add(Hat = new Sprite(GFX.Game, path));
            Hat.AddLoop("idle", "hat", 0.1f);
            Hat.Play("idle");
            Hat.CenterOrigin();
            Hat.Position += new Vector2(Machine.Width / 2, -Hat.Height / 2);

            Add(Shine = new Sprite(GFX.Game, path));
            Shine.AddLoop("shine", "shine", 0.1f);
            Shine.Position = new Vector2(15, -2);
            Shine.Play("shine");
            Shine.Visible = false;
            returnTo = Machine.RenderPosition;

            Add(Bag = new Image(GFX.Game["objects/PuzzleIslandHelper/gameshow/host/flammable"]));
            Bag.CenterOrigin();
            Bag.Position += new Vector2(Bag.Width / 2, Bag.Height / 2) + Vector2.One * 20;
            Bag.Visible = false;

            Add(Cart = new Image(cartFrame));
            Cart.RenderPosition = Machine.RenderPosition + new Vector2(-1, Machine.Height);

            Add(Style = new Image(GFX.Game[path + "style"]));
            Style.Position -= new Vector2(8, 18);
            Style.Visible = false;

            Collider = new Hitbox(Machine.Width, Machine.Height);
            Add(Light = new VertexLight(Color.White, 1, 32, 64));
            Light.Position += Vector2.One * 24;
            Visible = false;

            Add(new Coroutine(Shaker()));

        }
        private IEnumerator Shaker()
        {
            while (true)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime / Calc.Max(0.1f, 0.5f - SteamAmount))
                {
                    xShake = SteamAmount * i;
                    yield return null;
                }
                xShake = SteamAmount;
                for (float i = 0; i < 1; i += Engine.DeltaTime / Calc.Max(0.1f, 0.5f - SteamAmount))
                {
                    xShake = SteamAmount * (1 - i);
                    yield return null;
                }
                xShake = 0;
                yield return null;
            }
        }
        public IEnumerator Skedaddle()
        {
            float target = (Scene as Level).Marker("hostRunTo").X - Position.X;
            returnTo = Position;
            hatRotato = false;
            //todo: play cartoon run away sound
            while (Position.X != target)
            {
                Position.X = Calc.Approach(Position.X, target, 120 * Engine.DeltaTime);
                yield return null;
            }
            yield return null;
        }
        private IEnumerator ChargeRoutine()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                SteamAmount = i;
                yield return null;
            }
            while (!HoldingCharge) yield return null;
            while (HoldingCharge)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                {
                    SteamAmount = Calc.LerpClamp(1, 0.5f, i);
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                {
                    SteamAmount = Calc.LerpClamp(0.5f, 1f, i);
                    yield return null;
                }
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                SteamAmount = Calc.LerpClamp(SteamAmount, 0f, i);
                yield return null;
            }
            yield return null;
        }
        public void Steam()
        {
            Machine.Play("charge");
            Add(new Coroutine(ChargeRoutine()));
        }

        public override void Render()
        {
            Cart.Texture = cartFrame;
            Position.X += xShake;
            base.Render();
            Position.X -= xShake;
        }
        public void DrawBeam()
        {
            if (drawBeam)
            {
                Vector2 end = Vector2.Lerp(beamStart, beamEnd, beamAmount);
                Vector2 start = Vector2.Lerp(beamStart, beamEnd, Calc.Max(beamAmount - 0.2f, 0));
                Draw.Line(start, end, Color.Black * beamAlpha, beamThickness + 2);
                Draw.Line(start, end, Color.Red * beamAlpha, beamThickness);
                if (inBeamAlpha > 0)
                {
                    Draw.Line(start, end, Color.Lerp(Color.Red, Color.White, 0.5f) * inBeamAlpha, Calc.Max(beamThickness - 2, 1));
                }
            }
        }
        private IEnumerator KnockBack()
        {
            float start = Position.X;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
            {
                Position.X = Calc.LerpClamp(start, start - 8, Ease.SineOut(i));
                yield return null;
            }
            Shine.Visible = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
            {
                Position.X = Calc.LerpClamp(start - 8, start, i);
                yield return null;
            }
            Position.X = start;
        }
        private IEnumerator ThrowBag()
        {
            BezierCurve curve = new(0.11f, 1.97f, 0.68f, 2.07f);
            Vector2 start = Machine.RenderPosition + new Vector2(Machine.Width / 2, Machine.Height / 2) - new Vector2(Bag.Width / 2, Bag.Height / 2);
            Vector2 end = SceneAs<Level>().Camera.Position + new Vector2(160, 90);
            float maxHeight = 72;
            Bag.Visible = true;
            beamEnd = end;
            beamStart = Shine.RenderPosition + Vector2.One * 5;
            float y = Bag.RenderPosition.Y;
            float x = Bag.RenderPosition.X;

            Tween alpha = Tween.Create(Tween.TweenMode.Oneshot, null, throwTime / 4, false);
            Tween beam = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, throwTime / 4, false);
            Tween yt2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineIn, throwTime / 2, false);
            Tween yt = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, throwTime / 2, true);
            Tween xt = Tween.Create(Tween.TweenMode.Oneshot, null, throwTime, true);

            yt2.OnUpdate = (Tween t) => { y = Calc.LerpClamp(start.Y - maxHeight, end.Y, t.Eased); };
            alpha.OnUpdate = (Tween t) => { beamAlpha = inBeamAlpha = t.Eased; };
            beam.OnUpdate = (Tween t) => { beamAmount = t.Eased; };

            yt.OnUpdate = (Tween t) => { y = Calc.LerpClamp(start.Y, start.Y - maxHeight, t.Eased); };
            xt.OnUpdate = (Tween t) =>
            {
                x = Calc.LerpClamp(start.X, end.X, t.Eased);
                Bag.Rotation += 1f.ToRad();
            };
            yt.OnComplete = (Tween t) => { yt2.Start(); alpha.Start(); };
            alpha.OnComplete = (Tween t) => { beam.Start(); };
            alpha.OnStart = (Tween t) => { drawBeam = true; };
            beam.OnStart = (Tween t) => { Add(new Coroutine(KnockBack())); };

            Add(yt, yt2, xt, alpha, beam);

            bool closing = false;
            while (xt.Eased < 1)
            {
                if (xt.Eased > 0.3f && !closing)
                {
                    Machine.Play("close");
                    closing = true;
                }
                Bag.RenderPosition = new Vector2(x, y);
                yield return null;
            }
            DestroyedCurtains = true;
            Bag.Visible = false;
            drawBeam = false;
        }
        public void Reveal()
        {
            Visible = true;
        }
        public void ThrowFlammable()
        {
            Add(new Coroutine(ThrowRoutine()));
        }
        public IEnumerator ThrowRoutine()
        {
            Machine.Play("steamIn");
            while (Machine.CurrentAnimationID != "openIdle")
            {
                yield return null;
            }
            yield return ThrowBag();
        }
        public override void Update()
        {
            if (Bag.Visible)
            {
                LastBagPosition = Bag.RenderPosition;
            }
            Cart.RenderPosition = Machine.RenderPosition + new Vector2(-1, Machine.Height);
            Inside.RenderPosition = Machine.RenderPosition;
            if (hatRotato)
            {
                Hat.Rotation += 5f.ToRad();
            }
            beamStart = Shine.RenderPosition + Vector2.One * 5;

            Inside.Color = Color.Lerp(Color.Black, Color.Red, SteamAmount);
            Machine.Color = Color.Lerp(Color.White, Color.Red, SteamAmount / 2);

            base.Update();
        }
        public IEnumerator RunOff()
        {
            yield return 0.5f;
            Flair.Visible = false;
            float target = (Scene as Level).Marker("hostRunTo").X - Position.X;
            returnTo = Machine.Position;
            hatRotato = true;
            while (Machine.Position.X != target)
            {
                Machine.Position.X = Calc.Approach(Machine.Position.X, target, 60 * Engine.DeltaTime);
                yield return null;
            }
            yield return null;
        }
        public IEnumerator Return()
        {
            yield return 0.3f;

            while (Machine.Position.X != returnTo.X)
            {
                Machine.Position.X = Calc.Approach(Machine.Position.X, returnTo.X, 30 * Engine.DeltaTime);

                yield return null;
            }
            hatRotato = false;
            Hat.Rotation = 0;
            Audio.Play("event:/PianoBoy/cha");
            Style.Visible = true;
            Flair.Visible = true;
            yield return 1;
            Style.Visible = false;
            yield return null;
        }
    }
}
