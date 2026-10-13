using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mazed.Quiz
{
    public class QuestionBank : MonoBehaviour
    {
        public static QuestionBank Instance { get; private set; }

        [Header("Data Source")]
        [Tooltip("Path within Resources folder (without .json extension)")]
        [SerializeField] private string resourcesPath = "QuizData/questions";

        private List<QuizQuestion> allQuestions = new List<QuizQuestion>();
        private Dictionary<string, List<QuizQuestion>> questionsBySubject = new Dictionary<string, List<QuizQuestion>>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> answeredQuestionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static readonly string[] CoreSubjects = new string[]
        {
            "Object-Oriented Programming",
            "Operating Systems",
            "Database Management Systems",
            "Computer Networks",
            "Data Structures and Algorithms",
            "Software Engineering",
            "Web Development and Programming"
        };
        private int roundRobinIndex = 0;

        public int TotalQuestionsCount => allQuestions.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadQuestionBank();
        }

        public void LoadQuestionBank()
        {
            allQuestions.Clear();
            questionsBySubject.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcesPath);
            if (jsonAsset == null)
            {
                Debug.LogError($"[QuestionBank] Failed to load questions JSON from Resources/{resourcesPath}!");
                return;
            }

            try
            {
                QuizQuestionDatabase db = JsonUtility.FromJson<QuizQuestionDatabase>(jsonAsset.text);
                if (db != null && db.questions != null)
                {
                    allQuestions = db.questions;

                    foreach (var q in allQuestions)
                    {
                        if (string.IsNullOrEmpty(q.subject)) continue;

                        if (!questionsBySubject.ContainsKey(q.subject))
                        {
                            questionsBySubject[q.subject] = new List<QuizQuestion>();
                        }
                        questionsBySubject[q.subject].Add(q);
                    }

                    Debug.Log($"[QuestionBank] Successfully loaded {allQuestions.Count} questions across {questionsBySubject.Count} subjects.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QuestionBank] Exception parsing questions JSON: {ex.Message}");
            }
        }

        public QuizQuestion GetQuestionById(string id)
        {
            return allQuestions.FirstOrDefault(q => q.id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public List<QuizQuestion> GetQuestionsForSubject(string subject)
        {
            if (questionsBySubject.TryGetValue(subject, out var list))
            {
                return list;
            }
            return new List<QuizQuestion>();
        }

        public QuizQuestion GetNextQuestionForSubject(string subject, string difficulty = null)
        {
            // If subject is null or generic "Mixed", pick across the 4 core subjects in round-robin fashion
            if (string.IsNullOrEmpty(subject) || subject.Equals("Mixed", StringComparison.OrdinalIgnoreCase) || subject.Equals("Mixed Technical IT", StringComparison.OrdinalIgnoreCase))
            {
                string chosenSubject = CoreSubjects[roundRobinIndex % CoreSubjects.Length];
                roundRobinIndex++;
                subject = chosenSubject;
            }

            if (!questionsBySubject.TryGetValue(subject, out var list) || list.Count == 0)
            {
                Debug.LogWarning($"[QuestionBank] No questions found for subject: {subject}, falling back to all questions.");
                list = allQuestions;
            }

            // Filter by difficulty if specified
            List<QuizQuestion> candidateList = list;
            if (!string.IsNullOrEmpty(difficulty))
            {
                var diffFiltered = list.Where(q => string.Equals(q.difficulty, difficulty, StringComparison.OrdinalIgnoreCase)).ToList();
                if (diffFiltered.Count > 0)
                {
                    candidateList = diffFiltered;
                }
            }

            // Find an unanswered question first to prevent repetition
            var unanswered = candidateList.Where(q => !answeredQuestionIds.Contains(q.id)).ToList();
            if (unanswered.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, unanswered.Count);
                return unanswered[index];
            }

            // If all in candidate list have been answered, pick from all unanswered in that difficulty
            if (!string.IsNullOrEmpty(difficulty))
            {
                var allDiffUnanswered = allQuestions.Where(q => string.Equals(q.difficulty, difficulty, StringComparison.OrdinalIgnoreCase) && !answeredQuestionIds.Contains(q.id)).ToList();
                if (allDiffUnanswered.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, allDiffUnanswered.Count);
                    return allDiffUnanswered[index];
                }
            }

            // Fallback random
            int fallbackIndex = UnityEngine.Random.Range(0, candidateList.Count);
            return candidateList[fallbackIndex];
        }

        public QuizQuestion GetMixedQuestion(string difficulty = null, string preferredSubject = null)
        {
            return GetNextQuestionForSubject(preferredSubject, difficulty);
        }

        public void MarkQuestionAnswered(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                answeredQuestionIds.Add(id);
            }
        }

        public void ResetProgress()
        {
            answeredQuestionIds.Clear();
        }
    }
}
