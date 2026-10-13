using System;
using UnityEngine;

namespace Mazed.Core
{
    public enum DifficultyLevel
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    [System.Serializable]
    public class DifficultySettings
    {
        public string difficultyName;
        public float[] levelTimers = new float[10];
        public int correctPoints = 100;
        public int incorrectPenalty = 25;
        public int maxRetries = 2; // -1 for unlimited
        public bool showHints = true;
        public string questionDifficultyTag = "Medium";
    }

    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        private const string PREF_KEY_DIFFICULTY = "Mazed_SelectedDifficulty";

        [Header("Current Difficulty")]
        [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Medium;

        [Header("Difficulty Presets (10 Levels Each)")]
        [SerializeField] private DifficultySettings easySettings = new DifficultySettings
        {
            difficultyName = "EASY",
            levelTimers = new float[10] { 180f, 200f, 210f, 220f, 240f, 250f, 270f, 280f, 300f, 320f },
            correctPoints = 100,
            incorrectPenalty = 10,
            maxRetries = -1, // Unlimited
            showHints = true,
            questionDifficultyTag = "Easy"
        };

        [SerializeField] private DifficultySettings mediumSettings = new DifficultySettings
        {
            difficultyName = "MEDIUM",
            levelTimers = new float[10] { 150f, 165f, 180f, 195f, 210f, 220f, 235f, 245f, 255f, 270f },
            correctPoints = 100,
            incorrectPenalty = 25,
            maxRetries = 2,
            showHints = true,
            questionDifficultyTag = "Medium"
        };

        [SerializeField] private DifficultySettings hardSettings = new DifficultySettings
        {
            difficultyName = "HARD",
            levelTimers = new float[10] { 120f, 130f, 140f, 155f, 165f, 180f, 190f, 200f, 210f, 240f },
            correctPoints = 150,
            incorrectPenalty = 50,
            maxRetries = 0,
            showHints = false,
            questionDifficultyTag = "Hard"
        };

        public DifficultyLevel CurrentDifficulty => currentDifficulty;

        public DifficultySettings CurrentSettings
        {
            get
            {
                switch (currentDifficulty)
                {
                    case DifficultyLevel.Easy: return easySettings;
                    case DifficultyLevel.Hard: return hardSettings;
                    default: return mediumSettings;
                }
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load saved difficulty
            int saved = PlayerPrefs.GetInt(PREF_KEY_DIFFICULTY, (int)DifficultyLevel.Medium);
            currentDifficulty = (DifficultyLevel)Mathf.Clamp(saved, 0, 2);
        }

        public void SetDifficulty(DifficultyLevel difficulty)
        {
            currentDifficulty = difficulty;
            PlayerPrefs.SetInt(PREF_KEY_DIFFICULTY, (int)difficulty);
            PlayerPrefs.Save();
            Debug.Log($"[DifficultyManager] Difficulty set to: {difficulty}");

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnDifficultyChanged(difficulty);
            }
        }

        public float GetTimerForLevel(int levelIndex)
        {
            return GetTimerForDifficultyAndLevel(currentDifficulty, levelIndex);
        }

        public float GetTimerForDifficultyAndLevel(DifficultyLevel diff, int levelIndex)
        {
            int index = Mathf.Clamp(levelIndex - 1, 0, 9);
            switch (diff)
            {
                case DifficultyLevel.Easy: return easySettings.levelTimers[index];
                case DifficultyLevel.Hard: return hardSettings.levelTimers[index];
                default: return mediumSettings.levelTimers[index];
            }
        }

        public int GetCorrectPoints() => CurrentSettings.correctPoints;
        public int GetIncorrectPenalty() => CurrentSettings.incorrectPenalty;
        public int GetMaxRetries() => CurrentSettings.maxRetries;
        public string GetQuestionDifficultyTag() => CurrentSettings.questionDifficultyTag;
        public bool AreHintsEnabled() => CurrentSettings.showHints;
    }
}
