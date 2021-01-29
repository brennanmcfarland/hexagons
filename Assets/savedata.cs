using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct savedata {
	public string ID; //the ID (aka type or class name) of the object
	public bool isplayercharacter; //keeps track of if player character
	public int[] stats; //mutable stats
	public int[] position; //position or origin position
	public string[] modifiers; //basically anything else needing to be saved

	public bool verifydata()
	{
		if(ID == null
			||stats == null
		  	|| position == null
		   	|| modifiers == null
			)
			return false;
		else
			return true;
	}
}
