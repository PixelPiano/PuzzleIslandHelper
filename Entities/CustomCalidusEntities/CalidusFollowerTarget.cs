using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Linq;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework.Input;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities
{

    [Tracked]
    public class CalidusFollowerTarget : Entity
    {
        public float XOffset;
        public Vector2 Offset;
        public Vector2 AdditionalOffset;
        public Leader Leader;
        public Entity Follow;
        public Calidus Calidus;
        public CalidusFollowerTarget(Entity follow = null) : base()
        {
            Tag |= Tags.TransitionUpdate | Tags.Global;
            Leader = new Leader();
            Add(Leader);
            Follow = follow;
        }
        public static void SetOffset(Vector2 offset)
        {
            if (Engine.Scene is Level level)
            {
                var target = level.Tracker.GetEntity<CalidusFollowerTarget>();
                if (target != null)
                {
                    target.AdditionalOffset = offset;
                }
            }
        }
        public static void OffsetBy(Vector2 offset)
        {
            if (Engine.Scene is Level level)
            {
                var target = level.Tracker.GetEntity<CalidusFollowerTarget>();
                if (target != null)
                {
                    target.AdditionalOffset += offset;
                }
            }
        }
        public void Resume()
        {
            Active = true;
        }
        public void Stop()
        {
            Active = false;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Color color = Active ? Color.Magenta : Color.Gray;
            Draw.Point(Position - Vector2.UnitY, color);
            Draw.Point(Position + Vector2.UnitY, color);
            Draw.Point(Position - Vector2.UnitX, color);
            Draw.Point(Position + Vector2.UnitX, color);
        }
        public override void Update()
        {
            base.Update();
            Calidus = Scene.Tracker.GetEntity<Calidus>();
            if (Scene.GetPlayer() is Player player)
            {
                Offset.X = Calc.Approach(Offset.X, -(int)player.Facing * 10, Engine.DeltaTime * 25f);
                Offset.Y = Calc.Approach(Offset.Y, -20, Engine.DeltaTime * 25f);
                Follow = player;
            }
            
            Position = new Vector2(Follow.CenterX, Follow.Top) + Offset + AdditionalOffset;
        }
        /*        [OnLoad]
                public static void Load()
                {
                    Everest.Events.Player.OnSpawn += Player_OnSpawn;
                }

                [OnUnload]
                public static void Unload()
                {
                    Everest.Events.Player.OnSpawn -= Player_OnSpawn;
                }
                private static void Player_OnSpawn(Player obj)
                {
                    var f = obj.Scene.Tracker.GetEntity<CalidusFollowerTarget>();
                    if (f != null)
                    {
                        f.Position = obj.Position;
                    }
                }*/
    }
}
