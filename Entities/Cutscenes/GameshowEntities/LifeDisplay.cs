using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;



namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/LifeDisplay")]
    [Tracked]
    public class LifeDisplay : Entity
    {
        public Sprite[] Lives = new Sprite[5];
        public Image Image;
        public int Dead;
        public bool GameOver;
        public LifeDisplay(EntityData data, Vector2 offset) : this(data.Position + offset) { }
        public LifeDisplay(Vector2 position) : base(position)
        {
            Image = new Image(GFX.Game["objects/PuzzleIslandHelper/gameshow/lifeDisplay/marioxhk_monitor"]);
            Add(Image);
            Vector2 start = new Vector2(4, 0);
            int space = (int)((Image.Width - 26) / 5);
            for (int i = 0; i < 5; i++)
            {
                float offset = i * (space + 4);
                Lives[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/gameshow/playerLife/");
                Lives[i].AddLoop("dead", "funnyExplosion", 0.1f, 14);
                Lives[i].AddLoop("alive", "funnyExplosion", 0.1f, 0);
                Lives[i].Add("explode", "funnyExplosion", 0.1f, "dead");
                Add(Lives[i]);
                Lives[i].Play("alive");
                Lives[i].Position = start + Vector2.UnitX * offset;
                Lives[i].Visible = false;
            }
            Collider = new Hitbox(Image.Width, Image.Height);
        }
        public void ConsumeLife()
        {
            for (int i = Dead; i < Lives.Length; i++)
            {
                Lives[i].Play("explode");
                Audio.Play("event:/PianoBoy/funnyExplosion");
                Dead++;
                return;
            }
            GameOver = true;
        }

        public void TurnOn()
        {
            for (int i = 0; i < 5; i++)
            {
                Lives[i].Visible = true;
            }
        }
        public void TurnOff()
        {
            for (int i = 0; i < 5; i++)
            {
                Lives[i].Visible = false;
            }
        }
    }
}
