using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;



namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/LifeDisplay")]
    [Tracked]
    public class LifeDisplay : Entity
    {
        public const int MaxLives = 2;
        public Sprite[] Lives = new Sprite[MaxLives];
        public Image Image;
        private int dead;
        public bool GameOver;
        public LifeDisplay(EntityData data, Vector2 offset) : this(data.Position + offset) { }
        public LifeDisplay(Vector2 position) : base(position)
        {
            Depth = 4;
            Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/gameshow/lifeDisplay/marioxhk_monitor"]));
            for (int i = 0; i < MaxLives; i++)
            {
                Lives[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/gameshow/playerLife/");
                Lives[i].AddLoop("dead", "funnyExplosion", 0.1f, 14);
                Lives[i].AddLoop("alive", "funnyExplosion", 0.1f, 0);
                Lives[i].Add("explode", "funnyExplosion", 0.1f, "dead");
                Lives[i].Play("alive");
                float w = Lives[i].Width;
                Lives[i].X = (Image.Width / 2) - (w / 2) - w + (w * i);
            }
            Add(Lives);
            TurnOff();
            Collider = new Hitbox(Image.Width, Image.Height);
        }
        public void ConsumeLife()
        {
            if (dead < MaxLives)
            {
                Lives[dead].Play("explode");
                Audio.Play("event:/PianoBoy/funnyExplosion");
                dead++;
                if (dead == MaxLives)
                {
                    GameOver = true;
                }
            }

        }

        public void TurnOn()
        {
            for (int i = 0; i < MaxLives; i++)
            {
                Lives[i].Visible = true;
            }
        }
        public void TurnOff()
        {
            for (int i = 0; i < MaxLives; i++)
            {
                Lives[i].Visible = false;
            }
        }
    }
}
