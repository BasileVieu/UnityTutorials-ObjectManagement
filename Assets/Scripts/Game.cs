using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Game : PersistableObject
{
    const int saveVersion = 7;

    public static Game Instance { get; private set; }

    [SerializeField] ShapeFactory[] shapeFactories;

    [SerializeField] KeyCode createKey = KeyCode.C;
    [SerializeField] KeyCode destroyKey = KeyCode.X;
    [SerializeField] KeyCode newGameKey = KeyCode.N;
    [SerializeField] KeyCode saveKey = KeyCode.S;
    [SerializeField] KeyCode loadKey = KeyCode.L;

    [SerializeField] PersistentStorage storage;

    [SerializeField] int levelCount;

    [SerializeField] bool reseedOnLoad;

    [SerializeField] Slider creationSpeedSlider;
    [SerializeField] Slider destructionSpeedSlider;

    [SerializeField] float destroyDuration;

    public float CreationSpeed { get; set; }

    public float DestructionSpeed { get; set; }

    List<Shape> shapes;

    List<ShapeInstance> killList, markAsDyingList;

    float creationProgress, destructionProgress;

    int loadedLevelBuildIndex;

    Random.State mainRandomState;

    bool inGameUpdateLoop;

    int dyingShapeCount;

    void OnEnable()
    {
        Instance = this;
        if (shapeFactories[0].FactoryId != 0)
        {
            for (int i = 0; i < shapeFactories.Length; i++)
            {
                shapeFactories[i].FactoryId = i;
            }
        }
    }

    void Start()
    {
        mainRandomState = Random.state;
        shapes = new List<Shape>();
        killList = new List<ShapeInstance>();
        markAsDyingList = new List<ShapeInstance>();

        if (Application.isEditor)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name.Contains("Level "))
                {
                    SceneManager.SetActiveScene(loadedScene);
                    loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
        }

        BeginNewGame();
        StartCoroutine(LoadLevel(1));
    }

    void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            GameLevel.Current.SpawnShapes();
        }
        else if (Input.GetKeyDown(destroyKey))
        {
            DestroyShape();
        }
        else if (Input.GetKeyDown(newGameKey))
        {
            BeginNewGame();
            StartCoroutine(LoadLevel(loadedLevelBuildIndex));
        }
        else if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this, saveVersion);
        }
        else if (Input.GetKeyDown(loadKey))
        {
            BeginNewGame();
            storage.Load(this);
        }
        else
        {
            for (int i = 1; i <= levelCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    BeginNewGame();
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }
        }
    }

    void FixedUpdate()
    {
        inGameUpdateLoop = true;
        for (int i = 0; i < shapes.Count; i++)
        {
            shapes[i].GameUpdate();
        }

        GameLevel.Current.GameUpdate();
        inGameUpdateLoop = false;

        creationProgress += Time.deltaTime * CreationSpeed;
        while (creationProgress >= 1f)
        {
            creationProgress -= 1f;
            GameLevel.Current.SpawnShapes();
        }

        destructionProgress += Time.deltaTime * DestructionSpeed;
        while (destructionProgress >= 1f)
        {
            destructionProgress -= 1f;
            DestroyShape();
        }

        int limit = GameLevel.Current.PopulationLimit;
        if (limit > 0)
        {
            while (shapes.Count - dyingShapeCount > limit)
            {
                DestroyShape();
            }
        }

        if (killList.Count > 0)
        {
            for (int i = 0; i < killList.Count; i++)
            {
                if (killList[i].IsValid)
                {
                    KillImmediately(killList[i].Shape);
                }
            }

            killList.Clear();
        }

        if (markAsDyingList.Count > 0)
        {
            for (int i = 0; i < markAsDyingList.Count; i++)
            {
                if (markAsDyingList[i].IsValid)
                    MarkAsDyingImmediately(markAsDyingList[i].Shape);
            }

            markAsDyingList.Clear();
        }
    }

    void BeginNewGame()
    {
        Random.state = mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        mainRandomState = Random.state;
        Random.InitState(seed);

        creationSpeedSlider.value = CreationSpeed = 0;
        destructionSpeedSlider.value = DestructionSpeed = 0;

        for (int i = 0; i < shapes.Count; i++)
        {
            shapes[i].Recycle();
        }

        shapes.Clear();
        dyingShapeCount = 0;
    }

    IEnumerator LoadLevel(int _levelBuildIndex)
    {
        enabled = false;
        if (loadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }

        yield return SceneManager.LoadSceneAsync(_levelBuildIndex, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(_levelBuildIndex));
        loadedLevelBuildIndex = _levelBuildIndex;
        enabled = true;
    }

    void DestroyShape()
    {
        if (shapes.Count - dyingShapeCount > 0)
        {
            Shape shape = shapes[Random.Range(dyingShapeCount, shapes.Count)];
            if (destroyDuration <= 0f)
            {
                KillImmediately(shape);
            }
            else
            {
                shape.AddBehavior<DyingShapeBehavior>().Initialize(
                                                                   shape, destroyDuration
                                                                  );
            }
        }
    }

    public void AddShape(Shape _shape)
    {
        _shape.SaveIndex = shapes.Count;
        shapes.Add(_shape);
    }

    public Shape GetShape(int _index)
    {
        return shapes[_index];
    }

    public void Kill(Shape _shape)
    {
        if (inGameUpdateLoop)
        {
            killList.Add(_shape);
        }
        else
        {
            KillImmediately(_shape);
        }
    }

    void KillImmediately(Shape _shape)
    {
        int index = _shape.SaveIndex;
        _shape.Recycle();

        if (index < dyingShapeCount && index < --dyingShapeCount)
        {
            shapes[dyingShapeCount].SaveIndex = index;
            shapes[index] = shapes[dyingShapeCount];
            index = dyingShapeCount;
        }

        int lastIndex = shapes.Count - 1;
        if (index < lastIndex)
        {
            shapes[lastIndex].SaveIndex = index;
            shapes[index] = shapes[lastIndex];
        }

        shapes.RemoveAt(lastIndex);
    }

    public bool IsMarkedAsDying(Shape _shape)
    {
        return _shape.SaveIndex < dyingShapeCount;
    }

    public void MarkAsDying(Shape _shape)
    {
        if (inGameUpdateLoop)
        {
            markAsDyingList.Add(_shape);
        }
        else
        {
            MarkAsDyingImmediately(_shape);
        }
    }

    void MarkAsDyingImmediately(Shape _shape)
    {
        int index = _shape.SaveIndex;
        if (index < dyingShapeCount)
        {
            return;
        }

        shapes[dyingShapeCount].SaveIndex = index;
        shapes[index] = shapes[dyingShapeCount];
        _shape.SaveIndex = dyingShapeCount;
        shapes[dyingShapeCount++] = _shape;
    }

    public override void Save(GameDataWriter _writer)
    {
        _writer.Write(shapes.Count);
        _writer.Write(Random.state);
        _writer.Write(CreationSpeed);
        _writer.Write(creationProgress);
        _writer.Write(DestructionSpeed);
        _writer.Write(destructionProgress);
        _writer.Write(loadedLevelBuildIndex);
        GameLevel.Current.Save(_writer);

        for (int i = 0; i < shapes.Count; i++)
        {
            _writer.Write(shapes[i].OriginFactory.FactoryId);
            _writer.Write(shapes[i].ShapeId);
            _writer.Write(shapes[i].MaterialId);
            shapes[i].Save(_writer);
        }
    }

    public override void Load(GameDataReader _reader)
    {
        int version = _reader.Version;
        if (version > saveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        StartCoroutine(LoadGame(_reader));
    }

    IEnumerator LoadGame(GameDataReader _reader)
    {
        int version = _reader.Version;
        int count = version <= 0 ? -version : _reader.ReadInt();

        if (version >= 3)
        {
            Random.State state = _reader.ReadRandomState();
            if (!reseedOnLoad)
            {
                Random.state = state;
            }

            creationSpeedSlider.value = CreationSpeed = _reader.ReadFloat();
            creationProgress = _reader.ReadFloat();
            destructionSpeedSlider.value = DestructionSpeed = _reader.ReadFloat();
            destructionProgress = _reader.ReadFloat();
        }

        yield return LoadLevel(version < 2 ? 1 : _reader.ReadInt());
        if (version >= 3)
        {
            GameLevel.Current.Load(_reader);
        }

        for (int i = 0; i < count; i++)
        {
            int factoryId = version >= 5 ? _reader.ReadInt() : 0;
            int shapeId = version > 0 ? _reader.ReadInt() : 0;
            int materialId = version > 0 ? _reader.ReadInt() : 0;
            Shape instance = shapeFactories[factoryId].Get(shapeId, materialId);
            instance.Load(_reader);
        }

        for (int i = 0; i < shapes.Count; i++)
        {
            shapes[i].ResolveShapeInstances();
        }
    }
}