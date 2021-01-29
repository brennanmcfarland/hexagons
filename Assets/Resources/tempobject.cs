using UnityEngine;
using System.Collections;

//a temporary test object component, destroys itself after 30 frames
public class tempobject : MonoBehaviour {

	int persistence = 30;
	
	// Update is called once per frame
	void Update () {
		if(persistence == 0)
			GameObject.Destroy(gameObject);
		persistence--;
	}
}
