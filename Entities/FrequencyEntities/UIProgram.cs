using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    public abstract class UIProgram : Entity
    {
        public abstract class UITarget : Entity
        {
            public bool RenderOnce
            {
                get => renderOnce;
                set
                {
                    if (!value)
                    {
                        rendered = false;
                    }
                    renderOnce = value;
                }
            }
            private bool renderOnce;
            private bool rendered;
            public VirtualRenderTarget Target;
            public Color Color;
            public float Alpha = 1;
            public UIProgram UI;
            public UITarget(UIProgram ui, Color backgroundColor, int depth) : base()
            {
                UI = ui;
                Tag |= TagsExt.SubHUD;
                Depth = depth;
                Color = backgroundColor;
                Target = VirtualContent.CreateRenderTarget("fork-amp-speaker-ui-target", 1920, 1080);
                Add(new BeforeRenderHook(beforeRender));
            }
            public virtual void BeforeRender() { }
            public override void Render()
            {
                //base.Render();
                if (Alpha > 0)
                {
                    Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White * Alpha);
                }
            }
            public void RenderComponents()
            {
                base.Render();
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
            }
            private void beforeRender()
            {
                if (RenderOnce && rendered) return;
                Target.SetAsTarget(Color);
                BeforeRender();
                rendered = true;
            }
        }
        public class Foreground : UITarget
        {
            public Foreground(UIProgram ui, Color color) : base(ui, color, int.MinValue)
            {
                RenderOnce = true;
            }
        }
        public class Background : UITarget
        {
            public Background(UIProgram ui, Color color) : base(ui, color, int.MaxValue)
            {
                RenderOnce = true;
            }
        }
        public class EntitiesTarget : UITarget
        {
            public EntitiesTarget(UIProgram ui) : base(ui, Color.Transparent, 0)
            {

            }
            public override void BeforeRender()
            {
                base.BeforeRender();
                Draw.SpriteBatch.Begin();
                UI.RenderEntities(Scene);
                Draw.SpriteBatch.End();
            }
        }
        public List<Entity> Entities = [];
        public Color VectorColor, FGColor, BGColor;
        public bool InControl;
        public bool Finished;
        private float cantMoveTimer;
        private float cantConfirmTimer;
        private float cantCancelTimer;
        private bool canMove = true;
        private bool canConfirm = true;
        private bool canCancel = true;

        public Foreground FG;
        public Background BG;
        public EntitiesTarget entities;
        private bool start;
        public bool Running;
        public UIProgram(Color fgColor, Color bgColor, Color vectorColor, bool start = false) : base()
        {
            FGColor = fgColor;
            BGColor = bgColor;
            VectorColor = vectorColor;
            this.start = start;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (start)
            {
                Begin();
            }
        }
        public void Begin()
        {
            Running = true;
            FG = new Foreground(this, FGColor);
            BG = new Background(this, BGColor);
            entities = new EntitiesTarget(this);

            Scene.Add(BG, entities);
            Entities = CreateEntities(Scene);
            foreach (Entity e in Entities)
            {
                Scene.Add(e);
            }
            Scene.Add(FG);
            Add(new Coroutine(routine()));
        }
        public abstract List<Entity> CreateEntities(Scene scene);
        public virtual void RenderEntities(Scene scene)
        {
            foreach (Entity e in Entities)
            {
                if (e.Visible)
                {
                    e.Render();
                }
            }
        }
        public virtual void OnDirection(int x, int y)
        {

        }
        public virtual void OnCancel()
        {

        }
        public virtual void OnConfirm()
        {

        }
        public override void Update()
        {
            base.Update();
            if (cantMoveTimer > 0)
            {
                cantMoveTimer -= Engine.DeltaTime;
                if (cantMoveTimer <= 0) canMove = true;
            }
            if (cantConfirmTimer > 0)
            {
                cantConfirmTimer -= Engine.DeltaTime;
                if (cantConfirmTimer <= 0) canConfirm = true;
            }
            if (cantCancelTimer > 0)
            {
                cantCancelTimer -= Engine.DeltaTime;
                if (cantCancelTimer <= 0) canCancel = true;
            }
            if (InControl && !ForkAmpUI.UIActive)
            {
                if (canMove)
                {
                    OnDirection(Input.MenuLeft ? -1 : Input.MenuRight ? 1 : 0, Input.MenuUp ? -1 : Input.MenuDown ? 1 : 0);
                }
                if (Input.MenuCancel && canCancel)
                {
                    OnCancel();
                }
                else if (Input.MenuConfirm && canConfirm)
                {
                    OnConfirm();
                }
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            FG?.RemoveSelf();
            BG?.RemoveSelf();
            entities?.RemoveSelf();
            foreach (Entity e in Entities)
            {
                e.RemoveSelf();
            }
            Entities?.Clear();
            Finished = true;
        }
        public void DisableMovement(float time = -1)
        {
            cantMoveTimer = time;
            canMove = false;
        }
        public void DisableConfirm(float time = -1)
        {
            cantConfirmTimer = time;
            canConfirm = false;
        }
        public void DisableCancel(float time = -1)
        {
            cantCancelTimer = time;
            canCancel = false;
        }
        private IEnumerator routine()
        {
            yield return 1;
            foreach (Entity entity in CreateEntities(Scene))
            {
                Scene.Add(entity);
                Entities.Add(entity);
                entity.Visible = false;
            }
            yield return Ease.SineInOut.Lerp(1, f => FG.Alpha = 1 - f, true);
            InControl = true;
            while (Running)
            {
                yield return null;
            }
            yield return Ease.SineInOut.Lerp(1, f => FG.Alpha = f, true);
            RemoveSelf();
        }
        public void End()
        {
            Running = false;
        }

    }

}
