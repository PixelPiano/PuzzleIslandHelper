using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
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
        private List<string> flags = new();
        public bool FlagState
        {
            get
            {
                if (Scene is not Level level) return false;
                List<bool> bools = new();
                foreach (string s in flags)
                {

                    if (string.IsNullOrEmpty(s)) continue;
                    bool inverted = s[0] == '!';
                    bool flagState = level.Session.GetFlag(s);
                    bools.Add(flagState || !flagState && inverted);
                }
                foreach (bool b in bools)
                {
                    if (!b) return false;
                }
                return true;
            }
        }
        public PICutscene(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            flags = data.Attr("flag").Replace(" ", "").Split(',').ToList();
            Part = data.Int("part");
            Tag |= Tags.TransitionUpdate;
            cutsceneName = data.Attr("cutscene");
            Cutscene = cutsceneName switch
            {
                "Prologue" => new PrologueSequence(Part),
                "WarpToCalidus" => new WarpToCalidus(),
                "GetInvert" => new InvertCutsceneTrigger(),
                "GrassShift" => new GrassShift(Part),
                "Gameshow" => new Gameshow(Part),
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
            if (!FlagState)
            {
                RemoveSelf();
            }
            else
            {
                Player player = (scene as Level).Tracker.GetEntity<Player>();
                if (ActivateOnTransition && player is not null)
                {
                    OnEnter(player);
                }
            }
        }
        public override void OnEnter(Player player)
        {
            string name = "";
            if (Part > 0 && cutsceneName is "Prologue" or "GrassShift")
            {
                name = cutsceneName + Part;
            }
            Entered = true;
            if (!InCutscene)
            {
                InCutscene = true;
                SceneAs<Level>().Add(Cutscene);
            }

            if (!PianoModule.Session.UsedCutscenes.Contains(name))
            {
                PianoModule.Session.UsedCutscenes.Add(name);
            }
        }
    }
}
