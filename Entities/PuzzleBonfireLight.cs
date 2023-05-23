// PuzzleIslandHelper.PuzzleBonfireLight
using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity(new string[] { "PuzzleIslandHelper/PuzzleBonfireLight" })]
[Tracked(false)]
internal class BonfireLight : Entity
{
	public Color lightColor;

	private Sprite mySprite;

	private VertexLight light;

	private BloomPoint bloom;

	private float brightness;

	private float multiplier;

	private Wiggler wiggle;

	private float randBase = 0.5f;

	private float randAdd = 0.5f;

	private float randFreq = 0.25f;

	private bool disableOnPhotosensitive;

	public BonfireLight(EntityData data, Vector2 offset)
		: base(data.Position + offset)
	{
		base.Depth = -9999;
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		int startFade = 32;
		int endFade = 64;
		float radius = 32f;
		float frequency = 4f;
		float duration = 0.2f;
		if (data.Bool("backwardsCompatibility"))
		{
			startFade = data.Int("lightFadeStart");
			endFade = data.Int("lightFadeEnd");
			radius = data.Float("bloomRadius");
			randBase = data.Float("baseBrightness");
			randAdd = data.Float("brightnessVariance");
			randFreq = data.Float("flashFrequency");
			frequency = data.Float("wigglerFrequency");
			duration = data.Float("wigglerDuration");
			disableOnPhotosensitive = data.Bool("photosensitivityConcern");
		}
		lightColor = Calc.HexToColor(data.Attr("lightColor"));
		base.Tag = Tags.TransitionUpdate;
		Add(light = new VertexLight(new Vector2(0f, 0f), lightColor, 1f, startFade, endFade));
		Add(bloom = new BloomPoint(new Vector2(0f, 0f), 1f, radius));
		Add(wiggle = Wiggler.Create(duration, frequency, delegate(float wigglerVar)
		{
			light.Alpha = (bloom.Alpha = Math.Min(1f, brightness + wigglerVar * 0.25f) * multiplier);
		}));
	}
    public override void Awake(Scene scene){
        base.Awake(scene);
		Add(mySprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/puzzleBonfireLight/"));
		mySprite.AddLoop("spookyLight", "spookyLight", 0.1f);
		mySprite.CenterOrigin();
		mySprite.Y = 2f;
		Depth = -10001;
		mySprite.Play("spookyLight");
    }

	public override void Update()
	{
		if (disableOnPhotosensitive && Settings.Instance.DisableFlashes)
		{
			base.Update();
			return;
		}
		multiplier = Calc.Approach(multiplier, 1f, Engine.DeltaTime * 2f);
		if (base.Scene.OnInterval(randFreq))
		{
			brightness = randBase + Calc.Random.NextFloat(randAdd);
			wiggle.Start();
		}
		base.Update();
	}
}
