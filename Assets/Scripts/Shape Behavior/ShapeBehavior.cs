﻿using UnityEngine;

public abstract class ShapeBehavior
#if UNITY_EDITOR
	: ScriptableObject
#endif
{
	public abstract ShapeBehaviorType BehaviorType { get; }

	public abstract bool GameUpdate (Shape _shape);

	public abstract void Save (GameDataWriter _writer);

	public abstract void Load (GameDataReader _reader);

	public virtual void ResolveShapeInstances () {}

	public abstract void Recycle ();

#if UNITY_EDITOR
	public bool IsReclaimed { get; set; }

	void OnEnable () {
		if (IsReclaimed) {
			Recycle();
		}
	}
#endif
}