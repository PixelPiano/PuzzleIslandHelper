using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{


    [Tracked]
    public class Machine : Entity
    {
        public Sprite Sprite;
        public DotX3 Talk;
        public Interface Interface;
        public Color BackgroundColor;
        public bool UsesStartupMonitor;
        public bool UsesFloppyLoader;
        public bool TalkEnabled = true;
        public void SetSessionInterface()
        {
            PianoModule.Session.Interface = Interface;
        }
        public Machine(Vector2 position) : base(position)
        {

        }
        public Machine(Vector2 position, string path, Color backgroundColor) : this(position)
        {
            BackgroundColor = backgroundColor;
            Depth = 2;
            Sprite = new Sprite(GFX.Game, path);
            Sprite.AddLoop("idle", "", 0.1f);
            Add(Sprite);
            Sprite.Play("idle");
            Add(Talk = new DotX3(0, 0, Sprite.Width, Sprite.Height, new Vector2(Sprite.Width / 2, 0), Interact));
            Talk.PlayerMustBeFacing = false;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Talk.Enabled = TalkEnabled;
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = TalkEnabled;
        }
        public virtual void OnMonitorOff()
        {

        }
        public virtual void OnEnd()
        {

        }

        public virtual IEnumerator OnBegin(Player player)
        {
            yield return null;
        }
        public virtual void Interact(Player player)
        {
            if (Interface == null)
            {
                Scene.Add(Interface = new Interface(BackgroundColor, this));
            }
            Interface.FakeStarting = false;
            player.StateMachine.State = Player.StDummy;
            Add(new Coroutine(OnBegin(player)));
        }
    }
}