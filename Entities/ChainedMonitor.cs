using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities
{
    [CustomEntity("PuzzleIslandHelper/ChainedMonitor")]
    [Tracked]
    public class ChainedMonitor : Entity
    {
        private Sprite Monitor;
        private Sprite Chains;
        private Sprite Block;
        private Sprite Padlock;
        private Sprite Shutter;
        private bool inCutscene;
        private DotX3 Talk;
        public bool Usable
        {
            get
            {
                return PianoModule.Session.HasInvert;
            }
        }
        public bool Used
        {
            get
            {
                return !PianoModule.Session.ChainedMonitorsActivated.Contains(SceneAs<Level>().Session.Level);
            }
        }
        private bool Pressed;
        private float delay = 1 / 12f;
        public ChainedMonitor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = 1;

            Add(Monitor = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chains/"));
            Add(Chains = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chains/"));
            Add(Block = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chains/invertedUnlock/"));
            Add(Padlock = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chains/padlock/"));
            Add(Shutter = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chains/"));

            Shutter.AddLoop("closed", "shuttersOpen", 0.1f, 0);
            //Shutter.AddLoop("open", "shuttersOpen", 0.1f, 22);
            //Shutter.Add("opening", "shuttersOpen", 0.07f, "open");

            Monitor.AddLoop("idle", "monitorLonger", 1f);

            Chains.AddLoop("idle", "doubleGreen", delay);
            Chains.Add("break", "chainsFadeOut", delay);

            Block.AddLoop("idle", "invertFlag", 1f, 0);
            Block.Add("break", "invertFlag", delay);

            Padlock.AddLoop("idle", "idle", delay);
            Padlock.Add("unlock", "unlock", delay);

            Chains.Position += new Vector2(4, -1);
            Block.Position += new Vector2(52, 18);
            Padlock.Position = Block.Position;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Depth = 1;
            Monitor.Play("idle");
            if (!Used)
            {
                Chains.Play("idle");
                Block.Play("idle");
                Padlock.Play("idle");
            }

            if (Usable)
            {
                Remove(Shutter);
            }
            else
            {
                Shutter.Play("closed");
            }
        }
        private IEnumerator Sequence()
        {
            inCutscene = true;
            Chains.Play("break");
            Block.Play("break");
            Padlock.Play("unlock");
            yield return 2.3f;
            Audio.Play("event:/new_content/game/10_farewell/endscene_final_input");
            yield return 0.5f;
            Audio.Play("event:/game/02_old_site/lantern_hit");
            yield return null;
            InvertOverlay.ForceState(false);
            InvertOverlay.ResetState();
            PianoModule.Session.ChainedMonitorsActivated.Add(SceneAs<Level>().Session.Level);
        }
        public override void Update()
        {
            base.Update();
            if (Usable && !Used && InvertOverlay.State && !inCutscene)
            {
                InvertOverlay.ForceState(true);
                Add(new Coroutine(Sequence()));

            }

        }
    }
}