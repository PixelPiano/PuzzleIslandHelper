using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{

    [CustomEntity("PuzzleIslandHelper/PICutscene")]
    [Tracked]
    public class PICutscene : Trigger
    {
        private CutsceneEntity Cutscene;
        private int Part;
        private string cutsceneName;
        private bool OncePerRoom;
        private bool OncePerPlaythrough;
        private bool ActivateOnTransition;
        private bool Entered;
        private bool InCutscene;
        private string flag;

        public PICutscene(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            flag = data.Attr("flag");
            Part = data.Int("part");
            cutsceneName = data.Attr("cutscene");
            Cutscene = cutsceneName switch
            {
                "Prologue" => new PIPrologueSequence(Part),
                "Calidus1" => new DigiMeet(),
                "GetInvert" => new InvertCutsceneTrigger(),
                "GrassShift" => new GrassShift(Part),
                "Gameshow" => new Gameshow(),
                "End" => null,
                "TEST" => new TEST(),
                _ => null
            };
            OncePerRoom = data.Bool("oncePerRoom");
            OncePerPlaythrough = data.Bool("oncePerPlaythrough");
            ActivateOnTransition = data.Bool("activateOnTransition");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            (scene as Level).InCutscene = false;
            if (!FlagState())
            {
                RemoveSelf();
            }
            Player player = (scene as Level).Tracker.GetEntity<Player>();
            if (ActivateOnTransition && player is not null)
            {
                OnEnter(player);
            }
        }
        public bool FlagState()
        {
            return string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag);
        }
        public override void OnEnter(Player player)
        {
            string name = "";
            if(Part > 0 && (cutsceneName is "Prologue" or "GrassShift"))
            {
                name = cutsceneName + Part;
            }
/*            if ((Entered && OncePerRoom) || (OncePerPlaythrough && PianoModule.SaveData.UsedCutscenes.Contains(name)))
            {
                return;
            }*/
            Entered = true;
            if (!InCutscene)
            {
                InCutscene = true;
                SceneAs<Level>().Add(Cutscene);
            }

            if (!PianoModule.SaveData.UsedCutscenes.Contains(name))
            {
                PianoModule.SaveData.UsedCutscenes.Add(name);
            }
        }
    }
}
