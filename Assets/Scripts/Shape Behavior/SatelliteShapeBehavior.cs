using UnityEngine;

public sealed class SatelliteShapeBehavior : ShapeBehavior {

	public override ShapeBehaviorType BehaviorType {
		get {
			return ShapeBehaviorType.Satellite;
		}
	}

	ShapeInstance focalShape;

	float frequency;

	Vector3 cosOffset, sinOffset;

	Vector3 previousPosition;

	public void Initialize (
		Shape _shape, Shape _focalShape, float _radius, float _frequency
	) {
		this.focalShape = _focalShape;
		this.frequency = _frequency;

		Vector3 orbitAxis = Random.onUnitSphere;
		do {
			cosOffset = Vector3.Cross(orbitAxis, Random.onUnitSphere).normalized;
		}
		while (cosOffset.sqrMagnitude < 0.1f);
		sinOffset = Vector3.Cross(cosOffset, orbitAxis);
		cosOffset *= _radius;
		sinOffset *= _radius;

		_shape.AddBehavior<RotationShapeBehavior>().AngularVelocity =
			-360f * _frequency *
			_shape.transform.InverseTransformDirection(orbitAxis);

		GameUpdate(_shape);
		previousPosition = _shape.transform.localPosition;
	}

	public override bool GameUpdate (Shape _shape) {
		if (focalShape.IsValid) {
			float t = 2f * Mathf.PI * frequency * _shape.Age;
			previousPosition = _shape.transform.localPosition;
			_shape.transform.localPosition =
				focalShape.Shape.transform.localPosition +
				cosOffset * Mathf.Cos(t) + sinOffset * Mathf.Sin(t);
			return true;
		}

		_shape.AddBehavior<MovementShapeBehavior>().Velocity =
			(_shape.transform.localPosition - previousPosition) / Time.deltaTime;
		return false;
	}

	public override void Save (GameDataWriter _writer) {
		_writer.Write(focalShape);
		_writer.Write(frequency);
		_writer.Write(cosOffset);
		_writer.Write(sinOffset);
		_writer.Write(previousPosition);
	}

	public override void Load (GameDataReader _reader) {
		focalShape = _reader.ReadShapeInstance();
		frequency = _reader.ReadFloat();
		cosOffset = _reader.ReadVector3();
		sinOffset = _reader.ReadVector3();
		previousPosition = _reader.ReadVector3();
	}

	public override void ResolveShapeInstances () {
		focalShape.Resolve();
	}

	public override void Recycle () {
		ShapeBehaviorPool<SatelliteShapeBehavior>.Reclaim(this);
	}
}