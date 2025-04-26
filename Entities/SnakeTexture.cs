using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.PuzzleIslandHelper.Components.BitrailNode;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class SnakeTexture
    {
        private static Dictionary<string, Vector2> CornerTextureQuadss = new()
        {
            {"upLeft",new(2,1) },
            {"upRight",new(1,1)},
            {"downRight",new(1,0)},
            {"downLeft",new(2,0) }
        };
        private static string getDirection(float x1, float y1, float x2, float y2)
        {
            if (x2 < x1 && y1 == y2) return "left";
            if (x2 > x1 && y1 == y2) return "right";
            if (y2 < y1 && x1 == x2) return "up";
            if (y2 > y1 && x1 == x2) return "down";
            return "none";
        }
        private static string getCornerType(string direction, string nextDirection)
        {
            if ((direction == "right" && nextDirection == "up") || (direction == "down" && nextDirection == "left"))
                return "upLeft";

            else if ((direction == "down" && nextDirection == "right") || (direction == "left" && nextDirection == "up"))
                return "upRight";

            else if ((direction == "left" && nextDirection == "down") || (direction == "up" && nextDirection == "right"))
                return "downRight";

            else if ((direction == "up" && nextDirection == "left") || (direction == "right" && nextDirection == "down"))
                return "downLeft";

            return "unknown";
        }
        private static float getLength(float x1, float y1, float x2, float y2, float width, bool startNode, bool endNode)
        {
            double length = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            if (startNode)
            {
                length += width / 2;
            }
            if (endNode)
            {
                length -= width / 2;
            }
            return (float)Math.Floor(length);
        }
        private static (float x1, float y1, float width, float length)
            getStraightDrawingInfo(float x1, float y1, float x2, float y2, float width, float length, string direction, bool startNode, bool endNode)
        {

            return direction switch
            {
                "up" => (x2 - width / 2, y2 - (endNode ? 0 : width / 2), width, length),
                "right" => (x1 + (startNode ? 0 : width / 2), y1 - width / 2, width, length),
                "down" => (x1 - width / 2, y1 + (startNode ? 0 : width / 2), width, length),
                "left" => (x2 - (endNode ? 0 : width / 2), y2 - width / 2, width, length),
                _ => (x1, y1, width, length)
            };
        }
        private static (float verticalWallX, float horizontalWallY, float offsetX, float offsetY)
            getCornerDrawingInfo(string cornerType, string direction, float width, float length)
        {
            float verticalWallX = -1, horizontalWallY = -1;
            float offsetX = 0, offsetY = 0;
            switch (cornerType)
            {
                case "upLeft":
                    verticalWallX = width - 8;
                    horizontalWallY = width - 8;
                    break;
                case "upRight":
                    verticalWallX = 0;
                    horizontalWallY = width - 8;
                    break;
                case "downRight":
                    verticalWallX = 0;
                    horizontalWallY = 0;
                    break;
                case "downLeft":
                    verticalWallX = width - 8;
                    horizontalWallY = 0;
                    break;
            }
            if (direction == "right")
            {
                offsetX = length - width;
            }
            else if (direction == "down")
            {
                offsetY = length - width;
            }
            return (verticalWallX, horizontalWallY, offsetX, offsetY);
        }
        private static Image createImage(MTexture texture, float x, float y, float quadX, float quadY)
        {
            return new Image(texture.GetSubtexture((int)quadX * 8, (int)quadY * 8, 8, 8)) { X = x, Y = y };
        }
        private static List<Image> createStraightHorizontalImages(MTexture texture, float x, float y, float width, float length, string direction, bool startNode, bool endNode)
        {
            List<Image> images = [];
            for (int ox = 0; ox < length - 1; ox += 8)
            {
                images.Add(createImage(texture, x + ox, y, 0, 0));
            }
            return images;
        }
        private static List<Image> createStraightVerticalImages(MTexture texture, float x, float y, float width, float length, string direction, bool startNode, bool endNode)
        {
            List<Image> images = [];
            for (int oy = 0; oy < length - 1; oy += 8)
            {
                images.Add(createImage(texture, x, y + oy, 0, 1));
            }
            return images;
        }
        private static List<Image> createCornerImages(MTexture texture, float x, float y, float width, float aaa, float length, string direction, string cornerType, bool startNode, bool endNode)
        {
            List<Image> images = [];
            if (endNode || width < 8 || cornerType == "unknown") return images;
            Vector2 cornerQuad = CornerTextureQuadss[cornerType];
            (float verticalWallX, float horizontalWallY, float offsetX, float offsetY) info = getCornerDrawingInfo(cornerType, direction, aaa, length);
            for (int ox = 0; ox < 7; ox += 8)
            {
                for (int oy = 0; oy < 7; oy += 8)
                {
                    images.Add(createImage(texture, x + ox + info.offsetX, y + oy + info.offsetY, cornerQuad.X, cornerQuad.Y));
                }
            }
            return images;
        }
        private static List<Image> createSectionSprites(MTexture texture, Vector2 p, Vector2 n, Vector2 nn, float width, bool startNode, bool endNode)
        {
            List<Image> images = [];
            string direction = getDirection(p.X, p.Y, n.X, n.Y);
            string nextDirection = getDirection(n.X, n.Y, nn.X, nn.Y);
            string cornerType = getCornerType(direction, nextDirection);
            float pipeLength = getLength(p.X, p.Y, n.X, n.Y, width, startNode, endNode);
            bool vertical = direction == "up" || direction == "down";
            bool horizontal = direction == "left" || direction == "right";
            (float drawX, float drawY, float drawWidth, float drawLength) info = getStraightDrawingInfo(p.X, p.Y, n.X, n.Y, width, pipeLength, direction, startNode, endNode);
            var straightLength = !endNode && cornerType != "unknown" ? info.drawLength - info.drawWidth : info.drawLength;
            var drawingOffset = !endNode && (direction == "up" || direction == "left") ? info.drawWidth : 0;
            if (horizontal)
            {
                images.AddRange(createStraightHorizontalImages(texture, info.drawX + drawingOffset, info.drawY, info.drawWidth, straightLength, direction, startNode, endNode));
                images.AddRange(createCornerImages(texture, info.drawX, info.drawY, info.drawWidth, width, pipeLength, direction, cornerType, startNode, endNode));
            }
            else if (vertical)
            {
                images.AddRange(createStraightVerticalImages(texture, info.drawX, info.drawY + drawingOffset, info.drawWidth, straightLength, direction, startNode, endNode));
                images.AddRange(createCornerImages(texture, info.drawX, info.drawY, info.drawWidth, width, pipeLength, direction, cornerType, startNode, endNode));
            }
            return images;
        }
        private static List<Image> createImages(MTexture texture, Vector2 position, Vector2[] nodes)
        {
            List<Image> images = [];
            float width = 8;
            for (int j = 0; j < nodes.Length; j++)
            {
                bool startNode = j == 0;
                bool endNode = j == nodes.Length - 1;
                Vector2 n = nodes[j];
                Vector2 nn = endNode ? -Vector2.One : nodes[j + 1];
                images.AddRange(createSectionSprites(texture, position, n, nn, width, startNode, endNode));
                position = n;
            }
            return images;
        }
        private static string defaultPath = "objects/PuzzleIslandHelper/powerLineSimple";
        public static List<Image> Create(MTexture texture, Vector2[] nodes, Vector2 offset = default)
        {
            return createImages(texture ?? GFX.Game[defaultPath], offset, nodes);
        }
    }
}
