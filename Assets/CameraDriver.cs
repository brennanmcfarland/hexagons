using UnityEngine;
using System.Collections;

public class CameraDriver : MonoBehaviour {

	const float MAXZOOM = 2f;
	const float MINZOOM = 20f;
	void Start()
	{
		transform.SetParent(GameObject.FindWithTag("tilt").GetComponent<TiltDriver>().transform, true);
		transform.localPosition = new Vector3(0f,0f,-10f);
	}
	void FixedUpdate()
	{
		if(Input.GetAxisRaw("Zoom") == 1 && Vector3.Distance(transform.position, transform.parent.position) > MAXZOOM)
		{
			transform.position = Vector3.MoveTowards(transform.position, transform.parent.transform.position, .1f);
		}
		else if(Input.GetAxisRaw("Zoom") == -1 && Vector3.Distance(transform.position, transform.parent.position) < MINZOOM)
		{
			transform.position = Vector3.MoveTowards(transform.position, transform.parent.transform.position, -.1f);
		}
	}
	
}
