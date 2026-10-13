using UnityEngine;
using UnityEditor;
using Mazed.Environment;
using Mazed.Quiz;

namespace Mazed.Editor
{
    public static class MazeGameEditorMenu
    {
        [MenuItem("Maze Game/Build 3D Maze & Setup Scene", false, 1)]
        public static void BuildMazeInScene()
        {
            MazeBuilder builder = Object.FindFirstObjectByType<MazeBuilder>();
            if (builder == null)
            {
                GameObject managerGO = new GameObject("MazeManager");
                builder = managerGO.AddComponent<MazeBuilder>();
                Undo.RegisterCreatedObjectUndo(managerGO, "Create MazeManager");
            }

            builder.InitializeEnvironment();
            Selection.activeGameObject = builder.gameObject;

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            EditorUtility.DisplayDialog(
                "3D Maze Game Setup Complete",
                "The 3D Interactive Maze has been successfully configured in the active scene!\n\n" +
                "• 3D Maze Corridors and High Walls with BoxColliders\n" +
                "• PlayerRobot character with ThirdPersonController and Cinemachine\n" +
                "• 6 CS Quiz Terminals (OOP, OS, DBMS, Networks, DSA, SE & Web)\n" +
                "• 3 Locked Security Gates and 2 Collectible Keys\n" +
                "• Dynamic HUD, Quiz Modal, Pause Menu, Game Over & Victory Screens\n" +
                "• Background Music and Audio System\n\n" +
                "You can now press Play to test the complete game!",
                "Great!"
            );
        }

        [MenuItem("Maze Game/Verify Questions Database", false, 2)]
        public static void VerifyQuestionBank()
        {
            TextAsset json = Resources.Load<TextAsset>("QuizData/questions");
            if (json == null)
            {
                EditorUtility.DisplayDialog("Question Database Error", "Could not locate Resources/QuizData/questions.json!", "OK");
                return;
            }

            QuizQuestionDatabase db = JsonUtility.FromJson<QuizQuestionDatabase>(json.text);
            if (db == null || db.questions == null)
            {
                EditorUtility.DisplayDialog("Question Database Error", "Failed to deserialize questions JSON!", "OK");
                return;
            }

            int count = db.questions.Count;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Successfully verified {count} questions in the question bank!\n");

            var subjects = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var q in db.questions)
            {
                if (!subjects.ContainsKey(q.subject)) subjects[q.subject] = 0;
                subjects[q.subject]++;
            }

            foreach (var kvp in subjects)
            {
                sb.AppendLine($"• {kvp.Key}: {kvp.Value} questions");
            }

            EditorUtility.DisplayDialog("Question Database Verified", sb.ToString(), "OK");
        }
    }
}
