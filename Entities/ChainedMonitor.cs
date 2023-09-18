using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ChainedMonitor")]
    [Tracked]
    public class ChainedMonitor : Entity
    {
        private Sprite Monitor;
        private Sprite Chains;
        private Sprite Block;
        private Sprite Padlock;
        private string flag;
        private string flagOnComplete;
        private bool broken;
        private float delay = 1 / 12f;
        public ChainedMonitor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = 1;
            flag = data.Attr("flag");
            flagOnComplete = data.Attr("flagOnComplete");

            Add(Monitor = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chains/"));
            Add(Chains = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chains/"));
            Add(Block = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chains/invertedUnlock/"));
            Add(Padlock = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chains/padlock/"));

            Monitor.AddLoop("idle", "monitorLonger", 1f);

            Chains.AddLoop("idle", "doubleGreen", delay);
            Chains.Add("break", "chainsFadeOut", delay);

            Block.AddLoop("idle", "inverted", 1f, 0);
            Block.Add("break", "inverted", delay);

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
            if (!SceneAs<Level>().Session.GetFlag(flagOnComplete))
            {
                Chains.Play("idle");
                Block.Play("idle");
                Padlock.Play("idle");
            }
        }
        private IEnumerator Sequence()
        {
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
            SceneAs<Level>().Session.SetFlag(flag, false);
        }
        public override void Update()
        {
            base.Update();
            if (!SceneAs<Level>().Session.GetFlag(flagOnComplete) && SceneAs<Level>().Session.GetFlag(flag))
            {
                InvertOverlay.ForceState(true);
                Add(new Coroutine(Sequence()));
                SceneAs<Level>().Session.SetFlag(flagOnComplete);

            }
            
        }
    }
}