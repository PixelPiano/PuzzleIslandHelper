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
    public class DesktopClickable : Entity
    {
        public Interface Interface;
        private int delayFrames;
        private float timer;
        public bool AlwaysClickable;

        public DesktopClickable(Interface @interface, int delayFrames = 0, bool alwaysClickable = false) : base(@interface.Position)
        {
            Interface = @interface;
            Depth = Interface.BaseDepth - 1;
            this.delayFrames = delayFrames;
            AlwaysClickable = alwaysClickable;
            Visible = false;
        }
        /// <summary>
        /// Called just before the computer scene loads up (the poor man's awake)
        /// </summary>
        public virtual void Begin(Scene scene)
        {
        }
        /// <summary>
        /// Prepare anything that might need to interact with other desktop clickables (the poor man's added)
        /// </summary>
        public virtual void Prepare(Scene scene)
        {

        }

        /// <summary>
        /// Runs when the player clicks on the clickable
        /// </summary>
        public virtual void OnClick()
        {
            if (timer < delayFrames * Engine.DeltaTime) return;
            timer = 0;
        }
        public override void Update()
        {
            Position = Position.Floor();
            base.Update();
            if (timer < delayFrames * Engine.DeltaTime)
            {
                timer += Engine.DeltaTime;
            }
        }
    }
}