# AI Reflection

## Claude Code & Strategy Pattern Refactoring
Claude excelled at identifying the tight coupling inside `OrderService.cs` and separating business rules into clean strategy pattern abstractions. It structured the interface definitions logically so new rules can be added without modifying core service methods. However, close review was needed around nullability annotations—specifically regarding DTO mappings where non-nullable properties in C# 10/11 like `CustomerName` could introduce runtime warning CS8618 or subtle null reference bugs if not explicitly initialized.

## GitHub Copilot Test Generation
GitHub Copilot saved significant setup time when generating xUnit test cases from inline comments like `// Test: validation rejects orders with negative quantity`. It instantly populated mock `DbContext` setups and expected failure assertions. Where Copilot suggested something subtly wrong was in boundary edge cases—it initially assumed valid orders could hold zero quantities and defaulted to basic assertions without checking secondary state changes in order totals.

## 2 AM IST Production Debugging Choice
At 2 AM IST debugging a production incident, I reach for GitHub Copilot first. In an urgent production breakdown, inline context completion and local stack trace targeted fixes inside the IDE are faster for isolating precise method failures. While Claude is superior for high-level architectural refactoring and initial codebase generation, Copilot's low-latency completion right inside the failing test suite provides quicker resolution when minutes matter.