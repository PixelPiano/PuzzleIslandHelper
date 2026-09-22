using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TowerBlock")]
    [Tracked]
    public class TowerBlock : Solid
    {
        //wip of potential crack graphic
        //todo: revisit during polish
        /*        public class Cracks : GraphicsComponent
                {
                    public class crack
                    {
                        public bool HasSplit;
                        public bool CanSplit = true;
                        public Vector2 from;
                        public Vector2 to;
                        public float angle;
                        public float lerp;
                        public Tween Tween;
                    }
                    public List<crack> cracks = [];
                    public int LastBatchSize = 1;
                    public float Width;
                    public float Height;
                    public float AngleRange;
                    public float Angle;
                    public void Crack(int splits = 1, int segments = 1)
                    {
                        crack[] array = [];
                        cracks.CopyTo(array);
                        foreach (crack crack in array)
                        {
                            if (!crack.HasSplit && crack.CanSplit)
                            {
                                SplitFrom(crack, 3, 0.5f, segments, 0);
                                crack.HasSplit = true;
                            }
                        }
                    }
                    private bool colliding(crack crack, float angle, float length)
                    {
                        Vector2 from = crack.from;
                        Vector2 to = crack.from + Calc.AngleToVector(angle, length);
                        foreach (crack crack2 in cracks)
                        {
                            if (Collide.LineCheck(from, to, crack2.from, crack2.to))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    public void SplitFrom(crack crack, float length, float time, int total, int index)
                    {
                        crack next = new crack()
                        {
                            CanSplit = index >= total - 1,
                            lerp = 0,
                            from = crack.to
                        };
                        cracks.Add(next);
                        float angle = crack.angle + Calc.Random.Range(-AngleRange, AngleRange);
                        for (int i = 0; i < 20; i++)
                        {
                            if (!colliding(next, angle, length))
                            {

                            }
                        }
                        next.Tween = Tween.Set(this.Entity, Tween.TweenMode.Oneshot, time, Ease.QuintOut, t =>
                        {
                            next.length = length * t.Eased;
                        }, t =>
                        {
                            next.length = length;
                            if (index < total)
                            {
                                SplitFrom(next, length, time, total, index + 1);
                            }
                        });
                    }

                    public Cracks(Vector2 position, float width, float height, float angle, float angleRange) : base(true)
                    {
                        Position = position;
                        Width = width;
                        Height = height;
                        Angle = angle;
                        AngleRange = angleRange;
                    }

                }*/

        public bool Inside;
        private char tiletype;
        private TileGrid tileGrid;
        private EntityID id;
        public bool CanBreak;
        private List<Coroutine> coroutines = [];
        public TowerBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            Inside = data.Bool("inside", true);
            tiletype = data.Char("tiletype", '3');
            this.id = id;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)(base.X / 8f) - tileBounds.Left;
            int y = (int)(base.Y / 8f) - tileBounds.Top;
            int tilesX = (int)base.Width / 8;
            int tilesY = (int)base.Height / 8;

            tileGrid = GFX.FGAutotiler.GenerateOverlay(tiletype, x, y, tilesX, tilesY, solidsData).TileGrid;
            Add(new EffectCutout());
            base.Depth = -10501;
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            tileGrid.Position += new Vector2(amount.X, amount.Y * 0.5f);
        }
        public void PulseShake(float time = -1)
        {
            Audio.Play("event:/game/general/wall_break_stone", Position);
            StartShaking(time);
            List<int> order = [];
            for (int x = 0; x < (int)Width; x += 8)
            {
                order.Add(x);
            }
            order.Shuffle(Calc.Random);
            var coroutine = new Coroutine(debrisRoutine(order));
            coroutines.Add(coroutine);
            Add(coroutine);
        }
        public void Break()
        {
            coroutines.RemoveSelves();
            Audio.Play("event:/game/general/wall_break_stone", Position);
            for (int i = 0; i < Width / 8f; i++)
            {
                for (int j = 0; j < Height / 8f; j++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tiletype, true).BlastFrom(Center));
                }
            }
            Collidable = false;
            SceneAs<Level>().Session.DoNotLoad.Add(id);
            RemoveSelf();
        }
        private IEnumerator debrisRoutine(List<int> order)
        {
            foreach (int i in order)
            {
                Vector2 p = TopLeft + new Vector2(4 + i, -4);
                Scene.Add(Engine.Pooler.Create<Debris>().Init(p, tiletype, true).BlastFrom(TopCenter));
                yield return Engine.DeltaTime;
            }
        }
    }
}