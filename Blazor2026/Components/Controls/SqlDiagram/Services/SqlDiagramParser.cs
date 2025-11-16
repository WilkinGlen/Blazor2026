namespace Blazor2026.Components.Controls.SqlDiagram.Services;

using System.Text.RegularExpressions;
using Blazor2026.Models;

public static partial class SqlDiagramParser
{
    [GeneratedRegex(@"\bFROM\s+(?:\[?[\w]+\]?\.\[?[\w]+\]?\.)?\[?(\w+)\]?(?:\s+AS\s+(\w+))?", RegexOptions.IgnoreCase)]
    private static partial Regex FromTableRegex();

    [GeneratedRegex(@"\b(LEFT|RIGHT|INNER|FULL|CROSS)?\s*JOIN\s+(?:\[?[\w]+\]?\.\[?[\w]+\]?\.)?\[?(\w+)\]?(?:\s+AS\s+(\w+))?\s+ON\s+(\w+)\s*\.\s*(\w+)\s*=\s*(\w+)\s*\.\s*(\w+)", RegexOptions.IgnoreCase)]
    private static partial Regex JoinRegex();

    [GeneratedRegex(@"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SelectColumnsRegex();

    [GeneratedRegex(@"(\w+)\s*\.\s*\[?(\w+)\]?", RegexOptions.IgnoreCase)]
    private static partial Regex QualifiedColumnRegex();

    public static SqlDiagramData ParseSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return new SqlDiagramData();
        }

        var data = new SqlDiagramData();
        var tableAliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var fromMatch = FromTableRegex().Match(sql);
            if (fromMatch.Success)
            {
                var tableName = fromMatch.Groups[1].Value;
                var alias = fromMatch.Groups[2].Success ? fromMatch.Groups[2].Value : tableName;

                data.Tables.Add(new TableInfo
                {
                    Name = tableName,
                    Alias = alias,
                    X = 50,
                    Y = 50
                });
                tableAliasMap[alias] = tableName;
            }

            var joinMatches = JoinRegex().Matches(sql);

            foreach (Match joinMatch in joinMatches)
            {
                var joinType = joinMatch.Groups[1].Success ? joinMatch.Groups[1].Value : "INNER";
                var tableName = joinMatch.Groups[2].Value;
                var alias = joinMatch.Groups[3].Success ? joinMatch.Groups[3].Value : tableName;
                var leftTableAlias = joinMatch.Groups[4].Value;
                var leftColumn = joinMatch.Groups[5].Value;
                var rightTableAlias = joinMatch.Groups[6].Value;
                var rightColumn = joinMatch.Groups[7].Value;

                if (!data.Tables.Any(t => t.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase)))
                {
                    // Cascade tables with headers visible
                    // Horizontal offset: 150px (50% of typical table width)
                    // Vertical offset: 40px (approximate header height) so next table starts at bottom of previous header
                    int tableIndex = data.Tables.Count;
                    
                    data.Tables.Add(new TableInfo
                    {
                        Name = tableName,
                        Alias = alias,
                        X = 50 + tableIndex * 150,
                        Y = 50 + tableIndex * 40
                    });
                    tableAliasMap[alias] = tableName;
                }

                data.Joins.Add(new JoinRelationship
                {
                    FromTable = leftTableAlias,
                    ToTable = rightTableAlias,
                    FromColumn = leftColumn,
                    ToColumn = rightColumn,
                    JoinType = joinType
                });
            }

            var selectMatch = SelectColumnsRegex().Match(sql);
            if (selectMatch.Success)
            {
                var columnsText = selectMatch.Groups[1].Value;
                var qualifiedMatches = QualifiedColumnRegex().Matches(columnsText);
                foreach (Match colMatch in qualifiedMatches)
                {
                    var tableAlias = colMatch.Groups[1].Value;
                    var columnName = colMatch.Groups[2].Value;
                    var table = data.Tables.FirstOrDefault(t =>
                        t.Alias.Equals(tableAlias, StringComparison.OrdinalIgnoreCase));

                    if (table != null)
                    {
                        if (!table.Columns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                        {
                            table.Columns.Add(columnName);
                        }

                        _ = table.SelectedColumns.Add(columnName);
                    }
                }
            }

            foreach (var join in data.Joins)
            {
                var fromTable = data.Tables.FirstOrDefault(t =>
                    t.Alias.Equals(join.FromTable, StringComparison.OrdinalIgnoreCase));
                var toTable = data.Tables.FirstOrDefault(t =>
                    t.Alias.Equals(join.ToTable, StringComparison.OrdinalIgnoreCase));

                if (fromTable != null && !fromTable.Columns.Contains(join.FromColumn, StringComparer.OrdinalIgnoreCase))
                {
                    fromTable.Columns.Add(join.FromColumn);
                }

                if (toTable != null && !toTable.Columns.Contains(join.ToColumn, StringComparer.OrdinalIgnoreCase))
                {
                    toTable.Columns.Add(join.ToColumn);
                }
            }

            foreach (var table in data.Tables.Where(t => t.Columns.Count == 0))
            {
                table.Columns.Add($"{table.Name}Id");
            }
        }
        catch
        {
            return new SqlDiagramData();
        }

        return data;
    }
}
