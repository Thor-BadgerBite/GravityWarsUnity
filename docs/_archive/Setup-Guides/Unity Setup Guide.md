# Unity Project Setup Guide - Gravity Wars
**Complete Setup from Scratch**

---

## 📋 Prerequisites

- **Unity Version:** 2022.3.34f1 (LTS) - ✅ Already installed
- **Operating System:** Windows, Mac, or Linux
- **Unity Account:** Required for Unity Gaming Services (free)
- **Internet Connection:** Required for Unity Gaming Services

**Estimated Time:** 1-2 hours

---

## 🎯 What You'll Set Up

1. Unity Gaming Services (Cloud Save, Authentication)
2. Package Installation
3. Project Configuration
4. Manager Setup
5. Testing

---

## 📦 Part 1: Install Packages

### **Step 1: Open Package Manager**
1. In Unity Editor: `Window > Package Manager`
2. Top-left dropdown → Select **"Unity Registry"**

### **Step 2: Install Required Packages**

Install these in order (click Install, wait for completion):

#### **A. TextMeshPro** (if prompted)
- Should auto-install when you create first TMP object
- Click "Import TMP Essentials" when prompted

#### **B. Unity Gaming Services Core**
- Search: `com.unity.services.core`
- Click **Install**
- Wait for completion

#### **C. Unity Authentication**
- Search: `com.unity.services.authentication`
- Click **Install**

#### **D. Unity Cloud Save**
- Search: `com.unity.services.cloudsave`
- Click **Install**

### **Step 3: Optional Packages (for Online Multiplayer)**

These are optional - only install if you want online PVP:

#### **E. Unity Lobby**
- Search: `com.unity.services.lobby`
- Click **Install**

#### **F. Unity Netcode for GameObjects**
- Search: `com.unity.netcode.gameobjects`
- Click **Install**

### **Verify Installation**
- Switch Package Manager dropdown to **"In Project"**
- Confirm all installed packages appear
- Close Package Manager

---

## 🔐 Part 2: Setup Unity Gaming Services

### **Step 1: Create Unity Project ID**

1. Go to `Edit > Project Settings > Services`
2. Click **"Create Unity Project ID"**
3. Fill in:
   - **Organization:** Select or create your organization
   - **Project Name:** "Gravity Wars" (or your preferred name)
4. Click **Create**
5. Wait for confirmation (check top of Services window)

### **Step 2: Enable Cloud Save**

1. In Project Settings > Services, find **"Cloud Save"**
2. Click **"Go to Dashboard"** (opens web browser)
3. In Unity Dashboard:
   - Select your project
   - Click **"Cloud Save"** in left sidebar
   - Click **"Set up Cloud Save"**
   - Follow prompts to enable
4. Return to Unity Editor
5. Verify: Cloud Save shows **"Enabled"** with green checkmark

### **Step 3: Enable Authentication**

1. In Services window, find **"Authentication"**
2. Click **"Enable"**
3. Configuration auto-completes
4. Verify: Shows **"Enabled"** with green checkmark

### **Step 4: Enable Lobby** (Optional - for Online)

1. In Services window, find **"Lobby"**
2. Click **"Enable"**
3. In Dashboard, configure:
   - Max players per lobby: **2** (for 1v1)
   - Save settings
4. Verify: Shows **"Enabled"**

---

## ⚙️ Part 3: Configure Project Settings

### **Step 1: Player Settings**

Go to `Edit > Project Settings > Player`

#### **Company Name**
- Set to your studio name (e.g., "YourStudio")

#### **Product Name**
- Set to "Gravity Wars"

#### **API Compatibility Level**
- Location: `Other Settings > Configuration > Api Compatibility Level`
- Set to **".NET Standard 2.1"**

#### **Scripting Define Symbols** (Optional - for online)
- If you installed Unity Netcode, add: `UNITY_NETCODE_GAMEOBJECTS`
- Location: `Other Settings > Script Compilation > Scripting Define Symbols`

### **Step 2: Quality Settings**

Go to `Edit > Project Settings > Quality`

**For 2D Game:**
- Texture Quality: **Full Res**
- Anti Aliasing: **2x Multi Sampling** (or 4x for better quality)
- V Sync Count: **Every V Blank**

### **Step 3: Physics2D Settings**

Go to `Edit > Project Settings > Physics 2D`

**Add Layers:**
- Layer 6: "PlayerShip"
- Layer 7: "Missile"
- Layer 8: "Planet"
- Layer 9: "Boundary"

**Configure Layer Collision Matrix:**
- PlayerShip collides with: Missile, Planet, Boundary
- Missile collides with: PlayerShip, Planet, Boundary
- Planet collides with: PlayerShip, Missile
- Boundary collides with: PlayerShip, Missile

### **Step 4: Tags**

Go to `Edit > Project Settings > Tags and Layers`

**Add Tags:**
- "Player1"
- "Player2"
- "Missile"
- "Planet"
- "Boundary"

---

## 🧪 Part 4: Test Unity Gaming Services

### **Step 1: Create Test Scene**

1. Create new scene: `File > New Scene`
2. Save as: `Assets/Scenes/ServicesTest.unity`

### **Step 2: Create Test Script**

1. Create C# script: `Assets/Scripts/ServicesTestRunner.cs`
2. Copy this code:

```csharp
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ServicesTestRunner : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("=== Starting Unity Gaming Services Test ===");
        await RunTests();
    }

    async Task RunTests()
    {
        try
        {
            // Test 1: Initialize UGS
            Debug.Log("[Test 1/4] Initializing Unity Gaming Services...");
            await UnityServices.InitializeAsync();
            Debug.Log("[Test 1/4] ✅ PASSED - Services initialized");

            // Test 2: Sign in anonymously
            Debug.Log("[Test 2/4] Testing anonymous authentication...");
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            string playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"[Test 2/4] ✅ PASSED - Signed in as: {playerId}");

            // Test 3: Write to Cloud Save
            Debug.Log("[Test 3/4] Testing Cloud Save write...");
            var testData = new Dictionary<string, object>
            {
                { "test_string", "Hello from Gravity Wars!" },
                { "test_number", 42 },
                { "timestamp", System.DateTime.UtcNow.ToString() }
            };
            await CloudSaveService.Instance.Data.ForceSaveAsync(testData);
            Debug.Log("[Test 3/4] ✅ PASSED - Cloud save write successful");

            // Test 4: Read from Cloud Save
            Debug.Log("[Test 4/4] Testing Cloud Save read...");
            var keys = new HashSet<string> { "test_string", "test_number", "timestamp" };
            var loadedData = await CloudSaveService.Instance.Data.LoadAsync(keys);

            if (loadedData.TryGetValue("test_string", out var testString))
            {
                Debug.Log($"[Test 4/4] Read data: {testString}");
            }
            Debug.Log("[Test 4/4] ✅ PASSED - Cloud save read successful");

            // All tests passed
            Debug.Log("=================================");
            Debug.Log("✅ ALL TESTS PASSED!");
            Debug.Log("Unity Gaming Services are ready!");
            Debug.Log("=================================");
        }
        catch (System.Exception e)
        {
            Debug.LogError("=================================");
            Debug.LogError($"❌ TEST FAILED");
            Debug.LogError($"Error: {e.Message}");
            Debug.LogError($"Stack Trace: {e.StackTrace}");
            Debug.LogError("=================================");
            Debug.LogError("\nTroubleshooting:");
            Debug.LogError("1. Check internet connection");
            Debug.LogError("2. Verify services are enabled in Unity Dashboard");
            Debug.LogError("3. Check Project Settings > Services shows linked project");
        }
    }
}
```

### **Step 3: Run Test**

1. In Hierarchy, create Empty GameObject: "ServicesTest"
2. Attach `ServicesTestRunner.cs` script to it
3. Press **Play**
4. Watch Console

**Expected Output:**
```
=== Starting Unity Gaming Services Test ===
[Test 1/4] Initializing Unity Gaming Services...
[Test 1/4] ✅ PASSED - Services initialized
[Test 2/4] Testing anonymous authentication...
[Test 2/4] ✅ PASSED - Signed in as: <player_id>
[Test 3/4] Testing Cloud Save write...
[Test 3/4] ✅ PASSED - Cloud save write successful
[Test 4/4] Testing Cloud Save read...
[Test 4/4] Read data: Hello from Gravity Wars!
[Test 4/4] ✅ PASSED - Cloud save read successful
=================================
✅ ALL TESTS PASSED!
Unity Gaming Services are ready!
=================================
```

**If tests fail:**
- Check internet connection
- Verify services enabled in Unity Dashboard
- Check Console for specific error messages
- Ensure Project ID is linked in `Edit > Project Settings > Services`

---

## 🏗️ Part 5: Setup Game Managers

### **Step 1: Create Main Menu Scene**

1. Create new scene: `File > New Scene`
2. Save as: `Assets/Scenes/MainMenu.unity`
3. Delete default Main Camera and Directional Light

### **Step 2: Create ProgressionManager**

1. In Hierarchy, create Empty GameObject: `ProgressionManager`
2. Attach script: `Assets/Progression System/ProgressionManager.cs`
3. In Inspector, you'll see empty lists:
   - All Ship Bodies
   - All Perks
   - All Passives
   - All Move Types
   - All Missiles

**We'll fill these with ScriptableObjects later (see CONTENT_CREATION_GUIDE.md)**

4. Verify `Awake()` method calls `DontDestroyOnLoad(gameObject)` in code

### **Step 3: Create ServiceLocator** (Optional - for Cloud/Online)

1. Create Empty GameObject: `ServiceLocator`
2. Attach script: `Assets/Networking/ServiceLocator.cs`
3. Leave as-is for now (auto-configures on start)

### **Step 4: Create Canvas for UI**

1. Right-click Hierarchy → `UI > Canvas`
2. Name: `MainCanvas`
3. Canvas Scaler settings:
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 x 1080**
   - Match: **0.5** (width and height balanced)

4. Create simple UI for testing:
   - Right-click MainCanvas → `UI > Text - TextMeshPro`
   - Name: "TitleText"
   - Set text: "Gravity Wars"
   - Font Size: 72
   - Alignment: Center
   - Position: Top center

### **Step 5: Setup Build Settings**

1. Go to `File > Build Settings`
2. Click **"Add Open Scenes"** to add MainMenu
3. Add your existing game scene(s)
4. **Important:** MainMenu should be index 0 (top of list)

---

## 📁 Part 6: Create Folder Structure

Create these folders for organization:

```
Assets/
├── Scenes/
│   ├── MainMenu.unity ✅
│   ├── GameArena.unity
│   └── ServicesTest.unity ✅
├── Scripts/
│   ├── Core/
│   ├── UI/
│   ├── Gameplay/
│   └── Managers/
├── Data/
│   ├── Ships/
│   ├── Perks/
│   ├── Passives/
│   ├── Quests/
│   ├── Achievements/
│   └── BattlePass/
├── Prefabs/
│   ├── UI/
│   ├── Ships/
│   ├── Missiles/
│   └── VFX/
├── Art/
│   ├── Sprites/
│   ├── Materials/
│   └── Textures/
└── Audio/
    ├── Music/
    └── SFX/
```

You can create folders in Unity by right-clicking Project window → `Create > Folder`

---

## ✅ Part 7: Verify Setup

### **Checklist:**

**Services:**
- [ ] Unity Gaming Services test passes (all 4 tests green)
- [ ] Cloud Save enabled in Dashboard
- [ ] Authentication enabled in Dashboard

**Project Settings:**
- [ ] Company Name set
- [ ] Product Name set to "Gravity Wars"
- [ ] API Compatibility Level is .NET Standard 2.1
- [ ] Physics2D layers configured
- [ ] Tags created

**Scenes:**
- [ ] MainMenu scene created
- [ ] MainMenu is index 0 in Build Settings
- [ ] Canvas created with proper scaler settings

**Managers:**
- [ ] ProgressionManager exists in MainMenu scene
- [ ] ServiceLocator exists (if using cloud features)
- [ ] Both use DontDestroyOnLoad

**Folder Structure:**
- [ ] Data, Scripts, Prefabs, Art folders created
- [ ] Organized by type

---

## 🐛 Troubleshooting

### **Problem: "Services initialization failed"**
**Solutions:**
1. Check internet connection
2. Go to `Edit > Project Settings > Services`
3. Verify Project ID is linked
4. Try clicking "Refresh" in Services window
5. Restart Unity Editor

### **Problem: "Authentication failed"**
**Solutions:**
1. Verify Authentication is enabled in Dashboard
2. Check Console for specific error code
3. Try signing out: `AuthenticationService.Instance.SignOut()`
4. Clear PlayerPrefs: `Edit > Clear All PlayerPrefs`

### **Problem: "Cloud Save permission denied"**
**Solutions:**
1. Ensure Cloud Save is enabled in Dashboard
2. Verify player is signed in before saving
3. Check Unity account has proper permissions
4. Try re-enabling Cloud Save in Dashboard

### **Problem: "ProgressionManager not persisting between scenes"**
**Solutions:**
1. Verify `DontDestroyOnLoad(gameObject)` is called in Awake()
2. Check there's only ONE ProgressionManager (use Singleton pattern)
3. Ensure ProgressionManager is in first loaded scene (MainMenu)

### **Problem: "NullReferenceException in SaveSystem"**
**Solutions:**
1. Check save file path is valid
2. On mobile, use `Application.persistentDataPath`
3. On PC, ensure game has write permissions
4. Verify folder exists before saving

---

## 🎮 Part 8: Test Your Setup

### **Quick Test Procedure:**

1. **Test Services:**
   - Open `ServicesTest.unity`
   - Press Play
   - Verify all 4 tests pass
   - Stop Play

2. **Test Managers:**
   - Open `MainMenu.unity`
   - Press Play
   - Check Console: No errors
   - Check Hierarchy: ProgressionManager should say "(DontDestroyOnLoad)"
   - Stop Play

3. **Test Scene Loading:**
   - In MainMenu, add a button that loads game scene
   - Press Play → Click button → Load game scene
   - Check Hierarchy: ProgressionManager still exists
   - Verify no duplicates created

---

## 📚 Next Steps

After completing this setup:

### **Immediate Next Steps:**
1. ✅ **Read CONTENT_CREATION_GUIDE.md** - Create ships, perks, quests
2. ✅ **Read UI_CREATION_GUIDE.md** - Build main menu and game UI
3. ✅ **Test hotseat mode** - Make sure local PVP still works

### **Later Steps:**
4. **Read ART_PIPELINE_GUIDE.md** - Add 3D models and VFX
5. **Read ONLINE_MULTIPLAYER_GUIDE.md** - Implement netcode (if wanted)
6. **Read TESTING_GUIDE.md** - QA and balancing

---

## 🆘 Getting Help

**Official Documentation:**
- [Unity Gaming Services](https://docs.unity.com/ugs)
- [Cloud Save Docs](https://docs.unity.com/cloud-save)
- [Authentication Docs](https://docs.unity.com/authentication)

**Common Resources:**
- Unity Forums: https://forum.unity.com/
- Unity Discord: https://discord.gg/unity
- Stack Overflow: Tag `unity3d`

**Check Your Setup:**
- Console should have NO red errors
- Services window shows all services "Enabled"
- Test script passes all 4 tests
- Managers persist between scenes

---

## ✅ Setup Complete!

You now have:
- ✅ Unity Gaming Services configured
- ✅ Cloud Save functional
- ✅ Authentication working
- ✅ Project properly configured
- ✅ Folder structure organized
- ✅ Managers set up correctly

**Total Time:** Should have taken 1-2 hours

**Ready for next step:** Content Creation! 🚀

Next: `CONTENT_CREATION_GUIDE.md`
