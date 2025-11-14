# 🔐 Enterprise Secrets Management Guide

## Overview

EasyBuy.BE implements enterprise-grade secrets management following security best practices:

- **Development**: User Secrets (encrypted, stored outside source code)
- **Production**: Azure Key Vault (centralized, audited, RBAC-controlled)
- **Never commit secrets to source control**

## 📋 Table of Contents

1. [Development Setup (User Secrets)](#development-setup-user-secrets)
2. [Production Setup (Azure Key Vault)](#production-setup-azure-key-vault)
3. [CI/CD Pipeline Configuration](#cicd-pipeline-configuration)
4. [Secret Rotation](#secret-rotation)
5. [Troubleshooting](#troubleshooting)

---

## Development Setup (User Secrets)

### Prerequisites

- .NET 8.0 SDK installed
- User Secrets already initialized (UserSecretsId: `bac7034d-a6ec-4241-b029-8b8b74f98083`)

### Method 1: Visual Studio / Rider

1. Right-click on `EasyBuy.WebAPI` project
2. Select **"Manage User Secrets"**
3. Copy contents from `secrets.template.json` and update with your values
4. Save the file

### Method 2: Command Line

```bash
cd Presentation/EasyBuy.WebAPI

# Set individual secrets
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 32)"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=EasyBuyDB;Username=postgres;Password=YourPassword;"
dotnet user-secrets set "EmailSettings:SendGridApiKey" "SG.your-api-key"

# Or import entire file (after editing secrets.template.json)
cat secrets.template.json | dotnet user-secrets set
```

### Method 3: PowerShell Script

```powershell
# Generate secure JWT secret
$jwtSecret = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | % {[char]$_})

# Set secrets
dotnet user-secrets set "JwtSettings:SecretKey" $jwtSecret
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=EasyBuyDB;Username=postgres;Password=dev_password;"
```

### Verify User Secrets

```bash
dotnet user-secrets list
```

### User Secrets Location

- **Windows**: `%APPDATA%\Microsoft\UserSecrets\bac7034d-a6ec-4241-b029-8b8b74f98083\secrets.json`
- **macOS/Linux**: `~/.microsoft/usersecrets/bac7034d-a6ec-4241-b029-8b8b74f98083/secrets.json`

---

## Production Setup (Azure Key Vault)

### Step 1: Create Azure Key Vault

```bash
# Azure CLI
az login

# Create resource group
az group create --name easybuy-prod-rg --location eastus

# Create Key Vault
az keyvault create \
  --name easybuy-prod-kv \
  --resource-group easybuy-prod-rg \
  --location eastus \
  --enable-rbac-authorization true

# Get Key Vault URL
az keyvault show --name easybuy-prod-kv --query properties.vaultUri -o tsv
```

### Step 2: Add Secrets to Key Vault

**Important**: Azure Key Vault uses `--` (double dash) instead of `:` (colon) for hierarchical keys.

```bash
# JWT Secret
az keyvault secret set --vault-name easybuy-prod-kv \
  --name "JwtSettings--SecretKey" \
  --value "$(openssl rand -base64 32)"

# Database Connection
az keyvault secret set --vault-name easybuy-prod-kv \
  --name "ConnectionStrings--DefaultConnection" \
  --value "Host=prod-server;Port=5432;Database=EasyBuyDB;Username=prod_user;Password=secure_password;"

# Redis Connection
az keyvault secret set --vault-name easybuy-prod-kv \
  --name "ConnectionStrings--RedisConnection" \
  --value "prod-redis.redis.cache.windows.net:6380,password=redis_key,ssl=True,abortConnect=False"

# SendGrid API Key
az keyvault secret set --vault-name easybuy-prod-kv \
  --name "EmailSettings--SendGridApiKey" \
  --value "SG.your-sendgrid-api-key"

# Stripe Keys
az keyvault secret set --vault-name easybuy-prod-kv \
  --name "PaymentSettings--Stripe--SecretKey" \
  --value "sk_live_your-stripe-secret-key"

az keyvault secret set --vault-name easybuy-prod-kv \
  --name "PaymentSettings--Stripe--WebhookSecret" \
  --value "whsec_your-webhook-secret"

# Twilio
az keyvault secret set --vault-name easybuy-prod-kv \
  --name "SmsSettings--TwilioAccountSid" \
  --value "your-account-sid"

az keyvault secret set --vault-name easybuy-prod-kv \
  --name "SmsSettings--TwilioAuthToken" \
  --value "your-auth-token"
```

### Step 3: Configure Managed Identity

#### For Azure App Service

```bash
# Enable system-assigned managed identity
az webapp identity assign \
  --name easybuy-api \
  --resource-group easybuy-prod-rg

# Get the principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name easybuy-api \
  --resource-group easybuy-prod-rg \
  --query principalId -o tsv)

# Grant Key Vault access
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee $PRINCIPAL_ID \
  --scope /subscriptions/{subscription-id}/resourceGroups/easybuy-prod-rg/providers/Microsoft.KeyVault/vaults/easybuy-prod-kv
```

#### For Azure Kubernetes Service (AKS)

```bash
# Use Azure AD Workload Identity (recommended)
# Follow: https://learn.microsoft.com/en-us/azure/aks/workload-identity-overview
```

### Step 4: Configure Application

Update `appsettings.Production.json`:

```json
{
  "KeyVault": {
    "Url": "https://easybuy-prod-kv.vault.azure.net/"
  }
}
```

---

## CI/CD Pipeline Configuration

### GitHub Actions

```yaml
# .github/workflows/deploy-production.yml
env:
  AZURE_KEYVAULT_URL: ${{ secrets.AZURE_KEYVAULT_URL }}
  AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
  AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
  AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

steps:
  - name: Azure Login
    uses: azure/login@v1
    with:
      creds: ${{ secrets.AZURE_CREDENTIALS }}

  - name: Deploy to App Service
    run: |
      az webapp config appsettings set \
        --name easybuy-api \
        --resource-group easybuy-prod-rg \
        --settings \
        ASPNETCORE_ENVIRONMENT=Production \
        KeyVault__Url=${{ secrets.AZURE_KEYVAULT_URL }}
```

### Azure DevOps

```yaml
# azure-pipelines.yml
variables:
  - group: easybuy-production-secrets # Variable group linked to Key Vault

steps:
  - task: AzureWebApp@1
    inputs:
      azureSubscription: 'production-service-connection'
      appName: 'easybuy-api'
      appSettings: |
        -ASPNETCORE_ENVIRONMENT Production
        -KeyVault__Url $(KeyVaultUrl)
```

---

## Secret Rotation

### Best Practices

1. **Rotate secrets every 90 days** (JWT keys, API keys, database passwords)
2. **Use Azure Key Vault secret versions** for zero-downtime rotation
3. **Monitor secret expiration** with Azure Monitor alerts

### Rotation Process

```bash
# 1. Create new secret version
az keyvault secret set \
  --vault-name easybuy-prod-kv \
  --name "JwtSettings--SecretKey" \
  --value "$(openssl rand -base64 32)"

# 2. Application automatically picks up new version on restart
# 3. Verify application health
# 4. Disable old secret version after grace period

az keyvault secret set-attributes \
  --vault-name easybuy-prod-kv \
  --name "JwtSettings--SecretKey" \
  --version <old-version-id> \
  --enabled false
```

### Automated Rotation (Recommended)

```csharp
// TODO: Implement IHostedService for automatic secret refresh
// See: https://learn.microsoft.com/en-us/azure/key-vault/secrets/tutorial-rotation
```

---

## Troubleshooting

### Issue: "JWT SecretKey is not configured"

**Cause**: Secret not set in User Secrets or Key Vault

**Solution**:

```bash
# Development
dotnet user-secrets set "JwtSettings:SecretKey" "your-32-character-minimum-secret"

# Production - verify Key Vault secret
az keyvault secret show \
  --vault-name easybuy-prod-kv \
  --name "JwtSettings--SecretKey"
```

### Issue: "Managed Identity authentication failed"

**Cause**: App Service doesn't have permissions to Key Vault

**Solution**:

```bash
# Verify managed identity is enabled
az webapp identity show --name easybuy-api --resource-group easybuy-prod-rg

# Verify role assignment
az role assignment list \
  --assignee <principal-id> \
  --scope /subscriptions/{sub-id}/resourceGroups/easybuy-prod-rg/providers/Microsoft.KeyVault/vaults/easybuy-prod-kv
```

### Issue: Connection string not loading

**Cause**: Incorrect key naming (`:` vs `--`)

**Solution**: Azure Key Vault uses `--` for nested keys:

```
✅ ConnectionStrings--DefaultConnection
❌ ConnectionStrings:DefaultConnection
```

### Debug Key Vault Loading

Add to `Program.cs` (development only):

```csharp
if (builder.Environment.IsDevelopment())
{
    var config = builder.Configuration.AsEnumerable();
    foreach (var kvp in config)
    {
        Log.Debug("Config Key: {Key}", kvp.Key); // Don't log values!
    }
}
```

---

## Security Checklist

- [ ] **Never commit secrets to Git** (check `.gitignore` includes `appsettings.*.json` with secrets)
- [ ] **Use User Secrets for development** (never hardcode)
- [ ] **Use Key Vault for production** (with Managed Identity)
- [ ] **Enable Key Vault audit logging** (monitor access)
- [ ] **Implement secret rotation** (90-day policy)
- [ ] **Use minimum privilege** (grant only necessary permissions)
- [ ] **Enable soft-delete and purge protection** on Key Vault
- [ ] **Set Key Vault firewall rules** (restrict network access)
- [ ] **Use separate Key Vaults** for dev/staging/prod
- [ ] **Document all secrets** in secrets.template.json

---

## Additional Resources

- [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault Developer's Guide](https://learn.microsoft.com/en-us/azure/key-vault/general/developers-guide)
- [Managed Identities for Azure Resources](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)
- [Secret Rotation in Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/secrets/tutorial-rotation)

---

**Last Updated**: 2025-11-14
**Owner**: Infrastructure Team
**Review Cycle**: Quarterly
