using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabComputerAccess")]
    [Tracked]
    public class LabComputerAccess : Entity
    {
        private Sprite Lights;
        private Sprite Panel;
        private Entity entity;
        private string flag1;
        private string flag2;
        private string flag3;
        private static int State;
        private TalkComponent Talk;
        public LabComputerAccess(EntityData data, Vector2 offset) 
            : base(data.Position + offset)
        {
            Lights = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/labComputerAccess/");
            Panel = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/labComputerAccess/");

            Lights.AddLoop("0", "buttonOff", 1f);
            Lights.AddLoop("1", "buttonOne", 1f);
            Lights.AddLoop("2", "buttonTwo", 1f);
            Lights.AddLoop("3", "buttonOn", 1f);

            Depth = -13001;
            Panel.AddLoop("idle", "panelWithButton", 1f);

            flag1 = data.Attr("flagOne");
            flag2 = data.Attr("flagTwo");
            flag3 = data.Attr("flagThree");

            Add(Lights);
            Add(Talk = new DotX3(0,0,Panel.Width,Panel.Height,new Vector2(Panel.Width/2,0),Interact));
        }
        private int GetSetFlags(Level level)
        {
            int result = 0;
            if (level.Session.GetFlag(flag1))
            {
                result++;
            }
            if (level.Session.GetFlag(flag2))
            {
                result++;
            }
            if (level.Session.GetFlag(flag3))
            {
                result++;
            }
            return result;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(entity = new Entity(Position));
            entity.Depth = 1;
            entity.Add(Panel);
            Vector2 Offset = new Vector2(8, Panel.Height + 9);
            entity.Position -= Offset;
            Talk.Bounds = new Rectangle(-(int)Offset.X, -(int)Offset.Y, (int)Panel.Width, (int)Panel.Height);
            Talk.DrawAt = new Vector2(Talk.Bounds.X + Panel.Width/2, Talk.Bounds.Y);

            State = GetSetFlags(scene as Level);
            Panel.Play("idle");
            Lights.Play(State.ToString());
        }

        private void Interact(Player player)
        {
            //play click sound
            Audio.Play("event:/PianoBoy/Machines/ButtonPressA",Position + Panel.Position);
            if(State == 3)
            {
                Lights.Visible = false;
                SceneAs<Level>().Session.SetFlag("labComputerAccess");
            }
            else
            {
                Lights.Visible = true;
            }
        }
    }
}