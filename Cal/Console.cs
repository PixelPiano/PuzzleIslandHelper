using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Cal
{
    /// <summary>
    /// Interactable entity for booting up CalJr.
    /// </summary>
    [CustomEntity("PuzzleIslandHelper/WipEntity")]
    [Tracked]
    public class CalConsole : Entity
    {

        public Image sprite;


        public TalkComponent talk;


        public bool talking;


        public SoundSource sfx;


        public CalConsole(Vector2 position)
            : base(position)
        {
            base.Depth = 1000;
            AddTag(Tags.TransitionUpdate);
            AddTag(Tags.PauseUpdate);
            Add(sprite = new Image(GFX.Game["objects/pico8Console"]));
            sprite.JustifyOrigin(0.5f, 1f);
            Add(talk = new TalkComponent(new Rectangle(-12, -8, 24, 8), new Vector2(0f, -24f), OnInteract));
        }

        public CalConsole(EntityData data, Vector2 position)
            : this(data.Position + position)
        {
        }


        public override void Update()
        {
            base.Update();
            if (sfx == null)
            {
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.Y < base.Y + 16f)
                {
                    Add(sfx = new SoundSource("event:/env/local/03_resort/pico8_machine"));
                }
            }
        }



        public void OnInteract(Player player)
        {
            if (!talking)
            {
                (base.Scene as Level).PauseLock = true;
                talking = true;
                Add(new Coroutine(InteractRoutine(player)));
            }
        }



        public IEnumerator InteractRoutine(Player player)
        {
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X - 6);
            player.Facing = Facings.Right;
            bool wasUnlocked = Settings.Instance.Pico8OnMainMenu;
            Settings.Instance.Pico8OnMainMenu = true;
            if (!wasUnlocked)
            {
                UserIO.SaveHandler(file: false, settings: true);
                while (UserIO.Saving)
                {
                    yield return null;
                }
            }
            else
            {
                yield return 0.5f;
            }

            bool done = false;
            SpotlightWipe.FocusPoint = player.Position - (Scene as Level).Camera.Position + new Vector2(0f, -8f);
            new SpotlightWipe(Scene, wipeIn: false, () =>
            {
                done = true;
                /*                if (!wasUnlocked)
                                {
                                    Scene.Add(new UnlockedPico8Message( () =>
                                    {
                                        done = true;
                                    }));
                                }
                                else
                                {
                                    done = true;
                                }
                */
                Engine.Scene = new CalEmu(Scene as Level);
            });
            while (!done)
            {
                yield return null;
            }

            yield return 0.25f;
            talking = false;
            (Scene as Level).PauseLock = false;
            player.StateMachine.State = 0;
        }


        public override void SceneEnd(Scene scene)
        {
            if (sfx != null)
            {
                sfx.Stop();
                sfx.RemoveSelf();
                sfx = null;
            }

            base.SceneEnd(scene);
        }
    }
}
