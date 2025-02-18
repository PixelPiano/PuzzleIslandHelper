
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/DetectArea")]
    [Tracked]
    public class DetectArea : Trigger
    {
        public const string Prefix = "DetectArea:";
        public string ID;
        public bool SetFlagOnRemoved;
        public DetectArea(EntityData data, Vector2 offset) : base(data, offset)
        {
            ID = data.Attr("areaID");
            SetFlagOnRemoved = data.Bool("setFlagOnRemoved");
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if(!string.IsNullOrEmpty(ID)) SceneAs<Level>().Session.SetFlag(Prefix + ID);
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if(!string.IsNullOrEmpty(ID)) SceneAs<Level>().Session.SetFlag(Prefix + ID, false);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (SetFlagOnRemoved && !string.IsNullOrEmpty(ID)) (scene as Level).Session.SetFlag(Prefix + ID, false);
        }
        public bool Inside()
        {
            return InArea(SceneAs<Level>(), ID);
        }
        public static bool InArea(Level level, string id)
        {
            return !string.IsNullOrEmpty(id) && level.Session.GetFlag(Prefix + id);
        }
    }
}
