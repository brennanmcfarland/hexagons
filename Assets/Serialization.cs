using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Serialization : MonoBehaviour {

	public GameObject[,] tilemap; //VoxelDriver's tilemap is loaded into this for backend serialization

	Hashtable data; //data to be saved/loaded
	uint key; //key for hashtable, reset to 0 on new save/load and incremented with each addition

	void Update ()
	{
		if(Input.GetButtonDown("Cancel"))
		   backendsavemap();
		if(Input.GetButtonDown("Save"))
		{
			savesession("testsave");
		}
	}

	//saves all relevant data from the current session of play, including the map and all characters' states, overwrites if existing file of same name
	void savesession(string name)
	{
		Debug.Log("Saving play session");
		//test to see if the proper directory structure exists and create it if it doesn't
		if(!(Directory.Exists(Application.persistentDataPath+"/saves")))
			Directory.CreateDirectory(Application.persistentDataPath+"/saves");

		//initialize the hashtable and key
		data = new Hashtable();
		key = 0;

		data.Add("map", serializemap()); //serialize the map

		//START NEW STUFF
		data.Add("activecharacter", GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter); //serialize activecharacter

		//serialize the characters
		List<savedata> characters = new List<savedata>();
		for(int i=0; i<GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Count; i++)
		{
			characters.Add(GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i].GetComponent<Actor>().serializecharacter());
		}
		data.Add("characters", characters);
		//END NEW STUFF

		//serialize the hashtable
		BinaryFormatter serializer = new BinaryFormatter();
		FileStream file = File.Open(Application.persistentDataPath + "/saves/"+name+".save", FileMode.Create);
		serializer.Serialize(file, data);
		file.Close();

		data = null; //clear the hashtable to save space

		Debug.Log("Play session saved");
	}

	//loads all relevant data from the current session of play, including the map and all characters' states
	public void loadsession(string name)
	{
		//exit if the file of the specified name is not found and print an error message
		if(!File.Exists(Application.persistentDataPath + "/saves/"+name+".save"))
		{
			Debug.Log("Error: file " + name + "not found");
			return;
		}

		//otherwise, deserialize the hashtable and initialize key
		BinaryFormatter deserializer = new BinaryFormatter();
		FileStream file = File.OpenRead(Application.persistentDataPath + "/saves/"+name+".save");
		data = (Hashtable)deserializer.Deserialize(file);
		key = 0;
		file.Close();

		deserializemap(); //deserialize the map

		/******START NEW STUFF********
		 * 
		 * */

        /*
            TODO: not clearing characters like it should
        */
		//clear any current character data
		List<GameObject> actordata = GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters;
		while(actordata.Count > 0)
			actordata[actordata.Count-1].GetComponent<Actor>().remove();
        //while (GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Count > 0)
        //    Destroy(GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Count - 1]);
        //foreach (Transform child in GameObject.FindWithTag("Player").transform)
        //    Destroy(child.gameObject);
        GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Clear();

		//deserialize activecharacter
		GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter = (int)data["activecharacter"];

		//deserialize the characters
		List<savedata> characters = (List<savedata>)(data["characters"]);
		savedata loadedactor;
		for(int i=0; i<characters.Count; i++)
		{
			loadedactor = (savedata)characters[i];
            string loadedactorid = loadedactor.ID.Substring(0, loadedactor.ID.Length - 7).ToLower();
			GameObject.FindWithTag("Player").GetComponent<ActorDriver>().spawncharacter(loadedactorid,loadedactor.position[0],loadedactor.position[1],loadedactor.isplayercharacter);
		}

		/******END NEW STUFF********
		 * 
		 * */

		data = null; //clear the hashtable to save space

		Debug.Log("Play session loaded");
	}

	//saves the map to a binary file in the persistent data path, overwrites if one already exists
	public void savemap(string name)
	{
		/******START NEW STUFF********
		 * 
		 * */
		Debug.Log("Saving map");
		//test to see if the proper directory structure exists and create it if it doesn't
		if(!(Directory.Exists(Application.persistentDataPath+"/maps")))
			Directory.CreateDirectory(Application.persistentDataPath+"/maps");

		//serialize the map hashtable
		BinaryFormatter serializer = new BinaryFormatter();
		FileStream file = File.Open(Application.persistentDataPath + "/maps/"+name+".mapsave", FileMode.Create);
		serializer.Serialize(file, serializemap());
		file.Close();

		Debug.Log("Map saved");

		/******END NEW STUFF********
		 * 
		 * */
	}

	public void loadmap(string name)
	{
		/******START NEW STUFF********
		 * 
		 * */
		//exit if the file of the specified name is not found and print an error message
		if(!File.Exists(Application.persistentDataPath + "/maps/"+name+".mapsave"))
		{
			Debug.Log("Error: file " + name + "not found");
			return;
		}

		//otherwise, deserialize the hashtable and initialize key
		BinaryFormatter deserializer = new BinaryFormatter();
		FileStream file = File.OpenRead(Application.persistentDataPath + "/maps/"+name+".mapsave");
		data = (Hashtable)deserializer.Deserialize(file);
		key = 0;
		file.Close();

		deserializemap(); //deserialize the map

		data = null; //clear the hashtable to save space

		Debug.Log("Map loaded");
		/******END NEW STUFF********
		 * 
		 * */
	}

	//serializes the map to its own hashtable
	Hashtable serializemap()
	{
		Hashtable mapdata = new Hashtable(); //holds the map data to be serialized

		tilemap = GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap; //copy the current tilemap

		//serialize the dimensions of tilemap
		mapdata.Add("len", tilemap.GetLength(0));
		mapdata.Add("wid", tilemap.GetLength(1));

		//serialize the terrain types
		List<hextypes> tiletype = new List<hextypes>();
		for(int i=0; i<tilemap.GetLength(0); i++) //for every row
		{
			for(int j=0; j<tilemap.GetLength(1); j++) //for every tile in that row
			{
				//serialize the terrain of that tile
				if(tilemap[i,j] != null && tilemap[i,j].GetComponent<Tile>() != null)
					tiletype.Add(tilemap[i,j].GetComponent<Tile>().terraintype);
				else
					tiletype.Add(hextypes.none);
			}
		}
		mapdata.Add("types",tiletype);

		/*****START NEW STUFF******
		 ************************ */

		//serialize the terrain heights
		List<int> tileheight = new List<int>();
		for(int i=0; i<tilemap.GetLength(0); i++) //for every line
		{
			for(int j=0; j<tilemap.GetLength(1); j++) //for every tile in that row
			{
				if(tilemap[i,j] != null && tilemap[i,j].GetComponent<Tile>() != null)
					tileheight.Add((int)(tilemap[i,j].GetComponent<Tile>().getheight(true)));
				else
					tileheight.Add(-1);
			}
		}
		mapdata.Add ("heights", tileheight);

		//serialize if obstructed
		List<bool> tileobstructions = new List<bool>();
		for(int i=0; i<tilemap.GetLength(0); i++) //for every line
		{
			for(int j=0; j<tilemap.GetLength(1); j++) //for every tile in that row
			{
				if(tilemap[i,j] != null && tilemap[i,j].GetComponent<Tile>() != null)
					tileobstructions.Add(tilemap[i,j].GetComponent<Tile>().getifobstacle());
				else
					tileobstructions.Add(false);
			}
		}
		mapdata.Add("obstructions", tileobstructions);

		//serialize props
		List<string> propstrings = new List<string>();
		for(int i=0; i<GameObject.Find ("voxels").GetComponent<VoxelDriver>().props.Count; i++)
		{
			propstrings.Add(GameObject.Find("voxels").GetComponent<VoxelDriver>().props[i].name + GameObject.Find("voxels").GetComponent<VoxelDriver>().props[i].transform.position);
		}
		mapdata.Add("props", propstrings);

		//serialize obstacles
		List<string> obstaclestrings = new List<string>();
		for(int i=0; i<GameObject.Find ("voxels").GetComponent<VoxelDriver>().obstacles.Count; i++)
		{
			obstaclestrings.Add(GameObject.Find("voxels").GetComponent<VoxelDriver>().obstacles[i].name + GameObject.Find("voxels").GetComponent<VoxelDriver>().obstacles[i].transform.position);
		}
		mapdata.Add("obstacles", obstaclestrings);

		/*****END NEW STUFF******
		 ************************ */
		return mapdata;
	}

	//deserializes the map from data
	void deserializemap()
	{
		//CLEAR ALL TILES SO THEY'RE NOT FLOATING AROUND, THEN REINITIALIZE TILEMAP
		if(GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap != null)
		{
			for(int x = 0; x < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap.GetLength(0); x++)
			{
				for(int y = 0; y < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap.GetLength(1); y++)
				{
					GameObject.Find("voxels").GetComponent<VoxelDriver>().destroytile(x,y);
				}
			}
		}
		GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap = new GameObject[GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth, GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemaplength];

		Debug.Log(((Hashtable)data["map"])["len"]);
		//deserialize the dimensions of tilemap
		GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth = (int)(((Hashtable)data["map"])["len"]);
		GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth = (int)(((Hashtable)data["map"])["wid"]);	

		Debug.Log(data["map"]);
		Hashtable temptable = (Hashtable)data["map"];
		Debug.Log(temptable["types"]);
		//deserialize the terrain types
		List<hextypes> tiletype = (List<hextypes>)(((Hashtable)(data["map"]))["types"]);
		int tilenum = 0;
		for(int i=0; i<(int)(((Hashtable)data["map"])["len"]); i++) //for every row
		{
			for(int j=0; j<(int)(((Hashtable)data["map"])["wid"]); j++) //for every tile in that row
			{
				Debug.Log(tiletype);
				//deserialize the terrain of that tile
				if(tiletype[tilenum] != hextypes.none)
					GameObject.Find("voxels").GetComponent<VoxelDriver>().filltile(i,j,tiletype[tilenum],1);
				else
					GameObject.Find("voxels").GetComponent<VoxelDriver>().destroytile(i,j);
				tilenum++;
			}
		}

		/*****START NEW STUFF******
		 ************************ */

		//deserialize the terrain heights
		List<int> tileheight = (List<int>)(((Hashtable)(data["map"]))["heights"]);
		tilenum = 0;
		for(int x = 0; x < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth; x++)
		{
			for(int y = 0; y < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemaplength; y++)
			{
				int temp = (int)tileheight[tilenum++];
				if(temp != -1)
					GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x,y].GetComponent<Tile>().setheight(temp);
			}
		}

		//deserialize the terrain obstructions
		List<bool> tileobstructions = (List<bool>)(((Hashtable)(data["map"]))["obstructions"]);
		tilenum = 0;
		for(int x = 0; x < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth; x++)
		{
			for(int y = 0; y < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemaplength; y++)
			{
				if(tileobstructions[tilenum++] == true)
					GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x,y].GetComponent<Tile>().changeifobstacle(true);
			}
		}

		//deserialize props
		List<string> propstrings = (List<string>)(((Hashtable)(data["map"]))["props"]);
		for(int i = 0; i < propstrings.Count; i++)
			loadProp(propstrings[i]);
		//deserialize obstacles
		List<string> obstaclestrings = (List<string>)(((Hashtable)(data["map"]))["obstacles"]);
		for(int i = 0; i < obstaclestrings.Count; i++)
			loadObstacle(obstaclestrings[i]);

		/*****END NEW STUFF******
		 ************************ */
	}

	//loads a prop
	void loadProp(string propSaveData)
	{
		Debug.Log("Prop Data: " + propSaveData);
		string propname = propSaveData.Substring(0,propSaveData.IndexOf('('));
		string proppositionstring = propSaveData.Substring(propname.Length+8).TrimEnd(')');
		string[] temp = proppositionstring.Split(new[]{' ',','},4);
		Vector3 propposition = new Vector3(float.Parse(temp[0]),float.Parse(temp[2]),float.Parse(temp[3].TrimStart(' ')));
		Debug.Log("Prop Position is " + propposition.ToString());
		//and instantiate it
		GameObject.Find("voxels").GetComponent<VoxelDriver>().loadprop(propposition, propname);
		Debug.Log("Prop successfully loaded");
	}

	//loads an obstacle
	void loadObstacle(string obstacleSaveData){
		Debug.Log("Obstacle Data: " + obstacleSaveData);
		string obstaclename = obstacleSaveData.Substring(0,obstacleSaveData.IndexOf('('));	
		string obstaclepositionstring = obstacleSaveData.Substring(obstaclename.Length+8).TrimEnd(')');
		string[] temp = obstaclepositionstring.Split(new[]{' ',','},4);
		Vector3 obstacleposition = new Vector3(float.Parse(temp[0]),float.Parse(temp[2]),float.Parse(temp[3].TrimStart(' ')));
		Debug.Log("Obstacle Position is " + obstacleposition.ToString());
		//and instantiate it
		GameObject.Find("voxels").GetComponent<VoxelDriver>().loadobstacle(obstacleposition, obstaclename);
		Debug.Log("Obstacle successfully loaded");
	}

	//saves the current tilemap to a text file in assets for editing
	void backendsavemap()
	{
		tilemap = GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap; //copy the current tilemap

		//*******************SAVE TYPE MAP***************************
		string[] lines = new string[tilemap.GetLength(0)*2]; //create an array of strings, one for each line
		Material currenttilematerial;
		for(int i=0; i<lines.Length/2; i++) //for every line
		{
			for(int j=0; j<tilemap.GetLength(1); j++) //for every tile in that row
			{
				if(tilemap[i,j] != null && tilemap[i,j].GetComponent<Tile>() != null)
				{
					//append a text symbol representing its type to the appropriate line
					currenttilematerial = tilemap[i,j].GetComponent<Tile>().gettype();
					switch(currenttilematerial.ToString())
					{
					case "dirt (UnityEngine.Material)":
						lines[i*2] += "*";
						break;
					case "grass (UnityEngine.Material)":
						lines[i*2] += "`";
						break;
					case "sand (UnityEngine.Material)":
						lines[i*2] += ".";
						break;
					case "stone (UnityEngine.Material)":
						lines[i*2] += "#";
						break;
					default:
						Debug.Log ("Error determining terrain type when saving. " + currenttilematerial.ToString());
						break;
					}
				}
				else
				{
					lines[i*2] += "N";
				}
				lines[i*2] += "  ";
			}
		}
		//save a list of all props and obstacles, separated by spaces, on the second to last line
		lines[lines.Length-1] += "PROPS ";
		for(int i=0; i<GameObject.Find ("voxels").GetComponent<VoxelDriver>().props.Count; i++)
		{
			Debug.Log (GameObject.Find("voxels").GetComponent<VoxelDriver>().props[i].name);
			lines[lines.Length-1] += GameObject.Find("voxels").GetComponent<VoxelDriver>().props[i].name
				+ GameObject.Find("voxels").GetComponent<VoxelDriver>().props[i].transform.position.ToString()
				+ " ";
		}
		lines[lines.Length-1] += "OBSTACLES ";
		for(int i=0; i<GameObject.Find ("voxels").GetComponent<VoxelDriver>().obstacles.Count; i++)
		{
			lines[lines.Length-1] += GameObject.Find("voxels").GetComponent<VoxelDriver>().obstacles[i].name
				+ GameObject.Find("voxels").GetComponent<VoxelDriver>().obstacles[i].transform.position.ToString()
				+ " ";
		}
		File.WriteAllLines(@"Assets\TextMaps\temp_typemap.txt", lines); //write the array of strings to test file and close it
	
		//*******************SAVE HEIGHT MAP***************************
		lines = new string[tilemap.GetLength(0)*2]; //clear lines[]
		int currenttileheight;
		for(int i=0; i<lines.Length/2; i++) //for every line
		{
			for(int j=0; j<tilemap.GetLength(1); j++) //for every tile in that row
			{
				if(tilemap[i,j] != null && tilemap[i,j].GetComponent<Tile>() != null)
				{
					//append integer height to the appropriate line
					currenttileheight = (int)(tilemap[i,j].GetComponent<Tile>().getheight(true));
					lines[i*2] += currenttileheight.ToString();
					if(currenttileheight < 10)
						lines[i*2] += " ";
					lines[i*2] += " ";
				}
				else
				{
					lines[i*2] += "N  ";
				}
			}
		}
		File.WriteAllLines(@"Assets\TextMaps\temp_heightmap.txt", lines); //write the array of strings to test file and close it
		Debug.Log ("The map has been written to the temp text files.");
	}

	//loads the saved text file into tilemap and returns it
	public void backendloadmap()
	{
		string lines; //holds the current line in memory
		StreamReader file = new StreamReader(@"Assets\TextMaps\temp_typemap.txt"); //open the typemap file

		//find the dimensions of the tilemap
		int tilemapwidth = 0; //keeps track of the number of lines
		int tilemaplength = 0; //keeps track of the number of valid characters per line
		while((lines = file.ReadLine()) != null) //for every line
		{
			if(lines.Length > 0) //if the line is not empty
				tilemapwidth++; //increment tilemapwidth
		}
		file.Close();
		file = new StreamReader(@"Assets\TextMaps\temp_typemap.txt"); //restart the stream
		lines = file.ReadLine(); //read the first line
		for(int i=0; i<lines.Length; i++) //for every character
		{
			if(lines[i] != ' ') //if it's not a space
				tilemaplength++; //increment tilemaplength
		}
		//CLEAR ALL TILES SO THEY'RE NOT FLOATING AROUND, THEN REINITIALIZE TILEMAP
		for(int x = 0; x < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap.GetLength(0); x++)
		{
			for(int y = 0; y < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap.GetLength(1); y++)
			{
				GameObject.Find("voxels").GetComponent<VoxelDriver>().destroytile(x,y);
			}
		}
		GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap = new GameObject[tilemapwidth, tilemaplength]; //initialize tilemap
		file.Close ();
		file = new StreamReader(@"Assets\TextMaps\temp_typemap.txt"); //restart the stream
		//and fill the tilemap
		int linenumber = 0;
		//ERROR GETTING THE SUBSTRING TO CHECK IF REACHED LAST LINE
		while((lines = file.ReadLine()) != null && !(lines.Length >= 5 && lines.Substring(0,5).Equals("PROPS"))) //for every line
		{
			if(lines.Length > 0) //if the line is not empty
			{
				int j = 0; //i, but discounting spaces
				for(int i=0; i<lines.Length; i++) //for every character
				{
					if(lines[i] != ' ') //if it's not a space
					{
						//fill the tile with the correct terrain type
						switch(lines[i])
						{
						case '*':
							GameObject.Find("voxels").GetComponent<VoxelDriver>().filltile(linenumber,j,hextypes.dirt,1);
							break;	
						case '`':
							GameObject.Find("voxels").GetComponent<VoxelDriver>().filltile(linenumber,j,hextypes.grass,1);
							break;
						case '.':
							GameObject.Find("voxels").GetComponent<VoxelDriver>().filltile(linenumber,j,hextypes.sand,1);
							break;
						case '#':
							GameObject.Find("voxels").GetComponent<VoxelDriver>().filltile(linenumber,j,hextypes.stone,1);
							break;
						case 'N':
							break;
						default:
							Debug.Log ("Error determining terrain type when loading.");
							break;
						}
						j++;
					}
				}
				linenumber++;
			}
		}
		//then, set the appropriate height
		file = new StreamReader(@"Assets\TextMaps\temp_heightmap.txt"); //set the stream to heightmap
		//and set the heights of the tilemap

		linenumber = 0;
		while((lines = file.ReadLine()) != null) //for every line
		{
			if(lines.Length > 0) //if the line is not empty
			{
				int j = 0; //i, but discounting spaces
				bool spaces = false; //keeps track of whether streaming through spaces or other chars
				string numberchars = ""; //contains the string for the number
				for(int i=0; i<lines.Length; i++) //for every character
				{
					if(lines[i] == ' ') //if it's a space
					{
						if(spaces == false)
						{
							spaces = true;
							if(GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap[linenumber,j] != null)
							{
								//set the height according to the number in the text file
								if(numberchars.Length > 0)
									GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[linenumber,j].GetComponent<Tile>().setheight(int.Parse(numberchars));
							}
							numberchars = ""; //reset numberchars
						}
					}
					else //if it isn't a space
					{
						if(spaces == true)
						{
							spaces = false;
							j++;
						}
						if(lines[i] != 'N')
						{
							numberchars += lines[i]; //add the last character to numberchars
						}
					}
				}
				linenumber++;
			}
		}
	}
}
