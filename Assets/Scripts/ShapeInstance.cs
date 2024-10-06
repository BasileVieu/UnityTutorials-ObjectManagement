[System.Serializable]
public struct ShapeInstance {

	public bool IsValid {
		get {
			return Shape && instanceIdOrSaveIndex == Shape.InstanceId;
		}
	}

	public Shape Shape { get; private set; }

	int instanceIdOrSaveIndex;

	public ShapeInstance (Shape _shape) {
		Shape = _shape;
		instanceIdOrSaveIndex = _shape.InstanceId;
	}

	public ShapeInstance (int _saveIndex) {
		Shape = null;
		instanceIdOrSaveIndex = _saveIndex;
	}

	public void Resolve () {
		if (instanceIdOrSaveIndex >= 0) {
			Shape = Game.Instance.GetShape(instanceIdOrSaveIndex);
			instanceIdOrSaveIndex = Shape.InstanceId;
		}
	}

	public static implicit operator ShapeInstance (Shape _shape) {
		return new ShapeInstance(_shape);
	}
}