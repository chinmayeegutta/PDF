using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class SimpleTemplateEngine
{
    private static readonly Regex PlaceholderRegex = new(@"{{\s*([a-zA-Z0-9_.]+)\s*}}", RegexOptions.Compiled);
    private static readonly Regex EachBlockRegex = new(@"{{#each\s+([a-zA-Z0-9_.]+)}}(.*?){{/each}}", RegexOptions.Singleline | RegexOptions.Compiled);

    public string Parse(string template, Dictionary<string, object> data)
    {
        // First process each blocks (loops)
        string result = EachBlockRegex.Replace(template, match =>
        {
            string listKey = match.Groups[1].Value;
            string blockContent = match.Groups[2].Value;

            if (!TryGetValueByPath(data, listKey, out var listObj))
                return "";

            if (listObj is IEnumerable list && !(listObj is string))
            {
                var sb = new StringBuilder();
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> itemDict)
                    {
                        // Process block content with item as context
                        string processedBlock = ReplacePlaceholders(blockContent, itemDict);
                        sb.Append(processedBlock);
                    }
                    else
                    {
                        // If item is simple value, replace {{this}} placeholder
                        var singleDict = new Dictionary<string, object> { ["this"] = item };
                        string processedBlock = ReplacePlaceholders(blockContent, singleDict);
                        sb.Append(processedBlock);
                    }
                }
                return sb.ToString();
            }

            return "";
        });

        // Then replace simple placeholders
        result = ReplacePlaceholders(result, data);

        return result;
    }

    private string ReplacePlaceholders(string template, Dictionary<string, object> data)
    {
        return PlaceholderRegex.Replace(template, match =>
        {
            string path = match.Groups[1].Value;
            if (TryGetValueByPath(data, path, out var value) && value != null)
            {
                return value.ToString();
            }
            return "";
        });
    }

    private bool TryGetValueByPath(Dictionary<string, object> data, string path, out object value)
    {
        string[] parts = path.Split('.');
        object current = data;
        foreach (var part in parts)
        {
            if (current is Dictionary<string, object> dict)
            {
                if (!dict.TryGetValue(part, out current))
                {
                    value = null;
                    return false;
                }
            }
            else
            {
                value = null;
                return false;
            }
        }
        value = current;
        return true;
    }
}
