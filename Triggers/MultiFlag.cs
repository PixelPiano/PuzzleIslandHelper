
using Celeste.Mod.Entities;

using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/MultiFlag")]
    [Tracked]
    public class MultiFlag : Trigger
    {
        public string[] turnOn;
        public string[] turnOff;
        public string flag;
        public bool inverted;
        public bool State
        {
            get
            {
                if (string.IsNullOrEmpty(flag))
                {
                    return true;
                }
                bool flagState = SceneAs<Level>().Session.GetFlag(flag);
                return inverted ? !flagState : flagState;
            }
        }
        public enum Mode
        {
            OnEnter,
            OnLeave,
            OnStay,
            OnLevelStart,
        }
        public Mode TriggerMode;

        public MultiFlag(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            TriggerMode = data.Enum<Mode>("mode");
            flag = data.Attr("flag");
            if (!string.IsNullOrEmpty(flag) && Inverted(flag))
            {
                inverted = true;
                flag.Remove(0, 1);
            }
            turnOn = data.Attr("turnOn").Replace(" ", "").Split(',');
            turnOff = data.Attr("turnOff").Replace(" ", "").Split(',');
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (TriggerMode == Mode.OnEnter && State)
            {
                ChangeFlags(player.Scene);
            }
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (TriggerMode == Mode.OnLeave && State)
            {
                ChangeFlags(player.Scene);
            }
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (TriggerMode == Mode.OnStay && State)
            {
                ChangeFlags(player.Scene);
            }
        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            if (TriggerMode == Mode.OnLevelStart && State)
            {
                ChangeFlags(scene);
            }
        }
        public void ChangeFlags(Scene scene)
        {
            Level level = scene as Level;
            foreach (string flag in turnOn)
            {
                level.Session.SetFlag(flag, true);
            }
            foreach (string flag in turnOff)
            {
                level.Session.SetFlag(flag, false);
            }
        }
        public bool Inverted(string flag)
        {
            return flag[0] == '!';
        }
    }
}
