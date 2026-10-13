using System;
using System.Collections.Generic;
using UnityEngine;
using Mazed.Environment;

namespace Mazed.Core
{
    [System.Serializable]
    public class LevelInfo
    {
        public DifficultyLevel difficulty;
        public int levelNumber;
        public string title;
        public string subtitle;
        public string description;
        public int requiredQuizzes;
        public string requiredKeyId;
        public Vector3 spawnPosition;
        public float timeLimit;
        public int mazeBlueprintId; // 1 to 10
    }

    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Active Level & Difficulty")]
        [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Medium;
        [SerializeField] private int currentLevelNumber = 1;

        // Level Configurations for Easy, Medium, and Hard (10 levels each = 30 levels)
        private Dictionary<DifficultyLevel, List<LevelInfo>> levelDatabase = new Dictionary<DifficultyLevel, List<LevelInfo>>();

        public DifficultyLevel CurrentDifficulty => currentDifficulty;
        public int CurrentLevelNumber => currentLevelNumber;
        public const int LevelsPerDifficulty = 10;

        public LevelInfo CurrentLevelInfo => GetLevelInfo(currentDifficulty, currentLevelNumber);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildLevelDatabase();

            // Sync with DifficultyManager if present
            if (DifficultyManager.Instance != null)
            {
                currentDifficulty = DifficultyManager.Instance.CurrentDifficulty;
            }

            // Ensure Level 1 of every difficulty is unlocked initially
            EnsureInitialUnlockState();
        }

        private void EnsureInitialUnlockState()
        {
            foreach (DifficultyLevel diff in Enum.GetValues(typeof(DifficultyLevel)))
            {
                string key = GetUnlockedPrefKey(diff);
                if (!PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.SetInt(key, 1);
                }
            }
            PlayerPrefs.Save();
        }

        private string GetUnlockedPrefKey(DifficultyLevel diff) => $"Mazed_Unlocked_{diff}";
        private string GetCompletedPrefKey(DifficultyLevel diff, int lvl) => $"Mazed_Completed_{diff}_{lvl}";
        private string GetBestScorePrefKey(DifficultyLevel diff, int lvl) => $"Mazed_BestScore_{diff}_{lvl}";
        private string GetBestTimePrefKey(DifficultyLevel diff, int lvl) => $"Mazed_BestTime_{diff}_{lvl}";

        public int GetUnlockedLevelCount(DifficultyLevel diff)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(GetUnlockedPrefKey(diff), 1), 1, LevelsPerDifficulty);
        }

        public bool IsLevelUnlocked(DifficultyLevel diff, int levelNumber)
        {
            if (levelNumber == 1) return true; // Level 1 is always unlocked
            return levelNumber <= GetUnlockedLevelCount(diff);
        }

        public bool IsLevelCompleted(DifficultyLevel diff, int levelNumber)
        {
            return PlayerPrefs.GetInt(GetCompletedPrefKey(diff, levelNumber), 0) == 1;
        }

        public int GetBestScore(DifficultyLevel diff, int levelNumber)
        {
            return PlayerPrefs.GetInt(GetBestScorePrefKey(diff, levelNumber), 0);
        }

        public float GetBestTime(DifficultyLevel diff, int levelNumber)
        {
            return PlayerPrefs.GetFloat(GetBestTimePrefKey(diff, levelNumber), 0f);
        }

        private string GetStarsPrefKey(DifficultyLevel diff, int levelNumber) => $"Mazed_{diff}_Level_{levelNumber}_Stars";

        public int GetLevelStars(DifficultyLevel diff, int levelNumber)
        {
            return PlayerPrefs.GetInt(GetStarsPrefKey(diff, levelNumber), 0);
        }

        public int CalculateStarRating(DifficultyLevel diff, int levelNumber, float completionTime, float timeLimit, int correctQuizzes, int incorrectQuizzes, int hintsUsed)
        {
            // Level completed is at least 1 star
            int stars = 1;

            // Efficient time (cleared within 65% of the allotted time limit)
            bool efficientTime = timeLimit > 0f && (completionTime <= timeLimit * 0.65f);

            // High quiz accuracy (no incorrect attempts, or answered successfully)
            bool goodAccuracy = incorrectQuizzes == 0;

            // Low hint usage (0 or 1 hints used)
            bool lowHints = hintsUsed <= 1;

            if (efficientTime && goodAccuracy && lowHints)
            {
                stars = 3;
            }
            else if (efficientTime || goodAccuracy || lowHints)
            {
                stars = 2;
            }

            return stars;
        }

        public void SaveLevelCompletion(DifficultyLevel diff, int levelNumber, int score, float timeSeconds, int stars = 1)
        {
            // Mark completed
            PlayerPrefs.SetInt(GetCompletedPrefKey(diff, levelNumber), 1);

            // Update stars
            int currentBestStars = GetLevelStars(diff, levelNumber);
            if (stars > currentBestStars)
            {
                PlayerPrefs.SetInt(GetStarsPrefKey(diff, levelNumber), stars);
            }

            // Update best score
            int currentBestScore = GetBestScore(diff, levelNumber);
            if (score > currentBestScore)
            {
                PlayerPrefs.SetInt(GetBestScorePrefKey(diff, levelNumber), score);
            }

            // Update best time (lowest non-zero time)
            float currentBestTime = GetBestTime(diff, levelNumber);
            if (currentBestTime <= 0.01f || timeSeconds < currentBestTime)
            {
                PlayerPrefs.SetFloat(GetBestTimePrefKey(diff, levelNumber), timeSeconds);
            }

            // Unlock next level in this difficulty
            UnlockNextLevel(diff, levelNumber);

            PlayerPrefs.Save();
            Debug.Log($"[LevelManager] Saved {diff} Level {levelNumber} result: Score {score}, Time {timeSeconds:F1}s, Stars {stars}.");
        }

        public void UnlockNextLevel(DifficultyLevel diff, int completedLevel)
        {
            int nextLevel = completedLevel + 1;
            int currentlyUnlocked = GetUnlockedLevelCount(diff);
            if (nextLevel > currentlyUnlocked && nextLevel <= LevelsPerDifficulty)
            {
                PlayerPrefs.SetInt(GetUnlockedPrefKey(diff), nextLevel);
                PlayerPrefs.Save();
                Debug.Log($"[LevelManager] Unlocked {diff} Level {nextLevel}!");
            }
        }

        public void OnDifficultyChanged(DifficultyLevel diff)
        {
            currentDifficulty = diff;
            currentLevelNumber = 1;
            UI.UIManager.Instance?.UpdateHeaderMeta();
        }

        public LevelInfo GetLevelInfo(DifficultyLevel diff, int levelNumber)
        {
            int clampedNum = Mathf.Clamp(levelNumber, 1, LevelsPerDifficulty);
            if (levelDatabase.TryGetValue(diff, out var list) && list.Count >= LevelsPerDifficulty)
            {
                return list[clampedNum - 1];
            }

            // Fallback default
            return new LevelInfo
            {
                difficulty = diff,
                levelNumber = clampedNum,
                title = $"Level {clampedNum}",
                subtitle = "Technical Challenge",
                description = "Navigate the maze and solve objectives.",
                requiredQuizzes = 2,
                requiredKeyId = "BronzeKey",
                spawnPosition = new Vector3(0, 0.5f, 0),
                timeLimit = 180f,
                mazeBlueprintId = clampedNum
            };
        }

        public void LoadLevel(int levelNumber)
        {
            LoadLevel(currentDifficulty, levelNumber);
        }

        public void LoadLevel(DifficultyLevel diff, int levelNumber)
        {
            currentDifficulty = diff;
            currentLevelNumber = Mathf.Clamp(levelNumber, 1, LevelsPerDifficulty);

            if (DifficultyManager.Instance != null && DifficultyManager.Instance.CurrentDifficulty != diff)
            {
                DifficultyManager.Instance.SetDifficulty(diff);
            }

            LevelInfo info = CurrentLevelInfo;
            Debug.Log($"[LevelManager] Loading {diff} Level {currentLevelNumber}: {info.title}");

            // 1. Build Maze Geometry
            MazeBuilder builder = FindFirstObjectByType<MazeBuilder>();
            if (builder != null)
            {
                builder.BuildLevel(diff, currentLevelNumber);
            }

            // 2. Initialize Game Manager state for this level
            if (GameManager.Instance != null)
            {
                GameManager.Instance.InitializeLevel(info);
            }

            // 3. Update HUD header & clear lingering modals
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.HideAllModals();
                UI.UIManager.Instance.UpdateHeaderMeta();
            }
        }

        public void ReplayCurrentLevel()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
            else
            {
                LoadLevel(currentDifficulty, currentLevelNumber);
            }
        }

        public void LoadNextLevel()
        {
            if (currentLevelNumber < LevelsPerDifficulty)
            {
                LoadLevel(currentDifficulty, currentLevelNumber + 1);
            }
            else
            {
                Debug.Log($"[LevelManager] Conquered all {LevelsPerDifficulty} levels of {currentDifficulty} mode!");
            }
        }

        public bool IsCurrentLevelLastOfDifficulty => currentLevelNumber >= LevelsPerDifficulty;

        #region 30 Level Configurations Database
        private void BuildLevelDatabase()
        {
            levelDatabase.Clear();

            // 1. EASY LEVELS (1–10)
            List<LevelInfo> easyList = new List<LevelInfo>
            {
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 1, title = "Level 1 — Starter Labyrinth", subtitle = "First Steps", description = "Navigate starter corridors. Collect the Bronze Key and unlock the Exit Door.", requiredQuizzes = 0, requiredKeyId = "BronzeKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 180f, mazeBlueprintId = 1 },
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 2, title = "Level 2 — Twin Passages", subtitle = "Branching Corridors", description = "Master branching pathways. Retrieve the Silver Key and escape through the Exit Door.", requiredQuizzes = 0, requiredKeyId = "SilverKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 200f, mazeBlueprintId = 2 },
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 3, title = "Level 3 — The Grand Hall", subtitle = "Winding Corridors", description = "Traverse twin halls of stone. Retrieve the Gold Key and unlock the Exit Door.", requiredQuizzes = 0, requiredKeyId = "GoldKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 210f, mazeBlueprintId = 3 },
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 4, title = "Level 4 — Process Pathways", subtitle = "The Crossway", description = "Navigate the corridor labyrinth. Collect the Cyan Key and reach the Exit Door.", requiredQuizzes = 0, requiredKeyId = "CyanKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 220f, mazeBlueprintId = 4 },
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 5, title = "Level 5 — Emerald Crossroads", subtitle = "Vault Junction", description = "Explore branching pathways. Find the Emerald Key and open the Exit Door.", requiredQuizzes = 0, requiredKeyId = "EmeraldKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 240f, mazeBlueprintId = 5 },
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 6, title = "Level 6 — Ruby Catacombs", subtitle = "Inner Chambers", description = "Explore inner corridors. Retrieve the Ruby Key and reach the Exit Door.", requiredQuizzes = 0, requiredKeyId = "RubyKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 250f, mazeBlueprintId = 6 },
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 7, title = "Level 7 — Sapphire Sanctuary", subtitle = "Synchronized Chambers", description = "Traverse synchronized corridors. Collect the Sapphire Key and unlock the Exit Door.", requiredQuizzes = 0, requiredKeyId = "SapphireKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 270f, mazeBlueprintId = 7 },
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 8, title = "Level 8 — Amethyst Concourse", subtitle = "Multi-Layered Paths", description = "Navigate multi-layered maze halls. Retrieve the Amethyst Key and escape.", requiredQuizzes = 0, requiredKeyId = "AmethystKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 280f, mazeBlueprintId = 8 },
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 9, title = "Level 9 — Diamond Citadel", subtitle = "The Great Vault", description = "Traverse the diamond vault. Collect the Diamond Key and reach the Exit Door.", requiredQuizzes = 0, requiredKeyId = "DiamondKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 300f, mazeBlueprintId = 9 },
                new LevelInfo { difficulty = DifficultyLevel.Easy, levelNumber = 10, title = "Level 10 — Capstone Citadel", subtitle = "The Grand Trial", description = "The ultimate Easy trial. Retrieve the Master Key and conquer the Exit Door!", requiredQuizzes = 0, requiredKeyId = "MasterKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 320f, mazeBlueprintId = 10 },
            };
            levelDatabase[DifficultyLevel.Easy] = easyList;

            // 2. MEDIUM LEVELS (1–10)
            List<LevelInfo> medList = new List<LevelInfo>
            {
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 1, title = "Level 1 — Branching Junction", subtitle = "The First Fork", description = "Moderate starter labyrinth. Collect the Bronze Key and escape through the Exit Door.", requiredQuizzes = 0, requiredKeyId = "BronzeKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 150f, mazeBlueprintId = 1 },
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 2, title = "Level 2 — Circular Labyrinth", subtitle = "Twin Wings", description = "Navigate circular routes. Obtain the Silver Key and reach the Exit Door.", requiredQuizzes = 0, requiredKeyId = "SilverKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 165f, mazeBlueprintId = 2 },
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 3, title = "Level 3 — The Matrix", subtitle = "Grid Pathways", description = "Navigate routed corridors. Collect the Gold Key and pass the Exit Door.", requiredQuizzes = 0, requiredKeyId = "GoldKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 180f, mazeBlueprintId = 3 },
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 4, title = "Level 4 — Grand Normalizer", subtitle = "Deconstructed Halls", description = "Navigate complex paths. Retrieve the Cyan Key and unlock the Exit Door.", requiredQuizzes = 0, requiredKeyId = "CyanKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 195f, mazeBlueprintId = 4 },
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 5, title = "Level 5 — Echo Vaults", subtitle = "Inner Chambers", description = "Navigate deep corridors. Collect the Emerald Key and escape through the Exit Door.", requiredQuizzes = 0, requiredKeyId = "EmeraldKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 210f, mazeBlueprintId = 5 },
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 6, title = "Level 6 — Fortress Bastion", subtitle = "Stone Wings", description = "Navigate long halls. Retrieve the Ruby Key and unlock the Exit Door.", requiredQuizzes = 0, requiredKeyId = "RubyKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 220f, mazeBlueprintId = 6 },
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 7, title = "Level 7 — Ancient Sanctum", subtitle = "Winding Corridors", description = "Follow winding routes. Collect the Sapphire Key and escape through the Exit Door.", requiredQuizzes = 0, requiredKeyId = "SapphireKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 235f, mazeBlueprintId = 7 },
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 8, title = "Level 8 — Iron Citadel", subtitle = "Intersecting Chambers", description = "Navigate complex intersecting chambers. Retrieve the Amethyst Key and escape.", requiredQuizzes = 0, requiredKeyId = "AmethystKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 245f, mazeBlueprintId = 8 },
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 9, title = "Level 9 — The Great Gateway", subtitle = "Multi-Sector Labyrinth", description = "Determine optimal routes. Collect the Diamond Key and unlock the Exit Door.", requiredQuizzes = 0, requiredKeyId = "DiamondKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 255f, mazeBlueprintId = 9 },
                new LevelInfo { difficulty = DifficultyLevel.Medium, levelNumber = 10, title = "Level 10 — Grand Master Citadel", subtitle = "The Final Trial", description = "Conquer the 10th Medium citadel. Retrieve the Master Key and unlock the Exit Door!", requiredQuizzes = 0, requiredKeyId = "MasterKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 270f, mazeBlueprintId = 10 },
            };
            levelDatabase[DifficultyLevel.Medium] = medList;

            // 3. HARD LEVELS (1–10)
            List<LevelInfo> hardList = new List<LevelInfo>
            {
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 1, title = "Level 1 — Inner Bastion", subtitle = "The Rapid Trial", description = "Strict starter circuit. Collect the Bronze Key and reach the Exit Door.", requiredQuizzes = 0, requiredKeyId = "BronzeKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 120f, mazeBlueprintId = 1 },
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 2, title = "Level 2 — Shadow Corridors", subtitle = "Winding Paths", description = "Navigate complex paths under time pressure. Collect the Silver Key and reach the Exit Door.", requiredQuizzes = 0, requiredKeyId = "SilverKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 130f, mazeBlueprintId = 2 },
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 3, title = "Level 3 — The Labyrinth", subtitle = "Dead Ends & Turns", description = "Traverse intricate corridors. Retrieve the Gold Key and unlock the Exit Door.", requiredQuizzes = 0, requiredKeyId = "GoldKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 140f, mazeBlueprintId = 3 },
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 4, title = "Level 4 — Stone Sanctum", subtitle = "Chambers of Time", description = "Navigate sequence chambers. Obtain the Cyan Key and unlock the Exit Door.", requiredQuizzes = 0, requiredKeyId = "CyanKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 155f, mazeBlueprintId = 4 },
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 5, title = "Level 5 — The Catacombs", subtitle = "Deep Labyrinth", description = "Complex deep labyrinth. Collect the Emerald Key and reach the Exit Door.", requiredQuizzes = 0, requiredKeyId = "EmeraldKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 165f, mazeBlueprintId = 5 },
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 6, title = "Level 6 — Whispering Maze", subtitle = "Intricate Corridors", description = "Intricate corridors with narrow paths. Retrieve the Ruby Key and reach the Exit Door.", requiredQuizzes = 0, requiredKeyId = "RubyKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 180f, mazeBlueprintId = 6 },
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 7, title = "Level 7 — Crystal Labyrinth", subtitle = "Mirror Corridors", description = "Navigate non-blocking corridors. Collect the Sapphire Key and unlock the Exit Door.", requiredQuizzes = 0, requiredKeyId = "SapphireKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 190f, mazeBlueprintId = 7 },
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 8, title = "Level 8 — Fortress Subsystems", subtitle = "Guarded Gates", description = "Navigate multiple security gates. Retrieve the Amethyst Key and escape.", requiredQuizzes = 0, requiredKeyId = "AmethystKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 200f, mazeBlueprintId = 8 },
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 9, title = "Level 9 — Imperial Fortress", subtitle = "The Grand Chambers", description = "Multi-sector labyrinth. Collect the Diamond Key and pass the Exit Door.", requiredQuizzes = 0, requiredKeyId = "DiamondKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 210f, mazeBlueprintId = 9 },
                new LevelInfo { difficulty = DifficultyLevel.Hard, levelNumber = 10, title = "Level 10 — The Apex Citadel", subtitle = "The Ultimate Challenge", description = "The crowning trial. Retrieve the Master Key and conquer the Exit Door!", requiredQuizzes = 0, requiredKeyId = "MasterKey", spawnPosition = new Vector3(0, 0.5f, 0), timeLimit = 240f, mazeBlueprintId = 10 },
            };
            levelDatabase[DifficultyLevel.Hard] = hardList;
        }
        #endregion
    }
}
