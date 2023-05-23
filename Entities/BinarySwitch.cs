using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.BinarySwitch
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BinarySwitch")]
    public class BinarySwitch : Solid
    {

        private Player player;
        private Sprite[] sprites = new Sprite[6];
        private Sprite[] lights = new Sprite[5];
        //private Sprite[] levels = new Sprite[17];
        private bool[] lightOn = new bool[5];
        private float spacing = 2f;
        private bool buttonValid = false;
        ColliderList list = new ColliderList();
        private float pressAmount = 1;
        private bool PressingButton = false;
        private bool FadingLight = false;
        private readonly float lightY = -48;
        private int[,] placements = {
                                    {0,0,0,0,0},
                                    {0,0,0,0,1},
                                    {0,0,0,1,0},
                                    {0,0,0,1,1},
                                    {0,0,1,0,0},
                                    {0,0,1,0,1},
                                    {0,0,1,1,0},
                                    {0,0,1,1,1},
                                    {0,1,0,0,0},
                                    {0,1,0,0,1},
                                    {0,1,0,1,0},
                                    {0,1,0,1,1},
                                    {0,1,1,0,0},
                                    {0,1,1,0,1},
                                    {0,1,1,1,0},
                                    {0,1,1,1,1},
                                    {1,0,0,0,0},};
        private bool PuzzleClear = false;
        private int level = 0;
        private bool clearingLevel = false;
        private bool EndRoutine = false;
        private bool VerifyState()
        {
            int lightState = 0;
            for (int i = 0; i < 5; i++)
            {
                lightState = lightOn[i] ? 1 : 0;
                if (lightState != placements[level, i])
                {
                    level = 0;
                    return false;
                }
            }
            level++;
            if(level == 17)
            {
                PuzzleClear = true;
            }
            return true;
        }
       /* private IEnumerator LevelColor(bool state)
        {
            while (clearingLevel)
            {
                yield return null;
            }
            Color[] colors = new Color[17];
            for (int i = 0; i < 17; i++)
            {
                colors[i] = levels[i].Color;
            }
            if (state)
            {
                for (float j = 0; j <= 1; j += 0.2f)
                {
                    levels[level].SetColor(Color.Lerp(Color.White, Color.LimeGreen, j));
                    yield return null;
                }
            }
            else
            {
                for (float j = 0; j <= 1; j += 0.2f)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        levels[i].SetColor(Color.Lerp(colors[i], Color.Red, j));
                    }
                    yield return null;
                }
                yield return 0.5f;
                for (float j = 0; j <= 1; j += 0.2f)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        levels[i].SetColor(Color.Lerp(Color.Red, Color.White, j));
                    }
                    yield return null;
                }
            }
        }*/
        private IEnumerator LevelClear()
        {
            clearingLevel = true;
            Color endColor = VerifyState() ? Color.LimeGreen : Color.Red;
            Color[] _colors = new Color[5];
            for (int j = 0; j < 5; j++)
            {
                _colors[j] = lights[j].Color;
            }
            for (int j = 0; j < 5; j++)
            {
                for (float i = 0; i < 1; i += 0.2f)
                {
                    lights[j].SetColor(Color.Lerp(_colors[j], endColor, i));
                    yield return null;
                }
                for (float i = 0; i < 1; i += 0.2f)
                {
                    lights[j].SetColor(Color.Lerp(endColor, _colors[j], i));
                    yield return null;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                lights[i].Color = _colors[i];
            }
            clearingLevel = false;
            yield return null;
        }
        private IEnumerator PuzzleFinished()
        {
            Color endColor = Color.LimeGreen;
            Color[] _colors = new Color[5];
            for (int j = 0; j < 5; j++)
            {
                _colors[j] = lights[j].Color;
            }
            for (int k = 0; k < 4; k++)
            {
                for (int j = 0; j < 5; j++)
                {
                    for (float i = 0; i < 1; i += 0.5f)
                    {
                        lights[j].SetColor(Color.Lerp(Color.White, endColor, i));
                        yield return Engine.DeltaTime/2f;
                    }
                    for (float i = 0; i < 1; i += 0.5f)
                    {
                        lights[j].SetColor(Color.Lerp(endColor, Color.White, i));
                        yield return Engine.DeltaTime / 2f;
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        lights[i].Color = Color.White;
                    }
                }
            }
            for (int i = 0; i < 5; i++)
            {
                lights[i].Color = _colors[i];
            }
            yield return null;
        }
        private void CreateButtons()
        {
            for (int i = 0; i < 5; i++)
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
            Add(sprites[5] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/eightSwitch/"));
            sprites[5].AddLoop("idle", "buttonSubmit", 0.1f);
            sprites[5].Position.X = (sprites[5].Width + spacing) * 5;
            sprites[5].Play("idle");
            list.Add(new Hitbox(sprites[5].Width, sprites[5].Height, sprites[5].Position.X, 0));
            Collider = list;
/*            int _i = 0;
            for (int i = 0; i < 17; i++)
            {
                if(i == 8)
                {
                    _i = 1;
                }
                Add(levels[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/eightSwitch/"));
                levels[i].AddLoop("idle", "levelLight", 0.1f);
                levels[i].Position.X = sprites[5].Position.X + ((levels[i].Width + 1) * _i);
                levels[i].Position.Y = i > 8 ? -38 : -43;
                levels[i].Position.X -= i > 8 ? levels[i].Width + 1 : 0;
                levels[i].Play("idle");
                _i++;
            }*/
        }
        public BinarySwitch(EntityData data, Vector2 offset)
        : base(data.Position + offset, 5 * 20, 9, false)
        {
            Depth = 1;
            OnDashCollide = OnDashed;
        }
        private void ResetLights()
        {
            for(int i=0; i<5; i++)
            {
                lights[i].SetColor(Color.White);
                lightOn[i] = false;
            }
        }
        private void AdjustButtonPosition(float amount)
        {
            for (int i = 0; i < 6; i++)
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
            if (a < 0 || a > 4 || FadingLight)
            {
                yield break;
            }
            FadingLight = true;
            if (lightOn[a])
            {
                for (float j = 0; j < 1; j += 0.1f)
                {
                    lights[a].SetColor(Color.Lerp(Color.Yellow, Color.White, j));
                    yield return null;
                }
            }
            else
            {
                for (float j = 0; j < 1; j += 0.1f)
                {

                    lights[a].SetColor(Color.Lerp(Color.White, Color.Yellow, j));
                    yield return null;
                }
            }
            lightOn[a] = !lightOn[a];
            FadingLight = false;
            yield return null;
        }
        private IEnumerator ButtonPressed()
        {
            PressingButton = true;
            int index = GetButton();
            Add(new Coroutine(FadeLight(index), true));
            bool indexValid = index >= 0 && index < 6 && player.Position.Y > Position.Y - 4 && player.Position.X >= Left && player.Position.X <= Right;
            for (float j = 0; j < 1; j += 0.2f)
            {
                if (indexValid)
                {
                    list.colliders[index].Position.Y = sprites[index].Position.Y = Calc.Approach(0, 6, 6 * j);
                }
                Collider = list;
                yield return null;
            }
            if (index == 5)
            {
                if (!clearingLevel)
                {
                    Add(new Coroutine(LevelClear(), true));
                    //Add(new Coroutine(LevelColor(true), true));
                }
                else
                {
                    level = 0;
                    //Add(new Coroutine(LevelColor(false), true));
                }
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
            if (player == null || EndRoutine)
            {
                return;
            }
            if (PuzzleClear && !EndRoutine)
            {
                Add(new Coroutine(PuzzleFinished(), true));
                EndRoutine = true;
            }
            buttonValid = GetButton() >= 0 && GetButton() < 6 && player.Position.Y > Position.Y - 4 && player.Position.X >= Left && player.Position.X <= Right;
            if (!PressingButton && !player.DashAttacking)
            {
                AdjustButtonPosition(pressAmount);
            }
        }
    }
}