# Thinkbridge - Day 2: Dependency Injection at Depth

This repository contains the Day 2 tasks focusing on advanced Dependency Injection (DI) lifetimes, abstractions, and testability in .NET Core.

## Tasks Covered
* **Task 1**: Implemented the `IClock` abstraction with a production `SystemClock` (registered as a **singleton** in DI) and a `FakeClock` for robust unit testing.

## Project Structure
* **QuotesApi/**: Contains the core API services, controllers, and DI configurations (`Program.cs`).
* **OrderRefactor.Tests/**: Contains unit tests including the `FakeClock` implementation.

## Running Tests
To verify the implementation and run tests, use the following command in your terminal:
```powershell
dotnet test
