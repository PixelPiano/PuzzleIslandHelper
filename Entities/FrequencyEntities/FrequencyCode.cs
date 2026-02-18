using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    [CustomEntity("PuzzleIslandHelper/FrequencyCode")]
    public class FrequencyCode : Entity
    {
        public static HashSet<string> CompletedIDs => PianoModule.Session.CompletedFrequencyCodeIDs;
        public string CodeID;
        public float Duration;
        public Image Flash;
        public Sprite BaseSprite;
        public Sprite CompleteSprite;
        private bool stopOffSpriteWhenOn;
        private string flash, sprite, complete;
        private float[] rates;
        private bool useFlag;
        private FlagList flag;
        private FrequencyCodeComponent code;
        private Envelope envelope;
        private float topBound = 1, bottomBound = 0;
        private bool usesFlash;
        private Envelope.States state;
        public FrequencyCode(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(16, 16);
            stopOffSpriteWhenOn = data.Bool("stopOffSpriteWhenOn");
            flag = data.FlagList("flag");
            useFlag = data.Bool("useFlagInsteadOfID");
            Depth = data.Int("depth", -9000);
            flash = data.Attr("flashTexturePath");
            sprite = data.Attr("spriteOffPath");
            complete = data.Attr("spriteOnPath");

            CodeID = data.Attr("codeID");
            rates = [data.Float("rateA"), data.Float("rateB"), data.Float("rateC"), data.Float("rateD")];
            code = new FrequencyCodeComponent(rates);
            code.RequiresAudibleSound = true;
            code.StopAtFullPower = true;
            Add(code);
            code.OnFullPower = () =>
            {
                if (useFlag)
                {
                    flag.State = true;
                }
                else
                {
                    CompletedIDs.Add(CodeID);
                }
                envelope.Start();
                CompleteSprite.Color = Color.White * 0;
                if (usesFlash)
                {
                    Flash.Visible = true;
                    CompleteSprite.Play("idle");
                    Alarm.Set(this, Engine.DeltaTime * 5, () =>
                    {
                        Flash.Visible = false;
                        if (stopOffSpriteWhenOn) BaseSprite.Stop();
                    });
                }
                else
                {
                    if (stopOffSpriteWhenOn) BaseSprite.Stop();
                    CompleteSprite.Play("idle");
                }
            };
            envelope = new Envelope(1, 0.2f, 0.5f, 0, false, false, Envelope.Modes.Looping);
            envelope.OnUpdate = (e) =>
            {
                CompleteSprite?.SetColor(Color.White * Calc.LerpClamp(bottomBound, topBound, e.Eased));
            };
            envelope.OnStateChanged = (e) =>
            {
                state = e.State;
                if (e.State == Envelope.States.Sustain)
                {
                    bottomBound = bottomBound == 0.5f ? 0.3f : 0.5f;
                }
            };
            Add(envelope);
        }
        public string ResolvePathIssuesIfFixable(string input)
        {
            if(!GFX.Game.Has(input) && !GFX.Game.Has(input + "00") && (GFX.Game.Has("decals/" + input) || GFX.Game.Has("decals/" + input + "00")))
            {
                return "decals/" + input;
            }
            return input;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            sprite = ResolvePathIssuesIfFixable(sprite);
            complete = ResolvePathIssuesIfFixable(complete);
            flash = ResolvePathIssuesIfFixable(flash);
            usesFlash = GFX.Game.Has(flash);
            Flash = new Image(GFX.Game[flash]);
            BaseSprite = new Sprite(GFX.Game, sprite);
            CompleteSprite = new Sprite(GFX.Game, complete);
            BaseSprite.AddLoop("idle", "", 0.1f);
            CompleteSprite.AddLoop("idle", "", 0.1f);
            Add(BaseSprite, Flash, CompleteSprite);
            Flash.Visible = false;
            if (useFlag ? flag : CompletedIDs.Contains(CodeID))
            {
                if (!stopOffSpriteWhenOn) BaseSprite.Play("idle");
                CompleteSprite.Play("idle");
                code.Active = false;
                envelope.Start();
            }
            else
            {
                BaseSprite.Play("idle");
            }
        }
    }
}