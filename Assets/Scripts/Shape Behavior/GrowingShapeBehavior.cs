using UnityEngine;

public sealed class GrowingShapeBehavior : ShapeBehavior {

	public override ShapeBehaviorType BehaviorType {
		get {
			return ShapeBehaviorType.Growing;
		}
	}

	Vector3 originalScale;
	float duration;

	public void Initialize (Shape _shape, float _duration) {
		originalScale = _shape.transform.localScale;
		this.duration = _duration;
		_shape.transform.localScale = Vector3.zero;
	}

	public override bool GameUpdate (Shape _shape) {
		if (_shape.Age < duration) {
			float s = _shape.Age / duration;
			s = (3f - 2f * s) * s * s;
			_shape.transform.localScale = s * originalScale;
			return true;
		}
		_shape.transform.localScale = originalScale;
		return false;
	}

	public override void Save (GameDataWriter _writer) {
		_writer.Write(originalScale);
		_writer.Write(duration);
	}

	public override void Load (GameDataReader _reader) {
		originalScale = _reader.ReadVector3();
		duration = _reader.ReadFloat();
	}

	public override void Recycle () {
		ShapeBehaviorPool<GrowingShapeBehavior>.Reclaim(this);
	}
}