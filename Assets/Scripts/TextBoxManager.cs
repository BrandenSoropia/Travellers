using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Needed to import type "Text"

public class TextBoxManager : MonoBehaviour {
	// UI & Game Objects
	public GameObject textBox; // The panel containing the entire text box
	public Text speakerNameCurrentText; // Where the speaker's name is displayed
	public Text dialogueCurrentText; // Where the speaker's text is displayed
	// Source File
	public TextAsset textFile; // .txt file
	// Properties
	public List<string> text; // List of all the lines in the .txt file without empty space
	public Dictionary<string, string> characters; // Key: Name, Value: string 
	public string currentSpeaker; // Name of the current speaker
	public bool didSpeakerChange; // Flag for when speakers change
	public int currentLine; // Which line to start reading from
	public int endAtLine; // Which line number to stop reading

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
		// Populate dictionary with names and set default value
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

	/** Set speaker, set flag to true and skip line if line is only speaker's name. */
	void setSpeakerAndSkipLine (string name) {
		currentSpeaker = name;
		didSpeakerChange = true;
		currentLine += 1;
	}

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
		foreach (string name in characters.Keys) { // Set speaker and skip line if line is only speaker's name
			if (isLineSpeakerNameOnly(name, text[currentLine])) {
				setSpeakerAndSkipLine(name);
			}
		}

		if (didSpeakerChange) { // Update speaker and reset flag
			speakerNameCurrentText.text = currentSpeaker;
			didSpeakerChange = false;
		}

		dialogueCurrentText.text = text[currentLine];


		if (Input.GetKeyDown (KeyCode.Return)) { // Move to next line when "Enter" is pressed
			currentLine += 1;
		}

		if (currentLine > endAtLine) {
			textBox.SetActive(false); // Inactivate text box
		}
	}
}
