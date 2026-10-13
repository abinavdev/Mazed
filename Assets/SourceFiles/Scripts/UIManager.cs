using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mazed.Core;
using Mazed.Environment;

namespace Mazed.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD Displays")]
        [SerializeField] private GameObject hudRoot;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI keysText;
        [SerializeField] private TextMeshProUGUI quizProgressText;
        [SerializeField] private Button hudHintButton;
        [SerializeField] private TextMeshProUGUI hudHintText;
        [SerializeField] private TextMeshProUGUI objectiveBannerText;
        [SerializeField] private TextMeshProUGUI messageBannerText;
        [SerializeField] private GameObject messageBannerPanel;
        [SerializeField] private TextMeshProUGUI interactionPromptText;
        [SerializeField] private GameObject interactionPromptPanel;

        [Header("Landing / Main Menu Panel")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button selectDifficultyButton;
        [SerializeField] private Button selectLevelButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button landingMusicButton;
        [SerializeField] private TextMeshProUGUI landingMusicText;
        [SerializeField] private Button landingSfxButton;
        [SerializeField] private TextMeshProUGUI landingSfxText;

        [Header("Difficulty Selection Panel")]
        [SerializeField] private GameObject difficultyPanel;
        [SerializeField] private Button difficultyBackButton;
        [SerializeField] private Button easyButton;
        [SerializeField] private Button mediumButton;
        [SerializeField] private Button hardButton;
        [SerializeField] private TextMeshProUGUI currentDifficultyLabel;

        [Header("Level Selection Panel (10 Level Cards)")]
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private TextMeshProUGUI levelSelectTitleText;
        [SerializeField] private Button levelSelectEasyTabButton;
        [SerializeField] private Button levelSelectMediumTabButton;
        [SerializeField] private Button levelSelectHardTabButton;
        [SerializeField] private Button[] levelButtons; // Array of 10 buttons
        [SerializeField] private Button levelSelectBackButton;

        [Header("Settings Modal")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button settingsMusicButton;
        [SerializeField] private TextMeshProUGUI settingsMusicText;
        [SerializeField] private Button settingsSfxButton;
        [SerializeField] private TextMeshProUGUI settingsSfxText;
        [SerializeField] private Button settingsCloseButton;

        [Header("How To Play Modal")]
        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private Button howToPlayCloseButton;

        [Header("Pause Menu")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button pauseResumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseLevelSelectButton;
        [SerializeField] private Button pauseMainMenuButton;
        [SerializeField] private Button pauseMusicButton;
        [SerializeField] private TextMeshProUGUI pauseMusicText;
        [SerializeField] private Button pauseSfxButton;
        [SerializeField] private TextMeshProUGUI pauseSfxText;

        [Header("Level Complete Screen")]
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private TextMeshProUGUI levelCompleteTitleText;
        [SerializeField] private TextMeshProUGUI levelCompleteScoreText;
        [SerializeField] private TextMeshProUGUI levelCompleteTimeText;
        [SerializeField] private TextMeshProUGUI levelCompleteStatsText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button replayLevelButton;
        [SerializeField] private Button levelCompleteLevelSelectButton;
        [SerializeField] private Button levelCompleteMainMenuButton;

        [Header("Game Over / Level Failed Screen")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverTitleText;
        [SerializeField] private TextMeshProUGUI gameOverScoreText;
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button gameOverLevelSelectButton;
        [SerializeField] private Button gameOverMainMenuButton;

        [Header("Final Victory / Difficulty Completed Screen")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TextMeshProUGUI victoryTitleText;
        [SerializeField] private TextMeshProUGUI victoryScoreText;
        [SerializeField] private TextMeshProUGUI victoryTimeText;
        [SerializeField] private TextMeshProUGUI victoryStatsText;
        [SerializeField] private Button victoryPlayAgainButton;
        [SerializeField] private Button victoryLevelSelectButton;
        [SerializeField] private Button victoryMainMenuButton;

        private Coroutine messageCoroutine;
        private DifficultyLevel activeSelectTabDifficulty = DifficultyLevel.Medium;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            HookButtons();
            HideAllModals();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTimerTick += UpdateTimerDisplay;
                GameManager.Instance.OnScoreUpdated += UpdateScoreDisplay;
                GameManager.Instance.OnInventoryUpdated += UpdateInventoryDisplay;
                GameManager.Instance.OnQuizProgressUpdated += UpdateQuizProgressDisplay;
                GameManager.Instance.OnObjectiveUpdated += UpdateObjectiveDisplay;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnAttemptScoreChanged += UpdateScoreDisplay;
            }

            if (hudHintButton != null)
            {
                hudHintButton.onClick.RemoveAllListeners();
                hudHintButton.onClick.AddListener(() =>
                {
                    if (Mazed.Gameplay.HintManager.Instance != null)
                    {
                        Mazed.Gameplay.HintManager.Instance.UseHint();
                    }
                });
            }

            if (Mazed.Gameplay.HintManager.Instance != null)
            {
                Mazed.Gameplay.HintManager.Instance.OnHintsChanged += UpdateHintDisplay;
                UpdateHintDisplay(Mazed.Gameplay.HintManager.Instance.CurrentHints);
            }

            UpdateHeaderMeta();
            UpdateAudioButtonVisuals();
            UpdateDifficultyButtonVisuals();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTimerTick -= UpdateTimerDisplay;
                GameManager.Instance.OnScoreUpdated -= UpdateScoreDisplay;
                GameManager.Instance.OnInventoryUpdated -= UpdateInventoryDisplay;
                GameManager.Instance.OnQuizProgressUpdated -= UpdateQuizProgressDisplay;
                GameManager.Instance.OnObjectiveUpdated -= UpdateObjectiveDisplay;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnAttemptScoreChanged -= UpdateScoreDisplay;
            }

            if (Mazed.Gameplay.HintManager.Instance != null)
            {
                Mazed.Gameplay.HintManager.Instance.OnHintsChanged -= UpdateHintDisplay;
            }
        }

        public void HookButtons()
        {
            // Main Menu / Landing Page
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayClicked);
            }
            if (selectDifficultyButton != null)
            {
                selectDifficultyButton.onClick.RemoveAllListeners();
                selectDifficultyButton.onClick.AddListener(ShowDifficultySelection);
            }
            if (difficultyBackButton != null)
            {
                difficultyBackButton.onClick.RemoveAllListeners();
                difficultyBackButton.onClick.AddListener(ShowMainMenu);
            }
            if (selectLevelButton != null)
            {
                selectLevelButton.onClick.RemoveAllListeners();
                selectLevelButton.onClick.AddListener(() => ShowLevelSelect());
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(() => ShowSettings(true));
            }
            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(OnQuitClicked);
            }
            if (howToPlayButton != null)
            {
                howToPlayButton.onClick.RemoveAllListeners();
                howToPlayButton.onClick.AddListener(() => ShowHowToPlay(true));
            }
            if (howToPlayCloseButton != null)
            {
                howToPlayCloseButton.onClick.RemoveAllListeners();
                howToPlayCloseButton.onClick.AddListener(() => ShowHowToPlay(false));
            }

            // Audio buttons (Landing page)
            if (landingMusicButton != null)
            {
                landingMusicButton.onClick.RemoveAllListeners();
                landingMusicButton.onClick.AddListener(ToggleMusic);
            }
            if (landingSfxButton != null)
            {
                landingSfxButton.onClick.RemoveAllListeners();
                landingSfxButton.onClick.AddListener(ToggleSfx);
            }

            // Difficulty Buttons on Landing Page
            if (easyButton != null)
            {
                easyButton.onClick.RemoveAllListeners();
                easyButton.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Easy));
            }
            if (mediumButton != null)
            {
                mediumButton.onClick.RemoveAllListeners();
                mediumButton.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Medium));
            }
            if (hardButton != null)
            {
                hardButton.onClick.RemoveAllListeners();
                hardButton.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Hard));
            }

            // Level Selection Tabs
            if (levelSelectEasyTabButton != null)
            {
                levelSelectEasyTabButton.onClick.RemoveAllListeners();
                levelSelectEasyTabButton.onClick.AddListener(() => ShowLevelSelect(DifficultyLevel.Easy));
            }
            if (levelSelectMediumTabButton != null)
            {
                levelSelectMediumTabButton.onClick.RemoveAllListeners();
                levelSelectMediumTabButton.onClick.AddListener(() => ShowLevelSelect(DifficultyLevel.Medium));
            }
            if (levelSelectHardTabButton != null)
            {
                levelSelectHardTabButton.onClick.RemoveAllListeners();
                levelSelectHardTabButton.onClick.AddListener(() => ShowLevelSelect(DifficultyLevel.Hard));
            }

            // 10 Level Cards Buttons
            if (levelButtons != null)
            {
                for (int i = 0; i < levelButtons.Length; i++)
                {
                    int lvl = i + 1;
                    if (levelButtons[i] != null)
                    {
                        levelButtons[i].onClick.RemoveAllListeners();
                        levelButtons[i].onClick.AddListener(() => SelectLevel(lvl));
                    }
                }
            }
            if (levelSelectBackButton != null)
            {
                levelSelectBackButton.onClick.RemoveAllListeners();
                levelSelectBackButton.onClick.AddListener(ShowMainMenu);
            }

            // Settings Modal
            if (settingsMusicButton != null)
            {
                settingsMusicButton.onClick.RemoveAllListeners();
                settingsMusicButton.onClick.AddListener(ToggleMusic);
            }
            if (settingsSfxButton != null)
            {
                settingsSfxButton.onClick.RemoveAllListeners();
                settingsSfxButton.onClick.AddListener(ToggleSfx);
            }
            if (settingsCloseButton != null)
            {
                settingsCloseButton.onClick.RemoveAllListeners();
                settingsCloseButton.onClick.AddListener(() => ShowSettings(false));
            }

            // Pause Menu
            if (pauseResumeButton != null)
            {
                pauseResumeButton.onClick.RemoveAllListeners();
                pauseResumeButton.onClick.AddListener(() => GameManager.Instance?.TogglePause());
            }
            if (pauseRestartButton != null)
            {
                pauseRestartButton.onClick.RemoveAllListeners();
                pauseRestartButton.onClick.AddListener(() => {
                    HideAllModals();
                    GameManager.Instance?.RestartCurrentLevel();
                });
            }
            if (pauseLevelSelectButton != null)
            {
                pauseLevelSelectButton.onClick.RemoveAllListeners();
                pauseLevelSelectButton.onClick.AddListener(() => ShowLevelSelect());
            }
            if (pauseMainMenuButton != null)
            {
                pauseMainMenuButton.onClick.RemoveAllListeners();
                pauseMainMenuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
            }
            if (pauseMusicButton != null)
            {
                pauseMusicButton.onClick.RemoveAllListeners();
                pauseMusicButton.onClick.AddListener(ToggleMusic);
            }
            if (pauseSfxButton != null)
            {
                pauseSfxButton.onClick.RemoveAllListeners();
                pauseSfxButton.onClick.AddListener(ToggleSfx);
            }

            // Level Complete Screen
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveAllListeners();
                nextLevelButton.onClick.AddListener(() => {
                    HideAllModals();
                    GameManager.Instance?.NextLevel();
                });
            }
            if (replayLevelButton != null)
            {
                replayLevelButton.onClick.RemoveAllListeners();
                replayLevelButton.onClick.AddListener(() => {
                    HideAllModals();
                    GameManager.Instance?.RestartCurrentLevel();
                });
            }
            if (levelCompleteLevelSelectButton != null)
            {
                levelCompleteLevelSelectButton.onClick.RemoveAllListeners();
                levelCompleteLevelSelectButton.onClick.AddListener(() => ShowLevelSelect());
            }
            if (levelCompleteMainMenuButton != null)
            {
                levelCompleteMainMenuButton.onClick.RemoveAllListeners();
                levelCompleteMainMenuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
            }

            // Game Over / Level Failed Screen
            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.RemoveAllListeners();
                gameOverRestartButton.onClick.AddListener(() => {
                    HideAllModals();
                    GameManager.Instance?.RestartCurrentLevel();
                });
            }
            if (gameOverLevelSelectButton != null)
            {
                gameOverLevelSelectButton.onClick.RemoveAllListeners();
                gameOverLevelSelectButton.onClick.AddListener(() => ShowLevelSelect());
            }
            if (gameOverMainMenuButton != null)
            {
                gameOverMainMenuButton.onClick.RemoveAllListeners();
                gameOverMainMenuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
            }

            // Victory / Difficulty Completed Screen
            if (victoryPlayAgainButton != null)
            {
                victoryPlayAgainButton.onClick.RemoveAllListeners();
                victoryPlayAgainButton.onClick.AddListener(() => {
                    HideAllModals();
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.LoadLevel(10);
                    }
                });
            }
            if (victoryLevelSelectButton != null)
            {
                victoryLevelSelectButton.onClick.RemoveAllListeners();
                victoryLevelSelectButton.onClick.AddListener(() => ShowLevelSelect());
            }
            if (victoryMainMenuButton != null)
            {
                victoryMainMenuButton.onClick.RemoveAllListeners();
                victoryMainMenuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
            }
        }

        public void HideAllModals()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (difficultyPanel != null) difficultyPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (messageBannerPanel != null) messageBannerPanel.SetActive(false);
            if (interactionPromptPanel != null) interactionPromptPanel.SetActive(false);
        }

        public void ToggleMusic()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleMusicMute();
            }
            UpdateAudioButtonVisuals();
        }

        public void ToggleSfx()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleSfxMute();
            }
            UpdateAudioButtonVisuals();
        }

        public void UpdateAudioButtonVisuals()
        {
            bool musicMuted = AudioManager.Instance != null && AudioManager.Instance.isMusicMuted;
            bool sfxMuted = AudioManager.Instance != null && AudioManager.Instance.isSfxMuted;

            string musicStr = musicMuted ? "MUSIC: MUTED" : "MUSIC: ON";
            string sfxStr = sfxMuted ? "SFX: MUTED" : "SFX: ON";

            Color musicActiveCol = new Color(0.15f, 0.55f, 0.35f, 1f);
            Color musicMutedCol = new Color(0.55f, 0.2f, 0.2f, 1f);
            Color sfxActiveCol = new Color(0.18f, 0.45f, 0.65f, 1f);
            Color sfxMutedCol = new Color(0.55f, 0.2f, 0.2f, 1f);

            if (landingMusicText != null) landingMusicText.text = musicStr;
            if (landingMusicButton != null)
            {
                var img = landingMusicButton.GetComponent<Image>();
                if (img != null) img.color = musicMuted ? musicMutedCol : musicActiveCol;
            }

            if (landingSfxText != null) landingSfxText.text = sfxStr;
            if (landingSfxButton != null)
            {
                var img = landingSfxButton.GetComponent<Image>();
                if (img != null) img.color = sfxMuted ? sfxMutedCol : sfxActiveCol;
            }

            if (pauseMusicText != null) pauseMusicText.text = musicStr;
            if (pauseMusicButton != null)
            {
                var img = pauseMusicButton.GetComponent<Image>();
                if (img != null) img.color = musicMuted ? musicMutedCol : musicActiveCol;
            }

            if (pauseSfxText != null) pauseSfxText.text = sfxStr;
            if (pauseSfxButton != null)
            {
                var img = pauseSfxButton.GetComponent<Image>();
                if (img != null) img.color = sfxMuted ? sfxMutedCol : sfxActiveCol;
            }

            if (settingsMusicText != null) settingsMusicText.text = musicStr;
            if (settingsMusicButton != null)
            {
                var img = settingsMusicButton.GetComponent<Image>();
                if (img != null) img.color = musicMuted ? musicMutedCol : musicActiveCol;
            }

            if (settingsSfxText != null) settingsSfxText.text = sfxStr;
            if (settingsSfxButton != null)
            {
                var img = settingsSfxButton.GetComponent<Image>();
                if (img != null) img.color = sfxMuted ? sfxMutedCol : sfxActiveCol;
            }
        }

        public void ShowDifficultySelection()
        {
            HideAllModals();
            if (difficultyPanel != null) difficultyPanel.SetActive(true);
            UpdateDifficultyButtonVisuals();
        }

        public void SelectDifficulty(DifficultyLevel diff)
        {
            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.SetDifficulty(diff);
            }
            activeSelectTabDifficulty = diff;
            UpdateDifficultyButtonVisuals();
            ShowLevelSelect(diff);
        }

        public void UpdateDifficultyButtonVisuals()
        {
            DifficultyLevel currentDiff = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentDifficulty : DifficultyLevel.Medium;

            SetDiffBtnVisual(easyButton, currentDiff == DifficultyLevel.Easy, new Color(0.15f, 0.68f, 0.35f, 1f));
            SetDiffBtnVisual(mediumButton, currentDiff == DifficultyLevel.Medium, new Color(0.18f, 0.55f, 0.85f, 1f));
            SetDiffBtnVisual(hardButton, currentDiff == DifficultyLevel.Hard, new Color(0.85f, 0.25f, 0.25f, 1f));

            if (currentDifficultyLabel != null)
            {
                switch (currentDiff)
                {
                    case DifficultyLevel.Easy:
                        currentDifficultyLabel.text = "ACTIVE DIFFICULTY: <color=#2ECC71><b>EASY (10 LEVELS)</b></color>  •  Generous Timers & Open Mazes";
                        break;
                    case DifficultyLevel.Hard:
                        currentDifficultyLabel.text = "ACTIVE DIFFICULTY: <color=#E74C3C><b>HARD (10 LEVELS)</b></color>  •  Strict Timers & Complex Labyrinths";
                        break;
                    default:
                        currentDifficultyLabel.text = "ACTIVE DIFFICULTY: <color=#00E5FF><b>MEDIUM (10 LEVELS)</b></color>  •  Balanced Timers & 10 Mazes";
                        break;
                }
            }

            UpdateHeaderMeta();
        }

        private void SetDiffBtnVisual(Button btn, bool isSelected, Color activeColor)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isSelected ? activeColor : new Color(0.18f, 0.22f, 0.28f, 0.9f);
            }
        }

        public void ShowHowToPlay(bool show)
        {
            if (howToPlayPanel != null)
            {
                howToPlayPanel.SetActive(show);
            }
        }

        public void ShowSettings(bool show)
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(show);
                if (show) UpdateAudioButtonVisuals();
            }
        }

        public void OnQuitClicked()
        {
            Debug.Log("[UIManager] Quitting Game Expedition...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void UpdateHeaderMeta()
        {
            if (levelText != null && LevelManager.Instance != null)
            {
                var info = LevelManager.Instance.CurrentLevelInfo;
                string diffStr = LevelManager.Instance.CurrentDifficulty.ToString().ToUpper();
                levelText.text = info != null ? $"{diffStr} — {info.title.ToUpper()}" : "LEVEL 1";
            }
        }

        public void UpdateTimerDisplay(float secondsRemaining)
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(secondsRemaining / 60f);
            int seconds = Mathf.FloorToInt(secondsRemaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            if (secondsRemaining <= 30f)
            {
                timerText.color = new Color(1f, 0.25f, 0.25f, 1f);
            }
            else if (secondsRemaining <= 60f)
            {
                timerText.color = new Color(1f, 0.85f, 0.2f, 1f);
            }
            else
            {
                timerText.color = Color.white;
            }
        }

        public void UpdateScoreDisplay(int currentScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{currentScore}";
            }
        }

        public void UpdateHintDisplay(int hintsCount)
        {
            if (hudHintText != null)
            {
                if (hintsCount > 0)
                {
                    hudHintText.text = $"💡 [H] HINT ({hintsCount})";
                    hudHintText.color = new Color(1f, 0.92f, 0.25f);
                }
                else
                {
                    hudHintText.text = "💡 [H] 0 HINTS";
                    hudHintText.color = new Color(0.6f, 0.65f, 0.7f);
                }
            }
        }

        public void UpdateInventoryDisplay(HashSet<string> keys)
        {
            if (keysText != null)
            {
                if (keys == null || keys.Count == 0)
                {
                    string neededKey = GameManager.Instance != null && GameManager.Instance.ActiveLevelInfo != null && !string.IsNullOrEmpty(GameManager.Instance.ActiveLevelInfo.requiredKeyId)
                        ? GameManager.Instance.ActiveLevelInfo.requiredKeyId.Replace("Key", "").Trim().ToUpper()
                        : "";

                    if (!string.IsNullOrEmpty(neededKey))
                    {
                        keysText.text = $"<color=#94A3B8>🔑 NEED: {neededKey}</color>";
                    }
                    else
                    {
                        keysText.text = "<color=#94A3B8>🔑 NO KEY</color>";
                    }
                }
                else
                {
                    List<string> cleanKeys = new List<string>();
                    foreach (var k in keys)
                    {
                        cleanKeys.Add(k.Replace("Key", "").Trim().ToUpper());
                    }
                    keysText.text = $"<color=#FFD700><b>🔑 {string.Join(", ", cleanKeys)}</b></color>";
                }
            }
        }

        public void UpdateQuizProgressDisplay(int completed, int total)
        {
            if (quizProgressText != null)
            {
                if (total <= 0)
                {
                    if (completed > 0)
                    {
                        quizProgressText.text = $"<color=#2ECC71>🧩 SOLVED: {completed}</color>";
                    }
                    else
                    {
                        quizProgressText.text = "<color=#94A3B8>🧩 PUZZLES: OPTIONAL</color>";
                    }
                }
                else
                {
                    if (completed >= total)
                    {
                        quizProgressText.text = $"<color=#2ECC71><b>🧩 PUZZLES: {completed}/{total} ✓</b></color>";
                    }
                    else
                    {
                        quizProgressText.text = $"<color=#38BDF8>🧩 PUZZLES: {completed}/{total}</color>";
                    }
                }
            }
        }

        public void UpdateObjectiveDisplay(string objectiveText)
        {
            if (objectiveBannerText != null)
            {
                objectiveBannerText.text = objectiveText;
            }
        }

        public void ShowMessage(string message, float duration = 3f)
        {
            if (messageBannerText != null)
            {
                messageBannerText.text = message;
            }
            if (messageBannerPanel != null)
            {
                messageBannerPanel.SetActive(true);
            }

            if (messageCoroutine != null) StopCoroutine(messageCoroutine);
            messageCoroutine = StartCoroutine(HideMessageAfterDelay(duration));
        }

        private IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (messageBannerPanel != null)
            {
                messageBannerPanel.SetActive(false);
            }
        }

        public void ShowInteractionPrompt(string prompt)
        {
            if (interactionPromptText != null)
            {
                interactionPromptText.text = prompt;
            }
            if (interactionPromptPanel != null)
            {
                interactionPromptPanel.SetActive(true);
            }
        }

        public void HideInteractionPrompt()
        {
            if (interactionPromptPanel != null)
            {
                interactionPromptPanel.SetActive(false);
            }
        }

        public void ShowMainMenu()
        {
            HideAllModals();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (hudRoot != null) hudRoot.SetActive(false);

            UpdateAudioButtonVisuals();
            UpdateDifficultyButtonVisuals();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameState.MainMenu);
            }
        }

        public void OnPlayClicked()
        {
            HideAllModals();
            if (hudRoot != null) hudRoot.SetActive(true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
            else if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(1);
            }
        }

        public void ShowLevelSelect(DifficultyLevel? diffOverride = null)
        {
            HideAllModals();
            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(true);

                DifficultyLevel diff = diffOverride ?? (DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentDifficulty : DifficultyLevel.Medium);
                activeSelectTabDifficulty = diff;

                if (levelSelectTitleText != null)
                {
                    levelSelectTitleText.text = $"LEVEL SELECTION — {diff.ToString().ToUpper()} MODE";
                }

                // Update Tab Visuals
                SetDiffBtnVisual(levelSelectEasyTabButton, diff == DifficultyLevel.Easy, new Color(0.15f, 0.68f, 0.35f, 1f));
                SetDiffBtnVisual(levelSelectMediumTabButton, diff == DifficultyLevel.Medium, new Color(0.18f, 0.55f, 0.85f, 1f));
                SetDiffBtnVisual(levelSelectHardTabButton, diff == DifficultyLevel.Hard, new Color(0.85f, 0.25f, 0.25f, 1f));

                if (levelButtons != null && LevelManager.Instance != null)
                {
                    for (int i = 0; i < levelButtons.Length && i < 10; i++)
                    {
                        int lvl = i + 1;
                        bool isUnlocked = LevelManager.Instance.IsLevelUnlocked(diff, lvl);
                        bool isCompleted = LevelManager.Instance.IsLevelCompleted(diff, lvl);
                        int bestScore = LevelManager.Instance.GetBestScore(diff, lvl);
                        float bestTime = LevelManager.Instance.GetBestTime(diff, lvl);

                        if (levelButtons[i] != null)
                        {
                            levelButtons[i].interactable = isUnlocked;

                            var img = levelButtons[i].GetComponent<Image>();
                            if (img != null)
                            {
                                if (isCompleted) img.color = new Color(0.10f, 0.35f, 0.20f, 1f); // Green tint
                                else if (isUnlocked) img.color = new Color(0.15f, 0.25f, 0.40f, 1f); // Blue tint
                                else img.color = new Color(0.15f, 0.17f, 0.20f, 0.65f); // Gray locked
                            }

                            var txt = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                            if (txt != null)
                            {
                                var info = LevelManager.Instance.GetLevelInfo(diff, lvl);
                                int stars = LevelManager.Instance.GetLevelStars(diff, lvl);
                                string starString = stars switch
                                {
                                    3 => "<color=#FFD700>★★★</color>",
                                    2 => "<color=#FFD700>★★</color><color=#64748B>☆</color>",
                                    _ => "<color=#FFD700>★</color><color=#64748B>☆☆</color>"
                                };

                                if (isCompleted)
                                {
                                    string timeStr = bestTime > 0 ? $"{Mathf.FloorToInt(bestTime / 60):00}:{Mathf.FloorToInt(bestTime % 60):00}" : "--:--";
                                    txt.text = $"<b>LEVEL {lvl}</b>   {starString}\n<size=11><color=#2ECC71>COMPLETED</color></size>\n<size=11><color=#E2E8F0>{info.title}</color></size>\n<size=10><color=#F1C40F>Best: {bestScore} pts</color> | {timeStr}</size>";
                                }
                                else if (isUnlocked)
                                {
                                    txt.text = $"<b>LEVEL {lvl}</b>\n<size=11><color=#3498DB>▶ UNLOCKED</color></size>\n<size=11><color=#E2E8F0>{info.title}</color></size>\n<size=10><color=#94A3B8>Timer: {info.timeLimit:F0}s</color></size>";
                                }
                                else
                                {
                                    txt.text = $"<b>LEVEL {lvl}</b>\n<size=11><color=#E74C3C>🔒 LOCKED</color></size>\n<size=10><color=#64748B>Clear Level {lvl - 1} first</color></size>";
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SelectLevel(int level)
        {
            if (LevelManager.Instance != null && !LevelManager.Instance.IsLevelUnlocked(activeSelectTabDifficulty, level))
            {
                ShowMessage($"Level {level} is locked! Complete Level {level - 1} first.", 2.5f);
                return;
            }

            HideAllModals();
            if (hudRoot != null) hudRoot.SetActive(true);

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(activeSelectTabDifficulty, level);
            }
        }

        public void ShowPauseMenu(bool show)
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(show);
                if (show)
                {
                    UpdateAudioButtonVisuals();
                }
            }
        }

        public void ShowLevelCompleteScreen()
        {
            HideAllModals();
            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
            }

            if (GameManager.Instance != null)
            {
                if (levelCompleteTitleText != null && GameManager.Instance.ActiveLevelInfo != null)
                {
                    levelCompleteTitleText.text = $"{GameManager.Instance.ActiveLevelInfo.title.ToUpper()} CLEARED!";
                }

                int lvlScore = ScoreManager.Instance != null ? ScoreManager.Instance.LastLevelResultScore : GameManager.Instance.CurrentScore;
                int clearBonus = ScoreManager.Instance != null ? ScoreManager.Instance.LastClearBonus : 200;
                int timeBonus = ScoreManager.Instance != null ? ScoreManager.Instance.LastTimeBonus : 0;
                int baseScore = Mathf.Max(0, lvlScore - clearBonus - timeBonus);
                int campScore = ScoreManager.Instance != null ? ScoreManager.Instance.CampaignRunScore : lvlScore;
                int bestScore = ScoreManager.Instance != null ? ScoreManager.Instance.SavedBestScore : campScore;

                int earnedStars = GameManager.Instance != null ? GameManager.Instance.LastEarnedStars : 1;
                string starDisplay = earnedStars switch
                {
                    3 => "<color=#FFD700>★ ★ ★</color>",
                    2 => "<color=#FFD700>★ ★ </color><color=#475569>★</color>",
                    _ => "<color=#FFD700>★ </color><color=#475569>★ ★</color>"
                };
                string starDesc = earnedStars switch
                {
                    3 => "<color=#34D399>PERFECT EXPEDITION! (3 Stars)</color>",
                    2 => "<color=#60A5FA>GREAT NAVIGATOR! (2 Stars)</color>",
                    _ => "<color=#CBD5E1>LEVEL CONQUERED! (1 Star)</color>"
                };

                if (levelCompleteScoreText != null)
                {
                    levelCompleteScoreText.text = 
                        $"<size=36>{starDisplay}</size>\n" +
                        $"<size=15>{starDesc}</size>\n\n" +
                        $"SCORE EARNED THIS LEVEL\n" +
                        $"<size=46><color=#FFD700><b>+{lvlScore} PTS</b></color></size>\n" +
                        $"<size=14><color=#A7F3D0>Clear Bonus: +{clearBonus} PTS  •  Time Bonus: +{timeBonus} PTS" + 
                        (baseScore > 0 ? $"  •  Puzzles: +{baseScore} PTS" : "") + "</color></size>\n" +
                        $"<size=15><color=#BAE6FD>Total Campaign: <b>{campScore} PTS</b>   |   Record Best: <b>{bestScore} PTS</b></color></size>";
                }

                if (levelCompleteTimeText != null)
                {
                    int m = Mathf.FloorToInt(GameManager.Instance.ElapsedTime / 60f);
                    int s = Mathf.FloorToInt(GameManager.Instance.ElapsedTime % 60f);
                    levelCompleteTimeText.text = $"⏱ Clear Time: <b>{m:00}:{s:00}</b>";
                }

                if (levelCompleteStatsText != null)
                {
                    string keyName = GameManager.Instance.ActiveLevelInfo != null && !string.IsNullOrEmpty(GameManager.Instance.ActiveLevelInfo.requiredKeyId)
                        ? GameManager.Instance.ActiveLevelInfo.requiredKeyId.Replace("Key", " Key")
                        : "Level Key";
                    int bonusSolved = GameManager.Instance.CompletedQuizzesCount;
                    string bonusStr = bonusSolved > 0 ? $"{bonusSolved} Solved (+{bonusSolved * 100} pts)" : "Optional";
                    levelCompleteStatsText.text = $"🔑 Key Unlocked: <b>{keyName}</b>   |   🧩 Bonus Puzzles: <b>{bonusStr}</b>";
                }
            }
        }

        public void ShowGameOverScreen()
        {
            HideAllModals();
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (gameOverTitleText != null)
            {
                gameOverTitleText.text = "EXPEDITION FAILED — TIME EXPIRED";
            }

            if (GameManager.Instance != null)
            {
                int attemptScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentAttemptScore : GameManager.Instance.CurrentScore;
                int bestScore = ScoreManager.Instance != null ? ScoreManager.Instance.SavedBestScore : 0;

                if (gameOverScoreText != null)
                {
                    gameOverScoreText.text = $"Attempt Score: <b>{attemptScore} PTS</b>\nRecord Best: <b>{bestScore} PTS</b>";
                }
            }
        }

        public void ShowVictoryScreen()
        {
            HideAllModals();
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            DifficultyLevel diff = LevelManager.Instance != null ? LevelManager.Instance.CurrentDifficulty : DifficultyLevel.Medium;

            if (victoryTitleText != null)
            {
                victoryTitleText.text = $"DIFFICULTY CONQUERED! [{diff.ToString().ToUpper()} MODE]";
            }

            if (GameManager.Instance != null)
            {
                int finalCampScore = ScoreManager.Instance != null ? ScoreManager.Instance.CampaignRunScore : GameManager.Instance.CurrentScore;
                int bestScore = ScoreManager.Instance != null ? ScoreManager.Instance.SavedBestScore : finalCampScore;

                if (victoryScoreText != null)
                    victoryScoreText.text = $"TOTAL CAMPAIGN SCORE: {finalCampScore} PTS\n<size=18><color=#FFD700>RECORD BEST: {bestScore} PTS</color></size>";

                if (victoryTimeText != null)
                {
                    int m = Mathf.FloorToInt(GameManager.Instance.ElapsedTime / 60f);
                    int s = Mathf.FloorToInt(GameManager.Instance.ElapsedTime % 60f);
                    victoryTimeText.text = $"Total Campaign Time: {m:00}:{s:00}";
                }

                if (victoryStatsText != null)
                {
                    victoryStatsText.text = $"All 10 {diff} Labyrinths Conquered!\nTotal Campaign Score: {finalCampScore} PTS\nBonus Puzzles Solved: {GameManager.Instance.CompletedQuizzesCount}";
                }
            }
        }
    }
}
