using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/BatterySwitch")]
    [Tracked]
    public class BatterySwitch : Entity
    {

        public Sprite Handle;
        public Image Back;
        public Image Fill;
        public string SwitchID;
        private bool idEmpty => string.IsNullOrEmpty(SwitchID);
        public DotX3 Talk;
        public bool Activated
        {
            get
            {
                return !string.IsNullOrEmpty(SwitchID) && PianoModule.Session.DrillBatteryIds.Contains(SwitchID);
            }
        }
        public BatterySwitch(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 1;
            Back = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/batterySwitchBack"]);
            Fill = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/batterySwitchColor"]);
            Handle = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/drillMachine/batterySwitchHandle");
            SwitchID = data.Attr("batteryId");
            Handle.AddLoop("idleDown", "", 0.1f, 0);
            Handle.AddLoop("idleUp", "", 0.1f, 6);
            Handle.Add("activate", "", 0.1f, "idleUp");
            Handle.Visible = false;
            Add(Back, Fill, Handle);
            Collider = new Hitbox(Back.Width, Back.Height);
            Talk = new DotX3(0, 0, Width, Height + 16, Vector2.UnitX * Collider.HalfSize.X, Interact);
            Add(Talk);
            AddTag(Tags.TransitionUpdate);
        }
        private void Interact(Player player)
        {
            if (!Activated)
            {
                Add(new Coroutine(Activate(player)));
            }
        }
        private IEnumerator CameraLerp(Level level, Vector2 to)
        {
            Vector2 prev = level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                level.Camera.Position = Vector2.Lerp(prev, to, Ease.SineOut(i));
                yield return null;
            }
            yield return null;
        }
        public IEnumerator Activate(Player player)
        {
            if (Scene is not Level level) yield break;
            player.ForceCameraUpdate = false;
            Add(new Coroutine(CameraLerp(level, Position - new Vector2(160, 90))));
            player.StateMachine.State = Player.StDummy;
            yield return 0.2f;
            Handle.Play("activate");
            Handle.Rate /= 2;
            while (Handle.CurrentAnimationFrame < Handle.CurrentAnimationTotalFrames / 2)
            {
                yield return null;
            }
            Handle.Rate *= 2;
            while (Handle.CurrentAnimationID != "idleUp")
            {
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.2f)
            {
                Fill.Color = Color.Lerp(Fill.Color, Color.White, i);
                yield return null;
            }
            Fill.Color = Color.White;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Fill.Color = Color.Lerp(Color.White, Color.ForestGreen, Ease.SineOut(i));
                yield return null;
            }
            yield return null;
            player.ForceCameraUpdate = true;
            player.StateMachine.State = Player.StNormal;
            if (!idEmpty && !PianoModule.Session.DrillBatteryIds.Contains(SwitchID))
            {
                foreach (BatterySwitchPlate plate in level.Tracker.GetEntities<BatterySwitchPlate>())
                {

                    if (!plate.Activated && plate.BatteryID == SwitchID)
                    {
                        plate.Activate(false);
                        PianoModule.Session.DrillBatteryIds.Add(SwitchID);
                        break;
                    }
                }
            }

            yield return null;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Fill.Color = Activated ? Color.Green : Color.Red;
            Handle.Play(Activated ? "idleUp" : "idleDown");
        }
        public override void Render()
        {
            //Back.DrawSimpleOutline();
            base.Render();
            //Handle.DrawSimpleOutline();
            Handle.Render();
        }
    }
}