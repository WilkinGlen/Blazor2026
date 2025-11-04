# SQL Formatter Service

A static utility service for formatting SQL queries with intelligent indentation and structure.

## Overview

The `SqlFormatterService` provides static SQL formatting capabilities that make queries more readable by:
- Formatting SELECT columns on separate lines with indentation
- Progressive indentation for JOIN clauses
- Proper spacing around keywords (AS, FROM, WHERE, etc.)
- Smart handling of AND/OR conditions (keeps parenthesized conditions on same line)
- Empty lines between major clauses for better readability

## Location

**File**: `Blazor2026/Services/SqlFormatterService.cs`

## Registration

~~The service is registered in `Program.cs` as a scoped service:~~

**No registration needed** - The service contains only static methods and does not require dependency injection.

## Usage

### In Blazor Components

Call the static method directly:

```csharp
using Blazor2026.Services;

private void FormatMySql()
{
    string formatted = SqlFormatterService.FormatSql(originalSql);
}
```

### Example

**Input:**
```sql
SELECT Col1, Col2, Col3 FROM Table1 AS T1 LEFT JOIN Table2 AS T2 ON T1.Id = T2.Id LEFT JOIN Table3 AS T3 ON T1.Id = T3.Id WHERE (Status = 'A' AND Type = 'B') AND Active = 1
```

**Output:**
```sql
SELECT 
	Col1, 
	Col2, 
	Col3 
FROM Table1 AS T1 
	LEFT JOIN Table2 AS T2 
	ON T1.Id = T2.Id 
		LEFT JOIN Table3 AS T3 
		ON T1.Id = T3.Id 

WHERE (Status = 'A' AND Type = 'B')
AND Active = 1
```

## Features

### Progressive JOIN Indentation
Each subsequent JOIN is indented one more tab than the previous:
- 1st JOIN: 1 tab
- 2nd JOIN: 2 tabs
- 3rd JOIN: 3 tabs
- And so on...

### ON Clause Alignment
ON clauses are indented to match their corresponding JOIN statement.

### Smart Parenthesis Handling
AND/OR keywords within parentheses stay on the same line, while top-level AND/OR are placed on new lines.

### Source-Generated Regexes
All regex patterns use `GeneratedRegexAttribute` for optimal performance:
- Compiled at build time
- Zero runtime overhead
- AOT-compatible

## Supported SQL Keywords

- SELECT
- FROM
- WHERE
- AND / OR
- JOIN / LEFT JOIN / RIGHT JOIN / INNER JOIN
- ON
- AS (for aliases)

## Implementation Details

### Regex Patterns

The service uses 13 source-generated regex patterns:
1. `SelectRegex` - Matches SELECT keyword
2. `FromRegex` - Matches FROM keyword
3. `WhereRegex` - Matches WHERE keyword
4. `AndRegex` - Matches AND keyword
5. `OrRegex` - Matches OR keyword
6. `LeftJoinRegex` - Matches LEFT JOIN
7. `RightJoinRegex` - Matches RIGHT JOIN
8. `InnerJoinRegex` - Matches INNER JOIN
9. `OnRegex` - Matches ON keyword
10. `AsRegex` - Matches AS keyword
11. `CommaWithSpacesRegex` - Matches commas with optional whitespace
12. `SelectColumnsRegex` - Extracts column list between SELECT and FROM
13. `StandaloneJoinRegex` - Matches JOIN when not preceded by LEFT/RIGHT/INNER

All patterns are case-insensitive and use word boundaries to prevent partial matches.

### Error Handling

If an error occurs during formatting, the service returns:
```
"Error formatting SQL."
```

## Benefits

✅ **Separation of Concerns** - SQL formatting logic separated from UI components  
✅ **Reusability** - Can be used across multiple components without injection  
✅ **Testability** - Easy to unit test in isolation  
✅ **Performance** - Uses source-generated regexes  
✅ **Maintainability** - Centralized formatting rules  
✅ **No Dependencies** - Static utility class, no DI required  
✅ **Simplicity** - Direct method calls without service registration  

## Future Enhancements

Potential improvements:
- Support for additional SQL keywords (ORDER BY, GROUP BY, HAVING, etc.)
- Configurable indentation (spaces vs tabs, indent size)
- Different formatting styles/presets
- Support for subqueries
- Comment preservation
- Validation and syntax highlighting
