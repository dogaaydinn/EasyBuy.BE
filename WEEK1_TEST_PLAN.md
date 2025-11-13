# Week 1 Authentication System - Test Plan

This document provides a comprehensive test plan for the Week 1 authentication and authorization implementation.

## Pre-Testing Setup

### 1. Database Setup
```bash
# Create database migration
dotnet ef migrations add InitialCreate \
  --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
  --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj

# Apply migration
dotnet ef database update \
  --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
  --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj
```

### 2. Configuration Verification
Verify `appsettings.json` contains:
- ✅ Valid PostgreSQL connection string
- ✅ JWT settings (SecretKey, Issuer, Audience, ExpiryMinutes)
- ✅ Identity settings (password requirements, lockout settings)

### 3. Start Application
```bash
cd Presentation/EasyBuy.WebAPI
dotnet run
```

Expected output:
- Application starts on https://localhost:7001
- Database seeding completes successfully
- Swagger UI available at https://localhost:7001/swagger

---

## Test Suite

### Test 1: User Registration

#### Test 1.1: Successful Registration
**Endpoint:** `POST /api/v1/auth/register`

**Request:**
```json
{
  "email": "newuser@test.com",
  "userName": "newuser",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "firstName": "Test",
  "lastName": "User",
  "phoneNumber": "+1234567890"
}
```

**Expected Response (200 OK):**
```json
{
  "isSuccess": true,
  "message": "Registration successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-token",
    "tokenExpiry": "2025-11-13T12:00:00Z",
    "user": {
      "id": "guid",
      "userName": "newuser",
      "email": "newuser@test.com",
      "firstName": "Test",
      "lastName": "User",
      "phoneNumber": "+1234567890",
      "roles": ["Customer"]
    }
  }
}
```

**Verification:**
- ✅ User created in database
- ✅ User assigned "Customer" role
- ✅ JWT token is valid and contains correct claims
- ✅ Refresh token stored in database
- ✅ Welcome email sent (if email service configured)

#### Test 1.2: Duplicate Email Registration
**Request:**
```json
{
  "email": "admin@easybuy.com",
  "userName": "testuser",
  "password": "Test@123456",
  "confirmPassword": "Test@123456"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "error": "User with this email already exists"
}
```

#### Test 1.3: Duplicate Username Registration
**Request:**
```json
{
  "email": "unique@test.com",
  "userName": "admin",
  "password": "Test@123456",
  "confirmPassword": "Test@123456"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "error": "User with this username already exists"
}
```

#### Test 1.4: Password Mismatch
**Request:**
```json
{
  "email": "test@test.com",
  "userName": "testuser",
  "password": "Test@123456",
  "confirmPassword": "Different@123456"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "error": "Validation failed",
  "errors": {
    "ConfirmPassword": ["Passwords must match"]
  }
}
```

#### Test 1.5: Weak Password
**Request:**
```json
{
  "email": "test@test.com",
  "userName": "testuser",
  "password": "weak",
  "confirmPassword": "weak"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "error": "Validation failed",
  "errors": {
    "Password": [
      "Password must be at least 8 characters",
      "Password must contain at least one uppercase letter",
      "Password must contain at least one digit",
      "Password must contain at least one special character"
    ]
  }
}
```

---

### Test 2: User Login

#### Test 2.1: Successful Login with Email
**Endpoint:** `POST /api/v1/auth/login`

**Request:**
```json
{
  "emailOrUsername": "admin@easybuy.com",
  "password": "Admin@123456"
}
```

**Expected Response (200 OK):**
```json
{
  "isSuccess": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-token",
    "tokenExpiry": "2025-11-13T12:00:00Z",
    "user": {
      "id": "guid",
      "userName": "admin",
      "email": "admin@easybuy.com",
      "firstName": "System",
      "lastName": "Administrator",
      "roles": ["Admin"]
    }
  }
}
```

#### Test 2.2: Successful Login with Username
**Request:**
```json
{
  "emailOrUsername": "admin",
  "password": "Admin@123456"
}
```

**Expected Response (200 OK):**
- Same as Test 2.1

#### Test 2.3: Invalid Credentials
**Request:**
```json
{
  "emailOrUsername": "admin@easybuy.com",
  "password": "WrongPassword@123"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "error": "Invalid email/username or password"
}
```

#### Test 2.4: Non-Existent User
**Request:**
```json
{
  "emailOrUsername": "nonexistent@test.com",
  "password": "Test@123456"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "error": "Invalid email/username or password"
}
```

#### Test 2.5: Account Lockout
**Setup:** Attempt login with wrong password 5 times

**Expected Behavior:**
- First 4 attempts: "Invalid email/username or password"
- 5th attempt: Account locked
- Login attempt after lockout: "Account is locked. Please try again later."

**Verification:**
- ✅ Account locked for 5 minutes (configurable)
- ✅ Successful login after lockout period expires

---

### Test 3: Token Refresh

#### Test 3.1: Successful Token Refresh
**Endpoint:** `POST /api/v1/auth/refresh-token`

**Setup:** Obtain refresh token from login

**Request:**
```json
{
  "refreshToken": "base64-encoded-refresh-token-from-login"
}
```

**Expected Response (200 OK):**
```json
{
  "isSuccess": true,
  "message": "Token refreshed successfully",
  "data": {
    "token": "new-jwt-token",
    "refreshToken": "new-refresh-token",
    "tokenExpiry": "2025-11-13T13:00:00Z",
    "user": {
      "id": "guid",
      "userName": "admin",
      "email": "admin@easybuy.com",
      "roles": ["Admin"]
    }
  }
}
```

**Verification:**
- ✅ Old refresh token marked as used in database
- ✅ New refresh token stored in database
- ✅ New JWT token is valid
- ✅ Old refresh token cannot be reused

#### Test 3.2: Reuse of Refresh Token
**Request:** Use the same refresh token from Test 3.1 again

**Expected Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "error": "Refresh token has already been used"
}
```

#### Test 3.3: Expired Refresh Token
**Setup:** Use a refresh token that has expired (>30 days old)

**Expected Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "error": "Refresh token has expired"
}
```

#### Test 3.4: Invalid Refresh Token
**Request:**
```json
{
  "refreshToken": "invalid-token"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "error": "Invalid refresh token"
}
```

---

### Test 4: Protected Endpoints

#### Test 4.1: Access Protected Endpoint with Valid Token
**Endpoint:** `GET /api/v1/auth/me`

**Headers:**
```
Authorization: Bearer {valid-jwt-token}
```

**Expected Response (200 OK):**
```json
{
  "userId": "guid",
  "email": "admin@easybuy.com",
  "userName": "admin",
  "roles": ["Admin"]
}
```

#### Test 4.2: Access Protected Endpoint without Token
**Endpoint:** `GET /api/v1/auth/me`

**Headers:** None

**Expected Response (401 Unauthorized)**

#### Test 4.3: Access Protected Endpoint with Expired Token
**Headers:**
```
Authorization: Bearer {expired-jwt-token}
```

**Expected Response (401 Unauthorized)**

#### Test 4.4: Access Protected Endpoint with Invalid Token
**Headers:**
```
Authorization: Bearer invalid-token
```

**Expected Response (401 Unauthorized)**

---

### Test 5: Role-Based Authorization

#### Test 5.1: Admin Access
**Setup:** Create endpoint with `[Authorize(Policy = "RequireAdminRole")]`

**Test:**
- ✅ Admin user can access
- ❌ Manager user cannot access (403 Forbidden)
- ❌ Customer user cannot access (403 Forbidden)

#### Test 5.2: Manager Access
**Setup:** Create endpoint with `[Authorize(Policy = "RequireManagerRole")]`

**Test:**
- ✅ Admin user can access
- ✅ Manager user can access
- ❌ Customer user cannot access (403 Forbidden)

#### Test 5.3: Customer Access
**Setup:** Create endpoint with `[Authorize(Policy = "RequireCustomerRole")]`

**Test:**
- ❌ Admin user cannot access (403 Forbidden)
- ❌ Manager user cannot access (403 Forbidden)
- ✅ Customer user can access

---

### Test 6: Database Seeding

#### Test 6.1: Roles Seeded
**Verification:**
```sql
SELECT * FROM "Roles";
```

**Expected:**
- Admin
- Manager
- Customer
- Vendor

#### Test 6.2: Users Seeded
**Verification:**
```sql
SELECT * FROM "Users";
```

**Expected:**
- admin@easybuy.com (Admin role)
- manager@easybuy.com (Manager role)
- customer@easybuy.com (Customer role)

#### Test 6.3: Categories Seeded
**Expected:**
- Electronics
- Clothing
- Books
- Home & Garden
- Sports & Outdoors

#### Test 6.4: Products Seeded
**Expected:** 7 sample products

#### Test 6.5: Coupons Seeded
**Expected:**
- WELCOME10
- SAVE20
- FREESHIP

---

### Test 7: Security Features

#### Test 7.1: Password Hashing
**Verification:**
- ✅ Passwords are hashed in database (not plain text)
- ✅ Password hash is different each time (salted)

#### Test 7.2: JWT Token Claims
**Verification:** Decode JWT token and verify claims:
- ✅ NameIdentifier (user ID)
- ✅ Email
- ✅ Name (username)
- ✅ Role claims
- ✅ JTI (unique token ID)
- ✅ Sub (subject = user ID)
- ✅ Exp (expiration time)

#### Test 7.3: Refresh Token Security
**Verification:**
- ✅ Refresh tokens are cryptographically random (64 bytes)
- ✅ Refresh tokens stored securely in database
- ✅ JTI linking between access and refresh tokens
- ✅ Token rotation on refresh (old token marked as used)

#### Test 7.4: Soft Delete
**Test:** Delete a user account

**Verification:**
- ✅ User marked as deleted (IsDeleted = true)
- ✅ User cannot login
- ✅ User data preserved in database

---

### Test 8: Error Handling

#### Test 8.1: Global Exception Handler
**Test:** Trigger an unhandled exception

**Expected:**
- ✅ Returns 500 Internal Server Error
- ✅ Error logged to Serilog
- ✅ Correlation ID included in response
- ✅ Detailed error in development, generic message in production

#### Test 8.2: Validation Errors
**Test:** Send invalid data to any endpoint

**Expected:**
- ✅ Returns 400 Bad Request
- ✅ Clear validation error messages
- ✅ Multiple validation errors returned as array

---

### Test 9: Performance Tests

#### Test 9.1: Token Generation Performance
**Test:** Register 100 users concurrently

**Expected:**
- ✅ All registrations complete successfully
- ✅ Average response time < 500ms
- ✅ No duplicate usernames/emails created

#### Test 9.2: Login Performance
**Test:** 1000 concurrent login requests

**Expected:**
- ✅ All logins complete successfully
- ✅ Average response time < 200ms
- ✅ Database connection pool handles load

---

### Test 10: Integration Tests

#### Test 10.1: Complete User Flow
**Scenario:**
1. Register new user
2. Login with new credentials
3. Access protected endpoint with token
4. Refresh token before expiry
5. Access protected endpoint with new token

**Expected:**
- ✅ All steps complete successfully
- ✅ User can perform authorized actions

#### Test 10.2: Cross-Origin Requests (CORS)
**Test:** Make request from allowed origin (http://localhost:3000)

**Expected:**
- ✅ Request succeeds
- ✅ CORS headers present in response

**Test:** Make request from disallowed origin

**Expected:**
- ❌ Request blocked by CORS policy

---

## Swagger UI Testing

### Access Swagger
Navigate to: `https://localhost:7001/swagger`

### Test Authentication in Swagger
1. Click "Authorize" button
2. Register a new user via `/api/v1/auth/register`
3. Copy the JWT token from response
4. Click "Authorize" again
5. Enter: `Bearer {your-token}`
6. Click "Authorize"
7. Test protected endpoint `/api/v1/auth/me`

**Expected:**
- ✅ Protected endpoints accessible after authorization
- ✅ Swagger shows lock icon on protected endpoints

---

## Postman Collection

Create Postman collection with:
- Environment variables for base URL and tokens
- Pre-request scripts to auto-refresh tokens
- All test scenarios above

---

## Troubleshooting

### Issue: "Cannot connect to database"
**Solution:**
- Verify PostgreSQL is running
- Check connection string in appsettings.json
- Ensure database exists or migrations create it

### Issue: "JWT token invalid"
**Solution:**
- Verify SecretKey in appsettings.json matches token generation
- Check token expiration time
- Ensure Issuer and Audience match configuration

### Issue: "Database seeding fails"
**Solution:**
- Check database connection
- Verify migrations are applied
- Review logs for specific error

---

## Success Criteria

Week 1 authentication is considered complete when:
- ✅ All registration scenarios pass
- ✅ All login scenarios pass
- ✅ Token refresh works correctly
- ✅ Role-based authorization works
- ✅ Database seeding completes
- ✅ All security features verified
- ✅ Error handling works properly
- ✅ Performance meets targets
- ✅ Integration tests pass

---

## Next Steps

After Week 1 testing is complete, proceed to:
- **Week 2:** Order and Basket Management implementation
- **Week 3:** Advanced features (reviews, wishlist, notifications)
- **Week 4:** Testing and quality assurance
