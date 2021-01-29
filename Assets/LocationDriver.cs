using UnityEngine;
using System.Collections;

public class LocationDriver : MonoBehaviour {

	bool movingtopivot = false; //flag for if moving to new pivot
	Vector3 goalpivot; //goal location for the pivot

	float smoothtime = 1f;
	Vector3 velocity = Vector3.zero;


	bool onedge = false; //flag for if cursor on edge of screen
	const float CAMERADEVIATION = .001f;
	const int SCREENEDGEPROPORTION = 50; //proportion of the screen's width to the edge width

	//GameObject testsphere;

	void Start()
	{
		gameObject.transform.position = GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter].transform.position; //set the initial position of the pivot at the active player character
		setpivot(this.gameObject.transform.position); //initialize the pivot

		//testsphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		//testsphere.transform.localPosition = Vector3.zero;
		//testsphere.transform.SetParent(this.gameObject.transform);
	}

	void FixedUpdate()
	{
		//Debug.Log(.6f+.01f*Vector3.Distance(transform.position,GameObject.FindWithTag("MainCamera").transform.position));
		//if the mouse is towards the edge of the screen, move the pivot in that direction
		onedge = false;
		if(Input.mousePosition[0] < SCREENEDGEPROPORTION)
		{
			//get the rotation-dependent forward
			Vector3 relativeleft = Vector3.Cross(new Vector3
				(transform.position.x-GameObject.FindWithTag("MainCamera").transform.position.x
				 ,0f,transform.position.z-GameObject.FindWithTag("MainCamera").transform.position.z)
			                                     ,Vector3.up);
			Vector3.Normalize(relativeleft);
			setpivot (transform.position+(relativeleft
			                              *(.6f+.01f*Vector3.Distance(transform.position,GameObject.FindWithTag("MainCamera").transform.position))
			                              ));
			onedge = true;
		}
		else if(Input.mousePosition[0] > Screen.width-SCREENEDGEPROPORTION)
		{
			Vector3 relativeleft = Vector3.Cross(new Vector3
			                                     (transform.position.x-GameObject.FindWithTag("MainCamera").transform.position.x
			 									,0f,transform.position.z-GameObject.FindWithTag("MainCamera").transform.position.z)
			                                    ,Vector3.up);
			Vector3.Normalize(relativeleft);
			setpivot (transform.position-(relativeleft
			                              *(.6f+.01f*Vector3.Distance(transform.position,GameObject.FindWithTag("MainCamera").transform.position))
			                              ));

			onedge = true;
		}
		if(Input.mousePosition[1] < SCREENEDGEPROPORTION)
		{
			//get the rotation-dependent forward
			Vector3 relativeforward = new Vector3
				(transform.position.x-GameObject.FindWithTag("MainCamera").transform.position.x
				 ,0f,transform.position.z-GameObject.FindWithTag("MainCamera").transform.position.z);
			Vector3.Normalize(relativeforward);
			setpivot (transform.position-(relativeforward
			                              *(.6f+.01f*Vector3.Distance(transform.position,GameObject.FindWithTag("MainCamera").transform.position))
			                              ));
			onedge = true;
		}
		else if(Input.mousePosition[1] > Screen.height-SCREENEDGEPROPORTION)
		{
			//get the rotation-dependent forward
			Vector3 relativeforward = new Vector3
				(transform.position.x-GameObject.FindWithTag("MainCamera").transform.position.x
				 ,0f,transform.position.z-GameObject.FindWithTag("MainCamera").transform.position.z);
			Vector3.Normalize(relativeforward);
			setpivot (transform.position+(relativeforward
			          *(.6f+.01f*Vector3.Distance(transform.position,GameObject.FindWithTag("MainCamera").transform.position))
			          ));
			onedge = true;
		}

		//update movement and pivot if not on the edge
		if(movingtopivot == true)
		{
			if(onedge == false)
				setpivot (Vector3.MoveTowards(goalpivot,transform.position,.05f));
			movetopivot();
		}

	}

	public void setpivot(Vector3 newpivot)
	{
		//Debug.Log(newpivot);
		goalpivot = newpivot;
		movingtopivot = true;
	}

	//moves the pivot towards the goal
	void movetopivot()
	{
		if(Vector3.Distance(transform.position, goalpivot) < CAMERADEVIATION) //if the camera has moved to the pivot
		{
			movingtopivot = false;
			return;
		}
		else
		{
			transform.position = Vector3.SmoothDamp(transform.position, goalpivot, ref velocity, smoothtime);
		}
	}
}
