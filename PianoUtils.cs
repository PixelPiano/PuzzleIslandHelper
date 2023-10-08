// PuzzleIslandHelper.PuzzleIslandHelperCommands
using Celeste;
using Celeste.Mod.PuzzleIslandHelper;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

public class PianoUtils
{
    public static T SeekController<T>(Scene scene, Func<T> factory = null) where T : Entity
    {
        T controller = scene.Tracker.GetEntity<T>();

        if (controller is not null)
        {
            return controller;
        }

        foreach (Entity entity in scene.Entities.ToAdd)
        {
            if (entity is T t)
            {
                return t;
            }
        }

        if (factory is null)
        {
            return null;
        }

        scene.Add(controller = factory());
        return controller;
    }
}
