# Prompt  to generate the original OrderController.cs

Write a deliberately-bad OrderController.cs for an ASP.NET Core 10 minimal API project.
Requirements:
- ~300 lines
- One giant POST /api/orders action that mixes business logic, EF Core data access,
  validation, and HTTP response shaping all inline
- Four empty catch {} blocks that silently swallow exceptions
- Synchronous EF Core calls used inside an async action (blocking on .Result / .Wait())
- Returns raw `object` instead of typed response models
- Zero tests
- A couple of subtle bugs: one off-by-one error, one null-dereference bug
- Do not refactor or clean it up — this is intentionally bad, legacy-style code