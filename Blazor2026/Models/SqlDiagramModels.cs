namespace Blazor2026.Models;

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Alias { get; set; }
    
    public string DisplayName => !string.IsNullOrWhiteSpace(this.Alias) && this.Alias != this.Name
        ? $"{this.Alias} ({this.Name})"
        : this.Name;
}

public class TableInfo
{
    public string Name { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public List<ColumnInfo> Columns { get; set; } = [];
    public HashSet<string> SelectedColumns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public double X { get; set; }
    public double Y { get; set; }
}

public class JoinRelationship
{
    public string FromTable { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string FromColumn { get; set; } = string.Empty;
    public string ToColumn { get; set; } = string.Empty;
    public string JoinType { get; set; } = "INNER";
}

public class SqlDiagramData
{
    public List<TableInfo> Tables { get; set; } = [];
    public List<JoinRelationship> Joins { get; set; } = [];
}
