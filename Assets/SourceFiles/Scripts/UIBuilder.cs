using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using Mazed.Core;
using Mazed.Quiz;

namespace Mazed.UI
{
    public static class UIBuilder
    {
        public static Canvas BuildCanvas(UIManager uiManager, QuizManager quizManager)
        {
            // Destroy existing canvas if re-initializing
            Canvas existingCanvas = Object.FindAnyObjectByType<Canvas>();
            if (existingCanvas != null && existingCanvas.name == "MazedGameCanvas")
            {
                Object.DestroyImmediate(existingCanvas.gameObject);
            }

            EnsureEventSystem();

            GameObject canvasGO = new GameObject("MazedGameCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            // Build HUD
            BuildHUD(canvasGO, uiManager, defaultFont);

            // Build Quiz Modal
            BuildQuizModal(canvasGO, quizManager, defaultFont);

            // Build Main Menu
            BuildMainMenu(canvasGO, uiManager, defaultFont);

            // Build Difficulty Panel
            BuildDifficultyPanel(canvasGO, uiManager, defaultFont);

            // Build Level Select Panel
            BuildLevelSelectPanel(canvasGO, uiManager, defaultFont);

            // Build How To Play Modal
            BuildHowToPlayModal(canvasGO, uiManager, defaultFont);

            // Build Settings Modal
            BuildSettingsModal(canvasGO, uiManager, defaultFont);

            // Build Pause Menu
            BuildPauseMenu(canvasGO, uiManager, defaultFont);

            // Build Level Complete Screen
            BuildLevelCompleteScreen(canvasGO, uiManager, defaultFont);

            // Build Game Over Screen
            BuildGameOverScreen(canvasGO, uiManager, defaultFont);

            // Build Victory Screen
            BuildVictoryScreen(canvasGO, uiManager, defaultFont);

            uiManager.HookButtons();
            uiManager.HideAllModals();
            uiManager.ShowMainMenu();

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            EventSystem es = Object.FindAnyObjectByType<EventSystem>();
            InputSystemUIInputModule uiMod = null;
            if (es == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                es = esGO.AddComponent<EventSystem>();
                uiMod = esGO.AddComponent<InputSystemUIInputModule>();
            }
            else
            {
                var legacyModule = es.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                {
                    Object.DestroyImmediate(legacyModule);
                }
                uiMod = es.GetComponent<InputSystemUIInputModule>();
                if (uiMod == null)
                {
                    uiMod = es.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }

            if (es != null)
            {
                es.enabled = true;
            }

            if (uiMod != null)
            {
                uiMod.enabled = true;
                if (uiMod.actionsAsset == null)
                {
                    uiMod.AssignDefaultActions();
                }
            }
        }

        private static void BuildHUD(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject hudRoot = CreateUIObject("HUD_Root", parent.transform);
            SetStretch(hudRoot.GetComponent<RectTransform>());

            // Top Bar
            GameObject topBar = CreateUIObject("TopBar", hudRoot.transform);
            RectTransform topBarRT = topBar.GetComponent<RectTransform>();
            topBarRT.anchorMin = new Vector2(0, 1);
            topBarRT.anchorMax = new Vector2(1, 1);
            topBarRT.pivot = new Vector2(0.5f, 1);
            topBarRT.sizeDelta = new Vector2(0, 85);
            topBarRT.anchoredPosition = Vector2.zero;

            Image topBarBg = topBar.AddComponent<Image>();
            topBarBg.color = new Color(0.04f, 0.07f, 0.12f, 0.90f);

            // Level Title (Top Left)
            TextMeshProUGUI levelText = CreateText("LevelText", topBar.transform, "LEVEL 1 — BASIC MAZE", font, 19, TextAlignmentOptions.Left);
            RectTransform lvlRT = levelText.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(0, 0.5f);
            lvlRT.anchorMax = new Vector2(0, 0.5f);
            lvlRT.pivot = new Vector2(0, 0.5f);
            lvlRT.anchoredPosition = new Vector2(25, 13);
            lvlRT.sizeDelta = new Vector2(500, 32);
            levelText.color = new Color(0.3f, 0.85f, 1f);
            levelText.textWrappingMode = TextWrappingModes.NoWrap;
            levelText.overflowMode = TextOverflowModes.Ellipsis;

            // Difficulty Tag (Below Level Title)
            TextMeshProUGUI diffText = CreateText("DifficultyText", topBar.transform, "[MEDIUM DIFFICULTY • LEVEL 1/10]", font, 14, TextAlignmentOptions.Left);
            RectTransform dtRT = diffText.GetComponent<RectTransform>();
            dtRT.anchorMin = new Vector2(0, 0.5f);
            dtRT.anchorMax = new Vector2(0, 0.5f);
            dtRT.pivot = new Vector2(0, 0.5f);
            dtRT.anchoredPosition = new Vector2(25, -15);
            dtRT.sizeDelta = new Vector2(350, 24);
            diffText.color = new Color(1f, 0.8f, 0.2f);
            diffText.textWrappingMode = TextWrappingModes.NoWrap;

            // Timer (Center Left)
            TextMeshProUGUI timer = CreateText("TimerText", topBar.transform, "TIME: 08:00", font, 26, TextAlignmentOptions.Center);
            RectTransform timerRT = timer.GetComponent<RectTransform>();
            timerRT.anchorMin = new Vector2(0.38f, 0.5f);
            timerRT.anchorMax = new Vector2(0.38f, 0.5f);
            timerRT.pivot = new Vector2(0.5f, 0.5f);
            timerRT.anchoredPosition = Vector2.zero;
            timerRT.sizeDelta = new Vector2(200, 45);

            // Score (Center Right)
            TextMeshProUGUI score = CreateText("ScoreText", topBar.transform, "SCORE: 0", font, 26, TextAlignmentOptions.Center);
            RectTransform scoreRT = score.GetComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.52f, 0.5f);
            scoreRT.anchorMax = new Vector2(0.52f, 0.5f);
            scoreRT.pivot = new Vector2(0.5f, 0.5f);
            scoreRT.anchoredPosition = Vector2.zero;
            scoreRT.sizeDelta = new Vector2(200, 45);
            score.color = new Color(1f, 0.85f, 0.2f);

            // Quizzes Counter (Right Cluster: Inner)
            TextMeshProUGUI quizzes = CreateText("QuizProgressText", topBar.transform, "🧩 PUZZLES: OPTIONAL", font, 20, TextAlignmentOptions.Right);
            RectTransform qzRT = quizzes.GetComponent<RectTransform>();
            qzRT.anchorMin = new Vector2(1, 0.5f);
            qzRT.anchorMax = new Vector2(1, 0.5f);
            qzRT.pivot = new Vector2(1, 0.5f);
            qzRT.anchoredPosition = new Vector2(-425, 0);
            qzRT.sizeDelta = new Vector2(220, 45);
            quizzes.color = new Color(0.35f, 0.9f, 0.6f);

            // Keys Indicator (Right Cluster: Middle)
            TextMeshProUGUI keys = CreateText("KeysText", topBar.transform, "🔑 NEED: BRONZE", font, 20, TextAlignmentOptions.Right);
            RectTransform keysRT = keys.GetComponent<RectTransform>();
            keysRT.anchorMin = new Vector2(1, 0.5f);
            keysRT.anchorMax = new Vector2(1, 0.5f);
            keysRT.pivot = new Vector2(1, 0.5f);
            keysRT.anchoredPosition = new Vector2(-205, 0);
            keysRT.sizeDelta = new Vector2(210, 45);
            keys.color = new Color(0.4f, 0.9f, 1f);

            // Hint Token HUD Button (Right Cluster: Far Right)
            Button hudHintBtn = CreateButton("HudHintButton", topBar.transform, "💡 [H] HINT (1)", font, Vector2.zero, new Vector2(175, 42), new Color(0.10f, 0.28f, 0.48f, 1f));
            RectTransform hbRT = hudHintBtn.GetComponent<RectTransform>();
            hbRT.anchorMin = new Vector2(1, 0.5f);
            hbRT.anchorMax = new Vector2(1, 0.5f);
            hbRT.pivot = new Vector2(1, 0.5f);
            hbRT.anchoredPosition = new Vector2(-20, 0);

            TextMeshProUGUI hudHintText = hudHintBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (hudHintText != null)
            {
                hudHintText.color = new Color(1f, 0.92f, 0.25f);
                hudHintText.fontSize = 17;
            }

            // Objective Ribbon (Center below bar)
            GameObject objRibbon = CreateUIObject("ObjectiveRibbon", hudRoot.transform);
            RectTransform objRT = objRibbon.GetComponent<RectTransform>();
            objRT.anchorMin = new Vector2(0.5f, 1);
            objRT.anchorMax = new Vector2(0.5f, 1);
            objRT.pivot = new Vector2(0.5f, 1);
            objRT.sizeDelta = new Vector2(850, 42);
            objRT.anchoredPosition = new Vector2(0, -95);
            Image objBg = objRibbon.AddComponent<Image>();
            objBg.color = new Color(0.08f, 0.14f, 0.22f, 0.90f);

            TextMeshProUGUI objectiveText = CreateText("ObjectiveText", objRibbon.transform, "OBJECTIVE: Locate the Bronze Key in the maze.", font, 18, TextAlignmentOptions.Center);
            SetStretch(objectiveText.GetComponent<RectTransform>());
            objectiveText.color = new Color(1f, 0.95f, 0.6f);

            // Notification Banner
            GameObject bannerGO = CreateUIObject("MessageBanner", hudRoot.transform);
            RectTransform bannerRT = bannerGO.GetComponent<RectTransform>();
            bannerRT.anchorMin = new Vector2(0.5f, 1);
            bannerRT.anchorMax = new Vector2(0.5f, 1);
            bannerRT.pivot = new Vector2(0.5f, 1);
            bannerRT.sizeDelta = new Vector2(650, 50);
            bannerRT.anchoredPosition = new Vector2(0, -145);
            Image bannerBg = bannerGO.AddComponent<Image>();
            bannerBg.color = new Color(0.12f, 0.18f, 0.28f, 0.95f);

            TextMeshProUGUI bannerText = CreateText("BannerText", bannerGO.transform, "Notification", font, 20, TextAlignmentOptions.Center);
            SetStretch(bannerText.GetComponent<RectTransform>());
            bannerText.color = Color.white;

            // Interaction Prompt
            GameObject promptGO = CreateUIObject("InteractionPrompt", hudRoot.transform);
            RectTransform promptRT = promptGO.GetComponent<RectTransform>();
            promptRT.anchorMin = new Vector2(0.5f, 0.22f);
            promptRT.anchorMax = new Vector2(0.5f, 0.22f);
            promptRT.pivot = new Vector2(0.5f, 0.5f);
            promptRT.sizeDelta = new Vector2(650, 48);
            promptRT.anchoredPosition = Vector2.zero;
            Image promptBg = promptGO.AddComponent<Image>();
            promptBg.color = new Color(0.07f, 0.11f, 0.17f, 0.92f);

            TextMeshProUGUI promptText = CreateText("PromptText", promptGO.transform, "Press [E] to start Quiz Terminal", font, 19, TextAlignmentOptions.Center);
            SetStretch(promptText.GetComponent<RectTransform>());
            promptText.color = new Color(1f, 0.9f, 0.3f);

            // Controls overlay
            GameObject controlsGO = CreateUIObject("ControlsHint", hudRoot.transform);
            RectTransform controlsRT = controlsGO.GetComponent<RectTransform>();
            controlsRT.anchorMin = new Vector2(0, 0);
            controlsRT.anchorMax = new Vector2(0, 0);
            controlsRT.pivot = new Vector2(0, 0);
            controlsRT.sizeDelta = new Vector2(600, 35);
            controlsRT.anchoredPosition = new Vector2(25, 15);
            TextMeshProUGUI controlsText = CreateText("ControlsText", controlsGO.transform, "WASD / Arrows: Move | Space: Jump | E: Interact | Esc: Pause", font, 15, TextAlignmentOptions.Left);
            SetStretch(controlsText.GetComponent<RectTransform>());
            controlsText.color = new Color(0.8f, 0.85f, 0.9f, 0.7f);

            // Reflect into UIManager
            SetPrivateField(uiManager, "hudRoot", hudRoot);
            SetPrivateField(uiManager, "levelText", levelText);
            SetPrivateField(uiManager, "difficultyText", diffText);
            SetPrivateField(uiManager, "timerText", timer);
            SetPrivateField(uiManager, "scoreText", score);
            SetPrivateField(uiManager, "keysText", keys);
            SetPrivateField(uiManager, "quizProgressText", quizzes);
            SetPrivateField(uiManager, "hudHintButton", hudHintBtn);
            SetPrivateField(uiManager, "hudHintText", hudHintText);
            SetPrivateField(uiManager, "objectiveBannerText", objectiveText);
            SetPrivateField(uiManager, "messageBannerText", bannerText);
            SetPrivateField(uiManager, "messageBannerPanel", bannerGO);
            SetPrivateField(uiManager, "interactionPromptText", promptText);
            SetPrivateField(uiManager, "interactionPromptPanel", promptGO);

            bannerGO.SetActive(false);
            promptGO.SetActive(false);
        }

        private static void BuildQuizModal(GameObject parent, QuizManager quizManager, TMP_FontAsset font)
        {
            GameObject modalRoot = CreateUIObject("QuizModalRoot", parent.transform);
            SetStretch(modalRoot.GetComponent<RectTransform>());
            Image modalOverlay = modalRoot.AddComponent<Image>();
            modalOverlay.color = new Color(0f, 0f, 0f, 0.80f);

            // Dialog Card
            GameObject dialogCard = CreateUIObject("DialogCard", modalRoot.transform);
            RectTransform cardRT = dialogCard.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(980, 640);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = dialogCard.AddComponent<Image>();
            cardBg.color = new Color(0.08f, 0.12f, 0.17f, 0.98f);

            // Subject badge
            TextMeshProUGUI subjectBadge = CreateText("SubjectBadge", dialogCard.transform, "[ BONUS PUZZLE TERMINAL ]", font, 20, TextAlignmentOptions.Center);
            RectTransform badgeRT = subjectBadge.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 1f);
            badgeRT.anchorMax = new Vector2(0.5f, 1f);
            badgeRT.pivot = new Vector2(0.5f, 1f);
            badgeRT.sizeDelta = new Vector2(850, 40);
            badgeRT.anchoredPosition = new Vector2(0, -22);
            subjectBadge.color = new Color(0.25f, 0.85f, 1f);

            // Question body
            TextMeshProUGUI questionBody = CreateText("QuestionBody", dialogCard.transform, "Question text will appear here...", font, 23, TextAlignmentOptions.Center);
            RectTransform qRT = questionBody.GetComponent<RectTransform>();
            qRT.anchorMin = new Vector2(0.5f, 1f);
            qRT.anchorMax = new Vector2(0.5f, 1f);
            qRT.pivot = new Vector2(0.5f, 1f);
            qRT.sizeDelta = new Vector2(880, 115);
            qRT.anchoredPosition = new Vector2(0, -68);
            questionBody.color = Color.white;

            // 4 Option Buttons
            Button[] optionBtns = new Button[4];
            TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4];
            float buttonStartY = -205f;
            float buttonSpacing = 68f;

            for (int i = 0; i < 4; i++)
            {
                GameObject btnGO = CreateUIObject($"OptionButton_{i}", dialogCard.transform);
                RectTransform btnRT = btnGO.GetComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(0.5f, 1f);
                btnRT.anchorMax = new Vector2(0.5f, 1f);
                btnRT.pivot = new Vector2(0.5f, 1f);
                btnRT.sizeDelta = new Vector2(880, 56);
                btnRT.anchoredPosition = new Vector2(0, buttonStartY - (i * buttonSpacing));

                Image btnImg = btnGO.AddComponent<Image>();
                btnImg.color = new Color(0.16f, 0.21f, 0.28f, 1f);
                btnImg.raycastTarget = true;

                Button btn = btnGO.AddComponent<Button>();
                btn.targetGraphic = btnImg;
                btn.interactable = true;

                ColorBlock cb = btn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(0.35f, 0.75f, 1f, 1f);
                cb.pressedColor = new Color(0.2f, 0.5f, 0.8f, 1f);
                cb.selectedColor = new Color(0.35f, 0.75f, 1f, 1f);
                cb.colorMultiplier = 1f;
                btn.colors = cb;

                optionBtns[i] = btn;

                char prefix = (char)('A' + i);
                TextMeshProUGUI btnText = CreateText("Text", btnGO.transform, $"{prefix}. Option text", font, 20, TextAlignmentOptions.Left);
                RectTransform textRT = btnText.GetComponent<RectTransform>();
                SetStretch(textRT);
                textRT.offsetMin = new Vector2(25, 0);
                textRT.offsetMax = new Vector2(-25, 0);
                btnText.color = Color.white;
                optionTexts[i] = btnText;
            }

            // Feedback Panel
            GameObject feedbackGO = CreateUIObject("FeedbackPanel", dialogCard.transform);
            RectTransform fbRT = feedbackGO.GetComponent<RectTransform>();
            fbRT.anchorMin = new Vector2(0.5f, 0);
            fbRT.anchorMax = new Vector2(0.5f, 0);
            fbRT.pivot = new Vector2(0.5f, 0);
            fbRT.sizeDelta = new Vector2(920, 155);
            fbRT.anchoredPosition = new Vector2(0, 18);
            Image fbBg = feedbackGO.AddComponent<Image>();
            fbBg.color = new Color(0.05f, 0.08f, 0.12f, 0.96f);

            TextMeshProUGUI fbStatus = CreateText("StatusText", feedbackGO.transform, "CORRECT!", font, 22, TextAlignmentOptions.Center);
            RectTransform fbsRT = fbStatus.GetComponent<RectTransform>();
            fbsRT.anchorMin = new Vector2(0.5f, 1f);
            fbsRT.anchorMax = new Vector2(0.5f, 1f);
            fbsRT.pivot = new Vector2(0.5f, 1f);
            fbsRT.sizeDelta = new Vector2(880, 35);
            fbsRT.anchoredPosition = new Vector2(0, -10);

            TextMeshProUGUI fbExpl = CreateText("ExplanationText", feedbackGO.transform, "Explanation goes here", font, 17, TextAlignmentOptions.Center);
            RectTransform fbeRT = fbExpl.GetComponent<RectTransform>();
            fbeRT.anchorMin = new Vector2(0.5f, 1f);
            fbeRT.anchorMax = new Vector2(0.5f, 1f);
            fbeRT.pivot = new Vector2(0.5f, 1f);
            fbeRT.sizeDelta = new Vector2(880, 55);
            fbeRT.anchoredPosition = new Vector2(0, -45);
            fbExpl.color = new Color(0.85f, 0.9f, 0.95f);

            Button actionBtn = CreateButton("ActionButton", feedbackGO.transform, "CONTINUE", font, new Vector2(0, 12), new Vector2(320, 42), new Color(0.15f, 0.65f, 0.35f));
            RectTransform abRT = actionBtn.GetComponent<RectTransform>();
            abRT.anchorMin = new Vector2(0.5f, 0);
            abRT.anchorMax = new Vector2(0.5f, 0);
            abRT.pivot = new Vector2(0.5f, 0);
            TextMeshProUGUI actionText = actionBtn.GetComponentInChildren<TextMeshProUGUI>();

            SetPrivateField(quizManager, "quizModalRoot", modalRoot);
            SetPrivateField(quizManager, "subjectBadgeText", subjectBadge);
            SetPrivateField(quizManager, "questionBodyText", questionBody);
            SetPrivateField(quizManager, "optionButtons", optionBtns);
            SetPrivateField(quizManager, "optionTexts", optionTexts);
            SetPrivateField(quizManager, "feedbackPanel", feedbackGO);
            SetPrivateField(quizManager, "feedbackStatusText", fbStatus);
            SetPrivateField(quizManager, "feedbackExplanationText", fbExpl);
            SetPrivateField(quizManager, "actionButton", actionBtn);
            SetPrivateField(quizManager, "actionButtonText", actionText);

            quizManager.RegisterButtons(optionBtns, actionBtn);
            feedbackGO.SetActive(false);
            modalRoot.SetActive(false);
        }

        private static void BuildMainMenu(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject menuRoot = CreateUIObject("MainMenuPanel", parent.transform);
            SetStretch(menuRoot.GetComponent<RectTransform>());
            Image overlay = menuRoot.AddComponent<Image>();
            overlay.color = new Color(0.03f, 0.05f, 0.09f, 0.94f);

            // Sleek Modern Landing Card
            GameObject card = CreateUIObject("MenuCard", menuRoot.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(980, 720);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.05f, 0.08f, 0.14f, 0.98f);

            // Top Cyan Accent Glow Bar
            GameObject glowBar = CreateUIObject("TopGlowBar", card.transform);
            RectTransform gbRT = glowBar.GetComponent<RectTransform>();
            gbRT.anchorMin = new Vector2(0.5f, 1f);
            gbRT.anchorMax = new Vector2(0.5f, 1f);
            gbRT.pivot = new Vector2(0.5f, 1f);
            gbRT.sizeDelta = new Vector2(980, 4);
            gbRT.anchoredPosition = Vector2.zero;
            Image gbImg = glowBar.AddComponent<Image>();
            gbImg.color = new Color(0f, 0.9f, 1f, 0.9f);

            // Category Badge
            TextMeshProUGUI badge = CreateText("CategoryBadge", card.transform, "✦  3D LABYRINTH ESCAPE ADVENTURE  ✦", font, 13, TextAlignmentOptions.Center);
            RectTransform bRT = badge.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0.5f, 1f);
            bRT.anchorMax = new Vector2(0.5f, 1f);
            bRT.pivot = new Vector2(0.5f, 1f);
            bRT.sizeDelta = new Vector2(900, 22);
            bRT.anchoredPosition = new Vector2(0, -22);
            badge.color = new Color(0f, 1f, 0.65f, 0.95f);

            // Main Game Title
            TextMeshProUGUI title = CreateText("GameTitle", card.transform, "<b><color=#00FFA3>M A Z E D</color></b>", font, 52, TextAlignmentOptions.Center);
            RectTransform tRT = title.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1f);
            tRT.anchorMax = new Vector2(0.5f, 1f);
            tRT.pivot = new Vector2(0.5f, 1f);
            tRT.sizeDelta = new Vector2(900, 56);
            tRT.anchoredPosition = new Vector2(0, -48);
            title.color = Color.white;

            // Subtitle Tagline
            TextMeshProUGUI sub = CreateText("Subtitle", card.transform, "Explore 3D Labyrinths  •  Acquire Keys  •  Unlock Exit Doors  •  Beat The Clock", font, 15, TextAlignmentOptions.Center);
            RectTransform sRT = sub.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.5f, 1f);
            sRT.anchorMax = new Vector2(0.5f, 1f);
            sRT.pivot = new Vector2(0.5f, 1f);
            sRT.sizeDelta = new Vector2(900, 26);
            sRT.anchoredPosition = new Vector2(0, -104);
            sub.color = new Color(0.65f, 0.85f, 0.95f);

            // Active Difficulty & Status Ribbon
            GameObject pillGO = CreateUIObject("StatusPill", card.transform);
            RectTransform pillRT = pillGO.GetComponent<RectTransform>();
            pillRT.anchorMin = new Vector2(0.5f, 1f);
            pillRT.anchorMax = new Vector2(0.5f, 1f);
            pillRT.pivot = new Vector2(0.5f, 1f);
            pillRT.sizeDelta = new Vector2(820, 36);
            pillRT.anchoredPosition = new Vector2(0, -140);
            Image pillBg = pillGO.AddComponent<Image>();
            pillBg.color = new Color(0.06f, 0.12f, 0.18f, 0.95f);

            TextMeshProUGUI diffLabel = CreateText("CurrentDifficultyLabel", pillGO.transform, "ACTIVE DIFFICULTY: <color=#00FFA3><b>MEDIUM (10 LEVELS)</b></color>  •  10 Unique 3D Labyrinths", font, 14, TextAlignmentOptions.Center);
            SetStretch(diffLabel.GetComponent<RectTransform>());
            diffLabel.color = new Color(1f, 0.88f, 0.35f);

            // 1. Prominent Hero "PLAY GAME" Button
            Button playBtn = CreateButton("PlayButton", card.transform, "<b>▶   PLAY GAME</b>\n<size=12><color=#D1FAE5>Enter the 3D labyrinth & escape</color></size>", font, new Vector2(0, -196), new Vector2(720, 62), new Color(0.04f, 0.65f, 0.40f, 1f));
            TextMeshProUGUI playTxt = playBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (playTxt != null) playTxt.fontSize = 20;

            // 2x2 Feature Navigation Grid
            // Row 1: Difficulty & Level Select
            Button diffBtn = CreateButton("SelectDifficultyButton", card.transform, "<b>⚡  DIFFICULTY MODE</b>\n<size=11><color=#BAE6FD>Easy • Medium • Hard</color></size>", font, new Vector2(-188, -282), new Vector2(344, 56), new Color(0.12f, 0.38f, 0.68f, 1f));
            Button levelBtn = CreateButton("LevelSelectButton", card.transform, "<b>🗺️  LEVEL SELECTOR</b>\n<size=11><color=#DDD6FE>10 Mazes per Difficulty</color></size>", font, new Vector2(188, -282), new Vector2(344, 56), new Color(0.24f, 0.30f, 0.65f, 1f));

            // Row 2: How To Play & Settings
            Button htpBtn = CreateButton("HowToPlayButton", card.transform, "<b>📖  HOW TO PLAY</b>\n<size=11><color=#A5F3FC>Controls & Objectives</color></size>", font, new Vector2(-188, -352), new Vector2(344, 56), new Color(0.10f, 0.44f, 0.52f, 1f));
            Button settingsBtn = CreateButton("SettingsButton", card.transform, "<b>🔊  AUDIO & SETTINGS</b>\n<size=11><color=#CBD5E1>Soundtrack & Preferences</color></size>", font, new Vector2(188, -352), new Vector2(344, 56), new Color(0.20f, 0.28f, 0.38f, 1f));

            // Bottom Quick Action Bar: Music Pill, SFX Pill, and Quit
            Button musicBtn = CreateButton("LandingMusicBtn", card.transform, "MUSIC: ON", font, new Vector2(-240, -432), new Vector2(220, 42), new Color(0.14f, 0.50f, 0.35f, 1f));
            TextMeshProUGUI musicTxt = musicBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (musicTxt != null) musicTxt.fontSize = 15;

            Button sfxBtn = CreateButton("LandingSfxBtn", card.transform, "SFX: ON", font, new Vector2(0, -432), new Vector2(220, 42), new Color(0.16f, 0.42f, 0.65f, 1f));
            TextMeshProUGUI sfxTxt = sfxBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (sfxTxt != null) sfxTxt.fontSize = 15;

            Button quitBtn = CreateButton("QuitButton", card.transform, "✕  QUIT GAME", font, new Vector2(240, -432), new Vector2(220, 42), new Color(0.72f, 0.20f, 0.22f, 1f));
            TextMeshProUGUI quitTxt = quitBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (quitTxt != null) quitTxt.fontSize = 15;

            // Footer Quick Reference Hints
            TextMeshProUGUI hints = CreateText("Instructions", card.transform, "<b>Controls:</b> WASD to Move  |  Mouse Orbit Camera  |  Space to Jump  |  [E] Bonus Puzzle  |  [Esc] Pause", font, 13, TextAlignmentOptions.Center);
            RectTransform hRT = hints.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0.5f, 0f);
            hRT.anchorMax = new Vector2(0.5f, 0f);
            hRT.pivot = new Vector2(0.5f, 0f);
            hRT.sizeDelta = new Vector2(920, 30);
            hRT.anchoredPosition = new Vector2(0, 16);
            hints.color = new Color(0.55f, 0.65f, 0.78f);

            // Inject references to UIManager
            SetPrivateField(uiManager, "mainMenuPanel", menuRoot);
            SetPrivateField(uiManager, "playButton", playBtn);
            SetPrivateField(uiManager, "selectDifficultyButton", diffBtn);
            SetPrivateField(uiManager, "selectLevelButton", levelBtn);
            SetPrivateField(uiManager, "settingsButton", settingsBtn);
            SetPrivateField(uiManager, "quitButton", quitBtn);
            SetPrivateField(uiManager, "howToPlayButton", htpBtn);
            SetPrivateField(uiManager, "landingMusicButton", musicBtn);
            SetPrivateField(uiManager, "landingMusicText", musicTxt);
            SetPrivateField(uiManager, "landingSfxButton", sfxBtn);
            SetPrivateField(uiManager, "landingSfxText", sfxTxt);
            SetPrivateField(uiManager, "currentDifficultyLabel", diffLabel);
        }

        private static void BuildDifficultyPanel(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject diffRoot = CreateUIObject("DifficultyPanel", parent.transform);
            SetStretch(diffRoot.GetComponent<RectTransform>());
            Image overlay = diffRoot.AddComponent<Image>();
            overlay.color = new Color(0.04f, 0.07f, 0.12f, 0.96f);

            GameObject card = CreateUIObject("DiffCard", diffRoot.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(940, 700);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.06f, 0.09f, 0.15f, 0.98f);

            TextMeshProUGUI title = CreateText("Title", card.transform, "SELECT GAME DIFFICULTY", font, 36, TextAlignmentOptions.Center);
            RectTransform tRT = title.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1);
            tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.sizeDelta = new Vector2(850, 48);
            tRT.anchoredPosition = new Vector2(0, -25);
            title.color = new Color(0.3f, 0.85f, 1f);

            TextMeshProUGUI sub = CreateText("Sub", card.transform, "Each difficulty contains 10 distinct, completable 3D mazes with tailored timers and challenges.", font, 16, TextAlignmentOptions.Center);
            RectTransform sRT = sub.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.5f, 1);
            sRT.anchorMax = new Vector2(0.5f, 1);
            sRT.pivot = new Vector2(0.5f, 1);
            sRT.sizeDelta = new Vector2(850, 30);
            sRT.anchoredPosition = new Vector2(0, -75);
            sub.color = new Color(0.85f, 0.9f, 0.95f);

            // Easy Option
            Button easyBtn = CreateButton("EasyButton", card.transform, "<b>EASY MODE — 10 LEVELS</b>\n<size=13><color=#A8E6CF>Generous Timers (180s - 320s) | Wide Corridors | Relaxed Labyrinth Exploration</color></size>", font, new Vector2(0, -125), new Vector2(820, 88), new Color(0.12f, 0.55f, 0.3f));

            // Medium Option
            Button medBtn = CreateButton("MediumButton", card.transform, "<b>MEDIUM MODE — 10 LEVELS</b>\n<size=13><color=#FFE082>Balanced Timers (150s - 270s) | Branching Routes | Standard Labyrinth Challenge</color></size>", font, new Vector2(0, -230), new Vector2(820, 88), new Color(0.18f, 0.48f, 0.78f));

            // Hard Option
            Button hardBtn = CreateButton("HardButton", card.transform, "<b>HARD MODE — 10 LEVELS</b>\n<size=13><color=#FF8A80>Strict Timers (120s - 240s) | Complex Labyrinths | Fast-Paced Navigation & Reflexes</color></size>", font, new Vector2(0, -335), new Vector2(820, 88), new Color(0.75f, 0.25f, 0.25f));

            Button backBtn = CreateButton("BackButton", card.transform, "BACK TO MAIN MENU", font, new Vector2(0, -460), new Vector2(360, 50), new Color(0.25f, 0.3f, 0.38f));

            SetPrivateField(uiManager, "difficultyPanel", diffRoot);
            SetPrivateField(uiManager, "easyButton", easyBtn);
            SetPrivateField(uiManager, "mediumButton", medBtn);
            SetPrivateField(uiManager, "hardButton", hardBtn);
            SetPrivateField(uiManager, "difficultyBackButton", backBtn);

            diffRoot.SetActive(false);
        }

        private static void BuildLevelSelectPanel(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject lvlRoot = CreateUIObject("LevelSelectPanel", parent.transform);
            SetStretch(lvlRoot.GetComponent<RectTransform>());
            Image overlay = lvlRoot.AddComponent<Image>();
            overlay.color = new Color(0.04f, 0.07f, 0.12f, 0.96f);

            GameObject card = CreateUIObject("LevelCard", lvlRoot.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(1280, 840);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.06f, 0.09f, 0.15f, 0.98f);

            TextMeshProUGUI title = CreateText("Title", card.transform, "LEVEL SELECTION — MEDIUM MODE", font, 34, TextAlignmentOptions.Center);
            RectTransform tRT = title.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1);
            tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.sizeDelta = new Vector2(900, 44);
            tRT.anchoredPosition = new Vector2(0, -22);
            title.color = new Color(0.3f, 0.85f, 1f);

            TextMeshProUGUI sub = CreateText("Subtitle", card.transform, "Select any unlocked level to play or replay. Level 1 of all difficulties is available immediately.", font, 15, TextAlignmentOptions.Center);
            RectTransform sRT = sub.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.5f, 1);
            sRT.anchorMax = new Vector2(0.5f, 1);
            sRT.pivot = new Vector2(0.5f, 1);
            sRT.sizeDelta = new Vector2(900, 26);
            sRT.anchoredPosition = new Vector2(0, -66);
            sub.color = new Color(0.85f, 0.9f, 0.95f);

            // Difficulty Switcher Tabs
            Button easyTab = CreateButton("EasyTab", card.transform, "EASY (1-10)", font, new Vector2(-230, -98), new Vector2(210, 42), new Color(0.12f, 0.55f, 0.3f));
            Button medTab = CreateButton("MediumTab", card.transform, "MEDIUM (1-10)", font, new Vector2(0, -98), new Vector2(210, 42), new Color(0.18f, 0.48f, 0.78f));
            Button hardTab = CreateButton("HardTab", card.transform, "HARD (1-10)", font, new Vector2(230, -98), new Vector2(210, 42), new Color(0.75f, 0.25f, 0.25f));

            // 10 Level Cards Grid (2 columns x 5 rows)
            Button[] lvlBtns = new Button[10];
            float[] xPos = new float[2] { -295f, 295f };
            float[] yPos = new float[5] { -160f, -255f, -350f, -445f, -540f };

            for (int i = 0; i < 10; i++)
            {
                int lvlNum = i + 1;
                int col = i % 2;
                int row = i / 2;
                Vector2 pos = new Vector2(xPos[col], yPos[row]);
                Button btn = CreateButton($"LevelCard_{lvlNum}", card.transform, $"Level {lvlNum}", font, pos, new Vector2(560, 80), new Color(0.14f, 0.22f, 0.32f));
                var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.fontSize = 14;
                    txt.alignment = TextAlignmentOptions.Left;
                    RectTransform txtRT = txt.GetComponent<RectTransform>();
                    txtRT.offsetMin = new Vector2(16, 0);
                    txtRT.offsetMax = new Vector2(-16, 0);
                }
                lvlBtns[i] = btn;
            }

            Button backBtn = CreateButton("BackButton", card.transform, "BACK TO MAIN MENU", font, new Vector2(0, -640), new Vector2(360, 48), new Color(0.25f, 0.3f, 0.38f));

            SetPrivateField(uiManager, "levelSelectPanel", lvlRoot);
            SetPrivateField(uiManager, "levelSelectTitleText", title);
            SetPrivateField(uiManager, "levelSelectEasyTabButton", easyTab);
            SetPrivateField(uiManager, "levelSelectMediumTabButton", medTab);
            SetPrivateField(uiManager, "levelSelectHardTabButton", hardTab);
            SetPrivateField(uiManager, "levelButtons", lvlBtns);
            SetPrivateField(uiManager, "levelSelectBackButton", backBtn);

            lvlRoot.SetActive(false);
        }

        private static void BuildSettingsModal(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject settingsRoot = CreateUIObject("SettingsModalRoot", parent.transform);
            SetStretch(settingsRoot.GetComponent<RectTransform>());
            Image overlay = settingsRoot.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.88f);

            GameObject card = CreateUIObject("SettingsCard", settingsRoot.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(760, 540);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.07f, 0.11f, 0.17f, 0.98f);

            TextMeshProUGUI title = CreateText("Title", card.transform, "SETTINGS & AUDIO CONTROLS", font, 32, TextAlignmentOptions.Center);
            RectTransform tRT = title.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1);
            tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.sizeDelta = new Vector2(650, 46);
            tRT.anchoredPosition = new Vector2(0, -24);
            title.color = new Color(0.3f, 0.85f, 1f);

            TextMeshProUGUI sub = CreateText("Sub", card.transform, "Toggle background audio tracks and sound effects", font, 16, TextAlignmentOptions.Center);
            RectTransform sRT = sub.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.5f, 1);
            sRT.anchorMax = new Vector2(0.5f, 1);
            sRT.pivot = new Vector2(0.5f, 1);
            sRT.sizeDelta = new Vector2(650, 26);
            sRT.anchoredPosition = new Vector2(0, -70);
            sub.color = new Color(0.85f, 0.9f, 0.95f);

            Button musicBtn = CreateButton("SettingsMusicBtn", card.transform, "MUSIC: ON", font, new Vector2(-130, -118), new Vector2(230, 48), new Color(0.15f, 0.55f, 0.35f));
            TextMeshProUGUI musicTxt = musicBtn.GetComponentInChildren<TextMeshProUGUI>();

            Button sfxBtn = CreateButton("SettingsSfxBtn", card.transform, "SFX: ON", font, new Vector2(130, -118), new Vector2(230, 48), new Color(0.18f, 0.45f, 0.65f));
            TextMeshProUGUI sfxTxt = sfxBtn.GetComponentInChildren<TextMeshProUGUI>();

            string controlsInfo =
                "<b>GAMEPLAY CONTROLS & SHORTCUTS:</b>\n" +
                "• <b>WASD / Arrow Keys</b>: Steer 3D Exploration Bot\n" +
                "• <b>Mouse Movement</b>: Orbit Third-Person Camera\n" +
                "• <b>Spacebar</b>: Jump Over Low Barriers\n" +
                "• <b>[E] Key</b>: Interact with Glowing IT Quiz Terminals\n" +
                "• <b>Escape [Esc]</b>: Pause Expedition / Return to Menu";

            TextMeshProUGUI body = CreateText("ControlsInfo", card.transform, controlsInfo, font, 16, TextAlignmentOptions.Left);
            RectTransform bRT = body.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0.5f, 1);
            bRT.anchorMax = new Vector2(0.5f, 1);
            bRT.pivot = new Vector2(0.5f, 1);
            bRT.sizeDelta = new Vector2(650, 160);
            bRT.anchoredPosition = new Vector2(0, -195);
            body.color = new Color(0.9f, 0.93f, 0.98f);

            Button closeBtn = CreateButton("CloseButton", card.transform, "CLOSE SETTINGS", font, new Vector2(0, 28), new Vector2(320, 48), new Color(0.15f, 0.65f, 0.35f));
            RectTransform cbRT = closeBtn.GetComponent<RectTransform>();
            cbRT.anchorMin = new Vector2(0.5f, 0);
            cbRT.anchorMax = new Vector2(0.5f, 0);
            cbRT.pivot = new Vector2(0.5f, 0);

            SetPrivateField(uiManager, "settingsPanel", settingsRoot);
            SetPrivateField(uiManager, "settingsMusicButton", musicBtn);
            SetPrivateField(uiManager, "settingsMusicText", musicTxt);
            SetPrivateField(uiManager, "settingsSfxButton", sfxBtn);
            SetPrivateField(uiManager, "settingsSfxText", sfxTxt);
            SetPrivateField(uiManager, "settingsCloseButton", closeBtn);

            settingsRoot.SetActive(false);
        }

        private static void BuildHowToPlayModal(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject htpRoot = CreateUIObject("HowToPlayPanel", parent.transform);
            SetStretch(htpRoot.GetComponent<RectTransform>());
            Image overlay = htpRoot.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.88f);

            GameObject card = CreateUIObject("HowToPlayCard", htpRoot.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(920, 680);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.07f, 0.11f, 0.17f, 0.98f);

            TextMeshProUGUI title = CreateText("Title", card.transform, "HOW TO PLAY MAZED", font, 32, TextAlignmentOptions.Center);
            RectTransform tRT = title.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1);
            tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.sizeDelta = new Vector2(850, 48);
            tRT.anchoredPosition = new Vector2(0, -24);
            title.color = new Color(0.0f, 1f, 0.65f);

            string guideContent =
                "<color=#00FFA3><b>[ 🎮 3D NAVIGATION & CAMERA ]</b></color>\n" +
                "Use <b>WASD</b> or Arrow keys to steer your character through the maze corridors. Move your <b>Mouse</b> to orbit the third-person camera. Press <b>Spacebar</b> to jump.\n\n" +
                "<color=#FFD700><b>[ 🔑 ACQUIRING LEVEL KEYS ]</b></color>\n" +
                "Each maze holds a designated Key (Bronze, Silver, Gold, Cyan, etc.) hidden in side corridors. Explore the labyrinth to locate and collect the key!\n\n" +
                "<color=#00E5FF><b>[ 🚪 3D EXIT DOOR & LEVEL COMPLETION ]</b></color>\n" +
                "Once you possess the required key, head to the 3D Exit Door at the end of the labyrinth. Approaching the doorway automatically slides open the exit door to complete the level!\n\n" +
                "<color=#A78BFA><b>[ 🧩 OPTIONAL BONUS PUZZLES ]</b></color>\n" +
                "Discover glowing puzzle terminals placed in the maze. Approach and press <b>[E]</b> to solve multiple-choice challenges for <b>+100 Bonus Points</b>!\n\n" +
                "<color=#FBBF24><b>[ ⏱️ LEVEL TIMERS & HIGH SCORES ]</b></color>\n" +
                "Each level features an independent countdown timer. Clear the level before time runs out to earn +200 clear bonus points plus remaining time bonus!";

            TextMeshProUGUI body = CreateText("GuideBody", card.transform, guideContent, font, 16, TextAlignmentOptions.Left);
            RectTransform bRT = body.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0.5f, 1);
            bRT.anchorMax = new Vector2(0.5f, 1);
            bRT.pivot = new Vector2(0.5f, 1);
            bRT.sizeDelta = new Vector2(840, 470);
            bRT.anchoredPosition = new Vector2(0, -80);
            body.color = new Color(0.9f, 0.93f, 0.98f);

            Button closeBtn = CreateButton("CloseButton", card.transform, "GOT IT — CLOSE GUIDE", font, new Vector2(0, 28), new Vector2(340, 50), new Color(0.15f, 0.65f, 0.35f));
            RectTransform cbRT = closeBtn.GetComponent<RectTransform>();
            cbRT.anchorMin = new Vector2(0.5f, 0);
            cbRT.anchorMax = new Vector2(0.5f, 0);
            cbRT.pivot = new Vector2(0.5f, 0);

            SetPrivateField(uiManager, "howToPlayPanel", htpRoot);
            SetPrivateField(uiManager, "howToPlayCloseButton", closeBtn);

            htpRoot.SetActive(false);
        }

        private static void BuildPauseMenu(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject pauseRoot = CreateUIObject("PauseMenuPanel", parent.transform);
            SetStretch(pauseRoot.GetComponent<RectTransform>());
            Image overlay = pauseRoot.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.85f);

            GameObject card = CreateUIObject("PauseCard", pauseRoot.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(640, 560);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.08f, 0.12f, 0.17f, 0.98f);

            TextMeshProUGUI title = CreateText("Title", card.transform, "GAME PAUSED", font, 36, TextAlignmentOptions.Center);
            RectTransform tRT = title.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1);
            tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.sizeDelta = new Vector2(500, 55);
            tRT.anchoredPosition = new Vector2(0, -25);
            title.color = Color.white;

            Button resumeBtn = CreateButton("ResumeButton", card.transform, "RESUME EXPEDITION", font, new Vector2(0, -90), new Vector2(380, 48), new Color(0.15f, 0.55f, 0.85f));
            Button restartBtn = CreateButton("RestartButton", card.transform, "RESTART LEVEL", font, new Vector2(0, -148), new Vector2(380, 48), new Color(0.68f, 0.32f, 0.18f));
            Button levelBtn = CreateButton("PauseLevelSelectButton", card.transform, "LEVEL SELECTION", font, new Vector2(0, -206), new Vector2(380, 48), new Color(0.22f, 0.38f, 0.65f));

            // Audio quick toggles in Pause Menu
            Button musicBtn = CreateButton("PauseMusicBtn", card.transform, "MUSIC: ON", font, new Vector2(-100, -266), new Vector2(180, 42), new Color(0.15f, 0.55f, 0.35f));
            TextMeshProUGUI musicTxt = musicBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (musicTxt != null) musicTxt.fontSize = 15;

            Button sfxBtn = CreateButton("PauseSfxBtn", card.transform, "SFX: ON", font, new Vector2(100, -266), new Vector2(180, 42), new Color(0.18f, 0.45f, 0.65f));
            TextMeshProUGUI sfxTxt = sfxBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (sfxTxt != null) sfxTxt.fontSize = 15;

            Button menuBtn = CreateButton("MainMenuButton", card.transform, "MAIN MENU / LANDING PAGE", font, new Vector2(0, -328), new Vector2(380, 48), new Color(0.25f, 0.3f, 0.38f));

            SetPrivateField(uiManager, "pauseMenuPanel", pauseRoot);
            SetPrivateField(uiManager, "pauseResumeButton", resumeBtn);
            SetPrivateField(uiManager, "pauseRestartButton", restartBtn);
            SetPrivateField(uiManager, "pauseLevelSelectButton", levelBtn);
            SetPrivateField(uiManager, "pauseMusicButton", musicBtn);
            SetPrivateField(uiManager, "pauseMusicText", musicTxt);
            SetPrivateField(uiManager, "pauseSfxButton", sfxBtn);
            SetPrivateField(uiManager, "pauseSfxText", sfxTxt);
            SetPrivateField(uiManager, "pauseMainMenuButton", menuBtn);
        }

        private static void BuildLevelCompleteScreen(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject lcRoot = CreateUIObject("LevelCompletePanel", parent.transform);
            SetStretch(lcRoot.GetComponent<RectTransform>());
            Image overlay = lcRoot.AddComponent<Image>();
            overlay.color = new Color(0.01f, 0.05f, 0.04f, 0.95f);

            GameObject card = CreateUIObject("CompleteCard", lcRoot.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(840, 650);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.04f, 0.09f, 0.08f, 0.98f);

            // Top Glow Bar
            GameObject glowBar = CreateUIObject("TopGlowBar", card.transform);
            RectTransform gbRT = glowBar.GetComponent<RectTransform>();
            gbRT.anchorMin = new Vector2(0.5f, 1f);
            gbRT.anchorMax = new Vector2(0.5f, 1f);
            gbRT.pivot = new Vector2(0.5f, 1f);
            gbRT.sizeDelta = new Vector2(840, 4);
            gbRT.anchoredPosition = Vector2.zero;
            Image gbImg = glowBar.AddComponent<Image>();
            gbImg.color = new Color(0f, 1f, 0.65f, 0.95f);

            // Subtitle Badge
            TextMeshProUGUI badge = CreateText("Badge", card.transform, "✦  LABYRINTH CLEARED  ✦", font, 13, TextAlignmentOptions.Center);
            RectTransform bRT = badge.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0.5f, 1);
            bRT.anchorMax = new Vector2(0.5f, 1);
            bRT.pivot = new Vector2(0.5f, 1);
            bRT.sizeDelta = new Vector2(760, 22);
            bRT.anchoredPosition = new Vector2(0, -16);
            badge.color = new Color(0.0f, 1f, 0.65f, 0.95f);

            // Celebratory Title
            TextMeshProUGUI title = CreateText("Title", card.transform, "LEVEL COMPLETED!", font, 34, TextAlignmentOptions.Center);
            RectTransform tRT = title.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1);
            tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.sizeDelta = new Vector2(760, 44);
            tRT.anchoredPosition = new Vector2(0, -38);
            title.color = Color.white;

            // Highlighted Score Display Sub-Card
            GameObject scoreBox = CreateUIObject("ScoreBadgeBox", card.transform);
            RectTransform sbRT = scoreBox.GetComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(0.5f, 1);
            sbRT.anchorMax = new Vector2(0.5f, 1);
            sbRT.pivot = new Vector2(0.5f, 1);
            sbRT.sizeDelta = new Vector2(760, 160);
            sbRT.anchoredPosition = new Vector2(0, -90);
            Image sbBg = scoreBox.AddComponent<Image>();
            sbBg.color = new Color(0.08f, 0.16f, 0.13f, 0.96f);

            TextMeshProUGUI score = CreateText("ScoreText", scoreBox.transform, "SCORE EARNED THIS LEVEL\n<size=46><color=#FFD700><b>+350 PTS</b></color></size>\n<size=14><color=#A7F3D0>Clear Bonus: +200 PTS  •  Time Bonus: +150 PTS</color></size>\n<size=14><color=#BAE6FD>Total Campaign: 350 PTS  |  Record Best: 350 PTS</color></size>", font, 18, TextAlignmentOptions.Center);
            SetStretch(score.GetComponent<RectTransform>());

            // Time & Exploration Details
            TextMeshProUGUI time = CreateText("TimeText", card.transform, "⏱ Clear Time: 01:24", font, 20, TextAlignmentOptions.Center);
            RectTransform tmRT = time.GetComponent<RectTransform>();
            tmRT.anchorMin = new Vector2(0.5f, 1);
            tmRT.anchorMax = new Vector2(0.5f, 1);
            tmRT.pivot = new Vector2(0.5f, 1);
            tmRT.sizeDelta = new Vector2(740, 30);
            tmRT.anchoredPosition = new Vector2(0, -262);
            time.color = Color.white;

            TextMeshProUGUI stats = CreateText("StatsText", card.transform, "🔑 Exit Key: Bronze Key  |  🧩 Bonus Puzzles: Optional", font, 18, TextAlignmentOptions.Center);
            RectTransform stRT = stats.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0.5f, 1);
            stRT.anchorMax = new Vector2(0.5f, 1);
            stRT.pivot = new Vector2(0.5f, 1);
            stRT.sizeDelta = new Vector2(740, 30);
            stRT.anchoredPosition = new Vector2(0, -296);
            stats.color = new Color(0.85f, 0.95f, 0.9f);

            // Action Buttons
            Button nextBtn = CreateButton("NextLevelButton", card.transform, "<b>PROCEED TO NEXT LEVEL  ▶</b>", font, new Vector2(0, -354), new Vector2(480, 52), new Color(0.04f, 0.65f, 0.40f));
            Button replayBtn = CreateButton("ReplayLevelButton", card.transform, "<b>↺  REPLAY LEVEL</b>", font, new Vector2(0, -418), new Vector2(480, 46), new Color(0.14f, 0.44f, 0.72f));
            Button lvlSelectBtn = CreateButton("LevelSelectButton", card.transform, "<b>☵  LEVEL SELECTION</b>", font, new Vector2(0, -476), new Vector2(480, 46), new Color(0.22f, 0.32f, 0.50f));
            Button menuBtn = CreateButton("MainMenuButton", card.transform, "<b>✕  MAIN MENU</b>", font, new Vector2(0, -534), new Vector2(480, 46), new Color(0.22f, 0.26f, 0.34f));

            SetPrivateField(uiManager, "levelCompletePanel", lcRoot);
            SetPrivateField(uiManager, "levelCompleteTitleText", title);
            SetPrivateField(uiManager, "levelCompleteScoreText", score);
            SetPrivateField(uiManager, "levelCompleteTimeText", time);
            SetPrivateField(uiManager, "levelCompleteStatsText", stats);
            SetPrivateField(uiManager, "nextLevelButton", nextBtn);
            SetPrivateField(uiManager, "replayLevelButton", replayBtn);
            SetPrivateField(uiManager, "levelCompleteLevelSelectButton", lvlSelectBtn);
            SetPrivateField(uiManager, "levelCompleteMainMenuButton", menuBtn);
        }

        private static void BuildGameOverScreen(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject goRoot = CreateUIObject("GameOverPanel", parent.transform);
            SetStretch(goRoot.GetComponent<RectTransform>());
            Image overlay = goRoot.AddComponent<Image>();
            overlay.color = new Color(0.15f, 0.02f, 0.02f, 0.94f);

            GameObject card = CreateUIObject("GameOverCard", goRoot.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(680, 480);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.16f, 0.06f, 0.06f, 0.98f);

            TextMeshProUGUI title = CreateText("Title", card.transform, "TIME EXPIRED — LEVEL FAILED", font, 36, TextAlignmentOptions.Center);
            RectTransform tRT = title.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1);
            tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.sizeDelta = new Vector2(600, 55);
            tRT.anchoredPosition = new Vector2(0, -32);
            title.color = new Color(1f, 0.25f, 0.25f);

            TextMeshProUGUI score = CreateText("ScoreText", card.transform, "Attempt Score: 0", font, 24, TextAlignmentOptions.Center);
            RectTransform sRT = score.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.5f, 1);
            sRT.anchorMax = new Vector2(0.5f, 1);
            sRT.pivot = new Vector2(0.5f, 1);
            sRT.sizeDelta = new Vector2(500, 50);
            sRT.anchoredPosition = new Vector2(0, -100);
            score.color = Color.white;

            Button retryBtn = CreateButton("RetryButton", card.transform, "RETRY LEVEL", font, new Vector2(0, -190), new Vector2(380, 52), new Color(0.85f, 0.25f, 0.25f));
            Button lvlSelectBtn = CreateButton("LevelSelectButton", card.transform, "LEVEL SELECTION", font, new Vector2(0, -252), new Vector2(380, 48), new Color(0.18f, 0.45f, 0.72f));
            Button menuBtn = CreateButton("MainMenuButton", card.transform, "MAIN MENU", font, new Vector2(0, -310), new Vector2(380, 48), new Color(0.25f, 0.3f, 0.38f));

            SetPrivateField(uiManager, "gameOverPanel", goRoot);
            SetPrivateField(uiManager, "gameOverTitleText", title);
            SetPrivateField(uiManager, "gameOverScoreText", score);
            SetPrivateField(uiManager, "gameOverRestartButton", retryBtn);
            SetPrivateField(uiManager, "gameOverLevelSelectButton", lvlSelectBtn);
            SetPrivateField(uiManager, "gameOverMainMenuButton", menuBtn);
        }

        private static void BuildVictoryScreen(GameObject parent, UIManager uiManager, TMP_FontAsset font)
        {
            GameObject vicRoot = CreateUIObject("VictoryPanel", parent.transform);
            SetStretch(vicRoot.GetComponent<RectTransform>());
            Image overlay = vicRoot.AddComponent<Image>();
            overlay.color = new Color(0.02f, 0.12f, 0.06f, 0.94f);

            GameObject card = CreateUIObject("VictoryCard", vicRoot.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(780, 580);
            cardRT.anchoredPosition = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.06f, 0.16f, 0.10f, 0.98f);

            TextMeshProUGUI title = CreateText("Title", card.transform, "DIFFICULTY CONQUERED!", font, 38, TextAlignmentOptions.Center);
            RectTransform tRT = title.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1);
            tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.sizeDelta = new Vector2(650, 55);
            tRT.anchoredPosition = new Vector2(0, -28);
            title.color = new Color(0.3f, 1f, 0.5f);

            TextMeshProUGUI score = CreateText("ScoreText", card.transform, "FINAL DIFFICULTY SCORE: 2500 PTS", font, 26, TextAlignmentOptions.Center);
            RectTransform sRT = score.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.5f, 1);
            sRT.anchorMax = new Vector2(0.5f, 1);
            sRT.pivot = new Vector2(0.5f, 1);
            sRT.sizeDelta = new Vector2(600, 40);
            sRT.anchoredPosition = new Vector2(0, -88);
            score.color = new Color(1f, 0.85f, 0.2f);

            TextMeshProUGUI time = CreateText("TimeText", card.transform, "Total Clear Time: 16:45", font, 22, TextAlignmentOptions.Center);
            RectTransform tmRT = time.GetComponent<RectTransform>();
            tmRT.anchorMin = new Vector2(0.5f, 1);
            tmRT.anchorMax = new Vector2(0.5f, 1);
            tmRT.pivot = new Vector2(0.5f, 1);
            tmRT.sizeDelta = new Vector2(600, 35);
            tmRT.anchoredPosition = new Vector2(0, -135);
            time.color = Color.white;

            TextMeshProUGUI stats = CreateText("StatsText", card.transform, "All 10 IT Challenges Completed!\nCertified IT Explorer across OOP, OS, Networks, and DBMS!", font, 20, TextAlignmentOptions.Center);
            RectTransform stRT = stats.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0.5f, 1);
            stRT.anchorMax = new Vector2(0.5f, 1);
            stRT.pivot = new Vector2(0.5f, 1);
            stRT.sizeDelta = new Vector2(650, 65);
            stRT.anchoredPosition = new Vector2(0, -182);
            stats.color = new Color(0.85f, 0.95f, 0.9f);

            Button playAgainBtn = CreateButton("PlayAgainButton", card.transform, "REPLAY LEVEL 10 / DIFFICULTY", font, new Vector2(0, -270), new Vector2(380, 52), new Color(0.18f, 0.68f, 0.38f));
            Button lvlSelectBtn = CreateButton("LevelSelectButton", card.transform, "LEVEL SELECTION", font, new Vector2(0, -332), new Vector2(380, 48), new Color(0.2f, 0.45f, 0.7f));
            Button menuBtn = CreateButton("MainMenuButton", card.transform, "MAIN MENU", font, new Vector2(0, -390), new Vector2(380, 48), new Color(0.25f, 0.3f, 0.38f));

            SetPrivateField(uiManager, "victoryPanel", vicRoot);
            SetPrivateField(uiManager, "victoryTitleText", title);
            SetPrivateField(uiManager, "victoryScoreText", score);
            SetPrivateField(uiManager, "victoryTimeText", time);
            SetPrivateField(uiManager, "victoryStatsText", stats);
            SetPrivateField(uiManager, "victoryPlayAgainButton", playAgainBtn);
            SetPrivateField(uiManager, "victoryLevelSelectButton", lvlSelectBtn);
            SetPrivateField(uiManager, "victoryMainMenuButton", menuBtn);
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, TMP_FontAsset font, float size, TextAlignmentOptions alignment)
        {
            GameObject go = CreateUIObject(name, parent);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, string label, TMP_FontAsset font, Vector2 anchoredPos, Vector2 size, Color btnColor)
        {
            GameObject btnGO = CreateUIObject(name, parent);
            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            Image img = btnGO.AddComponent<Image>();
            img.color = btnColor;
            img.raycastTarget = true;

            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = true;

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.85f, 0.95f, 1f, 1f);
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            cb.selectedColor = new Color(0.85f, 0.95f, 1f, 1f);
            cb.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            TextMeshProUGUI txt = CreateText("Text", btnGO.transform, label, font, 18, TextAlignmentOptions.Center);
            SetStretch(txt.GetComponent<RectTransform>());
            txt.color = Color.white;

            return btn;
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
