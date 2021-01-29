using UnityEngine;
using System.Collections;

public class Grasstuft : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Material material = Resources.Load (@"Materials/"
		                                    +GameObject.Find("voxels").GetComponent<VoxelDriver>().biome
		                                    +"/grass", typeof(Material)) as Material;
		gameObject.GetComponent<Renderer>().material = material;
	}
}
