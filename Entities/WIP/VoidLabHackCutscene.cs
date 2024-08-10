using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/VoidLabHackCutscene")]
    [Tracked]

    public class VoidLabHackCutscene : Trigger
    {
        private Level level;

        private Sprite Computer;
        public VoidLabHackCutscene(EntityData data, Vector2 offset)
          : base(data, offset)
        {
            Computer = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
            Computer.AddLoop("idle", "interface", 0.1f);
            Computer.Position = new Vector2(data.Width / 2 - Computer.Width / 2, data.Height - Computer.Height);
            Add(Computer);
            Computer.Play("idle");
        }


        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Add(new Coroutine(Cutscene(player)));

        }
        private IEnumerator Cutscene(Player player)
        {
            //Madeline walks in front of the computer
            while (player.X != Computer.X + Position.X)
            {
                player.MoveTowardsX(Computer.X + Position.X, 8);
                yield return null;
            }
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
             * *sounds of clanging and smart scientist noises and Huouin Kyouma voice in the BackgroundColor but digitalized*
             * 
             * C: Hell of a password huh
             * F: Can't ever be too secure.
             * M: So I just do the same thing I did with the other computers?
             * C: If by "do the same thing" you mean click the Access Module icon, then yes.
             * C: Unfortunately we haven't had the chance to delete text files detailing our pre-computated lives on this one.
             * M: Bummer.
             * 
             */


            //Just as Madeline is about to approach the computer, the Lab starts collapsing again
            //With little acceleration to spare, Calidus directly overrides the computer and starts the access module program to get Madeline to safety
            //Music cuts out abruptly and screen goes black for 5 seconds, until instantly coming back onto a digital world area with Madeline
            // -> LastDigitalWorldIntro.cs
            yield return null;
        }

    }
}