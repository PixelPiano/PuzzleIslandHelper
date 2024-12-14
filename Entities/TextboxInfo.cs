using Monocle;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MonoMod.Cil;
using FrostHelper;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class TextboxInfo : Entity
    {
        private FancyText.Portrait Portrait;
        private string talking;
        private string lastMood;
        private string mood;
        private string lastTalking;
        private int lastIndex;
        private int currentIndex;
        /// <summary>
        /// Invoked when the first active <see cref="Textbox"/>'s index changes.
        /// <para></para>
        /// <see cref="FancyText.Portrait"/>: The current Portrait.
        /// <see cref="FancyText.Char"/>: The current Character.
        /// </summary>
        public static event Action<FancyText.Portrait, FancyText.Char> OnNextChar;
        /// <summary>
        /// Invoked when the portrait changes.
        /// <para></para>
        /// <see cref="FancyText.Portrait"/>: The current Portrait.
        /// </summary>
        public static event Action<FancyText.Portrait> OnPortraitChange;
        /// <summary>
        /// Invoked when the portrait theme changes.
        /// <para></para>
        /// <see cref="FancyText.Portrait"/>: The current Portrait.
        /// <see cref="string"/>: The Sprite ID.
        /// </summary>
        public static event Action<FancyText.Portrait, string> OnMoodChange;
        public static event Action<FancyText.Portrait> OnWaitForInput;

        public static event Action<Textbox> GetTextbox;
        public TextboxInfo() : base()
        {
            Tag = Tags.Global;
        }
        public override void Update()
        {
            base.Update();
            if (Scene.Tracker.GetEntities<Textbox>().Find(item => (item as Textbox).Opened) is Textbox t)
            {
                GetTextbox?.Invoke(t);
                if (t.portrait != Portrait)
                {
                    OnPortraitChange?.Invoke(t.portrait);
                    lastMood = mood = null;
                }
                Portrait = t.portrait;

                if (t.waitingForInput)
                {
                    OnWaitForInput?.Invoke(Portrait);
                }

                if (t.portraitExists && !string.IsNullOrEmpty(t.PortraitAnimation))
                {
                    mood = t.PortraitAnimation;
                    if (lastMood != mood)
                    {
                        OnMoodChange?.Invoke(Portrait, Portrait.Animation);
                    }
                    lastMood = t.PortraitAnimation;
                }
                else
                {
                    lastMood = mood = null;
                }

                currentIndex = Calc.Clamp(t.index, 0, t.Nodes.Count - 1);
                if (lastIndex != currentIndex)
                {
                    if (t.Nodes[currentIndex] is FancyText.Char c)
                    {
                        OnNextChar?.Invoke(Portrait, c);
                    }
                }
                lastIndex = currentIndex;
            }
            else
            {
                Portrait = null;
                talking = lastTalking = null;
                mood = lastMood = null;
                lastIndex = 0;
            }
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }

        private static void LevelLoader_OnLoadingThread(Level level)
        {
            level.Add(new TextboxInfo());
        }
        [OnUnload]
        public static void Unload()
        {
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }
    }

    public class CalidusTextboxLooker
    {
        public class CalidusNode : FancyText.Node
        {
            public static List<string> EntityStrings = new() { "player", "maddy", "madeline", "ghost", "jaques", "randy" };
            public Calidus GetCalidus()
            {
                return Engine.Scene?.Tracker.GetEntity<Calidus>();
            }
            public void Run()
            {
                if (GetCalidus() is Calidus calidus)
                {
                    if (Looking != Calidus.Looking.None)
                    {
                        Look(calidus, Looking);
                    }
                    if (!string.IsNullOrEmpty(LookEntity))
                    {
                        LookAtEntity(calidus, LookEntity);
                    }
                    if (Emotion != Calidus.Mood.None)
                    {
                        Mood(calidus, Emotion);
                    }
                }
            }
            public void LookAtEntity(Calidus calidus, string entityName)
            {
                if (GetEntity(entityName) is Entity entity)
                {
                    calidus.LookAt(entity);
                }
            }
            public void Look(Calidus calidus, Calidus.Looking look)
            {
                if (Looking == Calidus.Looking.Target)
                {
                    return;
                    //calidus.LookTarget = LookTarget;
                }
                calidus.Look(look);

            }
            public Entity GetEntity(string from)
            {
                return from switch
                {
                    "player" or "maddy" or "madeline" => Engine.Scene?.GetPlayer(),
                    "jaques" => Engine.Scene?.Tracker.GetEntity<FormativeRival>(),
                    "randy" => Engine.Scene?.Tracker.GetEntity<PrimitiveRival>(),
                    "ghost" => Engine.Scene?.Tracker.GetEntity<Ghost>(),
                    _ => null
                };
            }
            public void Mood(Calidus calidus, Calidus.Mood mood)
            {
                if (Emotion != Calidus.Mood.None)
                {
                    calidus.Emotion(mood);
                }
            }
            public Calidus.Looking Looking = Calidus.Looking.None;
            public Calidus.Mood Emotion = Calidus.Mood.None;
            public string LookEntity;
            public Vector2 LookTarget;
        }
        [OnLoad]
        public static void Load()
        {
            IL.Celeste.FancyText.Parse += FancyText_Parse;
        }
        [OnUnload]
        public static void Unload()
        {
            IL.Celeste.FancyText.Parse -= FancyText_Parse;
        }
        private static void FancyText_Parse(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(
                    MoveType.Before,
                    instr => instr.MatchLdloc(7),
                    instr => instr.MatchLdstr("break")
                    ))
            {
                ILLabel notEqual = cursor.DefineLabel();
                ILLabel @continue = cursor.DefineLabel();

                cursor.EmitLdloc(7);//string text
                cursor.EmitLdstr("calidus");
                cursor.EmitCall<string>("op_Equality");
                cursor.EmitBrfalse(notEqual); //if text != "calidus", jump ahead to [A]

                cursor.EmitLdloc(8);//List<string> list
                cursor.EmitLdarg(0); //this
                cursor.EmitLdfld(typeof(FancyText).GetField("group", BindingFlags.NonPublic | BindingFlags.Instance)); //this.group
                cursor.EmitDelegate(something);
                cursor.EmitBrfalse(@continue); //jump ahead to [B]
                cursor.MarkLabel(notEqual); //[A]

                if (cursor.TryGotoNext(MoveType.Before,
                    instr => instr.MatchLdloc(6),
                    instr => instr.MatchLdcI4(1),
                    instr => instr.MatchAdd(),
                    instr => instr.MatchStloc(6)))
                {
                    cursor.MarkLabel(@continue); //[B]
                }
            }
            //Console.WriteLine("SEARCHFORWORDSHERE" + cursor.Context);
        }
        private static void something(List<string> list, FancyText.Text text)
        {
            Console.WriteLine("BOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            /*Console.WriteLine("list count: " + list.Count);
            Console.WriteLine("nodes count: " + text.Nodes.Count);
            if (list.Count > 1)
            {
                bool add = false;
                CalidusNode node = new();
                if (list[0] == "look")
                {
                    string look = list[1];
                    if (Enum.TryParse(look, true, out Calidus.Looking result))
                    {
                        Console.WriteLine(look + " detected as value looking");
                        node.Looking = result;
                        add = true;
                    }
                    else
                    {
                        node.LookEntity = look.ToLower();
                        add = true;
                    }
                }
                else if (list[0] == "mood")
                {
                    if (Enum.TryParse(list[1], true, out Calidus.Mood mood))
                    {
                        node.Emotion = mood;
                        add = true;
                    }
                }
                if (add)
                {
                    text.Nodes.Add(node);
                }
            }*/
        }

    }
}