using UnityEngine;

public abstract class SpawnZone : GameLevelObject
{
    public abstract Vector3 SpawnPoint { get; }

    [SerializeField, Range(0f, 50f)] float spawnSpeed;

    [System.Serializable]
    public struct SpawnConfiguration
    {
        public enum MovementDirection
        {
            Forward,
            Upward,
            Outward,
            Random
        }

        public ShapeFactory[] factories;

        public MovementDirection movementDirection;

        public FloatRange speed;

        public FloatRange angularSpeed;

        public FloatRange scale;

        public ColorRangeHSV color;

        public bool uniformColor;

        public MovementDirection oscillationDirection;

        public FloatRange oscillationAmplitude;

        public FloatRange oscillationFrequency;

        [System.Serializable]
        public struct SatelliteConfiguration
        {
            public IntRange amount;

            [FloatRangeSlider(0.1f, 1f)] public FloatRange relativeScale;

            public FloatRange orbitRadius;

            public FloatRange orbitFrequency;

            public bool uniformLifecycles;
        }

        public SatelliteConfiguration satellite;

        [System.Serializable]
        public struct LifecycleConfiguration
        {
            [FloatRangeSlider(0f, 2f)] public FloatRange growingDuration;

            [FloatRangeSlider(0f, 100f)] public FloatRange adultDuration;

            [FloatRangeSlider(0f, 2f)] public FloatRange dyingDuration;

            public Vector3 RandomDurations => new Vector3(growingDuration.RandomValueInRange, adultDuration.RandomValueInRange, dyingDuration.RandomValueInRange);
        }

        public LifecycleConfiguration lifecycle;
    }

    [SerializeField] SpawnConfiguration spawnConfig;

    float spawnProgress;

    public virtual void SpawnShapes()
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        shape.gameObject.layer = gameObject.layer;
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
        
        SetupColor(shape);

        float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
        
        if (angularSpeed != 0f)
        {
            var rotation = shape.AddBehavior<RotationShapeBehavior>();
            rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
        }

        float speed = spawnConfig.speed.RandomValueInRange;
        
        if (speed != 0f)
        {
            var movement = shape.AddBehavior<MovementShapeBehavior>();
            movement.Velocity = GetDirectionVector(spawnConfig.movementDirection, t) * speed;
        }

        SetupOscillation(shape);

        Vector3 lifecycleDurations = spawnConfig.lifecycle.RandomDurations;

        int satelliteCount = spawnConfig.satellite.amount.RandomValueInRange;
        
        for (int i = 0; i < satelliteCount; i++)
        {
            CreateSatelliteFor(shape, spawnConfig.satellite.uniformLifecycles ? lifecycleDurations : spawnConfig.lifecycle.RandomDurations);
        }

        SetupLifecycle(shape, lifecycleDurations);
    }

    void CreateSatelliteFor(Shape _focalShape, Vector3 _lifecycleDurations)
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        shape.gameObject.layer = gameObject.layer;
        Transform t = shape.transform;
        t.localRotation = Random.rotation;
        t.localScale = _focalShape.transform.localScale * spawnConfig.satellite.relativeScale.RandomValueInRange;
        
        SetupColor(shape);
        
        shape.AddBehavior<SatelliteShapeBehavior>().Initialize(shape, _focalShape, spawnConfig.satellite.orbitRadius.RandomValueInRange, spawnConfig.satellite.orbitFrequency.RandomValueInRange);
        
        SetupLifecycle(shape, _lifecycleDurations);
    }

    void SetupColor(Shape _shape)
    {
        if (spawnConfig.uniformColor)
        {
            _shape.SetColor(spawnConfig.color.RandomInRange);
        }
        else
        {
            for (int i = 0; i < _shape.ColorCount; i++)
            {
                _shape.SetColor(spawnConfig.color.RandomInRange, i);
            }
        }
    }

    void SetupLifecycle(Shape _shape, Vector3 _durations)
    {
        if (_durations.x > 0f)
        {
            if (_durations.y > 0f || _durations.z > 0f)
            {
                _shape.AddBehavior<LifecycleShapeBehavior>().Initialize(
                                                                        _shape, _durations.x, _durations.y, _durations.z
                                                                       );
            }
            else
            {
                _shape.AddBehavior<GrowingShapeBehavior>().Initialize(
                                                                      _shape, _durations.x
                                                                     );
            }
        }
        else if (_durations.y > 0f)
        {
            _shape.AddBehavior<LifecycleShapeBehavior>().Initialize(
                                                                    _shape, _durations.x, _durations.y, _durations.z
                                                                   );
        }
        else if (_durations.z > 0f)
        {
            _shape.AddBehavior<DyingShapeBehavior>().Initialize(
                                                                _shape, _durations.z
                                                               );
        }
    }

    void SetupOscillation(Shape _shape)
    {
        float amplitude = spawnConfig.oscillationAmplitude.RandomValueInRange;
        float frequency = spawnConfig.oscillationFrequency.RandomValueInRange;
        if (amplitude == 0f || frequency == 0f)
        {
            return;
        }

        var oscillation = _shape.AddBehavior<OscillationShapeBehavior>();
        oscillation.Offset = GetDirectionVector(
                                                spawnConfig.oscillationDirection, _shape.transform
                                               ) * amplitude;
        oscillation.Frequency = frequency;
    }

    Vector3 GetDirectionVector(
            SpawnConfiguration.MovementDirection _direction, Transform _t
    )
    {
        switch (_direction)
        {
            case SpawnConfiguration.MovementDirection.Upward:
                return transform.up;
            case SpawnConfiguration.MovementDirection.Outward:
                return (_t.localPosition - transform.position).normalized;
            case SpawnConfiguration.MovementDirection.Random:
                return Random.onUnitSphere;
            default:
                return transform.forward;
        }
    }

    public override void GameUpdate()
    {
        spawnProgress += Time.deltaTime * spawnSpeed;
        while (spawnProgress >= 1f)
        {
            spawnProgress -= 1f;
            SpawnShapes();
        }
    }

    public override void Save(GameDataWriter _writer)
    {
        _writer.Write(spawnProgress);
    }

    public override void Load(GameDataReader _reader)
    {
        spawnProgress = _reader.ReadFloat();
    }
}