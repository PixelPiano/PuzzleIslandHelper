using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class FloppyLoader : Entity
    {
        public Interface Interface;
        public Image Tab;
        public Image ULTex, DLTex, SideTex, MiddleTex, TopTex, BottomTex;
        public List<Image> Panel = new();
        public float MaxWidth = 40;
        public float MaxHeight = 40;
        public bool State;
        public Vector2 OrigPosition;
        public float width;
        private float height;
        public List<Icon> Disks = new();
        private Coroutine routine;
        private bool changing;
        public FloppyLoader(Interface @interface) : base(@interface.Position)
        {
            Interface = @interface;
            Depth = Interface.BaseDepth - 1;
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/interface/floppyLoaderPanel"];
            ULTex = new Image(tex.GetSubtexture(0, 0, 8, 8));
            DLTex = new Image(tex.GetSubtexture(0, 0, 8, 8));
            DLTex.Effects = SpriteEffects.FlipVertically;

            TopTex = new Image(tex.GetSubtexture(8, 0, 8, 8));
            BottomTex = new Image(tex.GetSubtexture(8, 0, 8, 8));
            BottomTex.Effects = SpriteEffects.FlipVertically;
            SideTex = new Image(tex.GetSubtexture(0, 8, 8, 8));
            MiddleTex = new Image(tex.GetSubtexture(8, 8, 8, 8));
            Add(TopTex, BottomTex, SideTex, MiddleTex, ULTex, DLTex);
            Panel = new List<Image>() { ULTex, DLTex, TopTex, BottomTex, SideTex, MiddleTex };
            Tab = new Image(GFX.Game["objects/PuzzleIslandHelper/interface/floppyLoaderTab"]);
            Add(Tab);
            Collider = new Hitbox(Tab.Width, Tab.Height);
            routine = new Coroutine() { RemoveOnComplete = false };
            Add(routine);
        }
        public class Icon : Image
        {
            public FloppyDisk Disk;
            public bool Enabled;
            public Collider Collider;
            public Icon(Vector2 position, FloppyDisk disk) : base(GFX.Game["objects/PuzzleIslandHelper/interface/floppyIcon"], true)
            {
                Position = position;
                Disk = disk;
                Collider = new Hitbox(Width, Height);
            }
            public override void Update()
            {
                Collider.Position = RenderPosition;
                base.Update();
            }
            public bool Check(Interface computer)
            {
                return Visible && Collider.Collide(computer) && computer.LeftPressed;
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.HollowRect(Collider, Color.Cyan);
            }
        }
        public void Start(Scene scene)
        {
            Position = OrigPosition = Interface.Monitor.BottomRight - new Vector2(Width, Height);
            int x = 0, y = 0;
            int count = 0;
            foreach (FloppyDisk disk in PianoModule.Session.CollectedDisks)
            {
                Icon icon = new Icon(MiddleTex.Position + new Vector2(x, y), disk);
                count++;
                if (count >= 4)
                {
                    y += 8;
                    x = 0;
                    count = 0;
                }
                Disks.Add(icon);
                Add(icon);
            }
        }
        public void UpdateIconPositions()
        {
            int x = 0, y = 0;
            int count = 0;
            foreach (Icon i in Disks)
            {
                i.Position = MiddleTex.Position + new Vector2(x, y);
                count++;
                if (count >= 4)
                {
                    y += 8;
                    x = 0;
                    count = 0;
                }
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Rect(MiddleTex.RenderPosition, 8, 8, Color.White);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            int rows = (int)Calc.Max(1, PianoModule.Session.CollectedDisks.Count / 4);
            int columns = Calc.Clamp(PianoModule.Session.CollectedDisks.Count, 1, 4);
            columns += 2;
            width = columns * 8 + 16;
            height = rows * 8;
            DLTex.Position = new Vector2(Tab.Width, Tab.Height - 8);
            ULTex.Position = DLTex.Position + new Vector2(0, -(rows + 1) * 8);
            SideTex.Position = ULTex.Position + Vector2.UnitY * 8;
            TopTex.Position = new Vector2(DLTex.X + 8, ULTex.Position.Y);
            BottomTex.Position = new Vector2(DLTex.X + 8, DLTex.Position.Y);
            MiddleTex.Position = ULTex.Position + Vector2.One * 8;
            MiddleTex.Scale.X = BottomTex.Scale.X = TopTex.Scale.X = columns + 1;
            MiddleTex.Scale.Y = SideTex.Scale.Y = rows;
        }
        public void OnClick()
        {
            State = !State;
            if (State) ShowPanel();
        }
        public void HidePanel()
        {
            State = false;
            foreach (Image i in Panel)
            {
                i.Visible = false;
            }
            foreach (Icon i in Disks)
            {
                i.Visible = false;
            }
        }
        public void ShowPanel()
        {
            State = true;
            foreach (Image i in Panel)
            {
                i.Visible = true;
            }
            foreach (Icon i in Disks)
            {
                i.Visible = true;
            }
        }
        public override void Render()
        {
            base.Render();
        }
        private IEnumerator Open()
        {
            changing = true;
            ShowPanel();
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                Position.X = Calc.LerpClamp(OrigPosition.X, OrigPosition.X - width, Ease.CubeIn(i));
                yield return null;
            }
            Position.X = OrigPosition.X - width;
            changing = false;
        }
        private IEnumerator Close()
        {
            changing = true;
            ShowPanel();
            State = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                Position.X = Calc.LerpClamp(OrigPosition.X - width, OrigPosition.X, Ease.CubeIn(i));
                yield return null;
            }
            Position.X = OrigPosition.X;
            HidePanel();
            changing = false;
        }
        public override void Update()
        {

            if (!changing && Interface.LeftPressed && Interface.CollideCheck(this))
            {
                if (State)
                {
                    routine.Replace(Open());
                }
                else
                {
                    routine.Replace(Close());
                }
            }
            Position = Position.Floor();
            UpdateIconPositions();
            if (State)
            {
                foreach (Icon i in Disks)
                {
                    if (i.Check(Interface))
                    {
                        Interface.QuickLoadPreset(i.Disk.Preset);
                    }
                }
            }
            base.Update();
        }
    }
}