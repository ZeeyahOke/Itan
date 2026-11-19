using System;
using System.Linq;
using Itan.Quiz;
using NUnit.Framework;

namespace Itan.Tests
{
    public class QuizLogicTests
    {
        static QuizLogic NewQuiz(int seed = 1) => new QuizLogic(3, 3, new Random(seed));

        [Test]
        public void AllRightAnswers_ScoreIsThree()
        {
            var quiz = NewQuiz();
            while (!quiz.IsFinished) quiz.Answer(quiz.CurrentWord);
            Assert.AreEqual(3, quiz.Score);
            Assert.AreEqual(3, quiz.Rounds);
        }

        [Test]
        public void AllWrongAnswers_ScoreIsZero()
        {
            var quiz = NewQuiz();
            while (!quiz.IsFinished) quiz.Answer((quiz.CurrentWord + 1) % 3);
            Assert.AreEqual(0, quiz.Score);
        }

        [Test]
        public void OneWrongAnswer_ScoreIsTwo()
        {
            var quiz = NewQuiz();
            quiz.Answer(quiz.CurrentWord);
            quiz.Answer((quiz.CurrentWord + 1) % 3);
            quiz.Answer(quiz.CurrentWord);
            Assert.AreEqual(2, quiz.Score);
            Assert.IsTrue(quiz.IsFinished);
        }

        [Test]
        public void Answer_ReturnsWhetherThePickWasRight()
        {
            var quiz = NewQuiz();
            int right = quiz.CurrentWord;
            Assert.IsTrue(quiz.Answer(right));
            Assert.IsFalse(quiz.Answer((quiz.CurrentWord + 1) % 3));
        }

        [Test]
        public void EveryWordIsAskedExactlyOnce()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var quiz = NewQuiz(seed);
                var asked = new System.Collections.Generic.List<int>();
                while (!quiz.IsFinished) { asked.Add(quiz.CurrentWord); quiz.Answer(quiz.CurrentWord); }
                CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, asked);
            }
        }

        [Test]
        public void Options_IncludeTheRightWord_WithNoRepeats()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var quiz = NewQuiz(seed);
                int[] options = quiz.OptionsForRound();
                Assert.AreEqual(3, options.Length);
                Assert.Contains(quiz.CurrentWord, options);
                Assert.AreEqual(options.Length, options.Distinct().Count());
            }
        }

        [Test]
        public void AnsweringAfterTheEnd_Throws()
        {
            var quiz = NewQuiz();
            while (!quiz.IsFinished) quiz.Answer(quiz.CurrentWord);
            Assert.Throws<InvalidOperationException>(() => quiz.Answer(0));
        }
    }
}
