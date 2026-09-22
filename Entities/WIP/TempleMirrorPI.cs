//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue.CenterTriangle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/TempleMirrorPI")]
    [Tracked]
    public class TempleMirrorPI : Entity
    {
        private static List<ImpactPoint> impactPoints = [];
        private static List<ImpactPoint> bgCrackPoints = [];
        private static List<ImpactPoint> impactCrackPoints = [];
        public static HashSet<ShardData> GlassShards = [];
        public class ImpactPoint
        {
            public MTexture Texture;
            public MTexture MirrorTexture;
            public Vector2 Position;
            public Vector2 MirrorOffset;
            public float Alpha = 1;
            public float ShockAlpha;
            public float Rotation;
            public void Update()
            {
                if (ShockAlpha > 0)
                {
                    ShockAlpha = Calc.Approach(ShockAlpha, 0, Engine.DeltaTime / 5f);
                }
            }
        }
        public struct ShardData
        {
            public int Seed;
            public Vector2 EndPosition;
            public float EndRotation;
            public char? AnimID;
            public bool Simulated;
        }
        [Tracked]
        public class GlassShard : Actor
        {
            public Random Random;
            public ShardData EndData = default;
            public float MaxSpeed;
            private Vector2 blastFrom;
            private Vector2 smashDir;
            public Vector2 Speed;
            private float RotationRate;
            private float Rotation
            {
                get => Base.Rotation;
                set
                {
                    Base.Rotation = value;
                    Flash.Rotation = value;
                }
            }
            private float blastSpeed;
            public bool SlideStop;
            public Sprite Flash;
            public Image Base;
            private float flashDelay;
            public bool AddToCache = true;
            public float Shade
            {
                get => blackLerp;
                set
                {
                    blackLerp = value;
                    Base.Color = Color.Lerp(Color.White, Color.Black, 0.6f * value);
                    Flash.Color = Base.Color * flashAlpha;
                }
            }
            private float blackLerp;
            private bool atRest;
            private float flashAlpha = 0.5f;
            public GlassShard(Vector2 position, Vector2 blastFrom, Vector2 smashDir, float blastSpeed) : base(position)
            {
                this.blastFrom = blastFrom;
                this.smashDir = smashDir;
                this.blastSpeed = blastSpeed;
                Tag |= Tags.TransitionUpdate;
            }
            public GlassShard(ShardData data) : base(data.EndPosition)
            {
                EndData = data;
                Tag |= Tags.TransitionUpdate;
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                if (AddToCache && atRest)
                {
                    EndData.EndPosition = Position;
                    EndData.EndRotation = Rotation;
                    EndData.Simulated = true;
                    GlassShards.Add(EndData);
                }
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Add(new MirrorReflection());
                flashDelay = Calc.Random.Range(1, 4f);
                string path = "objects/PuzzleIslandHelper/mirrorCracks/";
                char animID = Calc.Random.Choose('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H');
                if (!EndData.Simulated || !EndData.AnimID.HasValue)
                {
                    EndData.AnimID = animID;
                }
                else
                {
                    animID = EndData.AnimID.Value;
                }
                string anim = "shardAnim" + animID;

                Base = new Image(GFX.Game[path + anim + "00"]);
                Flash = new Sprite(GFX.Game, path);
                Flash.Stop();
                Flash.Add("flash", anim, 0.05f, 1, 2, 3);
                Flash.Color = Color.White * flashAlpha;
                Flash.OnFinish = (s) => Flash.Texture = null;
                Add(Base);
                Add(Flash);
                Base.CenterOrigin();
                Flash.CenterOrigin();
                int width = Math.Max(3, (int)Base.Width - 2);
                int height = Math.Max(3, (int)Base.Height - 2);
                Collider = new Hitbox(width, height, -width / 2, -height / 2);
                if (EndData.Simulated)
                {
                    Rotation = EndData.EndRotation;
                    Position = EndData.EndPosition;
                    RotationRate = 0;
                    Speed = Vector2.Zero;
                    Random = new Random(EndData.Seed);
                    atRest = true;
                    Shade = 1;
                }
                else
                {
                    int seed = Calc.Random.Next();
                    Random = new Random(seed);
                    EndData.Seed = seed;
                    Rotation = Random.NextAngle();
                    RotationRate = blastSpeed * 0.1f * Random.Range(0.5f, 1);
                    Vector2 blastDir = Vector2.Normalize(Position - blastFrom);
                    Speed = Calc.AngleToVector((blastDir.Angle() + smashDir.Angle()) / 2, blastSpeed);
                }
            }
            public override void Update()
            {
                base.Update();
                if (flashDelay > 0)
                {
                    flashDelay -= Engine.DeltaTime;
                    if (flashDelay <= 0)
                    {
                        Flash.Visible = true;
                        Flash.Play("flash");
                        flashDelay = Calc.Random.Range(2f, 4f);
                    }
                }
                if (!atRest && !Advance())
                {
                    AddToCache = false;
                    RemoveSelf();
                }
                if (atRest && Shade < 1)
                {
                    Shade = Calc.Approach(Shade, 1, Engine.DeltaTime);
                }
            }
            public bool Advance()
            {
                if (Top > SceneAs<Level>().Bounds.Bottom)
                {
                    return false;
                }
                MaxSpeed = Math.Max(MaxSpeed, new Vector2(Math.Abs(Speed.X), Math.Abs(Speed.Y)).Length());
                Speed.Y = Calc.Approach(Speed.Y, 160f, Player.Gravity * Engine.DeltaTime);
                Rotation += RotationRate * Engine.DeltaTime;
                Flash.Rotation = Rotation;
                if (SlideStop)
                {
                    Speed.X = PianoUtils.RubberbandApproach(Speed.X, 0, 1);
                    RotationRate = PianoUtils.RubberbandApproach(RotationRate, 0, 0.04f);
                }
                else
                {
                    Speed.X = Calc.Approach(Speed.X, 0, 100f * Engine.DeltaTime);
                    RotationRate = Calc.Approach(RotationRate, 0, Engine.DeltaTime / 1.2f);
                }
                if (Speed.X != 0) MoveH(Speed.X * Engine.DeltaTime);
                if (Speed.Y != 0) MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                if (Speed.X == 0 && Speed.Y == 0 && RotationRate == 0)
                {
                    atRest = true;
                }
                return true;
            }
            public override void Render()
            {
                base.Render();
            }
            public void OnCollideV(CollisionData data)
            {
                //direction entity was moving in before the collision
                int dir = (int)data.Direction.Y;
                //move out of any solids
                while (dir != 0 && CollideCheck<Solid>())
                {
                    Position.Y += -dir;
                }
                if (dir == 1)
                {
                    if (Speed.Y > 5f)
                    {
                        Speed.Y *= -0.7f;
                        float xSpeedMult = Random.Range(-0.9f, 0.9f);
                        int multDir = Math.Sign(xSpeedMult);
                        RotationRate *= -multDir * 0.9f;
                        Speed.X *= xSpeedMult;
                        //bounce a little
                    }
                    else
                    {
                        Speed.Y = 0;
                        SlideStop = true;
                        //prevent micro bounces as they look stinky bad stinky stinky :(
                    }
                }
                else
                {
                    Speed.Y = 0;
                    //remove any upwards speed if hitting a ceiling
                }

            }
        }
        public class Layer : Entity
        {
            public float Alpha = 1;
            public bool Flashes;
            private VirtualRenderTarget target;
            private List<ImpactPoint> points;
            public bool Additive;
            public Layer(Vector2 position, int width, int height, int depth, bool reflect, List<ImpactPoint> points) : base(position)
            {
                this.points = points;
                target = VirtualContent.CreateRenderTarget("base layer", width, height);
                Depth = depth;
                if (reflect)
                {
                    MirrorSurface surface = null;
                    surface = new MirrorSurface(() =>
                    {
                        foreach (var p in points)
                        {
                            if (p.MirrorTexture != null)
                            {
                                surface.ReflectionOffset = p.MirrorOffset;
                                p.MirrorTexture.DrawCentered(Position + p.Position, surface.ReflectionColor, 1, p.Rotation);
                            }
                        }
                    });
                    Add(surface);
                }
                Add(new BeforeRenderHook(() =>
                {
                    target.SetAsTarget(true);
                    Draw.SpriteBatch.StandardBegin(Additive ? BlendState.Additive : BlendState.AlphaBlend);
                    foreach (ImpactPoint point in points)
                    {
                        point.Texture?.DrawCentered(point.Position, Color.White * point.Alpha, 1, point.Rotation);
                    }
                    Draw.SpriteBatch.End();
                    if (Flashes)
                    {
                        Draw.SpriteBatch.Begin();
                        foreach (ImpactPoint point in points)
                        {
                            if (point.ShockAlpha > 0 && point.MirrorTexture != null)
                            {
                                point.MirrorTexture?.DrawCentered(point.Position, Color.White * point.ShockAlpha, 1, point.Rotation);
                            }
                        }
                        Draw.SpriteBatch.End();
                    }
                }));
            }
            public override void Update()
            {
                base.Update();
                foreach (var p in points)
                {
                    p.Update();
                }
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(target, Position, Color.White * Alpha);

            }
            public void AddPoint(Vector2 position, string path, string mirrorPath, float rotation)
            {
                var textures = GFX.Game.GetAtlasSubtextures(path);

                if (textures.Count > 0)
                {
                    int index = Calc.Random.Range(0, textures.Count);
                    MTexture mirrorTexture = null;
                    if (!string.IsNullOrEmpty(mirrorPath))
                    {
                        var mirrorTextures = GFX.Game.GetAtlasSubtextures(mirrorPath);
                        if (mirrorTextures.Count > index)
                        {
                            mirrorTexture = mirrorTextures[index];
                        }
                    }
                    points.Add(new ImpactPoint()
                    {
                        Texture = textures[index],
                        MirrorTexture = mirrorTexture,
                        Position = position,
                        MirrorOffset = new Vector2(10f, 4f) + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4)),
                        Alpha = 1,//randomizeAlpha ? Calc.Random.Range(0.4f, 0.8f) : 1,
                        ShockAlpha = mirrorTexture != null ? 1 : 0,
                        Rotation = rotation
                    });
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                target?.Dispose();
                // mirror?.Dispose();
            }
        }
        public Solid Platform;
        public Layer ImpactLayer;
        public Layer BgCrackLayer;
        public Layer ImpactCrackLayer;
        public Entity Frame;
        public float Rotation = 0;//15f.ToRad();
        public class Bg : Entity
        {
            public MirrorSurface surface;
            public Vector2[] offsets;
            public List<MTexture> textures;
            private TempleMirrorPI parent;
            public Bg(TempleMirrorPI mirror, Vector2 position)
                : base(position)
            {
                parent = mirror;
                Depth = 9500;
                textures = GFX.Game.GetAtlasSubtextures("objects/temple/portal/reflection");
                Vector2 vector = new Vector2(10f, 4f);
                offsets = new Vector2[textures.Count];
                for (int i = 0; i < offsets.Length; i++)
                {
                    offsets[i] = vector + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4));
                }

                Add(surface = new MirrorSurface());
                surface.OnRender = delegate
                {
                    for (int j = 0; j < textures.Count; j++)
                    {
                        surface.ReflectionOffset = offsets[j];
                        textures[j].DrawCentered(Position, surface.ReflectionColor, 1, parent.Rotation);
                    }
                };

            }
            public override void Render()
            {
                GFX.Game["objects/temple/portal/surface"].DrawCentered(Position, Color.White, 1, parent.Rotation);
            }
        }
        /*        public class Inside : Entity
                {
                    public MirrorReflection reflection;
                    public const string Path = "objects/PuzzleIslandHelper/templeMirror/symbol";
                    public List<MTexture> textures = new()
                    {
                        GFX.Game[Path + "A"],
                        GFX.Game[Path + "B"],
                        GFX.Game[Path + "C"],
                        GFX.Game[Path + "D"],
                        GFX.Game[Path + "E"],
                        GFX.Game[Path + "F"]
                    };
                    public class Ring : Image
                    {
                        public Vector2 Center;
                        public float Radius;
                        public Ring(char type, Vector2 center, float radius) : base(GFX.Game[Path + type])
                        {
                            Center = center;
                            Radius = radius;
                            CenterOrigin();
                            Position = new Vector2(Width / 2, Height / 2);
                        }
                        public override void Render()
                        {
                            if (Texture != null)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    Texture.Draw(RenderPosition.RotateAroundDeg(Center, 360 / 8 * i), Origin, Color, Scale, Rotation, Effects);
                                }

                            }
                        }
                    }
                    public Inside(Vector2 position) : base(position)
                    {
                        Depth = 9499;

                        Add(reflection = new MirrorReflection()
                        {
                            IgnoreEntityVisible = true
                        });
                    }
                    public override void Render()
                    {
                        base.Render();

                    }
                }*/
        //public Inside inside;
        public TemplePortalTorch leftTorch;
        public TemplePortalTorch rightTorch;
        public VirtualRenderTarget buffer;
        public float bufferAlpha;
        public float bufferTimer;
        public BetterShaker Shaker;
        public Bg Background;
        public TempleMirrorPI(Vector2 position)
            : base(position)
        {
            Depth = 2010;
            Collider = new Hitbox(120f, 64f, -60f, -32f);
            Add(Shaker = new BetterShaker(OnShake));
        }
        public void OnShake(Vector2 amount)
        {
            Position += amount;
            BgCrackLayer.Position += amount;
            ImpactCrackLayer.Position += amount;
            ImpactLayer.Position += amount;
            Frame.Position += amount;
            Background.Position += amount;
        }
        public TempleMirrorPI(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }
        public void SmashAt(Vector2 position, Vector2 speed)
        {
            Vector2 origin = Collider.AbsolutePosition;
            float rotation = MathHelper.PiOver4 * Calc.Random.Range(0, 4);
            BgCrackLayer.AddPoint(position - origin, crackPath + "base", crackPath + "baseMirror", rotation);
            ImpactCrackLayer.AddPoint(position - origin, crackPath + "insideCracks", crackPath + "insideCracksMirror", rotation);
            ImpactLayer.AddPoint(position - origin, crackPath + "impacts", null, rotation);
            Shaker.ShakeFor(0.4f);

            int shards = Calc.Random.Range(3, 7);
            float angleArea = MathHelper.TwoPi / shards;
            float baseAngle = Calc.Random.NextAngle();
            for (int i = 0; i < shards; i++)
            {
                float angle = angleArea * i + baseAngle + Calc.Random.Range(-angleArea * 0.7f / 2, angleArea * 0.7f / 2);
                Scene.Add(new GlassShard(position + Calc.AngleToVector(angle, 2), position, Vector2.Normalize(speed), Math.Min(speed.Length(), 160f)));
            }

        }
        private string crackPath = "objects/PuzzleIslandHelper/mirrorCracks/";
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Platform = new Solid(BottomLeft, Width, 8, true);
            scene.Add(Platform);
            Platform.Collidable = false;
            scene.Add(Background = new Bg(this, Position));
            scene.Add(leftTorch = new TemplePortalTorch(Position + new Vector2(-90f, 0f)));
            scene.Add(rightTorch = new TemplePortalTorch(Position + new Vector2(90f, 0f)));

            if (!(scene as Level).Session.GetFlag("MirrorSmashed"))
            {
                bgCrackPoints.Clear();
                impactCrackPoints.Clear();
                impactPoints.Clear();
                GlassShards.Clear();
            }
            foreach (var data in GlassShards)
            {
                scene.Add(new GlassShard(data));
            }
            scene.Add(BgCrackLayer = new Layer(Collider.AbsolutePosition, (int)Width, (int)Height, Depth - 1, true, bgCrackPoints));
            scene.Add(ImpactCrackLayer = new Layer(Collider.AbsolutePosition, (int)Width, (int)Height, Depth - 2, true, impactCrackPoints));
            scene.Add(ImpactLayer = new Layer(Collider.AbsolutePosition, (int)Width, (int)Height, Depth - 3, false, impactPoints));
            BgCrackLayer.Alpha = 0.8f;
            ImpactCrackLayer.Additive = true;
            BgCrackLayer.Additive = true;
            ImpactCrackLayer.Flashes = true;
            scene.Add(Frame = new Entity(Position));
            Frame.Depth = Depth - 4;
            Image frame = new Image(GFX.Game["objects/temple/portal/portalframe"]);
            Frame.Add(frame);
            frame.CenterOrigin();

            leftTorch.Add(leftTorch.loopSfx = new SoundSource());
            rightTorch.Add(rightTorch.loopSfx = new SoundSource());
            leftTorch.sprite.Play("lit");
            rightTorch.sprite.Play("lit");
            leftTorch.Add(leftTorch.bloom = new BloomPoint(1f, 16f));
            leftTorch.Add(leftTorch.light = new VertexLight(Color.LightSeaGreen, 0f, 32, 128));
            rightTorch.Add(rightTorch.bloom = new BloomPoint(1f, 16f));
            rightTorch.Add(rightTorch.light = new VertexLight(Color.LightSeaGreen, 0f, 32, 128));
            leftTorch.loopSfx.Play("event:/game/05_mirror_temple/mainmirror_torch_loop");
            rightTorch.loopSfx.Play("event:/game/05_mirror_temple/mainmirror_torch_loop");
        }

        public override void Render()
        {
            base.Render();
            if (buffer != null)
            {
                Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Position + new Vector2((0f - Collider.Width) / 2f, (0f - Collider.Height) / 2f), Color.White * bufferAlpha);
            }
            GFX.Game["objects/PuzzleIslandHelper/templeMirror/goop00"].DrawCentered(Position, Color.White, 1, Rotation);
        }

        public override void Removed(Scene scene)
        {
            buffer?.Dispose();
            BgCrackLayer.RemoveSelf();
            ImpactCrackLayer.RemoveSelf();
            ImpactLayer.RemoveSelf();
            rightTorch.RemoveSelf();
            leftTorch.RemoveSelf();
            Frame.RemoveSelf();
            Background.RemoveSelf();
            Platform.RemoveSelf();
            base.Removed(scene);
        }
    }
}
