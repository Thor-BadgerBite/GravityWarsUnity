using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles saving and loading player data to/from JSON files.
/// Uses persistent data path for cross-platform compatibility.
/// </summary>
public static class SaveSystem
{
    private static readonly string SAVE_FOLDER = Application.persistentDataPath + "/Saves/";
    private static readonly string SAVE_FILE = "player_data.json";
    private static readonly string BACKUP_FILE = "player_data_backup.json";

    /// <summary>
    /// Saves player data to JSON file
    /// </summary>
    public static void SavePlayerData(PlayerAccountData data)
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

            Debug.Log($"[SaveSystem] Saved player data to: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to save: {e.Message}");
        }
    }

    /// <summary>
    /// Loads player data from JSON file
    /// </summary>
    public static PlayerAccountData LoadPlayerData()
    {
        try
        {
            string savePath = SAVE_FOLDER + SAVE_FILE;

            if (!File.Exists(savePath))
            {
                Debug.Log("[SaveSystem] No save file found");
                return null;
            }

            // Read JSON
            string json = File.ReadAllText(savePath);

            // Deserialize
            PlayerAccountData data = JsonUtility.FromJson<PlayerAccountData>(json);

            Debug.Log($"[SaveSystem] Loaded player data: {data.displayName}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load, trying backup: {e.Message}");

            // Try loading backup
            return LoadBackup();
        }
    }

    /// <summary>
    /// Loads backup save file
    /// </summary>
    private static PlayerAccountData LoadBackup()
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
            PlayerAccountData data = JsonUtility.FromJson<PlayerAccountData>(json);

            Debug.Log($"[SaveSystem] Loaded backup data: {data.displayName}");
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
            PlayerAccountData data = LoadPlayerData();
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
    public static PlayerAccountData ImportSaveData(string importPath)
    {
        try
        {
            if (!File.Exists(importPath))
            {
                Debug.LogError($"[SaveSystem] File not found: {importPath}");
                return null;
            }

            string json = File.ReadAllText(importPath);
            PlayerAccountData data = JsonUtility.FromJson<PlayerAccountData>(json);

            // Save as current player data
            SavePlayerData(data);

            Debug.Log($"[SaveSystem] Imported save data: {data.displayName}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to import: {e.Message}");
            return null;
        }
    }
}
