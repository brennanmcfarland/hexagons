using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum hextypes {grass, dirt, stone, sand, none};	//can expand later to include kinds of terrain

public class VoxelDriver : MonoBehaviour {
	
	public int tilemaplength;
	public int tilemapwidth;
	public string biome; //biome setting, controls terrain textures
	//public GameObject hex;
	public GameObject[,] tilemap;
	public List<GameObject> obstacles = new List<GameObject>();
	public List<GameObject> props = new List<GameObject>();
	public List<GameObject> tilehighlights = new List<GameObject>(); //may need to be changed later for farther moving characters
    public bool someonedied = false; //for use in update only

	List<int[]> pathlist = null; //list holding x and y values of a path, used in findpath, null if no path found
	bool flying = false; //for pathfinding purposes only, to see if tile height and obstacles should be ignored

	// Use this for initialization
	void Awake () {

		Debug.Log(Application.persistentDataPath);
		//test values
		tilemaplength = 10;
		tilemapwidth = 10;
		biome = "biome_2";

		tilemap = new GameObject[tilemapwidth, tilemaplength];	//initialize tilemap

		//floodfill();
		randommap(tilemaplength, tilemapwidth, 3);
		//GameObject.Find("serialized data").GetComponent<Serialization>().backendloadmap();
		//GameObject.Find("serialized data").GetComponent<Serialization>().loadmap("testsave");
		//GameObject.Find("serialized data").GetComponent<Serialization>().backendloadmap();
		//tilemap[3,2].GetComponent<Tile>().setheight(10);
		//addobstacle(2,4,"rock");
		//addobstacle(2,3,"rock");
		//addobstacle(3,3,"rock");
		//addprop(3,3,"grasstuft");
		//GameObject.Find("serialized data").GetComponent<Serialization>().loadsession("testsave");

		//reseed the random generator, so the same run of values doesn't occur for the same maps
		Random.seed = System.DateTime.Now.Millisecond;

		Vector3[] testobscoords = {new Vector3(6,5,0), new Vector3(5,5,-1)};
		//addobstacle (testobscoords,"testlarge");


		//for(int i=0; i<pathlist.Count; i++)
		//{
		//	Debug.Log (pathlist[i][0] + "," + pathlist[i][1]);
		//}
	}

	public List<int[]> getpath()
	{
		return pathlist;
	}

	//TODO: add method to destroy an obstacle and have it called whenever any underlaying tile is destroyed
	// and see if you can make it work for obstacles covering multiple tiles
	//last parameter is the collider, if null it creates a mesh collider
	void addobstacle(int x, int y, string go) //adds an obstacle on a given tile
	{
		GameObject goobstacle = Instantiate(Resources.Load (@"Obstacles/"+go), new Vector3((float)(x+y*.5),tilemap[x,y].GetComponent<Tile>().getheight(false),(float)(y*1.5/Mathf.Sqrt(3))), Quaternion.identity) as GameObject;
		go = char.ToUpper(go[0])+go.Substring(1); //convert the first letter to uppercase to preserve file organization
		System.Type componentType = System.Type.GetType(go+"_Script", false, false);
		goobstacle.AddComponent(componentType);
		obstacles.Add (goobstacle);
		tilemap[x,y].GetComponent<Tile>().changeifobstacle(true);
	}

	void addobstacle(Vector2[] coords, string go) //adds an obstacle on a given tile
	{
		GameObject goobstacle = Instantiate(Resources.Load (@"Obstacles/"+go), new Vector3((float)(coords[0].x+coords[0].y*.5),tilemap[(int)coords[0].x,(int)coords[0].y].GetComponent<Tile>().getheight(false),(float)(coords[0].y*1.5/Mathf.Sqrt(3))), Quaternion.identity) as GameObject;
		go = char.ToUpper(go[0])+go.Substring(1); //convert the first letter to uppercase to preserve file organization
		System.Type componentType = System.Type.GetType(go+"_Script", false, false);
		goobstacle.AddComponent(componentType);
		obstacles.Add (goobstacle);
		tilemap[(int)coords[0].x,(int)coords[0].y].GetComponent<Tile>().changeifobstacle(true);
		flattenobstaclebase(coords);
	}

	public void flattenobstaclebase(Vector2[] coords) //flattens the base area under a multiple-tile obstacle
	{
		int originheight = (int)(tilemap[(int)coords[0].x,(int)coords[0].y].GetComponent<Tile>().getheight(true)); //saves the origin's height so it doesn't have to be fetched each time
		tilemap[(int)coords[0].x,(int)coords[0].y].GetComponent<Tile>().changeifobstacle(true); //set origin as obstacle
		for(int i=1; i< coords.Length; i++)
		{
			tilemap[(int)coords[i].x,(int)coords[i].y].GetComponent<Tile>().changeifobstacle(true); //set obstructed tiles as obstacle
			if(tilemap[(int)coords[i].x,(int)coords[i].y].GetComponent<Tile>().getheight(false) != originheight)//set obstructed tiles at equal height to origin
				tilemap[(int)coords[i].x,(int)coords[i].y].GetComponent<Tile>().setheight(originheight);
		}
	}

	void addobstacle(Vector3[] coords, string go) //adds an obstacle spanning multiple tiles with variable tile heights, third value of coords is tile height relative to origin, origin must be first value in coords
	{
		int originheight = (int)(tilemap[(int)coords[0].x,(int)coords[0].y].GetComponent<Tile>().getheight(true)); //saves the origin's height so it doesn't have to be fetched each time

		int minimumneworiginheight = 0; //minimum height of the obstacle's placement
		for(int i=1; i<coords.Length; i++) //find minimum origin height
		{
			if(coords[0].z-coords[i].z > minimumneworiginheight)
				minimumneworiginheight = (int)(coords[0].z-coords[i].z);
		}
		if(minimumneworiginheight > originheight) //if the origin's height is below the mimimum
			originheight = minimumneworiginheight;//set it to the minimum
		GameObject goobstacle = Instantiate(Resources.Load(@"Obstacles/"+go), new Vector3(coords[0].x+coords[0].y*.5f,tilemap[(int)coords[0].x,(int)coords[0].y].GetComponent<Tile>().getheight(false),(float)(coords[0].y*1.5/Mathf.Sqrt(3))), Quaternion.identity) as GameObject;
		//goobstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go = char.ToUpper(go[0])+go.Substring(1); //convert the first letter to uppercase to preserve file organization
		System.Type componentType = System.Type.GetType(go+"_Script", false, false);
		goobstacle.AddComponent(componentType);
		obstacles.Add (goobstacle);
		tilemap[(int)coords[0].x,(int)coords[0].y].GetComponent<Tile>().changeifobstacle(true); //set origin as obstacle
		for(int i=1; i< coords.Length; i++)
		{
			tilemap[(int)coords[i].x,(int)coords[i].y].GetComponent<Tile>().changeifobstacle(true); //set obstructed tiles as obstacle
			//set obstructed tiles at their new height relative to the origin
			tilemap[(int)coords[i].x,(int)coords[i].y].GetComponent<Tile>().setheight((int)(originheight+coords[i].z));
		}
	}

	public void loadobstacle(Vector3 pos, string go) //also adds an obstacle, used in saving/loading to avoid needing position indices; NOTE: does not mark tiles as obstructed
	{
		GameObject goobstacle = Instantiate(Resources.Load (@"Obstacles/"+go), pos, Quaternion.identity) as GameObject;
		go = char.ToUpper(go[0])+go.Substring(1); //convert the first letter to uppercase to preserve file organization
		System.Type componentType = System.Type.GetType(go+"_Script", false, false);
		goobstacle.AddComponent(componentType);
		obstacles.Add (goobstacle);
	}

	void addprop(int x, int y, string go) //adds a "prop" (non-obstacle object for decoration) on a given tile
	{
		if(tilemap[x,y].GetComponent<Tile>().getifobstacle() == false) //will not place props on same spaces as obstacles
		{
			GameObject goprop = Instantiate(Resources.Load (@"Props/"+go), new Vector3((float)(x+y*.5),tilemap[x,y].GetComponent<Tile>().getheight(false),(float)(y*1.5/Mathf.Sqrt(3))), Quaternion.identity) as GameObject;
			go = char.ToUpper(go[0])+go.Substring(1); //convert the first letter to uppercase to preserve file organization
			System.Type componentType = System.Type.GetType(go, false, false);
			goprop.AddComponent(componentType);
			props.Add (goprop);
		}
	}

	public void loadprop(Vector3 pos, string go) //also adds a prop given a position vector, used in saving/loading to avoid needing position indices; NOTE: does not check for overlap
	{
		GameObject goprop = Instantiate(Resources.Load (@"Props/"+go), pos, Quaternion.identity) as GameObject;
		go = char.ToUpper(go[0])+go.Substring(1); //convert the first letter to uppercase to preserve file organization
		System.Type componentType = System.Type.GetType(go, false, false);
		goprop.AddComponent(componentType);
		props.Add (goprop);
	}

	public void filltile(int x, int y, hextypes terraintype, int height) //fills or replaces a spot on the map with a tile
	{
		destroytile (x,y);
		GameObject go = new GameObject();
		go.AddComponent<Tile>();
		tilemap[x,y] = go;
		tilemap[x,y].GetComponent<Tile>().init(x,y, terraintype, height);
	}

	public void destroytile(int x, int y) //destroys a tile at a certain spot if one exists
	{
		if(tilemap.GetLength(0) > x && tilemap.GetLength(1) > y && tilemap[x,y] != null)
		{
			tilemap[x,y].GetComponent<Tile>().reset();
			Destroy (tilemap[x,y]);
		}
	}

	void floodfill() //fills the map with tiles
	{
		for(int y=0; y<tilemaplength; y++)
		{
			for(int x=0; x<tilemapwidth; x++)
			{
				filltile (x,y, hextypes.stone, 1);
			}
		}
	}

	void checkerheight() //doubles height of every other tile
	{
		bool odd = false;
		for(int y=0; y<tilemaplength; y++)
		{
			for(int x=0; x<tilemapwidth; x++)
			{
				if(odd == true)
				{
					tilemap[x,y].GetComponent<Tile>().setheight(5);
					odd = false;
				}
				else
				{
					odd = true;
				}
			}
		}

	}

	void checkertype() //changes the terrain type of every other tile
	{
		bool odd = false;
		for(int y=0; y<tilemaplength; y++)
		{
			for(int x=0; x<tilemapwidth; x++)
			{
				if(odd == true)
				{
					tilemap[x,y].GetComponent<Tile>().settype(hextypes.stone);
					odd = false;
				}
				else
				{
					odd = true;
				}
			}
		}
	}

	void randommap(int length, int width, int maxheight) //randomly generates a map (and prints seed in case it's cool)
	{
		//seed the random generator
		int time = System.DateTime.Now.Millisecond;
		Debug.Log("The seed for this map is: " + time);
		Random.seed = time;
		
		for(int y=0; y<length; y++)
		{
			for(int x=0; x<width; x++)
			{
				switch(Random.Range(0,3))
		        {
					case 0:
						filltile (x,y, hextypes.grass, Random.Range (1,maxheight+1));
					break;
					case 1:
						filltile (x,y, hextypes.dirt, Random.Range (1,maxheight+1));
						break;
					case 2:
						filltile (x,y, hextypes.stone, Random.Range (1,maxheight+1));
						break;
					case 3:
						filltile (x,y, hextypes.sand, Random.Range (1,maxheight+1));
						break;
					default:
						Debug.Log ("Error randomizing tile terrain type.");
						break;
				}
			}
		}
	}

	void loadmap()	//loads a map 
	{
		
	}

	public void findpath(int startX, int startY, int endX, int endY, int range, int[][] tiles)
	{
		if(flying == false && GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter].GetComponent<Actor>().hasmoved == true)
		{
			pathlist = null;
			return;
		}

		binaryheap openset = new binaryheap(new heapnode(0,estimatehcost(startX, startY, endX, endY), startX, startY, null,0));
		openset.whichone = " -- open set ";
		List<heapnode> closedset = new List<heapnode>();
		heapnode parentnode = openset.getnode(1);
		heapnode temp; //temporary node for recalculating better paths
		bool isinclosedset;
		//openset.debugheap();

		//NEW: get all enemy positions
		int[][] enemypositions = getallenemypositions();
		if(tiles == null)
		{
			while(openset.getsize() != 0)
			{
				//check if the parent node is the goal, if so the best path is found (and it would be added to the closed list)
				if(parentnode.x == endX && parentnode.y == endY) //works if start and end are same tile, so nothing wrong here
				{
					openset.debugheap();
					
					//tracepath(closedset.getnode(closedset.getsize()-1));
					tracepath(closedset[closedset.Count-1]);
					//for(int i=0; i<pathlist.Count; i++)
					//	Debug.Log(pathlist[i][0] + "," + pathlist[i][1]);
					return; //exit the method
				}
				//make the parent node the tile with the lowest fcost
				parentnode = openset.getlowestfcostnode();
				//remove parent node from the open set
				openset.removenode();
				//openset.debugheap();
				//add parent node to the closed set
				closedset.Add(parentnode);
				//closedset.debugheap();

				//add all legal adjacent tiles to the list 
				for(int i=-1; i<=1; i++)
				{
					for(int j=-1; j<=1; j++)
					{
						if(i != j) //don't check the parent tile or 1,1 or -1,-1, those are illegal
						{
							//checking to see if in closed list
							isinclosedset = false;
							for(int k=0; k<closedset.Count; k++)
							{
								if(closedset[k].x == parentnode.x+i && closedset[k].y == parentnode.y+j)
									isinclosedset = true;
							}
							//checks if........ 
							if((parentnode.x+i >=0 && parentnode.x+i < tilemap.GetLength(1) && parentnode.y+j >=0 && parentnode.y+j < tilemap.GetLength(0) //A. tilemap goes that far 
							   && tilemap[parentnode.x+i, parentnode.y+j] != null //that tilemap space has a gameobject
							   && tilemap[parentnode.x+i, parentnode.y+j].GetComponent<Tile>() != null //B. that tilemap space has a tile 
								&& (flying == true || tilemap[parentnode.x+i,parentnode.y+j].GetComponent<Tile>().getifobstacle() == false) //C.if not obstructed
							   && isinclosedset == false//D. the tile is not on the closed list
							   && parentnode.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j)-parentnode.engagementpenalty <= range) //the new gcost minus engagement penalty is not higher than the movement range
							   && (openset.getnode(parentnode.x+i, parentnode.y+j) == null//E. not on openlist
							   || parentnode.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j) < parentnode.gcost))//F. gscore less than parent
							{
								//NEW: checking for enemy engagement
								for(int l=0; l<enemypositions.GetLength(0); l++) //scan through enemy positions
								{
									//if the coordinates match up
									if(enemypositions[l][0] == parentnode.x+i && enemypositions[l][1] == parentnode.y+j)
									{
										parentnode.engagementpenalty++; //increment the engagement penalty
										parentnode.gcost++; //increment the gcost
										break; //and break from the loop
									}
								}
								
								//the outer if loop is important for tracing the path
								if(openset.getnode(parentnode.x+i, parentnode.y+j) == null) //if not on the open set
								{
									openset.addtoheap(parentnode.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j), estimatehcost(parentnode.x+i, parentnode.y+j, endX, endY), parentnode.x+i, parentnode.y+j, parentnode, parentnode.engagementpenalty);//add it
									//openset.debugheap();
								}
								else if(parentnode.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j)+estimatehcost(parentnode.x+i, parentnode.y+j, endX, endY) < openset.getnode(parentnode.x+i, parentnode.y+j).parent.fcost) //if on open set, see if this is the better path to it
								{
									//reset the parent to parentnode
									temp = openset.getnode(parentnode.x+i, parentnode.y+j);
									temp.parent = closedset[closedset.Count-1];
									//recalculate gcost and hcost and engagementpenalty
									temp.gcost = temp.parent.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j);
									temp.hcost = estimatehcost(temp.x, temp.y, endX, endY);
									temp.engagementpenalty = temp.parent.engagementpenalty;
									openset.replacenode(temp);
								}
							}
						}
					}
				}
			}
		}
		else
		{
			Debug.Log ("tiles");
			for(int a=0; a<tiles.Length;a++)
			{
				for(int b=0; b<tiles[a].Length;b++)
				{
					Debug.Log (tiles[a][b]);
				}
			}
			Debug.Log ("end");
			while(openset.getsize() != 0)
			{
				//check if the parent node is the goal, if so the best path is found (and it would be added to the closed list)
				if(parentnode.x == endX && parentnode.y == endY) //works if start and end are same tile, so nothing wrong here
				{
					openset.debugheap();
					
					//tracepath(closedset.getnode(closedset.getsize()-1));
					tracepath(closedset[closedset.Count-1]);
					//for(int i=0; i<pathlist.Count; i++)
					//	Debug.Log(pathlist[i][0] + "," + pathlist[i][1]);
					return; //exit the method
				}
				//make the parent node the tile with the lowest fcost
				parentnode = openset.getlowestfcostnode();
				//remove parent node from the open set
				openset.removenode();
				//openset.debugheap();
				//add parent node to the closed set
				closedset.Add(parentnode);
				//closedset.debugheap();
				
				//add all legal adjacent tiles to the list 
				for(int i=-1; i<=1; i++)
				{
					for(int j=-1; j<=1; j++)
					{
						if(i != j) //don't check the parent tile or 1,1 or -1,-1, those are illegal
						{
							//checking to see if in closed list
							isinclosedset = false;
							for(int k=0; k<closedset.Count; k++)
							{
								if(closedset[k].x == parentnode.x+i && closedset[k].y == parentnode.y+j)
									isinclosedset = true;
							}
							bool isvalidtile = true;
							for(int tileindex = 0; tileindex < tiles.Length; tileindex++)
							{
								//checks if........ 
								if((parentnode.x+i+tiles[tileindex][0] >=0 && parentnode.x+i+tiles[tileindex][0] < tilemap.GetLength(1) && parentnode.y+j+tiles[tileindex][1] >=0 && parentnode.y+j+tiles[tileindex][1] < tilemap.GetLength(0) //A. tilemap goes that far 
								    && tilemap[parentnode.x+i+tiles[tileindex][0], parentnode.y+j+tiles[tileindex][1]] != null //that tilemap space has a gameobject
								    && tilemap[parentnode.x+i+tiles[tileindex][0], parentnode.y+j+tiles[tileindex][1]].GetComponent<Tile>() != null //B. that tilemap space has a tile 
									&& (flying == true || tilemap[parentnode.x+i+tiles[tileindex][0],parentnode.y+j+tiles[tileindex][1]].GetComponent<Tile>().getifobstacle() == false) //C.if not obstructed
								    && isinclosedset == false//D. the tile is not on the closed list
								    && parentnode.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j)-parentnode.engagementpenalty <= range) //the new gcost minus engagement penalty is not higher than the movement range
								   && (openset.getnode(parentnode.x+i, parentnode.y+j) == null//E. not on openlist
								    || parentnode.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j) < parentnode.gcost))//F. gscore less than parent
								{

								}
								else
								{
									isvalidtile = false;
								}
								if(isvalidtile == true)
								{
									//NEW: checking for enemy engagement
									for(int l=0; l<enemypositions.GetLength(0); l++) //scan through enemy positions
									{
										//if the coordinates match up
										if(enemypositions[l][0] == parentnode.x+i && enemypositions[l][1] == parentnode.y+j)
										{
											parentnode.engagementpenalty++; //increment the engagement penalty
											parentnode.gcost++; //increment the gcost
											break; //and break from the loop
										}
									}
									
									//the outer if loop is important for tracing the path
									if(openset.getnode(parentnode.x+i, parentnode.y+j) == null) //if not on the open set
									{
										openset.addtoheap(parentnode.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j), estimatehcost(parentnode.x+i, parentnode.y+j, endX, endY), parentnode.x+i, parentnode.y+j, parentnode, parentnode.engagementpenalty);//add it
										//openset.debugheap();
									}
									else if(parentnode.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j)+estimatehcost(parentnode.x+i, parentnode.y+j, endX, endY) < openset.getnode(parentnode.x+i, parentnode.y+j).parent.fcost) //if on open set, see if this is the better path to it
									{
										//reset the parent to parentnode
										temp = openset.getnode(parentnode.x+i, parentnode.y+j);
										temp.parent = closedset[closedset.Count-1];
										//recalculate gcost and hcost and engagementpenalty
										temp.gcost = temp.parent.gcost+addtogcost(parentnode.x, parentnode.y, parentnode.x+i, parentnode.y+j);
										temp.hcost = estimatehcost(temp.x, temp.y, endX, endY);
										temp.engagementpenalty = temp.parent.engagementpenalty;
										openset.replacenode(temp);
									}
								}
							}
						}
					}
				}
			}
		}
		//Debug.Log ("Failed to find path. Range: " + range);
		pathlist = null;
		return; //if a path is not found
	}

	int estimatehcost(int x, int y, int endX, int endY)
	{
		return Mathf.Abs(endX-x)+Mathf.Abs (endY-y);
	}

	int addtogcost(int parentx, int parenty, int x, int y)
	{
		int parentnodeheight = (int)(tilemap[parentx,parenty].GetComponent<Tile>().getheight(true));
		int nodeheight = (int)(tilemap[x,y].GetComponent<Tile>().getheight(true));
		if(flying == true) //if flying, ignore height restrictions
		{
			return 1;
		}
		else if(parentnodeheight < nodeheight) //if the next tile is higher than the parent one
		{
			return 1+ nodeheight-parentnodeheight;
		}
		else //if the next tile is at the same height or lower than the parent
		{
			return 1;
		}
	}

	void tracepath(heapnode node) //trace the path
	{
		pathlist = new List<int[]>();
		if(node.parent != null)
		{
			tracepath(node.parent);
		}
		//Debug.Log(node.x + "," + node.y + " on path");
		pathlist.Add(new int[]{node.x, node.y}); //add the node to the list
		return;
	}

	//used in pathfinding, return all enemy coordinates to test for engagement penalties
	int[][] getallenemypositions()
	{
		List<int[]> enemypositions = new List<int[]>(); //create a list to hold enemy coordinates

		for(int i=0; i<GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Count; i++)
		{
			if(GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i].GetComponent<ComputerActor>() != null)
				enemypositions.Add(new int[]{GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i].GetComponent<Actor>().Xindex,GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i].GetComponent<Actor>().Yindex});
		}
		return enemypositions.ToArray(); //convert to an array for quicker access and return
	}

	//serializes and returns a list of all props and obstacles
	public List<string>[] serializepropsandobstacles()
	{
		List<string> propIDs = new List<string>();
		for(int i=0; i<props.Count; i++)
		{
			propIDs.Add (props[i].name);
		}
		List<string> obstacleIDs = new List<string>();
		for(int i=0; i<obstacles.Count; i++)
		{
			obstacleIDs.Add(obstacles[i].name);
		}
		List<string>[] temp = {propIDs,obstacleIDs};
		return temp;
	}

	void Update()
	{
        someonedied = false;
		//initialize the cursor for this frame and reference so no need to keep finding it later
		GameObject guidriver = GameObject.FindWithTag("GUI");
		guidriver.GetComponent<GUIDriver>().showcursor();
		guidriver.GetComponent<GUIDriver>().setcursor(cursortypes.arrow);

		//find out what mouse is over and respond accordingly
		GameObject.FindWithTag ("GUI").GetComponent<GUIDriver>().tilehighlight.GetComponent<Renderer>().enabled = false; //disable the highlight tile
		GameObject.FindWithTag("GUI").GetComponent<GUIDriver>().setcursor(cursortypes.arrow); //reset the cursor
		RaycastHit hit;

		//get the active character's Actor object so no need to keep finding it later
		GameObject activecharacter = GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter];

		//if the mouse is over something and the game state is idle or ability_selected
		if((GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate == gamestates.idle
			|| GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate == gamestates.ability_selected)
			&& Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f))
		{
			//get the transform of where the highlight should be
			Transform currentobjecttransform = hit.collider.transform;
			//Debug.Log(currentobjecttransform.gameObject.GetComponent<ComputerActor>().ToString());

			//if the mouse is over a tile
			if(currentobjecttransform.parent != null && currentobjecttransform.parent.gameObject.GetComponent<Tile>() != null && currentobjecttransform.parent.gameObject.GetComponent<Tile>().getifobstacle() == false)
			{
				//highlight the tile if mouse is over and no obstacle
				if(GameObject.FindWithTag ("GUI").GetComponent<GUIDriver>().tilehighlight.transform != currentobjecttransform || GameObject.FindWithTag ("GUI").GetComponent<GUIDriver>().tilehighlight.GetComponent<Renderer>().enabled == false)
				{
					GameObject.FindWithTag ("GUI").GetComponent<GUIDriver>().tilehighlight.transform.position = new Vector3(currentobjecttransform.position.x,currentobjecttransform.position.y+.001f,currentobjecttransform.position.z);
					GameObject.FindWithTag ("GUI").GetComponent<GUIDriver>().tilehighlight.GetComponent<Renderer>().enabled = true;
					findpath(activecharacter.GetComponent<Actor>().Xindex,
							 activecharacter.GetComponent<Actor>().Yindex,
					         currentobjecttransform.parent.gameObject.GetComponent<Tile>().Xindex, 
					         currentobjecttransform.parent.gameObject.GetComponent<Tile>().Yindex, 
					         activecharacter.GetComponent<Actor>().getspeed(),
					         activecharacter.GetComponent<Actor>().getmultipletiles()
					         );
				}
				if(Input.GetButtonDown("Fire1")) //set waypoint if clicked
				{
					GameObject.FindWithTag ("Player").GetComponent<ActorDriver>().setwaypoint(currentobjecttransform.parent.gameObject.GetComponent<Tile>().Xindex, currentobjecttransform.parent.gameObject.GetComponent<Tile>().Yindex);
				}
			}
			//if the mouse is over an actor
			else if(currentobjecttransform.gameObject != null && currentobjecttransform.gameObject.GetComponent<Actor>() != null)
			{
				//set that player to not be an obstacle so pathfinding leads up to it
				tilemap[currentobjecttransform.gameObject.GetComponent<Actor>().Xindex,currentobjecttransform.gameObject.GetComponent<Actor>().Yindex].GetComponent<Tile>().changeifobstacle(false);

				//if the mouse is over a computer actor, find the path to it
				pathlist = null;
				if(currentobjecttransform.gameObject.GetComponent<ComputerActor>() != null)
				{
					findpath(activecharacter.GetComponent<Actor>().Xindex,
					         activecharacter.GetComponent<Actor>().Yindex,
					         currentobjecttransform.gameObject.GetComponent<Actor>().Xindex,
					         currentobjecttransform.gameObject.GetComponent<Actor>().Yindex,
					         activecharacter.GetComponent<Actor>().getspeed(),
					         activecharacter.GetComponent<Actor>().getmultipletiles()
					         );

					//if the attack is melee, find the path to attack and change the cursor if in range
					if((int)(activecharacter.GetComponent<Actor>().abilityranges[activecharacter.GetComponent<Actor>().selectedability]) == 1)
					{
						if(pathlist != null)
						{
							guidriver.GetComponent<GUIDriver>().setcursor(cursortypes.attack);
								
							//attack if clicked
							if(Input.GetButtonDown("Fire1"))
							{
								Debug.Log(pathlist.Count);
								//if adjacent, just attack
								if(pathlist.Count == 2)
								{
									activecharacter.GetComponent<Actor>().attack(currentobjecttransform.gameObject);
								}
								//otherwise, move then attack
								else
								{
									//gets stuck somewhere after this point
									//only happens to the second actor that attacks
									//so is some flag not getting reset?
									//also got stuck after pressing end turn button, so does it have to do with ending turn?
									Debug.Log("melee attack move");
									Debug.Log("GAMESTATE: " + GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate);
									//Debug.Log();
									pathlist.RemoveAt(pathlist.Count-1);
									GameObject.FindWithTag("Player").GetComponent<ActorDriver>().attackmove(pathlist[pathlist.Count-1][0],pathlist[pathlist.Count-1][1], currentobjecttransform.gameObject);
									Debug.Log("attack move call executed successfully");
								}
							}
						}

					}
					//if the attack is ranged, only change the cursor or enable attack if in range and in LOS
					else if((int)(activecharacter.GetComponent<Actor>().abilityranges[activecharacter.GetComponent<Actor>().selectedability]) != 0)
					{
						//recalculate path with ability range ignoring height differences and obstacles to determine if in range
						flying = true;
						findpath(activecharacter.GetComponent<Actor>().Xindex,
							activecharacter.GetComponent<Actor>().Yindex,
							currentobjecttransform.gameObject.GetComponent<Actor>().Xindex,
							currentobjecttransform.gameObject.GetComponent<Actor>().Yindex,
							(int)(activecharacter.GetComponent<Actor>().abilityranges[activecharacter.GetComponent<Actor>().selectedability]),
							activecharacter.GetComponent<Actor>().getmultipletiles()
						);
						if(pathlist != null && activecharacter.GetComponent<Actor>().lineofsight(currentobjecttransform.gameObject,100.0f) == true)
						{
							guidriver.GetComponent<GUIDriver>().setcursor(cursortypes.attack);
							//attack if clicked 
							if(Input.GetButtonDown("Fire1"))
							{
								activecharacter.GetComponent<Actor>().attack(currentobjecttransform.gameObject);
							}
						}
						flying = false;
					}
					//if range is not applicable, do nothing (activated when button clicked)

					//if(pathlist == null)
						//Debug.Log("NO PATH");

					/*
					if(Input.GetButtonDown("Fire1")) //if the character is in range and the player clicks, attack
					{
						//move and attack if it is adjacent
						if(Mathf.Abs((activecharacter.GetComponent<Actor>().Xindex-currentobjecttransform.gameObject.GetComponent<Actor>().Xindex)
							+(activecharacter.GetComponent<Actor>().Yindex-currentobjecttransform.gameObject.GetComponent<Actor>().Yindex)) < 2)
						{
							Debug.Log("A");
							activecharacter.GetComponent<Actor>().attack(currentobjecttransform.gameObject);
						}
						//move and attack if it is not ranged and can move to attack
						else if((int)(activecharacter.GetComponent<Actor>().abilityranges[activecharacter.GetComponent<Actor>().selectedability]) == 1)
						{
							Debug.Log("C");
							if(pathlist != null)
							{
								Debug.Log("B");
								//Debug.Log("is obstacle?  " + tilemap[currentobjecttransform.parent.gameObject.GetComponent<Actor>().Xindex,currentobjecttransform.parent.gameObject.GetComponent<Actor>().Yindex].GetComponent<Tile>().getifobstacle());
								//Debug.Log("length of pathlist " + pathlist.Count);
								pathlist.RemoveAt(pathlist.Count-1);
								if(pathlist.Count > 1) // >1 because the start and end tile are also counted and will mess up the movement method
									GameObject.FindWithTag("Player").GetComponent<ActorDriver>().setwaypoint(pathlist[pathlist.Count-1][0],pathlist[pathlist.Count-1][1], currentobjecttransform.gameObject);
								//activecharacter.SendMessage("attack",currentobjecttransform.gameObject,SendMessageOptions.RequireReceiver);		
							}
						}
						else //use ranged attack if a in range or in range after movement
						{
							activecharacter.SendMessage("rangedattack",currentobjecttransform.parent.gameObject,SendMessageOptions.RequireReceiver);		
						}
						//activecharacter.SendMessage("attack",currentobjecttransform.gameObject,SendMessageOptions.RequireReceiver);
					}*/
				}
                //reset that tile to being an obstacle
                if(someonedied == false)
				    tilemap[currentobjecttransform.gameObject.GetComponent<Actor>().Xindex,currentobjecttransform.gameObject.GetComponent<Actor>().Yindex].GetComponent<Tile>().changeifobstacle(true);
			}

		}
			
		//delete previous path highlights
		for(int i=0; i< tilehighlights.Count; i++)
		{
			Destroy(tilehighlights[i]);
		}
		//highlight path
		if(pathlist != null && GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate == gamestates.idle)
		{
			//highlight all tiles on path
			for(int i=0; i< pathlist.Count; i++)
			{
				tilehighlights.Add(Instantiate(Resources.Load ("hexmeshtop"), new Vector3((float)(pathlist[i][0]+pathlist[i][1]*.5),tilemap[pathlist[i][0],pathlist[i][1]].GetComponent<Tile>().getposition().y+.001f,(float)(pathlist[i][1]*1.5/Mathf.Sqrt(3))), Quaternion.identity) as GameObject);
				tilehighlights[tilehighlights.Count-1].GetComponent<Renderer>().material = Resources.Load(@"Materials/highlight", typeof(Material)) as Material;
			}
		}
		//Debug.Log("successfully got to the end of the update method");
	}
}


/*TODO:
 *  can optimize A* by adding all obstacles and non-valid tiles to closedset?
 * 
 * 
 * */