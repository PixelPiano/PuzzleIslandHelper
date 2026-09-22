using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FrostHelper.ModIntegration;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEvent("PuzzleIslandHelper/RedOrbCollect")]
    [Tracked]
    public class RedOrbGetCutscene : CutsceneEntity
    {
        //[ConstantEntity("effectTest")]
        public class EffectTest : Entity
        {
            public VirtualRenderTarget Target;
            public VirtualRenderTarget Shadow;
            public VirtualRenderTarget Shard;
            public MTexture Surface;
            public MTexture Reference;
            public bool Index0 = false;
            public bool EffectOn = true;
            public bool ShadowOn = true;
            public float Yaw, Pitch, Roll;
            private int ind = 0;
            public Matrix RotationMatrix;
            //roll == y
            //yaw == x
            public EffectTest() : base()
            {
                KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.E, () =>
                {
                    EffectOn = !EffectOn;
                });
                KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.T, () =>
                {
                    rotation += Engine.DeltaTime * 5f;
                }).OncePerInput = false;
                Tag |= Tags.Persistent;
                Depth = int.MinValue + 1000;
                /////////////////////////////////////////////
                Target = VirtualContent.CreateRenderTarget("aaaa", 320, 180);
                Shadow = VirtualContent.CreateRenderTarget("aaaa", 320, 180);
                Add(new BeforeRenderHook(() =>
                {
                    //SHARD.PREPARETARGET()
                    Shard.SetAsTarget(true);
                    Effect effect = EffectOn ? ShaderHelperIntegration.TryGetEffect("mirrorMemoryGlass") : null;
                    if (effect != null)
                    {
                        effect.ApplyIdentityParameters(SceneAs<Level>());
                        effect.Parameters["Rotation"]?.SetValueTranspose(RotationMatrix);
                    }
                    var tex1 = Engine.Graphics.GraphicsDevice.Textures[1];
                    Engine.Graphics.GraphicsDevice.Textures[1] = Reference.Texture.Texture_Safe;
                    Draw.SpriteBatch.StandardBegin(effect);
                    Surface.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1, 0, SpriteEffects.None);
                    Draw.SpriteBatch.End();
                    Engine.Graphics.GraphicsDevice.Textures[1] = tex1;
                    //
                    Target.SetAsTarget(true);
                    Draw.SpriteBatch.StandardBegin();
                    //FOREACH SHARD
                    Draw.SpriteBatch.Draw(Shard, ShardPosition, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //
                    Draw.SpriteBatch.End();

                    Shadow.SetAsTarget(true);
                    if (ShadowOn)
                    {
                        Draw.SpriteBatch.StandardBegin();
                        //FOREACH SHARD
                        Draw.SpriteBatch.Draw(Shard, ShardPosition + Calc.AngleToVector(rotation, 2), Color.DarkGray);
                        Draw.SpriteBatch.Draw(Shard, ShardPosition + Calc.AngleToVector(rotation, 4), Color.White);
                        //
                        Draw.SpriteBatch.End();

                        Draw.SpriteBatch.StandardBegin(TrailManager.MaxBlendState);
                        Draw.Rect(0f, 0f, Shadow.Width, Shadow.Height, Color.White);
                        Draw.SpriteBatch.End();
                    }
                }));
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.Rect(0, 0, 24, 24, Index0 ? Color.Lime : Color.Red);
                Draw.Rect(24, 0, 24, 24, EffectOn ? Color.Lime : Color.Red);
                Draw.Rect(48, 0, 24, 24, ShadowOn ? Color.Lime : Color.Red);

            }
            public Vector2 CenterOffset;
            public Vector2 ShardPosition => new Vector2(320, 180) / 2 - (new Vector2(Shard.Width, Shard.Height) / 2) + CenterOffset;
            private float rotation;
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Surface = GFX.Game["objects/PuzzleIslandHelper/circle"];
                Reference = GFX.Game["objects/PuzzleIslandHelper/debugEmil"];
                Shard = VirtualContent.CreateRenderTarget("aaaaaaa", Surface.Width, Surface.Height);
            }
            public override void Update()
            {
                base.Update();
                CenterOffset = Calc.AngleToVector(rotation, 40);
                Pitch = ((1 - (CenterOffset.Y / 120)) * MathHelper.PiOver2 - MathHelper.PiOver2) * -1;
                Yaw = ((1 - (CenterOffset.X / 120)) * MathHelper.PiOver2 - MathHelper.PiOver2) * 1;

                Yaw %= MathHelper.TwoPi;
                Pitch %= MathHelper.TwoPi;
                Roll %= MathHelper.TwoPi;
                RotationMatrix = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll);
            }
            public override void Render()
            {
                base.Render();
                Vector2 p = SceneAs<Level>().Camera.Position;
                //Draw.SpriteBatch.Draw(Shard, p, Color.White);
                Draw.SpriteBatch.Draw(Shadow, p, Color.Blue);
                Draw.SpriteBatch.Draw(Target, p, Color.White);
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
                Shard?.Dispose();
                Shadow?.Dispose();
            }
        }
        private class audioFreezer : Component
        {
            private EventInstance instance;
            private int position;
            private int frameStutter = 0;
            private int frameCount;
            public audioFreezer(EventInstance instance, int frameStutter) : base(true, false)
            {
                this.instance = instance;
                this.frameStutter = frameStutter;
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                if (instance.getDescription(out EventDescription d) == FMOD.RESULT.OK && d.getLength(out int length) == FMOD.RESULT.OK && instance.getTimelinePosition(out int p) == FMOD.RESULT.OK)
                {
                    position = p;
                }
                else
                {
                    RemoveSelf();
                }
            }
            public override void Update()
            {
                base.Update();
                if (frameCount < frameStutter)
                {
                    frameCount++;
                }
                else if (instance.getPlaybackState(out PLAYBACK_STATE state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
                {
                    frameCount = 0;
                    instance.setTimelinePosition(position);
                }
            }
        }
        public class MirrorMemory : Entity
        {
            public float GlitchAmount = 0;
            public RedMemoryOrb Orb;
            public Player Player;
            public TempleMirrorPI Mirror;
            public Characters characters;
            public Outline outline;
            public Inside inside;

            public MirrorMemory(Vector2 cameraPosition, RedMemoryOrb orb, Player player, TempleMirrorPI mirror) : base(cameraPosition)
            {
                Depth = int.MinValue + 10;
                Orb = orb;
                Player = player;
                Mirror = mirror;
                characters = new Characters(mirror, Orb, Player);
                outline = new Outline(Mirror.Collider.AbsolutePosition, Mirror.Width, Mirror.Height);
                inside = new Inside(Mirror);
                inside.Depth = Depth - 1;
                characters.Depth = Depth - 2;
                outline.Depth = Depth - 3;
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                characters?.RemoveSelf();
                outline?.RemoveSelf();
                inside?.RemoveSelf();
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                scene.Add(characters, outline, inside);
            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(X, Y, 320, 180, Color.Black);
            }
            public class Characters : Entity
            {
                public VertexOrb VertexOrb;
                public Player Player;
                public VirtualRenderTarget Target;
                public TempleMirrorPI Mirror;
                public Vector2 PlayerMarker;
                public Vector2 OrbMarker;
                public Characters(TempleMirrorPI mirror, RedMemoryOrb copyFrom, Player player) : base()
                {
                    Player = player;
                    Mirror = mirror;
                    Add(VertexOrb = new VertexOrb(copyFrom.Orb, true));
                    VertexOrb.EdgeColor = Color.Red;
                    VertexOrb.Color = Color.Transparent;
                    VertexOrb.CustomGetCenterColor = null;
                    VertexOrb.CustomGetEdgeColor = VertexOrb.CustomGetFillColor = null;
                    Target = VirtualContent.CreateRenderTarget("MirrorMemoryOrb", 320, 180);
                    Add(new BeforeRenderHook(() =>
                    {
                        Vector2 cam = SceneAs<Level>().Camera.Position;
                        VirtualRenderTarget temp = GameplayBuffers.TempA;
                        temp.SetAsTarget(true);
                        PianoUtils.DrawUserPrimitives<VertexPositionColor>(Matrix.Identity, (e) =>
                        {
                            VertexOrb.DirectRenderVertices();
                        });
                        Vector2 playerRenderPosition = PlayerMarker - cam;
                        Vector2 prev = Player.Position;
                        Player.Position = playerRenderPosition;
                        Player.Hair.MoveHairBy(playerRenderPosition - prev);
                        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);
                        Player.Render();
                        Draw.SpriteBatch.End();
                        Player.Position = prev;
                        Player.Hair.MoveHairBy(-(playerRenderPosition - prev));

                        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, TrailManager.MaxBlendState);
                        Draw.Rect(0f, 0f, temp.Width, temp.Height, new Color(1f, 1f, 1f, 1f));
                        Draw.SpriteBatch.End();

                        Effect effect = null;//ShaderHelper.TryGetEffect("outline");
                        /*                        effect.ApplyMatrixParameters(Matrix.Identity);
                                                effect.Parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * (GameplayBuffers.Gameplay.Width / 320));*/
                        Target.SetAsTarget(true);
                        Draw.SpriteBatch.StandardBegin(effect);
                        Draw.SpriteBatch.Draw(temp, Vector2.Zero, Color.White);
                        Draw.SpriteBatch.End();
                    }));
                }
                public override void Update()
                {
                    VertexOrb.Position = OrbMarker - SceneAs<Level>().Camera.Position;
                    base.Update();
                }
                public void SwapMemoryPosition(int index)
                {
                    Marker marker = Marker.Find("marker" + index);
                    int index2 = index switch
                    {
                        0 => 3,
                        1 => 2,
                        2 => 1,
                        3 => 0
                    };
                    Marker marker2 = Marker.Find("marker" + index2);
                    if (marker != null && marker2 != null)
                    {
                        PlayerMarker = new Vector2(marker.X + 3, Player.Y - 8);
                        OrbMarker = new Vector2(marker2.X + 3, Mirror.Y);
                        if (marker.Args.TryGetValue("facing", out string facing))
                        {
                            string lower = facing.ToLower();
                            Player.Facing = lower switch
                            {
                                "left" => Facings.Left,
                                "right" => Facings.Right,
                                "random" => Calc.Random.Choose(Facings.Left, Facings.Right),
                                _ => Player.Facing
                            };
                        }
                    }
                }
                public override void Render()
                {
                    base.Render();
                    Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White);
                }
                public override void DebugRender(Camera camera)
                {
                    base.DebugRender(camera);
                    Draw.Point(PlayerMarker, Color.Red);
                    Draw.Circle(OrbMarker, 10, Color.Magenta, 10);
                }
                public override void Removed(Scene scene)
                {
                    base.Removed(scene);
                    Target?.Dispose();
                }
            }
            public class Outline : Entity
            {
                private static string path = "objects/PuzzleIslandHelper/mirrorCracks/memoryFrameWiggle";
                private class texture : Image
                {
                    public List<MTexture> frames = [];
                    public float interval = 0.1f;
                    private float timer;
                    private int index;
                    private int x, y, w, h;
                    public override void Added(Entity entity)
                    {
                        base.Added(entity);
                        var f = GFX.Game.GetAtlasSubtextures(path);
                        foreach (var frame in f)
                        {
                            frames.Add(frame.GetSubtexture(x, y, w, h));
                        }
                        timer = interval;
                        Texture = frames[0];
                    }
                    public texture(int x, int y, int w, int h) : base(null, true)
                    {
                        this.x = x;
                        this.y = y;
                        this.w = w;
                        this.h = h;
                    }
                    public override void Update()
                    {
                        base.Update();
                        timer -= Engine.DeltaTime;
                        if (timer < 0)
                        {
                            timer = interval;
                            index = (index + 1) % frames.Count;
                            Texture = frames[index];
                        }
                    }
                }
                public Outline(Vector2 position, float width, float height) : base(position)
                {
                    Collider = new Hitbox(width, height);
                    Tag |= Tags.TransitionUpdate;
                }
                public override void Added(Scene scene)
                {
                    base.Added(scene);
                    Add(new texture(0, 0, 8, 8));
                    Add(new texture(16, 0, 8, 8) { X = Width - 8 });
                    Add(new texture(0, 16, 8, 8) { Y = Height - 8 });
                    Add(new texture(16, 16, 8, 8) { X = Width - 8, Y = Height - 8 });
                    for (float i = 8; i < Width - 8; i += 8)
                    {
                        Add(new texture(8, 0, 8, 8) { X = i });
                        Add(new texture(8, 16, 8, 8) { X = i, Y = Height - 8 });
                    }
                    for (float i = 8; i < Height - 8; i += 8)
                    {
                        Add(new texture(0, 8, 8, 8) { Y = i });
                        Add(new texture(16, 8, 8, 8) { Y = i, X = Width - 8 });
                    }
                    float floorY = 0;
                    Level level = Scene as Level;
                    while (!level.CollideCheck<SolidTiles>(BottomCenter + floorY * Vector2.UnitY))
                    {
                        floorY++;
                        if (Bottom + floorY > level.Bounds.Bottom)
                        {
                            RemoveSelf();
                            return;
                        }
                    }
                    for (float i = level.Bounds.Left; i < level.Bounds.Right; i += 8)
                    {
                        Add(new texture(8, 0, 8, 8) { X = i, Y = Height + floorY });
                    }
                }
            }
            public class Outside : Entity
            {
                private static string[] paths;
                [OnLoad]
                public static void Load()
                {
                    paths = Directory.GetFiles("Mods\\PuzzleIslandHelper\\Graphics\\Atlases\\Gameplay\\objects\\PuzzleIslandHelper\\mirrorCracks\\memory");
                }
                public List<Shard> Shards = [];
                public Outside() : base()
                {
                    Add(new BeforeRenderHook(() =>
                    {
                        Effect effect = null;
                        if (effect != null)
                        {
                            effect.ApplyIdentityParameters(SceneAs<Level>());
                        }
                        foreach (Shard shard in Shards)
                        {
                            shard.Target.SetAsTarget(true);
                            if (effect != null)
                            {
                                effect.Parameters["Rotation"].SetValueTranspose(shard.Rotation);
                                effect.Parameters["Angle"].SetValue(shard.Angle);
                            }
                            Draw.SpriteBatch.StandardBegin(effect);
                            shard.Texture.Draw(shard.Position + Vector2.One * shard.Padding);
                            Draw.SpriteBatch.End();
                        }
                    }));
                }
                public override void Render()
                {
                    base.Render();
                    Vector2 p = SceneAs<Level>().Camera.Position;
                    foreach (Shard shard in Shards)
                    {
                        Draw.SpriteBatch.Draw(shard.Target, p + shard.Position - Vector2.One * shard.Padding, Color.White);
                    }
                }
                public override void Added(Scene scene)
                {
                    base.Added(scene);
                    foreach (string s in paths)
                    {
                        Shard shard = new Shard(s);
                        scene.Add(shard);
                        Shards.Add(shard);
                    }
                }
                public override void Removed(Scene scene)
                {
                    base.Removed(scene);
                    foreach (Shard shard in Shards) shard.RemoveSelf();
                }
                public class Shard : Entity
                {
                    public VirtualRenderTarget Target;
                    public MTexture Texture;
                    public int Padding;
                    public Vector2 Offset;
                    public Matrix Rotation = Matrix.Identity;
                    public float Angle;
                    public Shard(string path, int padding = 0) : base()
                    {
                        Texture = GFX.Game[path];
                        if (path.Contains("_X") && path.Contains('Y'))
                        {
                            int xIndex = path.LastIndexOf('X');
                            int yIndex = path.LastIndexOf('Y');
                            if (yIndex - xIndex > 1)
                            {
                                string xP = path.Substring(xIndex + 1, (yIndex - xIndex) - 1);
                                if (int.TryParse(xP, out int x))
                                {
                                    X = x;
                                }
                            }
                            if (path.Length - yIndex > 1)
                            {
                                string yP = path.Substring(yIndex + 1);
                                if (int.TryParse(yP, out int y))
                                {
                                    Y = y;
                                }
                            }
                        }
                        Target = VirtualContent.CreateRenderTarget("shard target:" + path, Texture.Width + Padding * 2, Texture.Height + Padding * 2);
                        Collider = new Hitbox(Target.Width, Target.Height, -padding, -padding);
                    }
                    public override void Removed(Scene scene)
                    {
                        base.Removed(scene);
                        Target?.Dispose();
                    }
                }

            }
            public class Inside : Entity
            {
                public VirtualRenderTarget Target;
                public TempleMirrorPI Mirror;
                public Inside(TempleMirrorPI mirror)
                {
                    Mirror = mirror;
                    Target = VirtualContent.CreateRenderTarget("RedOrbGetCutscene:MemoryMirrorInside target", 320, 180);
                    Add(new BeforeRenderHook(() =>
                    {

                    }));
                }
                public override void Render()
                {
                    base.Render();
                    if (SceneAs<Level>() is Level level)
                    {
                        Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
                    }
                }
                public override void Removed(Scene scene)
                {
                    base.Removed(scene);
                    Target?.Dispose();
                }
            }
        }
        public RedMemoryOrb ReflectionOrb;
        public RedMemoryOrb FollowingOrb;
        public Player Player;
        public TempleMirrorPI Mirror;
        public bool Flickering;
        public float FlickerInterval = 0.07f;
        private bool flickerMax;
        private float flickerProgress;
        public float WhiteFade;
        public bool TiePosition;
        public bool WhiteFadeIn;
        public float ShadeAmount;
        public Alarm SmashAlarm;
        public VertexOrb.ColorMod SmashMod;
        private float smashflickerinterval = 0.08f;
        public float SmashColorModAmount;
        public MirrorMemory Memory;
        public RedOrbGetCutscene(EventTrigger trigger, Player player, string eventID) : base(false, false)
        {
            Depth = int.MinValue;
            Player = player;
            Add(SmashAlarm = Alarm.Create(Alarm.AlarmMode.Persist, () => SmashColorModAmount = 0, 0.3f));
            glitchIn = Tween.Create(Tween.TweenMode.Persist, Ease.CubeIn, 1);
            glitchOut = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, 1);
            glitchIn.OnUpdate = (t) => Glitch.Value = Calc.LerpClamp(glitchFrom, 1, t.Eased);
            glitchOut.OnUpdate = (t) => Glitch.Value = 1 - t.Eased;
            glitchIn.OnComplete = (t) =>
            {
                warped = true;
                SwapMemoryPosition();
                Glitch.Value = 1;
                glitchOut.Start();
            };
            glitchIn.OnStart = (t) =>
            {
                warped = false;
                glitchOut.Stop();
                glitchFrom = Glitch.Value;
            };
            glitchOut.OnComplete = (t) => Glitch.Value = 0;
            Add(glitchIn, glitchOut);
        }
        public override void OnBegin(Level level)
        {
            foreach (RedMemoryOrb orb in level.Tracker.GetEntities<RedMemoryOrb>())
            {
                if (orb.Reflection)
                {
                    ReflectionOrb = orb;
                }
                else if (orb.Follows && orb.Global)
                {
                    FollowingOrb = orb;
                }
                if (ReflectionOrb != null && FollowingOrb != null) break;
            }
            Mirror = level.Tracker.GetEntity<TempleMirrorPI>();
            if (ReflectionOrb != null && FollowingOrb != null && Mirror != null)
            {
                FollowingOrb.Orb.CustomGetCenterColor.Add(CenterColorMod);
                FollowingOrb.Orb.CustomGetEdgeColor.Add(EdgeColorMod);
                FollowingOrb.Orb.CustomGetFillColor.Add(FillColorMod);
                ReflectionOrb.Orb.CustomGetCenterColor.Add(CenterColorMod);
                ReflectionOrb.Orb.CustomGetEdgeColor.Add(EdgeColorMod);
                ReflectionOrb.Orb.CustomGetFillColor.Add(FillColorMod);
                Add(new Coroutine(Routine()));
            }
        }
        public override void Update()
        {
            base.Update();
            if (Flickering && Scene.OnInterval(FlickerInterval))
            {
                if (flickerMax)
                {
                    FollowingOrb.RedAlpha = Calc.LerpClamp(RedMemoryOrb.HiddenAlpha, 1, flickerProgress);
                    flickerMax = false;
                }
                else
                {
                    FollowingOrb.RedAlpha = 1;
                    flickerMax = true;
                }
            }
        }
        public override void OnEnd(Level level)
        {
            Player.StateMachine.Locked = false;
            Player.StateMachine.State = Player.StNormal;
            Level.Session.SetFlag("MirrorSmashed");
            Level.Session.SetFlag("notDust", false);
            OrbFlags.RedCollected = true;
            ReflectionOrb.RemoveSelf();
            FollowingOrb.RemoveSelf();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Mirror.Platform.Collidable = false;
            Memory?.RemoveSelf();
            (scene as Level).Session.SetFlag("notDust", false);
        }
        public override void Render()
        {
            base.Render();

            if (WhiteFade > 0)
            {
                Draw.Rect(SceneAs<Level>().Camera.Position, 320, 180, Color.White * WhiteFade);
            }
        }
        public IEnumerator Routine()
        {
            Player.StateMachine.State = 11;
            Player.StateMachine.Locked = true;
            Player.Dashes = 1;
            #region Finished
            Add(new Coroutine(CenterCamera()));
            yield return Player.DummyWalkToExact((int)Mirror.X);
            yield return 0.25f;
            yield return Player.DummyWalkToExact((int)Mirror.X - 16);
            FollowingOrb.State = MemoryOrb.StDummy;
            ReflectionOrb.State = MemoryOrb.StDummy;
            Vector2 orbRestPosition = FollowingOrb.Position;
            yield return 0.5f;
            yield return Player.DummyWalkToExact((int)Mirror.X + 16);
            yield return 0.5f;
            Player.Facing = Facings.Left;
            yield return 1.2f;
            yield return Player.DummyWalkToExact((int)Mirror.X);
            yield return 0.1f;
            Add(new Coroutine(Level.ZoomTo(new Vector2(160f, 90f), 2f, 5f)));

            yield return 2;
            ShakeBoth(0.5f, 1);
            yield return 1f;
            ShakeBoth(0.5f, 1);
            yield return 1f;

            VertexOrb.Shiver shiverA = ReflectionOrb.AddShiver(0.8f, 0, 2, 0, MathHelper.Pi, Ease.SineIn, Tween.TweenMode.Looping);
            VertexOrb.Shiver shiverB = FollowingOrb.AddShiver(0.8f, 0, 2, 0, MathHelper.Pi, Ease.SineIn, Tween.TweenMode.Looping);
            yield return 0.4f;
            FollowingOrb.AutoFade = false;
            FollowingOrb.ShakeFor(-1);
            ReflectionOrb.ShakeFor(-1);
            FollowingOrb.ShakeMult = ReflectionOrb.ShakeMult = default;
            FollowingOrb.ShiverMult = ReflectionOrb.ShiverMult = 1;
            yield return 0.2f;
            Add(new Coroutine(Player.DummyWalkTo((int)Player.X + 3, true, 0.8f)));
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2f)
            {
                FollowingOrb.ShakeMult = ReflectionOrb.ShakeMult = i * Vector2.One * 1.4f;
                shiverA.Duration = shiverB.Duration = Calc.LerpClamp(0.8f, 0.4f, i);
                shiverA.TargetMaxIntensity = shiverB.TargetMaxIntensity = Calc.LerpClamp(2, 8, i);
                shiverA.Area = shiverB.Area = Calc.AngleLerp(MathHelper.PiOver4, MathHelper.TwoPi, i);
                yield return null;
            }

            yield return 1;
            yield return FlickerRoutine(1.1f);
            FollowingOrb.RedAlpha = 1;
            FollowingOrb.RemoveTag(Tags.Global);
            FollowingOrb.RemoveTag(Tags.Persistent);
            float y = FollowingOrb.Y;
            bool walkedBack = false;
            //-red orb floats up, shaking, scared
            //words cycle faster and faster: No/Please/I can't/Don't //todo: brainstorm for this

            yield return PianoUtils.Lerp(Ease.SineOut, 2, f =>
            {
                if (f > 0.2f && !walkedBack)
                {
                    walkedBack = true;
                    Add(new Coroutine(Player.DummyWalkTo((int)Player.X + 8, true, 1.1f)));
                }
                ReflectionOrb.Y = FollowingOrb.Y = Calc.LerpClamp(y, Mirror.CenterY, f);
            }, true);

            //-until... Red orb smashes into mirror over and over again
            //-screen fades to white as smashing continues
            Coroutine smashCoroutine = null;
            smashCoroutine = new Coroutine(
                SmashRoutine(Mirror.Center, 52, 24, 0.5f, 0.4f, i =>
                {
                    Celeste.Freeze(0.05f);
                    FollowingOrb.ShakeFor(0.3f);
                    ReflectionOrb.ShakeFor(0.3f);
                    SceneAs<Level>().Shake(0.3f);
                    FollowingOrb.AddShiver(0.3f, 0, 8, 1, MathHelper.TwoPi, Ease.ExpoIn, Tween.TweenMode.Oneshot);
                    ReflectionOrb.AddShiver(0.3f, 0, 8, 1, MathHelper.TwoPi, Ease.ExpoIn, Tween.TweenMode.Oneshot);
                    if (i == 0)
                    {
                        //camera zooms back
                        Add(new Coroutine(Level.ZoomBack(0.6f)));
                        Add(new Coroutine(Player.DummyWalkTo((int)Mirror.Right - 8, true, 2)));
                        //maddy stumbles back
                    }
                    else if (i == 13)
                    {
                        //start fading in
                        Add(new Coroutine(PianoUtils.Lerp(Ease.SineIn, 3, f => WhiteFade = f, true)));
                    }
                    Mirror.SmashAt(FollowingOrb.Position.Round(), (FollowingOrb.Position - FollowingOrb.PreviousPosition) / Engine.DeltaTime * 0.5f);
                    SmashColorModAmount = 1;
                    SmashAlarm.Start();
                }, f =>
                {
                    ShadeAmount = Calc.LerpClamp(0.1f, 0, f);
                }, f =>
                {
                    ShadeAmount = Calc.LerpClamp(0, 0.1f, f);
                }));
            Add(smashCoroutine);
            while (WhiteFade < 1) yield return null;
            ShadeAmount = 0;
            yield return 1.4f;
            smashCoroutine.Cancel();
            #endregion
            //-screen fades back to red orb on ground, shivering
            //the mirror is shattered beyond recognition
            //maddy is sat on the ground, taken aback
            Level.ZoomSnap(new Vector2(160f, 90f), 2f);
            SetOrbPositions(orbRestPosition);
            float minInterval = 1;
            float maxInterval = 2;
            float shakeTime = 0.2f;
            float shakeMult = 1f;
            yield return null;
            IEnumerator intervalShake()
            {
                float frameInterval = 0.1f;
                int dir = 1;
                while (true)
                {
                    FollowingOrb.StopShaking();
                    ReflectionOrb.StopShaking();
                    float num = Calc.Random.NextFloat();
                    float frameTimer = frameInterval;
                    for (float i = 0; i < 1; i += Engine.DeltaTime / (minInterval + num * (maxInterval - minInterval)))
                    {
                        if (frameTimer > 0)
                        {
                            frameTimer -= Engine.DeltaTime;
                            if (frameTimer <= 0)
                            {
                                frameTimer = frameInterval * (1 - i * 0.5f);
                                float mult = i < 0.5f ? i / 0.5f : 1 - (i - 0.5f) / 0.5f;
                                ReflectionOrb.RenderOffset = FollowingOrb.RenderOffset = Vector2.UnitX * mult * dir;
                                dir *= -1;
                            }
                        }
                        yield return null;
                    }
                    yield return Calc.Random.Range(minInterval, maxInterval);
                    for (float i = 0; i < 1; i += Engine.DeltaTime / shakeTime) yield return null;
                }
            }
            Coroutine intervalShakeCoroutine = null;
            Add(intervalShakeCoroutine = new Coroutine(intervalShake()));

            for (float i = 0; i < 1; i += Engine.DeltaTime / 2f)
            {
                WhiteFade = Ease.SineInOut(1 - i);
                yield return null;
            }
            WhiteFade = 0;
            yield return 0.6f;
            Player.Facing = Facings.Left;
            Player.DummyAutoAnimate = true;
            Player.Sprite.Rate = 0.5f;

            //-maddy gets up and slowly walks over to the orb.
            float origDist = MathHelper.Distance(Player.CenterX, FollowingOrb.X);
            float dist = origDist;
            Camera camera = Level.Camera;
            float camX = Level.Camera.X;
            while (dist > FollowingOrb.Radius / 2)
            {
                Player.DummyMoving = false;
                bool prevInputDisabled = MInput.Disabled;
                MInput.Disabled = false;
                float target = 16f * Math.Clamp(Input.MoveX.Value, -1, 0);
                MInput.Disabled = prevInputDisabled;
                Player.DummyMoving = true;
                Player.Speed.X = Calc.Approach(Player.Speed.X, target, 1000f * Engine.DeltaTime);

                float lerp = 1 - (dist / origDist);

                //-the closer she gets, the more intense the orbs shivers get
                shakeMult = Calc.LerpClamp(1, 1.5f, dist);
                maxInterval = Calc.LerpClamp(2, 0.4f, lerp);
                minInterval = Calc.LerpClamp(1, 0.1f, lerp);
                yield return null;
                if (Player.CenterX < Mirror.CenterX)
                {
                    camera.X = camX - (Mirror.CenterX - Player.CenterX);
                }
                dist = MathHelper.Distance(Player.CenterX, FollowingOrb.X);
            }
            //-when she reaches the orb, she hugs it, and its shivers cease entirely.
            intervalShakeCoroutine.Cancel();

            FollowingOrb.RenderOffset = Vector2.Zero;
            ReflectionOrb.RenderOffset = Vector2.Zero;
            Player.Speed.X = 0;
            Add(new Coroutine(CameraTo(new Vector2(FollowingOrb.X - 160 / camera.Zoom, camera.Y), 0.7f, Ease.SineInOut)));
            //Player.Sprite.Play("hugOrb");
            //TODO: Make or get a sprite for Maddy hugging the orb
            yield return 1;

            //The orb flickers a little, then plays a sequence of frequencies at a very fast pace
            //Then, the scene freezes and the current sounds drag out
            /*            Add(new audioFreezer(Audio.currentAltMusicEvent, 1));
                        Add(new audioFreezer(Audio.currentMusicEvent, 1));
                        Add(new audioFreezer(Audio.currentAmbientEvent, 1));*/
            Celeste.Freeze(2);
            yield return null;
            Level.Session.SetFlag("notDust");
            Level.ResetZoom();
            camera.Y = Math.Min(Mirror.CenterY - 90, Level.Bounds.Bottom - 180);
            camera.X = Mirror.CenterX - 160;
            Player.Sprite.Rate = 1f;
            //todo: figure out how to freeze the audio bus
            Memory = new MirrorMemory(camera.Position, FollowingOrb, Player, Mirror);
            SwapMemoryPosition();
            Scene.Add(Memory);
            //load next level
            yield return new SwapImmediately(Textbox.Say("RedOrbCutscene", wait1, warpRoutine, fastWarpRoutine, orbEndRoutine));
            //the orb rises up and flies away.
            EndCutscene(Level);
        }

        public Color CenterColorMod(Color color)
        {
            if (Scene.OnInterval(smashflickerinterval))
            {
                color = Color.Lerp(color, Color.Orange, SmashColorModAmount);
            }
            color = Color.Lerp(color, Color.Black, ShadeAmount);
            return color;
        }
        public Color FillColorMod(Color color, int i)
        {
            if (Scene.OnInterval(smashflickerinterval, smashflickerinterval))
            {
                color = Color.Lerp(color, Color.Yellow, SmashColorModAmount);
            }
            color = Color.Lerp(color, Color.Black, ShadeAmount);
            return color;
        }
        public Color EdgeColorMod(Color color, int i)
        {
            if (Scene.OnInterval(smashflickerinterval))
            {
                color = Color.Lerp(color, Color.Cyan, SmashColorModAmount);
            }
            color = Color.Lerp(color, Color.Black, ShadeAmount);
            return color;
        }
        public void SetOrbPositions(Vector2 position)
        {
            FollowingOrb.Position = ReflectionOrb.Position = position;
        }
        private IEnumerator wait1()
        {
            yield return 1;
        }
        private int swapIndex = -1;
        private List<int> indices = [0, 1, 2, 3];
        public void SwapMemoryPosition()
        {
            int listindex = Calc.Random.Range(0, indices.Count);
            int index = indices[listindex];
            indices.RemoveAt(listindex);
            if (indices.Count == 0)
            {
                indices.AddRange([0, 1, 2, 3]);
            }
            Memory.characters.SwapMemoryPosition(index);
        }
        private Tween glitchIn;
        private Tween glitchOut;
        private bool warped;
        private float glitchFrom;
        private IEnumerator glitchWarp(float timeIn, float timeOut)
        {
            glitchIn.Stop();
            glitchOut.Stop();
            glitchIn.Duration = timeIn;
            glitchOut.Duration = timeOut;
            glitchIn.Start();
            warped = false;
            while (!warped)
            {
                yield return null;
            }
        }
        private IEnumerator warpRoutine() => glitchWarp(1, 0.6f);
        private IEnumerator fastWarpRoutine() => glitchWarp(0.5f, 0.2f);
        private IEnumerator orbEndRoutine()
        {
            yield return null;
        }
        public IEnumerator SmashRoutine(Vector2 smashCenter, float maxXPad, float maxYPad, float maxDuration, float maxDelay, Action<int> onImpact, Action<float> onIn = null, Action<float> onOut = null)
        {
            Vector2 target = smashCenter;
            int impacts = 0;
            float size = FollowingOrb.Radius;
            Level level = SceneAs<Level>();
            Camera camera = level.Camera;
            Rectangle r = new Rectangle((int)(camera.X - size * 2), (int)(camera.Y - size * 2), 320 + (int)(size * 4), 180 + (int)(size * 4));
            float duration = maxDuration;
            float delay = maxDelay;

            float intime = 0.4f;
            float outtimeOffset = -0.1f; //used to be 0.1f (TEST THIS)
            float delayTime = 0.6f;
            int loop = 0;
            bool startedFading = false;
            IEnumerator singleCharge(bool randomizeStartPosition, float inTime, float outTime)
            {
                if (randomizeStartPosition)
                {
                    if (FollowingOrb.Position.X > smashCenter.X)
                    {
                        target = smashCenter + PianoUtils.Random(-maxXPad, 0, -maxYPad, maxYPad);
                    }
                    else
                    {
                        target = smashCenter + PianoUtils.Random(0, maxXPad, -maxYPad, maxYPad);
                    }
                }
                Vector2 from = FollowingOrb.Position;
                Vector2 axis = Math.Abs(from.X - smashCenter.X) > Math.Abs(from.Y - smashCenter.Y) ? Vector2.UnitY : Vector2.UnitX;

                float fromAngle = (FollowingOrb.Position - target).Angle();
                int dir = Math.Sign(FollowingOrb.Position.X - target.X);
                float toAngle = Calc.ReflectAngle(fromAngle, axis);
                Vector2 to = target + Calc.AngleToVector(toAngle, 20);

                for (float i = 0; i < 1; i += Engine.DeltaTime / inTime)
                {
                    float lerp = Ease.CubeIn(i);
                    SetOrbPositions(Vector2.Lerp(from, target, lerp));
                    ReflectionOrb.Radius = FollowingOrb.Radius = size * (1f - lerp * 0.7f);
                    onIn?.Invoke(lerp);
                    yield return null;
                }
                SetOrbPositions(target);
                ReflectionOrb.Position = FollowingOrb.Position = target;
                ReflectionOrb.Radius = FollowingOrb.Radius = size;
                //collide with mirror
                onImpact?.Invoke(impacts);
                impacts++;
                for (float i = 0; i < 1; i += Engine.DeltaTime / outTime)
                {
                    float lerp = Ease.CubeOut(i);
                    ReflectionOrb.Radius = FollowingOrb.Radius = size * (0.7f + lerp * 0.3f);
                    SetOrbPositions(Vector2.Lerp(target, to, lerp));
                    onOut?.Invoke(lerp);
                    yield return null;
                }
            }
            yield return singleCharge(false, intime, intime + outtimeOffset);
            yield return delayTime;
            while (true)
            {
                yield return singleCharge(true, intime, intime + outtimeOffset);
                yield return delayTime;
                switch (loop)
                {
                    case < 3:
                        float l = Ease.CubeOut(loop / 3f);
                        intime = Calc.LerpClamp(0.4f, 0.2f, l);
                        delayTime = Calc.LerpClamp(0.7f, 0.05f, l * 1.5f);
                        break;
                    default:
                        outtimeOffset = 0;
                        delayTime = 0.05f;
                        intime = Calc.Approach(intime, 0.1f, Engine.DeltaTime);
                        break;

                }
                loop++;
            }
        }
        public void ShakeBoth(float time, float mult)
        {
            FollowingOrb.ShakeFor(time);
            ReflectionOrb.ShakeFor(time);
            FollowingOrb.ShakeMult = ReflectionOrb.ShakeMult = Vector2.One * mult;
        }
        public void StopShakingBoth()
        {
            FollowingOrb.StopShaking();
            ReflectionOrb.StopShaking();
        }

        private Vector2 IntersectionPointFromInside(Rectangle rect, Vector2 from, Vector2 to)
        {
            while (from != to)
            {
                Vector2 next = Calc.Approach(from, to, 1);
                if (!Collide.CheckRect(this, rect, next))
                {
                    break;
                }
                from = next;
            }
            return from;
        }
        public IEnumerator WhiteFadeRoutine(float duration)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                WhiteFade = i;
                yield return null;
            }
            WhiteFade = 1;
        }
        public IEnumerator ShakeRoutine(float minShakeTime, float maxShakeTime, float minWait, float maxWait, float minMult, float maxMult, int loops, int progressLoop)
        {
            Random random = new Random(1);
            ReflectionOrb.Shaker.CustomRandom = FollowingOrb.Shaker.CustomRandom = random;
            ReflectionOrb.StopShaking();
            FollowingOrb.StopShaking();
            ReflectionOrb.ShakeFor(-1);
            FollowingOrb.ShakeFor(-1);

            float shakeTime = maxShakeTime;
            float waitTime = maxWait;
            float shakeMult = minMult;

            for (int i = 0; i < progressLoop; i++)
            {
                ReflectionOrb.ShakeFor(shakeTime);
                FollowingOrb.ShakeFor(shakeTime);
                ReflectionOrb.ShakeMult = FollowingOrb.ShakeMult = Vector2.One * shakeMult;
                yield return shakeTime;
                ReflectionOrb.StopShaking();
                FollowingOrb.StopShaking();
                yield return waitTime;
            }
            yield return waitTime;
            for (int i = 0; i < loops - progressLoop; i++)
            {
                float lerp = i / (float)(loops - progressLoop);
                ReflectionOrb.ShakeFor(shakeTime);
                FollowingOrb.ShakeFor(shakeTime);
                ReflectionOrb.ShakeMult = FollowingOrb.ShakeMult = Vector2.One * shakeMult;
                yield return shakeTime;
                ReflectionOrb.StopShaking();
                FollowingOrb.StopShaking();
                if (waitTime > 0) yield return waitTime;
                shakeMult = Calc.LerpClamp(minMult, maxMult, lerp);
                waitTime = Calc.LerpClamp(maxWait, minWait, lerp);

            }
        }
        public IEnumerator FlickerRoutine(float duration)
        {
            FollowingOrb.AutoFade = false;
            Flickering = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                flickerProgress = i;
                yield return null;
            }
            Flickering = false;
        }
        public IEnumerator ColorMod()
        {
            ReflectionOrb.Orb.FlashColorMod(Color.White, 0.025f, 0f, 0);
            yield return 0.05f;
            Color fillInvert = ReflectionOrb.Color.Invert();
            Color centerInvert = ReflectionOrb.CenterColor.Invert();
            Color edgeInvert = ReflectionOrb.EdgeColor.Invert();
            ReflectionOrb.Orb.FlashColorMod(centerInvert, fillInvert, edgeInvert, 0.05f, 0.05f, 2);
            yield return 0.2f;
            ReflectionOrb.Orb.ColorB = fillInvert;
            ReflectionOrb.Orb.CenterColorB = centerInvert;
            ReflectionOrb.Orb.EdgeColorB = edgeInvert;
        }
        public IEnumerator CenterCamera()
        {
            Camera camera = Level.Camera;
            Vector2 target = Mirror.Center - new Vector2(160f, 90f);
            while ((camera.Position - target).Length() > 1f)
            {
                camera.Position += (target - camera.Position) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                yield return null;
            }
        }
    }
}
