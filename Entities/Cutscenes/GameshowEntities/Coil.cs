using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/GameshowCoil")]
    [Tracked]
    public class GameshowCoil : Solid
    {
        public int TotalHits;
        public bool InPlay;
        public int Flashes;
        public float Interval;
        public string Flag;
        public List<Image> Images = new();
        public GameshowCoil(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, 8, false)
        {
            OnDashCollide = DashCollide;
            Flashes = data.Int("flashes");
            Interval = data.Float("interval");
            Flag = data.Attr("flag");
            InPlay = true;
            for (int i = 0; i < Width; i += 8)
            {
                Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/gameshow/coil"]);
                image.X = i;
                Add(image);
                Images.Add(image);
            }
        }
        public override void Render()
        {
            foreach (Image i in Images)
            {
                i.DrawSimpleOutline();
            }
            base.Render();
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            foreach (Image i in Images)
            {
                i.Position += amount;
            }
        }
        public DashCollisionResults DashCollide(Player player, Vector2 direction)
        {
            if (InPlay)
            {
                Add(new Coroutine(OnDash()));
                StartShaking(0.3f);
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }
        public IEnumerator OnDash()
        {
            if (Scene is not Level level) yield break;
            for (int i = 0; i < Flashes; i++)
            {
                level.Session.SetFlag(Flag);
                yield return Interval;
                level.Session.SetFlag(Flag, false);
                yield return Interval;
            }
            TotalHits++;

            if (TotalHits == 3)
            {
                //level.Add(new GameshowBreakCutscene());
            }
        }
    }
}
