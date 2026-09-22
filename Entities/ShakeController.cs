using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SingularityShakeController")]
    [Tracked]
    public class ShakeController : Entity
    {
        public static string[] Percents = [
            "SingularitySlam:Delay",
            "SingularitySlam:WindUp",
            "SingularitySlam:Charge",
            "SingularitySlam:Shake",
            "SingularitySlam:RecoilDelay",
            "SingularitySlam:Recoil",
            ];
        public static string[] Durations = [
            "SingularitySlam:DelayTime",
            "SingularitySlam:WindUpTime",
            "SingularitySlam:ChargeTime",
            "SingularitySlam:ShakeTime",
            "SingularitySlam:RecoilDelayTime",
            "SingularitySlam:RecoilTime",
            ];
        public static string ShakeMultSlider = "SingularitySlam:ShakeMult";

        [Tracked]
        public class PersistentShake : Entity
        {
            public LevelShakeModifier ShakeMod;
            private float stateTimer;
            public enum States
            {
                Delay,
                WindUp,
                Charge,
                Shake,
                RecoilDelay,
                Recoil
            }
            private States state;
            public bool Started;
            public PersistentShake()
            {
                Collider = new Hitbox(32, 32);
                Tag |= Tags.Persistent | TagsExt.SubHUD;
                Add(ShakeMod = new LevelShakeModifier(1));
            }
            [OnLoad]
            public static void Load()
            {
                Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
            }
            [OnUnload]
            public static void Unload()
            {
                Everest.Events.Level.OnTransitionTo -= Level_OnTransitionTo;
            }
            private static void Level_OnTransitionTo(Level level, LevelData next, Vector2 direction)
            {
                PersistentShake shaker = level.Tracker.GetEntity<PersistentShake>();
                if (shaker != null)
                {
                    if (!(OrbsMergedFlag && !SingularitySlamFlag) || next.Entities.Find(item => item.Name == "PuzzleIslandHelper/SingularityShakeController") == null)
                    {
                        shaker.RemoveSelf();
                    }
                }
            }

            public float Duration;
            public override void Render()
            {
                base.Render();
                return;
                Vector2 p = Vector2.Zero;
                ActiveFont.Draw("DelayPercent:" + SceneAs<Level>().Session.GetSlider(Percents[0]), p, Color.White);
                p.Y += ActiveFont.LineHeight;
                ActiveFont.Draw("WindUpPercent:" + SceneAs<Level>().Session.GetSlider(Percents[1]), p, Color.White);
                p.Y += ActiveFont.LineHeight;
                ActiveFont.Draw("ChargePercent:" + SceneAs<Level>().Session.GetSlider(Percents[2]), p, Color.White);
                p.Y += ActiveFont.LineHeight;
                ActiveFont.Draw("ShakePercent:" + SceneAs<Level>().Session.GetSlider(Percents[3]), p, Color.White);
                p.Y += ActiveFont.LineHeight;
                ActiveFont.Draw("RecoilDelayPercent:" + SceneAs<Level>().Session.GetSlider(Percents[4]), p, Color.White);
                p.Y += ActiveFont.LineHeight;
                ActiveFont.Draw("RecoilPercent:" + SceneAs<Level>().Session.GetSlider(Percents[5]), p, Color.White);
                p.Y += ActiveFont.LineHeight;
                ActiveFont.Draw("SingleShake:" + ShakeMod.ShakeVector * ShakeMod.Mult, p, Color.White);
                p.Y += ActiveFont.LineHeight;
                ActiveFont.Draw("Mult:" + ShakeMod.Mult, p, Color.White);

            }

            public void ManualUpdate()
            {
                Session session = SceneAs<Level>().Session;
                float duration = session.GetSlider(Durations[(int)state]);
                Duration = duration;
                for (int i = 0; i < Percents.Length; i++)
                {
                    session.SetSlider(Percents[i], i != (int)state ? 0 : stateTimer / duration);
                }
                stateTimer += Engine.DeltaTime;
                if (stateTimer >= duration)
                {
                    state = (States)(((int)state + 1) % 6);
                    stateTimer = 0;
                    if (state == States.Shake)
                    {
                        ShakeMod.ShakeFor(-1);
                    }
                    else
                    {
                        ShakeMod.StopShaking();
                    }
                    session.SetCounter("SingularitySlam:State", (int)state);
                }

                ShakeMod.Mult = session.GetSlider(ShakeMultSlider) * (1 - session.GetSlider(Percents[(int)States.Shake]));

            }
            public override void Update()
            {
                Position = SceneAs<Level>().Camera.Position;
                if (OrbsMergedFlag && !SingularitySlamFlag)
                {
                    if (!Started)
                    {
                        Start(Scene, true);
                    }
                    ManualUpdate();
                }
                else if (Started)
                {
                    Stop();
                }
                if (Scene.Tracker.GetEntity<ShakeController>() == null)
                {
                    Stop();
                }
                base.Update();
            }
            public void Stop()
            {
                ShakeMod.StopShaking();
                ShakeMod.Mult = 0;
                stateTimer = 0;
                Started = false;
            }
            public void Start(Scene scene, bool reset = false)
            {
                if (reset)
                {
                    stateTimer = 0;
                    state = States.Shake;
                    ShakeMod.ShakeFor(-1);
                }
                ShakeMod.Mult = 1 + (scene as Level).Session.GetSlider(ShakeMultSlider);
                Started = true;
            }
        }
        public readonly static FlagData OrbsMergedFlag = new FlagData("OrbsMerged");
        public readonly static FlagData SingularitySlamFlag = new FlagData("SingularitySlam");
        public float Mult;
        public float Delay;
        public float Impact, WindUp, Charge, RecoilDelay, Recoil;
        public ShakeController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(32, 32);
            Mult = data.Float("mult");

            Delay = data.Float("delay");
            WindUp = data.Float("windUp");
            Charge = data.Float("charge");
            Impact = data.Float("impact");
            RecoilDelay = data.Float("recoilDelay");
            Recoil = data.Float("recoil");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Session.GetFlag("OrbsMerged"))
            {
                (scene as Level).Session.SetFlag("OrbsSlamming");
            }
            PersistentShake shaker = scene.Tracker.GetEntity<PersistentShake>();
            if (shaker == null)
            {
                scene.Add(shaker = new PersistentShake());
            }
            if (OrbsMergedFlag && !SingularitySlamFlag)
            {
                UpdateSliderValues();
                shaker.Start(scene, false);
            }
            else
            {
                ClearSliderValues(scene);
                shaker.Stop();
            }
        }
        public void UpdateSliderValues()
        {
            Session session = SceneAs<Level>().Session;
            session.SetSlider(Durations[0], Delay);
            session.SetSlider(Durations[1], WindUp);
            session.SetSlider(Durations[2], Charge);
            session.SetSlider(Durations[3], Impact);
            session.SetSlider(Durations[4], RecoilDelay);
            session.SetSlider(Durations[5], Recoil);
            session.SetSlider(ShakeMultSlider, Mult);
        }
        public static void ClearSliderValues(Scene scene)
        {
            Session session = (scene as Level).Session;
            session.SetSlider(Durations[0], 0);
            session.SetSlider(Durations[1], 0);
            session.SetSlider(Durations[2], 0);
            session.SetSlider(Durations[3], 0);
            session.SetSlider(Durations[4], 0);
            session.SetSlider(Durations[5], 0);
            session.SetSlider(ShakeMultSlider, 0);
        }
    }
}