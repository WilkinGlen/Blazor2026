namespace Blazor2026.Components.Pages;

public partial class Home
{
    private string sqlQuery = string.Empty;
    private string displayedSqlQuery = string.Empty;

    private void GenerateDiagram()
    {
        this.displayedSqlQuery = this.sqlQuery;
    }

    private void LoadSampleQuery()
    {
        this.sqlQuery = @"SELECT 
    o.OrderId AS OrderNumber, 
    o.OrderDate AS DateOrdered, 
    c.CustomerName AS Client, 
    p.ProductName AS Item, 
    od.Quantity AS Qty, 
    od.Price AS UnitPrice
FROM Orders AS o
INNER JOIN Customers AS c ON o.CustomerId = c.CustomerId
INNER JOIN OrderDetails AS od ON o.OrderId = od.OrderId
LEFT JOIN Products AS p ON od.ProductId = p.ProductId
WHERE o.OrderDate >= '2024-01-01'";

        this.GenerateDiagram();
    }
}
