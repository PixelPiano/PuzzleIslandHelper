using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using Celeste.Mod.Backdrops;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using FrostHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    public static class ShaderHelper
    {
        public static Effect ApplyStandardParameters(this Effect effect, Level level)
        {
            var parameters = effect.Parameters;
            Matrix? camera = level.Camera.Matrix;
            parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            parameters["CamPos"]?.SetValue(level.Camera.Position);
            parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * (GameplayBuffers.Gameplay.Width / 320));
            parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);

            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            // from communal helper
            Matrix halfPixelOffset = Matrix.Identity;

            parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);

            parameters["ViewMatrix"]?.SetValue(camera ?? Matrix.Identity);

            return effect;
        }
        public static Effect ApplyScreenSpaceParameters(this Effect effect, Level level)
        {
            var parameters = effect.Parameters;
            Matrix? camera = level.Camera.Matrix;
            parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            parameters["CamPos"]?.SetValue(level.Camera.Position);
            parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * (GameplayBuffers.Gameplay.Width / 320));
            parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);

            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            // from communal helper
            Matrix halfPixelOffset = Matrix.Identity;

            parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);

            parameters["ViewMatrix"]?.SetValue(Matrix.Identity);

            return effect;
        }

        public static Effect TryGetEffect(string id)
        {
            id = id.Replace('\\', '/');

            if (Everest.Content.TryGet($"Effects/PuzzleIslandHelper/Shaders/{id}.cso", out var effectAsset, true))
            {
                try
                {
                    Effect effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
                    return effect;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Failed to load the shader " + id);
                    Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Exception: \n" + ex.ToString());
                }
            }
            return null;
        }


    }
}