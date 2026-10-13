using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;
using Mazed.UI;
using Mazed.Gameplay;
using Mazed.Environment;
using Mazed.Quiz;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Mazed.Core
{
    public enum GameState
    {
        MainMenu,
        DifficultySelect,
        LevelSelect,
        Playing,
        Paused,
        LevelComplete,
        GameOver,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Playing;

        [Header("Timer Settings")]
        [SerializeField] private float initialTimeInSeconds = 480f;
        private float remainingTime;

        [Header("Scoring Rules")]
        public int correctPoints = 100;
        public int incorrectPenalty = 25;

        [Header("Progression Targets")]
        public int totalCheckpoints = 1;

        [Header("Player Reference")]
        [SerializeField] private GameObject playerObject;
        [SerializeField] private Transform spawnPoint;

        // Inventory and stats
        private HashSet<string> collectedKeyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> completedQuizCheckpointIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private int correctAnswersCount = 0;
        private int incorrectAnswersCount = 0;
        private float levelStartTime;

        // Active Level info
        private LevelInfo currentLevelInfo;

        // Events
        public event Action<float> OnTimerTick;
        public event Action<int> OnScoreUpdated;
        public event Action<HashSet<string>> OnInventoryUpdated;
        public event Action<int, int> OnQuizProgressUpdated;
        public event Action<GameState> OnStateChanged;
        public event Action<string> OnObjectiveUpdated;

        // Properties
        public GameState CurrentState => currentState;
        public float RemainingTime => remainingTime;
        public int CurrentScore => ScoreManager.Instance != null ? ScoreManager.Instance.CurrentAttemptScore : 0;
        public int CompletedQuizzesCount => completedQuizCheckpointIds.Count;
        public int CorrectAnswersCount => correctAnswersCount;
        public int IncorrectAnswersCount => incorrectAnswersCount;
        public float ElapsedTime => initialTimeInSeconds - remainingTime;
        public bool IsPlaying => currentState == GameState.Playing;
        public LevelInfo ActiveLevelInfo => currentLevelInfo;
        public int LastEarnedStars { get; private set; } = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            FindPlayerReferences();

            // Connect ScoreManager event
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnAttemptScoreChanged += (s) => OnScoreUpdated?.Invoke(s);
            }

            // Check if LevelManager exists, otherwise initialize default
            if (LevelManager.Instance != null)
            {
                currentLevelInfo = LevelManager.Instance.CurrentLevelInfo;
                if (currentLevelInfo != null && currentLevelInfo.timeLimit > 0f)
                {
                    initialTimeInSeconds = currentLevelInfo.timeLimit;
                }
            }
            remainingTime = initialTimeInSeconds;

            // Always boot cleanly to the Landing Page / Main Menu with free cursor
            GoToMainMenu();
        }

        public void InitializeLevel(LevelInfo level)
        {
            currentLevelInfo = level;
            levelStartTime = Time.time;

            // 1. Reset Score for this attempt
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetAttemptScore();
            }

            // 2. Fetch difficulty settings & level-specific timer
            if (level != null && level.timeLimit > 0f)
            {
                initialTimeInSeconds = level.timeLimit;
            }
            else if (DifficultyManager.Instance != null && level != null)
            {
                initialTimeInSeconds = DifficultyManager.Instance.GetTimerForLevel(level.levelNumber);
            }
            else
            {
                initialTimeInSeconds = 180f;
            }

            if (DifficultyManager.Instance != null)
            {
                correctPoints = DifficultyManager.Instance.GetCorrectPoints();
                incorrectPenalty = DifficultyManager.Instance.GetIncorrectPenalty();
            }
            else
            {
                correctPoints = 100;
                incorrectPenalty = 25;
            }

            remainingTime = initialTimeInSeconds;
            totalCheckpoints = level != null ? level.requiredQuizzes : 1;

            // 3. Clear objectives and stats
            collectedKeyIds.Clear();
            completedQuizCheckpointIds.Clear();
            correctAnswersCount = 0;
            incorrectAnswersCount = 0;
            LastEarnedStars = 1;

            if (HintManager.Instance != null)
            {
                int startingHints = (level != null && level.difficulty == DifficultyLevel.Easy) ? 2 : 1;
                HintManager.Instance.ResetForLevel(startingHints);
            }

            if (QuestionBank.Instance != null)
            {
                QuestionBank.Instance.ResetProgress();
            }

            // 4. Reposition Player to level spawn point
            FindPlayerReferences();
            if (playerObject != null && level != null)
            {
                CharacterController cc = playerObject.GetComponentInChildren<CharacterController>();
                if (cc != null) cc.enabled = false;

                Transform robot = playerObject.transform.Find("Robot");
                if (robot != null)
                {
                    robot.localPosition = Vector3.zero;
                    robot.localRotation = Quaternion.identity;
                }

                playerObject.transform.position = level.spawnPosition;
                playerObject.transform.rotation = Quaternion.identity;

                Physics.SyncTransforms();

                if (cc != null) cc.enabled = true;

                var tpc = playerObject.GetComponentInChildren<ThirdPersonController>();
                if (tpc != null)
                {
                    tpc.ResetMovementState();
                    tpc.ResetCameraRotation(0f);
                }

                var respawn = playerObject.GetComponentInChildren<StarterAssets.RespawnPlayer>();
                if (respawn != null)
                {
                    respawn.SetSpawnPosition(level.spawnPosition, Quaternion.identity);
                }

                var rb = playerObject.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                var inputs = playerObject.GetComponentInChildren<StarterAssets.StarterAssetsInputs>();
                if (inputs != null)
                {
                    inputs.isInputEnabled = true;
                    inputs.move = Vector2.zero;
                    inputs.look = Vector2.zero;
                    inputs.jump = false;
                    inputs.sprint = false;
                    inputs.SetCursorState(true);
                }
            }

            Time.timeScale = 1f;
            SetGameState(GameState.Playing);

            // 5. Notify initial HUD states
            OnTimerTick?.Invoke(remainingTime);
            OnScoreUpdated?.Invoke(0);
            OnInventoryUpdated?.Invoke(collectedKeyIds);
            OnQuizProgressUpdated?.Invoke(completedQuizCheckpointIds.Count, totalCheckpoints);
            UpdateObjectiveStatus();
            UIManager.Instance?.HideAllModals();
            UIManager.Instance?.UpdateHeaderMeta();

            Debug.Log($"[GameManager] Level {level.levelNumber} initialized with clean state (Score: 0, Time: {remainingTime}s).");
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                remainingTime -= Time.deltaTime;
                if (remainingTime <= 0f)
                {
                    remainingTime = 0f;
                    TriggerGameOver();
                }

                OnTimerTick?.Invoke(remainingTime);
            }
            else
            {
                // Maintain visible/unlocked cursor in all menu states
                if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }

            // Handle Escape key across all states
            bool escapePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                escapePressed = true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (!escapePressed && Input.GetKeyDown(KeyCode.Escape))
            {
                escapePressed = true;
            }
#endif
            if (escapePressed)
            {
                HandleEscapeKey();
            }
        }

        public void HandleEscapeKey()
        {
            if (QuizManager.Instance != null && QuizManager.Instance.IsQuizOpen)
            {
                QuizManager.Instance.CloseQuiz();
                return;
            }

            if (UIManager.Instance != null && UIManager.Instance.IsAnySubModalOpen)
            {
                UIManager.Instance.CloseSubModals();
                SetCursorLocked(false);
                return;
            }

            if (currentState == GameState.Playing)
            {
                TogglePause();
            }
            else if (currentState == GameState.Paused)
            {
                TogglePause();
            }
            else if (currentState == GameState.DifficultySelect || currentState == GameState.LevelSelect)
            {
                GoToMainMenu();
            }
        }

        public void FindPlayerReferences()
        {
            if (playerObject == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }

            if (spawnPoint == null)
            {
                GameObject sp = GameObject.Find("SpawnPoint") ?? GameObject.Find("SpawnPoint_1");
                if (sp != null)
                {
                    spawnPoint = sp.transform;
                }
            }
        }

        public void SetGameState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    SetPlayerInputActive(true);
                    SetCursorLocked(true);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;

                case GameState.MainMenu:
                case GameState.DifficultySelect:
                case GameState.LevelSelect:
                case GameState.Paused:
                case GameState.LevelComplete:
                case GameState.GameOver:
                case GameState.Victory:
                    Time.timeScale = 0f;
                    SetPlayerInputActive(false);
                    SetCursorLocked(false);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }

            // Trigger audio and UI where appropriate
            if (newState == GameState.GameOver)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOverSFX();
                if (UIManager.Instance != null) UIManager.Instance.ShowGameOverScreen();
            }
            else if (newState == GameState.LevelComplete)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayVictorySFX();
                if (UIManager.Instance != null) UIManager.Instance.ShowLevelCompleteScreen();
            }
            else if (newState == GameState.Victory)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayVictorySFX();
                if (UIManager.Instance != null) UIManager.Instance.ShowVictoryScreen();
            }
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
                if (UIManager.Instance != null) UIManager.Instance.ShowPauseMenu(true);
            }
            else if (currentState == GameState.Paused)
            {
                if (UIManager.Instance != null) UIManager.Instance.ShowPauseMenu(false);
                SetGameState(GameState.Playing);
            }
        }

        public void CollectKey(string keyId, string displayName, int reward)
        {
            if (string.IsNullOrEmpty(keyId)) return;

            if (!collectedKeyIds.Contains(keyId))
            {
                collectedKeyIds.Add(keyId);
                AddScore(reward);

                OnInventoryUpdated?.Invoke(collectedKeyIds);
                UpdateObjectiveStatus();

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage($"Acquired {displayName}! (+{reward} pts)", 3f);
                }
                Debug.Log($"[GameManager] Collected key: {keyId}");
            }
        }

        public bool HasKey(string keyId)
        {
            return !string.IsNullOrEmpty(keyId) && collectedKeyIds.Contains(keyId);
        }

        public void RecordQuizAnswer(string checkpointId, bool isCorrect)
        {
            if (isCorrect)
            {
                correctAnswersCount++;
                AddScore(correctPoints);

                if (!string.IsNullOrEmpty(checkpointId))
                {
                    completedQuizCheckpointIds.Add(checkpointId);
                }

                OnQuizProgressUpdated?.Invoke(completedQuizCheckpointIds.Count, totalCheckpoints);
                UpdateObjectiveStatus();
            }
            else
            {
                incorrectAnswersCount++;
                DeductScore(incorrectPenalty);
            }
        }

        public bool IsQuizCompleted(string checkpointId)
        {
            return !string.IsNullOrEmpty(checkpointId) && completedQuizCheckpointIds.Contains(checkpointId);
        }

        public void UpdateObjectiveStatus()
        {
            if (currentLevelInfo == null) return;

            string keyName = !string.IsNullOrEmpty(currentLevelInfo.requiredKeyId) 
                ? currentLevelInfo.requiredKeyId.Replace("Key", " Key") 
                : "Level Key";
            bool hasKey = string.IsNullOrEmpty(currentLevelInfo.requiredKeyId) || HasKey(currentLevelInfo.requiredKeyId);

            string objectiveText;
            if (!hasKey)
            {
                objectiveText = $"MISSION OBJECTIVE: Find the {keyName} hidden in the maze.";
            }
            else
            {
                objectiveText = $"KEY ACQUIRED! Reach and unlock the Exit Door to complete the level!";
            }

            OnObjectiveUpdated?.Invoke(objectiveText);
        }

        public void AddScore(int points)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPoints(points);
            }
            else
            {
                OnScoreUpdated?.Invoke(points);
            }
        }

        public void DeductScore(int points)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DeductPoints(points);
            }
        }

        public void CompleteLevel()
        {
            if (currentState == GameState.Victory || currentState == GameState.LevelComplete || currentState == GameState.GameOver) return;

            // Award remaining time bonus points: 1 pt per 2 seconds left
            int timeBonus = Mathf.RoundToInt(remainingTime / 2f);

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.CommitLevelCompletion(timeBonus, 200);
            }

            int finalAttemptScore = ScoreManager.Instance != null ? ScoreManager.Instance.LastLevelResultScore : CurrentScore;
            float completionTime = ElapsedTime;

            int hintsUsed = HintManager.Instance != null ? HintManager.Instance.HintsUsedThisAttempt : 0;
            int stars = 1;
            if (LevelManager.Instance != null && currentLevelInfo != null)
            {
                stars = LevelManager.Instance.CalculateStarRating(
                    currentLevelInfo.difficulty,
                    currentLevelInfo.levelNumber,
                    completionTime,
                    currentLevelInfo.timeLimit,
                    correctAnswersCount,
                    incorrectAnswersCount,
                    hintsUsed
                );
            }
            LastEarnedStars = stars;

            if (LevelManager.Instance != null && currentLevelInfo != null)
            {
                LevelManager.Instance.SaveLevelCompletion(currentLevelInfo.difficulty, currentLevelInfo.levelNumber, finalAttemptScore, completionTime, stars);

                if (LevelManager.Instance.IsCurrentLevelLastOfDifficulty)
                {
                    SetGameState(GameState.Victory);
                }
                else
                {
                    SetGameState(GameState.LevelComplete);
                }
            }
            else
            {
                SetGameState(GameState.LevelComplete);
            }
        }

        public void TriggerGameOver()
        {
            if (currentState == GameState.Victory || currentState == GameState.LevelComplete || currentState == GameState.GameOver) return;
            SetGameState(GameState.GameOver);
        }

        /// <summary>
        /// Restores current level to initial playable state: 0 attempt score, reset timer, reset player position/velocity, restored keys & doors.
        /// </summary>
        public void RestartCurrentLevel()
        {
            Time.timeScale = 1f;

            // 1. Reset current attempt score to zero
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetAttemptScore();
            }

            // 2. Reset timer
            if (currentLevelInfo != null && currentLevelInfo.timeLimit > 0f)
            {
                initialTimeInSeconds = currentLevelInfo.timeLimit;
            }
            else if (DifficultyManager.Instance != null && currentLevelInfo != null)
            {
                initialTimeInSeconds = DifficultyManager.Instance.GetTimerForLevel(currentLevelInfo.levelNumber);
            }
            else
            {
                initialTimeInSeconds = 180f;
            }
            remainingTime = initialTimeInSeconds;

            // 3. Reset keys, quizzes, and stats
            collectedKeyIds.Clear();
            completedQuizCheckpointIds.Clear();
            correctAnswersCount = 0;
            incorrectAnswersCount = 0;
            LastEarnedStars = 1;

            if (HintManager.Instance != null)
            {
                int startingHints = (currentLevelInfo != null && currentLevelInfo.difficulty == DifficultyLevel.Easy) ? 2 : 1;
                HintManager.Instance.ResetForLevel(startingHints);
            }

            if (QuestionBank.Instance != null)
            {
                QuestionBank.Instance.ResetProgress();
            }

            // 4. Rebuild maze geometry (restoring all keys, closed/locked doors, active terminals)
            MazeBuilder builder = FindFirstObjectByType<MazeBuilder>();
            if (builder != null && currentLevelInfo != null)
            {
                builder.BuildLevel(currentLevelInfo.difficulty, currentLevelInfo.levelNumber);
            }

            // 5. Reposition Player & reset movement velocity
            FindPlayerReferences();
            if (playerObject != null && currentLevelInfo != null)
            {
                CharacterController cc = playerObject.GetComponentInChildren<CharacterController>();
                if (cc != null) cc.enabled = false;

                Transform robot = playerObject.transform.Find("Robot");
                if (robot != null)
                {
                    robot.localPosition = Vector3.zero;
                    robot.localRotation = Quaternion.identity;
                }

                playerObject.transform.position = currentLevelInfo.spawnPosition;
                playerObject.transform.rotation = Quaternion.identity;

                Physics.SyncTransforms();

                if (cc != null) cc.enabled = true;

                var tpc = playerObject.GetComponentInChildren<ThirdPersonController>();
                if (tpc != null)
                {
                    tpc.ResetMovementState();
                    tpc.ResetCameraRotation(0f);
                }

                var respawn = playerObject.GetComponentInChildren<StarterAssets.RespawnPlayer>();
                if (respawn != null)
                {
                    respawn.SetSpawnPosition(currentLevelInfo.spawnPosition, Quaternion.identity);
                }

                var rb = playerObject.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                var inputs = playerObject.GetComponentInChildren<StarterAssets.StarterAssetsInputs>();
                if (inputs != null)
                {
                    inputs.isInputEnabled = true;
                    inputs.move = Vector2.zero;
                    inputs.look = Vector2.zero;
                    inputs.jump = false;
                    inputs.sprint = false;
                    inputs.SetCursorState(true);
                }
            }

            // 6. Close UI modals & update header
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideAllModals();
                UIManager.Instance.UpdateHeaderMeta();
            }

            Time.timeScale = 1f;
            SetGameState(GameState.Playing);

            // 7. Refresh HUD displays immediately
            OnTimerTick?.Invoke(remainingTime);
            OnScoreUpdated?.Invoke(0);
            OnInventoryUpdated?.Invoke(collectedKeyIds);
            OnQuizProgressUpdated?.Invoke(0, totalCheckpoints);
            UpdateObjectiveStatus();

            Debug.Log($"[GameManager] Level {currentLevelInfo?.levelNumber} restarted with 0 score and clean state.");
        }

        public void NextLevel()
        {
            Time.timeScale = 1f;
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 0f;
            SetGameState(GameState.MainMenu);
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMainMenu();
            }
        }

        public void StartNewGame()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetCampaignRun();
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(1);
            }
            else
            {
                RestartCurrentLevel();
            }
        }

        public void SetPlayerInputActive(bool active)
        {
            if (playerObject == null) FindPlayerReferences();

            if (playerObject != null)
            {
                var inputComponent = playerObject.GetComponentInChildren<StarterAssetsInputs>();
                if (inputComponent != null)
                {
                    inputComponent.isInputEnabled = active;
                }

#if ENABLE_INPUT_SYSTEM
                var playerInput = playerObject.GetComponentInChildren<UnityEngine.InputSystem.PlayerInput>();
                if (playerInput != null)
                {
                    if (active) playerInput.ActivateInput();
                    else playerInput.DeactivateInput();
                }
#endif
            }
        }

        public void SetCursorLocked(bool locked)
        {
            if (playerObject == null) FindPlayerReferences();

            if (playerObject != null)
            {
                var inputComponent = playerObject.GetComponentInChildren<StarterAssetsInputs>();
                if (inputComponent != null)
                {
                    inputComponent.SetCursorState(locked);
                }
            }

            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
