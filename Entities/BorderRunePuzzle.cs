using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BorderRunePuzzle")]
    public class BorderRunePuzzle : Entity
    {
        public Sprite[] Lights = new Sprite[4];
        public ShakeComponent Shaker;
        public Image Pannel;
        private Entity backEntity;
        private static string path = "objects/PuzzleIslandHelper/borderRune/";
        public static FlagData[] Flags = ["BorderRuneUp", "BorderRuneDown", "BorderRuneLeft", "BorderRuneRight"];
        public float OrigY;
        private string runeID;
        public static FlagData RoutineFlag = new FlagData("BorderRuneRoutine");
        public RuneDisplay RewardRune;
        public BorderRunePuzzle(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            runeID = data.Attr("runeId");
            OrigY = Position.Y;
            Depth = 5;
            Add(Pannel = new Image(GFX.Game[path + "pannel"]));
            string[] spriteSuffix = ["Up", "Down", "Left", "Right"];
            for (int i = 0; i < 4; i++)
            {
                Add(Lights[i] = new Sprite(GFX.Game, path));
                Lights[i].AddLoop("off", "light" + spriteSuffix[i], 0.1f, 0);
                Lights[i].AddLoop("on", "light" + spriteSuffix[i], 0.1f, 1);
            }
            Add(Shaker = new ShakeComponent(OnShake));
            Tag |= Tags.TransitionUpdate;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Image backImage = new Image(GFX.Game[path + "back"]);
            scene.Add(backEntity = new Entity(Position) { backImage });
            backEntity.Depth = 10;
            Collider = new Hitbox(backImage.Width, backImage.Height);
            bool routineDone = RoutineFlag;
            bool allOn = true;
            for (int i = 0; i < Lights.Length; i++)
            {
                if (routineDone) Flags[i].State = true;
                bool flag = Flags[i];
                allOn &= flag;
                Lights[i].Play(flag ? "on" : "off");
                Lights[i].Color = flag ? Color.Lime : Color.LightGray;
            }
            scene.Add(RewardRune = new RuneDisplay(
                Pannel.RenderPosition + Vector2.One * 4, 
                (int)Pannel.Width - 8, 
                (int)Pannel.Height - 8, 
                runeID, 
                true, 
                "", 
                Depth + 1));
            if (routineDone || allOn)
            {
                Y = (float)Math.Floor(OrigY + Pannel.Height * 0.7f);
                if (!routineDone)
                {
                    RoutineFlag.State = true;
                    Add(new Coroutine(revealRune()));
                }
            }
        }
        private IEnumerator revealRune()
        {
            Shaker.StartShaking(-1);
            yield return 1f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 3)
            {
                Y = (float)Math.Floor(OrigY + Pannel.Height * i);
                yield return null;
            }
            Shaker.StopShaking();
            Y = (float)Math.Floor(OrigY + Pannel.Height);
            yield return null;
        }
        private void OnShake(Vector2 shake)
        {
            Pannel.Position += shake;
            for (int i = 0; i < 4; i++)
            {
                Lights[i].Position += shake;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            backEntity?.RemoveSelf();
            RewardRune?.RemoveSelf();
        }
        public override void Update()
        {
            base.Update();
            bool allOn = true;
            for (int i = 0; i < Lights.Length; i++)
            {
                bool flag = Flags[i];
                allOn &= flag;
                Lights[i].Play(flag ? "on" : "off");
                Lights[i].Color = flag ? Color.Lime : Color.Gray;
            }
            if (allOn && !RoutineFlag)
            {
                RoutineFlag.State = true;
                Add(new Coroutine(revealRune()));
            }
        }
    }

    [CustomEntity("PuzzleIslandHelper/BorderRuneButton")]
    public class BorderRuneButton : Solid
    {
        public FlagData Flag;
        public Image Image;
        public BorderRuneButton(EntityData data, Vector2 offset) : base(data.Position + offset, 8, 16, true)
        {
            Depth = -1;
            int index = Calc.Clamp(data.Int("index"), 0, BorderRunePuzzle.Flags.Length);
            Flag = BorderRunePuzzle.Flags[index];
            OnDashCollide = OnDashCollideMethod;
            Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/borderRune/borderRuneButton"]));
            Tag |= Tags.TransitionUpdate;
        }
        public override void Render()
        {
            Image.DrawSimpleOutline();
            base.Render();
        }
        private IEnumerator pressRoutine()
        {
            Flag.State = true;
            float from = Position.Y;
            while (Position.Y > from + 14)
            {
                MoveTowardsY(from + 14, 3);
                yield return null;
            }
            MoveToY(from + 14);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Flag)
            {
                InstantPress();
            }
        }
        public void InstantPress()
        {
            MoveV(14);
        }
        public DashCollisionResults OnDashCollideMethod(Player player, Vector2 dir)
        {
            if (!Flag && HasPlayerOnTop() && dir.Y > 0)
            {
                Add(new Coroutine(pressRoutine()));
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }
    }
}