using System;
using UnityEngine;

namespace Mazed.Core
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private const string PREF_KEY_HIGH_SCORE = "Mazed_BestScore";

        [Header("Score Tracking")]
        [Tooltip("Score earned during the current active level attempt. Resets to 0 on restart/retry/replay.")]
        [SerializeField] private int currentAttemptScore = 0;

        [Tooltip("Score earned upon completing the most recent level attempt.")]
        [SerializeField] private int lastLevelResultScore = 0;

        [Tooltip("Clear bonus awarded for completing the most recent level.")]
        [SerializeField] private int lastClearBonus = 0;

        [Tooltip("Time bonus awarded for completing the most recent level.")]
        [SerializeField] private int lastTimeBonus = 0;

        [Tooltip("Cumulative score from all completed prior levels in the current campaign run.")]
        [SerializeField] private int campaignRunScore = 0;

        [Tooltip("Highest saved personal best score across all play sessions.")]
        [SerializeField] private int savedBestScore = 0;

        // Events
        public event Action<int> OnAttemptScoreChanged;
        public event Action<int> OnCampaignScoreChanged;
        public event Action<int> OnHighScoreChanged;

        // Public Properties
        public int CurrentAttemptScore => currentAttemptScore;
        public int LastLevelResultScore => lastLevelResultScore;
        public int LastClearBonus => lastClearBonus;
        public int LastTimeBonus => lastTimeBonus;
        public int CampaignRunScore => campaignRunScore;
        public int TotalRunScore => campaignRunScore + currentAttemptScore;
        public int SavedBestScore => savedBestScore;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            savedBestScore = PlayerPrefs.GetInt(PREF_KEY_HIGH_SCORE, 0);
        }

        private void Start()
        {
            OnAttemptScoreChanged?.Invoke(currentAttemptScore);
            OnCampaignScoreChanged?.Invoke(campaignRunScore);
            OnHighScoreChanged?.Invoke(savedBestScore);
        }

        /// <summary>
        /// Adds points to the current attempt score and checks for new best score.
        /// </summary>
        public void AddPoints(int points)
        {
            if (points <= 0) return;

            currentAttemptScore += points;
            CheckAndSaveHighScore();

            OnAttemptScoreChanged?.Invoke(currentAttemptScore);
            Debug.Log($"[ScoreManager] Added {points} pts. Attempt Score: {currentAttemptScore} | Total Run: {TotalRunScore}");
        }

        /// <summary>
        /// Deducts points from current attempt score (clamped to 0).
        /// </summary>
        public void DeductPoints(int points)
        {
            if (points <= 0) return;

            currentAttemptScore = Mathf.Max(0, currentAttemptScore - points);
            OnAttemptScoreChanged?.Invoke(currentAttemptScore);
            Debug.Log($"[ScoreManager] Deducted {points} pts. Attempt Score: {currentAttemptScore}");
        }

        /// <summary>
        /// Resets the current attempt score to zero. Called on Level Restart, Retry, Replay, and Level Init.
        /// </summary>
        public void ResetAttemptScore()
        {
            currentAttemptScore = 0;
            OnAttemptScoreChanged?.Invoke(currentAttemptScore);
            Debug.Log("[ScoreManager] Current attempt score reset to 0.");
        }

        /// <summary>
        /// Finalizes current attempt upon reaching the level exit, recording level result and adding to campaign total.
        /// </summary>
        public void CommitLevelCompletion(int timeBonus, int clearBonus = 200)
        {
            lastClearBonus = clearBonus;
            lastTimeBonus = Mathf.Max(0, timeBonus);
            lastLevelResultScore = currentAttemptScore + lastClearBonus + lastTimeBonus;
            campaignRunScore += lastLevelResultScore;

            CheckAndSaveHighScore();

            OnCampaignScoreChanged?.Invoke(campaignRunScore);
            Debug.Log($"[ScoreManager] Level Committed! Level Result: {lastLevelResultScore} (Base: {currentAttemptScore}, Clear: {lastClearBonus}, Time: {lastTimeBonus}) | Campaign Total: {campaignRunScore}");
        }

        /// <summary>
        /// Completely resets campaign run score (e.g. starting a brand new game from Main Menu).
        /// </summary>
        public void ResetCampaignRun()
        {
            campaignRunScore = 0;
            currentAttemptScore = 0;
            lastLevelResultScore = 0;
            lastClearBonus = 0;
            lastTimeBonus = 0;

            OnAttemptScoreChanged?.Invoke(currentAttemptScore);
            OnCampaignScoreChanged?.Invoke(campaignRunScore);
            Debug.Log("[ScoreManager] Full campaign run reset.");
        }

        private void CheckAndSaveHighScore()
        {
            int currentTotal = TotalRunScore;
            if (currentTotal > savedBestScore)
            {
                savedBestScore = currentTotal;
                PlayerPrefs.SetInt(PREF_KEY_HIGH_SCORE, savedBestScore);
                PlayerPrefs.Save();
                OnHighScoreChanged?.Invoke(savedBestScore);
            }
        }
    }
}
