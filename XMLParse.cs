using Celeste.Mod.PuzzleIslandHelper.Boss;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using YamlDotNet.Core.Tokens;

public static class XMLParse
{
    //
    // Summary:
    //     Gets a parsable value from the xml attribute named attr. If the attribute does
    //     not exist or is invalid, returns def.
    public static T Get<T>(this XmlAttributeCollection xml, string attr, T def) where T : IParsable<T>
    {
        XmlAttribute xmlAttribute = xml[attr];
        if (xmlAttribute == null)
        {
            return def;
        }

        if (T.TryParse(xmlAttribute.Value, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return def;
    }
    public static T Get<T>(this Dictionary<string, string> dict, string attr, T def) where T : IParsable<T>
    => dict.TryGetValue(attr, out string value) && T.TryParse(value, CultureInfo.InvariantCulture, out T result) ? result : def;
    public static T Get<T>(this Dictionary<string, XmlNode> dict, string attr, T def) where T : IParsable<T>
        => dict.TryGetValue(attr, out XmlNode value) ? value.AsT<T>() : def;
    public static T Get<T>(this ActionRegistry.XmlActionData list, string attr, T def) where T : IParsable<T>
        => list[attr] is XmlNode node ? node.AsT<T>() : def;
    public static T Get<T>(this XmlNode node, string attr, T def) where T : IParsable<T> => node.Attributes.Get(attr, def);
    public static T AsT<T>(this XmlNode node) where T : IParsable<T>
        => T.Parse(node.Value, CultureInfo.InvariantCulture);
    //
    // Summary:
    //     Gets a bool value from the xml attribute named attr. If the attribute does not
    //     exist or is invalid, returns def.
    public static bool GetBool(this XmlAttributeCollection xml, string attr, bool def)
    {
        XmlAttribute xmlAttribute = xml[attr];
        if (xmlAttribute == null)
        {
            return def;
        }

        if (bool.TryParse(xmlAttribute.Value, out var result))
        {
            return result;
        }
        return def;
    }
    public static bool GetBool(this Dictionary<string, string> dict, string attr, bool def)
        => dict.TryGetValue(attr, out string value) && bool.TryParse(value, out bool result) ? result : def;
    public static bool GetBool(this Dictionary<string, XmlNode> dict, string attr, bool def)
        => dict.TryGetValue(attr, out XmlNode value) ? value.AsBool() : def;
    public static bool GetBool(this ActionRegistry.XmlActionData list, string attr, bool def)
        => list[attr]?.AsBool() ?? def;
    public static bool GetBool(this XmlNode node, string attr, bool def) => node.Attributes.GetBool(attr, def);
    public static bool AsBool(this XmlNode node) => bool.Parse(node.Value);
    //
    // Summary:
    //     Gets a string value from the xml attribute named attr. If the attribute does
    //     not exist or is invalid, returns def.
    public static string GetString(this XmlAttributeCollection xml, string attr, string def)
    {
        XmlAttribute xmlAttribute = xml[attr];
        if (xmlAttribute == null)
        {
            return def;
        }
        return xmlAttribute.Value;
    }
    public static string GetString(this Dictionary<string, string> dict, string attr, string def)
        => dict.TryGetValue(attr, out string value) ? value : def;
    public static string GetString(this Dictionary<string, XmlNode> dict, string attr, string def)
        => dict.TryGetValue(attr, out XmlNode value) ? value.Value : def;
    public static string GetString(this ActionRegistry.XmlActionData list, string attr, string def)
        => list[attr]?.Value ?? def;
    public static string GetString(this XmlNode node, string attr, string def) => node.Attributes.GetString(attr, def);
    public static Ease.Easer GetEase(this XmlAttributeCollection xml, string attr, Ease.Easer def)
    {
        XmlAttribute xmlAttribute = xml[attr];
        if (xmlAttribute == null)
        {
            return def;
        }
        string value = xmlAttribute.Value.Replace(" ", "").ToLower();
        return Parse(value);
    }
    public static Ease.Easer GetEase(this Dictionary<string, string> dict, string attr, Ease.Easer def)
        => dict.TryGetValue(attr, out string value) ? Parse(value.Replace(" ", "").ToLower()) : def;
    public static Ease.Easer GetEase(this Dictionary<string, XmlNode> dict, string attr, Ease.Easer def)
        => dict.TryGetValue(attr, out XmlNode value) ? value.AsEase() : def;
    public static Ease.Easer GetEase(this XmlNode node, string attr, Ease.Easer def) => node.Attributes.GetEase(attr, def);
    public static Ease.Easer AsEase(this XmlNode node) => Parse(node.Value.Replace(" ", "").ToLower());
    public static Ease.Easer Parse(string input)
    {
        return input switch
        {
            "sineinout" or "sine" => Ease.SineInOut,
            "sinein" => Ease.SineIn,
            "sineout" => Ease.SineOut,
            "cubeinout" or "cube" => Ease.CubeInOut,
            "cubein" => Ease.CubeIn,
            "cubeout" => Ease.CubeOut,
            "elastic" or "elasticinout" => Ease.ElasticInOut,
            "elasticin" => Ease.ElasticIn,
            "elasticout" => Ease.ElasticOut,
            "quint" or "quintinout" => Ease.QuintInOut,
            "quintin" => Ease.QuintIn,
            "quineout" => Ease.QuintOut,
            "quad" or "quadinout" => Ease.QuadInOut,
            "quadin" => Ease.QuadIn,
            "quadout" => Ease.QuadOut,
            "back" or "backinout" => Ease.BackInOut,
            "backin" => Ease.BackIn,
            "backout" => Ease.BackOut,
            "bigback" or "bigbackinout" => Ease.BigBackInOut,
            "bigbackin" => Ease.BigBackIn,
            "bigbackout" => Ease.BigBackOut,
            "expo" or "expoinout" => Ease.ExpoInOut,
            "expoin" => Ease.ExpoIn,
            "expoout" => Ease.ExpoOut,
            "bounce" or "bounceinout" => Ease.BounceInOut,
            "bouncein" => Ease.BounceIn,
            "bounceout" => Ease.BounceOut,
            _ => Ease.Linear
        };
    }
    //
    // Summary:
    //     Gets a hex color value from the xml attribute named attr, by calling Monocle.Calc.HexToColor(System.String).
    //     If the attribute does not exist or is invalid, returns def.
    public static Color GetHexColor(this XmlAttributeCollection xml, string attr, Color def)
    {
        XmlAttribute xmlAttribute = xml[attr];
        if (xmlAttribute == null)
        {
            return def;
        }
        return Calc.HexToColor(xmlAttribute.Value);
    }
    public static Color GetHexColor(this Dictionary<string, string> dict, string attr, Color def)
        => dict.TryGetValue(attr, out string value) ? Calc.HexToColor(value) : def;
    public static Color GetHexColor(this Dictionary<string, XmlNode> dict, string attr, Color def)
        => dict.TryGetValue(attr, out XmlNode value) ? value.AsColor() : def;
    public static Color GetHexColor(this XmlNode node, string attr, Color def) => node.Attributes.GetHexColor(attr, def);
    public static Color AsColor(this XmlNode node) => Calc.HexToColor(node.Value);
    //
    // Summary:
    //     Gets a CSV int array value from the xml attribute named attr, by calling Monocle.Calc.ReadCSVIntWithTricks(System.String).
    //     If the attribute does not exist or is invalid, returns def.
    public static int[] GetCSVIntWithTricks(this XmlAttributeCollection xml, string attr, string def)
    {
        XmlAttribute xmlAttribute = xml[attr];
        if (xmlAttribute == null)
        {
            return Calc.ReadCSVIntWithTricks(def);
        }

        try
        {
            return Calc.ReadCSVIntWithTricks(xmlAttribute.Value);
        }
        catch (Exception)
        {
            return Calc.ReadCSVIntWithTricks(def);
        }
    }
    public static int[] GetCSVIntWithTricks(this Dictionary<string, string> dict, string attr, string def)
    {
        if (dict.TryGetValue(attr, out string value))
        {
            try
            {
                return Calc.ReadCSVIntWithTricks(value);
            }
            catch (Exception)
            {
                return Calc.ReadCSVIntWithTricks(def);
            }
        }
        return Calc.ReadCSVIntWithTricks(def);
    }
    public static int[] GetCSVIntWithTricks(this Dictionary<string, XmlNode> dict, string attr, string def)
    {
        if (dict.TryGetValue(attr, out XmlNode value))
        {
            try
            {
                return Calc.ReadCSVIntWithTricks(value.Value);
            }
            catch (Exception)
            {
                return Calc.ReadCSVIntWithTricks(def);
            }
        }
        return Calc.ReadCSVIntWithTricks(def);
    }
    public static int[] GetCSVIntWithTricks(this XmlNode node, string attr, string def) => node.Attributes.GetCSVIntWithTricks(attr, def);
    //
    // Summary:
    //     Equivalent to Celeste.Mod.Registry.DecalRegistryHandlers.DecalRegistryHandler.Get``1(System.Xml.XmlAttributeCollection,System.String,``0),
    //     but returns null for value types if the attribute does not exist.
    public static TParsable? GetNullable<TParsable>(this XmlAttributeCollection xml, string attr) where TParsable : struct, IParsable<TParsable>
    {
        XmlAttribute xmlAttribute = xml[attr];
        if (xmlAttribute == null)
        {
            return null;
        }

        if (TParsable.TryParse(xmlAttribute.Value, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return null;
    }
    public static TParsable? GetNullable<TParsable>(this Dictionary<string, string> dict, string attr) where TParsable : struct, IParsable<TParsable>
        => dict.TryGetValue(attr, out string value) && TParsable.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : null;
    public static TParsable? GetNullable<TParsable>(this Dictionary<string, XmlNode> dict, string attr) where TParsable : struct, IParsable<TParsable>
        => dict.TryGetValue(attr, out XmlNode value) ? value.AsNullable<TParsable>() : null;
    public static TParsable? GetNullable<TParsable>(this XmlNode node, string attr) where TParsable : struct, IParsable<TParsable>
    => node.Attributes.GetNullable<TParsable>(attr);
    public static TParsable? AsNullable<TParsable>(this XmlNode node) where TParsable : struct, IParsable<TParsable>
        => TParsable.TryParse(node.Value, CultureInfo.InvariantCulture, out var result) ? result : null;
    public static Vector2 GetVector2(this XmlAttributeCollection xml, string attrX, string attrY, Vector2 def)
    {
        return new Vector2(Get(xml, attrX, def.X), Get(xml, attrY, def.Y));
    }
    public static Vector2 GetVector2(this Dictionary<string, string> dict, string attrX, string attrY, Vector2 def)
    {
        return new Vector2(Get(dict, attrX, def.X), Get(dict, attrY, def.Y));
    }
    public static Vector2 GetVector2(this Dictionary<string, XmlNode> dict, string attrX, string attrY, Vector2 def)
    {
        return new Vector2(Get(dict, attrX, def.X), Get(dict, attrY, def.Y));
    }
    public static Vector2 GetVector2(this XmlNode node, string attrX, string attrY, Vector2 def) => node.Attributes.GetVector2(attrX, attrY, def);
}
