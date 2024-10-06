using UnityEngine;

public sealed class OscillationShapeBehavior : ShapeBehavior {

	public override ShapeBehaviorType BehaviorType {
		get {
			return ShapeBehaviorType.Oscillation;
		}
	}

	public Vector3 Offset { get; set; }

	public float Frequency { get; set; }

	float previousOscillation;

	public override bool GameUpdate (Shape _shape) {
		float oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * _shape.Age);
		_shape.transform.localPosition +=
			(oscillation - previousOscillation) * Offset;
		previousOscillation = oscillation;
		return true;
	}

	public override void Save (GameDataWriter _writer) {
		_writer.Write(Offset);
		_writer.Write(Frequency);
		_writer.Write(previousOscillation);
	}

	public override void Load (GameDataReader _reader) {
		Offset = _reader.ReadVector3();
		Frequency = _reader.ReadFloat();
		previousOscillation = _reader.ReadFloat();
	}

	public override void Recycle () {
		previousOscillation = 0f;
		ShapeBehaviorPool<OscillationShapeBehavior>.Reclaim(this);
	}
}