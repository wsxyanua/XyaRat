# ✅ BUILD CHECKLIST - XYARAT

**Mục đích:** Hướng dẫn build dự án XyaRat từ source code  
**Thời gian ước tính:** 15-20 phút (lần đầu)  
**Yêu cầu:** Windows 10/11, Visual Studio 2019/2022  

---

## 📋 CHECKLIST TỔNG QUAN

```
[ ] 1. Kiểm tra requirements
[ ] 2. Clone repository
[ ] 3. Restore NuGet packages
[ ] 4. Build solution
[ ] 5. Verify output
[ ] 6. Test functionality
[ ] 7. (Optional) Obfuscate code
```

---

## 1️⃣ KIỂM TRA REQUIREMENTS

### ✅ System Requirements

```
Windows OS:
[ ] Windows 10 (1809+) or Windows 11
[ ] 64-bit processor
[ ] 8GB RAM minimum (16GB recommended)
[ ] 10GB free disk space

Visual Studio:
[ ] Visual Studio 2019 or 2022 (Community/Pro/Enterprise)
[ ] Workload: .NET desktop development
[ ] Component: .NET Framework 4.0 targeting pack
[ ] Component: .NET Framework 4.6.1 SDK

.NET SDK:
[ ] .NET Framework 4.0 (Client)
[ ] .NET Framework 4.6.1 (Server)
[ ] .NET 9.0 SDK (optional, for WebPanel)

Optional:
[ ] Node.js 20 LTS (for WebPanel frontend)
[ ] Git for Windows
[ ] Windows Sandbox (for testing)
```

### ✅ Verify Installation

```powershell
# Check Visual Studio
Get-Command devenv.exe

# Check .NET Framework
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Version

# Check .NET SDK
dotnet --version

# Check Node.js (optional)
node --version
npm --version
```

**Expected output:**
```
✅ Visual Studio: 16.x or 17.x
✅ .NET Framework: 4.6.1 or higher
✅ .NET SDK: 9.0.x
✅ Node.js: 20.x.x
```

---

## 2️⃣ CLONE REPOSITORY

### Option A: Git Clone

```powershell
# Navigate to workspace
cd D:\

# Clone repository
git clone https://github.com/wsxyanua/XyaRat.git
cd XyaRat

# Verify structure
dir

Expected folders:
✅ Client/
✅ Server/
✅ Plugin/
✅ MessagePack/
✅ Tests/
✅ WebPanel/
✅ packages/
```

### Option B: Download ZIP

```
1. Go to: https://github.com/wsxyanua/XyaRat
2. Click "Code" → "Download ZIP"
3. Extract to: D:\XyaRat
4. Verify folder structure (same as above)
```

---

## 3️⃣ RESTORE NUGET PACKAGES

### Method 1: Visual Studio GUI

```
1. Open XyaRat.sln in Visual Studio
2. Wait for solution to load (~30 seconds)
3. Right-click on Solution 'XyaRat'
4. Select "Restore NuGet Packages"
5. Wait for completion (~2-5 minutes)

Progress:
[ ] Restoring packages for Server...
[ ] Restoring packages for Client...
[ ] Restoring packages for MessagePack...
[ ] Restoring packages for 18 Plugins...
[ ] All packages restored successfully
```

### Method 2: Command Line

```powershell
# Open Developer Command Prompt for VS
cd D:\XyaRat

# Restore packages
nuget restore XyaRat.sln

# OR using MSBuild
msbuild XyaRat.sln /t:Restore

Expected output:
✅ Restoring NuGet packages...
✅ All packages are already installed and there is nothing to restore.
```

### ✅ Verify Packages

```powershell
# Check packages folder
dir packages

Expected (40+ packages):
✅ AForge.2.2.5
✅ BouncyCastle.1.8.6.1
✅ dnlib.3.3.2
✅ FastColoredTextBox.2.16.24
✅ ILMerge.3.0.29
✅ Newtonsoft.Json.12.0.3
✅ System.Buffers.4.5.1
✅ ... and more
```

---

## 4️⃣ BUILD SOLUTION

### Method 1: Visual Studio GUI

```
1. Open XyaRat.sln
2. Select configuration: Release (top toolbar)
3. Select platform: Any CPU (top toolbar)
4. Press Ctrl+Shift+B or Build → Build Solution
5. Wait for build (~5-10 minutes first time)

Build Output Window should show:
========== Build: 24 succeeded, 0 failed ==========

Progress:
[ ] Building MessagePackLib...
[ ] Building Client...
[ ] Building Server...
[ ] Building 18 Plugins...
[ ] ILMerge Client dependencies...
[ ] Build succeeded
```

### Method 2: Command Line

```powershell
# Open Developer Command Prompt for VS
cd D:\XyaRat

# Clean solution (optional)
msbuild XyaRat.sln /t:Clean /p:Configuration=Release

# Build solution
msbuild XyaRat.sln /t:Build /p:Configuration=Release

# OR rebuild (clean + build)
msbuild XyaRat.sln /t:Rebuild /p:Configuration=Release

Expected output:
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:05:23.45
```

### ✅ Check for Errors

```
Common errors and solutions:

❌ Error: "MSBuild.ILMerge.Task not found"
   ✅ Solution: Restore NuGet packages first

❌ Error: "Framework 4.0 not found"
   ✅ Solution: Install via Visual Studio Installer
   
❌ Error: "Framework 4.6.1 not found"
   ✅ Solution: Install via Visual Studio Installer

❌ Error: "dnlib.dll could not be resolved"
   ✅ Solution: Delete packages folder and restore again

❌ Error: "ILMerge failed"
   ✅ Solution: Check ILMerge.props configuration
```

---

## 5️⃣ VERIFY OUTPUT

### ✅ Check Build Output

```powershell
# Navigate to output folder
cd D:\XyaRat\Binaries\Release

# List files
dir

Expected structure:
Binaries\Release\
├─ XyaRat.exe          ✅ (~2.5MB) - Server
├─ XyaRat.exe.config   ✅ - Config file
├─ *.dll               ✅ (~20 DLLs) - Dependencies
├─ Stub\
│  └─ Client.exe       ✅ (~40-50KB) - Agent
└─ Plugins\
   ├─ FileManager.dll  ✅
   ├─ RemoteDesktop.dll✅
   ├─ Keylogger.dll    ✅
   ├─ Recovery.dll     ✅
   └─ ... (18 DLLs total) ✅
```

### ✅ Verify File Sizes

```powershell
# Check Server
(Get-Item "XyaRat.exe").Length / 1MB
# Expected: ~2.5MB

# Check Client
(Get-Item "Stub\Client.exe").Length / 1KB
# Expected: ~40-50KB

# Check Plugins
(Get-ChildItem "Plugins\*.dll" | Measure-Object Length -Sum).Sum / 1MB
# Expected: ~5MB total
```

### ✅ Verify Dependencies

```powershell
# Check for missing DLLs
$requiredDlls = @(
    "BouncyCastle.Crypto.dll",
    "dnlib.dll",
    "FastColoredTextBox.dll",
    "Newtonsoft.Json.dll",
    "protobuf-net.dll",
    "Vestris.ResourceLib.dll"
)

foreach ($dll in $requiredDlls) {
    if (Test-Path $dll) {
        Write-Host "✅ $dll"
    } else {
        Write-Host "❌ $dll MISSING"
    }
}
```

---

## 6️⃣ TEST FUNCTIONALITY

### ✅ Quick Test - Server

```
1. Navigate to: D:\XyaRat\Binaries\Release
2. Double-click XyaRat.exe
3. Expected behavior:
   [ ] GUI opens successfully
   [ ] No error messages
   [ ] Main window displays client list
   [ ] Menu bar accessible
   [ ] Logs tab visible

4. Test basic functions:
   [ ] Connection → Ports → Can open ports dialog
   [ ] Builder → Can open builder dialog
   [ ] Help → About → Shows version 1.0.7
   
5. Close application:
   [ ] File → Exit or click X
   [ ] Application closes cleanly
```

### ✅ Quick Test - Client (DEBUG Mode)

```
⚠️ WARNING: Only test in isolated environment!

1. Copy Client.exe to test folder
2. Run in DEBUG mode (built with Debug config)
3. Expected behavior:
   [ ] Process starts (check Task Manager)
   [ ] No visible window (WinExe type)
   [ ] Connects to 127.0.0.1:8848 (DEBUG default)
   [ ] Appears in Server client list

4. Stop client:
   [ ] Kill process from Task Manager
   OR
   [ ] Use Server → Right-click client → Close
```

### ✅ Full Test - Windows Sandbox

```
Setup:
1. Enable Windows Sandbox:
   - Settings → Apps → Optional Features
   - Add "Windows Sandbox"
   - Restart PC

2. Start Sandbox:
   - Start Menu → Windows Sandbox

Test Steps:
1. Run Server on HOST machine:
   [ ] Start XyaRat.exe
   [ ] Connection → Ports → Start listening (port 5656)
   [ ] Note your IP: ipconfig

2. Copy Client to Sandbox:
   [ ] Drag & drop Client.exe to Sandbox desktop
   
3. Configure Client (using Builder):
   [ ] Builder → Host: [Your IP]
   [ ] Builder → Port: 5656
   [ ] Builder → Build
   [ ] Copy new Client.exe to Sandbox

4. Run Client in Sandbox:
   [ ] Double-click Client.exe in Sandbox
   [ ] Check Server: Client should appear in list
   [ ] Verify: IP, Country, OS info displayed

5. Test Features:
   [ ] File Manager: Browse files
   [ ] Remote Desktop: View screen
   [ ] Process Manager: List processes
   [ ] Shell: Execute commands

6. Cleanup:
   [ ] Close Sandbox (auto-cleanup)
   [ ] Stop Server
```

---

## 7️⃣ (OPTIONAL) OBFUSCATE CODE

### ⚠️ Why Obfuscate?

```
Problem: Client.exe easily reverse-engineered
Solution: Use obfuscation tool

Benefits:
✅ Harder to analyze
✅ String encryption
✅ Control flow obfuscation
✅ Anti-tampering
✅ Anti-debug (additional layer)

Drawback:
❌ May trigger AV detection
❌ Slower execution
❌ Larger file size
```

### Option 1: ConfuserEx (Free)

```
1. Download: https://github.com/mkaring/ConfuserEx/releases
2. Extract ConfuserEx
3. Run ConfuserEx.exe

Configuration:
1. Project → New Project
2. Base Directory: D:\XyaRat\Binaries\Release\Stub
3. Add Module → Client.exe
4. Settings (Tab):
   [ ] Anti Debug: Normal
   [ ] Anti Dump: Normal
   [ ] Anti ILDasm: Enable
   [ ] Anti Tamper: Normal
   [ ] Constants: Normal
   [ ] Control Flow: Normal
   [ ] Name Mangling: Enable
   [ ] Reference Proxy: Normal
   [ ] Resources: Normal

5. Protect → Start
6. Output: Confused\Client.exe

Verify:
[ ] File size increased (~20-30%)
[ ] Still runs correctly
[ ] Harder to decompile (test with dnSpy)
```

### Option 2: .NET Reactor (Commercial)

```
Features:
✅ NecroBit (IL to native code)
✅ Strong name removal
✅ Anti-tampering
✅ Licensing system
✅ Better obfuscation than ConfuserEx

Price: ~$180 (one-time)
Website: https://www.eziriz.com/

Recommended for production use.
```

---

## 🎯 TROUBLESHOOTING

### Build Failures

```
Issue: "Build failed with errors"
Check:
[ ] Read error messages carefully
[ ] Check Output window (View → Output)
[ ] Check Error List window (View → Error List)
[ ] Google specific error codes

Common fixes:
✅ Restore NuGet packages
✅ Clean solution and rebuild
✅ Delete bin/ and obj/ folders
✅ Restart Visual Studio
✅ Update Visual Studio to latest version
```

### Runtime Errors

```
Issue: "XyaRat.exe won't start"
Check:
[ ] .NET Framework 4.6.1 installed
[ ] All DLLs present in same folder
[ ] No antivirus blocking

Issue: "Client.exe crashes immediately"
Check:
[ ] Settings encrypted correctly
[ ] Certificate valid
[ ] Host/Port reachable
[ ] No anti-analysis detection (VM/Sandbox)

Issue: "Plugins not loading"
Check:
[ ] Plugins\ folder exists
[ ] All 18 DLLs present
[ ] DLLs not corrupted
[ ] Correct .NET Framework version
```

### Connection Issues

```
Issue: "Client can't connect to Server"
Check:
[ ] Server listening on correct port
[ ] Firewall allows connections
[ ] Host/Port configured correctly in Client
[ ] Network connectivity (ping test)
[ ] Certificate matches (Server ↔ Client)

Issue: "Client connects but immediately disconnects"
Check:
[ ] Certificate mismatch
[ ] SSL/TLS handshake failure
[ ] Server overloaded
[ ] Anti-virus blocking
```

---

## 📦 PACKAGING FOR DISTRIBUTION

### Create Release Package

```powershell
# Create release folder
New-Item -ItemType Directory -Path "D:\XyaRat-Release" -Force

# Copy Server
Copy-Item -Recurse "D:\XyaRat\Binaries\Release\*" -Destination "D:\XyaRat-Release\Server"

# Copy Client
Copy-Item "D:\XyaRat\Binaries\Release\Stub\Client.exe" -Destination "D:\XyaRat-Release\Client\"

# Copy Plugins
Copy-Item -Recurse "D:\XyaRat\Binaries\Release\Plugins" -Destination "D:\XyaRat-Release\Plugins"

# Copy Documentation
Copy-Item "D:\XyaRat\README.md" -Destination "D:\XyaRat-Release\"
Copy-Item "D:\XyaRat\USAGE.txt" -Destination "D:\XyaRat-Release\"
Copy-Item "D:\XyaRat\LICENSE" -Destination "D:\XyaRat-Release\"

# Create ZIP
Compress-Archive -Path "D:\XyaRat-Release\*" -DestinationPath "D:\XyaRat-v1.0.7.zip"
```

### Package Structure

```
XyaRat-v1.0.7.zip
├─ README.md           → Documentation
├─ USAGE.txt           → Quick start guide
├─ LICENSE             → MIT License
├─ Server\
│  ├─ XyaRat.exe       → Main server
│  ├─ *.dll            → Dependencies
│  └─ Plugins\         → 18 plugin DLLs
└─ Client\
   └─ Client.exe       → Agent stub
```

---

## ✅ FINAL CHECKLIST

```
Build Process:
[ ] ✅ Requirements installed
[ ] ✅ Repository cloned
[ ] ✅ NuGet packages restored
[ ] ✅ Solution built successfully
[ ] ✅ Output files verified
[ ] ✅ Server tested (runs OK)
[ ] ✅ Client tested (connects OK)
[ ] ✅ Plugins tested (load OK)

Optional:
[ ] ⚠️ Code obfuscated
[ ] ⚠️ Sandbox tested
[ ] ⚠️ AV detection checked
[ ] ⚠️ Release packaged

Documentation Read:
[ ] 📖 README.md
[ ] 📖 USAGE.txt
[ ] 📖 PROJECT_ANALYSIS_REPORT.md
[ ] 📖 FIX_SUMMARY.md
[ ] 📖 TECHNICAL_DEBT_ANALYSIS.md

Legal:
[ ] ⚖️ Read LICENSE
[ ] ⚖️ Understand legal disclaimer
[ ] ⚖️ Authorized testing only
[ ] ⚖️ Educational purpose compliance
```

---

## 🎓 NEXT STEPS

### For Developers

```
1. Read documentation thoroughly
2. Understand architecture (CLIENT ↔ SERVER)
3. Review security features
4. Test in isolated environment only
5. Study anti-detection mechanisms
6. Learn from code structure
7. Contribute improvements (GitHub)
```

### For Security Researchers

```
1. Set up test lab
   - Isolated VM network
   - Windows Sandbox
   - No internet connection

2. Analyze behavior
   - Monitor with Process Monitor
   - Capture with Wireshark
   - Analyze with dnSpy

3. Test evasion
   - VM detection
   - Sandbox detection
   - AV detection

4. Improve defenses
   - Understand techniques
   - Build better defenses
   - Share knowledge responsibly
```

### For Students

```
1. Study architecture
   - Client/Server pattern
   - Plugin system design
   - Network protocols

2. Learn security concepts
   - Encryption (AES-256)
   - Certificate pinning
   - Traffic obfuscation

3. Understand persistence
   - Registry manipulation
   - Task Scheduler
   - WMI events

4. Practice ethical hacking
   - Get authorization
   - Test own systems
   - Follow responsible disclosure
```

---

## ⚠️ IMPORTANT REMINDERS

```
🔴 LEGAL WARNING:
- Use for EDUCATIONAL purposes ONLY
- Get proper AUTHORIZATION before testing
- Understand LAWS in your jurisdiction
- DO NOT use for malicious activities
- Author NOT responsible for misuse

🟠 SECURITY WARNING:
- Antivirus WILL detect this software
- Use in ISOLATED environment only
- DO NOT test on production systems
- DO NOT deploy on unauthorized systems
- Understand the RISKS involved

🟡 TECHNICAL WARNING:
- This is EDUCATIONAL software
- NOT production-ready without hardening
- Requires OBFUSCATION for deployment
- May contain BUGS (report on GitHub)
- Updates may BREAK compatibility
```

---

**Checklist Version:** 1.0  
**Last Updated:** 26 tháng 11, 2025  
**Maintainer:** GitHub Copilot AI  
**Support:** github.com/wsxyanua/XyaRat/issues  

**Status:** ✅ **BUILD PROCESS DOCUMENTED**

