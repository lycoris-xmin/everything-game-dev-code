using System;
using System.Collections.Generic;
using System.Text;

namespace XianxiaRPG.Systems.Core;

/// <summary>
/// SYS-004 轻量 CSV 解析。纯 C#，不依赖 Godot，可 NUnit 直接测。
/// 支持双引号转义（"" 表示字面量引号）与逗号分隔字段。
/// 返回行列表，每行是字段列表；首行通常为表头。
/// </summary>
public static class CsvParser
{
    public static List<List<string>> Parse(string text)
    {
        var rows = new List<List<string>>();
        if (string.IsNullOrEmpty(text)) return rows;

        var row = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
                else if (c == '\n') { row.Add(field.ToString()); rows.Add(row); row = new List<string>(); field.Clear(); }
                else if (c != '\r') field.Append(c);
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }
        return rows;
    }
}
