using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/TilesetSwapper")]
    [Tracked]
    public class TilesetSwapper : Entity
    {
        public static FieldInfo lookupFieldInfo = typeof(Autotiler).GetField("lookup", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo terrainTypeInfo = typeof(Autotiler).GetField("TerrainType", BindingFlags.Instance | BindingFlags.NonPublic);

        private char onTile;
        private char offTile;
        private string flag;
        private bool invertFlag;
        public static List<char> ogTiles;
        public static List<char> newTiles;
        public bool State
        {
            get
            {
                return string.IsNullOrEmpty(flag) || Scene is Level level && level.Session.GetFlag(flag) != invertFlag;
            }
        }
        private bool previousState;
        public TilesetSwapper(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            onTile = data.Char("onTile");
            offTile = data.Char("offTile");
            flag = data.Attr("flag");
            invertFlag = data.Bool("invertFlag");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
        public static void cacheValidTiles()
        {
            if (ogTiles == null || newTiles == null)
            {
                IDictionary dictionary = lookupFieldInfo.GetValue(GFX.FGAutotiler) as IDictionary;
                IDictionary dictionary2 = lookupFieldInfo.GetValue(GFX.BGAutotiler) as IDictionary;
                ogTiles = dictionary.Keys.Cast<char>().ToList();
                newTiles = dictionary2.Keys.Cast<char>().ToList();
            }
        }
        public override void Update()
        {
            base.Update();
            if (State != previousState)
            {

            }
            previousState = State;
        }
        public void SwitchTiles(char from, char to)
        {
            Level level = Scene as Level;
            IEnumerable<MTexture> e = level.SolidTiles.Tiles.Tiles.GetEnumerator();
        }
    }
}
