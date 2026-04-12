using Celeste;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Autotiler;
/// <summary>A collection of methods + extension methods used in relation with Celeste.Autotiler.</summary>
public static class AutotilerExt
{
    public delegate char TileChooser(int x, int y, int swapAtX, int swapAtY);
    public static Generated GenerateCustomBox(this Autotiler autotiler, int startX, int startY, int tilesX, int tilesY, int swapAtX, int swapAtY, Behaviour behaviour, TileChooser condition)
    {
        char[,] array = new char[tilesY,tilesX];
        for (int i = 0; i < tilesY; i++)
        {
            for (int j = 0; j < tilesX; j++)
            {
                array[i,j] = condition(j, i, swapAtX, swapAtY);
            }
        }
        TileGrid tileGrid = new TileGrid(8, 8, tilesX, tilesY);
        AnimatedTiles animatedTiles = new AnimatedTiles(tilesX, tilesY, GFX.AnimatedTilesBank);
        Rectangle forceFill = new Rectangle(startX, startY, tilesX, tilesY);
        for (int x = startX; x < startX + tilesX; x++)
        {
            for (int y = startY; y < startY + tilesY; y++)
            {
                char id = array[y - startY, x - startX];
                Tiles tiles2 = autotiler.TileHandler(null, x, y, forceFill, id, behaviour);
                if (tiles2 != null)
                {
                    tileGrid.Tiles[x - startX, y - startY] = Calc.Random.Choose(tiles2.Textures);
                    if (tiles2.HasOverlays)
                    {
                        animatedTiles.Set(x - startX, y - startY, Calc.Random.Choose(tiles2.OverlapSprites));
                    }
                }
            }
        }
        Generated result = default(Generated);
        result.TileGrid = tileGrid;
        result.SpriteOverlay = animatedTiles;
        return result;
    }

}
