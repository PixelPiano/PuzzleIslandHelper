using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.XaphanHelper.Effects;
using FMOD.Studio;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/ObsoleteStone")]
    [Tracked]
    public class ObsoleteStone : Entity
    {
        private static string path = "objects/PuzzleIslandHelper/obsoleteStone/";
        public FrameCycler Cycler;
        public bool Glitchy;
        public bool CanTalk;
        public string Dialog;
        public Image Stone;
        public ObsoleteStone(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("glitchy"), data.Attr("dialog"))
        {
        }
        public ObsoleteStone(Vector2 position, bool glitchy, string dialog = "") : base(position)
        {
            Dialog = dialog;
            Glitchy = glitchy;
            Depth = 2;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            float delay = Calc.Random.Range(0.1f, 0.3f);
            char[] array = new char[3] { 'A', 'B', 'C' };
            Cycler = new FrameCycler(GFX.Game, path);
            Cycler.AddLoop("code", "code/code" + array[Calc.Random.Range(0, 3)], delay);
            Cycler.AddLoop("glitch", "glitchyWriting", delay);
            Cycler.AddLoop("glitchStone", "glitchStone", delay);
            Cycler.Play(Glitchy ? "glitch" : "code", true, true);
            Stone = new Image(GFX.Game[path + "stone"]);
            Collider = new Hitbox(Stone.Width,Stone.Height);
            Add(Stone, Cycler);
            Add(new Coroutine(GlitchRoutine()));
            if (!string.IsNullOrEmpty(Dialog))
            {
                Add(new DotX3(Collider, p => Add(new Coroutine(Cutscene(p)))));
            }
        }
        private IEnumerator Cutscene(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            yield return Textbox.Say(Dialog);
            player.StateMachine.State = Player.StNormal;
        }
        public IEnumerator GlitchRoutine()
        {
            while (true)
            {
                if (Glitchy)
                {
                    yield return Calc.Random.Range(1, 3);
                }
                else
                {
                    yield return Calc.Random.Range(6, 20);
                }
                int loops = Calc.Random.Chance(0.3f) ? 2 : 1;
                MTexture prev = Stone.Texture;
                Cycler.Pause();
                for (int i = 0; i < loops; i++)
                {
                    Stone.Texture = Cycler.RandomTexture("glitchStone");
                    Cycler.SetTexture(Cycler.RandomTexture("glitch"));
                    yield return Calc.Random.Choose(1, 2) * Engine.DeltaTime;
                }
                Cycler.Unpause();
                Stone.Texture = prev;
            }
        }
        public override void Render()
        {
            Stone.DrawSimpleOutline();
            base.Render();
        }
    }
}