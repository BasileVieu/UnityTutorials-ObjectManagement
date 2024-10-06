using System.IO;
using UnityEngine;

public class PersistentStorage : MonoBehaviour {

	string savePath;

	void Awake () {
		savePath = Path.Combine(Application.persistentDataPath, "saveFile");
	}

	public void Save (PersistableObject _o, int _version) {
		using (
			var writer = new BinaryWriter(File.Open(savePath, FileMode.Create))
		) {
			writer.Write(-_version);
			_o.Save(new GameDataWriter(writer));
		}
	}

	public void Load (PersistableObject _o) {
		byte[] data = File.ReadAllBytes(savePath);
		var reader = new BinaryReader(new MemoryStream(data));
		_o.Load(new GameDataReader(reader, -reader.ReadInt32()));
	}
}