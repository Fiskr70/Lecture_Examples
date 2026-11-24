namespace LogHub;

public static class Configuration
{
    public static Dictionary<string, string> Load(string path)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!File.Exists(path))
        {
            values["sink"] = "console";
            values["minSeverity"] = "Info";
            values["format"] = "plain";
            return values;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
            var fields = line.Split('=', 2);
            if (fields.Length == 2) values[fields[0].Trim()] = fields[1].Trim();
        }
        return values;
    }
}
