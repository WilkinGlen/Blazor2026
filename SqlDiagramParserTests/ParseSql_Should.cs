namespace SqlDiagramParserTests;

using Blazor2026.Components.Controls.SqlDiagram.Services;
using FluentAssertions;

public class ParseSql_Should
{
    [Fact]
    public void ReturnEmptyData_WhenSqlIsNull()
    {
        string? sql = null;

        var result = SqlDiagramParser.ParseSql(sql!);

        _ = result.Should().NotBeNull();
        _ = result.Tables.Should().BeEmpty();
        _ = result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void ReturnEmptyData_WhenSqlIsEmpty()
    {
        var sql = string.Empty;

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Should().NotBeNull();
        _ = result.Tables.Should().BeEmpty();
        _ = result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void ReturnEmptyData_WhenSqlIsWhitespace()
    {
        var sql = "   \t\n  ";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Should().NotBeNull();
        _ = result.Tables.Should().BeEmpty();
        _ = result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void ParseFromTable_WithoutAlias()
    {
        var sql = "SELECT * FROM Customers";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(1);
        _ = result.Tables[0].Name.Should().Be("Customers");
        _ = result.Tables[0].Alias.Should().Be("Customers");
        _ = result.Tables[0].X.Should().Be(50);
        _ = result.Tables[0].Y.Should().Be(50);
    }

    [Fact]
    public void ParseFromTable_WithAlias()
    {
        var sql = "SELECT * FROM Customers AS c";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(1);
        _ = result.Tables[0].Name.Should().Be("Customers");
        _ = result.Tables[0].Alias.Should().Be("c");
    }

    [Fact]
    public void ParseFromTable_WithSquareBrackets()
    {
        var sql = "SELECT * FROM [Customers]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(1);
        _ = result.Tables[0].Name.Should().Be("Customers");
    }

    [Fact]
    public void ParseFromTable_WithSchemaAndSquareBrackets()
    {
        var sql = "SELECT * FROM [dbo].[Customers]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(1);
        _ = result.Tables[0].Name.Should().Be("Customers");
    }

    [Fact]
    public void ParseInnerJoin_WithExplicitKeyword()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(2);
        _ = result.Tables[0].Alias.Should().Be("o");
        _ = result.Tables[1].Name.Should().Be("Customers");
        _ = result.Tables[1].Alias.Should().Be("c");

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].JoinType.Should().Be("INNER");
        _ = result.Joins[0].FromTable.Should().Be("o");
        _ = result.Joins[0].FromColumn.Should().Be("CustomerId");
        _ = result.Joins[0].ToTable.Should().Be("c");
        _ = result.Joins[0].ToColumn.Should().Be("Id");
    }

    [Fact]
    public void ParseJoin_WithoutJoinTypeKeyword_DefaultsToInner()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].JoinType.Should().Be("INNER");
    }

    [Fact]
    public void ParseLeftJoin()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    LEFT JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].JoinType.Should().Be("LEFT");
    }

    [Fact]
    public void ParseRightJoin()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    RIGHT JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].JoinType.Should().Be("RIGHT");
    }

    [Fact]
    public void ParseFullJoin()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    FULL JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].JoinType.Should().Be("FULL");
    }

    [Fact]
    public void ParseCrossJoin()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    CROSS JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].JoinType.Should().Be("CROSS");
    }

    [Fact]
    public void ParseMultipleJoins()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id
                    LEFT JOIN Products AS p ON o.ProductId = p.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(3);
        _ = result.Joins.Should().HaveCount(2);

        _ = result.Joins[0].JoinType.Should().Be("INNER");
        _ = result.Joins[0].FromTable.Should().Be("o");
        _ = result.Joins[0].ToTable.Should().Be("c");

        _ = result.Joins[1].JoinType.Should().Be("LEFT");
        _ = result.Joins[1].FromTable.Should().Be("o");
        _ = result.Joins[1].ToTable.Should().Be("p");
    }

    [Fact]
    public void PositionTables_InCascadingLayout()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id
                    LEFT JOIN Products AS p ON o.ProductId = p.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables[0].X.Should().Be(50);
        _ = result.Tables[0].Y.Should().Be(50);

        _ = result.Tables[1].X.Should().Be(200); // 50 + 1 * 150
        _ = result.Tables[1].Y.Should().Be(90);  // 50 + 1 * 40

        _ = result.Tables[2].X.Should().Be(350); // 50 + 2 * 150
        _ = result.Tables[2].Y.Should().Be(130); // 50 + 2 * 40
    }

    [Fact]
    public void ParseSelectColumns_WithQualifiedNames()
    {
        var sql = @"SELECT o.OrderId, c.CustomerName 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "OrderId");
        _ = ordersTable.SelectedColumns.Should().Contain("OrderId");

        var customersTable = result.Tables.First(t => t.Alias == "c");
        _ = customersTable.Columns.Should().Contain(c => c.Name == "CustomerName");
        _ = customersTable.SelectedColumns.Should().Contain("CustomerName");
    }

    [Fact]
    public void ParseSelectColumns_WithAliases()
    {
        var sql = @"SELECT o.OrderId AS OrderNumber, c.CustomerName AS Client 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        var orderColumn = ordersTable.Columns.First(c => c.Name == "OrderId");
        _ = orderColumn.Alias.Should().Be("OrderNumber");

        var customersTable = result.Tables.First(t => t.Alias == "c");
        var customerColumn = customersTable.Columns.First(c => c.Name == "CustomerName");
        _ = customerColumn.Alias.Should().Be("Client");
    }

    [Fact]
    public void ParseSelectColumns_WithAliasesWithoutAS()
    {
        var sql = @"SELECT o.OrderId OrderNumber, c.CustomerName Client 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        var orderColumn = ordersTable.Columns.First(c => c.Name == "OrderId");
        _ = orderColumn.Alias.Should().Be("OrderNumber");
    }

    [Fact]
    public void ParseSelectColumns_WithSquareBrackets()
    {
        var sql = @"SELECT o.[OrderId], c.[CustomerName] 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "OrderId");

        var customersTable = result.Tables.First(t => t.Alias == "c");
        _ = customersTable.Columns.Should().Contain(c => c.Name == "CustomerName");
    }

    [Fact]
    public void AddJoinColumns_ToTablesAutomatically()
    {
        var sql = @"SELECT o.OrderId 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "CustomerId");
        _ = ordersTable.SelectedColumns.Should().NotContain("CustomerId");

        var customersTable = result.Tables.First(t => t.Alias == "c");
        _ = customersTable.Columns.Should().Contain(c => c.Name == "Id");
        _ = customersTable.SelectedColumns.Should().NotContain("Id");
    }

    [Fact]
    public void AddDefaultColumn_ForTablesWithNoColumns()
    {
        var sql = "SELECT * FROM Customers";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables[0].Columns.Should().HaveCount(1);
        _ = result.Tables[0].Columns[0].Name.Should().Be("CustomersId");
    }

    [Fact]
    public void NotDuplicateColumns_WhenColumnAppearsMultipleTimes()
    {
        var sql = @"SELECT o.OrderId, o.OrderId 
                    FROM Orders AS o";

        var result = SqlDiagramParser.ParseSql(sql);

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        _ = ordersTable.Columns.Should().HaveCount(1);
        _ = ordersTable.SelectedColumns.Should().HaveCount(1);
    }

    [Fact]
    public void HandleCaseInsensitiveTableAliases()
    {
        var sql = @"SELECT O.OrderId, C.CustomerName 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        var ordersTable = result.Tables.First(t => t.Alias.Equals("o", StringComparison.OrdinalIgnoreCase));
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "OrderId");

        var customersTable = result.Tables.First(t => t.Alias.Equals("c", StringComparison.OrdinalIgnoreCase));
        _ = customersTable.Columns.Should().Contain(c => c.Name == "CustomerName");
    }

    [Fact]
    public void HandleMultilineQuery()
    {
        var sql = @"
            SELECT 
                o.OrderId AS OrderNumber, 
                o.OrderDate AS DateOrdered, 
                c.CustomerName AS Client
            FROM 
                Orders AS o 
            INNER JOIN 
                Customers AS c 
            ON 
                o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(2);
        _ = result.Joins.Should().HaveCount(1);

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        _ = ordersTable.Columns.Should().HaveCount(3);
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "OrderId" && c.Alias == "OrderNumber");
    }

    [Fact]
    public void HandleComplexQueryWithMultipleJoinsAndColumns()
    {
        var sql = @"
            SELECT 
                o.OrderId AS OrderNumber, 
                o.OrderDate AS DateOrdered, 
                c.CustomerName AS Client, 
                p.ProductName AS Item, 
                od.Quantity AS Qty, 
                od.Price AS UnitPrice
            FROM 
                Orders AS o 
            INNER JOIN 
                Customers AS c 
            ON 
                o.CustomerId = c.Id
            INNER JOIN 
                OrderDetails AS od 
            ON 
                o.OrderId = od.OrderId
            LEFT JOIN 
                Products AS p 
            ON 
                od.ProductId = p.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(4);
        _ = result.Tables.Should().Contain(t => t.Alias == "o");
        _ = result.Tables.Should().Contain(t => t.Alias == "c");
        _ = result.Tables.Should().Contain(t => t.Alias == "od");
        _ = result.Tables.Should().Contain(t => t.Alias == "p");

        _ = result.Joins.Should().HaveCount(3);
        _ = result.Joins[0].JoinType.Should().Be("INNER");
        _ = result.Joins[1].JoinType.Should().Be("INNER");
        _ = result.Joins[2].JoinType.Should().Be("LEFT");

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "OrderId");
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "OrderDate");
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "CustomerId");

        var productsTable = result.Tables.First(t => t.Alias == "p");
        _ = productsTable.Columns.Should().Contain(c => c.Name == "ProductName");
        _ = productsTable.Columns.Should().Contain(c => c.Name == "Id");
    }

    [Fact]
    public void ReturnEmptyData_WhenSqlIsMalformed()
    {
        var sql = "This is not a valid SQL query";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Should().NotBeNull();
        _ = result.Tables.Should().BeEmpty();
        _ = result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void HandleJoinWithSquareBracketsInColumnNames()
    {
        var sql = @"SELECT * FROM [Orders] AS o 
                    INNER JOIN [Customers] AS c ON o.[CustomerId] = c.[Id]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(2);
        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].FromColumn.Should().Be("CustomerId");
        _ = result.Joins[0].ToColumn.Should().Be("Id");
    }

    [Fact]
    public void NotAddDuplicateTables_WhenTableAppearsMultipleTimes()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id
                    LEFT JOIN Orders AS o2 ON o.ParentOrderId = o2.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(3);
        _ = result.Tables.Should().Contain(t => t.Alias == "o");
        _ = result.Tables.Should().Contain(t => t.Alias == "c");
        _ = result.Tables.Should().Contain(t => t.Alias == "o2");
    }

    [Fact]
    public void ParseLeftOuterJoin()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    LEFT OUTER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].JoinType.Should().Be("LEFT");
    }

    [Fact]
    public void ParseRightOuterJoin()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    RIGHT OUTER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].JoinType.Should().Be("RIGHT");
    }

    [Fact]
    public void ParseFullOuterJoin()
    {
        var sql = @"SELECT * FROM Orders AS o 
                    FULL OUTER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].JoinType.Should().Be("FULL");
    }

    [Fact]
    public void ParseMultipleColumnsFromSameTable()
    {
        var sql = @"SELECT o.OrderId, o.OrderDate, o.TotalAmount, c.CustomerName 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        _ = ordersTable.Columns.Should().HaveCount(4);
        _ = ordersTable.SelectedColumns.Should().HaveCount(3);
    }

    [Fact]
    public void ParseJoinWithSchemaQualifiedTables()
    {
        var sql = @"SELECT * FROM [dbo].[Orders] AS o 
                    INNER JOIN [dbo].[Customers] AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(2);
        _ = result.Tables[0].Name.Should().Be("Orders");
        _ = result.Tables[1].Name.Should().Be("Customers");
    }

    [Fact]
    public void ParseColumnsWithUnderscores()
    {
        var sql = @"SELECT o.order_id, o.created_date, c.customer_name 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.customer_id = c.id";

        var result = SqlDiagramParser.ParseSql(sql);

        var ordersTable = result.Tables.First(t => t.Alias == "o");
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "order_id");
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "created_date");
        _ = ordersTable.Columns.Should().Contain(c => c.Name == "customer_id");
    }

    [Fact]
    public void ParseWithExtraWhitespace()
    {
        var sql = @"SELECT    o.OrderId   ,   c.CustomerName   
                    FROM    Orders   AS   o   
                    INNER   JOIN   Customers   AS   c   ON   o.CustomerId   =   c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(2);
        _ = result.Joins.Should().HaveCount(1);
    }

    [Fact]
    public void ParseTablesWithNumbers()
    {
        var sql = @"SELECT o1.OrderId, o2.ParentOrderId 
                    FROM Orders2023 AS o1 
                    INNER JOIN Orders2022 AS o2 ON o1.ParentId = o2.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(2);
        _ = result.Tables.Should().Contain(t => t.Name == "Orders2023");
        _ = result.Tables.Should().Contain(t => t.Name == "Orders2022");
    }

    [Fact]
    public void ParseQueryWithOnlySelectKeyword()
    {
        var sql = "SELECT";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Should().NotBeNull();
        _ = result.Tables.Should().BeEmpty();
        _ = result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void ParseSelfJoinWithMultipleInstances()
    {
        var sql = @"SELECT * FROM Employees AS e1 
                    INNER JOIN Employees AS e2 ON e1.ManagerId = e2.Id
                    LEFT JOIN Employees AS e3 ON e2.ManagerId = e3.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(3);
        _ = result.Tables.Should().OnlyContain(t => t.Name == "Employees");
        _ = result.Tables.Select(t => t.Alias).Should().BeEquivalentTo(["e1", "e2", "e3"]);
    }

    [Fact]
    public void ParseSelectWithFunctions()
    {
        var sql = @"SELECT COUNT(o.OrderId), MAX(o.Amount), c.CustomerName 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(2);
        _ = result.Joins.Should().HaveCount(1);

        var customersTable = result.Tables.First(t => t.Alias == "c");
        _ = customersTable.Columns.Should().Contain(c => c.Name == "CustomerName");
    }
}
