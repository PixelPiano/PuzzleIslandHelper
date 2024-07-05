using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.FlipPuzzle
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FlipPuzzle")]
    public class FlipPuzzle : Solid
    {
        Coroutine debugCheck;
        private int xSide;
        private int ySide;
        private static int[,] cellValue = new int[5, 5] {{ 0, 1, 1, 1, 0},
                                                  { 1, 0, 0, 0, 1 },
                                                  { 1, 0, 0, 0, 1 },
                                                  { 0, 1, 1, 1, 0 },
                                                  { 1, 0, 0, 0, 1 } };
        private int[,] currentValue = cellValue.Clone() as int[,];
        private Vector2 debugger = new Vector2(-1, -1);
        private Sprite[,] sprite = new Sprite[5, 5];
        private bool canActivate;
        private bool isFlipping;
        public FlipPuzzle(EntityData data, Vector2 offset)
        : base(data.Position + offset, 80, 80, false)
        {
            OnDashCollide = OnDashed;
            for (int i = 0; i < 5; i++)
            {
                for (int k = 0; k < 5; k++)
                {
                    Add(sprite[k, i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/flipPuzzle/"));
                    sprite[k, i].AddLoop("idleRed", "idleRed", 0.1f);
                    sprite[k, i].AddLoop("idleBlue", "idleBlue", 0.1f);
                    sprite[k, i].Add("flipToBlue", "redToBlue", 0.1f, "idleBlue");
                    sprite[k, i].Add("flipToRed", "blueToRed", 0.1f, "idleRed");
                    sprite[k, i].Position += new Vector2(i * 16, k * 16);
                }
            }
            Collider = new Hitbox(80, 80);
            canActivate = true;
            isFlipping = false;
        }

        public Vector2 getImpact()
        {
            Vector2 output = new Vector2(-1, -1);
            Player player = Scene.Tracker.GetEntity<Player>();
            //int output = 0;
            if (player.Center.X >= Left
                && player.Center.X <= Right)
            {
                if (player.Center.Y >= Top)
                {
                    ySide = 1;
                }
                else
                {
                    ySide = 0;
                }

                output = new Vector2(1, (int)(player.Center.X - Left) / 16);
            }

            else if (player.Center.Y >= Top
               && player.Center.Y <= Bottom)
            {
                if (player.Center.X <= Left)
                {
                    xSide = 1;
                }
                else
                {
                    xSide = 0;
                }
                output = new Vector2(0, (int)(player.Center.Y - Top) / 16);
            }

            return output;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            for (int i = 0; i < 5; i++)
            {
                for (int k = 0; k < 5; k++)
                {
                    //Texture[i, k].PlayEvent("idleRed");
                    if (cellValue[k, i] == 0)
                    {
                        sprite[k, i].Play("idleRed");
                    }
                    else
                    {
                        sprite[k, i].Play("idleBlue");
                    }
                }
            }
        }

        public IEnumerator FlipColumn(int col)
        {
            isFlipping = true;
            if (col <= 4)
            {
                if (ySide == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Flip(sprite[i, col]);
                        for (float k = 0.0f; k < 2; k += 0.1f)
                        {
                            yield return null;
                        }
                    }
                }
                else
                {
                    for (int i = 4; i >= 0; i--)
                    {
                        Flip(sprite[i, col]);
                        for (float k = 0.0f; k < 2; k += 0.1f)
                        {
                            yield return null;
                        }
                    }
                }
            }
            for (int i = 0; i < 5; i++)
            {
                for (int k = 0; k < 5; k++)
                {
                    if (sprite[i, k].CurrentAnimationID == "idleBlue")
                    {
                        currentValue[i, k] = 1;
                    }
                    else
                    {
                        currentValue[i, k] = 0;
                    }
                }
            }
            isFlipping = false;
            canActivate = true;
            yield return null;
        }
        public IEnumerator FlipRow(int row)
        {
            isFlipping = true;
            if (row <= 4)
            {
                if (xSide == 1)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Flip(sprite[row, i]);
                        for (float k = 0.0f; k < 2; k += 0.1f)
                        {
                            yield return null;
                        }
                    }
                }
                else
                {
                    for (int i = 4; i >= 0; i--)
                    {
                        Flip(sprite[row, i]);
                        for (float k = 0.0f; k < 2; k += 0.1f)
                        {
                            yield return null;
                        }
                    }
                }
            }
            for (int i = 0; i < 5; i++)
            {
                for (int k = 0; k < 5; k++)
                {
                    if (sprite[i, k].CurrentAnimationID == "idleBlue")
                    {
                        currentValue[i, k] = 1;
                    }
                    else
                    {
                        currentValue[i, k] = 0;
                    }
                }
            }
            isFlipping = false;
            canActivate = true;
            yield return null;
        }
        public IEnumerator checkDebug()
        {
            while (debugger == new Vector2(-1, -1))
            {
                canActivate = false;
                yield return null;
            }
            canActivate = true;
            yield return null;
        }
        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            Coroutine coroutine;
            Vector2 impacted = new Vector2(-1, -1);
            if (canActivate && !isFlipping)
            {
                impacted = getImpact();
                if (impacted.X == 1)
                {
                    canActivate = false;
                    coroutine = new Coroutine(FlipColumn((int)impacted.Y), true);
                    Add(coroutine);
                }
                if (impacted.X == 0)
                {
                    canActivate = false;
                    coroutine = new Coroutine(FlipRow((int)impacted.Y), true);
                    Add(coroutine);
                }
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }

        public void Flip(Sprite sprite)
        {
            if (sprite.CurrentAnimationID == "idleBlue")
            {
                sprite.Play("flipToRed");
            }
            if (sprite.CurrentAnimationID == "idleRed")
            {
                sprite.Play("flipToBlue");
            }
        }
        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                debugger = getImpact();
                debugCheck = new Coroutine(checkDebug(), true);
                Add(debugCheck);
            }
        }
    }
}