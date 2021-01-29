using UnityEngine;
using System.Collections;

public class TiltDriver : MonoBehaviour {

	bool tiltingaroundpivot = false; //flag for tilting around pivot
	float tiltvelocity = 0f; //controls tilt velocity
	bool tiltaccelerationincreasing = true; //flag for if tilt velocity is increasing or decreasing
	int tiltdirection; //1 if forward, -1 if backward

	const float SPEED = 50f; //tilt speed

	void Start()
	{
		transform.localRotation = Quaternion.identity; //initialize rotation
	}

	/*
	void FixedUpdate ()
	{
		if(GameObject.FindWithTag("MainCamera").transform.position.y >= 0)
		{
			if(Input.GetButton("Vertical"))
			{
				if(Input.GetAxisRaw("Vertical") == 1)
					tiltdirection = 1;
				else
					tiltdirection = -1;
				if(tiltingaroundpivot == false)
				{
					tiltingaroundpivot = true;
					tiltvelocity = PivotDriver.ROTATIONALDEVIATION;
				}
				tiltaccelerationincreasing = true;
			}
		}
		if(tiltingaroundpivot == true)
		{
			tiltaroundpivot();
		}

	}
	*/

	void FixedUpdate()
	{
		if(Input.GetButton("Vertical"))
		{
			if(Input.GetAxisRaw("Vertical") == 1 && transform.localEulerAngles.y < 90f)
				transform.Rotate(SPEED*Time.deltaTime, 0f, 0f);
			else if (Input.GetAxisRaw("Vertical") == -1 && transform.localEulerAngles.x < 180f)
				transform.Rotate(-1*SPEED*Time.deltaTime, 0f, 0f);
		}
	}

	//tilts the camera around the pivot
	void tiltaroundpivot()
	{
		if(tiltvelocity < PivotDriver.ROTATIONALDEVIATION)
		{
			tiltingaroundpivot = false;
			tiltvelocity = 0f;
			return;
		}
		else
		{
			//bound the minimum angle of the camera to the ground
			if(GameObject.FindWithTag("MainCamera").transform.position.y < 0)
			{
				Debug.Log("LOWER BOUNDS");
				tiltdirection = 1;
				tiltvelocity = 10f;
				transform.Rotate(transform.TransformVector(new Vector3(.2f,0f,0f)), Space.World);
			}
			//bound the maximum angle of the camera to the apex
			else if(GameObject.FindWithTag("MainCamera").transform.position.y-transform.position.y > 9.9f)
			{
				Debug.Log("UPPER BOUNDS");
				tiltdirection = -1;
				tiltvelocity = 10f;
				transform.Rotate(transform.TransformVector(new Vector3(-.2f,0f,0f)), Space.World);
			}
				
			transform.Rotate(transform.TransformVector(new Vector3(tiltdirection*tiltvelocity*Time.deltaTime,0f, 0f)), Space.World);
			//then update tilt velocity
			if(tiltaccelerationincreasing == true && Input.GetButton("Vertical"))
			{
				if(Mathf.Abs(tiltvelocity) > PivotDriver.MAXROTATIONSPEED)
				{
					if(tiltvelocity > 0)
						tiltvelocity = PivotDriver.MAXROTATIONSPEED;
					else
						tiltvelocity = -PivotDriver.MAXROTATIONSPEED;
					tiltaccelerationincreasing = false;
				}
				else
				{
					tiltvelocity += PivotDriver.ROTATIONALACCELERATION;
				}
			}
			else
			{
				tiltvelocity -= PivotDriver.ROTATIONALACCELERATION;
			}
		}
	}

}
