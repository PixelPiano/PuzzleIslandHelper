using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Rules
{
    [Tracked]
    public abstract class Rule : Entity
    {
        public bool RuleIsActive;
        public Vector2 Direction;
        public float Timer;
        public float Duration;
        public enum Rules
        {
            Dash,
            InQuarter,
            InHalf,
            Crouching,
            Fast,
            Still,
            Jumping,
            Climbing,
        }
        public enum Directions
        {
            Right, Up, Left, Down, UpRight, UpLeft, DownLeft, DownRight
        }
        private Rules rule;
        private Directions dir;
        public Rule(Rules rule) : base()
        {
            this.rule = rule;
            Duration = GetDuration(rule);
        }
        public Rule(Rules rule, Directions dir) : this(rule)
        {
            Direction = GetDir(dir);
        }
        public static Vector2 GetDir(Directions dir)
        {
            return dir switch
            {
                Directions.Up => -Vector2.UnitY,
                Directions.Down => Vector2.UnitY,
                Directions.Left => -Vector2.UnitX,
                Directions.Right => Vector2.UnitX,
                Directions.UpRight => new Vector2(1,-1),
                Directions.UpLeft => -Vector2.One,
                Directions.DownRight => Vector2.One,
                Directions.DownLeft => new Vector2(-1, 1),
                _ => Vector2.Zero
            };
        }
        public static float GetDuration(Rules rule)
        {
            return 0;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is Level level && level.GetPlayer() is Player player)
            {
                if (Condition(player))
                {
                    OnSuccess(player);
                }
                else
                {
                    Timer = 0;
                    OnFail(player);
                }
            }
        }
        public void OnSuccess(Player player)
        {

        }
        public void OnFail(Player player)
        {
            if (!player.Dead)
            {
                player.Die(Vector2.Zero);
            }
        }
        public bool Condition(Player player)
        {
            return false;
        }
    }
}
