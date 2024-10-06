using UnityEngine;

public sealed class DyingShapeBehavior : ShapeBehavior {

	public override ShapeBehaviorType BehaviorType {
		get {
			return ShapeBehaviorType.Dying;
		}
	}

	Vector3 originalScale;
	float duration, dyingAge;

	public void Initialize (Shape _shape, float _duration) {
		originalScale = _shape.transform.localScale;
		this.duration = _duration;
		dyingAge = _shape.Age;
		_shape.MarkAsDying();
	}

	public override bool GameUpdate (Shape _shape) {
		float dyingDuration = _shape.Age - dyingAge;
		if (dyingDuration < duration) {
			float s = 1f - dyingDuration / duration;
			s = (3f - 2f * s) * s * s;
			_shape.transform.localScale = s * originalScale;
			return true;
		}
		_shape.Die();
		return true;
	}

	public override void Save (GameDataWriter _writer) {
		_writer.Write(originalScale);
		_writer.Write(duration);
		_writer.Write(dyingAge);
	}

	public override void Load (GameDataReader _reader) {
		originalScale = _reader.ReadVector3();
		duration = _reader.ReadFloat();
		dyingAge = _reader.ReadFloat();
	}

	public override void Recycle () {
		ShapeBehaviorPool<DyingShapeBehavior>.Reclaim(this);
	}
}