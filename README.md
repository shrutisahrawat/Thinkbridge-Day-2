# Thinkbridge_Shruti_Sahrawat
# Thinkbridge Backend Development Assignment
**Days 1, 2, and 3 — Complete Backend Architecture & Authentication**

---

## 📋 Overview

This assignment covers three days of backend development, building from foundations through advanced authentication patterns.

| Day | Focus | Status |
|-----|-------|--------|
| **Day 1** | Foundations: Hello, APIs, Refactoring, DDD Aggregates | ✅ Complete |
| **Day 2** | Advanced Architecture: DI, async/await, JWT Auth, Refresh Tokens | ✅ Complete |
| **Day 3** | Enterprise Auth: Entra ID Integration with Dual Schemes | ✅ Complete |

---

## 📚 Day 1 — Foundations (5 Pieces)

### [Piece 1: Hello in Two Languages](https://github.com/shrutisahrawat/Thinkschool)
Compare C# (.NET 10) vs TypeScript (Node 24) runtimes
- Build: .csproj vs nothing
- Runtime: .NET SDK vs native Node
- **Concepts:** Language fundamentals, runtime comparison

### [Piece 2: Minimal ASP.NET Core API](https://github.com/shrutisahrawat/quotes-api-day1)
First real API with EF Core + SQLite
- **Endpoints:** GET /api/quotes, POST /api/quotes, GET /api/quotes/{id}, DELETE /api/quotes/{id}
- **Features:** Dependency Injection, validation, error handling
- **Concepts:** Minimal APIs, EF Core fundamentals, async/await

### [Piece 3: Refactor God-Method](https://github.com/shrutisahrawat/Order-Refactor-Day1)
Refactor a legacy controller into layered architecture
- **Before:** Mixed business logic + data access + validation
- **After:** Controller → Service → Repository layers
- **Concepts:** Separation of concerns, testability, DI design

### [Piece 4: Real AI-Assisted Work](https://github.com/shrutisahrawat/Order-Refactor-Day1)
Use Claude Code to refactor messy code into patterns
- AI generates the offending code
- You review and refactor with Claude's help
- Document the process with REFACTOR_NOTES.md
- **Concepts:** Code review discipline, AI-assisted development

### [Piece 5: Build Real Aggregate](https://github.com/shrutisahrawat/Day1-Build-a-real-aggregate)
Domain-Driven Design (DDD) aggregate pattern
- **Entity:** Collection with Id, Name, OwnerId, Items
- **Value Object:** CollectionItem (immutable, QuoteId + AddedAt)
- **Invariants:** Name length, max 50 items, no duplicates
- **Repository:** ICollectionRepository with owned types
- **Concepts:** DDD basics, aggregate roots, invariant enforcement

---

## 🔐 Day 2 — Advanced Architecture & Authentication (6 Pieces)

**Repository:** [Thinkbridge-Day-2](https://github.com/shrutisahrawat/Thinkbridge-Day-2)

### Piece 1: Dependency Injection at Depth
Understanding DI lifetimes: Transient, Scoped, Singleton
- Added IClock abstraction (for testable datetime)
- Registered with appropriate lifetimes
- **Concepts:** DI container, service lifetimes, testability

### Piece 2: async/await with Cancellation Through Layers
Proper cancellation token flow
- Every async method takes `CancellationToken` as last parameter
- Tokens flow from controller → service → repository → EF
- Test cancellation mid-request
- **Concepts:** async patterns, cancellation, deadlock avoidance

### Piece 3: Test the Domain Layer
Unit testing with xUnit + Fluent Assertions
- Tests for Collection aggregate invariants
- Empty name throws
- Name > 80 chars throws
- Max 50 items enforced
- No duplicate QuoteIds
- **Concepts:** xUnit, Fluent Assertions, testing pyramid

### Piece 4: AI-Assisted Refactor — Anemic to Rich
Refactor Quote entity from simple properties to rich domain model
- Add invariants: Text (1-1000 chars), Author (1-200 chars)
- Static factory: `Quote.Create(author, text)`
- **Concepts:** Rich domain models, invariants, factory methods

### Piece 5: Implement JWT Auth (Your Own Issuer)
Custom JWT authentication
- **Login endpoint:** POST /api/auth/login → access_token + refresh_token
- Users table with bcrypt password hashing
- JWT signed with HS256 + 256-bit key
- Protected endpoints with `[Authorize]`
- **Concepts:** JWT, authentication, password hashing

### Piece 6: Refresh Tokens with Rotation & Reuse Detection
Security-hardened token refresh
- **RefreshTokens table:** TokenHash, UserId, ExpiresAt, RevokedAt, ReplacedByToken
- **On refresh:** Validate → generate new pair → mark old as replaced
- **Reuse detection:** If old token replayed → revoke entire family
- Security event logging
- **Concepts:** Token rotation, leak detection, family revocation

---

## 🌐 Day 3 — Enterprise Authentication (Entra ID)

**Repository:** [Thinkbridge-Day-2](https://github.com/shrutisahrawat/Thinkbridge-Day-2) (latest commits)

### What You Implemented

**Problem:** Internal JWT is fine for APIs. For customer-facing apps, delegate auth to Microsoft Entra ID.

**Solution:** Dual authentication scheme
- **Internal JWT** → for backend-to-backend calls (Day 2 auth)
- **Entra JWT** → for SPA/customer apps (new)
- **PolicyScheme** → reads token issuer, routes to correct validator

### Changes Made

**File: `appsettings.json`** - Added Entra configuration block with Tenant ID, Client ID, Audience

**File: `Program.cs`**
- Registered `InternalJwt` scheme (Day 2 logic unchanged)
- Registered `EntraJwt` scheme (validates against Microsoft's public keys)
- Registered `PolicyScheme` traffic cop (routes by issuer claim)

### Testing

✅ **Internal JWT:** Confirmed working (200 OK with valid token)
✅ **Code:** Ready for Entra token validation
✅ **Build:** Succeeds
✅ **Day 2:** Completely preserved (no breaking changes)

### Key Concepts

1. **Issuer Claim** — Identifies who created the token
2. **Authority** — Base URL for fetching validation keys
3. **Policy Scheme** — Meta-scheme that selects the real scheme
4. **Symmetric vs Asymmetric Keys**
5. **Dual Authentication** — Supporting multiple token sources

---

## 🏗️ Architecture Evolution
Day 1: Single-layered
├─ hello-cs, hello-ts (language comparison)
├─ QuotesApi (basic API)
├─ OrderRefactor (layered refactor)
└─ DDD aggregate pattern

Day 2: Enterprise-ready
├─ DI at depth (lifetimes)
├─ Async/cancellation patterns
├─ Rich domain models
├─ JWT authentication
└─ Secure refresh tokens

Day 3: Cloud-native
├─ Dual auth schemes
├─ Entra ID integration
├─ PolicyScheme routing
└─ Zero breaking changes

---

## 📁 Repository Links

| Day | Repository | Main Project | Status |
|-----|-----------|--------------|--------|
| **Day 1** | [Thinkschool](https://github.com/shrutisahrawat/Thinkschool) | hello-cs, hello-ts | ✅ |
| **Day 1** | [quotes-api-day1](https://github.com/shrutisahrawat/quotes-api-day1) | QuotesApi | ✅ |
| **Day 1** | [Order-Refactor-Day1](https://github.com/shrutisahrawat/Order-Refactor-Day1) | OrderRefactor | ✅ |
| **Day 1** | [Day1-Build-a-real-aggregate](https://github.com/shrutisahrawat/Day1-Build-a-real-aggregate) | QuotesApi + Collection | ✅ |
| **Day 2 & 3** | [Thinkbridge-Day-2](https://github.com/shrutisahrawat/Thinkbridge-Day-2) | OrderRefactor | ✅ |

---

## 🚀 Running the Code

### Day 1 — Hello World
```bash
cd hello-cs && dotnet run
cd hello-ts && node hello.ts
```

### Day 2 & 3 — OrderRefactor API
```bash
cd OrderRefactor
dotnet build
dotnet run
# API runs on http://localhost:5021
```

### Test Internal JWT
```powershell
# Login
$body = @{email="admin@quotes.com"; password="SecurePassword123"} | ConvertTo-Json
Invoke-WebRequest -Uri "http://localhost:5021/api/auth/login" `
  -Method POST -ContentType "application/json" -Body $body -UseBasicParsing
```





