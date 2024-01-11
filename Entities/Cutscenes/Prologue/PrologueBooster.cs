using Celeste.Mod.Entities;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using ExtendedVariants.Variants;
using static Celeste.TrackSpinner;
using MonoMod.Cil;
using Mono.Cecil.Cil;

// PuzzleIslandHelper.DecalEffects
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [CustomEntity("PuzzleIslandHelper/PrologueBooster")]
    [Tracked]
    public class PrologueBooster : Booster
    {

        private bool InBooster;
        public bool Boosted;
        public PrologueBooster(Vector2 position)
            : base(position, true)
        {
            InBooster = false;
        }

        public PrologueBooster(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }
        public new void OnPlayer(Player player)
        {
            if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
            {
                cannotUseTimer = 0.45f;
                if (red)
                {
                    player.RedBoost(this);
                }
                else
                {
                    player.Boost(this);
                }

                Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_enter" : "event:/game/04_cliffside/greenbooster_enter", Position);
                wiggler.Start();
                sprite.Play("inside");
                sprite.FlipX = player.Facing == Facings.Left;
            }
        }
        public new IEnumerator BoostRoutine(Player player, Vector2 dir)
        {
            InBooster = true;
            dir = -Vector2.UnitY;
            float angle = (-dir).Angle();
            while ((player.StateMachine.State == 2 || player.StateMachine.State == 5) && BoostingPlayer)
            {
                player.Speed = -Vector2.UnitY;
                sprite.RenderPosition = player.Center + playerOffset;
                loopingSfx.Position = sprite.Position;
                if (Scene.OnInterval(0.02f))
                {
                    (Scene as Level).ParticlesBG.Emit(particleType, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), angle);
                }

                yield return null;
            }
            InBooster = false;

            PlayerReleased();
            if (player.StateMachine.State == 4)
            {
                sprite.Visible = false;
            }

            while (SceneAs<Level>().Transitioning)
            {
                yield return null;
            }

            Tag = 0;
        }

    }
}
