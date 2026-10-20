using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Photos
{
    public class PhotoData
    {
        public HashSet<string> Tags { get; set; } = [];
        public string Path { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
    public class Photo
    {
        public static Dictionary<string, PhotoData> Datas = [];
        public PhotoData Data;
        private Texture2D texture;
        public Photo(string path)
        {
            Datas.TryGetValue(path, out Data);
        }
        public Photo(VirtualRenderTarget capture, string path)
        {
            Data = new()
            {
                Path = path,
                Title = path
            };
            texture = PianoUtils.SaveTargetAsPng(capture, path);
            Datas[path] = Data;
        }
        public void AddTags(params string[] tags)
        {
            foreach (var tag in tags)
            {
                Data.Tags.Add(tag);
            }
        }
        public void RemoveTags(params string[] tags)
        {
            foreach(string tag in tags)
            {
                Data.Tags.Remove(tag);
            }
        }
        public void Register()
        {
            Datas[Data.Path] = Data;
        }
    }

}