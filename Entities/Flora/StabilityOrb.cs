using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [ConstantEntity("PuzzleIslandHelper/StabilityOrb")]
    [Tracked]
    public class StabilityOrb : Entity
    {
        [CustomEntity("PuzzleIslandHelper/StabilityOrbTrigger")]
        [Tracked]
        public class StabilityOrbTrigger : Trigger
        {
            public string ID;
            public FlagData Flag;
            public StabilityOrbTrigger(EntityData data, Vector2 offset) : base(data, offset)
            {
                ID = data.Attr("areaID");
                Flag = new FlagData("StabilityOrbTrigger:{" + ID + '}');
            }
            public static bool GetState(Scene scene, string id) => (scene as Level).Session.GetFlag("StabilityOrbTrigger:{" + id + '}');
        }
        public GlobalFrequencyReceiver FrequencyReceiver;
        private float glow;
        public List<StabilityOrbTrigger> CollidingAreas = [];
        public Sprite Sprite;
        private Image white;
        private SineWave sine;
        public StabilityOrb() : base()
        {
            Visible = false;
            Tag |= Tags.Global | Tags.Persistent | Tags.TransitionUpdate;
            Add(FrequencyReceiver = new GlobalFrequencyReceiver([18, 1, 14, 9])
            {
                RequiresAudibleSound = true,
                StopAtFullPower = false,
                OnFullPower = () =>
                {
                    if (Scene.GetPlayer() is not Player player) return;
                    Vector2 position = player.TopCenter - Vector2.UnitY * 16;
                    if (Position == position) return;
                    Visible = true;
                    FrequencyReceiver.Delay = 0.5f;
                    RefreshAreaStates(position);
                    Tween.Set(this, Tween.TweenMode.Oneshot, 0.1f, Ease.CubeOut, t => { glow = t.Eased; },
                    t =>
                    {
                        SceneAs<Level>().Displacement.AddBurst(Sprite.RenderPosition, 0.3f, 0f, 40f);
                        Position = position;
                        SceneAs<Level>().Displacement.AddBurst(Sprite.RenderPosition, 0.3f, 0f, 40f);
                        Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.CubeIn, t => { glow = 1 - t.Eased; });
                    });
                },
            });

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/stabilityOrb"));
            Sprite.AddLoop("idle", "", 0.1f);
            Sprite.Play("idle");
            Sprite.CenterOrigin();
            Collider = Sprite.ColliderCentered();
            Add(white = new Image(GFX.Game["objects/PuzzleIslandHelper/stabilityOrbWhite"]));
            white.CenterOrigin();
            white.Color = Color.Transparent;
            Add(sine = new SineWave(0.5f)
            {
                UseRawDeltaTime = true,
                OnUpdate = (f) =>
                {
                    white.Y = Sprite.Y = f * 4;
                }
            });
        }
        public override void Update()
        {
            base.Update();
            white.Color = Color.White * glow;
            if (this.OnScreen())
            {
                if (Scene.OnInterval(1))
                {
                    Pulse.Circle(this, Pulse.Fade.Late, Pulse.Mode.Oneshot, Sprite.Position.YComp(), 0, Width * 2, 0.5f, true, Color.Blue, Color.White, null, Ease.SineOut, Sprite);
                }
            }
        }
        public void RefreshAreaStates(Vector2 nextPosition)
        {
            Vector2 position = Position;
            foreach (StabilityOrbTrigger trigger in CollidingAreas)
            {
                trigger.Flag.State = false;
            }
            CollidingAreas.Clear();
            Position = nextPosition;
            foreach (StabilityOrbTrigger trigger in CollideAll<StabilityOrbTrigger>())
            {
                CollidingAreas.Add(trigger);
                trigger.Flag.State = true;
            }
            Position = position;
        }
    }

}
