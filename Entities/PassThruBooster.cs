using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
// PuzzleIslandHelper.PassThruBooster
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PassThruBooster")]
    [TrackedAs(typeof(Booster))]
    public class PassThruBooster : Booster
    {
        private bool InFront;
        private int _Depth;
        private Player player;
        private static bool RetainState;
        public PassThruBooster(EntityData data, Vector2 offset)
          : base(data.Position + offset, data.Bool("red"))
        {
            _Depth = Depth;
            InFront = data.Bool("aboveFG");
        }
        public override void Update()
        {
            base.Update();
            if (RetainState)
            {
                SceneAs<Level>().SolidTiles.Collidable = false;
            }
        }
        private static void OnRelease(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
        {
            if (self is PassThruBooster booster)
            {
                RetainState = false;
                booster.SceneAs<Level>().SolidTiles.Collidable = true;
                booster.Depth = booster._Depth;
                if (booster.player.CollideCheck<Solid>() && !booster.player.CollideCheck<Booster>())
                {
                    booster.player.Die(Vector2.Zero);
                }
            }
            orig(self);
        }
        private static void OnDashed(On.Celeste.Booster.orig_OnPlayerDashed orig, Booster self, Vector2 dir)
        {
            RetainState = false;
            if (self is PassThruBooster booster)
            {
                booster.SceneAs<Level>().SolidTiles.Collidable = true;
            }
            orig(self, dir);
        }
        private static void OnPlayer(On.Celeste.Booster.orig_PlayerBoosted orig, Booster self, Player player,Vector2 dir)
        {
            RetainState = true;
            if (self is PassThruBooster booster)
            {
                booster.player = player;
                booster.SceneAs<Level>().SolidTiles.Collidable = false;
                booster.Depth = booster.CollideCheck(player) || booster.InFront ? -10000 : booster._Depth;
            }
            orig(self, player, dir);
        }
        [OnLoad]
        internal static void Load()
        {
            On.Celeste.Booster.PlayerReleased += OnRelease;
            On.Celeste.Booster.OnPlayerDashed += OnDashed;
            On.Celeste.Booster.PlayerBoosted += OnPlayer;
        }
        [OnUnload]
        internal static void Unload()
        {
            On.Celeste.Booster.PlayerReleased -= OnRelease;
            On.Celeste.Booster.OnPlayerDashed -= OnDashed;
            On.Celeste.Booster.PlayerBoosted-= OnPlayer;
        }

    }
}
