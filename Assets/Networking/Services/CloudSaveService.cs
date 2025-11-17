using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Security.Cryptography;
using System.Text;

namespace GravityWars.Networking
{
    /// <summary>
    /// Cloud save service for server-side player data synchronization.
    ///
    /// Features:
    /// - Automatic cloud sync with offline queue
    /// - Conflict resolution (merge strategy)
    /// - Data versioning for migration support
    /// - Anti-cheat validation (hash verification)
    /// - Multi-device support
    ///
    /// Usage:
    ///   await CloudSaveService.Instance.SaveToCloud(playerData);
    ///   var data = await CloudSaveService.Instance.LoadFromCloud();
    /// </summary>
    public class CloudSaveService : MonoBehaviour
    {
        #region Constants

        private const string PLAYER_DATA_KEY = "player_account_data";
        private const string PLAYER_DATA_HASH_KEY = "player_data_hash";
        private const string LAST_SYNC_TIME_KEY = "last_sync_timestamp";
        private const int SAVE_VERSION = 2; // Increment when making breaking changes

        #endregion

        #region Offline Queue

        private Queue<PlayerAccountData> _offlineQueue = new Queue<PlayerAccountData>();
        private bool _isSyncing = false;

        #endregion

        #region Public API - Save

        /// <summary>
        /// Saves player data to Unity Cloud Save.
        /// If offline, data is queued and will sync when connection is restored.
        /// </summary>
        public async Task<bool> SaveToCloud(PlayerAccountData data)
        {
            if (data == null)
            {
                Debug.LogError("[CloudSave] Cannot save null data");
                return false;
            }

            // Validate data integrity before saving
            if (!ValidateDataIntegrity(data))
            {
                Debug.LogError("[CloudSave] Data validation failed - possible tampering detected");
                return false;
            }

            // Check if online
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - queuing save for later");
                _offlineQueue.Enqueue(data);
                return false;
            }

            try
            {
                // Compute hash for anti-cheat
                string dataHash = ComputeDataHash(data);

                // Prepare data for cloud save
                var saveData = new Dictionary<string, object>
                {
                    { PLAYER_DATA_KEY, JsonUtility.ToJson(data) },
                    { PLAYER_DATA_HASH_KEY, dataHash },
                    { LAST_SYNC_TIME_KEY, DateTime.UtcNow.ToString("o") }
                };

                // Save to cloud
                await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);

                Debug.Log($"[CloudSave] Successfully saved to cloud for player: {data.playerID}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to save to cloud: {e.Message}");

                // Queue for retry
                _offlineQueue.Enqueue(data);
                return false;
            }
        }

        /// <summary>
        /// Queues data for cloud save (will sync when online).
        /// </summary>
        public void QueueForLater(PlayerAccountData data)
        {
            if (data != null)
            {
                _offlineQueue.Enqueue(data);
                Debug.Log($"[CloudSave] Queued save for later. Queue size: {_offlineQueue.Count}");
            }
        }

        #endregion

        #region Public API - Load

        /// <summary>
        /// Loads player data from Unity Cloud Save.
        /// Returns null if no cloud save exists (new player).
        /// </summary>
        public async Task<PlayerAccountData> LoadFromCloud()
        {
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - cannot load from cloud");
                return null;
            }

            try
            {
                // Load all player data keys
                var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { PLAYER_DATA_KEY, PLAYER_DATA_HASH_KEY, LAST_SYNC_TIME_KEY }
                );

                // Check if data exists
                if (!savedData.ContainsKey(PLAYER_DATA_KEY))
                {
                    Debug.Log("[CloudSave] No cloud save found - new player");
                    return null;
                }

                // Deserialize player data
                string json = savedData[PLAYER_DATA_KEY].Value.GetAsString();
                PlayerAccountData data = JsonUtility.FromJson<PlayerAccountData>(json);

                // Verify hash (anti-cheat)
                if (savedData.ContainsKey(PLAYER_DATA_HASH_KEY))
                {
                    string storedHash = savedData[PLAYER_DATA_HASH_KEY].Value.GetAsString();
                    string computedHash = ComputeDataHash(data);

                    if (storedHash != computedHash)
                    {
                        Debug.LogWarning("[CloudSave] Hash mismatch - data may have been tampered with!");
                        // In production, you might want to reject the data or flag the account
                    }
                }

                // Check if data needs migration
                if (data.saveVersion < SAVE_VERSION)
                {
                    Debug.Log($"[CloudSave] Migrating save from version {data.saveVersion} to {SAVE_VERSION}");
                    data = MigrateSaveData(data);
                }

                Debug.Log($"[CloudSave] Successfully loaded from cloud for player: {data.playerID}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to load from cloud: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Public API - Merge

        /// <summary>
        /// Merges cloud data with local data using smart conflict resolution.
        ///
        /// Strategy:
        /// - Take highest values for cumulative stats (levels, XP, matches played)
        /// - Union of unlocked items (never remove unlocks)
        /// - Most recent timestamp wins for settings (display name, etc.)
        /// </summary>
        public PlayerAccountData MergeData(PlayerAccountData cloudData, PlayerAccountData localData)
        {
            if (cloudData == null) return localData;
            if (localData == null) return cloudData;

            Debug.Log("[CloudSave] Merging cloud and local data...");

            var merged = new PlayerAccountData();

            // Identity - prefer cloud (server is authority)
            merged.playerID = cloudData.playerID;
            merged.accountCreatedDate = cloudData.accountCreatedDate;

            // Display name - most recent
            merged.displayName = (cloudData.lastLoginDate > localData.lastLoginDate)
                ? cloudData.displayName
                : localData.displayName;

            // Progression - take highest values
            merged.accountLevel = Mathf.Max(cloudData.accountLevel, localData.accountLevel);
            merged.accountXP = Mathf.Max(cloudData.accountXP, localData.accountXP);

            // Currency - take highest (player should never lose currency)
            merged.softCurrency = Mathf.Max(cloudData.softCurrency, localData.softCurrency);
            merged.hardCurrency = Mathf.Max(cloudData.hardCurrency, localData.hardCurrency);

            // Battle Pass - take highest tier
            merged.battlePassTier = Mathf.Max(cloudData.battlePassTier, localData.battlePassTier);
            merged.battlePassXP = Mathf.Max(cloudData.battlePassXP, localData.battlePassXP);
            merged.hasPremiumBattlePass = cloudData.hasPremiumBattlePass || localData.hasPremiumBattlePass;
            merged.currentSeasonID = cloudData.currentSeasonID; // Cloud is authority for season

            // Unlocks - union of all unlocks (never remove unlocks)
            merged.unlockedShipBodyIDs = MergeUnlockLists(cloudData.unlockedShipBodyIDs, localData.unlockedShipBodyIDs);
            merged.unlockedTier1PerkIDs = MergeUnlockLists(cloudData.unlockedTier1PerkIDs, localData.unlockedTier1PerkIDs);
            merged.unlockedTier2PerkIDs = MergeUnlockLists(cloudData.unlockedTier2PerkIDs, localData.unlockedTier2PerkIDs);
            merged.unlockedTier3PerkIDs = MergeUnlockLists(cloudData.unlockedTier3PerkIDs, localData.unlockedTier3PerkIDs);
            merged.unlockedPassiveIDs = MergeUnlockLists(cloudData.unlockedPassiveIDs, localData.unlockedPassiveIDs);
            merged.unlockedMoveTypeIDs = MergeUnlockLists(cloudData.unlockedMoveTypeIDs, localData.unlockedMoveTypeIDs);
            merged.unlockedMissileIDs = MergeUnlockLists(cloudData.unlockedMissileIDs, localData.unlockedMissileIDs);
            merged.unlockedSkinIDs = MergeUnlockLists(cloudData.unlockedSkinIDs, localData.unlockedSkinIDs);
            merged.unlockedColorSchemeIDs = MergeUnlockLists(cloudData.unlockedColorSchemeIDs, localData.unlockedColorSchemeIDs);
            merged.unlockedDecalIDs = MergeUnlockLists(cloudData.unlockedDecalIDs, localData.unlockedDecalIDs);

            // Custom loadouts - merge by loadoutID
            merged.customShipLoadouts = MergeLoadouts(cloudData.customShipLoadouts, localData.customShipLoadouts);

            // Ship progression - merge by loadoutKey, take highest levels
            merged.shipProgressionData = MergeShipProgression(cloudData.shipProgressionData, localData.shipProgressionData);

            // Statistics - take highest (cumulative)
            merged.totalMatchesPlayed = Mathf.Max(cloudData.totalMatchesPlayed, localData.totalMatchesPlayed);
            merged.totalMatchesWon = Mathf.Max(cloudData.totalMatchesWon, localData.totalMatchesWon);
            merged.totalRoundsWon = Mathf.Max(cloudData.totalRoundsWon, localData.totalRoundsWon);
            merged.totalDamageDealt = Mathf.Max(cloudData.totalDamageDealt, localData.totalDamageDealt);
            merged.totalMissilesFired = Mathf.Max(cloudData.totalMissilesFired, localData.totalMissilesFired);

            // Timestamps
            merged.lastLoginDate = DateTime.UtcNow;

            // Save version
            merged.saveVersion = SAVE_VERSION;

            Debug.Log("[CloudSave] Data merge complete");
            return merged;
        }

        #endregion

        #region Public API - Delete

        /// <summary>
        /// Deletes all cloud save data for the current player.
        /// WARNING: This is irreversible!
        /// </summary>
        public async Task<bool> DeleteCloudSave()
        {
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - cannot delete cloud save");
                return false;
            }

            try
            {
                await CloudSaveService.Instance.Data.Player.DeleteAsync(PLAYER_DATA_KEY);
                await CloudSaveService.Instance.Data.Player.DeleteAsync(PLAYER_DATA_HASH_KEY);
                await CloudSaveService.Instance.Data.Player.DeleteAsync(LAST_SYNC_TIME_KEY);

                Debug.Log("[CloudSave] Cloud save deleted successfully");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to delete cloud save: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Offline Queue Processing

        private void Update()
        {
            // Process offline queue when connection is restored
            if (!_isSyncing && _offlineQueue.Count > 0 && IsOnline())
            {
                ProcessOfflineQueue();
            }
        }

        private async void ProcessOfflineQueue()
        {
            _isSyncing = true;

            Debug.Log($"[CloudSave] Processing offline queue ({_offlineQueue.Count} items)");

            while (_offlineQueue.Count > 0 && IsOnline())
            {
                var data = _offlineQueue.Dequeue();
                bool success = await SaveToCloud(data);

                if (!success)
                {
                    // Put it back if save failed
                    _offlineQueue.Enqueue(data);
                    break;
                }
            }

            _isSyncing = false;
        }

        #endregion

        #region Anti-Cheat & Validation

        /// <summary>
        /// Computes SHA256 hash of player data for integrity verification.
        /// </summary>
        private string ComputeDataHash(PlayerAccountData data)
        {
            // Create a deterministic JSON representation
            string json = JsonUtility.ToJson(data);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Validates data integrity to detect tampering.
        /// Checks for impossible values, negative numbers, etc.
        /// </summary>
        private bool ValidateDataIntegrity(PlayerAccountData data)
        {
            // Check for negative values
            if (data.accountLevel < 1 || data.accountLevel > 50)
            {
                Debug.LogWarning($"[CloudSave] Invalid account level: {data.accountLevel}");
                return false;
            }

            if (data.accountXP < 0)
            {
                Debug.LogWarning($"[CloudSave] Negative XP detected: {data.accountXP}");
                return false;
            }

            if (data.softCurrency < 0 || data.hardCurrency < 0)
            {
                Debug.LogWarning($"[CloudSave] Negative currency detected");
                return false;
            }

            // Check for impossible progression rates
            var accountAge = (DateTime.UtcNow - data.accountCreatedDate).TotalHours;
            if (accountAge < 1 && data.accountLevel > 10)
            {
                Debug.LogWarning($"[CloudSave] Impossible progression rate detected");
                return false;
            }

            // All checks passed
            return true;
        }

        #endregion

        #region Data Migration

        /// <summary>
        /// Migrates save data from older versions to current version.
        /// </summary>
        private PlayerAccountData MigrateSaveData(PlayerAccountData oldData)
        {
            // Example migration logic
            if (oldData.saveVersion < 2)
            {
                // Migration from v1 to v2
                // Add any new fields with default values
                Debug.Log("[CloudSave] Migrating from v1 to v2");

                // Initialize new fields that didn't exist in v1
                if (oldData.unlockedSkinIDs == null)
                    oldData.unlockedSkinIDs = new List<string>();

                if (oldData.unlockedColorSchemeIDs == null)
                    oldData.unlockedColorSchemeIDs = new List<string>();

                if (oldData.unlockedDecalIDs == null)
                    oldData.unlockedDecalIDs = new List<string>();
            }

            // Update to current version
            oldData.saveVersion = SAVE_VERSION;
            return oldData;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if device has internet connection and Unity Services are ready.
        /// </summary>
        private bool IsOnline()
        {
            return Application.internetReachability != NetworkReachability.NotReachable
                && ServiceLocator.Instance.IsReady();
        }

        /// <summary>
        /// Merges two unlock lists, taking the union (all unlocks from both).
        /// </summary>
        private List<string> MergeUnlockLists(List<string> list1, List<string> list2)
        {
            var merged = new HashSet<string>();

            if (list1 != null)
                foreach (var item in list1)
                    merged.Add(item);

            if (list2 != null)
                foreach (var item in list2)
                    merged.Add(item);

            return new List<string>(merged);
        }

        /// <summary>
        /// Merges two loadout lists by loadoutID.
        /// </summary>
        private List<CustomShipLoadout> MergeLoadouts(List<CustomShipLoadout> cloud, List<CustomShipLoadout> local)
        {
            var merged = new Dictionary<string, CustomShipLoadout>();

            // Add all cloud loadouts
            if (cloud != null)
                foreach (var loadout in cloud)
                    merged[loadout.loadoutID] = loadout;

            // Add local loadouts (won't overwrite cloud if ID matches)
            if (local != null)
                foreach (var loadout in local)
                    if (!merged.ContainsKey(loadout.loadoutID))
                        merged[loadout.loadoutID] = loadout;

            return new List<CustomShipLoadout>(merged.Values);
        }

        /// <summary>
        /// Merges ship progression data, taking highest levels for each loadout.
        /// </summary>
        private List<ShipProgressionEntry> MergeShipProgression(List<ShipProgressionEntry> cloud, List<ShipProgressionEntry> local)
        {
            var merged = new Dictionary<string, ShipProgressionEntry>();

            // Process cloud data
            if (cloud != null)
                foreach (var entry in cloud)
                    merged[entry.loadoutKey] = entry;

            // Merge with local data
            if (local != null)
            {
                foreach (var localEntry in local)
                {
                    if (merged.ContainsKey(localEntry.loadoutKey))
                    {
                        // Take highest level/XP
                        var cloudEntry = merged[localEntry.loadoutKey];
                        merged[localEntry.loadoutKey] = new ShipProgressionEntry
                        {
                            loadoutKey = localEntry.loadoutKey,
                            displayName = cloudEntry.displayName,
                            shipLevel = Mathf.Max(cloudEntry.shipLevel, localEntry.shipLevel),
                            shipXP = Mathf.Max(cloudEntry.shipXP, localEntry.shipXP),
                            matchesPlayed = Mathf.Max(cloudEntry.matchesPlayed, localEntry.matchesPlayed),
                            matchesWon = Mathf.Max(cloudEntry.matchesWon, localEntry.matchesWon),
                            roundsWon = Mathf.Max(cloudEntry.roundsWon, localEntry.roundsWon),
                            totalDamage = Mathf.Max(cloudEntry.totalDamage, localEntry.totalDamage),
                            totalKills = Mathf.Max(cloudEntry.totalKills, localEntry.totalKills),
                            firstUsedDate = cloudEntry.firstUsedDate < localEntry.firstUsedDate ? cloudEntry.firstUsedDate : localEntry.firstUsedDate,
                            lastUsedDate = cloudEntry.lastUsedDate > localEntry.lastUsedDate ? cloudEntry.lastUsedDate : localEntry.lastUsedDate
                        };
                    }
                    else
                    {
                        // New entry from local
                        merged[localEntry.loadoutKey] = localEntry;
                    }
                }
            }

            return new List<ShipProgressionEntry>(merged.Values);
        }

        #endregion
    }
}
