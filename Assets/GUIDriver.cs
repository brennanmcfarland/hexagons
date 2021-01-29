using UnityEngine;
using System.Collections;

public enum cursortypes {arrow, loading, attack, useability};

public class GUIDriver : MonoBehaviour {
	
	public GameObject tilehighlight;

	// Use this for initialization
	void Start () {
		//instantiate the tile highlight and set to hidden
		tilehighlight = Instantiate(Resources.Load ("hexmeshhighlight"), Vector3.zero, Quaternion.identity) as GameObject;
		tilehighlight.GetComponent<Renderer>().material = Resources.Load(@"Materials/biome_1/grass", typeof(Material)) as Material;
		tilehighlight.GetComponent<Renderer>().enabled = false;

		Cursor.visible = true;
		setcursor(cursortypes.arrow);
	}
	
	// Update is called once per frame
	void Update () {
		/*
		//update moving and show or hide the cursor accordingly
		switch(GameObject.FindWithTag("gamestate").GetComponent<GameState>().gamestate)
		{
		case gamestates.idle:
			setcursor(cursortypes.arrow);
			break;
		case gamestates.moving:
			Cursor.visible = false;
			break;
		case gamestates.ability_selected:
			setcursor(cursortypes.arrow);
			break;
		case gamestates.ability_using:
			Cursor.visible = false;
			break;
		default:
			Debug.Log("Error; invalid game state");
			break;
		}*/
	}

	public void showcursor(){Cursor.visible = true;}
	public void hidecursor(){Cursor.visible = false;}

	public void setcursor(cursortypes cursortype)
	{
		switch(cursortype)
		{
		case cursortypes.arrow:
			Cursor.SetCursor(Resources.Load(@"Textures/cursor", typeof(Texture2D)) as Texture2D, Vector2.zero, CursorMode.Auto);
			break;
		case cursortypes.loading:
			Cursor.SetCursor(Resources.Load(@"Textures/cursor_loading", typeof(Texture2D)) as Texture2D, Vector2.zero, CursorMode.Auto);
			break;
		case cursortypes.attack:
			Cursor.SetCursor(Resources.Load(@"Textures/cursor_attack", typeof(Texture2D)) as Texture2D, Vector2.zero, CursorMode.Auto);
			break;
		default:
			Debug.Log ("Error; invalid cursor");
			break;
		}
	}
}
