using UnityEngine;
using System.Collections;

public class PivotDriver : MonoBehaviour {
	
	bool rotatingaroundpivot = false; //flag for rotating around pivot
	float rotationalvelocity = 0f; //controls rotational velocity
	bool rotationalaccelerationincreasing = true; //flag for if rotational velocity is increasing or decreasing
	int rotationdirection; //1 if clockwise, -1 if counterclockwise
	public const float ROTATIONALACCELERATION = 3f; //controls rotational acceleration
	public const float MAXROTATIONSPEED = 125f; //max speed of rotation
	public const float ROTATIONALDEVIATION = 1f;
	

	void Start()
	{
		transform.localPosition = Vector3.zero;
	}

	void FixedUpdate()
	{
		//Debug.Log ("Moving To: " + goalpivot);
		//input
		if(Input.GetButton("Horizontal"))
		{
			if(Input.GetAxisRaw("Horizontal") == 1)
				rotationdirection = 1;
			else
				rotationdirection = -1;
			if(rotatingaroundpivot == false)
			{
				rotatingaroundpivot = true;
				rotationalvelocity = ROTATIONALDEVIATION;
			}
			 rotationalaccelerationincreasing= true;
		}



		//update rotation
		if(rotatingaroundpivot == true)
		{
			rotatearoundpivot();	
		}

	}

	//rotates the camera around the pivot
	void rotatearoundpivot()
    {
		//Debug.Log ("ROTATING");
		if(rotationalvelocity < ROTATIONALDEVIATION)
		{
			rotatingaroundpivot = false;
			rotationalvelocity = 0f;
			return;
		}
		else
		{
			//rotate
			transform.Rotate(new Vector3(0f, -rotationdirection*rotationalvelocity*Time.deltaTime, 0f), Space.World);

			//then update rotational velocity
			if(rotationalaccelerationincreasing == true && Input.GetButton("Horizontal"))
			{
				if(Mathf.Abs(rotationalvelocity) > MAXROTATIONSPEED)
				{
					if(rotationalvelocity > 0)
						rotationalvelocity = MAXROTATIONSPEED;
					else
						rotationalvelocity = -MAXROTATIONSPEED;
					rotationalaccelerationincreasing = false;
				}
				else
				{
					rotationalvelocity += ROTATIONALACCELERATION;
				}
			}
			else
			{
				rotationalvelocity -= ROTATIONALACCELERATION;
			}
		}
	}

}
