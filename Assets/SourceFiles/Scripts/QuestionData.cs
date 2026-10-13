using System;
using System.Collections.Generic;

namespace Mazed.Quiz
{
    [Serializable]
    public class QuizQuestion
    {
        public string id;
        public string subject;
        public string difficulty = "Medium";
        public string question;
        public string[] options;
        public int correctAnswerIndex;
        public int correctOptionIndex => correctAnswerIndex;
        public string explanation;

        public bool IsCorrect(int chosenIndex)
        {
            return chosenIndex == correctAnswerIndex;
        }

        public string GetCorrectAnswerText()
        {
            if (options != null && correctAnswerIndex >= 0 && correctAnswerIndex < options.Length)
            {
                return options[correctAnswerIndex];
            }
            return string.Empty;
        }
    }

    [Serializable]
    public class QuizQuestionDatabase
    {
        public List<QuizQuestion> questions = new List<QuizQuestion>();
    }
}
