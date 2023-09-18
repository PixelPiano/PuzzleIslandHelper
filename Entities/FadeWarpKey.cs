using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
// PuzzleIslandHelper.FadeWarpKey
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FadeWarpKey")]
    [Tracked]
    public class FadeWarpKey : Entity
    {
        public int id;
        public bool Collected;
        private bool Standing;
        private Sprite sprite;
        public FadeWarpKey(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Depth = data.Int("depth");
            id = data.Int("keyId");
            Standing = data.Bool("standing");
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/fadeWarp/"));
            sprite.AddLoop("idle", Standing ? "key(Standing)" : "key(Flat)", 1f);
            Collider = new Hitbox(sprite.Width, sprite.Height);
            Add(new TalkComponent(new Rectangle(0,0,(int)sprite.Width,(int)sprite.Height), new Vector2(sprite.Width/2, 0), Collect));
            sprite.Play("idle");
        }
        public struct KeyData
        {
            public int id;
            public KeyData(FadeWarpKey Key)
            {
                id = Key.id;
            }
            public override string ToString()
            {
                return $"id of key is {id}.";
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            bool condition1 = false;
            bool condition2 = false;
            foreach(FadeWarp warp in (scene as Level).Tracker.GetEntities<FadeWarp>())
            {
                if(warp.isDoor && warp.keyId == id)
                {
                    condition1 = true;
                    break;
                }
            }
            foreach(KeyData data in PianoModule.Session.Keys)
            {
                if(data.id == id)
                {
                    condition2 = true;
                    break;
                }
            }
            if ((condition1 && condition2) || condition2)
            {
                RemoveSelf();
            }
        }

        private void Collect(Player player)
        {
            //Play ding sound or something
            player.StateMachine.State = 11;
            Collected = true;
            sprite.Visible = false;
            KeyData data = new KeyData(this);
            if(!PianoModule.Session.Keys.Contains(data))
            {
                PianoModule.Session.Keys.Add(data);
            }
            foreach(KeyData key in PianoModule.Session.Keys)
            {
                Console.WriteLine(key.ToString());
            }
            player.StateMachine.State = 0;
            RemoveSelf();
        }
    }
}