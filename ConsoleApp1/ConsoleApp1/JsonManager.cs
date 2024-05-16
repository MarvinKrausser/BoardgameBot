using ConsoleApp1.DataContainers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ConsoleApp1;

/// <summary>
///     Die Klasse ist für das Serialisieren und Deserialisieren der Json Dateien zuständig
/// </summary>
internal class JsonManager
{
    private static readonly Dictionary<messageEnum, string> _schemas = new();
    public static string path = string.Empty;

    /// <summary>
    ///     Überprüft ob ein übergebenes Json mit bestimmtem Typ valide ist
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsValid(string json, messageEnum? type)
    {
        if (type is null) return false;
        var jObject = JObject.Parse(json);
        var schemaJason = _schemas[(messageEnum)type];

        //Parst das Schema - Rückgabe ob geglückt oder nciht
        var schema = JSchema.Parse(schemaJason);
        IList<string> messages;
        if (jObject.IsValid(schema, out messages)) return true;
        Console.WriteLine($"Not valid: {type}");
        Console.WriteLine(json);
        foreach (var m in messages) Console.WriteLine(m);
        return false;
    }

    /// <summary>
    ///     Gibt den Typ einer Json Datei zurück
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static messageEnum? GetTypeJson(string json)
    {
        //Parse Objekt - rückgabe falls geglückt
        JObject jObject;
        try
        {
            jObject = JObject.Parse(json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

        if (jObject.First is null) return null;
        var type = jObject.First.ToObject<string>();
        if (type is null) return null;
        if (Enum.IsDefined(typeof(messageEnum), type)) return Enum.Parse<messageEnum>(type);

        return null;
    }

    public static T DeserializeJson<T>(string json)
    {
        var o = JsonConvert.DeserializeObject<T>(json);
        return o;
    }

    public static string ConvertToJson<T>(T ob)
    {
        return JsonConvert.SerializeObject(ob);
    }

    /// <summary>
    ///     Läd die Konfigurationsdateien die dem Spiel übergeben werden.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string getConfigJson(string name)
    {
        //name should be board or game
        //default /var/lib/app/config/board.json(game.json)

        //Ursprünglicher Pfad: E:\\Projekte\\JsonManager\\Examples\\" + name + "config.json"
        string input;
        using (var
               sr = new StreamReader(name)) //pfad geht von .exe aus unter bin/Debug/.net7.0/GameServer.exe
        {
            input = sr.ReadToEnd();
        }

        return input;
    }

    public static void ChooseDirectory()
    {
        var path1 = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Schemas\\error.schema.json"; //Local Project
        var path2 = "/src/ConsoleApp1/Schemas/error.schema.json"; //Docker File
        var path3 = "/ConsoleApp1/Schemas/error.schema.json"; //Docker Compose
        var path4 = string.Empty;

        if (File.Exists(path1))
        {
            path4 = @"..\\..\\..\\Schemas\\";
            path = @"..\\..\\..\\ReconnectToken\\";
        }
        else if (File.Exists(path2))
        {
            path4 = "/src/ConsoleApp1/Schemas/";
            path = "/src/ConsoleApp1/ReconnectToken/";
        }
        else if (File.Exists(path3))
        {
            path4 = "/ConsoleApp1/Schemas/";
            path = "/ConsoleApp1/ReconnectToken/";
        }


        foreach (var m in Enum.GetValues<messageEnum>())
        {
            using (var sr =
                   new StreamReader(path4 + m.ToString().ToLower().Replace("_", string.Empty) + ".schema.json"))
            {
                var schemaJason = sr.ReadToEnd();
                _schemas[m] = schemaJason;
            }
        }
    }
}