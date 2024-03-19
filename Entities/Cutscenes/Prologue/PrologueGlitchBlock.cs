using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [CustomEntity("PuzzleIslandHelper/PrologueGlitchBlock")]
    [Tracked]
    public class PrologueGlitchBlock : CustomFlagExitBlock
    {

        public const string glitchEvent = "event:/PianoBoy/invertGlitch2";
        public bool Activated;
        public PrologueGlitchBlock(Vector2 position, float width, float height, char tileType, string flag) : base(position, width, height, tileType, flag, false, true, true, glitchEvent, false)
        {
        }
        public PrologueGlitchBlock(PrologueBird bird) : this(bird.StartPosition - Vector2.One * 16, 32, 40, 'Q', "birdBlock")
        {

        }
        public PrologueGlitchBlock(Player player) : this(player.Position - Vector2.One * 22, 48, 56, 'Q', "playerBlock")
        {
        }
        public PrologueGlitchBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, 'Q', "", false, true, true, glitchEvent, true)
        {
            forceChange = true;
        }
        public void Activate()
        {
            Activated = true;
            if (!PianoModule.Session.ActiveGlitchBlocks.Contains(this))
            {
                PianoModule.Session.ActiveGlitchBlocks.Add(this);
            }
            else
            {
                return;
            }

            Collidable = true;
            forceGlitch = true;
            forceState = true;
            Add(new Coroutine(PrologueGlitchIncrement(), true));
            timer += Engine.RawDeltaTime;
            seed = Calc.Random.NextFloat();
            Audio.Play(audio, base.Center);
            newCutout.Alpha = newTiles.Alpha = 1;
        }
        public IEnumerator PrologueGlitchIncrement()
        {
            inRoutine = true;
            newCutout.Alpha = newTiles.Alpha = 1;
            glitchLimit = 0;
            while (glitchLimit < max)
            {
                glitchLimit += 1;
                yield return null;
            }
            inRoutine = false;
            newTiles.Visible = true;
        }

        public override void Update()
        {
            Collidable = Activated;
            forceState = Activated;
            forceGlitch = Activated;
            base_Update();
            timer += Engine.RawDeltaTime;
            seed = Calc.Random.NextFloat();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (PianoModule.Session.ActiveGlitchBlocks.Contains(this))
            {
                Activated = true;
                Add(new Coroutine(PrologueGlitchIncrement(), true));
            }
        }
    }
}
