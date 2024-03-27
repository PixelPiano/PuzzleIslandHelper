using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;


namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked(false)]
    public class MaskRenderHook : Component
    {
        public Action RenderDrawing;
        public Action RenderMask;
        public bool CameraMatrix;
        public Effect Effect;
        public VirtualRenderTarget Target;
        public VirtualRenderTarget Mask;
        public MaskRenderHook(Action drawing, Action mask, bool cameraMatrix = true, Effect effect = null)
            : base(active: false, visible: true)
        {
            RenderDrawing = drawing;
            RenderMask = mask;
            CameraMatrix = cameraMatrix;
            Effect = effect;
            Target = VirtualContent.CreateRenderTarget("MaskRenderHookTarget", 320, 180);
            Mask = VirtualContent.CreateRenderTarget("MaskRenderHookMask", 320, 180);

        }
    }
}
