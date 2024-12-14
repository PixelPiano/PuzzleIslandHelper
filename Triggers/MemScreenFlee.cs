using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/MemScreenFlee")]
    public class MemScreenFlee : Trigger
    {
        public bool OnlyOnce;
        public string Flag;
        public string EndFlag;
        public bool EndFlagState;
        public bool Inverted;
        public bool State => (string.IsNullOrEmpty(Flag) || SceneAs<Level>().Session.GetFlag(Flag)) != Inverted;
        public MemScreenFlee(EntityData data, Vector2 offset) : base(data, offset)
        {
            Flag = data.Attr("flag");
            Inverted = data.Bool("inverted");
            EndFlag = data.Attr("flagOnEnd");
            EndFlagState = data.Bool("endFlagState");
            Tag |= Tags.TransitionUpdate;
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (State)
            {
                Add(new Coroutine(Cutscene(player)));
            }
        }
        public IEnumerator Cutscene(Player player)
        {
            Level level = Scene as Level;
            if (level != null)
            {
                MemoryScreen screen = level.Tracker.GetEntity<MemoryScreen>();
                if (screen != null)
                {
                    screen.StateMachine.State = MemoryScreen.StDummy;
                    screen.DummyFacing = false;
                    screen.DummyFloating = false;
                    screen.Facing = player.X < screen.X ? Facings.Left : Facings.Right;

                    float from = screen.Y;
                    for (float i = 0; i < 1; i += Engine.DeltaTime / 0.45f)
                    {
                        screen.Position.Y = Calc.LerpClamp(from, from - 8, Ease.UpDown(i));
                        yield return null;
                    }
                    float fleeToX = level.Marker("fleeto").X;
                    screen.DummyFloating = true;
                    screen.DummyFacing = true;
                    while (screen.X != fleeToX && level.Bounds.Contains(new Point((int)screen.X - 12, (int)screen.Y)))
                    {
                        screen.MoveH(MemoryScreen.PatrolSpeed * 4 * Engine.DeltaTime);
                        yield return null;
                    }
                    level.Session.DoNotLoad.Add(screen.ID);
                    screen.RemoveSelf();
                }
                if (!string.IsNullOrEmpty(EndFlag))
                {
                    level.Session.SetFlag(EndFlag, EndFlagState);
                }
            }
            yield return null;
            RemoveSelf();
        }
    }
}
