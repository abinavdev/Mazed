using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mazed.Core;
using Mazed.Gameplay;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Mazed.Quiz
{
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        [Header("UI Panel Elements")]
        [SerializeField] private GameObject quizModalRoot;
        [SerializeField] private TextMeshProUGUI subjectBadgeText;
        [SerializeField] private TextMeshProUGUI questionBodyText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TextMeshProUGUI[] optionTexts;

        [Header("Feedback Panel")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackStatusText;
        [SerializeField] private TextMeshProUGUI feedbackExplanationText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionButtonText;

        [Header("Styling")]
        [SerializeField] private Color normalButtonColor = new Color(0.18f, 0.22f, 0.28f, 1f);
        [SerializeField] private Color correctButtonColor = new Color(0.15f, 0.68f, 0.38f, 1f);
        [SerializeField] private Color incorrectButtonColor = new Color(0.85f, 0.23f, 0.24f, 1f);

        private QuizCheckpoint currentCheckpoint;
        private QuizQuestion currentQuestion;
        private bool isAnsweringActive = false;
        private bool lastAnswerWasCorrect = false;
        private int currentQuestionRetries = 0;

        public bool IsQuizOpen => quizModalRoot != null && quizModalRoot.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            HookButtonListeners();

            if (quizModalRoot != null)
            {
                quizModalRoot.SetActive(false);
            }
        }

        public void RegisterButtons(Button[] buttons, Button actionBtn)
        {
            optionButtons = buttons;
            actionButton = actionBtn;
            HookButtonListeners();
        }

        public void HookButtonListeners()
        {
            if (optionButtons != null)
            {
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    int index = i;
                    if (optionButtons[i] != null)
                    {
                        optionButtons[i].onClick.RemoveAllListeners();
                        optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
                    }
                }
            }

            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(OnActionButtonClicked);
            }
        }

        private void Update()
        {
            if (!IsQuizOpen) return;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return;

            // Option selection (1-4 or A-D)
            if (isAnsweringActive && currentQuestion != null)
            {
                if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                {
                    OnOptionSelected(0);
                    return;
                }
                if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame || kb.bKey.wasPressedThisFrame)
                {
                    OnOptionSelected(1);
                    return;
                }
                if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame || kb.cKey.wasPressedThisFrame)
                {
                    OnOptionSelected(2);
                    return;
                }
                if (kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
                {
                    OnOptionSelected(3);
                    return;
                }
            }

            // Continue / Retry button advance (Space, Enter, E)
            if (!isAnsweringActive && feedbackPanel != null && feedbackPanel.activeSelf)
            {
                if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
                {
                    OnActionButtonClicked();
                    return;
                }
            }
#endif
        }

        public void OpenQuizForCheckpoint(QuizCheckpoint checkpoint)
        {
            if (checkpoint == null || checkpoint.IsCompleted) return;

            currentCheckpoint = checkpoint;
            currentQuestionRetries = 0;

            string difficultyTag = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetQuestionDifficultyTag() : "Medium";

            // Fetch question from bank
            if (QuestionBank.Instance != null)
            {
                currentQuestion = QuestionBank.Instance.GetNextQuestionForSubject(checkpoint.subject, difficultyTag);
            }

            if (currentQuestion == null)
            {
                Debug.LogError($"[QuizManager] Could not retrieve a question for subject '{checkpoint.subject}'!");
                return;
            }

            DisplayQuestion(currentQuestion);
        }

        private void DisplayQuestion(QuizQuestion q)
        {
            if (quizModalRoot == null) return;

            quizModalRoot.SetActive(true);
            isAnsweringActive = true;
            lastAnswerWasCorrect = false;

            // Ensure button listeners are properly active
            HookButtonListeners();

            // Pause player movement and free cursor
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPlayerInputActive(false);
                GameManager.Instance.SetCursorLocked(false);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(false);
            }

            if (subjectBadgeText != null)
            {
                string diff = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentDifficulty.ToString().ToUpper() : "NORMAL";
                subjectBadgeText.text = $"[ {q.subject.ToUpper()} | {diff} DIFFICULTY ]";
            }

            if (questionBodyText != null)
            {
                questionBodyText.text = q.question;
            }

            // Set options
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null)
                {
                    optionButtons[i].interactable = true;
                    var image = optionButtons[i].GetComponent<Image>();
                    if (image != null) image.color = normalButtonColor;

                    if (i < q.options.Length && optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
                    {
                        char prefix = (char)('A' + i);
                        optionTexts[i].text = $"{prefix}.  {q.options[i]}";
                        optionButtons[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        optionButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public void OnOptionSelected(int chosenIndex)
        {
            if (!isAnsweringActive || currentQuestion == null) return;
            isAnsweringActive = false;

            // Disable all option buttons
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null) optionButtons[i].interactable = false;
            }

            bool isCorrect = currentQuestion.IsCorrect(chosenIndex);
            lastAnswerWasCorrect = isCorrect;

            // Highlight buttons
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == null) continue;
                var image = optionButtons[i].GetComponent<Image>();
                if (image == null) continue;

                if (i == currentQuestion.correctAnswerIndex)
                {
                    image.color = correctButtonColor; // Always highlight correct answer in green
                }
                else if (i == chosenIndex && !isCorrect)
                {
                    image.color = incorrectButtonColor; // Highlight chosen wrong answer in red
                }
            }

            // Record in GameManager
            if (GameManager.Instance != null)
            {
                string cpId = currentCheckpoint != null ? currentCheckpoint.checkpointId : "";
                GameManager.Instance.RecordQuizAnswer(cpId, isCorrect);
            }

            // Award Hint Token on correct answer
            if (isCorrect && HintManager.Instance != null)
            {
                HintManager.Instance.AddHint(1);
            }

            // Mark question in QuestionBank
            if (QuestionBank.Instance != null)
            {
                QuestionBank.Instance.MarkQuestionAnswered(currentQuestion.id);
            }

            // Play audio
            if (AudioManager.Instance != null)
            {
                if (isCorrect) AudioManager.Instance.PlayCorrectQuizSFX();
                else AudioManager.Instance.PlayIncorrectQuizSFX();
            }

            // Show feedback panel
            ShowFeedback(isCorrect);
        }

        private void ShowFeedback(bool isCorrect)
        {
            if (feedbackPanel == null) return;

            feedbackPanel.SetActive(true);

            int correctPts = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetCorrectPoints() : 100;
            int penalty = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetIncorrectPenalty() : 25;

            if (feedbackStatusText != null)
            {
                feedbackStatusText.text = isCorrect 
                    ? $"<color=#2ECC71><b>CORRECT ANSWER! +{correctPts} PTS & +1 HINT TOKEN!</b></color>" 
                    : $"<color=#E74C3C><b>INCORRECT ANSWER (-{penalty} PTS)</b></color>";
            }

            if (feedbackExplanationText != null)
            {
                if (isCorrect)
                {
                    feedbackExplanationText.text = $"<b>Well done!</b>\n{currentQuestion.explanation}";
                }
                else
                {
                    char optLetter = (char)('A' + currentQuestion.correctAnswerIndex);
                    string optText = currentQuestion.GetCorrectAnswerText();
                    feedbackExplanationText.text = $"<b>Correct Answer:</b> Option {optLetter} — {optText}\n\n<b>Explanation:</b> {currentQuestion.explanation}";
                }
            }

            if (actionButtonText != null)
            {
                if (isCorrect)
                {
                    actionButtonText.text = "CONTINUE EXPEDITION";
                }
                else
                {
                    int maxRetries = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetMaxRetries() : 2;
                    if (maxRetries >= 0 && currentQuestionRetries >= maxRetries)
                    {
                        actionButtonText.text = "RETRY LIMIT REACHED - CLOSE";
                    }
                    else
                    {
                        actionButtonText.text = "RETRY QUESTION";
                    }
                }
            }
        }

        public void OnActionButtonClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSFX();

            if (lastAnswerWasCorrect)
            {
                // Mark checkpoint complete and unlock door
                if (currentCheckpoint != null)
                {
                    currentCheckpoint.MarkCompleted();
                }

                CloseQuiz();
            }
            else
            {
                int maxRetries = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetMaxRetries() : 2;
                currentQuestionRetries++;

                if (maxRetries >= 0 && currentQuestionRetries > maxRetries)
                {
                    // No more retries allowed
                    CloseQuiz();
                }
                else
                {
                    // Retry with new question for this checkpoint
                    if (currentCheckpoint != null)
                    {
                        string diffTag = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetQuestionDifficultyTag() : "Medium";
                        currentQuestion = QuestionBank.Instance.GetNextQuestionForSubject(currentCheckpoint.subject, diffTag);
                        if (currentQuestion != null)
                        {
                            DisplayQuestion(currentQuestion);
                        }
                        else
                        {
                            CloseQuiz();
                        }
                    }
                    else
                    {
                        CloseQuiz();
                    }
                }
            }
        }

        public void CloseQuiz()
        {
            if (quizModalRoot != null)
            {
                quizModalRoot.SetActive(false);
            }

            isAnsweringActive = false;

            // Restore player controls and cursor
            if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                GameManager.Instance.SetPlayerInputActive(true);
                GameManager.Instance.SetCursorLocked(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
