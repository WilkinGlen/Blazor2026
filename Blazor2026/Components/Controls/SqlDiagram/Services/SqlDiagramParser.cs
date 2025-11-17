namespace Blazor2026.Components.Controls.SqlDiagram.Services;

using System.Text.RegularExpressions;
using Blazor2026.Models;

public static partial class SqlDiagramParser
{
    // Matches FROM clause with optional schema/database/server prefixes
    // Captures only the actual table name (the last identifier in the chain)
    // Supports both simple aliases and multi-part bracketed aliases
    [GeneratedRegex(@"\bFROM\s+(?:(?:\[?[\w]+\]?\.(?:\[?[\w]+\]?\.)?)*)?\[?(\w+)\]?(?:\s+AS\s+(?:\[([^\]]+)\]|(\w+)))?", RegexOptions.IgnoreCase)]
    private static partial Regex FromTableRegex();

    // Matches JOIN clause with optional OUTER keyword and schema/database/server prefixes
    // Supports: INNER JOIN, LEFT JOIN, LEFT OUTER JOIN, RIGHT JOIN, RIGHT OUTER JOIN, FULL JOIN, FULL OUTER JOIN, CROSS JOIN
    // Supports square brackets and multi-part identifiers in table aliases and ON clause
    [GeneratedRegex(@"\b(LEFT|RIGHT|INNER|FULL|CROSS)?(?:\s+OUTER)?\s*JOIN\s+(?:(?:\[?[\w]+\]?\.(?:\[?[\w]+\]?\.)?)*)?\[?(\w+)\]?(?:\s+AS\s+(?:\[([^\]]+)\]|(\w+)))?\s+ON\s+(?:\[([^\]]+)\]|(\w+))\s*\.\s*\[?(\w+)\]?\s*=\s*(?:\[([^\]]+)\]|(\w+))\s*\.\s*\[?(\w+)\]?", RegexOptions.IgnoreCase)]
    private static partial Regex JoinRegex();

    [GeneratedRegex(@"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SelectColumnsRegex();

    // Updated regex to properly handle both bracketed and non-bracketed identifiers in SELECT columns
    // Matches: [table.alias].[column] AS [column alias]  (bracketed)
    // Or: table.column AS alias  (non-bracketed)
    // Or: [table].[column] AS alias  (mixed)
    [GeneratedRegex(@"(?:\[([^\]]+)\]|(\w+))\s*\.\s*(?:\[([^\]]+)\]|(\w+))(?:\s+(?:AS\s+)?(?:\[([^\]]+)\]|(\w+)))?", RegexOptions.IgnoreCase)]
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
                // Group 2: bracketed alias, Group 3: non-bracketed alias
                var alias = !string.IsNullOrEmpty(fromMatch.Groups[2].Value) ? fromMatch.Groups[2].Value : 
                           (!string.IsNullOrEmpty(fromMatch.Groups[3].Value) ? fromMatch.Groups[3].Value : tableName);

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
                // Group 3: bracketed alias, Group 4: non-bracketed alias
                var alias = !string.IsNullOrEmpty(joinMatch.Groups[3].Value) ? joinMatch.Groups[3].Value : 
                           (!string.IsNullOrEmpty(joinMatch.Groups[4].Value) ? joinMatch.Groups[4].Value : tableName);
                // Group 5: bracketed left table, Group 6: non-bracketed left table
                var leftTableAlias = !string.IsNullOrEmpty(joinMatch.Groups[5].Value) ? joinMatch.Groups[5].Value : joinMatch.Groups[6].Value;
                var leftColumn = joinMatch.Groups[7].Value;
                // Group 8: bracketed right table, Group 9: non-bracketed right table
                var rightTableAlias = !string.IsNullOrEmpty(joinMatch.Groups[8].Value) ? joinMatch.Groups[8].Value : joinMatch.Groups[9].Value;
                var rightColumn = joinMatch.Groups[10].Value;

                if (!data.Tables.Any(t => t.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase)))
                {
                    var tableIndex = data.Tables.Count;

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
                    // Group 1: bracketed table alias, Group 2: non-bracketed table alias
                    var tableAlias = !string.IsNullOrEmpty(colMatch.Groups[1].Value) ? colMatch.Groups[1].Value : colMatch.Groups[2].Value;
                    // Group 3: bracketed column name, Group 4: non-bracketed column name
                    var columnName = !string.IsNullOrEmpty(colMatch.Groups[3].Value) ? colMatch.Groups[3].Value : colMatch.Groups[4].Value;
                    // Group 5: bracketed column alias, Group 6: non-bracketed column alias
                    var columnAlias = !string.IsNullOrEmpty(colMatch.Groups[5].Value) ? colMatch.Groups[5].Value : 
                                     (!string.IsNullOrEmpty(colMatch.Groups[6].Value) ? colMatch.Groups[6].Value : null);

                    var table = data.Tables.FirstOrDefault(t =>
                        t.Alias.Equals(tableAlias, StringComparison.OrdinalIgnoreCase));

                    if (table != null)
                    {
                        if (!table.Columns.Any(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                        {
                            table.Columns.Add(new ColumnDetails
                            {
                                Name = columnName,
                                Alias = columnAlias
                            });
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

                if (fromTable != null && !fromTable.Columns.Any(c => c.Name.Equals(join.FromColumn, StringComparison.OrdinalIgnoreCase)))
                {
                    fromTable.Columns.Add(new ColumnDetails { Name = join.FromColumn });
                }

                if (toTable != null && !toTable.Columns.Any(c => c.Name.Equals(join.ToColumn, StringComparison.OrdinalIgnoreCase)))
                {
                    toTable.Columns.Add(new ColumnDetails { Name = join.ToColumn });
                }
            }

            foreach (var table in data.Tables.Where(t => t.Columns.Count == 0))
            {
                table.Columns.Add(new ColumnDetails { Name = $"{table.Name}Id" });
            }
        }
        catch
        {
            return new SqlDiagramData();
        }

        return data;
    }
}
