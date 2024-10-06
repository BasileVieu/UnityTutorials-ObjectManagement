using UnityEngine;

public class PersistableObject : MonoBehaviour {

	public virtual void Save (GameDataWriter _writer) {
		_writer.Write(transform.localPosition);
		_writer.Write(transform.localRotation);
		_writer.Write(transform.localScale);
	}

	public virtual void Load (GameDataReader _reader) {
		transform.localPosition = _reader.ReadVector3();
		transform.localRotation = _reader.ReadQuaternion();
		transform.localScale = _reader.ReadVector3();
	}
}