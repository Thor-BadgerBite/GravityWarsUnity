using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Handles saving and loading player data to/from JSON files and cloud storage.
/// Uses persistent data path for cross-platform compatibility.
///
/// PHASE 4+ UPDATE: Now includes cloud save synchronization via CloudSaveService.
/// </summary>
public static class SaveSystem
{
    private static readonly string SAVE_FOLDER = Application.persistentDataPath + "/Saves/";
    private static readonly string SAVE_FILE = "player_data.json";
    private static readonly string BACKUP_FILE = "player_data_backup.json";

    #region Cloud Save Configuration

    [Header("Cloud Save Settings")]
    public static bool enableCloudSync = true; // Toggle cloud save synchronization

    #endregion

    #region Save API (Local + Cloud)

    /// <summary>
    /// Saves player data to local JSON file.
    /// If cloud sync is enabled, also queues for cloud save.
    /// </summary>
    public static void SavePlayerData(PlayerProfileData data)
    {
        // Save locally (instant, synchronous)
        SavePlayerDataLocal(data);

        // Queue cloud save (async, non-blocking)
        if (enableCloudSync)
        {
            SavePlayerDataToCloudAsync(data);
        }
    }

    /// <summary>
    /// Saves player data to local JSON file only (no cloud sync).
    /// </summary>
    public static void SavePlayerDataLocal(PlayerProfileData data)
    {
        try
        {
            // Create save folder if it doesn't exist
            if (!Directory.Exists(SAVE_FOLDER))
            {
                Directory.CreateDirectory(SAVE_FOLDER);
            }

            string savePath = SAVE_FOLDER + SAVE_FILE;
            string backupPath = SAVE_FOLDER + BACKUP_FILE;

            // Create backup of existing save
            if (File.Exists(savePath))
            {
                File.Copy(savePath, backupPath, true);
            }

            // Serialize to JSON with pretty printing
            string json = JsonUtility.ToJson(data, true);

            // Write to file
            File.WriteAllText(savePath, json);

            Debug.Log($"[SaveSystem] Saved player data locally to: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to save locally: {e.Message}");
        }
    }

    /// <summary>
    /// Saves player data to cloud asynchronously (non-blocking).
    /// If offline, data is queued and will sync when connection is restored.
    /// </summary>
    public static async void SavePlayerDataToCloudAsync(PlayerProfileData data)
    {
        try
        {
            var cloudSave = GravityWars.Networking.ServiceLocator.Instance?.CloudSave;
            if (cloudSave != null)
            {
                bool success = await cloudSave.SaveToCloud(data);
                if (!success)
                {
                    Debug.LogWarning("[SaveSystem] Cloud save queued for later (offline or failed)");
                }
            }
            else
            {
                Debug.LogWarning("[SaveSystem] CloudSaveService not available - skipping cloud sync");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Cloud save failed: {e.Message}");
        }
    }

    #endregion

    #region Load API (Local + Cloud Merge)

    /// <summary>
    /// Loads player data from local JSON file.
    /// LEGACY METHOD: For offline-only use. Consider using LoadPlayerDataWithCloudMerge() instead.
    /// </summary>
    public static PlayerProfileData LoadPlayerData()
    {
        return LoadPlayerDataLocal();
    }

    /// <summary>
    /// Loads player data from local file only (no cloud sync).
    /// </summary>
    public static PlayerProfileData LoadPlayerDataLocal()
    {
        try
        {
            string savePath = SAVE_FOLDER + SAVE_FILE;

            if (!File.Exists(savePath))
            {
                Debug.Log("[SaveSystem] No local save file found");
                return null;
            }

            // Read JSON
            string json = File.ReadAllText(savePath);

            // Deserialize
            PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);

            Debug.Log($"[SaveSystem] Loaded local player data: {data.username}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load locally, trying backup: {e.Message}");

            // Try loading backup
            return LoadBackup();
        }
    }

    /// <summary>
    /// Loads player data from cloud and merges with local data (smart conflict resolution).
    /// This is the RECOMMENDED method for Phase 4+ to support multi-device play.
    ///
    /// Strategy:
    /// 1. Load from cloud (authoritative source)
    /// 2. Load from local (may have offline progress)
    /// 3. Merge using smart conflict resolution (highest values, union of unlocks)
    /// 4. Save merged result both locally and to cloud
    ///
    /// Returns: Merged player data, or null if both cloud and local are empty (new player)
    /// </summary>
    public static async Task<PlayerProfileData> LoadPlayerDataWithCloudMergeAsync()
    {
        try
        {
            PlayerProfileData cloudData = null;
            PlayerProfileData localData = null;

            // Load from cloud if online
            if (enableCloudSync && Application.internetReachability != NetworkReachability.NotReachable)
            {
                var cloudSave = GravityWars.Networking.ServiceLocator.Instance?.CloudSave;
                if (cloudSave != null)
                {
                    cloudData = await cloudSave.LoadFromCloud();
                    Debug.Log($"[SaveSystem] Cloud data: {(cloudData != null ? cloudData.username : "none")}");
                }
            }

            // Load from local
            localData = LoadPlayerDataLocal();
            Debug.Log($"[SaveSystem] Local data: {(localData != null ? localData.username : "none")}");

            // Merge data
            PlayerProfileData mergedData = null;

            if (cloudData != null && localData != null)
            {
                // Both exist - merge them
                Debug.Log("[SaveSystem] Merging cloud and local data...");
                var cloudSave = GravityWars.Networking.ServiceLocator.Instance?.CloudSave;
                mergedData = cloudSave.MergeData(cloudData, localData);

                // Save merged result
                SavePlayerDataLocal(mergedData);
                if (enableCloudSync)
                    await cloudSave.SaveToCloud(mergedData);
            }
            else if (cloudData != null)
            {
                // Cloud only - use cloud data
                Debug.Log("[SaveSystem] Using cloud data (no local save)");
                mergedData = cloudData;
                SavePlayerDataLocal(cloudData); // Cache locally
            }
            else if (localData != null)
            {
                // Local only - use local data
                Debug.Log("[SaveSystem] Using local data (no cloud save)");
                mergedData = localData;

                // Upload to cloud for future sync
                if (enableCloudSync)
                {
                    var cloudSave = GravityWars.Networking.ServiceLocator.Instance?.CloudSave;
                    await cloudSave.SaveToCloud(localData);
                }
            }
            else
            {
                // Neither exists - new player
                Debug.Log("[SaveSystem] No save data found (new player)");
                return null;
            }

            Debug.Log($"[SaveSystem] Load complete: {mergedData.username}");
            return mergedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load with cloud merge: {e.Message}");

            // Fallback to local only
            return LoadPlayerDataLocal();
        }
    }

    #endregion

    /// <summary>
    /// Loads backup save file
    /// </summary>
    private static PlayerProfileData LoadBackup()
    {
        try
        {
            string backupPath = SAVE_FOLDER + BACKUP_FILE;

            if (!File.Exists(backupPath))
            {
                Debug.LogError("[SaveSystem] No backup file found either");
                return null;
            }

            string json = File.ReadAllText(backupPath);
            PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);

            Debug.Log($"[SaveSystem] Loaded backup data: {data.username}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load backup: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Deletes save files (use with caution!)
    /// </summary>
    public static void DeleteSaveData()
    {
        try
        {
            string savePath = SAVE_FOLDER + SAVE_FILE;
            string backupPath = SAVE_FOLDER + BACKUP_FILE;

            if (File.Exists(savePath))
                File.Delete(savePath);

            if (File.Exists(backupPath))
                File.Delete(backupPath);

            Debug.Log("[SaveSystem] Deleted all save data");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to delete save data: {e.Message}");
        }
    }

    /// <summary>
    /// Checks if save file exists
    /// </summary>
    public static bool SaveFileExists()
    {
        string savePath = SAVE_FOLDER + SAVE_FILE;
        return File.Exists(savePath);
    }

    /// <summary>
    /// Gets the full path to the save file (for debugging)
    /// </summary>
    public static string GetSavePath()
    {
        return SAVE_FOLDER + SAVE_FILE;
    }

    /// <summary>
    /// Exports save data to a readable JSON file (for debugging/sharing)
    /// </summary>
    public static void ExportSaveData(string exportPath)
    {
        try
        {
            PlayerProfileData data = LoadPlayerData();
            if (data == null)
            {
                Debug.LogError("[SaveSystem] No data to export");
                return;
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(exportPath, json);

            Debug.Log($"[SaveSystem] Exported save data to: {exportPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to export: {e.Message}");
        }
    }

    /// <summary>
    /// Imports save data from an external JSON file
    /// </summary>
    public static PlayerProfileData ImportSaveData(string importPath)
    {
        try
        {
            if (!File.Exists(importPath))
            {
                Debug.LogError($"[SaveSystem] File not found: {importPath}");
                return null;
            }

            string json = File.ReadAllText(importPath);
            PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);

            // Save as current player data
            SavePlayerData(data);

            Debug.Log($"[SaveSystem] Imported save data: {data.username}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to import: {e.Message}");
            return null;
        }
    }
}
