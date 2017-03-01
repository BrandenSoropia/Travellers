using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

 // Needed to import type "Text"

public class TextBoxManager : MonoBehaviour
{
	// UI & Game Objects
	public GameObject speakerTextBox; // The panel containing the speaker's text box
	public Text speakerNameCurrentText; // Where the speaker's name is displayed
	public Text dialogueCurrentText;// Where the speaker's text is displayed
	// Source File
	public TextAsset textFile; // .txt file

	// Properties
	public List<string> text; // List of all the lines in the .txt file without empty space
	public Dictionary<string, Dictionary<string, string>> characters; // Key: Name, Value: Dictionary<string, string>
	public string currentSpeaker; // Name of the current speaker
	public Coroutine runningPrintDialogueCoroutine = null; // Current dialogue being printed letter by letter
	public List<List<string>> choices; // Array containing arrays of choice text and next text file

	// Property Change Flags
	public bool didSpeakerChange; // When speakers changes
	public bool didLineChange = true; // WHen line changes

	// Configs
	public int currentLine; // Which line to start reading from
	public int endAtLine; // Which line number to stop reading
	public int BEFORE_COLON_IDX = 0;
	public string CHARACTER_LIST_MARKER = "----";
	public string CHOICE_MARKER = "Choose:";
	public string NEXT_FILE_MARKER = "Next:";
	public float INNER_THOUGHT_TEXT_OPACITY = 0.75F;
	public float readSpeed = 0.1F; // Print letter speed
	public int NUM_PADDING_LINES = 2; // Number of lines after endAtLine to find choices or next file

	/**	Return true if line is just the speaker's name. Otherwise return false. */
	bool isLineSpeakerNameOnly (string name, string line)
	{
		string[] splitLine = line.Split (':');

		return splitLine [BEFORE_COLON_IDX].Contains (name);
	}

	/** Return a dictionary with key being character names and TODO value ??? */
	Dictionary<string, Dictionary<string, string>> generateCharacterDictionaryFromText ()
	{
		Dictionary<string, Dictionary<string, string>> characters = new Dictionary<string, Dictionary<string, string>> ();
		string currentCharacterDefinition = "";

		for (int lineNum = 0; lineNum < text.Count; lineNum++) {
			string line = text [lineNum];

			if (line.CompareTo (CHARACTER_LIST_MARKER) == 0) { // Stop reading once you have read through character definitions
				break;
			} else if (line.EndsWith (":")) { // Add character, set them as current character definition and prepare a dictionary for their properties
				string name = line.TrimEnd (':');

				characters.Add (name, new Dictionary<string, string> ()); 
				currentCharacterDefinition = name;
				continue;
			} else { // Add character property to its dictionary entry
				int nameIdx = 0;
				int valueIdx = 1;
				string[] property = line.Split (':');

				Dictionary<string, string> characterProperties = characters [currentCharacterDefinition];
				characterProperties.Add (property [nameIdx].Trim (), property [valueIdx].Trim ()); // Trim white space from ends
			}
		}

		return characters;
	}

	/** Return a copy of text without empty lines. */
	List<string> removeEmptyLinesFromText (string[] text)
	{
		List<string> noEmptyLinesText = new List<string> ();

		foreach (string line in text) {
			if (string.Compare (line, "") != 0)
				noEmptyLinesText.Add (line); // Only add non-empty line strings
		}

		return noEmptyLinesText;
	}

	/** Set speaker, set speaker change and line change flag to true and skip line if line is only speaker's name. */
	void setSpeakerAndSkipLine (string name)
	{
		currentSpeaker = name;
		didSpeakerChange = true;
		currentLine += 1;
		didLineChange = true;
	}

	/** Print line character by character at float readSpeed until line is finished. */
	IEnumerator printCharByChar (string line)
	{
		dialogueCurrentText.text = ""; // Clear previous text
		// Wait for some fraction of second then print letter until all letters printed
		foreach (char letter in line) {
			yield return new WaitForSeconds (readSpeed);
			dialogueCurrentText.text += letter;
		}
	}

	/** Return line without quotes and apply character's text colour. If narrator, does not trim quotes. */
	string trimAndSetColourOfLine (string line)
	{
		string[] colours = characters[currentSpeaker]["colour"].Split (','); // Get character's text colour and splits into individual numbers 
		Color speakerTextColor = new Color (float.Parse (colours [0]), float.Parse (colours [1]), float.Parse (colours [2]));

		if (line.StartsWith ("\'") && line.EndsWith ("\'")) { // Apply opacity for inner thoughts
			speakerTextColor.a = INNER_THOUGHT_TEXT_OPACITY;
		}

		dialogueCurrentText.color = speakerTextColor;

		if (currentSpeaker.CompareTo ("Narrator") == 0) { // Skip quote trimming if narrator
			return line;
		}

		return line.Trim (new char[2] { '\'', '\"' }); // Trim single and double quotes
	}

	/** Return index number of the line before choices or next file marker. Return -1 if not found. */
	int findLineNumBeforeChoiceOrNextFileMarker () {
		for (int lineNum = 0; lineNum < text.Count; lineNum++) { // Go through all the lines to find CHOICE_MARKER or NEXT_FILE_MARKER and set endAtLine to one line before
			if ((text [lineNum].CompareTo (CHOICE_MARKER) == 0) || (text [lineNum].CompareTo (NEXT_FILE_MARKER) == 0)) {
				return lineNum - 1;
			}
		}

		return -1;
	}

	/** Return array of choices. */
	List<List<string>> extractChoices () {
		int startOfChoices = endAtLine + NUM_PADDING_LINES;
		int endOfChoices = text.Count - startOfChoices;

		List<string> choicesLines = text.GetRange (startOfChoices, endOfChoices); // Get only choices from text 
		List<List<string>> choices = new List<List<string>>();

		foreach (string choiceLine in choicesLines) { // Extract choice text and next file and populate choices
			List<string> choice = new List<string>(choiceLine.Split (':')); // Convert string array to list

			choices.Add (choice);
		}

		return choices;
	}

	// Remove empty lines and initialize character dictionary
	void Start ()
	{
		if (textFile != null) {
			text = removeEmptyLinesFromText (textFile.text.Split ('\n'));
			characters = generateCharacterDictionaryFromText ();
		}

		if (endAtLine == 0) { // Default stop reading at last line
			// End at line before choice or next file
			endAtLine = findLineNumBeforeChoiceOrNextFileMarker();
		}

		// TODO create buttons for each one
		// Extract choices
		choices = extractChoices ();
	}

	void Update ()
	{
		foreach (string name in characters.Keys) { // Set speaker and skip line if line is only speaker's name
			if (isLineSpeakerNameOnly (name, text [currentLine])) {
				setSpeakerAndSkipLine (name);
			}
		}

		if (didSpeakerChange) { // Update speaker and reset flag
			speakerNameCurrentText.text = currentSpeaker;
			didSpeakerChange = false;
		}

		if (didLineChange) {
			string noQuotesLine = trimAndSetColourOfLine (text [currentLine]);

			runningPrintDialogueCoroutine = StartCoroutine (printCharByChar (noQuotesLine));
			didLineChange = false;
		}

		if (Input.GetKeyDown (KeyCode.Return) && (runningPrintDialogueCoroutine == null)) { // Move to next line when "Enter" is pressed. Cancel current printing dialogue
			currentLine += 1;
			didLineChange = true;
		}

		if (Input.GetKeyDown (KeyCode.Return) && (runningPrintDialogueCoroutine != null)) { // Move to next line when "Enter" is pressed. Cancel current printing dialogue
			StopCoroutine (runningPrintDialogueCoroutine);
			runningPrintDialogueCoroutine = null;
			dialogueCurrentText.text = trimAndSetColourOfLine (text [currentLine]);
		}

		if (currentLine > endAtLine) {
			speakerTextBox.SetActive (false); // Inactivate text box
			// TODO Display choices when reached end of file
		}
	}
}
