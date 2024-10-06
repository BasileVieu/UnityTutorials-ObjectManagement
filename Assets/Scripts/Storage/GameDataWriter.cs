using System.IO;
using UnityEngine;

public class GameDataWriter {

	BinaryWriter writer;

	public GameDataWriter (BinaryWriter _writer) {
		this.writer = _writer;
	}

	public void Write (float _value) {
		writer.Write(_value);
	}

	public void Write (int _value) {
		writer.Write(_value);
	}

	public void Write (Color _value) {
		writer.Write(_value.r);
		writer.Write(_value.g);
		writer.Write(_value.b);
		writer.Write(_value.a);
	}

	public void Write (Quaternion _value) {
		writer.Write(_value.x);
		writer.Write(_value.y);
		writer.Write(_value.z);
		writer.Write(_value.w);
	}

	public void Write (Random.State _value) {
		writer.Write(JsonUtility.ToJson(_value));
	}

	public void Write (ShapeInstance _value) {
		writer.Write(_value.IsValid ? _value.Shape.SaveIndex : -1);
	}

	public void Write (Vector3 _value) {
		writer.Write(_value.x);
		writer.Write(_value.y);
		writer.Write(_value.z);
	}
}