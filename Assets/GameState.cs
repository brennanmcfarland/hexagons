using UnityEngine;
using System.Collections;

public enum gamestates {idle, moving, ability_selected, ability_using};
/* IDLE: nothing is happening but the game is running
 * MOVING: one of the characters is moving
 * ABILITYSELECTED: the player has selected an ability and will either cancel or use it
 * */

public class GameState : MonoBehaviour {

	public gamestates gamestate = gamestates.idle; //starting game state

}
