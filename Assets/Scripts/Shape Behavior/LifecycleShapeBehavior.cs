using UnityEngine;

public sealed class LifecycleShapeBehavior : ShapeBehavior {

	public override ShapeBehaviorType BehaviorType {
		get {
			return ShapeBehaviorType.Lifecycle;
		}
	}

	float adultDuration, dyingDuration, dyingAge;

	public void Initialize (
		Shape _shape,
		float _growingDuration, float _adultDuration, float _dyingDuration
	) {
		this.adultDuration = _adultDuration;
		this.dyingDuration = _dyingDuration;
		dyingAge = _growingDuration + _adultDuration;

		if (_growingDuration > 0f) {
			_shape.AddBehavior<GrowingShapeBehavior>().Initialize(
				_shape, _growingDuration
			);
		}
	}

	public override bool GameUpdate (Shape _shape) {
		if (_shape.Age >= dyingAge) {
			if (dyingDuration <= 0f) {
				_shape.Die();
				return true;
			}
			if (!_shape.IsMarkedAsDying) {
				_shape.AddBehavior<DyingShapeBehavior>().Initialize(
					_shape, dyingDuration + dyingAge - _shape.Age
				);
			}
			return false;
		}
		return true;
	}

	public override void Save (GameDataWriter _writer) {
		_writer.Write(adultDuration);
		_writer.Write(dyingDuration);
		_writer.Write(dyingAge);
	}

	public override void Load (GameDataReader _reader) {
		adultDuration = _reader.ReadFloat();
		dyingDuration = _reader.ReadFloat();
		dyingAge = _reader.ReadFloat();
	}

	public override void Recycle () {
		ShapeBehaviorPool<LifecycleShapeBehavior>.Reclaim(this);
	}
}