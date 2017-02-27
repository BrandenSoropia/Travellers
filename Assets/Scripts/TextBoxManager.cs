using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Needed to import type "Text"

public class TextBoxManager : MonoBehaviour {

	public GameObject textBox;
	public Text currentText;
	public Dictionary<string, string> characters; // Key: Name, TODO Value: ??? 
	public string currentSpeaker;
	public TextAsset textFile;
	public List<string> text;
	public int currentLine; // Has to be set manually for more freedom (I think...)
	public int endAtLine;

	// Constants
	private const int BEFORE_COLON_IDX = 0;
	private const int CHARACTER_LIST_LINE = 0;
	private const string CHARACTER_LIST_MARKER = "Characters:";

	/**	Return true if line is just the speaker's name. Otherwise return false. */
	bool isLineSpeakerNameOnly (string name, string line) {
		string[] splitLine = line.Split(':');

		return splitLine[BEFORE_COLON_IDX].Contains(name);
	}

	/** Return a dictionary with key being character names and TODO value ??? */
	Dictionary<string, string> generateCharacterDictionaryFromLine (string line) {
		string[] names = line.Substring(CHARACTER_LIST_MARKER.Length).Split(','); // Remove marker from line and separate names by comma

		Dictionary<string, string> characters = new Dictionary<string, string>(); // Initialize empty dictionary
		// Populate dictionary with names and default value
		foreach (string name in names) {
			characters.Add(name.Trim(), "");
		}

		return characters;
	}

	/** Return a copy of text without empty lines. */
	List<string> removeEmptyLinesFromText (string[] text) {
		List<string> noEmptyLineText = new List<string>();

		foreach (string line in text) {
			if (string.Compare (line, "") != 0) noEmptyLineText.Add (line); // Only add non-empty line strings
		}

		return noEmptyLineText;
	}

	// Use this for initialization
	void Start () {
		if (textFile != null) {
			text = removeEmptyLinesFromText(textFile.text.Split('\n'));
			characters = generateCharacterDictionaryFromLine (text [CHARACTER_LIST_LINE]);
		}

		if (endAtLine == 0) { // Default stop reading at last line
			endAtLine = text.Count - 1;
		}
	}

	void Update () {
		currentText.text = text[currentLine];

		if (Input.GetKeyDown (KeyCode.Return)) { // Move to next line on "Enter" press
			currentLine += 1;
		}

		if (currentLine > endAtLine) {
			textBox.SetActive(false); // Inactivate text box
		}
	}
}
