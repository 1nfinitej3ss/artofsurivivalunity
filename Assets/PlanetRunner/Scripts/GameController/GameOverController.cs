using UnityEngine;
using System.Collections;

namespace PlanetRunner {
	public class GameOverController : MonoBehaviour {

		private GameController gameController;

		void Awake() {
			GameObject go = GameObject.FindGameObjectWithTag (Const.GAMECONTROLLER);

			if (go != null) {
				gameController = go.GetComponent<GameController> ();

			} else {
				Debug.Log ("GameOverController: GameController not found");
			}
		}

		void OnTriggerEnter2D(Collider2D col) {

			// This script and function is of no use in our scene.
			// The only explanation I can think of is that this was made for some 2d platformer game to check the deathZone.
			// I wouldn't delete this as MAYBE it could open some errors in the console, so just leave it as it is.
			if (col.gameObject.tag.Equals (Const.PLAYER)) {
				gameController.DoSetGameOverLevel ();
			}
		}
	}

}
