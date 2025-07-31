using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using static Celeste.Mod.FancyTileEntities.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Components;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TileSetPuzzle")]
    [Tracked]
    public class TileSetPuzzle : Solid
    {
        public TileGrid Tiles;
        public AnimatedTiles animatedTiles;
        public VirtualMap<char> tileMap;
        public TileInterceptor tileInterceptor;
        public List<LightOcclude> lightOccludes = [];
        public string data;
        public string rawdata;
        public string origData;
        public string rawOrigData;
        public bool[] changed;
        public int rows;
        public int columns;
        public int? seed;

        public TileSetPuzzle(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
        {
            string tiledata = data.Attr("tiledata");
            origData = tiledata;
            rawOrigData = tiledata.Replace("\n", "");
            this.data = tiledata;
            changed = new bool[rawOrigData.Length];
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Regenerate(origData);
        }
        public void Regenerate(string input, bool randomize = true)
        {
            data = input;
            rawdata = input.Replace("\n", "");
            Tiles?.RemoveSelf();
            Tiles = null;
            tileMap = null;
            animatedTiles?.RemoveSelf();
            animatedTiles = null;

            foreach (var light in lightOccludes)
            {
                light.RemoveSelf();
            }
            lightOccludes.Clear();
            tileInterceptor?.RemoveSelf();
            tileInterceptor = null;
            if (seed == null || randomize)
            {
                seed = Calc.Random.Next();
            }
            Calc.PushRandom(seed.Value);
            tileMap = GenerateTileMap(data);
            Autotiler.Generated generated = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour));
            Tiles = generated.TileGrid;
            Add(Tiles);
            Add(animatedTiles = generated.SpriteOverlay);
            Calc.PopRandom();

            ColliderList colliders = GenerateBetterColliderGrid(tileMap, 8, 8);
            Collider[] colliders2 = colliders.colliders;
            for (int i = 0; i < colliders2.Length; i++)
            {
                Hitbox hitbox = (Hitbox)colliders2[i];
                LightOcclude light = new LightOcclude(new Rectangle((int)hitbox.Position.X, (int)hitbox.Position.Y, (int)hitbox.Width, (int)hitbox.Height), 1);
                Add(light);
                lightOccludes.Add(light);
            }
            Collider = colliders;
            Add(tileInterceptor = new TileInterceptor(Tiles, highPriority: false));
            if (data.Contains('\n'))
            {
                rows = data.Where(item => item == '\n').Count() + 1;
                columns = data.IndexOf('\n');
            }
            else
            {
                rows = 1;
                columns = data.Length;
            }
        }
    }

    [CustomEntity("PuzzleIslandHelper/TileSetPuzzleController")]
    [Tracked]
    public class TileSetPuzzleController : Entity
    {
        private bool interacting;
        private float keyBuffer = 0.1f;
        private float bufferTimer = 0;
        private bool drawRect;
        private int selectedX;
        private int selectedY;
        private Alarm rectAlarm;
        private int selectedIndex => Puzzle == null ? 0 : selectedY * Puzzle.columns + selectedX;
        private TileSetPuzzle Puzzle;
        public TileSetPuzzleController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = int.MinValue;
            Collider = new Hitbox(data.Width, data.Height);
            Add(new DotX3(Collider, Interact));
            rectAlarm = Alarm.Set(this, 0.3f, delegate { drawRect = !drawRect; }, Alarm.AlarmMode.Looping);
        }
        public void Interact(Player player)
        {
            if (Scene.Tracker.GetEntity<TileSetPuzzle>() is TileSetPuzzle puzzle)
            {
                Puzzle = puzzle;
                interacting = true;
                player.DisableMovement();
                bufferTimer = keyBuffer;
            }
        }
        public override void Update()
        {
            base.Update();
            if (bufferTimer > 0)
            {
                bufferTimer -= Engine.DeltaTime;
            }
            else if (interacting)
            {
                if (Input.DashPressed)
                {
                    interacting = false;
                    Scene.GetPlayer()?.EnableMovement();
                }
                else
                {
                    int moveX = Input.MoveX.Value;
                    int moveY = Input.MoveY.Value;
                    if (moveX != 0 || moveY != 0)
                    {
                        selectedX += moveX;
                        if (selectedX < 0) selectedX = Puzzle.columns - 1;
                        if (selectedX > Puzzle.columns - 1) selectedX = 0;

                        selectedY += moveY;
                        if (selectedY < 0) selectedY = Puzzle.rows - 1;
                        if (selectedY > Puzzle.rows - 1) selectedY = 0;
                        resetTimer();
                    }
                    else if (Input.Jump)
                    {
                        int s = selectedIndex;
                        char n = Puzzle.changed[s] ? Puzzle.rawOrigData[s] : '0';
                        Puzzle.changed[s] = !Puzzle.changed[s];
                        string newRawData = Puzzle.rawdata.ReplaceAt(s, n);
                        string newData = "";
                        for (int i = 0; i < newRawData.Length; i++)
                        {
                            if (i % Puzzle.columns == 0)
                            {
                                newData += '\n';
                            }
                            newData += newRawData[i];
                        }
                        Puzzle.Regenerate(newData.Trim('\n'), false);
                        resetTimer();
                    }
                }
            }
        }
        private void resetTimer()
        {
            bufferTimer = keyBuffer;
            drawRect = true;
            rectAlarm.Start();
        }
        public override void Render()
        {
            base.Render();
            if (interacting)
            {
                Draw.HollowRect(Puzzle.Position, Puzzle.columns * 8, Puzzle.rows * 8, Color.Magenta);
                if (drawRect)
                {
                    Draw.HollowRect(Puzzle.X + selectedX * 8 - 1, Puzzle.Y + selectedY * 8 - 1, 10, 10, Color.White);
                }
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (scene.GetPlayer() is Player player && interacting)
            {
                interacting = false;
                player.EnableMovement();
            }
        }
    }

}