using UnityEngine;
using System.Collections;

public class Eaglewarrior_Script : Actor {

	//**************STATS*********************
	public override int MAXHEALTH{get{return 3;}}
	public override int NORMALSPEED{get{return 5;}}
	public override int NORMALATTACKSTRENGTH{get{return 4;}}
	public override int NORMALDEFENSESTRENGTH{get{return 5;}}
	public override int NORMALINITIATIVE{get{return 40;}}
	//state-dependent stats inherited from actor

	public override bool isdefaultattackranged{get{return false;}}
	public override float LOSOFFSETFROMORIGIN{get{return 1.15f;}}
	public override int[][] multipletiles{get{return null;}}
	public override Hashtable abilityranges{get{return new Hashtable()
			{
				{"sword",1},
				{"eagle",3},
				{"recon",0},
				{"divebomb",0}
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
		defaultability = "sword";
		selectedability = defaultability;
		//character = Instantiate(Resources.Load (@"Characters/barrabbas"), this.transform.position, Quaternion.identity) as GameObject;
		gameObject.GetComponent<Renderer>().material = Resources.Load(@"Materials/characters/eaglewarrior_concept", typeof(Material)) as Material;
		//character.transform.parent = this.transform;

		thisrigidbody = gameObject.AddComponent<Rigidbody>(); //add the rigidbody to optimize collisions
		thisrigidbody.isKinematic = true; //turn off physics

		mousecollider = gameObject.AddComponent<CapsuleCollider>();
		mousecollider.center = new Vector3(0f,.8f,0f);
		mousecollider.radius = .3f;
		mousecollider.height = 1.6f;

		hardacoustics = true;

		actions = new string[]{"sword","eagle","recon","divebomb"};
	}

	//use sword attack
	public void sword(GameObject targetactor)
	{
		object[] prms = new object[2];
		prms[0] = 4;
		prms[1] = null;
		targetactor.SendMessage("defend", prms, SendMessageOptions.RequireReceiver);
	}

	//use eagle attack
	public void eagle(GameObject targetactor)
	{
		object[] prms = new object[2];
		prms[0] = 2;
		prms[1] = null;
		targetactor.SendMessage("defend", prms, SendMessageOptions.RequireReceiver);
	}

	//use recon ability
	public void recon(GameObject targetactor)
	{
		Debug.Log("still working on this part...");
		//will increase movement or sight range?
	}

	//use special attack divebomb
	public void divebomb(GameObject targetactor)
	{
		Debug.Log("still working on this part...");
		//will rain eagles from sky with % chance on each tile (only enemies hurt)
	}

	//renders test UI, for testing purposes only use Unity's full UI system for final product
	void OnGUI()
	{
		if(gameObject.GetComponent<PlayerActor>() != null && GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter] == gameObject)
		{
			if(GUI.Button(new Rect(20,40,150,20), "sword"))
				changeability("sword");
			if(GUI.Button(new Rect(20,60,150,20), "eagle"))
				changeability("eagle");
			if(GUI.Button(new Rect(20,80,150,20), "recon"))
				changeability("recon");
			if(GUI.Button(new Rect(20,100,150,20), "revert to default"))
				cancelability();
			if(GUI.Button(new Rect(20,120,180,20), "special: divebomb"))
				changeability("divebomb");
		}
	}

	//ranged attack is recon
	public override void rangedattack(GameObject targetactor)
	{
		Debug.Log("Using recon");
		SendMessage("recon", targetactor);
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
