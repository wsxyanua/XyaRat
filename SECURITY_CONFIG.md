# 🔐 XyaRat Security Configuration Guide

## ⚠️ IMPORTANT: Remove Hardcoded Credentials

This guide explains how to configure XyaRat securely without hardcoded credentials.

---

## 📋 Table of Contents

1. [WebPanel Configuration](#webpanel-configuration)
2. [Server Certificate Configuration](#server-certificate-configuration)
3. [Client Encryption Key](#client-encryption-key)
4. [Environment Variables](#environment-variables)
5. [Production Deployment](#production-deployment)

---

## 1. WebPanel Configuration

### Method 1: Environment Variables (Recommended)

Set these environment variables before running WebPanel:

**Linux/Mac:**
```bash
export DefaultAdmin__Username="youradmin"
export DefaultAdmin__Password="YourSecureP@ssw0rd!"
export DefaultAdmin__CreateIfNotExists="true"
export Jwt__Key="Your_Random_JWT_Secret_Key_Min_32_Characters_Long"
```

**Windows (PowerShell):**
```powershell
$env:DefaultAdmin__Username="youradmin"
$env:DefaultAdmin__Password="YourSecureP@ssw0rd!"
$env:DefaultAdmin__CreateIfNotExists="true"
$env:Jwt__Key="Your_Random_JWT_Secret_Key_Min_32_Characters_Long"
```

**Windows (CMD):**
```cmd
set DefaultAdmin__Username=youradmin
set DefaultAdmin__Password=YourSecureP@ssw0rd!
set DefaultAdmin__CreateIfNotExists=true
set Jwt__Key=Your_Random_JWT_Secret_Key_Min_32_Characters_Long
```

### Method 2: appsettings.Production.json

Edit `WebPanel/appsettings.Production.json`:

```json
{
  "Jwt": {
    "Key": "GENERATE_RANDOM_32_CHAR_STRING_HERE",
    "ExpiryMinutes": 240
  },
  "DefaultAdmin": {
    "Username": "youradmin",
    "Password": "YourSecureP@ssw0rd!",
    "CreateIfNotExists": true
  }
}
```

**Generate random JWT key:**
```bash
# Linux/Mac
openssl rand -base64 32

# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

### Method 3: User Secrets (Development)

```bash
cd WebPanel
dotnet user-secrets set "DefaultAdmin:Username" "admin"
dotnet user-secrets set "DefaultAdmin:Password" "YourP@ssw0rd"
dotnet user-secrets set "Jwt:Key" "your-secret-key-here"
```

---

## 2. Server Certificate Configuration

The server now uses **DPAPI** (Data Protection API) to securely store certificate passwords.

### Method 1: Environment Variable (Recommended for Production)

```bash
# Set certificate password via environment variable
export XYARAT_CERT_PASSWORD="YourCertificatePassword123!"
```

### Method 2: Auto-Generated (Development)

On first run, the server will:
1. Generate a random secure password (32 chars)
2. Encrypt it using DPAPI (Windows LocalMachine scope)
3. Store in: `%APPDATA%\XyaRat\Config\secure.dat`

**Location:**
- Windows: `C:\ProgramData\XyaRat\Config\secure.dat`
- The file is encrypted and can only be decrypted on the same machine

### Using Custom Certificate

1. Generate certificate with your password:
```bash
openssl req -x509 -newkey rsa:2048 -keyout server.key -out server.crt -days 365 -nodes
openssl pkcs12 -export -out server.pfx -inkey server.key -in server.crt -password pass:YourPassword
```

2. Place certificate:
   - Path: `Server/Certificates/server.pfx`
   - Set environment variable: `XYARAT_CERT_PASSWORD=YourPassword`

---

## 3. Client Encryption Key

### Current Status
⚠️ **WARNING:** Client still has hardcoded encryption key in Settings.cs

### Secure Configuration (TODO)

The client should receive the encryption key from the builder, not hardcoded.

**Recommended approach:**
1. Generate random key during build
2. Encrypt key with machine-specific data
3. Use key derivation function (KDF)

---

## 4. Environment Variables Reference

### WebPanel
| Variable | Description | Example |
|----------|-------------|---------|
| `DefaultAdmin__Username` | Admin username | `admin` |
| `DefaultAdmin__Password` | Admin password | `SecureP@ss123!` |
| `DefaultAdmin__CreateIfNotExists` | Create if no users | `true` |
| `Jwt__Key` | JWT signing key (32+ chars) | `RandomBase64String...` |
| `Jwt__ExpiryMinutes` | Token expiry | `240` |
| `RatServer__Host` | RAT server IP | `127.0.0.1` |
| `RatServer__Port` | RAT server port | `5656` |
| `RatServer__EnableTls` | Use TLS | `true` |

### Server
| Variable | Description | Example |
|----------|-------------|---------|
| `XYARAT_CERT_PASSWORD` | Certificate password | `CertPass123!` |
| `XYARAT_ENCRYPTION_KEY` | Encryption key | `RandomKey...` |

---

## 5. Production Deployment

### Checklist

#### WebPanel
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure `Jwt:Key` (32+ characters)
- [ ] Change `DefaultAdmin:Username` and `Password`
- [ ] Set `DefaultAdmin:CreateIfNotExists=false` after first run
- [ ] Enable HTTPS (configure certificates)
- [ ] Set `AllowedHosts` to your domain
- [ ] Use strong database password if using external DB

#### Server
- [ ] Set `XYARAT_CERT_PASSWORD` environment variable
- [ ] Use proper CA-signed certificate (not self-signed)
- [ ] Configure certificate pinning thumbprints
- [ ] Enable TLS/SSL
- [ ] Configure IP whitelist
- [ ] Set up monitoring and logging

#### Client
- [ ] Generate unique encryption key per build
- [ ] Configure certificate pinning
- [ ] Test anti-detection features
- [ ] Verify persistence methods
- [ ] Test reconnection logic

### Docker Deployment

```dockerfile
# docker-compose.yml
version: '3.8'
services:
  webpanel:
    image: xyarat-webpanel
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DefaultAdmin__Username=${ADMIN_USER}
      - DefaultAdmin__Password=${ADMIN_PASS}
      - Jwt__Key=${JWT_SECRET}
    env_file:
      - .env.production
    volumes:
      - webpanel-data:/app/data
    ports:
      - "5000:5000"
```

**.env.production:**
```bash
ADMIN_USER=youradmin
ADMIN_PASS=YourSecurePassword123!
JWT_SECRET=Your32CharOrMoreRandomJWTSecretKey
```

---

## 6. Security Best Practices

### Password Requirements
- Minimum 12 characters
- Mix of uppercase, lowercase, numbers, symbols
- No dictionary words
- Unique for each environment

### Key Management
- Use different keys for dev/staging/production
- Rotate keys regularly (every 90 days)
- Never commit keys to git
- Use environment variables or secret management tools
- Consider Azure Key Vault or AWS Secrets Manager

### Monitoring
- Enable logging to detect unauthorized access
- Monitor failed login attempts
- Set up alerts for suspicious activity
- Regular security audits

---

## 7. Troubleshooting

### "No users in database" Error

**Cause:** `DefaultAdmin:CreateIfNotExists` is false or not configured

**Solution:**
```bash
# Set environment variable
export DefaultAdmin__CreateIfNotExists="true"
export DefaultAdmin__Username="admin"
export DefaultAdmin__Password="NewPassword123!"

# Or edit appsettings.json
```

### "JWT Key not configured" Error

**Cause:** `Jwt:Key` is empty or too short

**Solution:**
```bash
# Generate random key
openssl rand -base64 32

# Set in environment
export Jwt__Key="<generated-key>"
```

### Certificate Not Loading

**Cause:** Certificate password incorrect or file not found

**Solution:**
```bash
# Check certificate exists
ls Server/Certificates/server.pfx

# Set password
export XYARAT_CERT_PASSWORD="YourPassword"

# Or let it auto-generate
# Delete: %APPDATA%\XyaRat\Config\secure.dat
# Restart server
```

---

## 8. Migration from Hardcoded Config

### Old Version (Hardcoded)
```csharp
// ❌ OLD - INSECURE
public static string Key = "qwqdanchun";
public static string Password = "admin123";
```

### New Version (Secure)
```csharp
// ✅ NEW - SECURE
var key = SecureConfig.GetEncryptionKey();
var password = _configuration["DefaultAdmin:Password"];
```

### Migration Steps

1. **Backup current configuration**
2. **Update appsettings.json** - Remove hardcoded values
3. **Set environment variables** - Configure secure credentials
4. **Test in development** - Verify everything works
5. **Deploy to production** - Use production credentials
6. **Verify** - Check logs for any errors

---

## 9. Additional Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [DPAPI Documentation](https://docs.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [Certificate Management](https://docs.microsoft.com/en-us/dotnet/standard/security/cryptography-model)

---

## 📞 Support

If you encounter issues:
1. Check logs in `WebPanel/logs/` directory
2. Verify environment variables are set correctly
3. Test with minimal configuration first
4. Open issue on GitHub with error details

---

**Last Updated:** November 28, 2025  
**Version:** 1.0.8
