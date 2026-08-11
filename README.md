# Thinkbridge - Day 2: Advanced Backend Architecture & Authentication Suite

This repository contains the complete implementation for **Day 2** of the Thinkbridge technical session, covering advanced dependency injection lifetimes, async patterns, domain testing, domain model refactoring, and secure authentication pipelines (JWT, Custom Issuer, and Refresh Token Rotation).

---

## 📋 Day 2 Task Breakdown

### **8. Dependency Injection at Depth**
* Implemented the `IClock` abstraction with a production `SystemClock` (registered as a **singleton** in DI) and a `FakeClock` for robust unit testing.
* Configured advanced service lifetimes and managed complex service dependencies across application layers.

### **9. Async/Await with Cancellation Through Layers**
* Implemented non-blocking asynchronous operations passing `CancellationToken` down from controllers through services and repository layers.

### **10. Test the Domain Layer**
* Developed comprehensive unit tests focusing on business logic validation, state changes, and domain rules.

### **11. AI-Assisted Refactor: Anemic to Rich Domain Models**
* Refactored anemic data-holder models into rich domain models containing encapsulating behaviors, validation rules, and domain-driven design principles.

### **12. JWT, OAuth2, OIDC: What Each Is For**
* Established architectural clarity on JSON Web Tokens (authentication claims), OAuth2 (authorization delegation), and OpenID Connect (identity layer).

### **13. Implement JWT Auth (Your Own Issuer)**
* Built a custom token-generation and validation pipeline using a private symmetric key issuer (`/api/auth/login`).

### **14. Refresh Tokens with Rotation & Reuse-Detection**
* Implemented database-backed refresh tokens featuring automatic rotation upon use.
* Added security breach detection: re-submitting an already-revoked refresh token triggers an immediate revocation of the entire token family tree with a `401 Unauthorized` security alert.

---

## 🛠️ Project Structure & Tech Stack
* **QuotesApi/**: Contains core API services, controllers, and DI configurations (`Program.cs`).
* **OrderRefactor.Tests/**: Contains unit tests including the `FakeClock` implementation and domain tests.
* **Language & Framework:** C#, .NET 10.0, ASP.NET Core Web API
* **Database & ORM:** SQLite, Entity Framework Core (EF Core)
* **Testing:** xUnit
* **Security:** JWT, Cryptographic Token Generation, SHA-256 Hashing

---

## 🧪 Testing and Verification

### Run Unit Tests
To verify dependencies and test the domain layer:
```powershell
dotnet test
