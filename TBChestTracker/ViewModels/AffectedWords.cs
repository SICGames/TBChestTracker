using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class AffectedWords
    {
        public AffectedWords()
        {
            IncorrectWords = new ObservableCollection<AffectedWords>();
        }
        public string Word { get; set; }
        public ObservableCollection<AffectedWords> IncorrectWords { get; set; }
        public void AddIncorrectWord(string word)
        {
            
            this.IncorrectWords.Add(new AffectedWords { Word = word });
        }
        public AffectedWords(string word, string incorrectWord)
        {
            this.Word = word;
            this.IncorrectWords.Add(new AffectedWords { Word = incorrectWord });    
        }
        public AffectedWords(string word, ObservableCollection<AffectedWords> incorrectWords)
        {
            this.Word = word;
            this.IncorrectWords = incorrectWords;
        }
    }
}
