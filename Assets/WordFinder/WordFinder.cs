#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections.Generic;

// playground for finding specific words
// messy code, only used on demand then unmaintained

[CreateAssetMenu(fileName = "WORD FINDER", menuName = "WORD FINDER", order = 1)]
public class WordFinder : ScriptableObject
{
	public void Find()
	{
		var words = Resources.Load<TextAsset>("all").text.Split('\n');

		var writer = new StringWriter();
		
		foreach(var word in words)
		{
			var newWord = OneAteE(word, 0);
			//var newWord = DoubleLetters(word);
			//var newWord = Eat(word);
			//var newWord = RepeatedLetter(word);
			//var newWord = Plural(word);
			//var newWord = Pig(word);
			//var newWord = DoubleIn(word);

			if (newWord == null)
				continue;

			if (words.Contains(newWord))
		   		writer.WriteLine(word + "->" + newWord);
		   		//writer.WriteLine(newWord);
		}

		//var a= writer.ToString();
		//Debug.Log(a);
		GUIUtility.systemCopyBuffer = a;

		writer.Close();
	}

	string Eat(string word, bool excludeAte = true)
	{
		if (word.Length < 5) return null;

		var atePosition = word.IndexOf("ate");
		if (atePosition <= 0) return null;
		if (atePosition + 4 >= word.Length) return null;

		var eater = word[atePosition - 1];
		var eated = word[atePosition + 3];

		if (excludeAte)
		{
			word = word.Replace("ate", string.Empty);
			var count = word.ToCharArray().Count(x => x == eated);
			if (count == 1) return null;
		}
		
		return word.Replace(eated.ToString(), string.Empty);
	}

	string OneAteE(string word, int indicesToSkip = 0)
	{
		var onePosition = word.IndexOf("an");
		if (onePosition == -1) return null;
		if (!word.Contains('e')) return null;

		var foundIndexes = new List<int>();
		for (var i = word.IndexOf('e'); i > -1; i = word.IndexOf('e', i + 1))
			foundIndexes.Add(i);

		if (foundIndexes.Count() < 2) return null;

		var offset = 0;
		string newWord = word;
		foreach(var index in foundIndexes)
		{
			if (indicesToSkip > 0)
			{
				indicesToSkip--;
				continue;
			}

			if (index < onePosition)
				continue;

			if (index == onePosition + 2) 
				continue;

			newWord = newWord.Remove(index - offset, 1);
			offset++;

			break;
		}

		if (newWord == word)
			return null;

		return newWord;
	}

	string RepeatedLetter(string word)
	{
		//if (GetIndecesIn(word, 'd').Count != 2)
		//    return null;
		if (word.IndexOf("rd") == -1)
			return null;

		var newWord = word.Replace("rd", "r");

		return newWord; 
	}

	string Plural(string word)
	{
		//if (GetIndecesIn(word, 'd').Count != 2)
		//    return null;
		if (!word.StartsWith("re"))
			return null;

		var newWord = new string(word.Substring(1).Reverse().ToArray());
		newWord = "r" + newWord;

		return newWord;
	}

	readonly char[] vowels = new char[] {'a','e','i','o','u'};
	string Pig(string word)
	{

		if (word.Length <= 2)
			return null;
		
		var wordArr = word.ToCharArray();
		var secondIsVowel = false;
		foreach(var vowel in vowels)
		{
			if (wordArr[0] == vowel)
				return null;

			if (wordArr[1] == vowel)
				secondIsVowel = true;
		}
				
		if (secondIsVowel)
			return null;

		var newWord = word.Substring(2) + word.Substring(0, 2) + "ay";
		return newWord;
	}

	string DoubleIn(string word)
	{
		if (word.EndsWith("ing"))
			return null;

		var removed = word.Replace("in", "");
		if (word.Length - removed.Length < 4)
			return null;

		return word;
	}

	string DoubleLetter(string word)
	{
		var prevLetter = '@';
		for (var i = 0; i < word.Length; i++)
		{
			var letter = word[i];
			if (letter == prevLetter)
				return word;

			prevLetter = letter;
		}

		return null;
	}

	string DoubleLetters(string word)
	{
		var counts = new Dictionary<char, int>();
		word += "s";

		for (var i = 0; i < word.Length; i++)
		{
			var letter = word[i];

			if (!counts.ContainsKey(letter))
				counts.Add(letter, 1);
			else
				counts[letter] += 1;
		}

		foreach(var val in counts.Values)
			if(val != 3)
				return null;

		return word;
	}

	static List<int> GetIndecesIn(string word, string s)
	{
		var foundIndexes = new List<int>();
		for (var i = word.IndexOf(s); i > -1; i = word.IndexOf(s, i + 1))
			foundIndexes.Add(i);

		return foundIndexes;
	}

	static List<int> GetIndecesIn(string word, char c)
	{
		var foundIndexes = new List<int>();
		for (var i = word.IndexOf(c); i > -1; i = word.IndexOf(c, i + 1))
			foundIndexes.Add(i);

		return foundIndexes;
	}
}


[CustomEditor(typeof(WordFinder))]
public class WordFinderEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Find!"))
		{
			var finder = target as WordFinder;
			finder.Find();
		}       
	}
}
#endif

// 789190 -> ten


// stateside -> tide
// nonpatentable->optable
// patellar->par
// platelayer->payer
// platelet->pet
// satellite->site
// waterer -> we


// MAYBE
// enumerator->numerator