using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using Microsoft.Xna.Framework;

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
            parameters["CamPos"]?.SetValue(level.Camera.Position.Floor());
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
        public static Effect ApplyParameters(this Effect effect, Level level, Matrix matrix, float? amplitude = null)
        {
            var parameters = effect.Parameters;

            parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            parameters["CamPos"]?.SetValue(level.Camera.Position);
            parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * (GameplayBuffers.Gameplay.Width / 320f));
            parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);

            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            // from communal helper
            Matrix halfPixelOffset = Matrix.Identity;

            parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);
            parameters["ViewMatrix"]?.SetValue(matrix);
            if (amplitude.HasValue)
            {
                parameters["Amplitude"]?.SetValue(amplitude.Value);
            }
            return effect;
        }
        public static Effect ApplyMatrixParameters(this Effect effect, Matrix matrix)
        {
            var parameters = effect.Parameters;

            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            // from communal helper
            Matrix halfPixelOffset = Matrix.Identity;
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            Matrix matrix3 = matrix * Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
            matrix3 *= Matrix.CreateTranslation(-1f, 1f, 0f);
            parameters["World"]?.SetValue(matrix3);
            parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);
            parameters["ViewMatrix"]?.SetValue(matrix);
            parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            return effect;
        }
        public static Effect ApplyCameraParams(this Effect effect, Level level)
        {
            var parameters = effect.Parameters;
            Matrix matrix = level.Camera.Matrix;
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

            parameters["ViewMatrix"]?.SetValue(matrix);

            return effect;
        }
        public static Effect ApplyStandardParams(this Effect effect, Level level, bool identity)
        {
            var parameters = effect.Parameters;
            Matrix matrix = identity ? Matrix.Identity : level.Camera.Matrix;
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

            parameters["ViewMatrix"]?.SetValue(matrix);

            return effect;
        }
        public static Effect ApplyVectorZeroParams(this Effect effect, Level level)
        {
            var parameters = effect.Parameters;
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

        public static Effect TryGetEffect(string id, bool fullPath = false)
        {
            id = id.Replace('\\', '/');

            string name = fullPath ? $"Effects/{id}.cso" : $"Effects/PuzzleIslandHelper/Shaders/{id}.cso";
            if (Everest.Content.TryGet(name, out var effectAsset, true))
            {
                try
                {
                    Effect effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
                    return effect;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Failed to load the Shader " + id);
                    Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Exception: \n" + ex.ToString());
                }
            }
            return null;
        }
    }
}
