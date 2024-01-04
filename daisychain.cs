using System.Net;

namespace Daisy.Chain
{
    // Solver holds the logic for solving the anagrams
    // Is it good code? No. But did I try to follow best practices? Also no. But is it well tested? Again, no. But is it efficient? No.
    public class Solver{
        
        int numWords;
        int numLetters;

        List<string>[]? possibleWords;

        string[]? input;

        int endingIndex;

        List<string[]>? solutions;

        // Check if a word contains all the letters in an array of chars
        private static bool containsAll(string s, char[] letters)
        {
            Dictionary<char, int> wordCount = new Dictionary<char, int>();
            Dictionary<char, int> letterCount = new Dictionary<char, int>();

            foreach (char c in s.ToLower()){
                if (wordCount.ContainsKey(c)){
                    wordCount[c] += 1;
                }
                else{
                    wordCount.Add(c, 1);
                }
            }

            foreach (char c in letters){
                char lowercase = char.ToLower(c);
                if (letterCount.ContainsKey(lowercase)){
                    letterCount[lowercase] += 1;
                }
                else{
                    letterCount.Add(lowercase, 1);
                }
            }

            foreach (KeyValuePair<char, int> entry in letterCount){
                if (!wordCount.ContainsKey(entry.Key) || wordCount[entry.Key] <entry.Value){
                    return false;
                }
            }

            return true;

        }

        // Increment a number, rolling over to 0 when it reaches the max size
        private static int rollingIncrement(int index, int maxSize){
            if(index + 1 == maxSize){
                return 0;
            }
            return index + 1;
        }

        // Recursively solve the anagrams
        private void recursiveSolve(int index, char lastLetter, string[] solution, string firstGuessWord){
            // Get the letters of the anagram, plus the last letter of the previous word
            char[] letters = input[index].ToCharArray().Append(lastLetter).ToArray();
            

            // For each possible word for the anagram
            foreach(string word in possibleWords[index]){

                // If the word contains all the letters of the anagram, it's a solution
                if(containsAll(word, letters)){

                    string [] thisSolution = (string[]) solution.Clone();

                    thisSolution[index] = word;


                    if(index == endingIndex){
                        // If the index is the ending index, we're done (we've checked the whole loop)
                        if(word == firstGuessWord) {
                            // If we're back at the word we started, it's a valid loop
                            solutions.Add(thisSolution);

                            return;
                        } else {
                            // Otherwise, the solution is invalid
                            return;
                        }

                    } else {
                        //otherwise, retroactively solve the next anagram
                        recursiveSolve(rollingIncrement(index, numWords), word[word.Length - 1], thisSolution, firstGuessWord);
                    }
                }
            }
        }

        public List<string[]> solve(string[] input, string dictionaryPath){
            // Initialize the variables
            this.input = input;
            numWords = input.Length;
            numLetters = input[0].Length;
            possibleWords = new List<string>[numWords];

            // Create an array to store the solutions
            solutions = new List<string[]>();

	        // Read the dictionary file line by line and select the words that are the correct length and contain the correct letters
            
            using (WebClient client = new WebClient())
            {
                string dictionaryUrl = "https://www.souder.ca/puzzles/dictionary.txt";
                Stream stream = client.OpenRead(dictionaryUrl);
                using (StreamReader sr = new StreamReader(stream))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    if(s.Length -1 == numLetters){
                        for(int i = 0; i < numWords; i++){

                            if(containsAll(s, input[i].ToCharArray())){
                                if(possibleWords[i] == null){
                                    possibleWords[i] = new List<string>();
                                }
                                possibleWords[i].Add(s);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < numWords; i++){
                if(possibleWords[i] == null){
                    return solutions;
                }
            }

            // Find the anagram with the least possible words
            int minIndex = 0;
            int minCount = possibleWords[0].Count;

            for (int i = 1; i < numWords; i++){
                if(possibleWords[i].Count < minCount){
                    minIndex = i;
                    minCount = possibleWords[i].Count;
                }
            }

            // Set the ending index to the anagram with the least possible words
            endingIndex = minIndex;



            // For each possible word for the anagram with the least possible words
            foreach(string word in possibleWords[minIndex]){
		        // Start an array (that will be copied) to hold the solutions for this possibility
                string[] solution = new string[numWords];

                // Solve the anagrams recursively
                recursiveSolve(rollingIncrement(minIndex, numWords), word[word.Length - 1], new string[numWords], word);
            }

            return solutions;

        }
    }

    

}