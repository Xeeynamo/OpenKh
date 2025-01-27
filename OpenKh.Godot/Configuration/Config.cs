using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Configuration;

public enum Platform
{
    None,
    Steam //TODO
}
public static class PlatformHelpers
{
    public static System.Collections.Generic.Dictionary<Platform, string> Names = Enum.GetValues<Platform>().ToDictionary(i => i, i => i.ToString());
}

public class GamePathConfig
{
    public readonly string Identifier;
    public string GamePath = "";
    public Platform GamePlatform = Platform.None;
    public GamePathConfig(string identifier)
    {
        Identifier = identifier;
        Load();
    }
    public void Save()
    {
        var file = FileAccess.Open($"user://{Identifier}.khcfg", FileAccess.ModeFlags.Write);

        file.StorePascalString(Json.Stringify(new Dictionary
        {
            {"path", GamePath},
            {"platform", GamePlatform.ToString()},
            {"version", 1},
        }));
            
        file.Close();
    }
    public void Load()
    {
        var file = FileAccess.Open($"user://{Identifier}.khcfg", FileAccess.ModeFlags.Read);

        if (file == null) return;

        var str = file.GetPascalString();
            
        file.Close();

        var json = Json.ParseString(str);

        if (json.VariantType != Variant.Type.Dictionary) return;

        var jsonDict = json.AsGodotDictionary();

        if (jsonDict.TryGetValue("path", out var path)) GamePath = path.AsString();
        if (jsonDict.TryGetValue("platform", out var platform)) GamePlatform = Enum.Parse<Platform>(platform.AsString());
    }
}
public static class Config
{
    public static GamePathConfig HDRemixConfig = new("HDRemix");
    
    public static IReadOnlyCollection<GamePathConfig> Configs = new List<GamePathConfig>{ HDRemixConfig }.AsReadOnly();
}
