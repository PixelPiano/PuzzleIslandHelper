using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomFlagExitBlock")]
    [Tracked]
    public class CustomFlagExitBlock : ExitBlock
    {
        private TileGrid newTiles;
        public bool forceChange = false;
        public bool forceState = false;
        private EffectCutout newCutout;
        private readonly string flag;
        private readonly bool inverted;
        private readonly bool playSound;
        private readonly bool instant;
        private string audio = "";
        private Level l;
        private float timer;
        private float seed;
        private bool glitch = false;
        private bool canChange = false;
        private float glitchLimit = 0;
        private float max = 30;
        private bool inRoutine = false;
        private bool forceGlitch = false;
        private static VirtualRenderTarget _Tiles;
        public static VirtualRenderTarget Tiles => _Tiles ??= VirtualContent.CreateRenderTarget("Tiles", 320, 180);

        public CustomFlagExitBlock(EntityData data, Vector2 offset) : base(data, offset)
        {
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            playSound = data.Bool("playSound");
            instant = data.Bool("instant");
            audio = data.Attr("audioEvent", "event:/game/general/passage_closed_behind");
            forceGlitch = data.Bool("forceGlitchEffect");
            // I'm not sure what this transition listener is for.
            Remove(Get<TransitionListener>());
        }

        // In regular C# code we can't just call the parent's base method...
        // but with MonoMod magic we can do it anyway.
        [MonoModLinkTo("Celeste.Solid", "System.Void Update()")]
        public void base_Update()
        {
            base.Update();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            // get some variables from the parent class.
            DynData<ExitBlock> self = new DynData<ExitBlock>(this);
            newTiles = self.Get<TileGrid>("newTiles");
            newCutout = self.Get<EffectCutout>("newCutout");

            // hide the block if the flag is initially inactive.
            if (SceneAs<Level>().Session.GetFlag(flag) == inverted)
            {
                newCutout.Alpha = newTiles.Alpha = 0f;
                Collidable = false;
            }
        }

        public override void Update()
        {
            base_Update();

            bool wasCollidable = Collidable;
            bool isCollidable = SceneAs<Level>().Session.GetFlag(flag) != inverted && !CollideCheck<Player>();
            timer += Engine.DeltaTime;
            seed = Calc.Random.NextFloat();
            // the block is only collidable if the flag is set.
            glitch = (!wasCollidable && isCollidable) || (wasCollidable && !isCollidable);
            if(glitch && !inRoutine && !forceChange)
            {
                Add(new Coroutine(GlitchIncrement(isCollidable), true));
            }
            if (!glitch && playSound && !wasCollidable && isCollidable)
            {
                Audio.Play(audio, base.Center);
            }
            if (forceChange && !inRoutine)
            {
                Add(new Coroutine(GlitchIncrement(forceState), true));
            }

            // fade the block in or out depending on its enabled status.
            newCutout.Alpha = newTiles.Alpha = Calc.Approach(newTiles.Alpha, Collidable ? 1f : 0f, instant ? 1f : Engine.DeltaTime);
        }
        private IEnumerator GlitchIncrement(bool state)
        {
            Collidable = state;
            inRoutine = true;
            if (playSound)
            {
                Audio.Play(audio,Center);
            }
            glitchLimit = 0;
            while(glitchLimit < max)
            {
                glitchLimit += 1;
                yield return null;
            }
            inRoutine = false;
            newTiles.Visible = state;
        }
        public override void Render()
        {
            base.Render();
            if(Scene as Level == null || (!inRoutine && !forceGlitch))
            {
                return;
            }
            l = Scene as Level;
            Draw.SpriteBatch.End();
            #region Sprite 1
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Tiles);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            Draw.Rect(Collider, Color.Green);
            Draw.SpriteBatch.End();
            #endregion

            var glitchSave = Glitch.Value;
            Glitch.Value = 1;
            Glitch.Apply(Tiles, timer, seed, 20);
            Glitch.Value = glitchSave;

            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            GameplayRenderer.Begin();
            Draw.SpriteBatch.Draw(Tiles, l.Camera.Position, Color.White);
        }

    }
}