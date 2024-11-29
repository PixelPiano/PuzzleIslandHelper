using Celeste.Mod.Core;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TileLayoutController")]
    [Tracked]
    public class TileLayoutController : Entity
    {
        public string CopyFrom;
        public TileLayoutController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            CopyFrom = data.Attr("copyFromRoom");
        }
        [OnLoad]
        public static void Load()
        {
            //Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
            //On.Celeste.LevelLoader.StartLevel += LevelLoader_StartLevel;
        }

/*        private static void LevelLoader_StartLevel(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self)
        {
            orig(self);
            LevelLoader_OnLoadingThread(self.Level);
        }*/

        [OnUnload]
        public static void Unload()
        {
            //Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
            //On.Celeste.LevelLoader.StartLevel -= LevelLoader_StartLevel;
        }
/*        private static void LevelLoader_OnLoadingThread(Level level)
        {
            BackgroundTiles bg = level.BgTiles;
            SolidTiles fg = level.SolidTiles;
            MapData mapData = level.Session.MapData;
            Console.WriteLine("TESTTTT");
            Console.WriteLine(fg.Tiles.Tiles[0,0] == null ? '0' : '1');
            return;
            foreach (TileLayoutData data in PianoMapDataProcessor.CopiedTileseedData)
            {
                if (mapData.levelsByName.ContainsKey(data.CopyFrom))
                {
                    Console.WriteLine("WE FOUND VALID DATA: " + data.CopyFrom + "to: " + data.CopyTo);
                    Rectangle from = mapData.levelsByName[data.CopyFrom].TileBounds;
                    Rectangle to = mapData.levelsByName[data.CopyTo].TileBounds;
                    Console.WriteLine("From size: {" + from.Width + ", " + from.Height + "}");
                    Console.WriteLine("To size: {" + from.Width + ", " + from.Height + "}");
                    float w = Calc.Min(from.Width, to.Width);
                    float h = Calc.Min(from.Height, to.Height);
                    Point offset = Point.Zero;
                    int bgX = (int)bg.Position.X;
                    int bgY = (int)bg.Position.Y;
                    int fgX = (int)fg.Position.X;
                    int fgY = (int)fg.Position.Y;
                    string fromSet = "";
                    string toSet = "";
                    for (int i = 0; i < h; i++)
                    {
                        int fromY = from.Top + i;
                        int toY = to.Top + offset.Y + i;
                        for (int j = 0; j < w; j++)
                        {
                            int fromX = from.Left + j;
                            int toX = to.Left + offset.X + j;
                            *//*  
                              if (i == 0 && j == 0)
                              {
                                  Console.Write("search for this line");
                                  Console.Write("To: {" + toX + ", " + toY + "}");
                                  Console.Write("From: {" + fromX + ", " + fromY + "}");
                                  Console.WriteLine(fg.Tiles.Tiles[fromX - fgX, fromY - fgY] == null);
                                  Console.WriteLine(fg.Tiles.Tiles[toX - fgX, toY - fgY] == null);
                              }
                            *//*
                            //bg.Tiles.Tiles[toX - bgX, toY - bgY] = bg.Tiles.Tiles[fromX - bgX, fromY - bgY];
                            //fg.Tiles.Tiles[toX - fgX, toY - fgY] = fg.Tiles.Tiles[fromX - fgX, fromY - fgY];
                            fromSet += fg.Tiles.Tiles[fromX - fgX, fromY - fgY] == null ? '0' : '1';
                            toSet += fg.Tiles.Tiles[toX - fgX, toY - fgY] == null ? '0' : '1';
                        }
                        fromSet += '\n';
                        toSet += "\n";
                    }
                    Console.WriteLine("Tiles for level " + data.CopyFrom + ":");
                    Console.WriteLine(fromSet + "\n");
                    Console.WriteLine("Tiles for level " + data.CopyTo + ":");
                    Console.WriteLine(toSet + "\n");

                    Console.WriteLine("0,0: "+fg.Tiles.Tiles[0, 0] == null ? '0' : '1');
                }
            }
        }*/


    }
}
