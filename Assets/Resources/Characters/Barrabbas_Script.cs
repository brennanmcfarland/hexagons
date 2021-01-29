using UnityEngine;
using System.Collections;

public class Barrabbas_Script : Actor {

	//**************STATS*********************
	public override int MAXHEALTH{get{return 7;}}
	public override int NORMALSPEED{get{return 4;}}
	public override int NORMALATTACKSTRENGTH{get{return 6;}}
	public override int NORMALDEFENSESTRENGTH{get{return 1;}}
	public override int NORMALINITIATIVE{get{return 75;}}
	//state-dependent stats inherited from actor

	public override bool isdefaultattackranged{get{return false;}}
	public override float LOSOFFSETFROMORIGIN{get{return 1.25f;}}
	public override int[][] multipletiles{get{return null;}}
	public override Hashtable abilityranges{get{return new Hashtable()
			{
				{"doubleflail",1},
				{"chargeandgore",1},
				{"bellow",0},
				{"demonicinfluence",0}
			};
		}}

	public GameObject character;


	public void spawn(int[] coords) //coords[0] = x, coords[1] = y
	{
		health = MAXHEALTH;
		speed = NORMALSPEED;
		attackstrength = NORMALATTACKSTRENGTH;
		defensestrength = NORMALDEFENSESTRENGTH;
		initiative = NORMALINITIATIVE;
		defaultability = "doubleflail";
		selectedability = defaultability;
		//character = Instantiate(Resources.Load (@"Characters/barrabbas"), this.transform.position, Quaternion.identity) as GameObject;
		gameObject.GetComponent<Renderer>().material = Resources.Load(@"Materials/characters/barrabbas_concept", typeof(Material)) as Material;
		//character.transform.parent = this.transform;

		thisrigidbody = gameObject.AddComponent<Rigidbody>(); //add the rigidbody to optimize collisions
		thisrigidbody.isKinematic = true; //turn off physics

		mousecollider = gameObject.AddComponent<CapsuleCollider>();
		mousecollider.center = new Vector3(0f,.8f,0f);
		mousecollider.radius = .4f;
		mousecollider.height = 2.0f;

		actions = new string[]{"doubleflail","chargeandgore","bellow","demonicinfluence"};

	}

	//use double flail attack
	public void doubleflail(GameObject targetactor)
	{
		object[] prms = new object[2];
		prms[0] = 6;
		prms[1] = null;
		targetactor.SendMessage("defend", prms, SendMessageOptions.RequireReceiver);
	}

	//use charge and gore attack
	public void chargeandgore(GameObject targetactor)
	{
		attackmodifiers attackmods = new attackmodifiers();
		attackmods.armorpiercing = true;
		object[] prms = new object[2];
		prms[0] = 3;
		prms[1] = attackmods;
		targetactor.SendMessage("defend", prms, SendMessageOptions.RequireReceiver);
	}

	//use bellow ability
	public void bellow(GameObject targetactor)
	{
		//temporary in order to visualize range of effect, will redo in GUI later
		GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		temp.AddComponent<tempobject>();
		temp.transform.position = new Vector3(transform.position.x, transform.position.y+LOSOFFSETFROMORIGIN, transform.position.z);
		temp.AddComponent<SphereCollider>();
		temp.transform.localScale = 5.0f*Vector3.one*2.0f;

		alllineofsight(1,5.0f); //calculate all targets in range and line of sight
		if(actorsinabilityrange.Count == 0)
			Debug.Log("no enemies in range");
		for(int i=0; i<actorsinabilityrange.Count; i++)
		{
			GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[actorsinabilityrange[i]].GetComponent<Actor>().modifier_fear = true;
		}
	}

	//use special attack demonic influence
	public void demonicinfluence(GameObject targetactor)
	{
		Debug.Log("still working on this part...");
		//will summon a demon squad to fight as normal characters
	}

	//renders test UI, for testing purposes only use Unity's full UI system for final product
	void OnGUI()
	{
		if(gameObject.GetComponent<PlayerActor>() != null && GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter] == gameObject)
		{
			if(GUI.Button(new Rect(20,40,150,20), "double flail"))
				changeability("doubleflail");
			if(GUI.Button(new Rect(20,60,150,20), "charge and gore"))
				changeability("chargeandgore");
			if(GUI.Button(new Rect(20,80,150,20), "bellow"))
				changeability("bellow");
			if(GUI.Button(new Rect(20,100,150,20), "revert to default"))
				cancelability();
			if(GUI.Button(new Rect(20,120,180,20), "special: demonic influence"))
				changeability("demonicinfluence");
		}
	}
		
	//no ranged attack, so does nothing
	public override void rangedattack(GameObject targetactor)
	{
		Debug.Log("Error: no ranged attack for " + gameObject.GetType() + "!");
	}
		
	//must override these methods of the same names in actor class
	public override float getLOSoffsetfromorigin()
	{
		return LOSOFFSETFROMORIGIN;
	}
	public override int getspeed()
	{
		return speed;
	}
	public override int[][] getmultipletiles()
	{
		return multipletiles;
	}
	//serializes the character to a representative string
	//the actual serialization to a save file is handled by serialization
	public override savedata serializecharacter()
	{
		savedata temp = new savedata();
		if(this.gameObject.GetComponent<PlayerActor>())
			temp.isplayercharacter = true;
		else
			temp.isplayercharacter = false;
		int[] tempstats = {health,speed,attackstrength,defensestrength};
		int[] tempposition = {Xindex,Yindex};
		string[] tempmodifiers = {"unmodified"};
		temp.ID = this.GetType().ToString();
		temp.stats = tempstats;
		temp.position = tempposition;
		temp.modifiers = tempmodifiers;

		if(!temp.verifydata())
			Debug.Log ("Error: Invalid save data for object " + this.GetType());
		return temp;
	}

	//deserializes the character once the class is created, instantiation is already taken care of elsewhere
	public override void deserializecharacter(savedata savestate)
	{
		if(savestate.verifydata() == false)
		{
			Debug.Log("Error loading character " + this.ToString());
			return;
		}
		health = savestate.stats[0];
		speed = savestate.stats[1];
		attackstrength = savestate.stats[2];
		defensestrength = savestate.stats[3];
		Debug.Log("Character " + this.ToString() + " successfully loaded");
	}
}
