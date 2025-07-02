using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    /*
     * 0: FESTIVAL_RIVALS_00 (auto)
     * 1a: FESTIVAL_RIVALS_01a (talk to Jaques)
     * 1b: FESTIVAL_RIVALS_01b (talk to Randy)
     * 2: FESTIVAL_RIVALS_2 (talk to Jaques twice before talking to Randy once)
     * 3: 03 (talk to Jaques after talking to both Jaques and Randy once)
     * 4: 04 (talk to Jaques after he asks you to judge the competition)
     */

    [CustomEntity("PuzzleIslandHelper/Passengers/FormativeRival")]
    [Tracked]
    public class FormativeRival : VertexPassenger
    {
        public TextboxListener TextboxListener;
        public DotX3 Talk;
        public bool FacePlayerOnTalk = true;
        public bool IntroCutscenePlayed => SceneAs<Level>().Session.GetFlag("RivalsHaveEntered");
        private float mouthYOffset = 1;
        public int TimesTalkedTo
        {
            get
            {
                if (Scene is not Level level) return 0;
                return level.Session.GetCounter("TimesTalkedToJaques");
            }
            set
            {
                if (Scene is Level level)
                {
                    level.Session.SetCounter("TimesTalkedToJaques", value);
                }
            }
        }
        public FormativeRival(EntityData data, Vector2 offset) : base(data.Position + offset, 15, 20, null, data.Attr("dialog"), Vector2.One, new(-1, 1), 0.95f)
        {
            MinWiggleTime = 1;
            MaxWiggleTime = 2.5f;
            float legsY = 11;
            AddTriangle(new(7, 0), new(13, 8), new(0, 10), 1, Vector2.One, new(Color.LightGreen, Color.Green, Color.Turquoise));

            AddTriangle(new(3, 20), new(10, 20), new(5, legsY - 1), 0.1f, Vector2.One, new(Color.Green, Color.DarkGreen, Color.Turquoise));
            AddTriangle(new(10, 20), new(15, 20), new(12, legsY), 0.1f, Vector2.One, new(Color.Green, Color.DarkGreen, Color.Turquoise));

            AddQuad(new(3, -9), new(3, 0), new(10, -9), new(10, 0), 0.8f, Vector2.One, new(Color.Turquoise, Color.DarkBlue, Color.DarkTurquoise));

            AddTriangle(new(-1, 1), new(6, -2), new(6, 1), 0.8f, Vector2.One, new(Color.Turquoise, Color.LightBlue, Color.LightBlue));
            AddTriangle(new(6, -3), new(14, 1), new(6, 1), 0.8f, Vector2.One, new(Color.Turquoise, Color.LightBlue, Color.LightBlue));
            /*
             * the code that creates leg destruction
             * 
                        AddTriangle(2, 6, 9, 0, 9, 6, 1, Vector2.One, new(1, null, Color.Green, Color.DarkGreen, Color.SeaGreen));
                        AddQuad(3, 6, 9, 6, 3, 16, 9, 16, 1, Vector2.One, new(2, null, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
                        AddTriangle(4, 6, 6, 12, 4, 16, 1, Vector2.One);
                        AddTriangle(8, 6, 6, 8, 8, 16, 1, Vector2.One);
                        AddTriangle(3, 20, 4, 16, 5, 20, 1, Vector2.One);
                        AddTriangle(7, 20, 8, 16, 9, 20, 1, Vector2.One);
            */

            TextboxListener = new("Jaques", OnChar, OnPortrait, OnMood, OnWait);
            Add(TextboxListener);
            Color2 = Color.Red;
        }
        private void OnPortrait(FancyText.Portrait portrait)
        {
            //Flash(Color.Red);
        }
        private void OnMood(FancyText.Portrait portrait, string spriteId)
        {
            //Flash(Color.Yellow);
        }
        private void Flash(Color color)
        {
            Color2 = color;
            ColorMixLerp = 1f;
        }
        private void OnChar(FancyText.Portrait portrait, FancyText.Char character)
        {
            /*            if(!Baked) return;
                        if (character != null && !character.IsPunctuation)
                        {
                            if (Offsets[2] == Vector2.Zero)
                            {
                                Offsets[2] = -Vector2.UnitY;
                            }
                            Offsets[2] *= -1;
                        }
                        else
                        {
                            Offsets[2] = Vector2.Zero;
                        }*/
        }
        private void OnWait(FancyText.Portrait portrait)
        {
            Offsets[2] = Vector2.Zero;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;

            Talk = new DotX3(Collider, Interact);
            Add(Talk);
            Talk.Enabled = IntroCutscenePlayed;
            Facing = Facings.Right;
        }
        public void Interact(Player player)
        {
            FestivalCutscenes.Types type;
            int talkedTo = TimesTalkedTo;
            type = talkedTo switch
            {
                0 => FestivalCutscenes.Types.Jaques1,
                1 => FestivalCutscenes.Types.Jaques2,
                2 => FestivalCutscenes.Types.JaquesIntoCompetition,
                _ => FestivalCutscenes.Types.None
            };
            PrimitiveRival randy = player.Scene.Tracker.GetEntity<PrimitiveRival>();
            if (randy != null && randy.TimesTalkedTo < 1 && talkedTo == 1)
            {
                type = FestivalCutscenes.Types.JaquesMentionRandy;
            }
            if (type != FestivalCutscenes.Types.None)
            {
                Scene.Add(new FestivalCutscenes(type));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
            Offsets[2] = Vector2.UnitY;
        }
        public override void Update()
        {

            base.Update();
            ColorMixLerp = Calc.Approach(ColorMixLerp, 0, Engine.DeltaTime);
        }
    }
}
