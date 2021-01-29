using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorDriver : MonoBehaviour {
	public List<GameObject> characters = new List<GameObject>(); //must never be sparse or it will mess up cycleactivecharacter
	public int activecharacter = 0;
	const float WAYPOINTDEVIATION = .1f;
	float speed = 1.0f;
	List<int[]> pathlist = null; //list holding x and y values of a path, used in findpath, null if no path found, must be updated from voxeldriver
	int pathlistindex = 1; //index for the pathlist
	Vector3 waypoint;
	Vector3 intermediatewaypoint; //waypoint for each tile on the way to the ultimate waypoint, saves CPU time fetching it over and over again
	public bool abilityselected = false; //signals whether or not the user has selected an ability to use
	GameObject targetactor = null; //holds on to the target actor while moving before attacking


	// Use this for initialization
	void Start () {
        spawnmenucharacters();
        //spawncharacter("eaglewarrior",5,5,true);
		//spawncharacter("firegolem",5,6,true);
		//spawncharacter("stonegolem",9,9,false);
		//spawncharacter("eaglewarrior",8,9,false);
		//Debug.Log(characters[0]);Debug.Log(characters[1]);Debug.Log(characters[2]);
		//set the camera's starting pivot point
		GameObject.FindWithTag("location").GetComponent<LocationDriver>().setpivot(characters[activecharacter].transform.position);
	}
	public void spawncharacter(string go, int x, int y, bool isplayercharacter)
	{
		GameObject gocharacter = Instantiate(Resources.Load (@"Characters/"+go),Vector3.zero, Quaternion.identity) as GameObject;
        Debug.Log(GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x, y].GetComponent<Tile>());
        Debug.Log(gocharacter);
		gocharacter.transform.position = new Vector3((float)(x+y*.5),GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x,y].GetComponent<Tile>().getheight(false),(float)(y*1.5/Mathf.Sqrt(3)));
		go = char.ToUpper(go[0])+go.Substring(1); //convert the first letter to uppercase to preserve file organization
		System.Type componentType = System.Type.GetType(go+"_Script", false, false);
		gocharacter.AddComponent(componentType);
		if(isplayercharacter == true)
			gocharacter.AddComponent<PlayerActor>();
		else
			gocharacter.AddComponent<ComputerActor>();
		gocharacter.transform.localScale = Vector3.one;
		gocharacter.transform.parent = GameObject.FindWithTag("Player").transform;
        gocharacter.GetComponent<Actor>().Xindex = x;
        gocharacter.GetComponent<Actor>().Yindex = y;
		characters.Add (gocharacter);
		characters[characters.Count-1].GetComponent<Actor>().actorinit(x,y);

        GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x, y].GetComponent<Tile>().changeifobstacle(true);

        //since the character is added to the end of the list, move it towards the beginning until it
        //is in the proper spot as determined by initiative or it's in front
        int spawnedcharacterindex = characters.Count-1;
		GameObject temp; //holds character while swapping
		while(spawnedcharacterindex != 0 &&
			characters[spawnedcharacterindex].GetComponent<Actor>().NORMALINITIATIVE < characters[spawnedcharacterindex-1].GetComponent<Actor>().NORMALINITIATIVE)
		{
			temp = characters[spawnedcharacterindex];
			characters[spawnedcharacterindex] = characters[spawnedcharacterindex-1];
			characters[spawnedcharacterindex-1] = temp;
			Debug.Log("HI" + go);
			spawnedcharacterindex--;
		}
	}

    public void spawnmenucharacters()
    {
        //spawn player characters
        for (int i = 0; i < SceneLoader.player_barrabbascount; i++)
            spawnmenuplayercharacter("barrabbas");
        for (int i = 0; i < SceneLoader.player_eaglewarriorcount; i++)
            spawnmenuplayercharacter("eaglewarrior");
        for (int i = 0; i < SceneLoader.player_firegolemcount; i++)
            spawnmenuplayercharacter("firegolem");
        for (int i = 0; i < SceneLoader.player_stonegolemcount; i++)
            spawnmenuplayercharacter("stonegolem");

        //spawn computer characters
        for (int i = 0; i < SceneLoader.computer_barrabbascount; i++)
            spawnmenucomputercharacter("barrabbas");
        for (int i = 0; i < SceneLoader.computer_eaglewarriorcount; i++)
            spawnmenucomputercharacter("eaglewarrior");
        for (int i = 0; i < SceneLoader.computer_firegolemcount; i++)
            spawnmenucomputercharacter("firegolem");
        for (int i = 0; i < SceneLoader.computer_stonegolemcount; i++)
            spawnmenucomputercharacter("stonegolem");
    }

    public void spawnmenuplayercharacter(string go)
    {
        int x = 0;
        int y = 0;
        bool validSpawnPoint = false;

        while(validSpawnPoint == false)
        {
            Debug.Log(x + "," + y + "is obstacle: " + GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x, y].GetComponent<Tile>().getifobstacle());
            if (GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x, y].GetComponent<Tile>() != null
                && GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x, y].GetComponent<Tile>().getifobstacle() == false)
            {
                validSpawnPoint = true;
                break;
            }

            if (x >= GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth)
            {
                x = -1;
                y++;
            }
            x++;
        }

        Debug.Log("spawning at " + x + "," + y);
        spawncharacter(go, x, y, true);
        GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x, y].GetComponent<Tile>().changeifobstacle(true);
    }

    public void spawnmenucomputercharacter(string go)
    {
        int x = GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth-1;
        int y = GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemaplength-1;
        bool validSpawnPoint = false;

        while (validSpawnPoint == false)
        {
            if (GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x, y].GetComponent<Tile>() != null
                && GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x, y].GetComponent<Tile>().getifobstacle() == false)
            {
                validSpawnPoint = true;
                break;
            }

            if (x < 0)
            {
                x = GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth;
                y--;
            }
            x--;
        }

        Debug.Log("spawning at " + x + "," + y);
        spawncharacter(go, x, y, false);
        GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x, y].GetComponent<Tile>().changeifobstacle(true);
    }

	public void cycleactiveplayercharacter() //cycles through active player characters
	{
		Debug.Log("cycling active player character");
		int previousactivecharacterindex = activecharacter;
		do{
			//Debug.Log ("cycling active character");
			cycleactivecharacter();
		}while(characters[activecharacter].GetComponent<PlayerActor>() == null && activecharacter != previousactivecharacterindex);
		if(characters[activecharacter] == null || characters[activecharacter].GetComponent<PlayerActor>() == null)
		{
			Debug.Log ("All player characters dead.  You lose." + activecharacter);
			Application.Quit();
		}
		//update the camera's pivot point
		GameObject.FindWithTag("location").GetComponent<LocationDriver>().setpivot(characters[activecharacter].GetComponent<PlayerActor>().transform.position);
	}

	public void cycleactivecharacter() //cycles to next active character
	{
		if(activecharacter+1 < characters.Count)
			activecharacter++;
		else
			endround();
		if(characters[activecharacter].GetComponent<Actor>().isturnover == true && getifturnover() == false)
		{
			//cycleactivecharacter();
		}
		if(characters[activecharacter].GetComponent<Actor>().modifier_fear == true)
		{
			characters[activecharacter].GetComponent<Actor>().modifier_fear = false;
			cycleactivecharacter();
		}
		Debug.Log("active character is " + activecharacter);
		if(characters[activecharacter].GetComponent<ComputerActor>() != null)
			characters[activecharacter].GetComponent<ComputerActor>().AItaketurn();
	}

	/*
	public void cycleactivecharacter() //cycles through active characters
	{
		//Debug.Log("cycling active character");
		if(activecharacter+1 < characters.Count)
			activecharacter++;
		else
			activecharacter = 0;
		Debug.Log("about to get to point");
		//THIS IS THE PROBLEM STATEMENT, IT DOESN'T GET STUCK IF THIS IS REMOVED
		if(characters[activecharacter].GetComponent<Actor>().isturnover == true && getifturnover() == false)
		{
			Debug.Log("HEY");
			cycleactivecharacter();
		}
		if(characters[activecharacter].GetComponent<Actor>().modifier_fear == true)
		{
			characters[activecharacter].GetComponent<Actor>().modifier_fear = false;
			cycleactivecharacter();
		}
		//Debug.Log ("Active character is: " + activecharacter);
	}*/

	void Update()
	{
		if(GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate == gamestates.moving)
		{
			move();
		}
		else if(Input.GetButtonDown("Jump") && characters[activecharacter].GetComponent<PlayerActor>() != null)
		{
			cycleactivecharacter();
		}
	}

	//renders test UI, for testing purposes only use Unity's full UI system for final product
	void OnGUI()
	{
		if(GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate == gamestates.idle
			|| GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate == gamestates.ability_selected)
		{
			if(GUI.Button(new Rect(Screen.width-150,40,100,20), "end turn"))
				cycleactivecharacter();
		}
	}

	public void setwaypoint(int x, int y) //sets the waypoint for computer or player character
	{
		Debug.Log("setting waypoint");
		pathlist = GameObject.Find ("voxels").GetComponent<VoxelDriver>().getpath(); //update the path
		if(pathlist != null)
		{
			pathlistindex = 1;
			waypoint = GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap[x,y].GetComponent<Tile>().getposition();
			//Debug.Log("WAYPOINT: " + waypoint);
			characters[activecharacter].GetComponent<Actor>().setwaypoint(x,y); //update obstacles and x,y position
			GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate = gamestates.moving;
			intermediatewaypoint = GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap[pathlist[pathlistindex][0], pathlist[pathlistindex][1]].GetComponent<Tile>().getposition();//set the intermediate waypoint to the second value in the path list (the first is the starting point)
		}
	}

	public void attackmove(int x, int y, GameObject targetactor) //sets the waypoint for computer or player character that's going to attack
	{
		Debug.Log("attack moving");
		this.targetactor = targetactor;
		setwaypoint(x,y);
	}

	void move()
	{
		Debug.Log("moving");
		if(Vector3.Distance(characters[activecharacter].transform.position, intermediatewaypoint) < WAYPOINTDEVIATION) //if character is at the intermediate waypoint
		{
			if(pathlistindex == pathlist.Count-1) //if the character is at the waypoint
			{
				//Debug.Log ("Finished moving");
				GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate = gamestates.idle;
				if(targetactor != null)
				{
					characters[activecharacter].GetComponent<Actor>().attack(targetactor);
					targetactor = null;
				}
				return;
			}
			else
			{
				pathlistindex++;
				//assign next path node to intermediate waypoint
				//Debug.Log ("index " + pathlistindex);
				//Debug.Log ("pathlist.count: " + pathlist.Count); //crashes when this gets to 2, this triggers every frame, not good
				//update the intermediatewaypoint
				intermediatewaypoint = GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap[pathlist[pathlistindex][0], pathlist[pathlistindex][1]].GetComponent<Tile>().getposition();
			}
		}
		//move towards the intermediate waypoint
		characters[activecharacter].transform.position = Vector3.MoveTowards(characters[activecharacter].transform.position, intermediatewaypoint, speed*Time.deltaTime); //the last parameter, the delta, may cause issues, also with checking to see if it's there yet
		//Debug.Log ("Character position: " + characters[activecharacter].transform.position.ToString());
	}

	//ends the round and resets for the next
	public void endround()
	{
		activecharacter = 0;
		gameObject.BroadcastMessage("newround", SendMessageOptions.DontRequireReceiver); //reset all actors for next round
	}

	//returns if all characters have moved
	public bool getifturnover()
	{
		bool isturnover = true; //flag for testing if the player's turn is over
		for(int i=0; i<characters.Count; i++)
		{
			if(characters[i].GetComponent<Actor>().isturnover == false)
				isturnover = false;
		}
		return isturnover;
	}

	//tests if player turn over and ends the player's turn if so
	//public void testifplayerturnover()
	//{
	//	if(getifplayerturnover() == true)
	//		endplayerturn();
	//}

	public bool getifplayerturnover() //returns if all player characters have moved
	{
		Debug.Log("testing if player turn over");
		bool isturnover = true; //flag for testing if the player's turn is over
		for(int i=0; i<characters.Count; i++)
		{
			if(characters[i].GetComponent<PlayerActor>() != null)
			{
				if(characters[i].GetComponent<Actor>().isturnover == false)
					isturnover = false;
			}
		}
		return isturnover;
	}

	//ends the player's turn, controls computer actors and resets for the next round
	void endplayerturn()
	{
		Debug.Log("ending player's turn");
		//*****END PLAYER'S TURN*****

		//*****PASS CONTROL TO THE COMPUTER ACTORS*****
		//AI();
		Debug.Log ("Active character is: " + activecharacter);
		//*****RESET FOR THE NEXT ROUND*****
		gameObject.BroadcastMessage("newround", SendMessageOptions.DontRequireReceiver);//reset all actors for the next round
	}

	//right now this is just a placeholder, will do complex AI later (and actors may not be moved in order)
	/*void AI()
	{
		Debug.Log("AI");
		for(int i=0; i<characters.Count; i++)
		{
			if(characters[i].GetComponent<ComputerActor>() != null)
			{
				characters[i].GetComponent<ComputerActor>().testAI();
			}
		}
	}*/
}
