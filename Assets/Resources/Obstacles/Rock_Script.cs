using UnityEngine;
using System.Collections;

public class Rock_Script : MonoBehaviour {

	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Renderer>().material = Resources.Load(@"Materials/"
		                                                              +GameObject.Find("voxels").GetComponent<VoxelDriver>().biome
		                                                              +"/stone", typeof(Material)) as Material;
		MeshCollider collider = (MeshCollider)gameObject.AddComponent(typeof(MeshCollider));
		gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
	}
}
