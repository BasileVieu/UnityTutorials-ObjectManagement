using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject {

	static int colorPropertyId = Shader.PropertyToID("_Color");

	static MaterialPropertyBlock sharedPropertyBlock;

	public float Age { get; private set; }

	public int InstanceId { get; private set; }

	public bool IsMarkedAsDying {
		get {
			return Game.Instance.IsMarkedAsDying(this);
		}
	}

	public int SaveIndex { get; set; }

	public int ColorCount {
		get {
			return colors.Length;
		}
	}

	public int MaterialId { get; private set; }

	public int ShapeId {
		get {
			return shapeId;
		}
		set {
			if (shapeId == int.MinValue && value != int.MinValue) {
				shapeId = value;
			}
			else {
				Debug.LogError("Not allowed to change ShapeId.");
			}
		}
	}

	public ShapeFactory OriginFactory {
		get {
			return originFactory;
		}
		set {
			if (originFactory == null) {
				originFactory = value;
			}
			else {
				Debug.LogError("Not allowed to change origin factory.");
			}
		}
	}

	ShapeFactory originFactory;

	int shapeId = int.MinValue;

	[SerializeField]
	MeshRenderer[] meshRenderers;

	Color[] colors;

	List<ShapeBehavior> behaviorList = new List<ShapeBehavior>();

	void Awake () {
		colors = new Color[meshRenderers.Length];
	}

	public T AddBehavior<T> () where T : ShapeBehavior, new() {
		T behavior = ShapeBehaviorPool<T>.Get();
		behaviorList.Add(behavior);
		return behavior;
	}

	public void Die () {
		Game.Instance.Kill(this);
	}

	public void GameUpdate () {
		Age += Time.deltaTime;
		for (int i = 0; i < behaviorList.Count; i++) {
			if (!behaviorList[i].GameUpdate(this)) {
				behaviorList[i].Recycle();
				behaviorList.RemoveAt(i--);
			}
		}
	}

	public void MarkAsDying () {
		Game.Instance.MarkAsDying(this);
	}

	public void Recycle () {
		Age = 0f;
		InstanceId += 1;
		for (int i = 0; i < behaviorList.Count; i++) {
			behaviorList[i].Recycle();
		}
		behaviorList.Clear();
		OriginFactory.Reclaim(this);
	}

	public void ResolveShapeInstances () {
		for (int i = 0; i < behaviorList.Count; i++) {
			behaviorList[i].ResolveShapeInstances();
		}
	}

	public void SetColor (Color _color) {
		if (sharedPropertyBlock == null) {
			sharedPropertyBlock = new MaterialPropertyBlock();
		}
		sharedPropertyBlock.SetColor(colorPropertyId, _color);
		for (int i = 0; i < meshRenderers.Length; i++) {
			colors[i] = _color;
			meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
		}
	}

	public void SetColor (Color _color, int _index) {
		if (sharedPropertyBlock == null) {
			sharedPropertyBlock = new MaterialPropertyBlock();
		}
		sharedPropertyBlock.SetColor(colorPropertyId, _color);
		colors[_index] = _color;
		meshRenderers[_index].SetPropertyBlock(sharedPropertyBlock);
	}

	public void SetMaterial (Material _material, int _materialId) {
		for (int i = 0; i < meshRenderers.Length; i++) {
			meshRenderers[i].material = _material;
		}
		MaterialId = _materialId;
	}

	public override void Save (GameDataWriter _writer) {
		base.Save(_writer);
		_writer.Write(colors.Length);
		for (int i = 0; i < colors.Length; i++) {
			_writer.Write(colors[i]);
		}
		_writer.Write(Age);
		_writer.Write(behaviorList.Count);
		for (int i = 0; i < behaviorList.Count; i++) {
			_writer.Write((int)behaviorList[i].BehaviorType);
			behaviorList[i].Save(_writer);
		}
	}

	public override void Load (GameDataReader _reader) {
		base.Load(_reader);
		if (_reader.Version >= 5) {
			LoadColors(_reader);
		}
		else {
			SetColor(_reader.Version > 0 ? _reader.ReadColor() : Color.white);
		}
		if (_reader.Version >= 6) {
			Age = _reader.ReadFloat();
			int behaviorCount = _reader.ReadInt();
			for (int i = 0; i < behaviorCount; i++) {
				ShapeBehavior behavior =
					((ShapeBehaviorType)_reader.ReadInt()).GetInstance();
				behaviorList.Add(behavior);
				behavior.Load(_reader);
			}
		}
		else if (_reader.Version >= 4) {
			AddBehavior<RotationShapeBehavior>().AngularVelocity =
				_reader.ReadVector3();
			AddBehavior<MovementShapeBehavior>().Velocity = _reader.ReadVector3();
		}
	}

	void LoadColors (GameDataReader _reader) {
		int count = _reader.ReadInt();
		int max = count <= colors.Length ? count : colors.Length;
		int i = 0;
		for (; i < max; i++) {
			SetColor(_reader.ReadColor(), i);
		}
		if (count > colors.Length) {
			for (; i < count; i++) {
				_reader.ReadColor();
			}
		}
		else if (count < colors.Length) {
			for (; i < colors.Length; i++) {
				SetColor(Color.white, i);
			}
		}
	}
}