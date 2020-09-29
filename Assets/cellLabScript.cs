using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class cellLabScript : MonoBehaviour {

	//publics
	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMSelectable[] Checkboxes;
	public KMSelectable[] Buttons;
	public TextMesh[] Text;
	public KMBombModule Module;

	//animation
	public MeshRenderer Backing;
	public GameObject[] CellBits;
	public Renderer[] StuffToHide;

	//system
	private string speciesName;
	private string sciCode;

	//user
	private bool[][] isChecked = new bool[0][];
	private int[][] cellProperties = new int[0][];
	private int[][] cellAnswers = new int[0][];
	private int currentCell;

	//logging
	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	void Awake()
	{
		_moduleID = _moduleIdCounter++;
		for (int i = 0; i < Checkboxes.Length; i++)
		{
			int x = i;
			Checkboxes[i].OnInteract += delegate ()
			{
				Buttons[x].AddInteractionPunch(.5f);
				Audio.PlaySoundAtTransform("Blip", Module.transform);
				isChecked[currentCell][x] = !isChecked[currentCell][x];
				if (isChecked[currentCell][x])
					Checkboxes[x].GetComponent<MeshRenderer>().material.color = new Color32(255, 199, 63, 255);
				else
					Checkboxes[x].GetComponent<MeshRenderer>().material.color = new Color32(31, 31, 31, 255);
				CheckSolve();
				return false;
			};
			Checkboxes[i].OnHighlight += delegate ()
			{
				if (isChecked[currentCell][x])
					Checkboxes[x].GetComponent<MeshRenderer>().material.color = new Color32(255, 199, 63, 255);
				else
					Checkboxes[x].GetComponent<MeshRenderer>().material.color = new Color32(31, 31, 31, 255);
			};
			Checkboxes[i].OnHighlightEnded += delegate ()
			{
				if (isChecked[currentCell][x])
					Checkboxes[x].GetComponent<MeshRenderer>().material.color = new Color32(255, 159, 0, 255);
				else
					Checkboxes[x].GetComponent<MeshRenderer>().material.color = new Color32(0, 0, 0, 255);
			};
		}
		for (int i = 0; i < Buttons.Length; i++)
		{
			int x = i;
			Buttons[i].OnInteract += delegate ()
			{
				Buttons[x].AddInteractionPunch(.5f);
				Audio.PlaySoundAtTransform("Blip", Module.transform);
				if (x == 0)
					currentCell = (currentCell + 1) % cellProperties.Length;
				else
				{
					cellProperties[currentCell][x - 1] = (cellProperties[currentCell][x - 1] + 1) % 24;
					if (x == 1)
						cellProperties[currentCell][x - 1] %= 16;
					if (x == 5)
						cellProperties[currentCell][x - 1] %= 6;
				}
				UpdateModule();
				CheckSolve();
				return false;
			};
			Buttons[i].OnHighlight += delegate ()
			{
				Buttons[x].GetComponent<MeshRenderer>().material.color = new Color32(39, 41, 39, 255);
			};
			Buttons[i].OnHighlightEnded += delegate ()
			{
				Buttons[x].GetComponent<MeshRenderer>().material.color = new Color32(23, 25, 23, 255);
			};
		}
	}

	void Start () {
		speciesName = NameGen();
		sciCode = CodeGen();
		Debug.LogFormat("[Cell Lab #{0}] The species name is {1}. Its database identifier is {2}.", _moduleID, speciesName[0].ToString().ToUpperInvariant() + speciesName.Substring(1, speciesName.Length - 1), sciCode);
		Text[1].text = "ID: " + sciCode + "       Name: " + (speciesName[0].ToString().ToUpperInvariant() + speciesName.Substring(1, speciesName.Length - 1));
		for (int i = 0; i < 5; i++)
			PropertiseCell();
		RandomiseReferences();
		UpdateModule();
	}

	private string NameGen()
	{
		bool vowelprev = (Rnd.Range(0, 2) == 1);
		string vowels = "aeiou";
		string consonants = "bdfghklmnprstvwz";
		int length = Rnd.Range(3, 8);
		string name = String.Empty;
		while (length > name.Length)
		{
			if (vowelprev)
			{
				name += consonants[Rnd.Range(0, consonants.Length)];
				if (Rnd.Range(0, 3) == 0 && name.Length > 1 && length > name.Length + 2)
				{
					name += consonants[Rnd.Range(0, consonants.Length)];
				}
				vowelprev = false;
			}
			else
			{
				name += vowels[Rnd.Range(0, vowels.Length)];
				if (Rnd.Range(0, 3) == 0 && length > name.Length)
				{
					name += vowels[Rnd.Range(0, vowels.Length)];
				}
				vowelprev = true;
			}
		}
		return name;
	}

	private string CodeGen()
	{
		string code = String.Empty;
		string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		for (int i = 0; i < 2; i++)
		{
			code += letters[Rnd.Range(0, letters.Length)];
		}
		code += Rnd.Range(0, 10).ToString();
		return code;
	}

	private void PropertiseCell()
	{
		//splitmass, angle, child1angle, child2angle, type, child1, child2
		cellProperties = cellProperties.Concat(new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, Rnd.Range(0, 256), Rnd.Range(0, 256), Rnd.Range(0, 256) } }).ToArray();
		isChecked = isChecked.Concat(new bool[][] { new bool[] { false, true, true } }).ToArray();
		int h = 0, s = 0, v = 0;
		v = cellProperties[cellProperties.Length - 1].Skip(7).Max() * 100 / 255;
		if (v != 0)
		{
			s = (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1].Skip(7).Min()) * 100 / cellProperties[cellProperties.Length - 1].Skip(7).Max();
			if (s != 0)
			{
				if (cellProperties[cellProperties.Length - 1].Skip(7).Max() == cellProperties[cellProperties.Length - 1][7])
				{
					if (cellProperties[cellProperties.Length - 1].Skip(7).Min() == cellProperties[cellProperties.Length - 1][9])
					{
						//0 - 60
						h = (cellProperties[cellProperties.Length - 1][8] - cellProperties[cellProperties.Length - 1].Skip(7).Min()) * 60 / (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1].Skip(7).Min());
					}
					else
					{
						//300 - 0
						h = 300 + (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1][9]) * 60 / (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1].Skip(7).Min());
					}
				}
				else if (cellProperties[cellProperties.Length - 1].Skip(7).Max() == cellProperties[cellProperties.Length - 1][8])
				{
					if (cellProperties[cellProperties.Length - 1].Skip(7).Min() == cellProperties[cellProperties.Length - 1][9])
					{
						//60 - 120
						h = 60 + (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1][7]) * 60 / (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1].Skip(7).Min());
					}
					else
					{
						//120 - 180
						h = 120 + (cellProperties[cellProperties.Length - 1][9] - cellProperties[cellProperties.Length - 1].Skip(7).Min()) * 60 / (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1].Skip(7).Min());
					}
				}
				else
				{
					if (cellProperties[cellProperties.Length - 1].Skip(7).Min() == cellProperties[cellProperties.Length - 1][8])
					{
						//240 - 300
						h = 240 + (cellProperties[cellProperties.Length - 1][7] - cellProperties[cellProperties.Length - 1].Skip(7).Min()) * 60 / (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1].Skip(7).Min());
					}
					else
					{
						//180 - 240
						h = 180 + (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1][8]) * 60 / (cellProperties[cellProperties.Length - 1].Skip(7).Max() - cellProperties[cellProperties.Length - 1].Skip(7).Min());
					}
				}
			}
		}
		Debug.LogFormat("[Cell Lab #{0}] M{1} has RGB values {2}, resulting in HSV values {3}.", _moduleID, cellProperties.Length, cellProperties[cellProperties.Length - 1].Skip(7).Join(", "), new int[] { h, s, v }.Join(", "));
		if (v < 5 || s < 5)
		{
			cellAnswers = cellAnswers.Concat(new int[][] { new int[] { 15, -1, -1, -1, -1, 0 } }).ToArray();
			Debug.LogFormat("[Cell Lab #{0}] M{1} will never split.", _moduleID, cellProperties.Length, cellProperties[cellProperties.Length - 1].Skip(7).Join(", "), new int[] { h, s, v }.Join(", "));
		}
		else
		{
			cellAnswers = cellAnswers.Concat(new int[][] { new int[] { h % 15, h / 15, (s - 5) % 24, (v - 5) % 24, 4 * ((v - 5) / 48) + 2 * ((s - 5) / 24 % 2) + ((v - 5) / 24 % 2), 0 } }).ToArray();
			string[] masses = { "0,00ng", "0,22ng", "0,44ng", "0,66ng", "0,88ng", "1,10ng", "1,32ng", "1,54ng", "1,76ng", "1,98ng", "2,20ng", "2,42ng", "2,64ng", "2,86ng", "3,08ng", "never" };
			string[] sentences = { "All boxes should be unchecked", "Only the child 2 box should be checked", "Only the child 1 box should be checked", "Both child boxes should be checked", "Only the parent box should be checked", "Only the child 1 box should be unchecked", "Only the child 2 box should be unchecked", "All boxes should be checked" };
			Debug.LogFormat("[Cell Lab #{0}] M{1} will split when its mass is {2} at an angle of {3}°. Its children will be relatively angled at {4}. {5}.", _moduleID, cellAnswers.Length, masses[cellAnswers[cellAnswers.Length - 1][0]], cellAnswers[cellAnswers.Length - 1][1] * 15, cellAnswers[cellAnswers.Length - 1].Skip(2).Take(2).Select(x => (x * 15).ToString() + "°").Join(" and "), sentences[cellAnswers[cellAnswers.Length - 1][4]]);
		}
	}

	private void RandomiseReferences()
	{
		for (int i = 0; i < cellProperties.Length; i++)
		{
			cellProperties[i][5] = Rnd.Range(0, cellProperties.Length);
			cellProperties[i][6] = Rnd.Range(0, cellProperties.Length);
			cellAnswers[cellProperties[i][5]][5]++;
			if (cellProperties[i][5] != cellProperties[i][6])
				cellAnswers[cellProperties[i][6]][5]++;
			Debug.LogFormat("[Cell Lab #{0}] M{1} will split into {2}.", _moduleID, i + 1, cellProperties[i].Skip(5).Take(2).Select(x => "M" + (x + 1).ToString()).Join(" and "));
		}
		int[] conversion = { 4, 5, 1, 0, 2, 3 };
		string[] modes = { "phagocyte", "flagellocyte", "photocyte", "devorocyte", "lipocyte", "keratinocyte" };
		for (int i = 0; i < cellProperties.Length; i++)
		{
			cellAnswers[i][5] = conversion[cellAnswers[i][5]];
			Debug.LogFormat("[Cell Lab #{0}] M{1} should be a {2}.", _moduleID, i + 1, modes[cellAnswers[i][5]]);
		}
	}

	private void UpdateModule()
	{
		string[] modes = { "Pg", "Fl", "Pt", "Dv", "Lp", "Kt" };
		string[] masses = { "0,00ng", "0,22ng", "0,44ng", "0,66ng", "0,88ng", "1,10ng", "1,32ng", "1,54ng", "1,76ng", "1,98ng", "2,20ng", "2,42ng", "2,64ng", "2,86ng", "3,08ng", "never" };
		Text[0].text = "Edit mode: <color=#" + Hexify(cellProperties[currentCell][7], cellProperties[currentCell][8], cellProperties[currentCell][9]) + ">M" + (currentCell + 1) + " " + modes[cellProperties[currentCell][4]] + "</color>\nMake adhesin:\nChild 1:\n Mode: <color=#" + Hexify(cellProperties[cellProperties[currentCell][5]][7], cellProperties[cellProperties[currentCell][5]][8], cellProperties[cellProperties[currentCell][5]][9]) + ">M" + (cellProperties[currentCell][5] + 1) + "</color> Keep adhesin:\nChild 2:\n Mode: <color=#" + Hexify(cellProperties[cellProperties[currentCell][6]][7], cellProperties[cellProperties[currentCell][6]][8], cellProperties[cellProperties[currentCell][6]][9]) + ">M" + (cellProperties[currentCell][6] + 1) + "</color> Keep adhesin:\n\nSplit mass:    " + masses[cellProperties[currentCell][0]] + "\nSplit angle:   " + (cellProperties[currentCell][1] * 15) + "°\nChild 1 angle: " + (cellProperties[currentCell][2] * 15) + "°\nChild 2 angle: " + (cellProperties[currentCell][3] * 15) + "°\nRed color:     " + cellProperties[currentCell][7] + "\nGreen color:   " + cellProperties[currentCell][8] + "\nBlue color:    " + cellProperties[currentCell][9];
		for (int i = 0; i < Checkboxes.Length; i++)
		{
			if (isChecked[currentCell][i])
				Checkboxes[i].GetComponent<MeshRenderer>().material.color = new Color32(255, 159, 0, 255);
			else
				Checkboxes[i].GetComponent<MeshRenderer>().material.color = new Color32(0, 0, 0, 255);
		}
	}

	private string Hexify(int R, int G, int B)
	{
		string chars = "0123456789abcdef";
		return chars[R / 16].ToString() + chars[R % 16].ToString() + chars[G / 16].ToString() + chars[G % 16].ToString() + chars[B / 16].ToString() + chars[B % 16].ToString();
	}

	private void CheckSolve()
	{
		bool[][] bools = new bool[][] { new bool[] { false, false, false }, new bool[] { false, false, true }, new bool[] { false, true, false }, new bool[] { false, true, true }, new bool[] { true, false, false }, new bool[] { true, false, true }, new bool[] { true, true, false }, new bool[] { true, true, true } };
		bool solve = true;
		for (int i = 0; i < cellProperties.Length; i++)
		{
			if (cellAnswers[i][0] != 15 || cellProperties[i][0] != 15)
			{
				for (int j = 0; j < 4; j++)
				{
					if (cellAnswers[i][j] != cellProperties[i][j])
					{
						solve = false;
					}
				}
				for (int j = 0; j < 3; j++)
				{
					if (bools[cellAnswers[i][4]][j] != isChecked[i][j])
					{
						solve = false;
					}
				}
			}
			if (cellAnswers[i][5] != cellProperties[i][4])
			{
				solve = false;
			}
		}
		if (solve)
		{
			Module.HandlePass();
			StartCoroutine(AnimateSolve());
		}
	}

	private IEnumerator AnimateSolve()
	{
		yield return null;
		foreach (var item in StuffToHide)
		{
			item.enabled = false;
		}
		Backing.material.color = new Color32(191, 191, 223, 255);

		CellBits[7].GetComponent<MeshRenderer>().enabled = true;
		CellBits[8].GetComponent<MeshRenderer>().enabled = true;
		CellBits[7].GetComponent<MeshRenderer>().material.color = new Color(cellProperties[0][7] / 255f, cellProperties[0][8] / 255f, cellProperties[0][9] / 255f);
		CellBits[8].GetComponent<MeshRenderer>().material.color = new Color(cellProperties[0][7] / 510f, cellProperties[0][8] / 510f, cellProperties[0][9] / 510f);
		switch (cellProperties[0][4])
		{
			case 1:
				CellBits[4].GetComponent<MeshRenderer>().enabled = true;
				CellBits[4].GetComponent<MeshRenderer>().material.color = new Color(cellProperties[0][7] / 510f, cellProperties[0][8] / 510f, cellProperties[0][9] / 510f);
				while (true)
				{
					CellBits[4].transform.localEulerAngles += new Vector3(0, 0, -10f);
					yield return null;
				}
			case 2:
				CellBits[1].GetComponent<MeshRenderer>().enabled = true;
				break;
			case 3:
				CellBits[2].GetComponent<MeshRenderer>().enabled = true;
				CellBits[2].GetComponent<MeshRenderer>().material.color = new Color(cellProperties[0][7] / 510f, cellProperties[0][8] / 510f, cellProperties[0][9] / 510f);
				break;
			case 4:
				CellBits[6].GetComponent<MeshRenderer>().enabled = true;
				CellBits[6].GetComponent<MeshRenderer>().material.color = new Color(cellProperties[0][7] / 255f, cellProperties[0][8] / 255f, cellProperties[0][9] / 255f);
				break;
			case 5:
				CellBits[5].GetComponent<MeshRenderer>().enabled = true;
				CellBits[5].GetComponent<MeshRenderer>().material.color = new Color(cellProperties[0][7] / 510f, cellProperties[0][8] / 510f, cellProperties[0][9] / 510f);
				break;
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} goto M1' to move to M1. '!{0} set splitmass 0,22ng' to set the current cells split mass to 0,22ng. Valid parameters for 'set' are splitmass, splitangle, child1angle, child2angle, type, makeadhesin, child1adhesin, child2adhesin. Type must be abbreviated, boxes must be checked/unchecked by setting them true/false. Commands can be chained using a semicolon.";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		string[] cmds = command.Split(';');
		string[] validbasecmds = { "goto", "set" };
		string[] validgotocmds = { "m1", "m2", "m3", "m4", "m5" };
		string[] validsetcmds = { "splitmass", "splitangle", "child1angle", "child2angle", "type", "makeadhesin", "child1adhesin", "child2adhesin" };
		string[] validmasscmds = { "0,00ng", "0,22ng", "0,44ng", "0,66ng", "0,88ng", "1,10ng", "1,32ng", "1,54ng", "1,76ng", "1,98ng", "2,20ng", "2,42ng", "2,64ng", "2,86ng", "3,08ng", "never" };
		string[] validanglecmds = { "0", "15", "30", "45", "60", "75", "90", "105", "120", "135", "150", "165", "180", "195", "210", "225", "240", "255", "270", "285", "300", "315", "330", "345", };
		string[] validtypes = { "pg", "fl", "pt", "dv", "lp", "kt" };
		string[] bools = { "true", "false" };
		for (int i = 0; i < cmds.Length; i++)
		{
			if (cmds[i].Split(' ').Length < 2 || !validbasecmds.Contains(cmds[i].Split(' ')[0]))
			{
				yield return "sendtochaterror Invalid command.";
				yield break;
			}
			if (cmds[i].Split(' ')[0] == "set")
			{
				if (cmds[i].Split(' ').Length != 3 || !validsetcmds.Contains(cmds[i].Split(' ')[1]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
				switch (cmds[i].Split(' ')[1])
				{
					case "splitmass":
						if (!validmasscmds.Contains(cmds[i].Split(' ')[2]))
						{
							yield return "sendtochaterror Invalid command.";
							yield break;
						}
						break;
					case "splitangle":
						if (!validanglecmds.Contains(cmds[i].Split(' ')[2]))
						{
							yield return "sendtochaterror Invalid command.";
							yield break;
						}
						break;
					case "child1angle":
						if (!validanglecmds.Contains(cmds[i].Split(' ')[2]))
						{
							yield return "sendtochaterror Invalid command.";
							yield break;
						}
						break;
					case "child2angle":
						if (!validanglecmds.Contains(cmds[i].Split(' ')[2]))
						{
							yield return "sendtochaterror Invalid command.";
							yield break;
						}
						break;
					case "type":
						if (!validtypes.Contains(cmds[i].Split(' ')[2]))
						{
							yield return "sendtochaterror Invalid command.";
							yield break;
						}
						break;
					case "makeadhesin":
						if (!bools.Contains(cmds[i].Split(' ')[2]))
						{
							yield return "sendtochaterror Invalid command.";
							yield break;
						}
						break;
					case "child1adhesin":
						if (!bools.Contains(cmds[i].Split(' ')[2]))
						{
							yield return "sendtochaterror Invalid command.";
							yield break;
						}
						break;
					case "child2adhesin":
						if (!bools.Contains(cmds[i].Split(' ')[2]))
						{
							yield return "sendtochaterror Invalid command.";
							yield break;
						}
						break;
				}
			}
			else
			{
				if (cmds[i].Split(' ').Length != 2 || !validgotocmds.Contains(cmds[i].Split(' ')[1]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
			}
		}
		for (int i = 0; i < cmds.Length; i++)
		{
			if (cmds[i].Split(' ')[0] == "set")
			{
				switch (cmds[i].Split(' ')[1])
				{
					case "splitmass":
						while (validmasscmds[cellProperties[currentCell][0]] != cmds[i].Split(' ')[2])
						{
							Buttons[1].OnInteract();
							yield return null;
						}
						break;
					case "splitangle":
						while (validanglecmds[cellProperties[currentCell][1]] != cmds[i].Split(' ')[2])
						{
							Buttons[2].OnInteract();
							yield return null;
						}
						break;
					case "child1angle":
						while (validanglecmds[cellProperties[currentCell][2]] != cmds[i].Split(' ')[2])
						{
							Buttons[3].OnInteract();
							yield return null;
						}
						break;
					case "child2angle":
						while (validanglecmds[cellProperties[currentCell][3]] != cmds[i].Split(' ')[2])
						{
							Buttons[4].OnInteract();
							yield return null;
						}
						break;
					case "type":
						while (validtypes[cellProperties[currentCell][4]] != cmds[i].Split(' ')[2])
						{
							Buttons[5].OnInteract();
							yield return null;
						}
						break;
					case "makeadhesin":
						if (isChecked[currentCell][0] != (cmds[i].Split(' ')[2] == "true"))
						{
							Checkboxes[0].OnInteract();
						}
						break;
					case "child1adhesin":
						if (isChecked[currentCell][1] != (cmds[i].Split(' ')[2] == "true"))
						{
							Checkboxes[1].OnInteract();
						}
						break;
					case "child2adhesin":
						if (isChecked[currentCell][2] != (cmds[i].Split(' ')[2] == "true"))
						{
							Checkboxes[2].OnInteract();
						}
						break;
				}
			}
			else
			{
				while (validgotocmds[currentCell] != cmds[i].Split(' ')[1])
				{
					Buttons[0].OnInteract();
					yield return null;
				}
			}
		}
		yield return null;
	}
}
