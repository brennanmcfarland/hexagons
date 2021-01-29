using UnityEngine;
using System.Collections;

public class ComputerActor : MonoBehaviour {

	public void init(int x, int y, int initialhealth, int s, int a, int d, float l)
	{
		//base.baseinit(x,y,initialhealth,s,a,d,l);
	}

	//just a stub to test AI, will be removed later
	public void AItaketurn()
	{
		Debug.Log ("AI is doing fancy stuff...");

		//determine if and where to move

		//determine what ability to use, if any, and on what target

		//and end the turn
		GameObject.FindWithTag("Player").GetComponent<ActorDriver>().cycleactivecharacter();
	}
}
