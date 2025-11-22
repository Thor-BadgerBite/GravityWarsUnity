using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor utility to generate achievement templates.
///
/// Usage:
///   In Unity Editor: Tools → Gravity Wars → Generate Achievement Templates
///
/// This will create 50+ achievement ScriptableObjects in Assets/Achievements/Templates/
/// </summary>
public class AchievementTemplateGenerator : EditorWindow
{
    private const string TEMPLATE_PATH = "Assets/Achievements/Templates/";

    [MenuItem("Tools/Gravity Wars/Generate Achievement Templates")]
    public static void ShowWindow()
    {
        GetWindow<AchievementTemplateGenerator>("Achievement Template Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Achievement Template Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This will create 50+ achievement templates in:");
        GUILayout.Label(TEMPLATE_PATH, EditorStyles.miniLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Generate All Achievement Templates", GUILayout.Height(40)))
        {
            GenerateAllTemplates();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Clear All Achievement Templates", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete",
                "This will delete all achievement templates. Are you sure?",
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

        // Generate combat achievements (tiered)
        createdCount += GenerateCombatAchievements();

        // Generate progression achievements
        createdCount += GenerateProgressionAchievements();

        // Generate collection achievements
        createdCount += GenerateCollectionAchievements();

        // Generate skill achievements
        createdCount += GenerateSkillAchievements();

        // Generate social achievements
        createdCount += GenerateSocialAchievements();

        // Generate secret achievements
        createdCount += GenerateSecretAchievements();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            $"Generated {createdCount} achievement templates successfully!",
            "OK");

        Debug.Log($"[AchievementTemplateGenerator] Generated {createdCount} achievement templates");
    }

    #region Combat Achievements

    private int GenerateCombatAchievements()
    {
        var achievements = new List<AchievementTemplate>
        {
            // First Blood (single)
            new AchievementTemplate
            {
                fileName = "Combat_FirstBlood",
                displayName = "First Blood",
                description = "Win your first match!",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.WinMatches,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 100,
                accountXPReward = 50,
                achievementPoints = 10
            },

            // Win Matches (tiered)
            new AchievementTemplate
            {
                fileName = "Combat_WinMatches_Bronze",
                displayName = "Warrior",
                description = "Win 10 matches.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.WinMatches,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Bronze,
                targetValue = 10,
                softCurrencyReward = 250,
                accountXPReward = 100,
                achievementPoints = 15
            },
            new AchievementTemplate
            {
                fileName = "Combat_WinMatches_Silver",
                displayName = "Veteran",
                description = "Win 50 matches.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.WinMatches,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Silver,
                targetValue = 50,
                softCurrencyReward = 500,
                hardCurrencyReward = 10,
                accountXPReward = 250,
                achievementPoints = 30
            },
            new AchievementTemplate
            {
                fileName = "Combat_WinMatches_Gold",
                displayName = "Champion",
                description = "Win 100 matches.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.WinMatches,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Gold,
                targetValue = 100,
                softCurrencyReward = 1000,
                hardCurrencyReward = 25,
                accountXPReward = 500,
                achievementPoints = 50
            },
            new AchievementTemplate
            {
                fileName = "Combat_WinMatches_Platinum",
                displayName = "Legend",
                description = "Win 500 matches!",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.WinMatches,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Platinum,
                targetValue = 500,
                softCurrencyReward = 5000,
                hardCurrencyReward = 100,
                accountXPReward = 2000,
                achievementPoints = 100
            },

            // Deal Damage (tiered)
            new AchievementTemplate
            {
                fileName = "Combat_DealDamage_Bronze",
                displayName = "Bombardier",
                description = "Deal 10,000 total damage.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.DealDamage,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Bronze,
                targetValue = 10000,
                softCurrencyReward = 200,
                accountXPReward = 100,
                achievementPoints = 15
            },
            new AchievementTemplate
            {
                fileName = "Combat_DealDamage_Silver",
                displayName = "Devastator",
                description = "Deal 50,000 total damage.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.DealDamage,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Silver,
                targetValue = 50000,
                softCurrencyReward = 500,
                hardCurrencyReward = 10,
                accountXPReward = 250,
                achievementPoints = 30
            },
            new AchievementTemplate
            {
                fileName = "Combat_DealDamage_Gold",
                displayName = "Annihilator",
                description = "Deal 100,000 total damage.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.DealDamage,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Gold,
                targetValue = 100000,
                softCurrencyReward = 1000,
                hardCurrencyReward = 25,
                accountXPReward = 500,
                achievementPoints = 50
            },

            // Hit Missiles (tiered)
            new AchievementTemplate
            {
                fileName = "Combat_HitMissiles_Bronze",
                displayName = "Marksman",
                description = "Hit 100 missiles.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.HitMissiles,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Bronze,
                targetValue = 100,
                softCurrencyReward = 200,
                accountXPReward = 100,
                achievementPoints = 15
            },
            new AchievementTemplate
            {
                fileName = "Combat_HitMissiles_Silver",
                displayName = "Sharpshooter",
                description = "Hit 500 missiles.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.HitMissiles,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Silver,
                targetValue = 500,
                softCurrencyReward = 500,
                hardCurrencyReward = 10,
                accountXPReward = 250,
                achievementPoints = 30
            },
            new AchievementTemplate
            {
                fileName = "Combat_HitMissiles_Gold",
                displayName = "Deadeye",
                description = "Hit 1000 missiles!",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.HitMissiles,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Gold,
                targetValue = 1000,
                softCurrencyReward = 1000,
                hardCurrencyReward = 25,
                accountXPReward = 500,
                achievementPoints = 50
            },

            // Win Rounds
            new AchievementTemplate
            {
                fileName = "Combat_WinRounds_Bronze",
                displayName = "Round Winner",
                description = "Win 50 rounds.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.WinRounds,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Bronze,
                targetValue = 50,
                softCurrencyReward = 200,
                accountXPReward = 100,
                achievementPoints = 15
            },
            new AchievementTemplate
            {
                fileName = "Combat_WinRounds_Silver",
                displayName = "Round Master",
                description = "Win 250 rounds.",
                category = AchievementCategory.Combat,
                conditionType = AchievementConditionType.WinRounds,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Silver,
                targetValue = 250,
                softCurrencyReward = 500,
                hardCurrencyReward = 10,
                accountXPReward = 250,
                achievementPoints = 30
            },
        };

        return CreateAchievementAssets(achievements);
    }

    #endregion

    #region Progression Achievements

    private int GenerateProgressionAchievements()
    {
        var achievements = new List<AchievementTemplate>
        {
            // Account Level
            new AchievementTemplate
            {
                fileName = "Progression_ReachLevel10",
                displayName = "Getting Started",
                description = "Reach account level 10.",
                category = AchievementCategory.Progression,
                conditionType = AchievementConditionType.ReachAccountLevel,
                achievementType = AchievementType.Single,
                targetValue = 10,
                softCurrencyReward = 250,
                accountXPReward = 0,
                achievementPoints = 10
            },
            new AchievementTemplate
            {
                fileName = "Progression_ReachLevel25",
                displayName = "Rising Star",
                description = "Reach account level 25.",
                category = AchievementCategory.Progression,
                conditionType = AchievementConditionType.ReachAccountLevel,
                achievementType = AchievementType.Single,
                targetValue = 25,
                softCurrencyReward = 500,
                hardCurrencyReward = 10,
                accountXPReward = 0,
                achievementPoints = 25
            },
            new AchievementTemplate
            {
                fileName = "Progression_ReachLevel50",
                displayName = "Veteran Commander",
                description = "Reach account level 50.",
                category = AchievementCategory.Progression,
                conditionType = AchievementConditionType.ReachAccountLevel,
                achievementType = AchievementType.Single,
                targetValue = 50,
                softCurrencyReward = 1000,
                hardCurrencyReward = 25,
                accountXPReward = 0,
                achievementPoints = 50
            },
            new AchievementTemplate
            {
                fileName = "Progression_ReachLevel100",
                displayName = "Legendary Commander",
                description = "Reach account level 100!",
                category = AchievementCategory.Progression,
                conditionType = AchievementConditionType.ReachAccountLevel,
                achievementType = AchievementType.Single,
                targetValue = 100,
                softCurrencyReward = 5000,
                hardCurrencyReward = 100,
                accountXPReward = 0,
                achievementPoints = 100
            },

            // Earn Currency
            new AchievementTemplate
            {
                fileName = "Progression_EarnCurrency_10k",
                displayName = "Wealth Builder",
                description = "Earn 10,000 total coins.",
                category = AchievementCategory.Progression,
                conditionType = AchievementConditionType.EarnTotalCurrency,
                achievementType = AchievementType.Incremental,
                targetValue = 10000,
                softCurrencyReward = 500,
                accountXPReward = 100,
                achievementPoints = 20
            },
            new AchievementTemplate
            {
                fileName = "Progression_EarnCurrency_50k",
                displayName = "Fortune Seeker",
                description = "Earn 50,000 total coins.",
                category = AchievementCategory.Progression,
                conditionType = AchievementConditionType.EarnTotalCurrency,
                achievementType = AchievementType.Incremental,
                targetValue = 50000,
                softCurrencyReward = 1500,
                hardCurrencyReward = 25,
                accountXPReward = 250,
                achievementPoints = 40
            },
            new AchievementTemplate
            {
                fileName = "Progression_EarnCurrency_100k",
                displayName = "Tycoon",
                description = "Earn 100,000 total coins!",
                category = AchievementCategory.Progression,
                conditionType = AchievementConditionType.EarnTotalCurrency,
                achievementType = AchievementType.Incremental,
                targetValue = 100000,
                softCurrencyReward = 3000,
                hardCurrencyReward = 50,
                accountXPReward = 500,
                achievementPoints = 60
            },
        };

        return CreateAchievementAssets(achievements);
    }

    #endregion

    #region Collection Achievements

    private int GenerateCollectionAchievements()
    {
        var achievements = new List<AchievementTemplate>
        {
            // Unlock Ships
            new AchievementTemplate
            {
                fileName = "Collection_UnlockAllShips",
                displayName = "Ship Collector",
                description = "Unlock all ship archetypes.",
                category = AchievementCategory.Collection,
                conditionType = AchievementConditionType.UnlockAllShips,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 1000,
                hardCurrencyReward = 20,
                accountXPReward = 250,
                achievementPoints = 40
            },

            // Unlock Missiles
            new AchievementTemplate
            {
                fileName = "Collection_UnlockAllMissiles",
                displayName = "Arsenal Master",
                description = "Unlock all missile types.",
                category = AchievementCategory.Collection,
                conditionType = AchievementConditionType.UnlockAllMissiles,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 1500,
                hardCurrencyReward = 30,
                accountXPReward = 300,
                achievementPoints = 50
            },

            // Unlock Perks
            new AchievementTemplate
            {
                fileName = "Collection_UnlockAllPerks",
                displayName = "Perk Expert",
                description = "Unlock all perks.",
                category = AchievementCategory.Collection,
                conditionType = AchievementConditionType.UnlockAllPerks,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 1500,
                hardCurrencyReward = 30,
                accountXPReward = 300,
                achievementPoints = 50
            },

            // Unlock All Items
            new AchievementTemplate
            {
                fileName = "Collection_UnlockAllItems",
                displayName = "Completionist",
                description = "Unlock everything in the game!",
                category = AchievementCategory.Collection,
                conditionType = AchievementConditionType.UnlockAllItems,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 10000,
                hardCurrencyReward = 200,
                accountXPReward = 1000,
                achievementPoints = 150
            },

            // Unlock Cosmetics
            new AchievementTemplate
            {
                fileName = "Collection_UnlockAllCosmetics",
                displayName = "Fashion Icon",
                description = "Unlock all cosmetic items.",
                category = AchievementCategory.Collection,
                conditionType = AchievementConditionType.UnlockAllCosmetics,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 2000,
                hardCurrencyReward = 50,
                accountXPReward = 400,
                achievementPoints = 75
            },
        };

        return CreateAchievementAssets(achievements);
    }

    #endregion

    #region Skill Achievements

    private int GenerateSkillAchievements()
    {
        var achievements = new List<AchievementTemplate>
        {
            // Win Streak
            new AchievementTemplate
            {
                fileName = "Skill_WinStreak3",
                displayName = "On a Roll",
                description = "Win 3 matches in a row.",
                category = AchievementCategory.Skill,
                conditionType = AchievementConditionType.WinMatchesInRow,
                achievementType = AchievementType.Single,
                targetValue = 3,
                softCurrencyReward = 300,
                accountXPReward = 100,
                achievementPoints = 20
            },
            new AchievementTemplate
            {
                fileName = "Skill_WinStreak5",
                displayName = "Unstoppable",
                description = "Win 5 matches in a row!",
                category = AchievementCategory.Skill,
                conditionType = AchievementConditionType.WinMatchesInRow,
                achievementType = AchievementType.Single,
                targetValue = 5,
                softCurrencyReward = 750,
                hardCurrencyReward = 15,
                accountXPReward = 250,
                achievementPoints = 40
            },
            new AchievementTemplate
            {
                fileName = "Skill_WinStreak10",
                displayName = "Dominator",
                description = "Win 10 matches in a row!!",
                category = AchievementCategory.Skill,
                conditionType = AchievementConditionType.WinMatchesInRow,
                achievementType = AchievementType.Single,
                targetValue = 10,
                softCurrencyReward = 2000,
                hardCurrencyReward = 50,
                accountXPReward = 500,
                achievementPoints = 75
            },

            // Flawless Victory
            new AchievementTemplate
            {
                fileName = "Skill_FlawlessVictory",
                displayName = "Flawless",
                description = "Win a match without taking any damage.",
                category = AchievementCategory.Skill,
                conditionType = AchievementConditionType.WinWithoutTakingDamage,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 500,
                hardCurrencyReward = 10,
                accountXPReward = 200,
                achievementPoints = 30
            },

            // Perfect Accuracy
            new AchievementTemplate
            {
                fileName = "Skill_PerfectAccuracy",
                displayName = "Perfect Aim",
                description = "Win a match with 95%+ accuracy.",
                category = AchievementCategory.Skill,
                conditionType = AchievementConditionType.WinWithPerfectAccuracy,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 500,
                hardCurrencyReward = 10,
                accountXPReward = 200,
                achievementPoints = 30
            },

            // Quick Victory
            new AchievementTemplate
            {
                fileName = "Skill_QuickVictory",
                displayName = "Speed Demon",
                description = "Win a match in under 60 seconds.",
                category = AchievementCategory.Skill,
                conditionType = AchievementConditionType.WinIn60Seconds,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 400,
                hardCurrencyReward = 8,
                accountXPReward = 150,
                achievementPoints = 25
            },

            // High Damage Single Shot
            new AchievementTemplate
            {
                fileName = "Skill_HighDamageSingleShot",
                displayName = "Critical Hit",
                description = "Deal 500+ damage with a single shot.",
                category = AchievementCategory.Skill,
                conditionType = AchievementConditionType.DealDamageWithSingleShot,
                achievementType = AchievementType.Single,
                targetValue = 500,
                softCurrencyReward = 300,
                accountXPReward = 100,
                achievementPoints = 20
            },

            // Win With All Archetypes
            new AchievementTemplate
            {
                fileName = "Skill_WinWithAllArchetypes",
                displayName = "Master of All",
                description = "Win matches with all ship archetypes.",
                category = AchievementCategory.Skill,
                conditionType = AchievementConditionType.WinWithAllArchetypes,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 1000,
                hardCurrencyReward = 20,
                accountXPReward = 300,
                achievementPoints = 50
            },
        };

        return CreateAchievementAssets(achievements);
    }

    #endregion

    #region Social Achievements

    private int GenerateSocialAchievements()
    {
        var achievements = new List<AchievementTemplate>
        {
            // Play With Friend
            new AchievementTemplate
            {
                fileName = "Social_PlayWithFriend",
                displayName = "Friendly Skies",
                description = "Play a match with a friend.",
                category = AchievementCategory.Social,
                conditionType = AchievementConditionType.PlayWithFriend,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 200,
                accountXPReward = 50,
                achievementPoints = 10
            },

            // Win Against Friend
            new AchievementTemplate
            {
                fileName = "Social_WinAgainstFriend",
                displayName = "Friendly Rivalry",
                description = "Win a match against a friend.",
                category = AchievementCategory.Social,
                conditionType = AchievementConditionType.WinAgainstFriend,
                achievementType = AchievementType.Single,
                targetValue = 1,
                softCurrencyReward = 300,
                accountXPReward = 75,
                achievementPoints = 15
            },

            // Play Matches (tiered social)
            new AchievementTemplate
            {
                fileName = "Social_PlayMatches_Bronze",
                displayName = "Socializer",
                description = "Play 25 online matches.",
                category = AchievementCategory.Social,
                conditionType = AchievementConditionType.PlayMatches,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Bronze,
                targetValue = 25,
                softCurrencyReward = 250,
                accountXPReward = 100,
                achievementPoints = 15
            },
            new AchievementTemplate
            {
                fileName = "Social_PlayMatches_Silver",
                displayName = "Networker",
                description = "Play 100 online matches.",
                category = AchievementCategory.Social,
                conditionType = AchievementConditionType.PlayMatches,
                achievementType = AchievementType.Tiered,
                tier = AchievementTier.Silver,
                targetValue = 100,
                softCurrencyReward = 500,
                hardCurrencyReward = 10,
                accountXPReward = 250,
                achievementPoints = 30
            },
        };

        return CreateAchievementAssets(achievements);
    }

    #endregion

    #region Secret Achievements

    private int GenerateSecretAchievements()
    {
        var achievements = new List<AchievementTemplate>
        {
            // Secret: Win 100 matches with Tank
            new AchievementTemplate
            {
                fileName = "Secret_TankMaster",
                displayName = "Tank Commander",
                description = "Win 100 matches with the Tank archetype.",
                category = AchievementCategory.Secret,
                conditionType = AchievementConditionType.WinWithArchetype,
                achievementType = AchievementType.Single,
                isSecret = true,
                targetValue = 100,
                requiredContext = "Tank",
                softCurrencyReward = 2000,
                hardCurrencyReward = 40,
                accountXPReward = 500,
                achievementPoints = 60
            },

            // Secret: Win 100 matches with Sniper
            new AchievementTemplate
            {
                fileName = "Secret_SniperMaster",
                displayName = "Sniper Elite",
                description = "Win 100 matches with the Sniper archetype.",
                category = AchievementCategory.Secret,
                conditionType = AchievementConditionType.WinWithArchetype,
                achievementType = AchievementType.Single,
                isSecret = true,
                targetValue = 100,
                requiredContext = "Sniper",
                softCurrencyReward = 2000,
                hardCurrencyReward = 40,
                accountXPReward = 500,
                achievementPoints = 60
            },

            // Secret: Deal 1000 damage in one match
            new AchievementTemplate
            {
                fileName = "Secret_MassiveDestruction",
                displayName = "Weapons of Mass Destruction",
                description = "Deal 1000+ damage in a single match.",
                category = AchievementCategory.Secret,
                conditionType = AchievementConditionType.DealDamageInOneMatch,
                achievementType = AchievementType.Single,
                isSecret = true,
                targetValue = 1000,
                softCurrencyReward = 1000,
                hardCurrencyReward = 25,
                accountXPReward = 300,
                achievementPoints = 50
            },

            // Secret: Use perk 100 times
            new AchievementTemplate
            {
                fileName = "Secret_PerkAddict",
                displayName = "Perk Enthusiast",
                description = "Activate perks 100 times.",
                category = AchievementCategory.Secret,
                conditionType = AchievementConditionType.UsePerkNTimes,
                achievementType = AchievementType.Single,
                isSecret = true,
                targetValue = 100,
                softCurrencyReward = 800,
                hardCurrencyReward = 15,
                accountXPReward = 250,
                achievementPoints = 40
            },

            // Secret: Complete 50 daily quests
            new AchievementTemplate
            {
                fileName = "Secret_QuestGrinder",
                displayName = "Quest Completionist",
                description = "Complete 50 daily quests.",
                category = AchievementCategory.Secret,
                conditionType = AchievementConditionType.CompleteDailyQuest,
                achievementType = AchievementType.Single,
                isSecret = true,
                targetValue = 50,
                softCurrencyReward = 1500,
                hardCurrencyReward = 30,
                accountXPReward = 400,
                achievementPoints = 50
            },

            // Secret: Win on all maps
            new AchievementTemplate
            {
                fileName = "Secret_MapMaster",
                displayName = "Map Conqueror",
                description = "Win at least one match on every map.",
                category = AchievementCategory.Secret,
                conditionType = AchievementConditionType.WinOnAllMaps,
                achievementType = AchievementType.Single,
                isSecret = true,
                targetValue = 1,
                softCurrencyReward = 1500,
                hardCurrencyReward = 30,
                accountXPReward = 400,
                achievementPoints = 55
            },
        };

        return CreateAchievementAssets(achievements);
    }

    #endregion

    #region Asset Creation

    private int CreateAchievementAssets(List<AchievementTemplate> templates)
    {
        int count = 0;

        foreach (var template in templates)
        {
            string assetPath = TEMPLATE_PATH + template.fileName + ".asset";

            // Check if already exists
            var existingAsset = AssetDatabase.LoadAssetAtPath<AchievementDataSO>(assetPath);
            if (existingAsset != null)
            {
                Debug.Log($"[AchievementTemplateGenerator] Achievement template already exists: {template.fileName}");
                continue;
            }

            // Create new achievement ScriptableObject
            var achievement = ScriptableObject.CreateInstance<AchievementDataSO>();

            // Set all properties
            achievement.achievementID = template.fileName.ToLower();
            achievement.displayName = template.displayName; // Fixed: username -> displayName
            achievement.description = template.description;
            achievement.achievementType = template.achievementType;
            achievement.category = template.category;
            achievement.conditionType = template.conditionType;
            achievement.targetValue = template.targetValue;
            achievement.isSecret = template.isSecret;
            achievement.tier = template.tier;
            achievement.softCurrencyReward = template.softCurrencyReward; // Fixed: creditsReward -> softCurrencyReward
            achievement.hardCurrencyReward = template.hardCurrencyReward; // Fixed: gemsReward -> hardCurrencyReward
            achievement.accountXPReward = template.accountXPReward;
            achievement.achievementPoints = template.achievementPoints;
            achievement.requiredContext = template.requiredContext;
            achievement.sortOrder = count;

            // Create asset
            AssetDatabase.CreateAsset(achievement, assetPath);
            count++;

            Debug.Log($"[AchievementTemplateGenerator] Created achievement template: {template.fileName}");
        }

        return count;
    }

    private void ClearAllTemplates()
    {
        if (!AssetDatabase.IsValidFolder(TEMPLATE_PATH))
        {
            Debug.LogWarning("[AchievementTemplateGenerator] Template folder does not exist");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:AchievementDataSO", new[] { TEMPLATE_PATH });
        int deletedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
            deletedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[AchievementTemplateGenerator] Deleted {deletedCount} achievement templates");
    }

    #endregion

    #region Data Structures

    private class AchievementTemplate
    {
        public string fileName;
        public string displayName;
        public string description;
        public AchievementType achievementType;
        public AchievementCategory category;
        public AchievementConditionType conditionType;
        public int targetValue;
        public bool isSecret = false;
        public AchievementTier tier = AchievementTier.None;
        public int softCurrencyReward;
        public int hardCurrencyReward;
        public int accountXPReward;
        public int achievementPoints;
        public string requiredContext = "";
    }

    #endregion
}
