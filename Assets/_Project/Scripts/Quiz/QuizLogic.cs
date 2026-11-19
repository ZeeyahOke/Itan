using System;

namespace Itan.Quiz
{
    /// <summary>
    /// Pure quiz rules, kept free of Unity objects so they can be unit tested.
    /// Each word is asked once, in a shuffled order. Each round offers the
    /// right meaning plus others, shuffled. A right pick scores one point.
    /// </summary>
    public class QuizLogic
    {
        readonly int wordCount;
        readonly int optionsPerRound;
        readonly Random random;
        readonly int[] order;

        public int Round { get; private set; }
        public int Score { get; private set; }
        public int Rounds => order.Length;
        public bool IsFinished => Round >= order.Length;

        /// <summary>The word being asked about this round.</summary>
        public int CurrentWord => order[Round];

        public QuizLogic(int wordCount, int optionsPerRound, Random random)
        {
            if (wordCount < 1) throw new ArgumentException("Need at least one word", nameof(wordCount));
            this.wordCount = wordCount;
            this.optionsPerRound = Math.Max(1, Math.Min(optionsPerRound, wordCount));
            this.random = random;

            order = new int[wordCount];
            for (int i = 0; i < wordCount; i++) order[i] = i;
            Shuffle(order);
        }

        /// <summary>Word indices to show as cards this round: the right one plus others, shuffled.</summary>
        public int[] OptionsForRound()
        {
            int[] others = new int[wordCount - 1];
            for (int i = 0, k = 0; i < wordCount; i++)
                if (i != CurrentWord) others[k++] = i;
            Shuffle(others);

            int[] options = new int[optionsPerRound];
            options[0] = CurrentWord;
            for (int i = 1; i < optionsPerRound; i++) options[i] = others[i - 1];
            Shuffle(options);
            return options;
        }

        /// <summary>Record the visitor's pick for this round and move on.</summary>
        /// <returns>True if the pick was the right word.</returns>
        public bool Answer(int pickedWord)
        {
            if (IsFinished) throw new InvalidOperationException("The quiz is already finished");
            bool correct = pickedWord == CurrentWord;
            if (correct) Score++;
            Round++;
            return correct;
        }

        void Shuffle(int[] items)
        {
            for (int i = items.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }
}
