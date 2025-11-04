namespace Blazor2026.Services;

using System.Text;
using System.Text.RegularExpressions;

public static partial class SqlFormatterService
{
    [GeneratedRegex(@"\bSELECT\b", RegexOptions.IgnoreCase)]
    private static partial Regex SelectRegex();

    [GeneratedRegex(@"\bFROM\b", RegexOptions.IgnoreCase)]
    private static partial Regex FromRegex();

    [GeneratedRegex(@"\bWHERE\b", RegexOptions.IgnoreCase)]
    private static partial Regex WhereRegex();

    [GeneratedRegex(@"\bAND\b", RegexOptions.IgnoreCase)]
    private static partial Regex AndRegex();

    [GeneratedRegex(@"\bOR\b", RegexOptions.IgnoreCase)]
    private static partial Regex OrRegex();

    [GeneratedRegex(@"\bLEFT\s+JOIN\b", RegexOptions.IgnoreCase)]
    private static partial Regex LeftJoinRegex();

    [GeneratedRegex(@"\bRIGHT\s+JOIN\b", RegexOptions.IgnoreCase)]
    private static partial Regex RightJoinRegex();

    [GeneratedRegex(@"\bINNER\s+JOIN\b", RegexOptions.IgnoreCase)]
    private static partial Regex InnerJoinRegex();

    [GeneratedRegex(@"\bON\b", RegexOptions.IgnoreCase)]
    private static partial Regex OnRegex();

    [GeneratedRegex(@"\bAS\b", RegexOptions.IgnoreCase)]
    private static partial Regex AsRegex();

    [GeneratedRegex(@"\s*,\s*", RegexOptions.None)]
    private static partial Regex CommaWithSpacesRegex();

    [GeneratedRegex(@"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SelectColumnsRegex();

    [GeneratedRegex(@"(?<!LEFT\s)(?<!RIGHT\s)(?<!INNER\s)\bJOIN\b", RegexOptions.IgnoreCase)]
    private static partial Regex StandaloneJoinRegex();

    public static string FormatSql(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return string.Empty;
        }

        try
        {
            var result = sql.Trim();
            result = SelectRegex().Replace(result, "SELECT");
            var selectMatch = SelectColumnsRegex().Match(result);
            if (selectMatch.Success)
            {
                var columns = selectMatch.Groups[1].Value;
                var formattedColumns = CommaWithSpacesRegex().Replace(columns.Trim(), ", \n\t");
                result = result.Replace(selectMatch.Value, $"SELECT \n\t{formattedColumns} \nFROM");
            }

            result = FromRegex().Replace(result, "\nFROM");
            result = AsRegex().Replace(result, " AS ");
            result = LeftJoinRegex().Replace(result, "\n\tLEFT JOIN");
            result = RightJoinRegex().Replace(result, "\n\tRIGHT JOIN");
            result = InnerJoinRegex().Replace(result, "\n\tINNER JOIN");
            result = StandaloneJoinRegex().Replace(result, "\n\tJOIN");
            result = OnRegex().Replace(result, "\n\tON");

            var lines = result.Split('\n');
            var joinCount = 0;

            for (var i = 0; i < lines.Length; i++)
            {
                var trimmedLine = lines[i].Trim();

                if (trimmedLine.StartsWith("LEFT JOIN", StringComparison.OrdinalIgnoreCase) ||
                    trimmedLine.StartsWith("RIGHT JOIN", StringComparison.OrdinalIgnoreCase) ||
                    trimmedLine.StartsWith("INNER JOIN", StringComparison.OrdinalIgnoreCase) ||
                    trimmedLine.StartsWith("JOIN", StringComparison.OrdinalIgnoreCase) &&
                    !trimmedLine.Contains("LEFT JOIN", StringComparison.OrdinalIgnoreCase) &&
                    !trimmedLine.Contains("RIGHT JOIN", StringComparison.OrdinalIgnoreCase) &&
                    !trimmedLine.Contains("INNER JOIN", StringComparison.OrdinalIgnoreCase))
                {
                    var indentation = new string('\t', joinCount + 1);
                    lines[i] = indentation + trimmedLine;
                    joinCount++;
                }
                else if (trimmedLine.StartsWith("ON", StringComparison.OrdinalIgnoreCase))
                {
                    var indentation = new string('\t', joinCount);
                    lines[i] = indentation + trimmedLine;
                }
            }

            result = string.Join('\n', lines);
            result = WhereRegex().Replace(result, "\n\nWHERE");
            var whereIndex = result.IndexOf("\n\nWHERE", StringComparison.OrdinalIgnoreCase);

            if (whereIndex >= 0)
            {
                var beforeWhere = result[..whereIndex];
                var afterWhere = result[whereIndex..];
                var processed = new StringBuilder();
                var parenDepth = 0;
                var i = 0;

                while (i < afterWhere.Length)
                {
                    if (afterWhere[i] == '(')
                    {
                        parenDepth++;
                        _ = processed.Append(afterWhere[i]);
                        i++;
                    }
                    else if (afterWhere[i] == ')')
                    {
                        parenDepth--;
                        _ = processed.Append(afterWhere[i]);
                        i++;
                    }
                    else if (parenDepth == 0)
                    {
                        var remaining = afterWhere[i..];
                        var andMatch = AndRegex().Match(remaining);
                        var orMatch = OrRegex().Match(remaining);

                        if (andMatch.Success && andMatch.Index == 0)
                        {
                            _ = processed.Append("\nAND");
                            i += andMatch.Length;
                        }
                        else if (orMatch.Success && orMatch.Index == 0)
                        {
                            _ = processed.Append("\nOR");
                            i += orMatch.Length;
                        }
                        else
                        {
                            _ = processed.Append(afterWhere[i]);
                            i++;
                        }
                    }
                    else
                    {
                        _ = processed.Append(afterWhere[i]);
                        i++;
                    }
                }

                result = beforeWhere + processed.ToString();
            }

            return result;
        }
        catch (Exception)
        {
            return "Error formatting SQL.";
        }
    }
}
