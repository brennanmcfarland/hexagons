using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Serialization_old : MonoBehaviour {

	public GameObject[,] tilemap; //VoxelDriver's tilemap is loaded into this for serialization

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

		//generate a random string for the temporary map file name until it doesn't match a current map file name
		Random.seed = (int)Time.time;
		string tempmapfilename;
		do{
			tempmapfilename = Random.value.ToString();
		}while(File.Exists(Application.persistentDataPath+"/maps/"+tempmapfilename));
		//save the temporary map file
		savemap(tempmapfilename);






		//and copy it into a new save file with the given name, deleting the temporary map file
		File.Copy(Application.persistentDataPath+"/maps/"+tempmapfilename+".mapsave",
			Application.persistentDataPath+"/saves/"+name+".save",true);
		File.Delete(Application.persistentDataPath+"/maps/"+tempmapfilename+".mapsave");

		//then create the streams
		FileStream file = File.Open(Application.persistentDataPath+"/saves/"+name+".save",FileMode.Append);
		BinaryFormatter serializer = new BinaryFormatter();
		//serialize " SESSIONDATA " to indicate the end of the map part of the file
		serializer.Serialize(file, " SESSIONDATA ");

		//SERIALIZE ACTIVECHARACTER
		serializer.Serialize(file, GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter);

		//SERIALIZE THE CHARACTERS
		for(int i=0; i<GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters.Count; i++)
		{
			serializer.Serialize(file, GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[i].GetComponent<Actor>().serializecharacter());
		}

		file.Close();
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

		//otherwise, set up the binary formatter and open the file
		BinaryFormatter deserializer = new BinaryFormatter();
		FileStream file = File.OpenRead(Application.persistentDataPath + "/saves/"+name+".save");

		//DESERIALIZE THE MAP
		//generate a random string for the temporary map file name until it doesn't match a current map file name
		Random.seed = (int)Time.time;
		string tempmapfilename;
		do{
			tempmapfilename = Random.value.ToString();
		}while(File.Exists(Application.persistentDataPath+"/maps/"+tempmapfilename+".mapsave"));
		//save the temporary map file via a temporary stream, making sure the proper directory exists
		if(!(Directory.Exists(Application.persistentDataPath+"/maps")))
			Directory.CreateDirectory(Application.persistentDataPath+"/maps");
		BinaryFormatter tempserializer = new BinaryFormatter();
		FileStream tempfile = File.Open(Application.persistentDataPath + "/maps/"+tempmapfilename+".mapsave",FileMode.CreateNew);
		//BinaryFormatter flagdeserializer = new BinaryFormatter(); //test for the flag to stop loading the map
		//FileStream tempfileflag = File.Open(Application.persistentDataPath + "/maps/"+tempmapfilename+".mapsave",FileMode.Open);
		while(true)
		{
			object mapdatum = deserializer.Deserialize(file);
			if(mapdatum.Equals(" SESSIONDATA "))
			{
				Debug.Log("TERMINATED");
				tempfile.SetLength(tempfile.Length-13);
				break;
			}
			if(mapdatum.Equals(" obstacles "))
				Debug.Log("OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
			tempserializer.Serialize(tempfile,mapdatum);
			Debug.Log("||"+mapdatum+"||");
		}
		tempfile.Close();
		//TODO: OBSTACLES IS PRESENT AS OF THIS POINT IN TEMPFILE
		//TODO: OBSTACLES IS MISSING AS OF THIS POINT

		//BinaryFormatter testdeserializer = new BinaryFormatter();
		//FileStream testfile = File.Open(Application.persistentDataPath + "/maps/"+tempmapfilename+".mapsave",FileMode.Open);
		//while(true)
		//{
		//	object mapdatum = testdeserializer.Deserialize(testfile);
		//	Debug.Log(mapdatum);
		//	if(mapdatum.Equals(" obstacles "))
		//		Debug.Log("HHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH");
		//}
		//TEST
		//FileStream filetest = File.Open(Application.persistentDataPath+"/maps/"+tempmapfilename+".mapsave",FileMode.Open);
		//BinaryFormatter deserializertest = new BinaryFormatter();
		//while(true)
		//{
		//	if(deserializertest.Deserialize(filetest) == " obstacles ")
		//		Debug.Log("OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
		//}
		//Debug.Log("OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO" + stringtest + "OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
		//END TEST


		//Debug.Log(": " + deserializer.Deserialize(file));
		//Debug.Log(": " + deserializer.Deserialize(file));
		//Debug.Log(": " + deserializer.Deserialize(file));
		loadmap(tempmapfilename);
		File.Delete(Application.persistentDataPath+"/maps/"+tempmapfilename+".mapsave");

		//DESERIALIZE ACTIVECHARACTER
		GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter = (int)deserializer.Deserialize(file);
		savedata loadedactor;

		//DESERIALIZE THE CHARACTERS
		while(file.Position < file.Length) //until we get to the end of the file
		{
			loadedactor = (savedata)deserializer.Deserialize(file);
			GameObject.FindWithTag("Player").GetComponent<ActorDriver>().spawncharacter(loadedactor.ID,loadedactor.position[0],loadedactor.position[1],loadedactor.isplayercharacter);
		}

		file.Close();
		Debug.Log("Play session loaded");
	}
	//saves the map to a binary file in the persistent data path, overwrites if one already exists
	void savemap_new(string name)
	{
		//test to see if the proper directory structure exists and create it if it doesn't
		if(!(Directory.Exists(Application.persistentDataPath+"/maps")))
			Directory.CreateDirectory(Application.persistentDataPath+"/maps");

		tilemap = GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap; //copy the current tilemap
		BinaryFormatter serializer = new BinaryFormatter();
		FileStream file = File.Open(Application.persistentDataPath + "/maps/"+name+".mapsave",FileMode.Create);
		//creates a binary stream and creates or overwrites the save file
		//SAVE THE DIMENSIONS OF TILEMAP
		//serializer.Serialize(file,tilemap.GetLength(0));
		//serializer.Serialize(file,tilemap.GetLength(1));
		Hashtable data = new Hashtable();
		//data.Add(tilemap.GetLength(0));
		//data.Add(tilemap.GetLength(1));

		//SAVE THE MATERIAL OF EACH TILE
		Material currenttilematerial;
		for(int i=0; i<tilemap.GetLength(0); i++) //for every row
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
						serializer.Serialize(file,'*');
						break;
					case "grass (UnityEngine.Material)":
						serializer.Serialize(file,'`');
						break;
					case "sand (UnityEngine.Material)":
						serializer.Serialize(file,'.');
						break;
					case "stone (UnityEngine.Material)":
						serializer.Serialize(file,'#');
						break;
					default:
						Debug.Log ("Error determining terrain type when saving. " + currenttilematerial.ToString());
						break;
					}
				}
				else
				{
					serializer.Serialize(file,'N');
				}
			}
		}
		//SAVE THE HEIGHT OF EACH TILE (and if it's obstructed)
		for(int i=0; i<tilemap.GetLength(0); i++) //for every line
		{
			for(int j=0; j<tilemap.GetLength(1); j++) //for every tile in that row
			{
				if(tilemap[i,j] != null && tilemap[i,j].GetComponent<Tile>() != null)
				{
					serializer.Serialize(file,(int)tilemap[i,j].GetComponent<Tile>().getheight(true));
					//serializer.Serialize(file,' ');
					serializer.Serialize(file,tilemap[i,j].GetComponent<Tile>().getifobstacle());
				}
				else
				{
					//must be an integer or it will get confused with terrain type
					serializer.Serialize(file,-1);
					serializer.Serialize(file,false);
				}
			}
		}

		//serialize a space to signal the end of the map and the beginning of props
		serializer.Serialize(file,"[props]");

		//save a list of all props and obstacles, separated by spaces
		for(int i=0; i<GameObject.Find ("voxels").GetComponent<VoxelDriver>().props.Count; i++)
		{
			serializer.Serialize(file,GameObject.Find("voxels").GetComponent<VoxelDriver>().props[i].name + GameObject.Find("voxels").GetComponent<VoxelDriver>().props[i].transform.position);
		}
		serializer.Serialize(file,"[obstacles]");
		for(int i=0; i<GameObject.Find ("voxels").GetComponent<VoxelDriver>().obstacles.Count; i++)
		{
			serializer.Serialize(file,GameObject.Find("voxels").GetComponent<VoxelDriver>().obstacles[i].name + GameObject.Find("voxels").GetComponent<VoxelDriver>().obstacles[i].transform.position);
		}

		file.Close();
		Debug.Log ("Map saved");

	}

	void savemap(string name)
	{
		//test to see if the proper directory structure exists and create it if it doesn't
		if(!(Directory.Exists(Application.persistentDataPath+"/maps")))
			Directory.CreateDirectory(Application.persistentDataPath+"/maps");

		tilemap = GameObject.Find ("voxels").GetComponent<VoxelDriver>().tilemap; //copy the current tilemap
		BinaryFormatter serializer = new BinaryFormatter();
		FileStream file = File.Open(Application.persistentDataPath + "/maps/"+name+".mapsave",FileMode.Create);
		//creates a binary stream and creates or overwrites the save file
		//SAVE THE DIMENSIONS OF TILEMAP
		serializer.Serialize(file,tilemap.GetLength(0));
		serializer.Serialize(file,tilemap.GetLength(1));

		//SAVE THE MATERIAL OF EACH TILE
		Material currenttilematerial;
		for(int i=0; i<tilemap.GetLength(0); i++) //for every row
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
						serializer.Serialize(file,'*');
						break;
					case "grass (UnityEngine.Material)":
						serializer.Serialize(file,'`');
						break;
					case "sand (UnityEngine.Material)":
						serializer.Serialize(file,'.');
						break;
					case "stone (UnityEngine.Material)":
						serializer.Serialize(file,'#');
						break;
					default:
						Debug.Log ("Error determining terrain type when saving. " + currenttilematerial.ToString());
						break;
					}
				}
				else
				{
					serializer.Serialize(file,'N');
				}
			}
		}
		//SAVE THE HEIGHT OF EACH TILE (and if it's obstructed)
		for(int i=0; i<tilemap.GetLength(0); i++) //for every line
		{
			for(int j=0; j<tilemap.GetLength(1); j++) //for every tile in that row
			{
				if(tilemap[i,j] != null && tilemap[i,j].GetComponent<Tile>() != null)
				{
					serializer.Serialize(file,(int)tilemap[i,j].GetComponent<Tile>().getheight(true));
					//serializer.Serialize(file,' ');
					serializer.Serialize(file,tilemap[i,j].GetComponent<Tile>().getifobstacle());
				}
				else
				{
					//must be an integer or it will get confused with terrain type
					serializer.Serialize(file,-1);
					serializer.Serialize(file,false);
				}
			}
		}

		//serialize a space to signal the end of the map and the beginning of props
		serializer.Serialize(file,"[props]");

		//save a list of all props and obstacles, separated by spaces
		for(int i=0; i<GameObject.Find ("voxels").GetComponent<VoxelDriver>().props.Count; i++)
		{
			serializer.Serialize(file,GameObject.Find("voxels").GetComponent<VoxelDriver>().props[i].name + GameObject.Find("voxels").GetComponent<VoxelDriver>().props[i].transform.position);
		}
		serializer.Serialize(file,"[obstacles]");
		for(int i=0; i<GameObject.Find ("voxels").GetComponent<VoxelDriver>().obstacles.Count; i++)
		{
			serializer.Serialize(file,GameObject.Find("voxels").GetComponent<VoxelDriver>().obstacles[i].name + GameObject.Find("voxels").GetComponent<VoxelDriver>().obstacles[i].transform.position);
		}

		file.Close();
		Debug.Log ("Map saved");

	}

	public void loadmap(string name)
	{
		//exit if the file of the specified name is not found and print an error message
		if(!File.Exists(Application.persistentDataPath + "/maps/"+name+".mapsave"))
		{
			Debug.Log("Error: file " + name + "not found");
			return;
		}

		//otherwise, set up the binary formatter and open the file
		BinaryFormatter deserializer = new BinaryFormatter();
		FileStream file = File.OpenRead(Application.persistentDataPath + "/maps/"+name+".mapsave");

		//LOAD THE DIMENSIONS OF THE TILEMAP
		GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth = (int)deserializer.Deserialize(file);
		GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemaplength = (int)deserializer.Deserialize(file);
		//GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap = new GameObject[GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth,GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemaplength];

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

		//LOAD THE MATERIAL OF EACH TILE
		for(int x = 0; x < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth; x++)
		{
			for(int y = 0; y < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemaplength; y++)
			{
				switch((char)deserializer.Deserialize(file))
				{
				case 'N':
					GameObject.Find("voxels").GetComponent<VoxelDriver>().destroytile(x,y);
					break;
				case '*':
					GameObject.Find("voxels").GetComponent<VoxelDriver>().filltile(x,y,hextypes.dirt,1);
					break;
				case '`':
					GameObject.Find("voxels").GetComponent<VoxelDriver>().filltile(x,y,hextypes.grass,1);
					break;
				case '.':
					GameObject.Find("voxels").GetComponent<VoxelDriver>().filltile(x,y,hextypes.sand,1);
					break;
				case '#':
					GameObject.Find("voxels").GetComponent<VoxelDriver>().filltile(x,y,hextypes.stone,1);
					break;
				default:
					Debug.Log("Error determining terrain type when loading.");
					break;
				}
			}
		}
		//LOAD THE HEIGHT OF EACH TILE
		for(int x = 0; x < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemapwidth; x++)
		{
			for(int y = 0; y < GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemaplength; y++)
			{
				//does it have to do with N?
				//Debug.Log("Deserialized " + deserializer.Deserialize(file).GetType());
				int temp = (int)deserializer.Deserialize(file);
				if(temp != -1)
				{
					Debug.Log("Tile is" + temp + ", temp is " + temp.ToString());
					GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x,y].GetComponent<Tile>().setheight(temp);
					//load if that tile's an obstacle
					if((bool)deserializer.Deserialize(file) == true)
						GameObject.Find("voxels").GetComponent<VoxelDriver>().tilemap[x,y].GetComponent<Tile>().changeifobstacle(true);
				}
			}
		}

		//deserialize the space
		Debug.Log("the space should be: |" + deserializer.Deserialize(file) + "|");

		// load the list of all props, separated by spaces
		string prop = (file.Position < file.Length) ? (string)deserializer.Deserialize(file) : string.Empty;  //parse the deserialized string into the object name and position
		string propname;
		string proppositionstring;
		Vector3 propposition;
		while(!prop.Equals("[obstacles]"))
		{
			Debug.Log("PROP IS " +prop);
			propname = prop.Substring(0,prop.IndexOf('('));
			Debug.Log("Name: " + propname);
			proppositionstring = prop.Substring(propname.Length+8).TrimEnd(')');
			string[] temp = proppositionstring.Split(new[]{' ',','},4);
			propposition = new Vector3(float.Parse(temp[0]),float.Parse(temp[2]),float.Parse(temp[3].TrimStart(' ')));
			Debug.Log(propposition.ToString());
			//and instantiate it
			GameObject.Find("voxels").GetComponent<VoxelDriver>().loadprop(propposition, propname);

			prop = (string)deserializer.Deserialize(file); //get next prop
		}

		//load the list of all obstacles, separated by spaces
		/*string obstacle;
		string obstaclename;
		string obstaclepositionstring;
		Vector3 obstacleposition;
		Debug.Log("File length is " + file.Length.ToString());
		Debug.Log("Initial file position is " + file.Position.ToString());
		while(file.Position < file.Length) //until we get to the end of the file
		{
			Debug.Log(file.Position + ", " + file.Length);
			//parse the deserialized string into the object name and position
			obstacle = (string)deserializer.Deserialize(file); //here's where it's getting stuck, for some reason
			obstaclename = obstacle.Substring(0,obstacle.IndexOf('('));
			Debug.Log("Name: " + obstaclename);
			obstaclepositionstring = obstacle.Substring(obstaclename.Length+8).TrimEnd(')');
			string[] temp = obstaclepositionstring.Split(new[]{' ',','},4);
			obstacleposition = new Vector3(float.Parse(temp[0]),float.Parse(temp[2]),float.Parse(temp[3].TrimStart(' ')));
			Debug.Log(obstacleposition.ToString());
			//and instantiate it
			GameObject.Find("voxels").GetComponent<VoxelDriver>().loadobstacle(obstacleposition, obstaclename);
			//TODO: need to get it to work for multiple tile obstacles also, somehow preserve that data or figure it out
		
			Debug.Log("New file position is " + file.Position.ToString());
		}
*/
		var dataItem = deserializer.Deserialize(file);
		while(dataItem != null && file.Position < file.Length){
			loadObstacle((string)dataItem);
			dataItem = deserializer.Deserialize(file);
			Debug.Log("Next obstacle data item is: " + dataItem.ToString());
		}

		file.Close();
		Debug.Log ("Map loaded");
	}

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
