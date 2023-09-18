using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.PuzzleIslandHelper.Entities.Transitions;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CharacterTest")]
    [Tracked]

    public class CharacterTest : Trigger
    {
        private Level level;
        private Calidus Calidus;
        private Freid Freid;
        private bool Played;
        private float TestFloat;

        public CharacterTest(EntityData data, Vector2 offset)
          : base(data, offset)
        {
            TestFloat = data.Float("testFloat");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Played)
            {
                return;
            }
            Played = true;
/*            Freid = level.Tracker.GetEntity<Freid>();
            Played = true;

            Add(new Coroutine(Cutscene(player)));*/
            level.Add(new MemoryTextscene(Vector2.Zero));
        }
        private IEnumerator Cutscene(Player player)
        {
            Played = true;
            Freid.Emotion("scared");
            yield return 4f;
            Freid.Emotion("normal");
            Freid.LookAtPlayer = true;
            yield return 1;
            Freid.SetLookTarget(null);
/*            foreach (Freid.Mood @enum in Enum.GetValues(typeof(Freid.Mood)).Cast<Freid.Mood>())
            {
                Freid.Emotion(@enum);
                yield return 3;
            }*/
            //Madeline walks in front of the computer
            //Calidus and Freid reiterate that the program on the computer can deactivate the digital world manually
            /*
             * C: Remember, in that computer is a program that can shut down the [DIGITAL WORLD] for good. 
             * C: Freid, do you still have the password?
             * F: Wh- you didn't think to ask this BEFORE we went through the eldritch horror laboratory?!
             * C: Well it's not like we would need sticky notes, would we?
             * F: That doesn't even have anything t- *sigh*
             * C: ...
             * F: ...
             * M: ...
             * F: ...
             * C: Freid?
             * F: ...Yeah I still have it.
             * C: We're wasting precious seconds y'know.
             * F: You're the one who brought up sticky notes of all things.
             * C: (You had the most ellipses.)
             * F: Gimme a sec.
             * *sounds of clanging and smart scientist noises and Huouin Kyouma voice in the background but digitalized*
             * 
             * C: Hell of a password huh
             * F: Can't ever be too secure.
             * M: So I just do the same thing I did with the other computers?
             * C: If by "do the same thing" you mean click the Access Module icon, then yes.
             * C: Unfortunately we haven't had the chance to forget text files detailing our pre-computated lives on this one.
             * M: Bummer.
             * 
             */


            //Just as Madeline is about to approach the computer, the Lab starts collapsing again
            //With little time to spare, Calidus directly overrides the computer and starts the access module program to get Madeline to safety
            //Music cuts out abruptly and screen goes black for 5 seconds, until instantly coming back onto a digital world area with Madeline
            // -> LastDigitalWorldIntro.cs
            yield return null;
            yield return null;

        }

        private IEnumerator ZoomTo(Vector2 AbsolutePosition, float amount, float duration)
        {
            if (level is null)
            {
                yield break;
            }
            yield return level.ZoomTo(level.Camera.CameraToScreen(AbsolutePosition), amount, duration);
        }

        private void PlayerFaceTo(Player player, Entity entity)
        {
            player.Facing = entity.Position.X > player.Position.X ? Facings.Right : Facings.Left;
        }
        private IEnumerator ZoomBack()
        {
            yield return level.ZoomBack(1);
        }

    }
}