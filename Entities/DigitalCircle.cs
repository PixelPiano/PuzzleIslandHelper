using Celeste.Mod.Entities;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.IO.Ports;

// PuzzleIslandHelper.DecalEffects
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DigitalCircle")]
    public class DigitalCircle : Entity
    {
        private bool inEndSequence = false;
        private Sprite oval1;
        private Sprite oval2;
        private Sprite fill;
        private Sprite orbs;
        private Sprite outline;
        private Sprite text;
        public static float radius = 0;
        public static float beamLength = 0;
        public static float rotationAdd = 0;
        private EventInstance sfx;
        private Entity FG;
        private Level l;
        private bool inAudioRoutine = false;
        private bool playingAudio = false;
        private bool glitch = false;
        private float opacity = 0.5f;
        private Color color = Color.White;

        private float seed;
        private float timer = 0f;
        private float amount = 2f;
        private float amplitude = 20f;

        private Tween rotationAdder = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineIn, 5, false);
        private static VirtualRenderTarget _OvalObject;
        public static VirtualRenderTarget OvalObject => _OvalObject ??= VirtualContent.CreateRenderTarget("OvalObject", 320, 180);
        private static VirtualRenderTarget _OvalObject2;
        public static VirtualRenderTarget OvalObject2 => _OvalObject2 ??= VirtualContent.CreateRenderTarget("OvalObject2", 320, 180);
        private static VirtualRenderTarget _FGTextures;
        public static VirtualRenderTarget FGTextures => _FGTextures ??= VirtualContent.CreateRenderTarget("FGTextures", 320, 180);
        public DigitalCircle(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            color = data.HexColor("color", Color.Green);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            rotationAdd = 0f;
            Depth = 9002;
            Add(oval1 = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/circle/"));
            Add(oval2 = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/circle/"));
            Add(fill = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/circle/"));

            oval1.AddLoop("idle", "oval", 1f);
            oval2.AddLoop("idle", "oval", 1f);
            fill.AddLoop("idle", "fill", 1f);
            oval1.Visible = false;
            oval2.Visible = false;
            oval1.JustifyOrigin(new Vector2(0.5f, 0.5f));
            oval2.JustifyOrigin(new Vector2(0.5f, 0.5f));
            fill.JustifyOrigin(new Vector2(0.5f, 0.5f));
            Position += new Vector2(oval1.Width / 3, oval1.Height / 2 - 2);
            Collider = new Hitbox(oval1.Width, oval1.Height);

            scene.Add(FG = new Entity(Position));

            FG.Depth = Depth - 1;
            FG.Add(text = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/circle/"));
            FG.Add(orbs = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/circle/"));
            FG.Add(outline = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/circle/"));
            text.AddLoop("idle", "inscription", 1f);
            orbs.AddLoop("idle", "orbs", 1f);
            outline.AddLoop("idle", "outline", 1f);
            orbs.JustifyOrigin(new Vector2(0.5f, 0.5f));
            text.JustifyOrigin(new Vector2(0.5f, 0.5f));
            outline.JustifyOrigin(new Vector2(0.5f, 0.5f));
            text.Position.X += 1;

            text.Visible = false;
            orbs.Visible = false;
            outline.Visible = false;
            fill.Visible = false;
            orbs.Play("idle");
            fill.Play("idle");
            outline.Play("idle");
            text.Play("idle");
            oval1.Play("idle");
            oval2.Play("idle");

            rotationAdder.OnUpdate = (Tween t) => 
            {
                rotationAdd = MathHelper.Lerp(0, 360, t.Eased);
            };
            Add(new Coroutine(ScaleY(), true));

        }

        public override void Render()
        {
            base.Render();
            if (Scene is not Level)
            {
                return;
            }
            l = Scene as Level;
            //Draw.Circle(Position, 30, Color.Red, 1, 20);
            Draw.SpriteBatch.End();
            #region Sprite 1
            Engine.Graphics.GraphicsDevice.SetRenderTarget(OvalObject);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            oval1.Render();
            Draw.SpriteBatch.End();
            #endregion
            Engine.Graphics.GraphicsDevice.SetRenderTarget(FGTextures);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            fill.Render();
            text.Render();
            orbs.Render();
            outline.Render();
            Draw.SpriteBatch.End();
            #region Sprite 2
            Engine.Graphics.GraphicsDevice.SetRenderTarget(OvalObject2);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            oval2.Render();
            Draw.SpriteBatch.End();
            #endregion
            if (glitch)
            {
                var glitchSave = Glitch.Value;
                Glitch.Value = amount;
                Glitch.Apply(OvalObject, timer, seed, amplitude);
                Glitch.Apply(OvalObject2, timer, seed, amplitude);
                Glitch.Apply(FGTextures, timer, seed, amplitude);
                Glitch.Value = glitchSave;
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            GameplayRenderer.Begin();
            Draw.SpriteBatch.Draw(OvalObject, l.Camera.Position, color * opacity);
            Draw.SpriteBatch.Draw(OvalObject2, l.Camera.Position, color * opacity);
            Draw.SpriteBatch.Draw(FGTextures, l.Camera.Position, Color.White);
        }
        private IEnumerator AudioAndWait()
        {
            inAudioRoutine = true;

            sfx = Audio.Play("event:/PianoBoy/jelly/colorCodeSequence");
            sfx.getPlaybackState(out PLAYBACK_STATE state);
            while (state != PLAYBACK_STATE.STOPPING && state != PLAYBACK_STATE.STOPPED)
            {
                playingAudio = true;
                sfx.getPlaybackState(out state);
                yield return null;
                if (state == PLAYBACK_STATE.SUSTAINING)
                {
                    //do burst
                    sfx.triggerCue();
                    SceneAs<Level>().Displacement.AddBurst(TopLeft, 2f, 0, outline.Width / 2, 1);
                    yield return null;
                    SceneAs<Level>().Displacement.AddBurst(TopLeft, 1.5f, 0, SceneAs<Level>().Bounds.Width, 0.1f);
                    yield return null;
                }
                sfx.getPlaybackState(out state);
                yield return null;
            }
            SceneAs<Level>().Session.SetFlag("colorCodeAudio", false);
            inAudioRoutine = false;
            playingAudio = false;
            yield return null;
        }
        public override void Update()
        {
            base.Update();
            if (Scene as Level == null)
            {
                return;
            }
            oval1.Rotation += 0.03f + rotationAdd;
            oval2.Rotation += 0.05f + rotationAdd;
            text.Rotation += 0.01f + rotationAdd;
            fill.Rotation += rotationAdd;
            outline.Rotation += rotationAdd;
            orbs.Rotation += rotationAdd;
            timer += Engine.DeltaTime;
            seed = Calc.Random.NextFloat();
            if (!playingAudio && !inAudioRoutine && SceneAs<Level>().Session.GetFlag("colorCodeAudio") && !SceneAs<Level>().Session.GetFlag("colorCode"))
            {
                Add(new Coroutine(AudioAndWait(), true));
            }
            if (SceneAs<Level>().Session.GetFlag("colorCode") && !inEndSequence)
            {
                Add(new Coroutine(OnComplete(), true));
            }
        }
        private IEnumerator OnComplete()
        {
            inEndSequence = true;
            for(float i = 0; i<1; i+=0.002f)
            {
                rotationAdd = Calc.LerpClamp(0, 0.75f, Ease.SineIn(i));
                radius = Calc.LerpClamp(0,30, Ease.SineIn(i)*2);
                beamLength = Calc.LerpClamp(0, 320, Ease.SineIn(i));
                yield return null;
            }
            float _radius = radius;
            for (float i = 0; i < 1; i += 0.01f)
            {
                beamLength = Calc.LerpClamp(320, 0, Ease.SineIn(i)*2);
                radius = Calc.LerpClamp(_radius, 200, Ease.CubeIn(i));
                yield return null;
            }
            glitch = true;
            SceneAs<Level>().Session.SetFlag("colorCodeGrade", true);
            yield return 1;
            RemoveSelf();
            yield return null;
        }
        private IEnumerator ScaleY()
        {
            while (true)
            {
                for (float i = 0; i < 1; i += 0.01f)
                {
                    oval1.Scale.Y = Calc.LerpClamp(1, 0, Ease.SineIn(i));
                    oval2.Scale.Y = Calc.LerpClamp(1, 0, Ease.SineIn(i));
                    yield return null;
                }
                for (float i = 0; i < 1; i += 0.01f)
                {
                    oval1.Scale.Y = Calc.LerpClamp(0, 1, Ease.SineIn(i));
                    oval2.Scale.Y = Calc.LerpClamp(0, 1, Ease.SineIn(i));
                    yield return null;
                }
            }
        }
    }
}
