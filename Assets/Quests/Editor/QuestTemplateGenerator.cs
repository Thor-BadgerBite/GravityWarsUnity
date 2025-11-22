using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor utility to generate quest templates.
///
/// Usage:
///   In Unity Editor: Tools → Gravity Wars → Generate Quest Templates
///
/// This will create 20+ quest ScriptableObjects in Assets/Quests/Templates/
/// </summary>
public class QuestTemplateGenerator : EditorWindow
{
    private const string TEMPLATE_PATH = "Assets/Quests/Templates/";

    [MenuItem("Tools/Gravity Wars/Generate Quest Templates")]
    public static void ShowWindow()
    {
        GetWindow<QuestTemplateGenerator>("Quest Template Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Quest Template Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This will create 20+ quest templates in:");
        GUILayout.Label(TEMPLATE_PATH, EditorStyles.miniLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Generate All Quest Templates", GUILayout.Height(40)))
        {
            GenerateAllTemplates();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Clear All Quest Templates", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete",
                "This will delete all quest templates. Are you sure?",
                "Yes, Delete All", "Cancel"))
            {
                ClearAllTemplates();
            }
        }
    }

    private void GenerateAllTemplates()
    {
        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder(TEMPLATE_PATH))
        {
            System.IO.Directory.CreateDirectory(TEMPLATE_PATH);
            AssetDatabase.Refresh();
        }

        int createdCount = 0;

        // Generate daily quests
        createdCount += GenerateDailyQuests();

        // Generate weekly quests
        createdCount += GenerateWeeklyQuests();

        // Generate season quests
        createdCount += GenerateSeasonQuests();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            $"Generated {createdCount} quest templates successfully!",
            "OK");

        Debug.Log($"[QuestTemplateGenerator] Generated {createdCount} quest templates");
    }

    #region Daily Quests (Easy, Quick)

    private int GenerateDailyQuests()
    {
        var dailyQuests = new List<QuestTemplate>
        {
            // Win Matches
            new QuestTemplate
            {
                fileName = "Daily_WinMatches_Easy",
                displayName = "Daily Victory I",
                description = "Win 3 matches to prove your skill!",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.WinMatches,
                targetValue = 3,
                difficulty = QuestDifficulty.Easy,
                softCurrencyReward = 100,
                accountXPReward = 50
            },
            new QuestTemplate
            {
                fileName = "Daily_WinMatches_Medium",
                displayName = "Daily Victory II",
                description = "Win 5 matches for bigger rewards!",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.WinMatches,
                targetValue = 5,
                difficulty = QuestDifficulty.Medium,
                softCurrencyReward = 200,
                accountXPReward = 100
            },

            // Play Matches
            new QuestTemplate
            {
                fileName = "Daily_PlayMatches",
                displayName = "Keep Playing",
                description = "Play 5 matches (win or lose!)",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.PlayMatches,
                targetValue = 5,
                difficulty = QuestDifficulty.Easy,
                softCurrencyReward = 75,
                accountXPReward = 40
            },

            // Win Rounds
            new QuestTemplate
            {
                fileName = "Daily_WinRounds",
                displayName = "Round Champion",
                description = "Win 10 rounds in any matches.",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.WinRounds,
                targetValue = 10,
                difficulty = QuestDifficulty.Medium,
                softCurrencyReward = 150,
                accountXPReward = 75
            },

            // Deal Damage
            new QuestTemplate
            {
                fileName = "Daily_DealDamage_Easy",
                displayName = "Artillery Strike",
                description = "Deal 1000 damage to enemy ships.",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.DealDamage,
                targetValue = 1000,
                difficulty = QuestDifficulty.Easy,
                softCurrencyReward = 100,
                accountXPReward = 50
            },
            new QuestTemplate
            {
                fileName = "Daily_DealDamage_Hard",
                displayName = "Bombardment",
                description = "Deal 2500 damage to enemy ships.",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.DealDamage,
                targetValue = 2500,
                difficulty = QuestDifficulty.Hard,
                softCurrencyReward = 250,
                hardCurrencyReward = 5,
                accountXPReward = 125
            },

            // Fire Missiles
            new QuestTemplate
            {
                fileName = "Daily_FireMissiles",
                displayName = "Trigger Happy",
                description = "Fire 20 missiles in matches.",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.FireMissiles,
                targetValue = 20,
                difficulty = QuestDifficulty.Easy,
                softCurrencyReward = 80,
                accountXPReward = 40
            },

            // Hit Missiles
            new QuestTemplate
            {
                fileName = "Daily_HitMissiles",
                displayName = "Sharpshooter",
                description = "Hit enemy ships with 15 missiles.",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.HitMissiles,
                targetValue = 15,
                difficulty = QuestDifficulty.Medium,
                softCurrencyReward = 150,
                accountXPReward = 75
            },

            // Use Perk
            new QuestTemplate
            {
                fileName = "Daily_UsePerk",
                displayName = "Perk Master",
                description = "Activate perks 10 times.",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.UsePerkNTimes,
                targetValue = 10,
                difficulty = QuestDifficulty.Easy,
                softCurrencyReward = 100,
                accountXPReward = 50
            },

            // Play With Archetype
            new QuestTemplate
            {
                fileName = "Daily_PlayTank",
                displayName = "Tank Training",
                description = "Play 3 matches with the Tank archetype.",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.PlayMatchesWithArchetype,
                targetValue = 3,
                requiredArchetype = ShipArchetype.Tank,
                difficulty = QuestDifficulty.Easy,
                softCurrencyReward = 120,
                accountXPReward = 60
            },
            new QuestTemplate
            {
                fileName = "Daily_PlaySniper",
                displayName = "Sniper Training",
                description = "Play 3 matches with the Sniper archetype.",
                questType = QuestType.Daily,
                objectiveType = QuestObjectiveType.PlayMatchesWithArchetype,
                targetValue = 3,
                requiredArchetype = ShipArchetype.DamageDealer, // Changed from Sniper (doesn't exist)
                difficulty = QuestDifficulty.Easy,
                softCurrencyReward = 120,
                accountXPReward = 60
            },
        };

        return CreateQuestAssets(dailyQuests);
    }

    #endregion

    #region Weekly Quests (Medium-Hard, Longer)

    private int GenerateWeeklyQuests()
    {
        var weeklyQuests = new List<QuestTemplate>
        {
            // Win Matches
            new QuestTemplate
            {
                fileName = "Weekly_WinMatches",
                displayName = "Weekly Domination",
                description = "Win 20 matches this week!",
                questType = QuestType.Weekly,
                objectiveType = QuestObjectiveType.WinMatches,
                targetValue = 20,
                difficulty = QuestDifficulty.Medium,
                softCurrencyReward = 500,
                hardCurrencyReward = 10,
                accountXPReward = 250
            },

            // Win Rounds
            new QuestTemplate
            {
                fileName = "Weekly_WinRounds",
                displayName = "Round Master",
                description = "Win 50 rounds this week.",
                questType = QuestType.Weekly,
                objectiveType = QuestObjectiveType.WinRounds,
                targetValue = 50,
                difficulty = QuestDifficulty.Hard,
                softCurrencyReward = 600,
                hardCurrencyReward = 15,
                accountXPReward = 300
            },

            // Deal Damage
            new QuestTemplate
            {
                fileName = "Weekly_DealDamage",
                displayName = "Devastation",
                description = "Deal 10,000 damage this week!",
                questType = QuestType.Weekly,
                objectiveType = QuestObjectiveType.DealDamage,
                targetValue = 10000,
                difficulty = QuestDifficulty.Hard,
                softCurrencyReward = 750,
                hardCurrencyReward = 20,
                accountXPReward = 400
            },

            // Hit Missiles
            new QuestTemplate
            {
                fileName = "Weekly_HitMissiles",
                displayName = "Precision Expert",
                description = "Hit 100 missiles this week.",
                questType = QuestType.Weekly,
                objectiveType = QuestObjectiveType.HitMissiles,
                targetValue = 100,
                difficulty = QuestDifficulty.Hard,
                softCurrencyReward = 700,
                hardCurrencyReward = 15,
                accountXPReward = 350
            },

            // Win With Archetype
            new QuestTemplate
            {
                fileName = "Weekly_WinWithTank",
                displayName = "Tank Mastery",
                description = "Win 10 matches using Tank archetype.",
                questType = QuestType.Weekly,
                objectiveType = QuestObjectiveType.WinWithArchetype,
                targetValue = 10,
                requiredArchetype = ShipArchetype.Tank,
                difficulty = QuestDifficulty.Medium,
                softCurrencyReward = 400,
                hardCurrencyReward = 10,
                accountXPReward = 200
            },
            new QuestTemplate
            {
                fileName = "Weekly_WinWithSniper",
                displayName = "Sniper Mastery",
                description = "Win 10 matches using Sniper archetype.",
                questType = QuestType.Weekly,
                objectiveType = QuestObjectiveType.WinWithArchetype,
                targetValue = 10,
                requiredArchetype = ShipArchetype.DamageDealer, // Changed from Sniper (doesn't exist)
                difficulty = QuestDifficulty.Medium,
                softCurrencyReward = 400,
                hardCurrencyReward = 10,
                accountXPReward = 200
            },

            // Win Streak
            new QuestTemplate
            {
                fileName = "Weekly_WinStreak",
                displayName = "Unstoppable",
                description = "Reach a 5-match win streak!",
                questType = QuestType.Weekly,
                objectiveType = QuestObjectiveType.ReachWinStreak,
                targetValue = 5,
                difficulty = QuestDifficulty.VeryHard,
                softCurrencyReward = 1000,
                hardCurrencyReward = 30,
                accountXPReward = 500
            },
        };

        return CreateQuestAssets(weeklyQuests);
    }

    #endregion

    #region Season Quests (Very Hard, Long-term)

    private int GenerateSeasonQuests()
    {
        var seasonQuests = new List<QuestTemplate>
        {
            // Win Matches
            new QuestTemplate
            {
                fileName = "Season_WinMatches_50",
                displayName = "Season Warrior",
                description = "Win 50 matches this season.",
                questType = QuestType.Season,
                objectiveType = QuestObjectiveType.WinMatches,
                targetValue = 50,
                difficulty = QuestDifficulty.Medium,
                softCurrencyReward = 1000,
                hardCurrencyReward = 25,
                accountXPReward = 500
            },
            new QuestTemplate
            {
                fileName = "Season_WinMatches_100",
                displayName = "Season Champion",
                description = "Win 100 matches this season!",
                questType = QuestType.Season,
                objectiveType = QuestObjectiveType.WinMatches,
                targetValue = 100,
                difficulty = QuestDifficulty.VeryHard,
                softCurrencyReward = 2500,
                hardCurrencyReward = 100,
                accountXPReward = 1000
            },

            // Deal Damage
            new QuestTemplate
            {
                fileName = "Season_DealDamage",
                displayName = "Total Destruction",
                description = "Deal 50,000 damage this season!",
                questType = QuestType.Season,
                objectiveType = QuestObjectiveType.DealDamage,
                targetValue = 50000,
                difficulty = QuestDifficulty.VeryHard,
                softCurrencyReward = 3000,
                hardCurrencyReward = 150,
                accountXPReward = 1500
            },

            // Reach Account Level
            new QuestTemplate
            {
                fileName = "Season_ReachLevel20",
                displayName = "Veteran Pilot",
                description = "Reach account level 20 this season.",
                questType = QuestType.Season,
                objectiveType = QuestObjectiveType.ReachAccountLevel,
                targetValue = 20,
                difficulty = QuestDifficulty.Hard,
                softCurrencyReward = 1500,
                hardCurrencyReward = 50,
                accountXPReward = 0
            },
            new QuestTemplate
            {
                fileName = "Season_ReachLevel50",
                displayName = "Legendary Commander",
                description = "Reach account level 50 this season!",
                questType = QuestType.Season,
                objectiveType = QuestObjectiveType.ReachAccountLevel,
                targetValue = 50,
                difficulty = QuestDifficulty.VeryHard,
                softCurrencyReward = 5000,
                hardCurrencyReward = 250,
                accountXPReward = 0
            },

            // Earn Currency
            new QuestTemplate
            {
                fileName = "Season_EarnCurrency",
                displayName = "Wealth Accumulator",
                description = "Earn 10,000 coins this season.",
                questType = QuestType.Season,
                objectiveType = QuestObjectiveType.EarnCurrency,
                targetValue = 10000,
                difficulty = QuestDifficulty.Hard,
                softCurrencyReward = 2000,
                hardCurrencyReward = 75,
                accountXPReward = 750
            },

            // Win With Multiple Archetypes
            new QuestTemplate
            {
                fileName = "Season_MasterAllShips",
                displayName = "Ship Master",
                description = "Win matches with all 4 ship archetypes.",
                questType = QuestType.Season,
                objectiveType = QuestObjectiveType.WinWithArchetype,
                targetValue = 4, // Would need special logic to track unique archetypes
                difficulty = QuestDifficulty.Hard,
                softCurrencyReward = 1500,
                hardCurrencyReward = 50,
                accountXPReward = 600
            },
        };

        return CreateQuestAssets(seasonQuests);
    }

    #endregion

    #region Asset Creation

    private int CreateQuestAssets(List<QuestTemplate> templates)
    {
        int count = 0;

        foreach (var template in templates)
        {
            string assetPath = TEMPLATE_PATH + template.fileName + ".asset";

            // Check if already exists
            var existingAsset = AssetDatabase.LoadAssetAtPath<QuestDataSO>(assetPath);
            if (existingAsset != null)
            {
                Debug.Log($"[QuestTemplateGenerator] Quest template already exists: {template.fileName}");
                continue;
            }

            // Create new quest ScriptableObject
            var quest = ScriptableObject.CreateInstance<QuestDataSO>();

            // Set all properties
            quest.questID = template.fileName.ToLower();
            quest.displayName = template.displayName; // Fixed: username -> displayName
            quest.description = template.description;
            quest.questType = template.questType;
            quest.objectiveType = template.objectiveType;
            quest.targetValue = template.targetValue;
            quest.difficulty = template.difficulty;
            quest.softCurrencyReward = template.softCurrencyReward; // Fixed: creditsReward -> softCurrencyReward
            quest.hardCurrencyReward = template.hardCurrencyReward; // Fixed: gemsReward -> hardCurrencyReward
            quest.accountXPReward = template.accountXPReward;
            quest.requiredArchetype = template.requiredArchetype;
            quest.requiredMissileType = template.requiredMissileType;

            // Create asset
            AssetDatabase.CreateAsset(quest, assetPath);
            count++;

            Debug.Log($"[QuestTemplateGenerator] Created quest template: {template.fileName}");
        }

        return count;
    }

    private void ClearAllTemplates()
    {
        if (!AssetDatabase.IsValidFolder(TEMPLATE_PATH))
        {
            Debug.LogWarning("[QuestTemplateGenerator] Template folder does not exist");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:QuestDataSO", new[] { TEMPLATE_PATH });
        int deletedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
            deletedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[QuestTemplateGenerator] Deleted {deletedCount} quest templates");
    }

    #endregion

    #region Data Structures

    private class QuestTemplate
    {
        public string fileName;
        public string displayName;
        public string description;
        public QuestType questType;
        public QuestObjectiveType objectiveType;
        public int targetValue;
        public QuestDifficulty difficulty;
        public int softCurrencyReward;
        public int hardCurrencyReward;
        public int accountXPReward;
        public ShipArchetype requiredArchetype = ShipArchetype.AllAround;
        public string requiredMissileType = "";
    }

    #endregion
}
