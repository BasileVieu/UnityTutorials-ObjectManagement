using UnityEditor;
using UnityEngine;

static class RegisterLevelObjectMenuItem {

	const string menuItem = "GameObject/Register Level Object";

	[MenuItem(menuItem, true)]
	static bool ValidateRegisterLevelObject () {
		if (Selection.objects.Length == 0) {
			return false;
		}
		foreach (Object o in Selection.objects) {
			if (!(o is GameObject)) {
				return false;
			}
		}
		return true;
	}

	[MenuItem(menuItem)]
	static void RegisterLevelObject () {
		if (Selection.objects.Length == 0) {
			Debug.LogWarning("No level object selected.");
			return;
		}

		foreach (Object o in Selection.objects) {
			Register(o as GameObject);
		}
	}

	static void Register (GameObject _o) {
		if (PrefabUtility.GetPrefabType(_o) == PrefabType.Prefab) {
			Debug.LogWarning(_o.name + " is a prefab asset.", _o);
			return;
		}

		var levelObject = _o.GetComponent<GameLevelObject>();
		if (levelObject == null) {
			Debug.LogWarning(_o.name + " isn't a game level object.", _o);
			return;
		}

		foreach (GameObject rootObject in _o.scene.GetRootGameObjects()) {
			var gameLevel = rootObject.GetComponent<GameLevel>();
			if (gameLevel != null) {
				if (gameLevel.HasLevelObject(levelObject)) {
					Debug.LogWarning(_o.name + " is already registered.", _o);
					return;
				}

				Undo.RecordObject(gameLevel, "Register Level Object.");
				gameLevel.RegisterLevelObject(levelObject);
				Debug.Log(
					_o.name + " registered to game level " +
					gameLevel.name + " in scene " + _o.scene.name + ".", _o
				);
				return;
			}
		}
		Debug.LogWarning(_o.name + " isn't part of a game level.", _o);
	}
}