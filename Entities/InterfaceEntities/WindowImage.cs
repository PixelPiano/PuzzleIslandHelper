using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class WindowImage : WindowComponent
    {
        public MTexture Texture;
        public float Alpha
        {
            get
            {
                return alpha * windowAlpha;
            }
            set
            {
                alpha = value;
            }
        }
        private float alpha = 1;
        private float windowAlpha => Window.Alpha;
        public virtual float Width => Texture.Width;
        public virtual float Height => Texture.Height;
        public Vector2 ImageOffset;

        public Vector2 Origin;

        public Vector2 Scale = Vector2.One;

        public float Rotation;

        public Color Color = Color.White;
        public SpriteEffects Effects;

        public bool Outline;

        public Color OutlineColor = Color.Black;
        public float X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position.X = value;
            }
        }

        public float Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position.Y = value;
            }
        }

        public bool FlipX
        {
            get
            {
                return (Effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
            }
            set
            {
                Effects = (value ? (Effects | SpriteEffects.FlipHorizontally) : (Effects & ~SpriteEffects.FlipHorizontally));
            }
        }

        public bool FlipY
        {
            get
            {
                return (Effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            }
            set
            {
                Effects = (value ? (Effects | SpriteEffects.FlipVertically) : (Effects & ~SpriteEffects.FlipVertically));
            }
        }
        public WindowImage(Window window, MTexture texture)
            : base(window)
        {
            Texture = texture;
        }
        public WindowImage(Window window, MTexture texture, bool active)
            : base(window, active)
        {
            Texture = texture;
        }

        public override void Render()
        {
            if (Alpha > 0)
            {            if (Outline)
            {
                DrawOutline(OutlineColor * Alpha);
            }
            DrawTexture(Color * Alpha);
                if (Outline)
                {
                    DrawOutline(OutlineColor);
                }
                DrawTexture(Color);
            }
        }
        public void DrawTexture(Color color)
        {
            if (Texture != null)
            {
                Texture.Draw(RenderPosition + ImageOffset, Origin, color * Alpha, Scale, Rotation, Effects);
            }
        }
        public void DrawOutline(int offset = 1)
        {
            DrawOutline(Color.Black, offset);
        }

        public void DrawOutline(Color color, int offset = 1)
        {
            Vector2 position = Position;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Position = position + new Vector2(i * offset, j * offset);
                        DrawTexture(color);
                    }
                }
            }

            Position = position;
        }

        public void DrawSimpleOutline()
        {
            Vector2 position = Position;
            Color color = Color.Black;
            Position = position + new Vector2(-1f, 0f);
            DrawTexture(color);
            Position = position + new Vector2(0f, -1f);
            DrawTexture(color);
            Position = position + new Vector2(1f, 0f);
            DrawTexture(color);
            Position = position + new Vector2(0f, 1f);
            DrawTexture(color);
            Position = position;
        }

        public WindowImage SetOrigin(float x, float y)
        {
            Origin.X = x;
            Origin.Y = y;
            return this;
        }

        public WindowImage CenterOrigin()
        {
            Origin.X = Width / 2f;
            Origin.Y = Height / 2f;
            return this;
        }

        public WindowImage JustifyOrigin(Vector2 at)
        {
            Origin.X = Width * at.X;
            Origin.Y = Height * at.Y;
            return this;
        }

        public WindowImage JustifyOrigin(float x, float y)
        {
            Origin.X = Width * x;
            Origin.Y = Height * y;
            return this;
        }

        public WindowImage SetColor(Color color)
        {
            Color = color;
            return this;
        }
        public override void OnOpened(Scene scene)
        {
        }
        public override void OnClosed(Scene scene)
        {
        }
    }
    public class WindowSprite : WindowImage
    {
        public class Animation
        {
            public float Delay;

            public MTexture[] Frames;

            //
            // Summary:
            //     Used to determine the animation to play once this one ends.
            public Chooser<string> Goto;
        }

        //
        // Summary:
        //     The animation speed modifier.
        public float Rate = 1f;

        //
        // Summary:
        //     Whether to update animations based on Monocle.Engine.RawDeltaTime.
        public bool UseRawDeltaTime;

        public Vector2? Justify;

        //
        // Summary:
        //     Invoked when this sprite stops animating.
        public Action<string> OnFinish;

        //
        // Summary:
        //     Invoked when current the animation loops.
        public Action<string> OnLoop;

        //
        // Summary:
        //     Invoked when when the current frame changes.
        public Action<string> OnFrameChange;

        //
        // Summary:
        //     Invoked when the animation is about to CurrentNode, loop, or change.
        public Action<string> OnLastFrame;

        //
        // Summary:
        //     Invoked when the current animation changes.
        public Action<string, string> OnChange;


        public Atlas atlas;

        //
        // Summary:
        //     The root directory of this sprite's animations.
        public string Path;


        public Dictionary<string, Animation> animations;


        public Animation currentAnimation;


        public float animationTimer;


        public int width;


        public int height;

        //
        // Summary:
        //     The relative center of the sprite.
        public Vector2 Center => new Vector2(Width / 2f, Height / 2f);

        //
        // Summary:
        //     Whether this sprite is currently playing an animation.
        public bool Animating;

        public string CurrentAnimationID;
        public string LastAnimationID;

        public int CurrentAnimationFrame;

        public int CurrentAnimationTotalFrames
        {
            get
            {
                if (currentAnimation != null)
                {
                    return currentAnimation.Frames.Length;
                }

                return 0;
            }
        }

        public override float Width => width;

        public override float Height => height;

        public Dictionary<string, Animation> Animations => animations;

        //
        // Summary:
        //     Create a new Sprite from a texture Monocle.Atlas and a root path.
        //
        // Parameters:
        //   atlas:
        //     The atlas to draw textures from.
        //
        //   path:
        //     The root path for this sprite's animation textures.
        public WindowSprite(Window window, Atlas atlas, string path)
            : base(window, null, active: true)
        {
            this.atlas = atlas;
            Path = path;
            animations = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
            CurrentAnimationID = "";
        }

        //
        // Summary:
        //     Reset this Sprite from a texture Monocle.Atlas and a root path.
        //
        // Parameters:
        //   atlas:
        //     The atlas to draw textures from.
        //
        //   path:
        //     The root path for this sprite's animation textures.
        public void Reset(Atlas atlas, string path)
        {
            this.atlas = atlas;
            Path = path;
            animations = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
            currentAnimation = null;
            CurrentAnimationID = "";
            OnFinish = null;
            OnLoop = null;
            OnFrameChange = null;
            OnChange = null;
            Animating = false;
        }

        //
        // Summary:
        //     Retrieve the Monocle.MTexture associated with a specific frame of an animation.
        //
        // Parameters:
        //   animation:
        //     The name of the animation.
        //
        //   frame:
        //     The frame startIndex.
        public MTexture GetFrame(string animation, int frame)
        {
            return animations[animation].Frames[frame];
        }

        public override void Update()
        {
            if (!Animating)
            {
                return;
            }

            if (UseRawDeltaTime)
            {
                animationTimer += Engine.RawDeltaTime * Rate;
            }
            else
            {
                animationTimer += Engine.DeltaTime * Rate;
            }

            if (!(Math.Abs(animationTimer) >= currentAnimation.Delay))
            {
                return;
            }

            CurrentAnimationFrame += Math.Sign(animationTimer);
            animationTimer -= (float)Math.Sign(animationTimer) * currentAnimation.Delay;
            if (CurrentAnimationFrame < 0 || CurrentAnimationFrame >= currentAnimation.Frames.Length)
            {
                string currentAnimationID = CurrentAnimationID;
                if (OnLastFrame != null)
                {
                    OnLastFrame(CurrentAnimationID);
                }

                if (!(currentAnimationID == CurrentAnimationID))
                {
                    return;
                }

                if (currentAnimation.Goto != null)
                {
                    CurrentAnimationID = currentAnimation.Goto.Choose();
                    if (OnChange != null)
                    {
                        OnChange(LastAnimationID, CurrentAnimationID);
                    }

                    LastAnimationID = CurrentAnimationID;
                    currentAnimation = animations[LastAnimationID];
                    if (CurrentAnimationFrame < 0)
                    {
                        CurrentAnimationFrame = currentAnimation.Frames.Length - 1;
                    }
                    else
                    {
                        CurrentAnimationFrame = 0;
                    }

                    SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
                    if (OnLoop != null)
                    {
                        OnLoop(CurrentAnimationID);
                    }
                }
                else
                {
                    if (CurrentAnimationFrame < 0)
                    {
                        CurrentAnimationFrame = 0;
                    }
                    else
                    {
                        CurrentAnimationFrame = currentAnimation.Frames.Length - 1;
                    }

                    Animating = false;
                    string currentAnimationID2 = CurrentAnimationID;
                    CurrentAnimationID = "";
                    currentAnimation = null;
                    animationTimer = 0f;
                    if (OnFinish != null)
                    {
                        OnFinish(currentAnimationID2);
                    }
                }
            }
            else
            {
                SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
            }
        }


        public void SetFrame(MTexture texture)
        {
            if (texture != Texture)
            {
                Texture = texture;
                if (width == 0)
                {
                    width = texture.Width;
                }

                if (height == 0)
                {
                    height = texture.Height;
                }

                if (Justify.HasValue)
                {
                    Origin = new Vector2((float)Texture.Width * Justify.Value.X, (float)Texture.Height * Justify.Value.Y);
                }

                if (OnFrameChange != null)
                {
                    OnFrameChange(CurrentAnimationID);
                }
            }
        }

        //
        // Summary:
        //     Set the current animation to the specified frame.
        //
        // Parameters:
        //   frame:
        public void SetAnimationFrame(int frame)
        {
            animationTimer = 0f;
            CurrentAnimationFrame = frame % currentAnimation.Frames.Length;
            SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
        }

        //
        // Summary:
        //     Add a repeating animation to the sprite that can then be referenced using id.
        //
        // Parameters:
        //   id:
        //     The Animation id.
        //
        //   path:
        //     Path relative to Monocle.Sprite.Path to draw textures from.
        //
        //   delay:
        //     Delay between each frame.
        public void AddLoop(string id, string path, float delay)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = GetFrames(path),
                Goto = new Chooser<string>(id, 1f)
            };
        }

        //
        // Parameters:
        //   frames:
        //     The frame indices to use in the animation.
        public void AddLoop(string id, string path, float delay, params int[] frames)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = GetFrames(path, frames),
                Goto = new Chooser<string>(id, 1f)
            };
        }

        //
        // Parameters:
        //   frames:
        //     The textures to use as frames for this animation.
        public void AddLoop(string id, float delay, params MTexture[] frames)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = frames,
                Goto = new Chooser<string>(id, 1f)
            };
        }

        //
        // Summary:
        //     Add an animation to the sprite that can then be referenced using id.
        //
        // Parameters:
        //   id:
        //     The Animation id.
        //
        //   path:
        //     Path relative to Monocle.Sprite.Path to draw textures from.
        public void Add(string id, string path)
        {
            animations[id] = new Animation
            {
                Delay = 0f,
                Frames = GetFrames(path),
                Goto = null
            };
        }

        //
        // Parameters:
        //   delay:
        //     Delay between each frame.
        public void Add(string id, string path, float delay)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = GetFrames(path),
                Goto = null
            };
        }

        //
        // Parameters:
        //   frames:
        //     The frame indices to use in the animation.
        public void Add(string id, string path, float delay, params int[] frames)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = GetFrames(path, frames),
                Goto = null
            };
        }

        //
        // Parameters:
        //   into:
        //     The animation to play once this one ends.
        public void Add(string id, string path, float delay, string into)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = GetFrames(path),
                Goto = Chooser<string>.FromString<string>(into)
            };
        }

        //
        // Parameters:
        //   into:
        //     Determines the animation to play once this one ends.
        public void Add(string id, string path, float delay, Chooser<string> into)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = GetFrames(path),
                Goto = into
            };
        }

        //
        // Parameters:
        //   frames:
        //     The frame indices to use in the animation.
        public void Add(string id, string path, float delay, string into, params int[] frames)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = GetFrames(path, frames),
                Goto = Chooser<string>.FromString<string>(into)
            };
        }

        //
        // Parameters:
        //   frames:
        //     The textures to use as frames for this animation.
        public void Add(string id, float delay, string into, params MTexture[] frames)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = frames,
                Goto = Chooser<string>.FromString<string>(into)
            };
        }

        //
        // Parameters:
        //   frames:
        //     The frame indices to use in the animation.
        public void Add(string id, string path, float delay, Chooser<string> into, params int[] frames)
        {
            animations[id] = new Animation
            {
                Delay = delay,
                Frames = GetFrames(path, frames),
                Goto = into
            };
        }


        public MTexture[] GetFrames(string path, int[] frames = null)
        {
            MTexture[] array;
            if (frames == null || frames.Length == 0)
            {
                array = atlas.GetAtlasSubtextures(Path + path).ToArray();
            }
            else
            {
                string text = Path + path;
                MTexture[] array2 = new MTexture[frames.Length];
                for (int i = 0; i < frames.Length; i++)
                {
                    MTexture atlasSubtexturesAt = atlas.GetAtlasSubtexturesAt(text, frames[i]);
                    if (atlasSubtexturesAt == null)
                    {
                        throw new Exception("Can't find sprite " + text + " with index " + frames[i]);
                    }

                    array2[i] = atlasSubtexturesAt;
                }

                array = array2;
            }

            width = Math.Max(array[0].Width, width);
            height = Math.Max(array[0].Height, height);
            return array;
        }

        //
        // Summary:
        //     Remove all animation data from the sprite.
        public void ClearAnimations()
        {
            animations.Clear();
        }

        //
        // Summary:
        //     Play an animation stored in this sprite.
        //
        // Parameters:
        //   id:
        //     The animation to play.
        //
        //   restart:
        //     Whether to restart the animation if it is already playing.
        //
        //   randomizeFrame:
        //     Whether to randomize the starting frame and animation timer.
        public void Play(string id, bool restart = false, bool randomizeFrame = false)
        {
            if (CurrentAnimationID != id || restart)
            {
                if (OnChange != null)
                {
                    OnChange(LastAnimationID, id);
                }

                string text3 = (LastAnimationID = (CurrentAnimationID = id));
                currentAnimation = animations[id];
                Animating = currentAnimation.Delay > 0f;
                if (randomizeFrame)
                {
                    animationTimer = Calc.Random.NextFloat(currentAnimation.Delay);
                    CurrentAnimationFrame = Calc.Random.Next(currentAnimation.Frames.Length);
                }
                else
                {
                    animationTimer = 0f;
                    CurrentAnimationFrame = 0;
                }

                SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
            }
        }

        //
        // Summary:
        //     Play an animation stored in this sprite.
        //
        // Parameters:
        //   id:
        //     The animation to play.
        //
        //   offset:
        //     The amount to add to the animation timer.
        //
        //   restart:
        //     Whether to restart the animation if it is already playing.
        public void PlayOffset(string id, float offset, bool restart = false)
        {
            if (!(CurrentAnimationID != id || restart))
            {
                return;
            }

            if (OnChange != null)
            {
                OnChange(LastAnimationID, id);
            }

            string text3 = (LastAnimationID = (CurrentAnimationID = id));
            currentAnimation = animations[id];
            if (currentAnimation.Delay > 0f)
            {
                Animating = true;
                float num = currentAnimation.Delay * (float)currentAnimation.Frames.Length * offset;
                CurrentAnimationFrame = 0;
                while (num >= currentAnimation.Delay)
                {
                    CurrentAnimationFrame++;
                    num -= currentAnimation.Delay;
                }

                CurrentAnimationFrame %= currentAnimation.Frames.Length;
                animationTimer = num;
                SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
            }
            else
            {
                animationTimer = 0f;
                Animating = false;
                CurrentAnimationFrame = 0;
                SetFrame(currentAnimation.Frames[0]);
            }
        }

        //
        // Summary:
        //     Play an animation, returning an IEnumerator that will return null until the the
        //     sprite stops animating.
        //
        // Parameters:
        //   id:
        //     The animation to play.
        //
        //   restart:
        //     Whether to restart the animation if it is already playing.
        public IEnumerator PlayRoutine(string id, bool restart = false)
        {
            Play(id, restart);
            return PlayUtil();
        }

        //
        // Summary:
        //     Play an animation stored in this sprite, setting Monocle.Sprite.Rate to negative.
        //     Returns an IEnumerator that will return null until the the sprite stops animating.
        //
        // Parameters:
        //   id:
        //     The animation to play.
        //
        //   restart:
        //     Whether to restart the animation if it is already playing.
        public IEnumerator ReverseRoutine(string id, bool restart = false)
        {
            Reverse(id, restart);
            return PlayUtil();
        }


        public IEnumerator PlayUtil()
        {
            while (Animating)
            {
                yield return null;
            }
        }

        //
        // Summary:
        //     Play an animation stored in this sprite, setting Monocle.Sprite.Rate to negative.
        //
        // Parameters:
        //   id:
        //     The animation to play.
        //
        //   restart:
        //     Whether to restart the animation if it is already playing.
        public void Reverse(string id, bool restart = false)
        {
            Play(id, restart);
            if (Rate > 0f)
            {
                Rate *= -1f;
            }
        }

        //
        // Summary:
        //     Whether this sprite has an animation matching id.
        //
        // Parameters:
        //   id:
        public bool Has(string id)
        {
            if (id != null)
            {
                return animations.ContainsKey(id);
            }

            return false;
        }

        //
        // Summary:
        //     Stop the currently playing animation.
        public void Stop()
        {
            Animating = false;
            currentAnimation = null;
            CurrentAnimationID = "";
        }


        public WindowSprite(Window window)
            : base(window, null, active: true)
        {
        }


        public WindowSprite CreateClone()
        {
            return CloneInto(new WindowSprite(Window));
        }


        public WindowSprite CloneInto(WindowSprite clone)
        {
            clone.Texture = Texture;
            clone.Position = Position;
            clone.Justify = Justify;
            clone.Origin = Origin;
            clone.animations = new Dictionary<string, Animation>(animations, StringComparer.OrdinalIgnoreCase);
            clone.currentAnimation = currentAnimation;
            clone.animationTimer = animationTimer;
            clone.width = width;
            clone.height = height;
            clone.Animating = Animating;
            clone.CurrentAnimationID = CurrentAnimationID;
            clone.LastAnimationID = LastAnimationID;
            clone.CurrentAnimationFrame = CurrentAnimationFrame;
            return clone;
        }

        //
        // Summary:
        //     Draw a rectangle from the current frame.
        //
        // Parameters:
        //   offset:
        //     Relative offset to draw at.
        //
        //   rectangle:
        //     Rectangle to draw.
        public void DrawSubrect(Vector2 offset, Rectangle rectangle)
        {
            if (Texture != null)
            {
                Rectangle relativeRect = Texture.GetRelativeRect(rectangle);
                Vector2 vector = new Vector2(0f - Math.Min((float)rectangle.X - Texture.DrawOffset.X, 0f), 0f - Math.Min((float)rectangle.Y - Texture.DrawOffset.Y, 0f));
                Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, base.RenderPosition + offset, relativeRect, Color, Rotation, Origin - vector, Scale, Effects, 0f);
            }
        }

        public void LogAnimations()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, Animation> animation in animations)
            {
                Animation value = animation.Value;
                stringBuilder.Append(animation.Key);
                stringBuilder.Append("\n{\n\t");
                object[] frames = value.Frames;
                stringBuilder.Append(string.Join("\n\t", frames));
                stringBuilder.Append("\n}\n");
            }

            Calc.Log(stringBuilder.ToString());
        }
    }
}