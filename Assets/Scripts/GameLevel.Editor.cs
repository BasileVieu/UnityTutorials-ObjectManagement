#if UNITY_EDITOR

using UnityEngine;

partial class GameLevel {

	public bool HasMissingLevelObjects {
		get {
			if (levelObjects != null) {
				for (int i = 0; i < levelObjects.Length; i++) {
					if (levelObjects[i] == null) {
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool HasLevelObject (GameLevelObject _o) {
		if (levelObjects != null) {
			for (int i = 0; i < levelObjects.Length; i++) {
				if (levelObjects[i] == _o) {
					return true;
				}
			}
		}
		return false;
	}

	public void RegisterLevelObject (GameLevelObject _o) {
		if (Application.isPlaying) {
			Debug.LogError("Do not invoke in play mode!");
			return;
		}

		if (HasLevelObject(_o)) {
			return;
		}

		if (levelObjects == null) {
			levelObjects = new GameLevelObject[] { _o };
		}
		else {
			System.Array.Resize(ref levelObjects, levelObjects.Length + 1);
			levelObjects[levelObjects.Length - 1] = _o;
		}
	}

	public void RemoveMissingLevelObjects () {
		if (Application.isPlaying) {
			Debug.LogError("Do not invoke in play mode!");
			return;
		}

		int holes = 0;
		for (int i = 0; i < levelObjects.Length - holes; i++) {
			if (levelObjects[i] == null) {
				holes += 1;
				System.Array.Copy(
					levelObjects, i + 1, levelObjects, i,
					levelObjects.Length - i - holes
				);
				i -= 1;
			}
		}
		System.Array.Resize(ref levelObjects, levelObjects.Length - holes);
	}
}

#endif