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

    [Fact]
    public void ParseMultiPartBracketedTableAliases()
    {
        var sql = @"SELECT 
                        [Database.Schema.Table1].[Column1] AS [Col1],
                        [Database.Schema.Table2].[Column2] AS [Col2]
                    FROM [Database].[Schema].[Table1] AS [Database.Schema.Table1]
                    INNER JOIN [Database].[Schema].[Table2] AS [Database.Schema.Table2]
                        ON [Database.Schema.Table1].[Id] = [Database.Schema.Table2].[Table1Id]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(2);
        _ = result.Tables[0].Name.Should().Be("Table1");
        _ = result.Tables[0].Alias.Should().Be("Database.Schema.Table1");
        _ = result.Tables[1].Name.Should().Be("Table2");
        _ = result.Tables[1].Alias.Should().Be("Database.Schema.Table2");

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].FromTable.Should().Be("Database.Schema.Table1");
        _ = result.Joins[0].ToTable.Should().Be("Database.Schema.Table2");

        var table1 = result.Tables.First(t => t.Alias == "Database.Schema.Table1");
        _ = table1.Columns.Should().Contain(c => c.Name == "Column1" && c.Alias == "Col1");
        _ = table1.Columns.Should().Contain(c => c.Name == "Id");

        var table2 = result.Tables.First(t => t.Alias == "Database.Schema.Table2");
        _ = table2.Columns.Should().Contain(c => c.Name == "Column2" && c.Alias == "Col2");
        _ = table2.Columns.Should().Contain(c => c.Name == "Table1Id");
    }

    [Fact]
    public void ParseBracketedColumnsWithSpacesInAliases()
    {
        var sql = @"SELECT 
                        [ApiSelfService.dbo.PrototypeApis].[Name] AS [PrototypeApis Name],
                        [ApiSelfService.dbo.PrototypeApis].[Description] AS [PrototypeApis Description]
                    FROM [ApiSelfService].[dbo].[PrototypeApis] AS [ApiSelfService.dbo.PrototypeApis]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(1);
        _ = result.Tables[0].Name.Should().Be("PrototypeApis");
        _ = result.Tables[0].Alias.Should().Be("ApiSelfService.dbo.PrototypeApis");

        var table = result.Tables[0];
        _ = table.Columns.Should().HaveCount(2);
        _ = table.Columns.Should().Contain(c => c.Name == "Name" && c.Alias == "PrototypeApis Name");
        _ = table.Columns.Should().Contain(c => c.Name == "Description" && c.Alias == "PrototypeApis Description");
        _ = table.SelectedColumns.Should().Contain("Name");
        _ = table.SelectedColumns.Should().Contain("Description");
    }

    [Fact]
    public void ParseComplexMultiPartAliasQuery()
    {
        var sql = @"SELECT 
                        [ApiSelfService.dbo.PrototypeApis].[Name] AS [PrototypeApis Name],
                        [ApiSelfService.dbo.PrototypeApis].[Description] AS [PrototypeApis Description],
                        [ApiSelfService.dbo.PrototypeNotes].[Note] AS [PrototypeNotes Note],
                        [CacheCore.dbo.RdrDetails].[rdr_wdw_lgon_id] AS [RdrDetails StandardId]
                    FROM [ApiSelfService].[dbo].[PrototypeApis] AS [ApiSelfService.dbo.PrototypeApis]
                    LEFT JOIN [ApiSelfService].[dbo].[PrototypeNotes] AS [ApiSelfService.dbo.PrototypeNotes]
                        ON [ApiSelfService.dbo.PrototypeApis].[Id] = [ApiSelfService.dbo.PrototypeNotes].[PrototypeApiId]
                    LEFT JOIN [CacheCore].[dbo].[RdrDetails] AS [CacheCore.dbo.RdrDetails]
                        ON [CacheCore.dbo.RdrDetails].[rdr_wdw_lgon_id] = [ApiSelfService.dbo.PrototypeNotes].[CreatedBy]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(3);
        _ = result.Tables.Should().Contain(t => t.Alias == "ApiSelfService.dbo.PrototypeApis");
        _ = result.Tables.Should().Contain(t => t.Alias == "ApiSelfService.dbo.PrototypeNotes");
        _ = result.Tables.Should().Contain(t => t.Alias == "CacheCore.dbo.RdrDetails");

        _ = result.Joins.Should().HaveCount(2);
        _ = result.Joins[0].JoinType.Should().Be("LEFT");
        _ = result.Joins[0].FromTable.Should().Be("ApiSelfService.dbo.PrototypeApis");
        _ = result.Joins[0].FromColumn.Should().Be("Id");
        _ = result.Joins[0].ToTable.Should().Be("ApiSelfService.dbo.PrototypeNotes");
        _ = result.Joins[0].ToColumn.Should().Be("PrototypeApiId");

        _ = result.Joins[1].JoinType.Should().Be("LEFT");
        _ = result.Joins[1].FromTable.Should().Be("CacheCore.dbo.RdrDetails");
        _ = result.Joins[1].FromColumn.Should().Be("rdr_wdw_lgon_id");
        _ = result.Joins[1].ToTable.Should().Be("ApiSelfService.dbo.PrototypeNotes");
        _ = result.Joins[1].ToColumn.Should().Be("CreatedBy");

        var prototypeApisTable = result.Tables.First(t => t.Alias == "ApiSelfService.dbo.PrototypeApis");
        _ = prototypeApisTable.Columns.Should().Contain(c => c.Name == "Name" && c.Alias == "PrototypeApis Name");
        _ = prototypeApisTable.Columns.Should().Contain(c => c.Name == "Description" && c.Alias == "PrototypeApis Description");
        _ = prototypeApisTable.Columns.Should().Contain(c => c.Name == "Id");

        var prototypeNotesTable = result.Tables.First(t => t.Alias == "ApiSelfService.dbo.PrototypeNotes");
        _ = prototypeNotesTable.Columns.Should().Contain(c => c.Name == "Note" && c.Alias == "PrototypeNotes Note");
        _ = prototypeNotesTable.Columns.Should().Contain(c => c.Name == "PrototypeApiId");
        _ = prototypeNotesTable.Columns.Should().Contain(c => c.Name == "CreatedBy");

        var rdrDetailsTable = result.Tables.First(t => t.Alias == "CacheCore.dbo.RdrDetails");
        _ = rdrDetailsTable.Columns.Should().Contain(c => c.Name == "rdr_wdw_lgon_id" && c.Alias == "RdrDetails StandardId");
    }

    [Fact]
    public void ParseBracketedColumnsWithUnderscores()
    {
        var sql = @"SELECT 
                        [Schema.Table].[column_with_underscores] AS [Column Alias],
                        [Schema.Table].[another_column] AS [Another Column]
                    FROM [Database].[Schema].[Table] AS [Schema.Table]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(1);
        var table = result.Tables[0];
        _ = table.Columns.Should().Contain(c => c.Name == "column_with_underscores" && c.Alias == "Column Alias");
        _ = table.Columns.Should().Contain(c => c.Name == "another_column" && c.Alias == "Another Column");
    }

    [Fact]
    public void ParseMixedBracketedAndNonBracketedAliases()
    {
        var sql = @"SELECT 
                        [Schema.Table1].[Column1] AS [Col 1],
                        t2.Column2 AS Col2
                    FROM [Database].[Schema].[Table1] AS [Schema.Table1]
                    INNER JOIN [Database].[Schema].[Table2] AS t2
                        ON [Schema.Table1].[Id] = t2.[Table1Id]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(2);
        _ = result.Tables[0].Alias.Should().Be("Schema.Table1");
        _ = result.Tables[1].Alias.Should().Be("t2");

        var table1 = result.Tables.First(t => t.Alias == "Schema.Table1");
        _ = table1.Columns.Should().Contain(c => c.Name == "Column1" && c.Alias == "Col 1");
    }

    [Fact]
    public void ParseDuplicateColumnsInBracketedFormat()
    {
        var sql = @"SELECT 
                        [Schema.Table].[Column1] AS [Alias1],
                        [Schema.Table].[Column1] AS [Alias2]
                    FROM [Database].[Schema].[Table] AS [Schema.Table]";

        var result = SqlDiagramParser.ParseSql(sql);

        var table = result.Tables[0];
        _ = table.Columns.Should().HaveCount(1);
        _ = table.SelectedColumns.Should().HaveCount(1);
        _ = table.Columns[0].Name.Should().Be("Column1");
    }

    [Fact]
    public void ParseBracketedAliasesInJoinConditions()
    {
        var sql = @"SELECT *
                    FROM [DB1.Schema1.Table1] AS [DB1.Schema1.Table1]
                    INNER JOIN [DB2.Schema2.Table2] AS [DB2.Schema2.Table2]
                        ON [DB1.Schema1.Table1].[JoinKey] = [DB2.Schema2.Table2].[JoinKey]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Joins.Should().HaveCount(1);
        _ = result.Joins[0].FromTable.Should().Be("DB1.Schema1.Table1");
        _ = result.Joins[0].ToTable.Should().Be("DB2.Schema2.Table2");
        _ = result.Joins[0].FromColumn.Should().Be("JoinKey");
        _ = result.Joins[0].ToColumn.Should().Be("JoinKey");
    }

    [Fact]
    public void ParseMultipleBracketedColumnsFromSameTable()
    {
        var sql = @"SELECT 
                        [Api.dbo.Table].[Col1] AS [Column 1],
                        [Api.dbo.Table].[Col2] AS [Column 2],
                        [Api.dbo.Table].[Col3] AS [Column 3],
                        [Api.dbo.Table].[Col4] AS [Column 4]
                    FROM [ApiDb].[dbo].[Table] AS [Api.dbo.Table]";

        var result = SqlDiagramParser.ParseSql(sql);

        var table = result.Tables[0];
        _ = table.Columns.Should().HaveCount(4);
        _ = table.SelectedColumns.Should().HaveCount(4);
        _ = table.Columns.Should().Contain(c => c.Name == "Col1" && c.Alias == "Column 1");
        _ = table.Columns.Should().Contain(c => c.Name == "Col2" && c.Alias == "Column 2");
        _ = table.Columns.Should().Contain(c => c.Name == "Col3" && c.Alias == "Column 3");
        _ = table.Columns.Should().Contain(c => c.Name == "Col4" && c.Alias == "Column 4");
    }

    [Fact]
    public void ParseBracketedAliasesWithMultipleJoins()
    {
        var sql = @"SELECT 
                        [T1].[Col1],
                        [T2].[Col2],
                        [T3].[Col3]
                    FROM [DB].[Schema].[Table1] AS [T1]
                    INNER JOIN [DB].[Schema].[Table2] AS [T2]
                        ON [T1].[Id] = [T2].[T1Id]
                    LEFT JOIN [DB].[Schema].[Table3] AS [T3]
                        ON [T2].[Id] = [T3].[T2Id]";

        var result = SqlDiagramParser.ParseSql(sql);

        _ = result.Tables.Should().HaveCount(3);
        _ = result.Joins.Should().HaveCount(2);

        _ = result.Joins[0].FromTable.Should().Be("T1");
        _ = result.Joins[0].ToTable.Should().Be("T2");
        _ = result.Joins[1].FromTable.Should().Be("T2");
        _ = result.Joins[1].ToTable.Should().Be("T3");
    }
}
