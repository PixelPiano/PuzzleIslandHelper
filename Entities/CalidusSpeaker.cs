﻿using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CalidusSpeaker")]
    [Tracked]

    public class CalidusSpeaker : Entity
    {
        public bool Calidus;
        public static MTexture DebugTex = GFX.Game["objects/PuzzleIslandHelper/wip/texture"];
        public enum Cutscenes
        {
            Maddy0,
            Calidus1,
            Maddy1,
            Calidus2,
            Maddy2,
            Calidus3,
            Maddy3
        }

        public DotX3 Talk;
        public string TeleportTo;
        public float FadeTime;
        public float WaitTime;
        public CalidusSpeaker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(DebugTex.Width, DebugTex.Height);
            Add(new Image(DebugTex));
            Add(Talk = new(Collider, Interact));
            TeleportTo = data.Attr("teleportTo");
            Calidus = data.Bool("calidus");
            FadeTime = data.Float("fadeTime");
            WaitTime = data.Float("waitTime");

        }
        public void Interact(Player player)
        {
            ;
            Scene.Add(new CalidusSpeakerCutscene(Cutscenes.Maddy0, false, TeleportTo, FadeTime, WaitTime));
        }
        public static void InstantTeleportToSpawn(Level level, string room)
        {
            Player player = level.GetPlayer();
            bool roomHasCalidus = PianoMapDataProcessor.CalidusSpawners.ContainsKey(room);
            bool isCalidus = player is PlayerCalidus;
            if (level == null || player is null || string.IsNullOrEmpty(room))
            {
                return;
            }
            level.OnEndOfFrame += delegate
            {
                Vector2 levelOffset = level.LevelOffset;
                Vector2 val2 = player.Position - level.LevelOffset;
                Vector2 val3 = level.Camera.Position - level.LevelOffset;
                Facings facing = player.Facing;
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = room;
                Session session = level.Session;
                Level level2 = level;
                Rectangle bounds = level.Bounds;
                float num = bounds.Left;
                bounds = level.Bounds;
                session.RespawnPoint = level2.GetSpawnPoint(new Vector2(num, bounds.Top));
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.None);
                Vector2 val4 = level.DefaultSpawnPoint - level.LevelOffset - val2;
                if (roomHasCalidus != isCalidus)
                {
                    if (isCalidus)
                    {
                        player = new Player(player.Position, PlayerSpriteMode.Madeline);
                    }
                    else
                    {
                        player = new PlayerCalidus(player.Position, PianoMapDataProcessor.CalidusSpawners[room]);
                    }
                }
                level.Camera.Position = level.LevelOffset + val3;
                player.Position = session.RespawnPoint.HasValue ? session.RespawnPoint.Value : level.DefaultSpawnPoint;
                player.Facing = facing;
                if (!isCalidus)
                {
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset + val4);
                }
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
        public class CalidusSpeakerCutscene : CutsceneEntity
        {
            public Cutscenes Cutscene;
            public bool RespectSession;
            public string TeleportTo;
            public float FadeTime;
            public float WaitTime;
            public CalidusSpeakerCutscene(Cutscenes cutscene, bool respectSession, string teleportTo, float fadeTime, float waitTime) : base()
            {
                Cutscene = cutscene;
                RespectSession = respectSession;
                TeleportTo = teleportTo;
                FadeTime = fadeTime;
                WaitTime = waitTime;
            }
            public override void OnBegin(Level level)
            {
                if (level.GetPlayer() is not Player player)
                {
                    RemoveSelf();
                    return;
                }
                if (!GetFlag())
                {
                    Func<Player, IEnumerator> scene = Cutscene switch
                    {
                        Cutscenes.Maddy0 => Maddy0,
                        Cutscenes.Maddy1 => Maddy1,
                        Cutscenes.Maddy2 => Maddy2,
                        Cutscenes.Maddy3 => Maddy3,
                        Cutscenes.Calidus1 => Calidus1,
                        Cutscenes.Calidus2 => Calidus2,
                        Cutscenes.Calidus3 => Calidus3,
                        _ => null
                    };
                    if (scene != null)
                    {
                        if (Cutscene.ToString().ToLower().Contains("calidus"))
                        {
                            Add(new Coroutine(scene(player as PlayerCalidus)));
                        }
                        else
                        {
                            Add(new Coroutine(scene(player)));
                        }
                    }
                }
            }
            public IEnumerator Maddy0(Player player)
            {
                //static noise
                //Maddy: hmm? 
                //Maddy: Oh it's the boss! Better listen closely.
                //Maddy: ...
                //Maddy: Uh. Hello? Boss?
                //Maddy: ...
                //Maddy: I... does this means I can leave early?

                yield return null;
                EndCutscene(Level);
            }
            public IEnumerator Calidus1(Player player)
            {
                //static noise, but maddy can be heard saying "hello?"
                //calidus: Hello? Is someone there?
                //calidus: Can you hear me??
                //calidus: Hello? ...Hello??
                //*static noise cuts out*
                //calidus: Drat.
                yield return null;
                EndCutscene(Level);
            }
            public IEnumerator Maddy1(Player player)
            {

                //static noise
                //Maddy: hmm? 
                //Maddy: Oh it's the boss! Better listen closely.
                //Maddy: ...
                //Maddy: Uh. Hello? Boss?
                //Maddy: ...
                //Maddy: I... does this means I can leave early?
                //*static gets louder*
                //Maddy: Hey, I think something's wrong with the speaker.
                //Maddy:...
                //Maddy: Hey, dude!
                //Maddy: W-what..?
                //Maddy: Hey, snap out of it man!
                //*static + glitchy text from calidus*
                //Maddy: Who...
                //Maddy: No, wait!
                //Maddy: Why did that... feel familiar...
                yield return null;
                EndCutscene(Level);
            }
            public IEnumerator Calidus2(Player player)
            {
                yield return null;
                EndCutscene(Level);
            }
            public IEnumerator Maddy2(Player player)
            {
                if (player is not PlayerCalidus)
                {

                }
                yield return null;
                EndCutscene(Level);
            }
            public IEnumerator Calidus3(Player player)
            {
                yield return null;
                EndCutscene(Level);
            }
            public IEnumerator Maddy3(Player player)
            {
                if (player is not PlayerCalidus)
                {

                }
                yield return null;
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                SetFlag();
                if (!string.IsNullOrEmpty(TeleportTo))
                {
                    level.Add(new Fader(FadeTime, WaitTime, TeleportTo));
                }
            }
            public void SetFlag()
            {
                Level.Session.SetFlag("CalidusSpeakerCutscene_" + Cutscene.ToString() + "_Watched");
            }
            public bool GetFlag()
            {
                return !RespectSession || Level.Session.GetFlag("CalidusSpeakerCutscene_" + Cutscene.ToString() + "_Watched");
            }
            [Tracked]
            public class Fader : Entity
            {
                public float Alpha;
                public string TeleportTo;
                public float FadeDuration;
                public float PauseDuration;
                public Fader(float fadeDuration, float wait, string teleportTo) : base()
                {
                    Tag |= Tags.TransitionUpdate | Tags.Persistent;
                    FadeDuration = fadeDuration;
                    PauseDuration = wait;
                    Depth = -100000;
                    TeleportTo = teleportTo;
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, fadeDuration, true);
                    tween.OnUpdate = t => { Alpha = t.Eased; };
                    tween.OnComplete = delegate { Add(Alarm.Create(Alarm.AlarmMode.Oneshot, Teleport, wait / 2f, true)); };
                    Add(tween);
                }
                public void Teleport()
                {
                    if (!string.IsNullOrEmpty(TeleportTo))
                    {
                        InstantTeleportToSpawn(SceneAs<Level>(), TeleportTo);
                        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
                        {
                            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, FadeDuration, true);
                            tween.OnUpdate = t => { Alpha = 1 - t.Eased; };
                            tween.OnComplete = delegate { Alpha = 0; RemoveSelf(); };
                            Add(tween);
                        }, PauseDuration / 2f, true));
                    }
                    else
                    {
                        RemoveSelf();
                    }
                }
                public override void Render()
                {
                    base.Render();
                    if (Scene is Level level && Alpha > 0)
                    {
                        Draw.Rect(level.Camera.Position, 320, 180, Color.Black * Alpha);
                    }
                }
            }
        }
    }
}
