using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	//these are the scale of the object to fit radius = 1 unity unit
	static float XSCALEUNITS = 0.5773771f;
	static float YSCALEUNITS = 0.1443443f;
	static float ZSCALEUNITS = 0.5773771f;
	static float HEXTOPOFFSET = .08f; //offset for the height of the hexmeshtop mesh for placing things on
	static bool ishighlighted;
	public bool obstacle = false;
	int oldscale = 1;
	public int Xindex;
	public int Yindex;
	public hextypes terraintype;
	GameObject hex;
	GameObject hextop;
	MeshCollider topcollider;
	MeshCollider sidecollider;
	Mesh mesh;
	Vector2[] uvs;
	Material hexmaterial;
	Material highlightmaterial; //material for when an object is highlighted

	public void init(int x, int y, hextypes terraintype, int height)
	{
		Xindex = x;
		Yindex = y;
		this.terraintype = terraintype;
		hex = Instantiate(Resources.Load ("hexmesh"), new Vector3((float)(x+y*.5),0,(float)(y*1.5/Mathf.Sqrt(3))), Quaternion.identity) as GameObject;
		hex.transform.SetParent(GameObject.FindWithTag("tiles").transform);
		//hex = Instantiate(Resources.Load ("hexmesh"), new Vector3((float)(x+(y%2)*.5),0,(float)(y*1.5/Mathf.Sqrt(3))), Quaternion.identity) as GameObject;
		hextop = Instantiate(Resources.Load ("hexmeshtop"), new Vector3(hex.transform.position.x,hex.transform.position.y,hex.transform.position.z), Quaternion.identity) as GameObject;

		topcollider = (MeshCollider)hextop.gameObject.AddComponent(typeof(MeshCollider));
		hextop.GetComponent<MeshCollider>().sharedMesh = hextop.GetComponent<MeshFilter>().mesh;
		sidecollider = (MeshCollider)hex.gameObject.AddComponent(typeof(MeshCollider));
		hex.GetComponent<MeshCollider>().sharedMesh = hex.GetComponent<MeshFilter>().mesh;

		hextop.transform.parent = this.transform;

		mesh = hex.GetComponent<MeshFilter>().mesh;
		uvs = mesh.uv;
		settype (terraintype);
		highlightmaterial = Resources.Load (@"Material/highlight", typeof(Material)) as Material;
		if(height != 1)
			setheight(height);
		gameObject.transform.SetParent(GameObject.FindWithTag("tiles").transform);
	}
	public void setheight(int y)
	{
		//change the height
		hex.transform.localScale = new Vector3(100,100*y,100);
		//hex.transform.localScale = new Vector3(XSCALEUNITS, YSCALEUNITS*y, ZSCALEUNITS);

		//and adjust the uvs
		for(int i=0; i<uvs.Length; i++)
		{
				uvs[i] = Vector2.Scale (mesh.uv[i], new Vector2(y,1));
		}
		mesh.uv = uvs;
		hextop.transform.Translate(0f, (float)((y-oldscale)*2*YSCALEUNITS), 0f);

		oldscale = y;
	}
	public float getheight(bool units)
	{
		if(units == false)
			return oldscale*YSCALEUNITS*2+HEXTOPOFFSET;
		else
			return (float)oldscale;
	}
	public Vector3 getposition()
	{
		return new Vector3(hextop.transform.position.x,hextop.transform.position.y+HEXTOPOFFSET+2*YSCALEUNITS,hextop.transform.position.z);
	}
	public void settype(hextypes terraintype)
	{
		//Debug.Log(@"Materials/"
		//          +GameObject.Find("voxels").GetComponent<VoxelDriver>().biome
		//          +"/grass");
		          switch(terraintype)
		          {
		case hextypes.grass:
			hexmaterial = Resources.Load (@"Materials/"
			                              +GameObject.Find("voxels").GetComponent<VoxelDriver>().biome
			                              +"/grass", typeof(Material)) as Material;
			break;
		case hextypes.dirt:
			hexmaterial = Resources.Load (@"Materials/"
			                              +GameObject.Find("voxels").GetComponent<VoxelDriver>().biome
			                              +"/dirt", typeof(Material)) as Material;
			break;
		case hextypes.stone:
			hexmaterial = Resources.Load (@"Materials/"
			                              +GameObject.Find("voxels").GetComponent<VoxelDriver>().biome
			                              +"/stone", typeof(Material)) as Material;
			break;
		case hextypes.sand:
			hexmaterial = Resources.Load(@"Materials/"
			                              +GameObject.Find("voxels").GetComponent<VoxelDriver>().biome
			                              +"/sand", typeof(Material)) as Material;
			break;
		default:
			Debug.Log ("Error determining terrain type.");
			break;
		}
		hex.GetComponent<Renderer>().material = hexmaterial;
		hextop.GetComponent<Renderer>().material = hexmaterial;
	}
	public Material gettype()
	{
		return hexmaterial;
	}
	public void reset()
	{
		Debug.Log("DESTROY");
		Destroy (hex);
		Destroy(hextop);
	}
	public void changeifobstacle (bool o)
	{
        if (Xindex==8 && Yindex==9 && o == true && obstacle == false)
            Debug.Log("set to obstacle");
        if (Xindex == 8 && Yindex == 9 && o == false && obstacle == true)
            Debug.Log("unset from obstacle");
		obstacle = o;
	}
	public bool getifobstacle()
	{
		return obstacle;
	}
}

/* TODO:
 * 	mess around with the texture color, make it less garishly bright/a little more realistic color-wise
 * 	add color variation, maybe randomize it when the tile is spawned from the script?  yeah try that and
 * 	see if it works, maybe across tiles, but that may be too complicated/processor heavy
 * 	and of course make it dirt on the sides
 * 	and add other materials: dirt, rocks, desert/sand, etc, this can be fun
 * */
