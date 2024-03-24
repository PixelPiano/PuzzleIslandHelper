using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.EightSwitch
namespace Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities
{
    [CustomEntity("PuzzleIslandHelper/EightSwitch")]
    public class EightSwitch : Solid
    {

        private Player player;
        private Sprite[] sprites = new Sprite[8];
        private Sprite[] lights = new Sprite[8];
        private bool[] lightOn = new bool[8];
        private float spacing = 2f;
        private bool buttonValid = false;
        private ColliderList list = new ColliderList();
        private float pressAmount = 1;
        private bool PressingButton = false;
        private bool FadingLight = false;
        private readonly float lightY = -48;
        private int[,] placements = {
                                    {0, 1, 0, 1, 0, 1, 0, 0},
                                    {0, 1, 0, 1, 0, 0, 0, 1},
                                    {0, 0, 1, 1, 0, 0, 1, 0},
                                    {1, 0, 0, 1, 1, 0, 0, 0},
                                    {0, 1, 0, 0, 0, 1, 1, 0},
                                    {0, 0, 1, 1, 0, 1, 0, 0},
                                    {1, 0, 0, 0, 1, 1, 0, 0},
                                    {0, 0, 1, 0, 0, 1, 0, 1}};
        private bool PuzzleClear = false;
        private void CreateButtons()
        {
            for (int i = 0; i < 8; i++)
            {
                Add(sprites[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/eightSwitch/"));
                Add(lights[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/eightSwitch/"));
                lights[i].AddLoop("idle", "light", 0.1f);
                sprites[i].AddLoop("idle", "buttonIdle", 0.1f);

                sprites[i].Position.X = (sprites[i].Width + spacing) * i;
                lights[i].Position.X = (sprites[i].Width + spacing + 1) * i;
                lights[i].Position.Y = lightY;
                lights[i].Position.X -= 6;
                lights[i].Play("idle");
                sprites[i].Play("idle");
                list.Add(new Hitbox(sprites[i].Width, sprites[i].Height, sprites[i].Position.X, 0));
            }
            Collider = list;
        }
        public EightSwitch(EntityData data, Vector2 offset)
        : base(data.Position + offset, 160, 9, false)
        {
            Depth = 1;
            OnDashCollide = OnDashed;
        }
        private void AdjustButtonPosition(float amount)
        {
            for (int i = 0; i < 8; i++)
            {
                list.colliders[i].Position.Y = sprites[i].Position.Y = i == GetButton() && buttonValid ? Calc.Approach(amount, 0, Engine.DeltaTime) : Calc.Approach(0, amount, Engine.DeltaTime);
            }
            Collider = list;
        }
        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (direction != new Vector2(0, 1) || PuzzleClear)
            {
                return DashCollisionResults.NormalCollision;
            }
            if (!PressingButton)
            {
                Add(new Coroutine(ButtonPressed(), true));
            }
            return DashCollisionResults.Rebound;
        }
        private IEnumerator FadeLight(int a)
        {
            if (a < 0 || a > 7 || FadingLight)
            {
                yield break;
            }
            FadingLight = true;
            for (int i = 0; i < 8; i++)
            {
                if (lightOn[i])
                {
                    for (float j = 0; j < 1; j += 0.1f)
                    {
                        if (placements[a, i] == 1)
                        {
                            lights[i].SetColor(Color.Lerp(Color.Yellow, Color.White, j));
                            lightOn[i] = false;
                            yield return null;
                        }
                    }
                }
                else
                {
                    for (float j = 0; j < 1; j += 0.1f)
                    {
                        if (placements[a, i] == 1)
                        {
                            lights[i].SetColor(Color.Lerp(Color.White, Color.Yellow, j));
                            lightOn[i] = true;
                            yield return null;
                        }
                    }
                }
                yield return null;
            }
            PuzzleClear = AllOn();
            FadingLight = false;
            yield return null;
        }
        private bool AllOn()
        {
            bool result = true;
            for (int i = 0; i < 8; i++)
            {
                if (!lightOn[i])
                {
                    result = false;
                }
            }
            return result;
        }
        private IEnumerator ButtonPressed()
        {
            PressingButton = true;
            int index = GetButton();
            Add(new Coroutine(FadeLight(index), true));
            bool indexValid = index >= 0 && index < 8 && player.Position.Y > Position.Y - 4 && player.Position.X >= Left && player.Position.X <= Right;
            for (float j = 0; j < 1; j += 0.2f)
            {
                if (indexValid)
                {
                    list.colliders[index].Position.Y = sprites[index].Position.Y = Calc.Approach(0, 6, 6 * j);
                }
                Collider = list;
                yield return null;
            }
            for (float j = 0; j < 1; j += 0.2f)
            {
                if (indexValid)
                {
                    list.colliders[index].Position.Y = sprites[index].Position.Y = Calc.Approach(6, 0, 6 * j);
                }
                Collider = list;
                yield return 0.03f;
            }
            PressingButton = false;
            yield return null;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            CreateButtons();
        }
        public int GetButton()
        {
            int output = -1;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                return output;
            }
            if (player.Center.X >= Left
                && player.Center.X <= Right)
            {
                output = (int)(player.Center.X - Left) / (int)(sprites[0].Width + spacing);
            }

            return output;
        }

        public override void Update()
        {
            base.Update();
            player = Scene.Tracker.GetEntity<Player>();
            if (player == null || PuzzleClear)
            {
                return;
            }
            buttonValid = GetButton() >= 0 && GetButton() < 8 && player.Position.Y > Position.Y - 4 && player.Position.X >= Left && player.Position.X <= Right;
            if (!PressingButton && !player.DashAttacking)
            {
                AdjustButtonPosition(pressAmount);
            }
        }
    }
}