using UnityEngine;

public sealed class MovementShapeBehavior : ShapeBehavior {

	public override ShapeBehaviorType BehaviorType {
		get {
			return ShapeBehaviorType.Movement;
		}
	}

	public Vector3 Velocity { get; set; }

	public override bool GameUpdate (Shape _shape) {
		_shape.transform.localPosition += Velocity * Time.deltaTime;
		return true;
	}

	public override void Save (GameDataWriter _writer) {
		_writer.Write(Velocity);
	}

	public override void Load (GameDataReader _reader) {
		Velocity = _reader.ReadVector3();
	}

	public override void Recycle () {
		ShapeBehaviorPool<MovementShapeBehavior>.Reclaim(this);
	}
}