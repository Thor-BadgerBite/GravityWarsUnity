# Unity Gaming Services Installation Guide

## Context Window Status
**Remaining: ~99,700 tokens out of 200,000**

---

## Overview

This guide will help you install the Unity Gaming Services packages needed for your online multiplayer features. After installation, all the networking code will compile properly.

## Current State

✅ **Working**: Hotseat mode (local 2-player)
✅ **Working**: All progression systems (account XP, ship XP, battle pass)
✅ **Working**: Save/load functionality
❌ **Not Working**: Online multiplayer (needs packages)

## Packages to Install

You need to install these Unity packages in the following order:

### 1. Core Services (Required First)
- **Unity Gaming Services Core** - Foundation for all services
- **Unity Authentication** - Player account authentication

### 2. Multiplayer Packages
- **Netcode for GameObjects** - Main networking framework
- **Unity Transport** (dependency - may auto-install)
- **Unity Relay** - NAT punch-through for peer-to-peer
- **Unity Lobby** - Matchmaking and lobby system

### 3. Optional Packages
- **Unity Analytics** - Player behavior tracking
- **Unity Cloud Save** - Server-side save storage
- **LeanTween** - UI animations (not a Unity package, see below)

---

## Step-by-Step Installation

### Method 1: Unity Package Manager (Recommended)

#### Step 1: Open Package Manager
1. Open Unity Editor
2. Go to **Window → Package Manager**
3. Click the **+** button in top-left corner
4. Select **Add package by name...**

#### Step 2: Install Core Services
Add these packages one at a time:

```
com.unity.services.core
```
Wait for it to install, then add:
```
com.unity.services.authentication
```

#### Step 3: Install Netcode for GameObjects
```
com.unity.netcode.gameobjects
```
This will automatically install `com.unity.transport` as a dependency.

#### Step 4: Install Relay and Lobby
```
com.unity.services.relay
```
Wait for it to install, then add:
```
com.unity.services.lobbies
```

#### Step 5: Install Analytics (Optional)
```
com.unity.services.analytics
```

#### Step 6: Install Cloud Save (Optional)
```
com.unity.services.cloudsave
```

### Method 2: Unity Hub / Unity Registry

If the above doesn't work, you can search for packages in the Package Manager:

1. Open **Window → Package Manager**
2. Change dropdown from "Packages: In Project" to **"Unity Registry"**
3. Search for each package:
   - Gaming Services Core
   - Authentication
   - Netcode for GameObjects
   - Transport
   - Relay
   - Lobby
   - Analytics
   - Cloud Save
4. Click **Install** for each

### Method 3: Manual Package Installation via Git URL

If you need specific versions, use Git URLs:

1. **Window → Package Manager**
2. Click **+** → **Add package from git URL...**
3. Enter these URLs one at a time:

```
https://github.com/Unity-Technologies/com.unity.netcode.gameobjects.git
```

---

## Installing LeanTween (UI Animations)

LeanTween is NOT a Unity package. Install it from Asset Store:

### Option A: Unity Asset Store
1. Open Unity Asset Store in browser
2. Search for "LeanTween"
3. Download and import into project

### Option B: GitHub (Free)
1. Go to: https://github.com/dentedpixel/LeanTween
2. Download the repository
3. Copy `LeanTween.cs` into your `Assets/` folder

### Option C: Skip It
LeanTween is only used for UI animations. Your game will work fine without it - the code already has fallbacks that set alpha values directly without animations.

---

## Unity Gaming Services Setup (Account Configuration)

After installing packages, you need to link your project to Unity Gaming Services:

### Step 1: Create Unity Cloud Project
1. Go to **Edit → Project Settings → Services**
2. Click **Create a Unity Project ID** (if you don't have one)
3. Follow the on-screen instructions
4. Select **General** or **Gaming** project type

### Step 2: Enable Required Services
1. In Project Settings → Services, enable:
   - **Authentication** (required)
   - **Lobby** (required for matchmaking)
   - **Relay** (required for NAT punch-through)
   - **Analytics** (optional)
   - **Cloud Save** (optional)

### Step 3: Configure Services
1. Click on each service in the Services window
2. Click **Set up** or **Configure**
3. Follow the setup wizard for each service

---

## Verification After Installation

### Check 1: Packages Installed
1. Open **Window → Package Manager**
2. Change to "Packages: In Project"
3. Verify you see:
   - Gaming Services Core
   - Authentication
   - Netcode for GameObjects
   - Transport
   - Relay
   - Lobby

### Check 2: Compilation
1. Let Unity recompile scripts
2. Check Console for errors
3. All networking files should now compile successfully

### Check 3: Services Connected
1. Go to **Edit → Project Settings → Services**
2. Verify your project is linked
3. Each enabled service should show "Setup complete"

---

## Expected Compilation Results

### After Installing Core + Authentication
✅ Cloud Save features will compile
❌ Networking features still need Netcode

### After Installing Netcode + Relay + Lobby
✅ All networking files will compile
✅ Online multiplayer features available
✅ Hotseat mode still works

### After Installing LeanTween
✅ UI animations will work smoothly
⚠️ Game works fine without it (fallback to instant transitions)

---

## Troubleshooting

### Problem: "Package not found"
**Solution**: Make sure you're using Unity 2022.3 LTS. Some packages require minimum Unity versions.

### Problem: "Dependency version conflict"
**Solution**: Update Unity to latest 2022.3.x patch. Go to Unity Hub → Installs → Update

### Problem: "Services window is empty"
**Solution**:
1. Restart Unity Editor
2. Go to Edit → Project Settings → Services
3. Sign in with your Unity account

### Problem: Compilation errors after installing packages
**Solution**:
1. Close Unity
2. Delete `Library/` folder in your project
3. Reopen Unity (will rebuild everything)

### Problem: "Failed to resolve assembly" errors persist
**Solution**:
1. Uncomment the `using` statements in networking files:
   - Remove `//` from lines like `// using Unity.Netcode;`
2. Remove the `#if UNITY_NETCODE_GAMEOBJECTS` directives if needed

---

## Package Versions (Unity 2022.3 LTS)

These are the recommended versions for Unity 2022.3:

- **Gaming Services Core**: 1.12.0 or later
- **Authentication**: 3.3.0 or later
- **Netcode for GameObjects**: 1.7.0 or later
- **Transport**: 1.4.0 or later (auto-installed)
- **Relay**: 1.1.0 or later
- **Lobby**: 1.2.0 or later
- **Analytics**: 5.0.0 or later
- **Cloud Save**: 3.1.0 or later

---

## What Happens After Installation

### Files That Will Compile
All files in these folders will start compiling:
- `Assets/Networking/` (12 files)
- `Assets/Multiplayer/` (11 files)
- `Assets/Online/` (networking-dependent files)

### Files That Stay Wrapped
If you keep the conditional compilation (`#if UNITY_NETCODE_GAMEOBJECTS`), the networking code will automatically enable once packages are installed. Unity defines this symbol automatically when Netcode is detected.

### Features That Become Available
✅ Online matchmaking (ranked/unranked)
✅ Lobby system with join codes
✅ Relay-based peer-to-peer networking
✅ Server-authoritative match coordination
✅ Network state synchronization
✅ Cloud save backup
✅ Analytics tracking

---

## Next Steps After Installation

1. **Verify Compilation**: Check Unity Console - should be 0 errors
2. **Test Hotseat**: Make sure local 2-player still works
3. **Configure Services**: Set up authentication, lobby, relay in Unity Dashboard
4. **Test Online**: Try creating a lobby and joining with join code
5. **Deploy**: Build and test on multiple machines

---

## Important Notes

⚠️ **Hotseat Mode First**: Your hotseat mode should work fine WITHOUT any packages. Only install packages when you're ready to test online features.

⚠️ **Netcode Symbol**: Unity automatically defines `UNITY_NETCODE_GAMEOBJECTS` when the Netcode package is installed. You don't need to manually define it.

⚠️ **Git Ignore**: Make sure your `.gitignore` includes `Library/` and `Temp/` - don't commit Unity-generated files.

⚠️ **Collaboration**: If working with a team, commit the `Packages/manifest.json` file so others get the same package versions.

---

## Estimated Installation Time

- **Core Services**: 2-3 minutes
- **Netcode + Transport**: 3-5 minutes
- **Relay + Lobby**: 2-3 minutes
- **Optional Services**: 2-3 minutes each
- **Total**: ~15-20 minutes

---

## Support Resources

- **Unity Manual**: https://docs.unity.com/ugs/manual/overview/manual/unity-gaming-services-home
- **Netcode Docs**: https://docs-multiplayer.unity3d.com/netcode/current/about/
- **Unity Discussions**: https://discussions.unity.com/c/multiplayer-networking/
- **Your Code Docs**: See `GETTING_GAME_WORKING_GUIDE.md` in project root

---

## Quick Reference Commands

### Check Current Packages (Terminal)
```bash
cat Packages/manifest.json
```

### View Package Versions
```bash
cat Packages/packages-lock.json | grep -A 2 "com.unity"
```

---

**Last Updated**: 2025-11-20
**Unity Version**: 2022.3.34f1
**Game**: Gravity Wars (2D Multiplayer Space Combat)
