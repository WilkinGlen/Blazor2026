namespace SqlDiagramParserTests;

using Blazor2026.Components.Controls.SqlDiagram.Services;
using Blazor2026.Models;
using FluentAssertions;

public class ParseSql_Should
{
    [Fact]
    public void ReturnEmptyData_WhenSqlIsNull()
    {
        // Arrange
        string? sql = null;

        // Act
        var result = SqlDiagramParser.ParseSql(sql!);

        // Assert
        result.Should().NotBeNull();
        result.Tables.Should().BeEmpty();
        result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void ReturnEmptyData_WhenSqlIsEmpty()
    {
        // Arrange
        var sql = string.Empty;

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Should().NotBeNull();
        result.Tables.Should().BeEmpty();
        result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void ReturnEmptyData_WhenSqlIsWhitespace()
    {
        // Arrange
        var sql = "   \t\n  ";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Should().NotBeNull();
        result.Tables.Should().BeEmpty();
        result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void ParseFromTable_WithoutAlias()
    {
        // Arrange
        var sql = "SELECT * FROM Customers";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(1);
        result.Tables[0].Name.Should().Be("Customers");
        result.Tables[0].Alias.Should().Be("Customers");
        result.Tables[0].X.Should().Be(50);
        result.Tables[0].Y.Should().Be(50);
    }

    [Fact]
    public void ParseFromTable_WithAlias()
    {
        // Arrange
        var sql = "SELECT * FROM Customers AS c";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(1);
        result.Tables[0].Name.Should().Be("Customers");
        result.Tables[0].Alias.Should().Be("c");
    }

    [Fact]
    public void ParseFromTable_WithSquareBrackets()
    {
        // Arrange
        var sql = "SELECT * FROM [Customers]";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(1);
        result.Tables[0].Name.Should().Be("Customers");
    }

    [Fact]
    public void ParseFromTable_WithSchemaAndSquareBrackets()
    {
        // Arrange
        var sql = "SELECT * FROM [dbo].[Customers]";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(1);
        result.Tables[0].Name.Should().Be("Customers");
    }

    [Fact]
    public void ParseInnerJoin_WithExplicitKeyword()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(2);
        result.Tables[0].Alias.Should().Be("o");
        result.Tables[1].Name.Should().Be("Customers");
        result.Tables[1].Alias.Should().Be("c");
        
        result.Joins.Should().HaveCount(1);
        result.Joins[0].JoinType.Should().Be("INNER");
        result.Joins[0].FromTable.Should().Be("o");
        result.Joins[0].FromColumn.Should().Be("CustomerId");
        result.Joins[0].ToTable.Should().Be("c");
        result.Joins[0].ToColumn.Should().Be("Id");
    }

    [Fact]
    public void ParseJoin_WithoutJoinTypeKeyword_DefaultsToInner()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Joins.Should().HaveCount(1);
        result.Joins[0].JoinType.Should().Be("INNER");
    }

    [Fact]
    public void ParseLeftJoin()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    LEFT JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Joins.Should().HaveCount(1);
        result.Joins[0].JoinType.Should().Be("LEFT");
    }

    [Fact]
    public void ParseRightJoin()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    RIGHT JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Joins.Should().HaveCount(1);
        result.Joins[0].JoinType.Should().Be("RIGHT");
    }

    [Fact]
    public void ParseFullJoin()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    FULL JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Joins.Should().HaveCount(1);
        result.Joins[0].JoinType.Should().Be("FULL");
    }

    [Fact]
    public void ParseCrossJoin()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    CROSS JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Joins.Should().HaveCount(1);
        result.Joins[0].JoinType.Should().Be("CROSS");
    }

    [Fact]
    public void ParseMultipleJoins()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id
                    LEFT JOIN Products AS p ON o.ProductId = p.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(3);
        result.Joins.Should().HaveCount(2);
        
        result.Joins[0].JoinType.Should().Be("INNER");
        result.Joins[0].FromTable.Should().Be("o");
        result.Joins[0].ToTable.Should().Be("c");
        
        result.Joins[1].JoinType.Should().Be("LEFT");
        result.Joins[1].FromTable.Should().Be("o");
        result.Joins[1].ToTable.Should().Be("p");
    }

    [Fact]
    public void PositionTables_InCascadingLayout()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id
                    LEFT JOIN Products AS p ON o.ProductId = p.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables[0].X.Should().Be(50);
        result.Tables[0].Y.Should().Be(50);
        
        result.Tables[1].X.Should().Be(200); // 50 + 1 * 150
        result.Tables[1].Y.Should().Be(90);  // 50 + 1 * 40
        
        result.Tables[2].X.Should().Be(350); // 50 + 2 * 150
        result.Tables[2].Y.Should().Be(130); // 50 + 2 * 40
    }

    [Fact]
    public void ParseSelectColumns_WithQualifiedNames()
    {
        // Arrange
        var sql = @"SELECT o.OrderId, c.CustomerName 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        ordersTable.Columns.Should().Contain(c => c.Name == "OrderId");
        ordersTable.SelectedColumns.Should().Contain("OrderId");
        
        var customersTable = result.Tables.First(t => t.Alias == "c");
        customersTable.Columns.Should().Contain(c => c.Name == "CustomerName");
        customersTable.SelectedColumns.Should().Contain("CustomerName");
    }

    [Fact]
    public void ParseSelectColumns_WithAliases()
    {
        // Arrange
        var sql = @"SELECT o.OrderId AS OrderNumber, c.CustomerName AS Client 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        var orderColumn = ordersTable.Columns.First(c => c.Name == "OrderId");
        orderColumn.Alias.Should().Be("OrderNumber");
        
        var customersTable = result.Tables.First(t => t.Alias == "c");
        var customerColumn = customersTable.Columns.First(c => c.Name == "CustomerName");
        customerColumn.Alias.Should().Be("Client");
    }

    [Fact]
    public void ParseSelectColumns_WithAliasesWithoutAS()
    {
        // Arrange
        var sql = @"SELECT o.OrderId OrderNumber, c.CustomerName Client 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        var orderColumn = ordersTable.Columns.First(c => c.Name == "OrderId");
        orderColumn.Alias.Should().Be("OrderNumber");
    }

    [Fact]
    public void ParseSelectColumns_WithSquareBrackets()
    {
        // Arrange
        var sql = @"SELECT o.[OrderId], c.[CustomerName] 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        ordersTable.Columns.Should().Contain(c => c.Name == "OrderId");
        
        var customersTable = result.Tables.First(t => t.Alias == "c");
        customersTable.Columns.Should().Contain(c => c.Name == "CustomerName");
    }

    [Fact]
    public void AddJoinColumns_ToTablesAutomatically()
    {
        // Arrange
        var sql = @"SELECT o.OrderId 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        ordersTable.Columns.Should().Contain(c => c.Name == "CustomerId");
        ordersTable.SelectedColumns.Should().NotContain("CustomerId"); // Not in SELECT
        
        var customersTable = result.Tables.First(t => t.Alias == "c");
        customersTable.Columns.Should().Contain(c => c.Name == "Id");
        customersTable.SelectedColumns.Should().NotContain("Id"); // Not in SELECT
    }

    [Fact]
    public void AddDefaultColumn_ForTablesWithNoColumns()
    {
        // Arrange
        var sql = "SELECT * FROM Customers";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables[0].Columns.Should().HaveCount(1);
        result.Tables[0].Columns[0].Name.Should().Be("CustomersId");
    }

    [Fact]
    public void NotDuplicateColumns_WhenColumnAppearsMultipleTimes()
    {
        // Arrange
        var sql = @"SELECT o.OrderId, o.OrderId 
                    FROM Orders AS o";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        ordersTable.Columns.Should().HaveCount(1);
        ordersTable.SelectedColumns.Should().HaveCount(1);
    }

    [Fact]
    public void HandleCaseInsensitiveTableAliases()
    {
        // Arrange
        var sql = @"SELECT O.OrderId, C.CustomerName 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        var ordersTable = result.Tables.First(t => t.Alias.Equals("o", StringComparison.OrdinalIgnoreCase));
        ordersTable.Columns.Should().Contain(c => c.Name == "OrderId");
        
        var customersTable = result.Tables.First(t => t.Alias.Equals("c", StringComparison.OrdinalIgnoreCase));
        customersTable.Columns.Should().Contain(c => c.Name == "CustomerName");
    }

    [Fact]
    public void HandleMultilineQuery()
    {
        // Arrange
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

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(2);
        result.Joins.Should().HaveCount(1);
        
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        ordersTable.Columns.Should().HaveCount(3);
        ordersTable.Columns.Should().Contain(c => c.Name == "OrderId" && c.Alias == "OrderNumber");
    }

    [Fact]
    public void HandleComplexQueryWithMultipleJoinsAndColumns()
    {
        // Arrange
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

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(4);
        result.Tables.Should().Contain(t => t.Alias == "o");
        result.Tables.Should().Contain(t => t.Alias == "c");
        result.Tables.Should().Contain(t => t.Alias == "od");
        result.Tables.Should().Contain(t => t.Alias == "p");
        
        result.Joins.Should().HaveCount(3);
        result.Joins[0].JoinType.Should().Be("INNER");
        result.Joins[1].JoinType.Should().Be("INNER");
        result.Joins[2].JoinType.Should().Be("LEFT");
        
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        ordersTable.Columns.Should().Contain(c => c.Name == "OrderId");
        ordersTable.Columns.Should().Contain(c => c.Name == "OrderDate");
        ordersTable.Columns.Should().Contain(c => c.Name == "CustomerId");
        
        var productsTable = result.Tables.First(t => t.Alias == "p");
        productsTable.Columns.Should().Contain(c => c.Name == "ProductName");
        productsTable.Columns.Should().Contain(c => c.Name == "Id");
    }

    [Fact]
    public void ReturnEmptyData_WhenSqlIsMalformed()
    {
        // Arrange
        var sql = "This is not a valid SQL query";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Should().NotBeNull();
        result.Tables.Should().BeEmpty();
        result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void HandleJoinWithSquareBracketsInColumnNames()
    {
        // Arrange
        var sql = @"SELECT * FROM [Orders] AS o 
                    INNER JOIN [Customers] AS c ON o.[CustomerId] = c.[Id]";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(2);
        result.Joins.Should().HaveCount(1);
        result.Joins[0].FromColumn.Should().Be("CustomerId");
        result.Joins[0].ToColumn.Should().Be("Id");
    }

    [Fact]
    public void NotAddDuplicateTables_WhenTableAppearsMultipleTimes()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id
                    LEFT JOIN Orders AS o2 ON o.ParentOrderId = o2.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        // Should have Orders (o), Customers (c), and Orders (o2) - 3 distinct aliases
        result.Tables.Should().HaveCount(3);
        result.Tables.Should().Contain(t => t.Alias == "o");
        result.Tables.Should().Contain(t => t.Alias == "c");
        result.Tables.Should().Contain(t => t.Alias == "o2");
    }

    [Fact]
    public void ParseLeftOuterJoin()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    LEFT OUTER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Joins.Should().HaveCount(1);
        result.Joins[0].JoinType.Should().Be("LEFT");
    }

    [Fact]
    public void ParseRightOuterJoin()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    RIGHT OUTER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Joins.Should().HaveCount(1);
        result.Joins[0].JoinType.Should().Be("RIGHT");
    }

    [Fact]
    public void ParseFullOuterJoin()
    {
        // Arrange
        var sql = @"SELECT * FROM Orders AS o 
                    FULL OUTER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Joins.Should().HaveCount(1);
        result.Joins[0].JoinType.Should().Be("FULL");
    }

    [Fact]
    public void ParseMultipleColumnsFromSameTable()
    {
        // Arrange
        var sql = @"SELECT o.OrderId, o.OrderDate, o.TotalAmount, c.CustomerName 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        ordersTable.Columns.Should().HaveCount(4); // 3 selected + 1 join
        ordersTable.SelectedColumns.Should().HaveCount(3);
    }

    [Fact]
    public void ParseJoinWithSchemaQualifiedTables()
    {
        // Arrange
        var sql = @"SELECT * FROM [dbo].[Orders] AS o 
                    INNER JOIN [dbo].[Customers] AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(2);
        result.Tables[0].Name.Should().Be("Orders");
        result.Tables[1].Name.Should().Be("Customers");
    }

    [Fact]
    public void ParseColumnsWithUnderscores()
    {
        // Arrange
        var sql = @"SELECT o.order_id, o.created_date, c.customer_name 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.customer_id = c.id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        var ordersTable = result.Tables.First(t => t.Alias == "o");
        ordersTable.Columns.Should().Contain(c => c.Name == "order_id");
        ordersTable.Columns.Should().Contain(c => c.Name == "created_date");
        ordersTable.Columns.Should().Contain(c => c.Name == "customer_id");
    }

    [Fact]
    public void ParseWithExtraWhitespace()
    {
        // Arrange
        var sql = @"SELECT    o.OrderId   ,   c.CustomerName   
                    FROM    Orders   AS   o   
                    INNER   JOIN   Customers   AS   c   ON   o.CustomerId   =   c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(2);
        result.Joins.Should().HaveCount(1);
    }

    [Fact]
    public void ParseTablesWithNumbers()
    {
        // Arrange
        var sql = @"SELECT o1.OrderId, o2.ParentOrderId 
                    FROM Orders2023 AS o1 
                    INNER JOIN Orders2022 AS o2 ON o1.ParentId = o2.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(2);
        result.Tables.Should().Contain(t => t.Name == "Orders2023");
        result.Tables.Should().Contain(t => t.Name == "Orders2022");
    }

    [Fact]
    public void ParseQueryWithOnlySelectKeyword()
    {
        // Arrange
        var sql = "SELECT";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Should().NotBeNull();
        result.Tables.Should().BeEmpty();
        result.Joins.Should().BeEmpty();
    }

    [Fact]
    public void ParseSelfJoinWithMultipleInstances()
    {
        // Arrange
        var sql = @"SELECT * FROM Employees AS e1 
                    INNER JOIN Employees AS e2 ON e1.ManagerId = e2.Id
                    LEFT JOIN Employees AS e3 ON e2.ManagerId = e3.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        result.Tables.Should().HaveCount(3);
        result.Tables.Should().OnlyContain(t => t.Name == "Employees");
        result.Tables.Select(t => t.Alias).Should().BeEquivalentTo(new[] { "e1", "e2", "e3" });
    }

    [Fact]
    public void ParseSelectWithFunctions()
    {
        // Arrange
        var sql = @"SELECT COUNT(o.OrderId), MAX(o.Amount), c.CustomerName 
                    FROM Orders AS o 
                    INNER JOIN Customers AS c ON o.CustomerId = c.Id";

        // Act
        var result = SqlDiagramParser.ParseSql(sql);

        // Assert
        // Should at least parse tables and joins correctly
        result.Tables.Should().HaveCount(2);
        result.Joins.Should().HaveCount(1);
        
        // The CustomerName should be parsed from the function call
        var customersTable = result.Tables.First(t => t.Alias == "c");
        customersTable.Columns.Should().Contain(c => c.Name == "CustomerName");
    }
}
