# InsurancePortal.IdentityService.Infrastructure - Architecture & Design Guide

## Overview

The **InsurancePortal.IdentityService.Infrastructure** project is a .NET 10 data access and security layer that implements core infrastructure patterns for identity management. It follows a Clean Architecture approach with clear separation of concerns between data persistence, security, and external integrations.

### Technology Stack
- **.NET Version**: 10.0
- **ORM**: Entity Framework Core 10.0.6
- **Database**: SQL Server
- **Authentication**: JWT (JSON Web Tokens) with System.IdentityModel.Tokens.Jwt 8.17.0
- **Configuration**: Microsoft.Extensions.Configuration 10.0.6

---

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│              InsurancePortal.IdentityService.Api             │
│                     (Presentation Layer)                     │
└────────────────────────┬────────────────────────────────────┘
						 │
┌────────────────────────▼────────────────────────────────────┐
│         InsurancePortal.IdentityService.Application          │
│                    (Business Logic Layer)                    │
│                   (Interfaces & DTOs)                        │
└────────────────────────┬────────────────────────────────────┘
						 │
┌────────────────────────▼────────────────────────────────────┐
│       InsurancePortal.IdentityService.Infrastructure         │
│              (Data Access & Security Layer)                  │
│           ⬇ This Project - Implements Interfaces ⬇          │
└─────────────────────────────────────────────────────────────┘
						 │
┌────────────────────────▼────────────────────────────────────┐
│       InsurancePortal.IdentityService.Domain                 │
│                   (Domain Entities Layer)                    │
│                   (Business Models)                          │
└─────────────────────────────────────────────────────────────┘
```

---

## Project Structure & Modules

### 1. **Persistence Module**
Handles all database operations using Entity Framework Core.

#### Components:

##### `IdentityDbContext.cs`
- **Purpose**: Entity Framework DbContext for the identity database
- **Responsibilities**:
  - Defines DbSets for domain entities (User, Credential, ExternalIdentity)
  - Configures entity relationships and mapping
  - Applies fluent configuration to entities
- **Key DbSets**:
  - `DbSet<UserIdentity>` - User accounts
  - `DbSet<Credential>` - User credentials/passwords
  - `DbSet<ExternalIdentity>` - OAuth/SSO identities

##### `UserRepository.cs`
- **Pattern**: Repository Pattern (Implements IUserRepository from Application layer)
- **Responsibilities**:
  - Encapsulates data access logic for Users and Credentials
  - Provides CRUD operations for user accounts
  - Handles credential retrieval by user
  - Uses async/await for non-blocking database operations
- **Key Methods**:
  - `GetUserByIdAsync(Guid userId)` - Retrieve user by primary key
  - `GetUserByEmailAsync(Guid tenantId, string email)` - Multi-tenant user lookup
  - `AddUserAsync(UserIdentity user)` - Create new user
  - `AddCredentialAsync(Credential credential)` - Store user credentials
  - `GetCredentialByUserIdAsync(Guid userId)` - Retrieve credential

##### `Configuration/` Directory
Entity configurations implementing `IEntityTypeConfiguration<T>` pattern:
- **UserIdentityConfiguration.cs** - User entity mapping (table structure, constraints, relationships)
- **CredentialConfiguration.cs** - Credential entity mapping (password hash, salt, links to user)
- **ExternalIdentityConfiguration.cs** - External identity mapping (OAuth provider data)

##### `Migrations/` Directory
Entity Framework Core migrations for database schema versioning:
- **20260618224016_InitialCreate.cs** - Initial database schema creation
- **20260618224016_InitialCreate.Designer.cs** - Migration metadata
- **IdentityDbContextModelSnapshot.cs** - Current model state snapshot for future migrations

---

### 2. **API Authentication Middleware**
Handles bearer token authentication and authorization for protected endpoints.

#### Components:

##### `Program.cs` - JWT Authentication Configuration
- **Purpose**: Configure ASP.NET Core authentication pipeline with JWT Bearer support
- **Responsibilities**:
  - Register JWT Bearer authentication scheme
  - Configure token validation parameters
  - Add authentication middleware to request pipeline
  - Add authorization middleware to enforce [Authorize] attributes
- **Configuration Source**: `appsettings.json` → `JwtSettings` section
  - `SecretKey` - Symmetric key for HMAC-SHA256 signature verification (must be 32+ characters)
  - `Issuer` - Expected token issuer
  - `Audience` - Expected token audience
  - `ExpirationMinutes` - Token lifetime configuration
- **Security Features**:
  - Validates token signature using symmetric key
  - Checks token expiration with zero clock skew
  - Validates issuer and audience claims
  - Automatically returns 401 Unauthorized for invalid tokens
  - Uses Bearer authentication scheme by default

##### `HomeController.cs` - Protected Endpoint Example
- **Purpose**: Demonstrates Bearer token authentication in an ASP.NET Core controller
- **Endpoint**: `GET /api/home/index`
- **Authentication**: `[Authorize(AuthenticationSchemes = "Bearer")]` attribute
- **Responsibilities**:
  - Only accessible to authenticated users with valid JWT Bearer token
  - Extracts email claim from validated JWT token (generated by `JwtTokenService`)
  - Returns welcome message with authenticated user information
  - Provides timestamp and endpoint description
- **Security Pattern**:
  - Requires Bearer token in `Authorization` header: `Bearer {token}`
  - Automatic 401 Unauthorized if token missing or invalid
  - **Email claim guaranteed by authorization** - only authenticated requests reach the method
  - Extracts email claim via `User.FindFirst("email")?.Value` (matching JwtTokenService payload)
  - Demonstrates claims-based identity model
- **Implementation Details**:
  - Single `Index()` method - only one entry point
  - No fallback values needed (authentication ensures email exists)
  - Email claim comes directly from JWT token generated by `JwtTokenService.GenerateToken()`

#### Authentication Flow Diagram
```
┌─────────────────────────────────────────────────────────────┐
│ 1. Client Logs In                                           │
│    POST /api/auth/login {email, password}                  │
│    ↓                                                        │
│    LoginHandler validates credentials                       │
│    ↓                                                        │
│    JwtTokenService generates JWT token                      │
│    (Token contains: user ID, email, tenant, expiration)    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Client Stores Token                                      │
│    Securely stores JWT (in cookie, local storage, etc.)    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Client Requests Protected Resource                       │
│    GET /api/home/index                                     │
│    Header: Authorization: Bearer {token}                    │
│    ↓                                                        │
│    Request reaches authentication middleware                │
│    ↓                                                        │
│    Middleware extracts token from header                    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Token Validation (Zero Clock Skew)                       │
│    ✓ Check Bearer scheme format                             │
│    ✓ Verify HMAC-SHA256 signature (SecretKey)               │
│    ✓ Validate expiration timestamp                          │
│    ✓ Validate issuer claim                                  │
│    ✓ Validate audience claim                                │
│    ↓                                                        │
│    If valid: Extract claims → User object                  │
│    If invalid: Return 401 Unauthorized                     │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Authorization Check (Attribute on Controller)            │
│    Controller has [Authorize(AuthenticationSchemes = "Bearer")]
│    ✓ Verify authentication scheme matches                   │
│    ✓ Verify [Authorize] attribute requirements              │
│    ↓                                                        │
│    If authorized: Proceed to controller action              │
│    If not authorized: Return 401 Unauthorized              │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Controller Action Executes                               │
│    HomeController.Index()                                  │
│    ✓ Access user claims from User property                 │
│    ✓ GET User.FindFirst("email")?.Value                    │
│    ✓ Build response with user info                         │
│    ↓                                                        │
│    Return 200 OK with welcome message                      │
└─────────────────────────────────────────────────────────────┘
```

---

### 3. **Security Module**
Implements cryptographic and authentication services.

#### Components:

##### `JwtTokenService.cs`
- **Pattern**: Strategy Pattern (Implements ITokenService from Application layer)
- **Purpose**: JWT token generation and configuration
- **Responsibilities**:
  - Generates JWT tokens for authenticated users
  - Configures token claims (user ID, tenant ID, email)
  - Sets token expiration based on configuration
  - Signs tokens with HMAC SHA256
  - Consumes configuration (Jwt:Secret, Jwt:Issuer, Jwt:Audience, Jwt:ExpireMinutes)
- **Token Structure**:
  ```
  Header: { alg: "HS256", typ: "JWT" }
  Payload: 
  {
	sub: <UserId>,
	tenant: <TenantId>,
	email: <UserEmail>,
	iat: <IssuedAt>,
	exp: <ExpirationTime>,
	iss: <Issuer>,
	aud: <Audience>
  }
  ```
- **Configuration Keys**: Located in appsettings.json
  - `Jwt:Secret` - Symmetric key for signing
  - `Jwt:Issuer` - Token issuer identifier
  - `Jwt:Audience` - Token audience identifier
  - `Jwt:ExpireMinutes` - Token lifetime in hours

##### `PassowordHasher.cs`
- **Pattern**: Strategy Pattern (Implements IPasswordHasher from Application layer)
- **Purpose**: Cryptographically secure password hashing
- **Algorithm**: PBKDF2 (Password-Based Key Derivation Function 2)
- **Security Parameters**:
  - Salt Size: 128 bits (16 bytes) - Prevents rainbow table attacks
  - Salt Length: 32 bytes - Hash output length
  - Iterations: 100,000 - Computational cost to slow brute-force attacks
  - Hash Algorithm: SHA256
- **Responsibilities**:
  - Generates random salt for each password
  - Derives hash using PBKDF2 with SHA256
  - Verifies passwords against stored hashes
  - Implements IPasswordHasher contract
- **Note**: Uses static PBKDF2 method to comply with .NET security guidelines (avoids SYSLIB0060 obsolete warning)

---

### 3. **External Providers Module**
Handles OAuth/SSO integrations with external identity providers.

#### Components:

---

## HomeController Implementation Guide

### Overview
The `HomeController` demonstrates how to implement Bearer token authentication in ASP.NET Core using the JWT authentication pipeline configured in the Infrastructure layer.

### Endpoint Details

#### GET /api/home/index
- **Route**: `/api/home/index`
- **HTTP Method**: GET
- **Authentication Required**: Bearer Token (JWT)
- **Authorization Attribute**: `[Authorize(AuthenticationSchemes = "Bearer")]`

### Request/Response

#### Success Response (200 OK)
```json
{
  "message": "Welcome to Insurance Portal Identity Service",
  "authenticatedUser": "user@example.com",
  "timestamp": "2024-01-15T10:30:45.123Z",
  "description": "This endpoint is only accessible with a valid Bearer token"
}
```

#### Error Response (401 Unauthorized)
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "traceId": "0HN3UQU8V7LFD:00000001"
}
```

### Configuration Required

Add to `appsettings.json` (**required - no default values**):
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-must-be-at-least-32-characters-long-1234567890",
    "Issuer": "InsurancePortalIdentityService",
    "Audience": "InsurancePortalClient",
    "ExpirationMinutes": 60
  }
}
```

⚠️ **Critical Configuration Note:**
- `JwtSettings:SecretKey` is **mandatory** - the application will throw an error if missing
- SecretKey must be at least **32 characters** for HMAC-SHA256
- Configuration validation happens at startup, preventing runtime errors
- Invalid configuration will cause: `InvalidOperationException: JwtSettings:SecretKey is required...`

**Security Requirements**:
- Never use weak or default secret keys
- Use strong, random secret keys (64+ characters recommended)
- Use environment variables in production
- Store in Azure Key Vault for production deployments
- Never commit secrets to version control
- Rotate keys periodically

### Token Validation Sequence

```
1. Extract Token from Authorization Header
   ├─ Format validation: "Bearer {token}"
   ├─ Extract base64url-encoded payload
   └─ Decode token parts (header.payload.signature)

2. Validate Token Signature
   ├─ Use HMAC-SHA256 algorithm
   ├─ Verify using SecretKey from appsettings.json
   └─ Reject if signature doesn't match

3. Validate Token Claims
   ├─ Check expiration (exp claim)
   ├─ Validate issuer (iss claim) = Issuer from config
   ├─ Validate audience (aud claim) = Audience from config
   └─ Allow zero clock skew for strict time checking

4. Extract User Claims
   ├─ Extract "email" claim
   ├─ Extract "sub" (subject/user ID) claim
   ├─ Extract "tenant" claim (if present)
   └─ Build ClaimsPrincipal from claims

5. Authorize Request
   ├─ Check Authentication Scheme = "Bearer"
   ├─ Verify [Authorize] attribute exists
   └─ Grant or deny access
```

### Testing the Endpoint

#### Using curl

**Step 1: Login to get token**
```bash
curl -X POST https://localhost:7278/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'
```

**Step 2: Use token to access protected endpoint**
```bash
curl -X GET https://localhost:7278/api/home/index \
  -H "Authorization: Bearer {token_from_step_1}"
```

#### Using JavaScript/Fetch
```javascript
// Get token
const loginResponse = await fetch('https://localhost:7278/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'user@example.com', password: 'password123' })
});
const { token } = await loginResponse.json();

// Call protected endpoint
const response = await fetch('https://localhost:7278/api/home/index', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});
const data = await response.json();
console.log(data);
```

#### Using Swagger UI
1. Navigate to `https://localhost:7278/swagger`
2. First, call `/api/auth/login` endpoint to get token
3. Click "Authorize" button (lock icon)
4. Enter token in format: `Bearer {token}`
5. Try `/api/home/index` endpoint

### Unit Testing

The `HomeControllerTests` class provides test coverage:

**Test 1: Index_WithValidUser_ShouldReturnOk**
- Arranges valid user with email claim (matching `JwtTokenService.GenerateToken()` payload)
- Acts on `HomeController.Index()`
- Asserts returns `AuthenticatedUserResponseDto` with email and welcome message

**Test 2: Index_ShouldExtractEmailFromJwtClaims**
- Arranges authenticated user with email claim
- Acts on `HomeController.Index()`
- Asserts returns 200 OK with all response properties populated
- Validates email extraction, message content, timestamp, and description

**Why These Tests?**
- The `[Authorize]` attribute ensures only authenticated requests reach the method
- Email claim is **guaranteed** by authentication middleware
- No fallback scenarios exist (no "Unknown User" case)
- Unit tests focus on the method logic, not authorization (authorization is tested separately via integration tests)
- Uses `AuthenticatedUserResponseDto` for strong typing and cross-assembly compatibility

**Run Tests**:
```bash
dotnet test InsurancePortal.IdentityService.UnitTest
```

### Common Issues & Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| **InvalidOperationException**: JwtSettings:SecretKey is required | Missing SecretKey in appsettings.json | Add `JwtSettings:SecretKey` to `appsettings.json` (must be 32+ chars) |
| No authentication handlers are registered | SecretKey not configured | Ensure `JwtSettings:SecretKey` is set before starting the app |
| 401 Unauthorized | No Authorization header | Include: `Authorization: Bearer {token}` |
| 401 Unauthorized | Invalid token format | Use format: `Bearer ` followed by token |
| 401 Unauthorized | Token expired | Get new token from login endpoint |
| 401 Unauthorized | Wrong secret key | Verify SecretKey in appsettings.json matches token generation |
| 401 Unauthorized | Issuer mismatch | Verify Issuer in config matches token's "iss" claim |
| 401 Unauthorized | Audience mismatch | Verify Audience in config matches token's "aud" claim |
| 500 Server Error | Missing entire JwtSettings section | Add complete JwtSettings section to appsettings.json |

### Security Best Practices

#### In Development
- Use a simple secret key for testing
- Access Swagger UI for ease of testing
- Monitor JWT tokens using jwt.io debugger
- Review claims in generated tokens
- Test error scenarios

#### In Production
- ✅ Use strong, random secret key (64+ characters recommended)
- ✅ Store secrets in Azure Key Vault
- ✅ Use environment variables for configuration
- ✅ Enable HTTPS only (no HTTP)
- ✅ Set appropriate token expiration (15-60 minutes)
- ✅ Implement refresh token rotation
- ✅ Log authentication failures
- ✅ Monitor for suspicious token usage
- ✅ Rotate secret key periodically
- ✅ Restrict CORS to trusted origins only
- ✅ Use secure HTTP-only cookies for token storage

### Integration with Other Modules

The HomeController integrates with:

**1. JWT Token Service & Configuration**
- `JwtTokenService.GenerateToken()` creates tokens with `email` claim
- HomeController extracts email via `User.FindFirst("email")?.Value`
- Configuration ensures token generation and validation use same keys

**2. Authentication Middleware (Program.cs)**
- Validates token signature before reaching controller
- Extracts claims and builds `ClaimsPrincipal` 
- Enforces Bearer authentication scheme

**3. Authorization Framework**
- `[Authorize]` attribute gates method access
- Returns 401 Unauthorized before method executes if invalid token
- Guarantees authenticated access only

**4. User Repository (Persistence Layer)** (Optional)
- Retrieve full user details using email from claims
- Log authentication events to database
- Audit trail for security compliance

### Implementation Pattern

The simplified implementation follows this pattern:

```csharp
[HttpGet("index")]
[Authorize(AuthenticationSchemes = "Bearer")]
public IActionResult Index()
{
    // Email claim guaranteed by [Authorize]
    var userEmail = User.FindFirst("email")?.Value;

    // Return strongly-typed DTO for cross-assembly compatibility
    var response = new AuthenticatedUserResponseDto(
        Message: "Welcome to Insurance Portal Identity Service",
        AuthenticatedUser: userEmail,
        Timestamp: DateTime.UtcNow,
        Description: "This endpoint is only accessible with a valid Bearer token"
    );

    return Ok(response);
}
```

**Key Design Decisions:**
- ✅ Uses `AuthenticatedUserResponseDto` public record (no anonymous types)
- ✅ Works across assembly boundaries for testing/consumption
- ✅ No try-catch block (no exception-prone operations)
- ✅ No null checks on email (guaranteed by `[Authorize]`)
- ✅ No fallback values (no "Unknown User")
- ✅ Direct claim extraction matching `JwtTokenService` payload
- ✅ Clean, focused business logic

**DTO Definition:**
```csharp
public record AuthenticatedUserResponseDto(
    string Message,
    string AuthenticatedUser,
    DateTime Timestamp,
    string Description);
```

### Extending to Other Endpoints

You can add the `[Authorize(AuthenticationSchemes = "Bearer")]` attribute to any controller action to protect it:

```csharp
[HttpGet("profile")]
[Authorize(AuthenticationSchemes = "Bearer")]
public IActionResult GetProfile()
{
    var userEmail = User.FindFirst("email")?.Value;
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    return Ok(new { userEmail, userId });
}
```

### Role-Based Access Control

To implement role-based authorization:

```csharp
[HttpPost("admin-only")]
[Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
public IActionResult AdminOnlyAction()
{
    return Ok("Admin access granted");
}
```

---

##### `OAuthProviders.cs`
- **Pattern**: Strategy Pattern with inheritance
- **Purpose**: Base class for OAuth provider implementations
- **Architecture**:
  - Abstract base class `OAuthProviderBase`
  - Extensible for multiple OAuth providers (Google, Microsoft, GitHub, etc.)
  - Encapsulates HTTP communication with OAuth endpoints
- **Responsibilities**:
  - Generates OAuth authorization URLs
  - Exchanges authorization codes for tokens
  - Retrieves user profile information from providers
  - Handles OAuth-specific authentication flows
- **Key Methods** (Abstract):
  - `GenerateAuthorizationUrl(string redirectUri, string state)` - Create redirect URL for user consent
  - Implicit exchange mechanism for authorization codes
- **HTTP Communication**: Uses HttpClient for external API calls
- **Secret Management**: Client ID and Secret injected via constructor

---

## Roles & Permissions Design

This section defines the authorization model for the InsurancePortal platform. It covers the business services (User, Quote, Payment, Policy, Claims, Dashboard), the roles that consume them, and how permissions are scoped. It is a design specification; no code is implemented here.

### Design Principles

- **Deny by default**: any action not explicitly granted to a role is denied.
- **Permissions over roles**: roles are bundles of permissions. Endpoints check permissions, not role names.
- **Least privilege**: each role receives only the permissions required for its business function.
- **Scoped access**: permissions are paired with a scope (`Own`, `Assigned`, `Organization`, `Tenant`, `Any`) so that a user can only act on data they are entitled to see.
- **Separation of internal and external users**: insurer staff (Admin, Associates, Agents) and external parties (Providers) are isolated by organization membership and tenant.

### Permission Naming Convention

Format: `<service>.<resource>.<action>[.<scope>]`

- Services: `user`, `quote`, `payment`, `policy`, `claim`, `dashboard`
- Actions: `read`, `create`, `update`, `delete`, `approve`, `issue`, `cancel`, `refund`, `settle`, `assign`, `submit`
- Scope suffix is optional. Omitted scope means the action is only valid for the scope defined in the matrix.

Permissions are stored in the `Permission` table and linked to roles through `RolePermission`. The JWT carries the effective permission set for the authenticated user as `permission` claims, in addition to the `role` claim.

### Scopes

| Scope | Meaning | Example |
|-------|---------|---------|
| `Own` | Records where `UserId` equals the caller | Registered user viewing own policy |
| `Assigned` | Records assigned to the caller (agent-owned customers, claims assigned to an associate) | Agent editing a quote for an assigned customer |
| `Organization` | Records belonging to the caller's provider or agency organization | Provider Staff viewing claims for their clinic |
| `Tenant` | Any record within the caller's tenant (`TenantId`) | Associate reviewing policies in the tenant |
| `Any` | Platform-wide, no tenant restriction | Admin managing all tenants |

### Services and Permissions

#### 1. User Services

| Permission | Description |
|------------|-------------|
| `user.profile.read.own` | View own profile |
| `user.profile.update.own` | Update own profile and change own password |
| `user.convert.own` | Convert own Guest account to Registered, completing required attributes |
| `user.read.tenant` | View users within tenant |
| `user.read.any` | View any user across tenants |
| `user.create.tenant` | Create users within tenant |
| `user.disable.tenant` | Disable or enable users within tenant |
| `user.role.assign.tenant` | Assign roles to users within tenant |
| `user.role.assign.any` | Assign any role, including Admin, across tenants |
| `user.org.manage.organization` | Manage staff membership of own provider organization |

#### 2. Quote Service

| Permission | Description |
|------------|-------------|
| `quote.create.guest` | Create a quote as a Guest. Saved with partial attributes and linked to the Guest account |
| `quote.read.own` | View own quotes |
| `quote.create.own` | Create a quote for self |
| `quote.update.own` | Update own quote while in draft |
| `quote.create.assigned` | Create a quote on behalf of an assigned customer |
| `quote.update.assigned` | Update quote for an assigned customer |
| `quote.read.assigned` | View quotes for assigned customers |
| `quote.read.tenant` | View all quotes within tenant |
| `quote.approve.tenant` | Approve or reject quotes for underwriting |
| `quote.delete.tenant` | Withdraw or delete quotes within tenant |

#### 3. Payment Service

| Permission | Description |
|------------|-------------|
| `payment.read.own` | View own payment history and invoices |
| `payment.create.own` | Pay own premium or invoice |
| `payment.read.assigned` | View payments for assigned customers |
| `payment.read.tenant` | View all payments within tenant |
| `payment.refund.tenant` | Issue refunds within tenant |
| `payment.reconcile.tenant` | Reconcile and export payment ledgers within tenant |
| `payment.read.organization` | View payments related to provider services (claim settlements, provider payouts) |

#### 4. Policy Service

| Permission | Description |
|------------|-------------|
| `policy.read.own` | View own policies and documents |
| `policy.issue.assigned` | Issue policy from an approved quote for an assigned customer |
| `policy.read.assigned` | View policies for assigned customers |
| `policy.update.assigned` | Endorse or modify policies for assigned customers |
| `policy.renew.assigned` | Renew policies for assigned customers |
| `policy.cancel.tenant` | Cancel policies within tenant |
| `policy.read.tenant` | View all policies within tenant |
| `policy.issue.tenant` | Issue policies within tenant |

#### 5. Claims Service

| Permission | Description |
|------------|-------------|
| `claim.submit.own` | Submit a claim against own policy |
| `claim.read.own` | View own claims and status |
| `claim.document.upload.own` | Upload supporting documents to own claim |
| `claim.read.assigned` | View claims assigned to the caller |
| `claim.read.tenant` | View all claims within tenant |
| `claim.review.tenant` | Review and assess claims within tenant |
| `claim.approve.tenant` | Approve or deny claims within tenant |
| `claim.settle.tenant` | Trigger settlement payment for approved claims |
| `claim.assign.tenant` | Assign claims to associates |
| `claim.read.organization` | View claims submitted to own provider organization |
| `claim.submit.organization` | Submit provider invoices or treatment/repair details for a claim |
| `claim.status.update.organization` | Update provider-side status (received, in progress, completed) |

#### 6. Dashboard Service

| Permission | Description |
|------------|-------------|
| `dashboard.personal.view.own` | View personal dashboard (policies, open claims, upcoming payments) |
| `dashboard.agent.view.assigned` | View agent dashboard (pipeline, commissions, customer policies) |
| `dashboard.operations.view.tenant` | View operational dashboard (claims queue, quote backlog, payments) |
| `dashboard.provider.view.organization` | View provider dashboard (claims received, settlement status) |
| `dashboard.admin.view.any` | View platform dashboard (users, tenants, system health, audit) |

### Role Definitions

| Role | Type | Scope Baseline | Purpose |
|------|------|----------------|---------|
| `Guest` | External, lightweight account | `Own` (limited) | Browse products, create and manage own quotes, and convert to Registered |
| `Registered` | External, customer | `Own` | Manage own profile, quotes, policies, payments, and claims |
| `Admin` | Internal, platform | `Any` | Manage tenants, users, roles, and platform configuration |
| `Associates` | Internal, insurer staff | `Tenant` | Underwrite quotes, issue policies, process and approve claims |
| `Agents` | Internal, sales | `Assigned` | Sell and service policies for assigned customers |
| `Providers Admin` | External, provider | `Organization` | Manage provider organization, staff, and claim responses |
| `Providers Staff` | External, provider | `Organization` (limited) | Process claims and update status for the provider organization |

### Role-to-Permission Matrix

Legend: `✔` granted, `—` not granted. Scope in brackets where it differs from the permission default.

| Permission | Guest | Registered | Agents | Associates | Providers Staff | Providers Admin | Admin |
|------------|:-----:|:----------:|:------:|:----------:|:---------------:|:---------------:|:-----:|
| `user.profile.read.own` | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `user.profile.update.own` | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `user.convert.own` | ✔ | — | — | — | — | — | — |
| `user.read.tenant` | — | — | — | ✔ | — | — | ✔ |
| `user.read.any` | — | — | — | — | — | — | ✔ |
| `user.create.tenant` | — | — | — | — | — | — | ✔ |
| `user.disable.tenant` | — | — | — | — | — | — | ✔ |
| `user.role.assign.tenant` | — | — | — | — | — | — | ✔ |
| `user.role.assign.any` | — | — | — | — | — | — | ✔ |
| `user.org.manage.organization` | — | — | — | — | — | ✔ | — |
| `quote.create.guest` | ✔ | — | — | — | — | — | — |
| `quote.read.own` | ✔ | ✔ | — | — | — | — | — |
| `quote.create.own` | — | ✔ | — | — | — | — | — |
| `quote.update.own` | ✔ | ✔ | — | — | — | — | — |
| `quote.create.assigned` | — | — | ✔ | — | — | — | — |
| `quote.update.assigned` | — | — | ✔ | — | — | — | — |
| `quote.read.assigned` | — | — | ✔ | — | — | — | — |
| `quote.read.tenant` | — | — | — | ✔ | — | — | ✔ |
| `quote.approve.tenant` | — | — | — | ✔ | — | — | — |
| `quote.delete.tenant` | — | — | — | ✔ | — | — | ✔ |
| `payment.read.own` | — | ✔ | — | — | — | — | — |
| `payment.create.own` | — | ✔ | — | — | — | — | — |
| `payment.read.assigned` | — | — | ✔ | — | — | — | — |
| `payment.read.tenant` | — | — | — | ✔ | — | — | ✔ |
| `payment.refund.tenant` | — | — | — | ✔ | — | — | — |
| `payment.reconcile.tenant` | — | — | — | — | — | — | ✔ |
| `payment.read.organization` | — | — | — | — | ✔ | ✔ | — |
| `policy.read.own` | — | ✔ | — | — | — | — | — |
| `policy.issue.assigned` | — | — | ✔ | — | — | — | — |
| `policy.read.assigned` | — | — | ✔ | — | — | — | — |
| `policy.update.assigned` | — | — | ✔ | — | — | — | — |
| `policy.renew.assigned` | — | — | ✔ | — | — | — | — |
| `policy.read.tenant` | — | — | — | ✔ | — | — | ✔ |
| `policy.issue.tenant` | — | — | — | ✔ | — | — | — |
| `policy.cancel.tenant` | — | — | — | ✔ | — | — | ✔ |
| `claim.submit.own` | — | ✔ | — | — | — | — | — |
| `claim.read.own` | — | ✔ | — | — | — | — | — |
| `claim.document.upload.own` | — | ✔ | — | — | — | — | — |
| `claim.read.assigned` | — | — | — | ✔ | — | — | — |
| `claim.read.tenant` | — | — | — | ✔ | — | — | ✔ |
| `claim.review.tenant` | — | — | — | ✔ | — | — | — |
| `claim.approve.tenant` | — | — | — | ✔ | — | — | — |
| `claim.settle.tenant` | — | — | — | ✔ | — | — | — |
| `claim.assign.tenant` | — | — | — | ✔ | — | — | ✔ |
| `claim.read.organization` | — | — | — | — | ✔ | ✔ | — |
| `claim.submit.organization` | — | — | — | — | ✔ | ✔ | — |
| `claim.status.update.organization` | — | — | — | — | ✔ | ✔ | — |
| `dashboard.personal.view.own` | — | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `dashboard.agent.view.assigned` | — | — | ✔ | — | — | — | ✔ |
| `dashboard.operations.view.tenant` | — | — | — | ✔ | — | — | ✔ |
| `dashboard.provider.view.organization` | — | — | — | — | ✔ | ✔ | — |
| `dashboard.admin.view.any` | — | — | — | — | — | — | ✔ |

### Role Notes

- **Guest**: a lightweight persisted account created on first quote. Quotes are saved with partial attributes and remain linked to the same `UserId` after conversion. Guests cannot purchase, pay, or file claims until converted to Registered.
- **Registered**: self-service role. Newly registered users receive this role by default. Accounts are always scoped to `Own` records.
- **Admin**: platform administrators. Manages users and roles, and has read visibility across tenants. Business actions (underwriting, claim approval, refunds) are intentionally not granted to Admin to preserve segregation of duties. Admin can be granted these only through a documented break-glass process.
- **Associates**: insurer back-office staff. Tenant-scoped. Cannot approve their own submitted quotes or claims (four-eyes principle, enforced in the application layer).
- **Agents**: sales producers. Access is limited to customers assigned to the agent. Agents cannot approve quotes or settle claims.
- **Providers Admin**: owner or manager of a provider organization (hospital, repair shop, clinic, etc.). Can manage staff membership within the organization.
- **Providers Staff**: day-to-day operators for a provider organization. Can respond to claims and update status but cannot submit or change financial settlement data, and cannot manage other users.

### Organization and Tenancy Model

- Every internal user belongs to exactly one `TenantId`. Associates, Agents, and Admin inherit tenant membership.
- Provider users belong to exactly one `OrganizationId` (provider entity) within a tenant. Providers Admin and Providers Staff are scoped to that organization only.
- A user may hold multiple roles, but the effective permission set is the union of those roles, filtered by their scope. Conflicting cross-role grants (for example, Associate and Provider Staff) are not allowed for the same identity; separate identities are required.

### Guest Lifecycle and Conversion

- **Creation**: a Guest account is created when a quote is first saved. The `UserIdentity` record is flagged as Guest and has nullable profile attributes (name, date of birth, contact details) that are required only for conversion.
- **Conversion**: `user.convert.own` lets the Guest complete required attributes and set credentials. The same `UserId` is kept, so existing quotes are preserved. The role is changed from `Guest` to `Registered` and a new permission set is issued on the next token.
- **Duplicate handling**: if the email already belongs to a Registered account, conversion is refused and the user is directed to log in and merge quotes.
- **Tenancy**: Guests are assigned to a default tenant until an agent or associate reassigns them.

### User Context

A single `CurrentUserContext` represents the authenticated caller for each request. It is built once from the validated JWT and is read-only for the rest of the request.

| Property | Source | Purpose |
|----------|--------|---------|
| `UserId` | `sub` claim | Owner matching for `Own` scope |
| `Email` | `email` claim | Display, notifications, and Guest-to-Registered duplicate checks |
| `Username` | `preferred_username` claim | Display and login identification |
| `FirstName` | `given_name` claim | Display and personalization |
| `LastName` | `family_name` claim | Display and personalization |
| `TenantId` | `tenant_id` claim | Tenant scope checks |
| `OrganizationId` | `org_id` claim (providers only) | Organization scope checks |
| `Roles` | `role` claims | Display and coarse checks |
| `Permissions` | `permission` claims | Authoritative capability checks |
| `IsGuest` | Account flag | Restricts Guest actions until conversion |
| `AccountStatus` | Account state | Blocks disabled or locked accounts |

- Exposed to the Application layer through an interface (for example, `ICurrentUserContext`) implemented in Infrastructure.
- Services never read raw claims directly; they use `CurrentUserContext` so authorization logic is consistent.
- Anonymous requests produce an empty context with no permissions.

### Enforcement Approach (Design Only)

- Authorization policies are named after permissions (for example, `quote.read.tenant`) so that endpoints declare the exact capability they need.
- Scope is evaluated against the resource's `OwnerUserId`, `AssignedUserId`, `OrganizationId`, or `TenantId`, as applicable.
- Permission changes take effect on the next token issuance. Revocation of high-risk permissions (`user.role.assign.any`, `payment.refund.tenant`, `claim.settle.tenant`) requires short token lifetimes or a token revocation list.
- Every grant and denial of a sensitive permission is written to an audit log.

### Open Decisions

- [ ] Confirm retention period for Guest accounts that never convert.
- [ ] Confirm whether Agents may view commission data within the `dashboard.agent.view.assigned` scope.
- [ ] Confirm whether Providers Staff may view patient or insured personal data beyond claim details.
- [ ] Confirm the break-glass process for granting Admin business permissions.

---

## Key Design Patterns & Principles

### 1. **Dependency Injection (DI)**
- Infrastructure services are registered in the DI container
- Supports loose coupling between layers
- Enable easy testing with mock implementations
- Example Services:
  - `IUserRepository` → `UserRepository`
  - `ITokenService` → `JwtTokenService`
  - `IPasswordHasher` → `PassowordHasher`

### 2. **Repository Pattern**
- Abstracts data access logic
- Centralizes CRUD operations
- Enables testing with mock repositories
- Interfaces defined in Application layer, implementations in Infrastructure

### 3. **Entity Framework Core Fluent Configuration**
- Uses `IEntityTypeConfiguration<T>` for clean entity mapping
- Separation of configuration from DbContext
- Enables reusability and maintainability

### 4. **Strategy Pattern**
- Multiple implementations of ITokenService
- Multiple implementations of IPasswordHasher
- Multiple OAuth provider strategies can be added

### 5. **Multi-Tenancy Support**
- User and credential queries filter by TenantId
- Ensures data isolation between tenants
- Visible in `GetUserByEmailAsync(Guid tenantId, string email)`

### 6. **CQRS (Command Query Responsibility Segregation)**
- Commands change state; queries read state and never mutate it
- Separate models per side: command handlers use the domain aggregate (`UserIdentity`) and its behavior methods; query handlers return read-only DTOs projected directly from EF Core with `AsNoTracking()`
- Implemented commands (`Application/Commands/`): `RegisterUserCommand`, `LoginCommand`, `ConvertGuestCommand`, `ExternalAuthCommand` (external login, account linking, and unlinking)
- Implemented queries (`Application/Queries/`): `GetCurrentUserQuery` (returns `CurrentUserProfileDto`), `GetUserRolesQuery` (returns role names)
- Planned, not yet implemented: `AssignRoleCommand`, `GetUserProfileQuery`
- Each command and query is a dedicated class with a single public operation; the former `Application/Handlers/` types have been removed
- Read operations use the dedicated query interface `IUserQueries` (implemented by `Infrastructure/Persistence/UserQueries.cs`), separate from the write-side `IUserRepository`, so read paths can be optimized or pointed at a read replica later without touching command logic
- Read endpoints exposed by `AuthController`: `GET api/auth/me` (current user profile) and `GET api/auth/me/roles` (current user's roles); both require Bearer authentication and scope to the current user ID
- Commands return only success/identifier or a newly issued token; they do not return full entity graphs
- Validation and authorization for commands run before handler execution; queries rely on `ICurrentUserContext` for scoping
- Persistence stays in a single SQL Server database for now; separate read stores are a future option, not a current requirement

---

## Data Model

### Authorization Tables

Roles and permissions are stored as first-class tables. Role and permission definitions are reference data; user-to-role assignments are transactional data.

| Table | Purpose | Key Columns |
|-------|---------|-------------|
| `Role` | Catalog of roles | `Id` (PK, int IDENTITY), `Name` (unique, e.g. `Registered`), `Type` (External/Internal/Provider), `Description`, `IsActive` |
| `Permission` | Catalog of permissions | `Id` (PK, int IDENTITY), `Code` (unique, e.g. `quote.read.own`), `Service`, `Resource`, `Action`, `Scope` (Own/Assigned/Organization/Tenant/Any), `Description` |
| `RolePermission` | Role-to-permission assignment (many-to-many) | `RoleId` (FK), `PermissionId` (FK), composite PK `{RoleId, PermissionId}` |
| `UserRole` | User-to-role assignment (many-to-many) | `UserId` (FK to `UserIdentity`), `RoleId` (FK to `Role`), `AssignedAt`, `AssignedBy`, composite PK `{UserId, RoleId}` |

Relationships:
- `Role` 1 ── * `RolePermission` * ── 1 `Permission`
- `UserIdentity` 1 ── * `UserRole` * ── 1 `Role`
- A user's effective permissions are the distinct union of `Permission` rows reached through their `UserRole` → `RolePermission` assignments.

Notes:
- `UserRole.RoleId` references `Role.Id`; the current `RoleName` string column is replaced by this foreign key.
- `Permission.Code` is the value emitted in the JWT `permission` claim.
- Seed data for `Role`, `Permission`, and `RolePermission` comes from the matrix in Roles & Permissions. Seeding runs as a migration step so the database and the matrix stay in sync.
- Role and permission rows are not edited at runtime through the API; changes go through migrations.
- Deleting a role or permission is blocked while assignments exist.

### Entity Relationships

```
┌──────────────────────────────────────────────────────────────┐
│                      UserIdentity                            │
├──────────────────────────────────────────────────────────────┤
│ • Id (PK): Guid                                              │
│ • TenantId: Guid (Multi-tenant isolation)                   │
│ • Email: string                                              │
│ • CreatedAt: DateTime                                        │
│ • UpdatedAt: DateTime                                        │
│ • IsActive: bool                                             │
└────────────┬─────────────────────────────────────────────────┘
			 │
			 │ (1:1)
			 │
┌────────────▼─────────────────────────────────────────────────┐
│                       Credential                             │
├──────────────────────────────────────────────────────────────┤
│ • Id (PK): Guid                                              │
│ • UserId (FK): Guid                                          │
│ • PasswordHash: string (PBKDF2 hash)                         │
│ • PasswordSalt: string (Random salt)                         │
│ • CreatedAt: DateTime                                        │
│ • UpdatedAt: DateTime                                        │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                    ExternalIdentity                          │
├──────────────────────────────────────────────────────────────┤
│ • Id (PK): Guid                                              │
│ • UserId (FK): Guid                                          │
│ • Provider: string (e.g., "Google", "Microsoft")            │
│ • ExternalId: string (Provider's user ID)                   │
│ • EmailFromProvider: string                                  │
│ • CreatedAt: DateTime                                        │
│ • UpdatedAt: DateTime                                        │
└──────────────────────────────────────────────────────────────┘
```

---

## Database Configuration

### Connection String
- Configured via `appsettings.json`
- Uses SQL Server provider (Microsoft.EntityFrameworkCore.SqlServer)
- Supports encrypted connection strings

### Migrations Strategy
- Code-first approach using EF Core migrations
- Initial migration: `20260618224016_InitialCreate`
- Role and permission migration: `AddRolesGuestAndProfile` (creates `UserRole`; will be replaced by the `Role`, `Permission`, `UserRole`, and `RolePermission` schema described under Data Model)
- Run migrations: `Update-Database` (Package Manager Console)
- Migrations stored in `Persistence/Migrations/` directory

---

## Security Considerations

### 1. **Password Security**
- ✅ PBKDF2 with SHA256 algorithm
- ✅ 128-bit random salt per password
- ✅ 100,000 iterations (computational cost)
- ✅ No plaintext storage
- ✅ Constant-time comparison recommended for verification

### 2. **Token Security**
- ✅ HMAC SHA256 signing
- ✅ Symmetric key stored securely (configuration/environment)
- ✅ Claims include user ID and tenant ID
- ✅ Configurable expiration
- ⚠️ Ensure Jwt:Secret is strong (minimum 32 characters recommended)
- ⚠️ Rotate secrets regularly in production

### 3. **Data Protection**
- ✅ Multi-tenant isolation by TenantId
- ✅ Repository enforces tenant filtering
- ⚠️ SQL Server Transparent Data Encryption (TDE) recommended
- ⚠️ Connection string should use integrated authentication in internal networks

### 4. **External Provider Security**
- ✅ HTTP client for secure communication
- ✅ Client secrets injected via constructor
- ⚠️ Use HTTPS-only for OAuth endpoints
- ⚠️ Validate redirect URIs to prevent open redirect attacks

---

## Async/Await Pattern

All data access methods use async/await:
- Prevents thread pool starvation
- Improves scalability
- Non-blocking I/O operations
- Proper use of `SaveChangesAsync()` for database commits

Example:
```csharp
public Task<UserIdentity?> GetUserByIdAsync(Guid userId) 
	=> _context.User.FindAsync(userId).AsTask();
```

---

## Configuration Management

### Required appsettings.json Entries

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=.;Database=IdentityDb;Trusted_Connection=true;"
  },
  "Jwt": {
	"Secret": "your-very-long-secret-key-at-least-32-characters",
	"Issuer": "InsurancePortal.IdentityService",
	"Audience": "InsurancePortalClients",
	"ExpireMinutes": 60
  },
  "OAuth": {
	"Google": {
	  "ClientId": "xxx.apps.googleusercontent.com",
	  "ClientSecret": "your-client-secret"
	},
	"Microsoft": {
	  "ClientId": "azure-app-id-guid",
	  "ClientSecret": "azure-client-secret"
	}
  }
}
```

---

## Dependency Graph

```
JwtTokenService
	↓
IConfiguration (from Microsoft.Extensions.Configuration)

PassowordHasher
	↓
System.Security.Cryptography (BCL)

UserRepository
	↓
IdentityDbContext
	↓
DbSet<UserIdentity>, DbSet<Credential>, DbSet<ExternalIdentity>
	↓
Microsoft.EntityFrameworkCore

OAuthProviders
	↓
HttpClient
	↓
System.Net.Http (BCL)
```

---

## Extension Points

### Adding New OAuth Providers
1. Inherit from `OAuthProviderBase`
2. Implement abstract methods:
   - `GenerateAuthorizationUrl()`
   - Code exchange logic
   - User profile retrieval
3. Register in DI container in Application layer

### Adding New Security Algorithms
1. Create new class implementing `IPasswordHasher`
2. Implement `Hash()` and `Verify()` methods
3. Register in DI container with strategy selection

### Adding New Entity Types
1. Create entity in Domain project
2. Create `IEntityTypeConfiguration<T>` in `Persistence/Configuration/`
3. Add DbSet to `IdentityDbContext`
4. Create migration: `Add-Migration <MigrationName>`
5. Implement repository pattern if needed

---

## Common Operations & Workflows

### User Registration Flow
```
1. Receive login credentials in API
2. Call IPasswordHasher.Hash(password) → get hash + salt
3. Create UserIdentity and Credential entities
4. Call IUserRepository.AddUserAsync(user)
5. IUserRepository calls DbContext.SaveChangesAsync()
6. Database writes UserIdentity and Credential records
```

### Authentication & Token Generation Flow
```
1. Receive email/password in API
2. Call IUserRepository.GetUserByEmailAsync(tenantId, email)
3. Call IPasswordHasher.Verify(password, storedHash)
4. If valid, call ITokenService.GenerateToken(user)
5. JwtTokenService creates token with claims
6. Return token to client
```

### OAuth Sign-In Flow
```
1. Get authorization code from OAuth provider callback
2. Instantiate OAuth provider implementation
3. Call ExchangeCodeForToken(code, redirectUri)
4. Retrieve user profile from provider
5. Call IUserRepository.GetUserByEmail() or create new user
6. Create ExternalIdentity record linking provider account
7. Generate JWT token using ITokenService
8. Return token to client
```

---

## Performance Considerations

### Optimization Strategies
1. **Database Queries**
   - Use `FirstOrDefaultAsync()` instead of `ToListAsync()` where possible
   - Lazy loading: Consider explicit includes if N+1 queries detected
   - Index on TenantId and Email columns recommended

2. **Token Generation**
   - Cache JWT secret key once (current implementation parses each time)
   - Consider token refresh strategy for long-lived sessions

3. **Password Hashing**
   - 100,000 iterations: CPU-intensive operation
   - Consider background processing for bulk user imports
   - Run on thread pool thread (already async)

### Monitoring
- Log slow queries (>500ms)
- Monitor token generation requests
- Track failed authentication attempts (security & performance)

---

## Testing Strategy

### Unit Test Recommendations
1. **UserRepository Tests**
   - Mock DbContext
   - Test CRUD operations
   - Test filtering by TenantId

2. **JwtTokenService Tests**
   - Mock IConfiguration
   - Verify token structure
   - Verify claim contents
   - Test expiration calculation

3. **PassowordHasher Tests**
   - Test hash generation uniqueness (different salts)
   - Test verification logic
   - Test edge cases (empty strings, null)
   - Performance: ensure iterations are applied

4. **OAuthProvider Tests**
   - Mock HttpClient
   - Test authorization URL generation
   - Test code exchange flow
   - Test error handling

### Integration Test Recommendations
1. Use test database (SQLite in-memory recommended)
2. Test end-to-end registration → authentication flow
3. Test multi-tenancy isolation

---

## Troubleshooting Guide

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| `DbContext not registered` | Missing DI configuration | Register in API's Program.cs: `services.AddDbContext<IdentityDbContext>()` |
| `JWT validation fails` | Secret key mismatch | Ensure `Jwt:Secret` matches between token generation and validation (API layer) |
| `Migration fails` | Connection string invalid | Verify `ConnectionStrings:DefaultConnection` in appsettings.json |
| `Multi-tenant data leak` | Missing TenantId filter | Always include TenantId in WHERE clause for user queries |
| `Password verification fails` | Salt/hash mismatch | Ensure both are stored correctly; verify Pbkdf2 iterations match |
| `OAuth redirect fails` | Mismatched redirect URI | Ensure configured redirect URI in OAuth provider matches API callback endpoint |

---

## Deployment Checklist

- [ ] Database migrations applied to production
- [ ] `Jwt:Secret` set to strong, random value (>32 chars)
- [ ] Connection string uses production database
- [ ] OAuth credentials configured for production environment
- [ ] SQL Server backups enabled
- [ ] Connection encryption enabled (Encrypt=true)
- [ ] Monitor failed authentication attempts
- [ ] Set up alerts for high token generation latency
- [ ] Review and rotate secrets quarterly

---

## Unit Testing Infrastructure

### Overview

The **InsurancePortal.IdentityService.UnitTest** project provides comprehensive unit test coverage for the AuthController and related components. Tests are written using **xUnit** framework with **Moq** for dependency mocking and **FluentAssertions** for readable assertions.

### Project Structure

```
InsurancePortal.IdentityService.UnitTest/
├── Controllers/
│   ├── AuthControllerRegisterTests.cs           (Register endpoint tests)
│   ├── AuthControllerLoginTests.cs              (Login endpoint tests)
│   ├── AuthControllerInitiateExternalLoginTests.cs (OAuth initiation tests)
│   ├── AuthControllerExternalLoginCallbackTests.cs (OAuth callback tests)
│   └── AuthControllerGetExternalProvidersTests.cs  (Provider enumeration tests)
├── Fixtures/
│   ├── AuthControllerTestBase.cs                (Base test class with mocks)
│   └── TestDataFactory.cs                       (Mock data generation)
└── InsurancePortal.IdentityService.UnitTest.csproj
```

### Test Framework Stack

| Component | Package | Version | Purpose |
|-----------|---------|---------|---------|
| Test Runner | xunit | 2.8.1 | Test execution and discovery |
| Visual Studio Integration | xunit.runner.visualstudio | 2.8.1 | IDE integration |
| SDK | Microsoft.NET.Test.Sdk | 17.11.1 | Test infrastructure |
| Mocking | Moq | 4.20.70 | Dependency mocking |
| Assertions | FluentAssertions | 6.12.0 | Readable test assertions |
| ASP.NET Core | Microsoft.AspNetCore.Mvc.* | 2.2.0 | Controller testing |

### Test Classes & Coverage

#### 1. **AuthControllerRegisterTests** (6 tests)

**Positive Tests:**
- ✅ `Register_WithValidCredentials_ShouldReturnOk` - Successful user registration
- ✅ `Register_WithDifferentValidEmails_ShouldReturnOk` - Multiple email format support (Theory test)

**Negative Tests:**
- ❌ `Register_WithDuplicateEmail_ShouldReturnBadRequest` - Prevents duplicate accounts
- ❌ `Register_WithNullEmail_ShouldReturnBadRequest` - Validates null email
- ❌ `Register_WithEmptyEmail_ShouldReturnBadRequest` - Validates empty email
- ❌ `Register_WithEmptyPassword_ShouldReturnBadRequest` - Validates empty password
- ❌ `Register_WhenRepositoryThrows_ShouldReturnBadRequest` - Exception handling

**Mocks Used:**
- `IUserRepository` - For user lookup and creation
- `IPasswordHasher` - For password hashing operations
- `IPasswordHasher` exception scenarios

#### 2. **AuthControllerLoginTests** (8 tests)

**Positive Tests:**
- ✅ `Login_WithValidCredentials_ShouldReturnOkWithToken` - Successful authentication
- ✅ `Login_WithDifferentValidEmails_ShouldReturnOkWithToken` - Multiple email formats (Theory test)

**Negative Tests:**
- ❌ `Login_WithNonExistentEmail_ShouldReturnUnauthorized` - User not found
- ❌ `Login_WithInvalidPassword_ShouldReturnUnauthorized` - Wrong password
- ❌ `Login_WithEmptyEmail_ShouldReturnUnauthorized` - Empty email validation
- ❌ `Login_WithEmptyPassword_ShouldReturnUnauthorized` - Empty password validation
- ❌ `Login_WhenUserHasNoCredential_ShouldReturnUnauthorized` - Missing credential record
- ❌ `Login_WithInactiveUser_ShouldReturnUnauthorized` - Inactive user check
- ❌ `Login_WhenRepositoryThrows_ShouldReturnUnauthorized` - Exception handling

**Mocks Used:**
- `IUserRepository` - User lookup, credential retrieval
- `IPasswordHasher` - Password verification
- `ITokenService` - JWT token generation

#### 3. **AuthControllerInitiateExternalLoginTests** (8 tests)

**Positive Tests:**
- ✅ `InitiateExternalLogin_WithGoogleProvider_ShouldReturnOkWithAuthUrl` - Google OAuth flow
- ✅ `InitiateExternalLogin_WithGitHubProvider_ShouldReturnOkWithAuthUrl` - GitHub OAuth flow
- ✅ `InitiateExternalLogin_WithMicrosoftProvider_ShouldReturnOkWithAuthUrl` - Microsoft OAuth flow

**Negative Tests:**
- ❌ `InitiateExternalLogin_WithUnsupportedProvider_ShouldReturnBadRequest` - Invalid provider
- ❌ `InitiateExternalLogin_WithNullProviderName_ShouldReturnBadRequest` - Null provider name
- ❌ `InitiateExternalLogin_WithUnconfiguredProvider_ShouldReturnBadRequest` - Provider not initialized
- ❌ `InitiateExternalLogin_WhenProviderThrows_ShouldReturnBadRequest` - Provider error handling
- ❌ `InitiateExternalLogin_WithEmptyRedirectUri_ShouldHandleGracefully` - Empty URI handling

**Mocks Used:**
- `GoogleOAuthProvider` - Google authorization URL generation
- `GitHubOAuthProvider` - GitHub authorization URL generation
- `MicrosoftOAuthProvider` - Microsoft authorization URL generation

---

---

## Critical Configuration Fix

### Issue Resolved: "No authentication handlers are registered"

**Problem**: If `JwtSettings:SecretKey` was missing from `appsettings.json`, the authentication handler would not be registered, causing:
```
InvalidOperationException: No authentication handlers are registered. 
Did you forget to call AddAuthentication().Add[SomeAuthHandler]("Bearer")?
```

**Solution**: Authentication handler registration is now **mandatory** and fails fast with a clear error message:
```csharp
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException(
        "JwtSettings:SecretKey is required and must be configured in appsettings.json. " +
        "The secret key must be at least 32 characters long.");
}
```

**Why This Matters**:
- ✅ **Fast failure at startup** - Catch configuration errors immediately, not at runtime
- ✅ **Clear error message** - Developers know exactly what's missing
- ✅ **Consistent behavior** - Authentication is always available when HomeController uses `[Authorize]`
- ✅ **Production-safe** - Prevents mysterious 401 errors from missing configuration

---

## Complete Implementation Summary

### What Was Implemented

#### 1. Code Files
| File | Purpose | Location |
|------|---------|----------|
| `HomeController.cs` | Protected endpoint demonstrating Bearer token auth | `Api/Controllers/` |
| `HomeControllerTests.cs` | Unit tests for HomeController | `UnitTest/Controllers/` |
| `Program.cs` (Modified) | JWT Bearer authentication configuration | `Api/` |
| `*.csproj` (Modified) | Added JWT Bearer NuGet package | `Api/` |

#### 2. JWT Authentication Pipeline
```
appsettings.json (JwtSettings)
    ↓
Program.cs (AddAuthentication + AddJwtBearer)
    ↓
Middleware Layer (UseAuthentication + UseAuthorization)
    ↓
Controller Action ([Authorize(AuthenticationSchemes = "Bearer")])
    ↓
HomeController.Index() (Access User.Claims)
```

#### 3. Security Layers
1. **Token Signature Verification** - HMAC-SHA256 using secret key
2. **Expiration Validation** - Check token not expired (zero clock skew)
3. **Issuer Validation** - Verify issuer matches configuration
4. **Audience Validation** - Verify audience matches configuration
5. **Authorization Check** - Enforce [Authorize] attributes
6. **Claims Extraction** - Build identity from token claims

### Architecture Integration

**Layer Architecture**:
```
Presentation Layer (API)
├─ HomeController (protected endpoint)
└─ AuthController (token generation)

↓ Depends On

Business Logic Layer (Application)
├─ ITokenService (token generation interface)
├─ IUserRepository (user data interface)
├─ IPasswordHasher (password operations interface)
└─ DTOs & Handlers

↓ Implements

Infrastructure Layer (THIS PROJECT)
├─ JwtTokenService (JWT generation)
├─ PasswordHasher (PBKDF2 hashing)
├─ UserRepository (database access)
├─ Authentication Middleware (Program.cs)
└─ HomeController (example protected endpoint)

↓ Uses

Domain Layer
├─ UserIdentity (user entity)
├─ Credential (password storage)
└─ ExternalIdentity (OAuth identities)
```

### Configuration Files

**appsettings.json Required Section**:
```json
{
  "JwtSettings": {
    "SecretKey": "your-32-character-minimum-secret-key-here",
    "Issuer": "InsurancePortalIdentityService",
    "Audience": "InsurancePortalClient",
    "ExpirationMinutes": 60
  }
}
```

### Testing Coverage

| Test Suite | Tests | Status |
|-----------|-------|--------|
| HomeControllerTests | 2 | ✅ Passing |
| AuthControllerRegisterTests | 6 | ✅ Passing |
| AuthControllerLoginTests | 8 | ✅ Passing |
| AuthControllerExternalLoginTests | 8 | ✅ Passing |
| **Total** | **24** | **✅ All Passing** |

### Files Created/Modified Summary

| File | Type | Change |
|------|------|--------|
| `HomeController.cs` | Code | ✨ Created |
| `HomeControllerTests.cs` | Test | ✨ Created |
| `Program.cs` | Code | 🔄 Modified |
| `Api.csproj` | Config | 🔄 Modified |
| `ARCHITECTURE.md` | Docs | 🔄 Updated |

### Security Features

✅ **Authentication**
- Bearer token authentication using JWT
- HMAC-SHA256 signature verification
- Automatic token validation

✅ **Authorization**
- [Authorize] attribute support
- Claims-based authorization
- Role-based access control ready

✅ **Validation**
- Token signature verification
- Expiration checking with zero clock skew
- Issuer and audience validation
- Manual token key validation

✅ **Error Handling**
- 401 Unauthorized for invalid tokens
- Proper HTTP status codes
- Detailed error messages in logs

### Quick Reference

**To Add Protected Endpoint**:
```csharp
[HttpGet("endpoint")]
[Authorize(AuthenticationSchemes = "Bearer")]
public IActionResult ProtectedAction()
{
    var userEmail = User.FindFirst("email")?.Value;
    return Ok(userEmail);
}
```

**To Get Bearer Token**:
```bash
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password123"
}
```

**To Call Protected Endpoint**:
```bash
GET /api/home/index
Authorization: Bearer {token}
```

### Performance Considerations

- ✅ Stateless authentication (no session required)
- ✅ Asymmetric workload (validation faster than generation)
- ✅ Configurable expiration (balance security vs user experience)
- ✅ Scalable across multiple servers (no server state)

### Maintenance & Support

**To Update JWT Configuration**:
1. Modify `JwtSettings` in `appsettings.json`
2. No code changes required
3. Restart application to apply changes

**To Rotate Secret Key**:
1. Update `SecretKey` in `appsettings.json`
2. Existing tokens with old key will fail validation
3. Users must re-authenticate

**To Change Token Expiration**:
1. Modify `ExpirationMinutes` in `appsettings.json`
2. Affects new tokens only
3. Existing tokens remain valid until their expiration

**To Add Roles/Claims**:
1. Modify token payload in `JwtTokenService`
2. Add claims when generating token
3. Use `[Authorize(Roles = "Admin")]` on controllers

### Next Steps for Production

1. **Generate Strong Secret Key**
   ```bash
   # Generate 64-character key
   openssl rand -base64 48
   ```

2. **Store in Azure Key Vault**
   ```
   Add configuration provider for Key Vault
   Reference secret from vault in Program.cs
   ```

3. **Enable HTTPS Only**
   ```
   Use app.UseHttpsRedirection() in production
   Configure SSL certificate
   ```

4. **Implement Refresh Tokens**
   ```
   Add refresh token endpoint
   Rotate tokens periodically
   ```

5. **Add Token Logging**
   ```
   Log authentication events
   Monitor for suspicious patterns
   ```

---

## Summary

The Infrastructure layer now provides complete Bearer token authentication using JWT. The `HomeController` demonstrates how to use the `[Authorize(AuthenticationSchemes = "Bearer")]` attribute to protect endpoints. All configuration is centralized in `appsettings.json` under `JwtSettings`, making it easy to manage across development, testing, and production environments.

The implementation follows ASP.NET Core best practices, integrates seamlessly with the existing Clean Architecture, and provides a secure, scalable foundation for authentication across the InsurancePortal.IdentityService API.


#### 4. **AuthControllerExternalLoginCallbackTests** (10 tests)

**Positive Tests:**
- ✅ `ExternalLoginCallback_WithValidGoogleCodeForExistingUser_ShouldReturnOkWithToken` - Existing user Google
- ✅ `ExternalLoginCallback_WithValidCodeForNewUser_ShouldReturnOkWithTokenAndNewUserFlag` - New user creation
- ✅ `ExternalLoginCallback_WithValidMicrosoftCode_ShouldReturnOkWithToken` - Microsoft provider flow

**Negative Tests:**
- ❌ `ExternalLoginCallback_WithMissingCode_ShouldReturnBadRequest` - Empty code validation
- ❌ `ExternalLoginCallback_WithNullCode_ShouldReturnBadRequest` - Null code handling
- ❌ `ExternalLoginCallback_WithUnsupportedProvider_ShouldReturnBadRequest` - Invalid provider
- ❌ `ExternalLoginCallback_WithUnconfiguredProvider_ShouldReturnBadRequest` - Provider not configured
- ❌ `ExternalLoginCallback_WhenProviderFailsToGetUserInfo_ShouldReturnBadRequest` - User info retrieval failure
- ❌ `ExternalLoginCallback_WhenProviderThrowsException_ShouldReturnBadRequest` - Provider exception
- ❌ `ExternalLoginCallback_WhenExternalAuthHandlerFails_ShouldReturnBadRequest` - Handler failure
- ❌ `ExternalLoginCallback_WhenHandlerReturnsNullToken_ShouldReturnBadRequest` - Null token response

**Mocks Used:**
- `IUserRepository` - Email lookup, user creation
- `ITokenService` - JWT token generation
- OAuth Providers - User info retrieval

#### 5. **AuthControllerGetExternalProvidersTests** (8 tests)

**Positive Tests:**
- ✅ `GetExternalProviders_WithAllProvidersConfigured_ShouldReturnOkWithAllProviders` - All enabled
- ✅ `GetExternalProviders_WithOnlyGoogleConfigured_ShouldReturnOnlyGoogle` - Google only
- ✅ `GetExternalProviders_WithOnlyGitHubConfigured_ShouldReturnOnlyGitHub` - GitHub only
- ✅ `GetExternalProviders_WithOnlyMicrosoftConfigured_ShouldReturnOnlyMicrosoft` - Microsoft only
- ✅ `GetExternalProviders_WithGoogleAndGitHubConfigured_ShouldReturnBoth` - Multiple providers

**Negative Tests:**
- ❌ `GetExternalProviders_WithNoProvidersConfigured_ShouldReturnEmptyList` - No providers
- ❌ `GetExternalProviders_ShouldNeverReturn4xxError` - Always succeeds (defensive test)

**Mocks Used:**
- OAuth Provider instances - Null/non-null provider configurations

### Mock Data Factory

**TestDataFactory.cs** provides reusable mock data generation:

#### User & Entity Data
```csharp
// Create valid/invalid users
TestDataFactory.CreateValidUser()
TestDataFactory.CreateInactiveUser()

// Create credentials
TestDataFactory.CreateValidCredential(userId)

// Create external identities
TestDataFactory.CreateValidExternalIdentity(userId)
```

#### DTO Data
```csharp
// Registration
TestDataFactory.CreateValidRegisterDto()
TestDataFactory.CreateInvalidRegisterDto_EmptyEmail()
TestDataFactory.CreateInvalidRegisterDto_WeakPassword()

// Login
TestDataFactory.CreateValidLoginDto()
TestDataFactory.CreateInvalidLoginDto_WrongPassword()

// External Login
TestDataFactory.CreateValidExternalLoginRequestDto()
TestDataFactory.CreateExternalLoginRequestDto_InvalidProvider()
```

#### Response Data
```csharp
TestDataFactory.CreatePasswordHashResult()
TestDataFactory.CreateValidExternalUserInfoDto()
TestDataFactory.CreateValidExternalLoginResponseDto()
TestDataFactory.CreateFailedExternalLoginResponseDto()
```

### Base Test Class

**AuthControllerTestBase.cs** provides:

**Mock Dependencies:**
```csharp
protected Mock<IUserRepository> MockUserRepository
protected Mock<ITokenService> MockTokenService
protected Mock<IPasswordHasher> MockPasswordHasher
protected Mock<IHttpClientFactory> MockHttpClientFactory
protected Mock<IConfiguration> MockConfiguration
protected Mock<GoogleOAuthProvider> MockGoogleProvider
protected Mock<GitHubOAuthProvider> MockGitHubProvider
protected Mock<MicrosoftOAuthProvider> MockMicrosoftProvider
```

**Helper Methods:**
```csharp
// Response assertions
AssertBadRequest(IActionResult result, string expectedErrorKeyword)
AssertUnauthorized(IActionResult result, string expectedErrorKeyword)
T AssertOkWithValue<T>(IActionResult result)
dynamic AssertOkResult(IActionResult result)
void AssertOkWithToken(IActionResult result)

// Default configuration setup
SetupDefaultConfiguration()
```

### Test Execution

#### Running All Tests
```powershell
dotnet test InsurancePortal.IdentityService.UnitTest/InsurancePortal.IdentityService.UnitTest.csproj
```

#### Running Specific Test Class
```powershell
dotnet test --filter "FullyQualifiedName~AuthControllerLoginTests"
```

#### Running Tests with Coverage
```powershell
dotnet test /p:CollectCoverage=true
```

#### Visual Studio Test Explorer
- Open Test Explorer (Ctrl+E, T)
- Build project
- View test hierarchy by class/method
- Run individual tests or all tests

### Test Results Summary

| Test Class | Total | Passing | Coverage |
|------------|-------|---------|----------|
| AuthControllerRegisterTests | 6 | 6 | Register endpoint |
| AuthControllerLoginTests | 8 | 8 | Login endpoint |
| AuthControllerInitiateExternalLoginTests | 8 | 8 | OAuth initiation |
| AuthControllerExternalLoginCallbackTests | 10 | 10 | OAuth callback |
| AuthControllerGetExternalProvidersTests | 8 | 8 | Provider enumeration |
| **TOTAL** | **40** | **40** | **AuthController, DTOs, Handlers** |

### AAA Pattern (Arrange-Act-Assert)

All tests follow the AAA pattern:

```csharp
[Fact]
public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
{
    // ARRANGE: Set up test data and mocks
    var loginDto = TestDataFactory.CreateValidLoginDto();
    var user = TestDataFactory.CreateValidUser();
    var token = "jwt_token";

    MockUserRepository.Setup(/* ... */).ReturnsAsync(user);
    MockPasswordHasher.Setup(/* ... */).Returns(true);
    MockTokenService.Setup(/* ... */).Returns(token);

    // ACT: Execute the code under test
    var result = await _controller.Login(loginDto);

    // ASSERT: Verify the results
    AssertOkWithToken(result);
    MockTokenService.Verify(ts => ts.GenerateToken(user), Times.Once);
}
```

### Best Practices Implemented

✅ **Isolation** - All dependencies mocked, no database access
✅ **Naming** - Clear test names describing behavior
✅ **AAA Pattern** - Consistent Arrange-Act-Assert structure
✅ **DRY** - Shared mock setup in base class, factory methods for data
✅ **Focus** - Each test verifies one behavior
✅ **Coverage** - Positive and negative cases for each endpoint
✅ **Documentation** - XML comments explaining test purpose
✅ **Maintainability** - Easy to add new tests using factory
✅ **Performance** - Fast unit tests with no external dependencies
✅ **Repeatability** - Deterministic results, idempotent operations

### Continuous Integration

Tests should run as part of CI/CD pipeline:

```yaml
# Example Azure Pipeline
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/InsurancePortal.IdentityService.UnitTest.csproj'
    arguments: '--configuration Release --logger trx'
```

### Common Issues & Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Tests not discovered | Wrong project format | Ensure `<IsTestProject>true</IsTestProject>` in csproj |
| Mock verification fails | Incorrect setup/call count | Verify `Times.Once`, `Times.Never` usage |
| Null reference in test | Missing mock setup | Add corresponding `.Setup()` call |
| Test timeout | Async deadlock | Use `await` on async operations |
| Flaky tests | Date/time dependencies | Use fixed DateTime or factory methods |

### Future Enhancements

- [ ] Integration tests with in-memory SQLite database
- [ ] Performance benchmarking tests
- [ ] OAuth provider mock servers (WireMock)
- [ ] Test coverage reporting (OpenCover, Coverlet)
- [ ] Load testing for authentication endpoints
- [ ] Security testing (OWASP Top 10)
- [ ] Contract testing for API responses

---

## Related Documentation

- **Domain Project**: Business entity definitions (UserIdentity, Credential, ExternalIdentity)
- **Application Project**: Interfaces (IUserRepository, ITokenService, IPasswordHasher)
- **.NET Entity Framework Documentation**: https://learn.microsoft.com/en-us/ef/core/
- **JWT Standards**: RFC 7519 (https://tools.ietf.org/html/rfc7519)
- **PBKDF2 Standard**: RFC 2898 (https://tools.ietf.org/html/rfc2898)
- **OAuth 2.0**: RFC 6749 (https://tools.ietf.org/html/rfc6749)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.3 | 2025 | Added Guest quote creation, Guest-to-Registered conversion, and CurrentUserContext design |
| 1.2 | 2025 | Added Roles & Permissions design (7 roles, 6 services, scoped permission model) |
| 1.1 | 2025 | Added comprehensive unit testing infrastructure documentation (40 tests for AuthController) |
| 1.0 | 2025 | Initial architecture documentation |

---

**Last Updated**: 2025
**Target Audience**: Developers, Architects, AI Systems
**Format**: Markdown (AI-optimized structure)
