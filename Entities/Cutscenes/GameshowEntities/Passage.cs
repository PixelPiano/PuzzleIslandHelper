using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Threading;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/Passage")]
    [Tracked]
    public class Passage : Entity
    {
        public string TeleportTo;
        public Facings Facing;
        private Solid Solid;
        private Image DoorBg;
        private Image DoorFg;
        private Image LightFg;
        private Image LightBg;
        private bool FadeState;
        private float bgLightAlpha;
        private float bgLightTarget;
        private float fgLightTarget;
        private float fgLightAlpha;
        private bool teleporting;
        public static bool AllTeleportsActive;
        private bool deactivated;
        public float FadeTime;
        public Action OnTransition;
        public Color LightColor;
        public string FolderPath;
        public int EndPlayerState = Player.StNormal;
        public Passage(Vector2 position, string teleportTo, Facings facing, float fadeTime, Color lightColor, string folderPath) : base(position)
        {
            FolderPath = folderPath;
            AllTeleportsActive = true;
            TeleportTo = teleportTo;
            Facing = facing;
            LightColor = lightColor;
            LightFg = CenterAndReposition(new Image(GFX.Game[folderPath + "lightFg(side)"]));
            LightBg = CenterAndReposition(new Image(GFX.Game[folderPath + "light(side)"]));
            DoorFg = CenterAndReposition(new Image(GFX.Game[folderPath + "texture"].GetSubtexture(26, 0, 26, 32)));
            DoorBg = CenterAndReposition(new Image(GFX.Game[folderPath + "texture"].GetSubtexture(0, 0, 26, 32)));
            Depth = 1;
            LightFg.Color = LightBg.Color = LightColor * 0;
            FadeTime = fadeTime;
            float w = Facing == Facings.Right ? 8 : 7;
            float x = Facing == Facings.Right ? 18 : 0;

            ColliderList list = new ColliderList(
                new Hitbox(DoorBg.Width / 2, 10, Facing == Facings.Right ? DoorBg.Width / 2 : 0),
                new Hitbox(w, 9, x, 10));

            Solid = new Solid(Position, 0, 0, true)
            {
                Depth = -10001,
                Collider = list
            };
            Collider = new Hitbox(DoorBg.Width + 3, DoorBg.Height - 1, (int)Facing * 10, 1);
            LightFg.Position.X -= (Facing == Facings.Left ? 1 : 0) * LightFg.Width / 2;
            Solid.Add(DoorFg, LightFg);
            Add(DoorBg, LightBg);
        }
        public Passage(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("teleportTo"), data.Enum<Facings>("facing"), data.Float("fadeTime", 1), data.HexColor("lightColor"), data.Attr("folderPath"))
        {

        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            AllTeleportsActive = true;
            scene.Remove(Solid);
        }
        public void Deactivate()
        {
            deactivated = true;
            LightBg.Color = LightFg.Color = LightColor * 0;
            fgLightAlpha = fgLightTarget = bgLightAlpha = bgLightTarget = 0;
            FadeState = false;
        }
        public override void Update()
        {
            base.Update();
            if (!AllTeleportsActive || teleporting || deactivated) return;
            LightBg.Color = LightColor * (bgLightAlpha = Calc.Approach(bgLightAlpha, bgLightTarget, Engine.DeltaTime * 2));
            LightFg.Color = LightColor * (fgLightAlpha = Calc.Approach(fgLightAlpha, fgLightTarget, Engine.DeltaTime * 5));
            if (Scene is Level level && level.GetPlayer() is Player player)
            {

                if (fgLightAlpha >= 1)
                {
                    fgLightAlpha = 1;
                    TeleportPlayer(player);
                    return;
                }
                float check = player.CenterX;
                if (!CollideCheck<Player>() || (int)player.Facing == (int)Facing)
                {
                    FadeOut();
                }
                else
                {
                    FadeIn();
                }
                if (FadeState)
                {
                    fgLightTarget = 1f - Calc.Clamp(MathHelper.Distance(check, Facing == Facings.Right ? Left + 5 : Right - 5) / DoorBg.Width / 2, 0, 1f);
                }
            }
        }
        public void TeleportPlayer(Player player)
        {
            if (!AllTeleportsActive) return;
            teleporting = true;
            AllTeleportsActive = false;
            Scene.Add(new PassageTransition(player, this));
            OnTransition?.Invoke();
        }

        public void FadeOut()
        {
            bgLightTarget = 0;
            fgLightTarget = 0;
            FadeState = false;
        }
        public void FadeIn()
        {
            bgLightTarget = 1;
            fgLightTarget = 0;
            FadeState = true;
        }
        public Image CenterAndReposition(Image image)
        {
            image.CenterOrigin();
            image.Position += new Vector2(image.Width / 2, image.Height / 2);
            image.Scale = new Vector2(-(int)Facing, 1);
            return image;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Solid);
        }
    }
}
