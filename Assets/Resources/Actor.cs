using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Actor : MonoBehaviour {

	//**************ABSTRACT FIELDS*********************
	public abstract int MAXHEALTH{get;} //it's only a property because it's supposed to be constant
	protected int health; //not constant stuff is just overridden, eliminates the overhead
	public abstract int NORMALSPEED{get;}
	protected int speed;
	public abstract int NORMALATTACKSTRENGTH{get;}
	protected int attackstrength;
	public abstract int NORMALDEFENSESTRENGTH{get;}
	protected int defensestrength;
	public abstract int NORMALINITIATIVE{get;}
	protected int initiative;

	public abstract bool isdefaultattackranged{get;} //is the default attack ranged or melee?
	public abstract float LOSOFFSETFROMORIGIN{get;}//LOS offset from the object's origin
	public abstract Hashtable abilityranges{get;} //contains the range in spaces of each ability (0 if not applicable)
	public abstract int[][] multipletiles{get;}//relative inidces of tiles to origin tile, null if only 1 tile, otherwise must include origin tile

	public GameObject character; //the character object itself
	protected Mesh mesh; //the character mesh itself
	public string selectedability; //the selected ability of the character
	protected string defaultability; //the default ability of the character

	//**************************************************
	//*************INHERITED FIELDS*********************

	public CapsuleCollider mousecollider;
	public Rigidbody thisrigidbody;  //necessary for optimal collision performance
	
	public int Xindex;
	public int Yindex;

	public bool hasmoved = false; //flag for if the character has moved yet in this turn
	public bool isturnover = false; //flag for if the character has used its turn
	
	protected List<int> actorsinabilityrange = null; //updated every time LOS is calculated for all

	//**************************************************
	//******************MODIFIERS***********************
	public bool modifier_fear = false;
    public int modifier_burn = 0; //rounds left for burning modifier

	//**************************************************
	//******************ABSTRACT METHODS*******************
	public abstract void rangedattack(GameObject targetactor); //does nothing if no ranged attack exists
	public abstract float getLOSoffsetfromorigin();
	public abstract int getspeed();
	public abstract int[][] getmultipletiles();
	public abstract savedata serializecharacter();
	public abstract void deserializecharacter(savedata savestate);
	//*****************************************************

	protected bool hardacoustics = false; //true if the acoustics are hard, false if the acoustics are soft
	protected AudioSource audiosource;
	protected AudioClip hardhit;  //sound to play for a hit on something hard
	protected AudioClip softhit; //sound to play for a hit on something soft

	protected string[] actions = null; //holds a list of methods that show up in GUI if a player actor or can be called by AI if a computer actor


	// Use this for initialization
	public void actorinit(int x, int y)
	{
		Xindex = x;
		Yindex = y;
		Debug.Log ("spawning character at: " + x + "," + y);
		transform.position = new Vector3((float)(x+y*.5),GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x,y].GetComponent<Tile>().getheight(false),(float)(y*1.5/Mathf.Sqrt(3)));
		GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap[x,y].GetComponent<Tile>().changeifobstacle(true); //marks the tile the player is on as an obstacle so nothing else can be on it
		int[] temp = {x,y};
		SendMessage("spawn",temp, SendMessageOptions.DontRequireReceiver);

		//to visualize LOS origin, for testing purposes only
		GameObject LOSorigin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		LOSorigin.transform.localScale = new Vector3(.3f,.3f,.3f);
		LOSorigin.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y+LOSOFFSETFROMORIGIN, gameObject.transform.position.z);
		LOSorigin.transform.SetParent(gameObject.transform);

		audiosource = gameObject.AddComponent<AudioSource>();
		audiosource.playOnAwake = false;
		updatesoundeffects(defaultability);
	}

	//updates the possible sound effects when an ability is switched
	public void updatesoundeffects(string effectname)
	{
		hardhit = Resources.Load(@"Sounds/CharacterSounds/"+effectname+"_hard", typeof(AudioClip)) as AudioClip;
		softhit = Resources.Load(@"Sounds/CharacterSounds/"+effectname+"_soft", typeof(AudioClip)) as AudioClip;
		Debug.Log("hard sound is now " + hardhit);
		Debug.Log("soft sound is now " + softhit);
	}

	public void setwaypoint(int x, int y) //should parts of this be moved to voxeldriver?
	{
		Debug.Log("setting waypoint");
		//Debug.Log (Xindex + "," + Yindex);
		hasmoved = true;
		GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap[x,y].GetComponent<Tile>().changeifobstacle(true); //marks the tile the player moving to as an obstacle so nothing else can be on it
		GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap[Xindex,Yindex].GetComponent<Tile>().changeifobstacle(false); //marks the previously on tile as not an obstacle
		Xindex = x;
		Yindex = y;
	}

	//default attack method, can be overridden
	public virtual void attack(GameObject targetactor)
	{
		Debug.Log("Using " + selectedability + " on " + targetactor);

		//play the appropriate audio clip
		if(targetactor.GetComponent<Actor>().hardacoustics == true)
			audiosource.PlayOneShot(hardhit);
		else
			audiosource.PlayOneShot(softhit);

		SendMessage(selectedability, targetactor, SendMessageOptions.RequireReceiver);
		isturnover = true;
		selectedability = defaultability;
		GameObject.FindWithTag("Player").GetComponent<ActorDriver>().cycleactivecharacter();
	}

	public virtual void defend(object[] prms) //default defense method, can be overridden
	{
		Debug.Log("Defending");
		int attackstrength = (int)prms[0];
		attackmodifiers attackmods = (attackmodifiers)prms[1];

		int instanceattackstrength = (int)(Random.value*attackstrength); //returns a random # between 0 and attack strength, inclusive
		int instancedefensestrength = (int)(Random.value*defensestrength); //returns a random # between 0 and defense strength, inclusive
		int totaldamage = instanceattackstrength-instancedefensestrength;

		//check and adjust for modifiers
		if(attackmods != null)
		{
            if(attackmods.unblockable == true)
            {
                totaldamage = instanceattackstrength;
                Debug.Log("unblockable damage!");
            }
			else if(attackmods.armorpiercing == true) //halves the effective defense
			{
				totaldamage += instancedefensestrength/2;
				Debug.Log("armor peircing halves effective defense!");
			}

            if(attackmods.burning == true)
            {
                modifier_burn = 3;
            }
		}
		Debug.Log("base attack " + instanceattackstrength + ", base defense " + instancedefensestrength + ", total damage " + totaldamage + ", health is now " + health);
		if(totaldamage > 0)
		{
			health -= totaldamage;
			if(health <=0)
			{
				health = 0;
				die ();
			}
		}
	}

	public void changeability(string newability) //change the selected ability to something other than the default
	{
		Debug.Log("changing ability");
		GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate = gamestates.ability_selected;
		selectedability = newability;
		GameObject.FindWithTag("Player").GetComponent<ActorDriver>().abilityselected = true;
		updatesoundeffects(newability);
		Debug.Log("selected ability is now " + selectedability);
	}

	public void cancelability() //revert to the default ability and de-select
	{
		Debug.Log("canceling ability");
		GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate = gamestates.idle;
		selectedability = defaultability;
		GameObject.FindWithTag("Player").GetComponent<ActorDriver>().abilityselected = false;
		Debug.Log("reverted back to default ability: " + selectedability);
	}

	public void newround() //resets for a new turn sequence
	{
		Debug.Log("called newround");
		hasmoved = false;
		isturnover = false;
        applyroundbasedmodifiers();
	}

	//kill the current actor
	public void die()
	{
        remove();
	}

    //remove the current actor from the game
    public void remove()
    {
        GameObject.FindWithTag("tiles").GetComponent<VoxelDriver>().someonedied = true;
        Debug.Log("The character is dead.");
        for (int i = 0; i < GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Count; i++)
        {
            //find this actor's index and delete it from the list
            if (GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i].GetInstanceID() == this.gameObject.GetInstanceID())
            {
                GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.RemoveAt(i);
                break;
            }
        }
        GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[Xindex, Yindex].GetComponent<Tile>().changeifobstacle(false);
        Debug.Log(Xindex + "," + Yindex + GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[Xindex, Yindex] + ": " + GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[Xindex, Yindex].GetComponent<Tile>().getifobstacle());

        //cycle the active player character if this is the active player character dying
        if (GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter] == this.gameObject)
            GameObject.FindWithTag("Player").GetComponent<ActorDriver>().cycleactivecharacter();
        Destroy(this.gameObject);
    }

	//
	public void attackmove()
	{
		if(isdefaultattackranged)
		{
			rangedattack();
		}
		else
		{

		}
	}

	//
	public void rangedattack()
	{

	}

	//determines line of sight for 0: all actors 1: all enemy actors 2: all player actors
	public void alllineofsight(int whichactors, float abilityrange)
	{
		Debug.Log("alllineofsight");
		actorsinabilityrange = new List<int>();
		
		if(whichactors == 0)
		{
			for(int i=0; i<GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Count; i++)
			{
				if(lineofsight(GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i], abilityrange) == true)
					actorsinabilityrange.Add (i);
			}
		}
		else if(whichactors == 1)
		{
			for(int i=0; i<GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Count; i++)
			{
				if(GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i].GetComponent<ComputerActor>() != null)
				{
					if(lineofsight(GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i], abilityrange) == true)
					{
						actorsinabilityrange.Add (i);
					}
				}
			}
		}
		else if(whichactors == 2)
		{
			for(int i=0; i<GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Count; i++)
			{
				if(GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i].GetComponent<PlayerActor>() != null)
				{
					if(lineofsight(GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i], abilityrange) == true)
						actorsinabilityrange.Add (i);				
				}
			}
		}
		else
		{
			Debug.Log ("Error: invalid parameters for LOS calculations");
		}
	}

	//determines line of sight for a single particular actor, returns if in LOS or not
	//abilityrange is the max distance that the ability can reach
	//note that targetactor MUST have an actor component or it will crash the game
	public bool lineofsight(GameObject targetactor, float abilityrange)
	{
		Debug.Log(Vector3.Distance(new Vector3(transform.position.x,transform.position.y+getLOSoffsetfromorigin(), transform.position.z),new Vector3(targetactor.transform.position.x, targetactor.transform.position.y+targetactor.GetComponent<Actor>().getLOSoffsetfromorigin(), targetactor.transform.position.z)-new Vector3(transform.position.x,transform.position.y+getLOSoffsetfromorigin(), transform.position.z)));
		RaycastHit targethit;
		//if the raycast hits something (the other actor is in range or there's an obstacle)
		if(Physics.Raycast(new Vector3(transform.position.x,transform.position.y+getLOSoffsetfromorigin(), transform.position.z), new Vector3(targetactor.transform.position.x, targetactor.transform.position.y+targetactor.GetComponent<Actor>().getLOSoffsetfromorigin(), targetactor.transform.position.z)-new Vector3(transform.position.x,transform.position.y+getLOSoffsetfromorigin(), transform.position.z), out targethit, abilityrange) == true)
		{
			//if the raycast hit the other actor and not an obstacle
			if(targethit.transform.gameObject == targetactor)
			{
				return true;
			}
		}
		//if either test fails, return false
		return false;
		
	}

    public void applyroundbasedmodifiers()
    {
        if(modifier_burn > 0)
        {
            modifier_burn--;
            object[] prms = new object[2];
            prms[0] = 1;
            prms[1] = new attackmodifiers();
            ((attackmodifiers)prms[1]).unblockable = true;
            defend(prms);
        }
    }

}
