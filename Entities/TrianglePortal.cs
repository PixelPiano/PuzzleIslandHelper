using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TrianglePortal")]
    [Tracked]
    public class TrianglePortal : Entity
    {
        //for the love of god do not use this entity in your map
        #region BasicVariables
        private bool First;
        private float rectColorRate = 0;
        private float innerFlashRate = 0;
        private float rate = 0.002f;
        private float minColor = 0f;
        private float maxColor = 0.8f;
        private float[] randomColors = new float[9];
        private ParticleSystem system;
        private readonly bool linearDesign = true;
        private bool usesFlags = false;
        private string[] lightFlags = new string[3];
        private bool[] lightBools = new bool[3];
        public bool portalState = false;
        private string flag;

        private Color firstColor;
        private Color secondColor;
        private Color thirdColor;
        private Color[] colors = new Color[15];

        private Vector2 lightRenderA;
        private Vector2 lightRenderB;
        private Vector2 lightRenderC;

        private Sprite triangle;
        private Sprite[] innerTriangle = new Sprite[15];
        private Sprite[] rects = new Sprite[15];
        private Sprite[] nodes = new Sprite[3];
        private Sprite[] lights = new Sprite[3];
        //no i will not change this

        private Entity nodeEntity;
        private Entity nodeLight;

        private Level l;
        private Player player;



        private static VirtualRenderTarget _PortalMask;
        private static VirtualRenderTarget _PortalObject;
        private static VirtualRenderTarget _ParticleObject;
        private static VirtualRenderTarget _Debug;
        public static VirtualRenderTarget Debug => _Debug ??= VirtualContent.CreateRenderTarget("Debug", 320, 180);

        public static VirtualRenderTarget PortalMask => _PortalMask ??= VirtualContent.CreateRenderTarget("PortalMask", 320, 180);
        public static VirtualRenderTarget PortalObject => _PortalObject ??= VirtualContent.CreateRenderTarget("PortalObject", 320, 180);
        public static VirtualRenderTarget ParticleObject => _ParticleObject ??= VirtualContent.CreateRenderTarget("PortalObject", 320, 180);
        #endregion

        #region EventVariables
        private bool inEvent = false;
        private bool scaleStart = false;
        private bool inRotateEvent = false;
        private bool EventComplete = false;
        private float particleDistance = 8;
        private float[] freezeTimes = new float[] { 1, 1, 0.8f, 0.7f, 0.5f, 0.38f, 0.2f, 0.1f, 0.15f, 0.2f, 2 };
        private List<Entity> blockList;
        private ParticleType PlayerPoof = new ParticleType
        {
            Size = 2f,
            Color = Calc.HexToColor("00ff00"),
            Color2 = Calc.HexToColor("0000ff"),
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -MathHelper.Pi / 2f,
            DirectionRange = MathHelper.PiOver2,
            LifeMin = 0.8f,
            LifeMax = 2f,
            SpeedMin = 20f,
            SpeedMax = 60f,
            SpeedMultiplier = 0.25f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 5f
        };
        private ParticleType TriangleEdge = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("00ffe5") * 0.2f,
            Color2 = Calc.HexToColor("00ff99") * 0.2f,
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -MathHelper.Pi / 2f,
            DirectionRange = MathHelper.PiOver2,
            LifeMin = 0.06f,
            LifeMax = 0.6f,
            SpeedMin = 20f,
            SpeedMax = 60f,
            SpeedMultiplier = 0.5f,
            FadeMode = ParticleType.FadeModes.Linear,
            Friction = 5f
        };
        private ParticleType AroundPlayer = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("00ff00") * 0.2f,
            Color2 = Calc.HexToColor("55ff66") * 0.2f,
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -MathHelper.Pi / 2f,
            DirectionRange = MathHelper.PiOver2,
            LifeMin = 0.06f,
            LifeMax = 0.6f,
            SpeedMin = 20f,
            SpeedMax = 60f,
            SpeedMultiplier = 0.25f,
            FadeMode = ParticleType.FadeModes.Linear,
            Friction = 5f
        };
        private float angle1;
        private float angle2;
        private float angle3;
        #endregion
        public TrianglePortal(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            First = data.Bool("first");
            for (int i = 0; i < 3; i++)
            {
                lightFlags[i] = data.Attr($"light{i + 1}flag");
            }
            Depth = 1;
            flag = data.Attr("flag");
            usesFlags = data.Bool("usesFlags", false);
            portalState = !usesFlags;
            Collider = new Hitbox(data.Width, data.Height);
            Add(new BeforeRenderHook(BeforeRender));
        }
        private void DrawInside()
        {
            Draw.Rect((int)Position.X, (int)Position.Y, (int)Width + 10, (int)Height + 10, Color.Lerp(Color.Black, Color.Green, rectColorRate));
            for (int i = 0; i < innerTriangle.Length; i++)
            {
                //Vector2 _scale = innerTriangle[i].Scale;
                innerTriangle[i].SetColor(Color.Lerp(colors[i], Color.White, innerFlashRate));
                if (!linearDesign)
                {
                    if (i % 7 == 0)
                    {
                        rects[i].DrawSimpleOutline();
                    }

                    else
                    {
                        rects[i].Render();
                    }
                    innerTriangle[i].Render();
                }
                else
                {
                    innerTriangle[i].Render();
                    rects[i].Render();
                }
            }
            AppearParticles();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.SpriteBatch.Draw(Debug, camera.Position, Color.White);
        }
        private void BeforeRender()
        {
            if (Scene is not Level || SceneAs<Level>().Session.GetFlag(flag))
            {
                return;
            }
            l = Scene as Level;
            EasyRendering.SetRenderMask(PortalMask, triangle, l);
            EasyRendering.DrawToObject(Debug, triangle, l);
            EasyRendering.DrawToObject(Debug, DrawInside, l);
            EasyRendering.MaskToObject(PortalObject, PortalMask, DrawInside);

        }
        public override void Render()
        {
            base.Render();
            player = Scene.Tracker.GetEntity<Player>();
            if (player == null || Scene as Level == null || SceneAs<Level>().Session.GetFlag(flag))
            {
                return;
            }
            l = Scene as Level;
            if (portalState)
            {
                Draw.SpriteBatch.Draw(ParticleObject, l.Camera.Position, Color.White);
                Draw.SpriteBatch.Draw(PortalObject, l.Camera.Position, Color.White);
                float firstThick = 4;
                float secondThick = 1.5f;
                angle1 = Calc.Angle(lightRenderA, lightRenderB);
                angle2 = Calc.Angle(lightRenderB, lightRenderC);
                angle3 = Calc.Angle(lightRenderC, lightRenderA);
                Draw.Line(lightRenderA, lightRenderB, firstColor * randomColors[0], firstThick);
                Draw.Line(lightRenderB, lightRenderC, firstColor * randomColors[1], firstThick);
                Draw.Line(lightRenderC, lightRenderA, firstColor * randomColors[2], firstThick);

                Draw.Line(lightRenderA, lightRenderB, secondColor * randomColors[3], secondThick);
                Draw.Line(lightRenderB, lightRenderC, secondColor * randomColors[4], secondThick);
                Draw.Line(lightRenderC, lightRenderA, secondColor * randomColors[5], secondThick);

                Draw.Line(lightRenderA, lightRenderB, thirdColor * randomColors[6], 1);
                Draw.Line(lightRenderB, lightRenderC, thirdColor * randomColors[7], 1);
                Draw.Line(lightRenderC, lightRenderA, thirdColor * randomColors[8], 1);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            #region Sprites
            Add(triangle = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/portal/"));
            SceneAs<Level>().Session.SetFlag("startWaiting",false);
            scene.Add(nodeEntity = new Entity(Position));
            scene.Add(nodeLight = new Entity(Position));
            for (int i = 0; i < nodes.Length; i++)
            {
                nodeEntity.Add(nodes[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/portal/"));
                nodeLight.Add(lights[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/portal/"));
                nodes[i].AddLoop("idle", "portalNodeA", 0.1f);
                lights[i].AddLoop("idle", "light", 0.05f);
                nodes[i].Position += new Vector2(0, 1);
            }
            nodes[0].Position += new Vector2(-nodes[0].Width / 2, Height - nodes[0].Height / 2);
            nodes[1].Position += new Vector2(Width - nodes[1].Width / 2, Height - nodes[1].Height / 2);
            nodes[2].FlipY = true;
            nodes[2].Position += new Vector2(Width / 2 - nodes[2].Width / 2, -nodes[2].Height / 2 - 1);
            for (int i = 0; i < 3; i++)
            {
                lightBools[i] = SceneAs<Level>().Session.GetFlag(lightFlags[i]);
                if (!lightBools[i])
                {
                    lights[i].Visible = false;
                }
                lights[i].Position = nodes[i].Position + new Vector2(1, 0);
                nodes[i].Play("idle");
                lights[i].Play("idle");
            }
            nodes[0].Position.Y += nodes[0].Height / 2 + 2;
            nodes[1].Position.Y = nodes[0].Position.Y;
            nodes[2].Position.Y -= nodes[2].Height / 2 + 2;
            nodeEntity.Depth = 2;
            nodeLight.Depth = nodeEntity.Depth - 4;
            triangle.AddLoop("idle", "triangle", 1f);

            triangle.Origin = new Vector2(triangle.Width, triangle.Height);
            triangle.Scale = new Vector2(Width / triangle.Width, Height / triangle.Height);
            triangle.Position += new Vector2(Width, Height);
            for (int i = 0; i < innerTriangle.Length; i++)
            {
                Add(innerTriangle[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/portal/"));
                innerTriangle[i].AddLoop("idle", "triangle", 1f);
                innerTriangle[i].CenterOrigin();
                innerTriangle[i].Position = triangle.Position - new Vector2((triangle.Width / 2) * triangle.Scale.X, (triangle.Height / 2) * triangle.Scale.Y - 8);
                innerTriangle[i].Scale = triangle.Scale * (1 - (float)i / innerTriangle.Length);
                innerTriangle[i].SetColor(Color.Lerp(Color.Black, Color.LightGreen, (float)i / innerTriangle.Length));
                colors[i] = innerTriangle[i].Color;
                innerTriangle[i].Visible = false;
                innerTriangle[i].Play("idle");
                Add(rects[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/portal/"));
                rects[i].AddLoop("idle", "rectangle", 1f);
                rects[i].CenterOrigin();
                rects[i].Position = triangle.Position - new Vector2((triangle.Width / 2) * triangle.Scale.X, (triangle.Height / 2) * triangle.Scale.Y - 8);
                rects[i].Scale *= (1.2f - (float)i * 1.2f / rects.Length);
                rects[i].SetColor(Color.Lerp(Color.Black, Color.LightGreen * 0.22f, (float)i / rects.Length));
                rects[i].Visible = false;
                rects[i].Play("idle");
            } //inner shapes

            triangle.SetColor(Color.Green);
            triangle.Visible = false;
            triangle.Play("idle");
            #endregion
            lightRenderA = lights[0].Position + Position + new Vector2(lights[0].Width / 2, lights[0].Height / 2);
            lightRenderB = lights[1].Position + Position + new Vector2(lights[1].Width / 2, lights[1].Height / 2);
            lightRenderC = lights[2].Position + Position + new Vector2(lights[2].Width / 2, lights[2].Height / 2);
            triangle.AddLoop("burnt", "burnMarks", 0.1f);
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                triangle.Play("burnt");
                triangle.Visible = true;
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i].SetColor(Color.Gray);
                    lights[i].RemoveSelf();
                }
            }
            else
            {
                Add(new Coroutine(fadeLight(), false));
                Add(new Coroutine(RotationLerp(), false));
            }
            //for when you need slightly similar logic in similar things but loops won't work 
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            SceneAs<Level>().Session.SetFlag("TimerEvent", false);
            SceneAs<Level>().Session.SetFlag("GlitchCutsceneEnd", false);
            blockList = scene.Tracker.GetEntities<CustomFlagExitBlock>();
            foreach (CustomFlagExitBlock block in blockList)
            {
                block.forceChange = false;
            }
            scene.Add(system = new ParticleSystem(Depth + 1, 1000));
        }
        public override void Update()
        {
            base.Update();
            player = Scene.Tracker.GetEntity<Player>();
            if (player == null || SceneAs<Level>().Session.GetFlag(flag))
            {
                return;
            }
            
            firstColor = Color.Lerp(Color.SpringGreen, Color.White, Calc.Random.Range(minColor, maxColor));
            secondColor = Color.Lerp(Color.Turquoise, Color.White, Calc.Random.Range(minColor, maxColor));
            thirdColor = Color.Lerp(Color.Green, Color.White, Calc.Random.Range(minColor, maxColor));
            for (int i = 0; i < randomColors.Length; i++)
            {
                randomColors[i] = Calc.Random.Range(0.7f, 1);
            }
            if (EventComplete)
            {
                player.MoveToX(Center.X);
                player.MoveToY(Center.Y + 16);
            }
            if (CollideCheck(player) && !inEvent && !EventComplete && portalState)
            {
                SceneAs<Level>().Session.SetFlag("TimerEvent");
                if (First)
                {
                    if (!SceneAs<Level>().Session.GetFlag("StartingPortalEnd"))
                    {
                        Add(new Coroutine(StartingEvent(), true));
                    }
                }
                else
                {
                    Add(new Coroutine(EndingEvent(), true));
                }
            }
            for (int i = 1; i < innerTriangle.Length + 1; i++)
            {
                innerTriangle[i - 1].Rotation += 0.01f - i * rate;
                rects[i - 1].Rotation += 0.01f;
            }
            for (int i = 0; i < 3; i++)
            {
                if (usesFlags)
                {
                    lightBools[i] = SceneAs<Level>().Session.GetFlag(lightFlags[i]);
                    if (lightBools[i])
                    {
                        lights[i].Visible = true;
                    }

                    if (lightBools[0] && lightBools[1] && lightBools[2])
                    {
                        portalState = true;
                    }
                }
                else
                {
                    lights[i].Visible = true;
                }
            }

        }
        #region Particles
        private void AppearParticles()
        {
            if (!portalState)
            {
                return;
            }
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
            particlesBG.Depth = -10500;
            player = Scene.Tracker.GetEntity<Player>();
            particlesBG.Visible = false;
            for (int i = 0; i < 120; i += 30)
            {
                particlesBG.Visible = false;
                particlesBG.Emit(AroundPlayer, 4, new Vector2(Center.X, Position.Y + Height - 8), new Vector2(Width, particleDistance), angle1 + 90f.ToDeg());
                system.Emit(TriangleEdge, 2, new Vector2(Center.X, Position.Y + Height), new Vector2(Width / 2, Height / 8f), Color.Lerp(TriangleEdge.Color, Color.White, innerFlashRate), angle1 - 90f.ToDeg());
                particlesBG.Render();
            }
            int _temp = (int)Width / 6;
            Vector2 distance = new Vector2(MathHelper.Distance(lights[1].Position.X, lights[2].Position.X), MathHelper.Distance(lights[1].Position.Y, lights[2].Position.Y));
            for (float j = 0; j < 1; j += 0.06f)
            {
                float _X = Calc.Approach(lights[1].Position.X, lights[2].Position.X, distance.X * j);
                float _Y = Calc.Approach(lights[1].Position.Y, lights[2].Position.Y, distance.Y * j);
                particlesBG.Emit(AroundPlayer, 1, Position + new Vector2(_X, _Y), new Vector2(particleDistance, 2), angle2 + 90f.ToDeg());
                system.Emit(TriangleEdge, 1, Position + new Vector2(_X, _Y), new Vector2(10, 2), Color.Lerp(TriangleEdge.Color, Color.White, innerFlashRate), angle2 - 90f.ToDeg());
                _X = Calc.Approach(lights[0].Position.X, lights[2].Position.X, distance.X * j);
                _Y = Calc.Approach(lights[0].Position.Y, lights[2].Position.Y, distance.Y * j);
                particlesBG.Emit(AroundPlayer, 1, Position + new Vector2(_X + 8, _Y), new Vector2(particleDistance, 2), angle3 + 90f.ToDeg());
                system.Emit(TriangleEdge, 1, Position + new Vector2(_X + 8, _Y), new Vector2(10, 2), Color.Lerp(TriangleEdge.Color, Color.White, innerFlashRate), angle3 - 90f.ToDeg());
            }

        }
        //all ive breathed for the last 5 hours is particle system
        private void PoofParticles()
        {
            //poof :o
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
            particlesBG.Depth = -10500;
            player = Scene.Tracker.GetEntity<Player>();
            for (int i = 0; i < 120; i += 30)
            {
                particlesBG.Emit(PlayerPoof, 4, player.Center, new Vector2(player.Width / 2, player.Height));
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator GlitchCutscene()
        {
            //audio glitching here is unintentional but makes the event 100% better because of it
            int index = 0;
            foreach (CustomFlagExitBlock block in blockList)
            {
                Celeste.Freeze(Calc.Random.Range(0.3f, 0.7f));
                block.forceChange = true;
                block.forceState = true;
                yield return index != -1 ? freezeTimes[index] : Calc.Random.Range(0.01f, 0.1f);
                index = index != -1 ? index != freezeTimes.Length - 1 ? index + 1 : -1 : -1;
            }

            float _amount = Glitch.Value;
            Glitch.Value = 1;
            SceneAs<Level>().Session.SetFlag("BigGlitching");
            yield return 5;
            Glitch.Value = _amount;
            SceneAs<Level>().Session.SetFlag(flag);
            SceneAs<Level>().Session.SetFlag("BigGlitching",false);
            SceneAs<Level>().Session.SetFlag("GlitchCutsceneEnd");
            PianoModule.SaveData.Escaped = true;
            yield return null;
        }
        private IEnumerator fadeLight()
        {
            //sorry i forgor to capitalize the f
            while (true)
            {
                player = Scene.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    yield break;
                }
                if (SceneAs<Level>().Session.GetFlag(flag))
                {
                    player.Light.Alpha = 1;
                    yield break;
                }
                if (player.Position.X < Center.X + Width && player.Position.X > Center.X - Width)
                {
                    if (player.Light.Alpha != 0)
                    {
                        player.Light.Alpha -= Engine.DeltaTime;
                    }
                }
                else
                {
                    if (player.Light.Alpha < 1)
                    {
                        player.Light.Alpha += Engine.DeltaTime;
                    }
                    else
                    {
                        player.Light.Alpha = 1;
                    }
                }

                yield return null;
            }
        }
        private IEnumerator StartingEvent()
        {
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                yield break;
            }
            //maddie get out of there oh no she cant hear us she's eating binary too loudly
            inEvent = true;
            player = Scene.Tracker.GetEntity<Player>();
            Vector2 _position = player.Position;
            player.DummyGravity = true;
            for (float i = 0; i < 1; i += 0.01f)
            {
                player.Speed.Y = 5;
                player.MoveToX(Calc.LerpClamp(_position.X, Center.X, i));
                player.MoveToY(Calc.LerpClamp(_position.Y, Center.Y + 16, i));
                yield return null;
            }
            Add(new Coroutine(HoldPosition(), true));
            Add(new Coroutine(RotateCutscene(), true));
            yield return 1f;
            SceneAs<Level>().Session.SetFlag("startWaiting");
            while (!scaleStart)
            {
                yield return null;
            }

            for (float i = 0; i < 1; i += 0.08f)
            {
                player.Sprite.Scale.X = Calc.LerpClamp(1, 2f, i);
                yield return null;
            }
            for (float i = 0; i < 1; i += 0.1f)
            {

                player.Sprite.Scale.X = Calc.LerpClamp(2, 0, i);
                player.Sprite.Scale.Y = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            for (int i = 0; i < 16; i++)
            {
                PoofParticles();
                player.Visible = false;
                yield return null;
            }
            player.Visible = false;
            yield return 2f;

            SceneAs<Level>().Session.SetFlag("StartingPortalEnd");
            inEvent = false;
            EventComplete = true;
        }
        private IEnumerator EndingEvent()
        {
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                yield break;
            }
            //maddie get out of there oh no she cant hear us she's eating binary too loudly
            inEvent = true;
            player = Scene.Tracker.GetEntity<Player>();
            Vector2 _position = player.Position;
            player.DummyGravity = true;
            for (float i = 0; i < 1; i += 0.01f)
            {
                player.Speed.Y = 5;
                player.MoveToX(Calc.LerpClamp(_position.X, Center.X, i));
                player.MoveToY(Calc.LerpClamp(_position.Y, Center.Y + 16, i));
                yield return null;
            }
            Add(new Coroutine(HoldPosition(), true));
            Add(new Coroutine(RotateCutscene(), true));
            yield return 1f;
            SceneAs<Level>().Session.SetFlag("startWaiting");
            while (!scaleStart)
            {
                yield return null;
            }

            for (float i = 0; i < 1; i += 0.08f)
            {
                player.Sprite.Scale.X = Calc.LerpClamp(1, 2f, i);
                yield return null;
            }
            for (float i = 0; i < 1; i += 0.1f)
            {

                player.Sprite.Scale.X = Calc.LerpClamp(2, 0, i);
                player.Sprite.Scale.Y = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            for (int i = 0; i < 16; i++)
            {
                PoofParticles();
                player.Visible = false;
                DigitalEffect.ForceStop = true;
                yield return null;
            }
            yield return 1f;

            Add(new Coroutine(GlitchCutscene(), true));
            inEvent = false;
            EventComplete = true;
        }
        private IEnumerator RotateCutscene()
        {
            //SPEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEN
            inRotateEvent = true;
            float _rate = rate;
            float _min = minColor;
            float _max = maxColor;
            for (float i = 0; i < 1; i += 0.004f)
            {
                rate = Calc.LerpClamp(_rate, _rate + 0.3f, Ease.CubeIn(i));
                minColor = 0.7f;
                maxColor = 1f;
                yield return null;
            }
            scaleStart = true;
            innerFlashRate = 1;
            yield return 0.1f;
            float _rate2 = rate;
            for (float i = 0; i < 1; i += 0.01f)
            {
                rate = Calc.LerpClamp(_rate2, _rate, Ease.SineOut(i));
                innerFlashRate = Calc.LerpClamp(1, 0, Ease.SineOut(i));
                yield return null;
            }
            yield return null;
            innerFlashRate = 0;
            rate = _rate;
            scaleStart = true;
            yield return null;
            minColor = _min;
            maxColor = _max;
            inRotateEvent = false;

        }
        private IEnumerator HoldPosition()
        {
            while (inEvent)
            {
                player.MoveToX(Center.X);
                player.MoveToY(Center.Y + 16);
                yield return null;
            }
        }
        private IEnumerator RotationLerp()
        {
            //The cooler Speen
            while (true)
            {
                if (inRotateEvent)
                {
                    yield break;
                }
                float _rate = rate;
                yield return 0.7f;
                for (float i = 0; i < 1; i += 0.09f)
                {
                    rate = Calc.LerpClamp(_rate, _rate + 0.007f, Ease.CubeIn(i));
                    yield return null;
                }
                innerFlashRate = 1;
                yield return 0.1f;
                float _rate2 = rate;
                for (float i = 0; i < 1; i += 0.01f)
                {
                    rate = Calc.LerpClamp(_rate2, _rate, Ease.SineOut(i));
                    innerFlashRate = Calc.LerpClamp(1, 0, Ease.SineOut(i));
                    yield return null;
                }
                yield return null;
                innerFlashRate = 0;
                rate = _rate;
            }
        }
        #endregion
    }
}
