using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class ShapeFactory : ScriptableObject
{
    public int FactoryId
    {
        get => factoryId;
        set
        {
            if (factoryId == int.MinValue && value != int.MinValue)
            {
                factoryId = value;
            }
            else
            {
                Debug.Log("Not allowed to change factoryId.");
            }
        }
    }

    [System.NonSerialized] int factoryId = int.MinValue;

    [SerializeField] Shape[] prefabs;

    [SerializeField] Material[] materials;

    [SerializeField] bool recycle;

    List<Shape>[] pools;

    Scene poolScene;

    public Shape Get(int _shapeId = 0, int _materialId = 0)
    {
        Shape instance;
        
        if (recycle)
        {
            if (pools == null)
            {
                CreatePools();
            }

            List<Shape> pool = pools[_shapeId];
            
            int lastIndex = pool.Count - 1;
            
            if (lastIndex >= 0)
            {
                instance = pool[lastIndex];
                instance.gameObject.SetActive(true);
                pool.RemoveAt(lastIndex);
            }
            else
            {
                instance = Instantiate(prefabs[_shapeId]);
                instance.OriginFactory = this;
                instance.ShapeId = _shapeId;
                
                SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
            }
        }
        else
        {
            instance = Instantiate(prefabs[_shapeId]);
            instance.ShapeId = _shapeId;
        }

        instance.SetMaterial(materials[_materialId], _materialId);
        
        Game.Instance.AddShape(instance);
        
        return instance;
    }

    public Shape GetRandom() => Get(Random.Range(0, prefabs.Length), Random.Range(0, materials.Length));

    public void Reclaim(Shape _shapeToRecycle)
    {
        if (_shapeToRecycle.OriginFactory != this)
        {
            Debug.LogError("Tried to reclaim shape with wrong factory.");
            
            return;
        }

        if (recycle)
        {
            if (pools == null)
            {
                CreatePools();
            }

            pools[_shapeToRecycle.ShapeId].Add(_shapeToRecycle);
            
            _shapeToRecycle.gameObject.SetActive(false);
        }
        else
        {
            Destroy(_shapeToRecycle.gameObject);
        }
    }

    void CreatePools()
    {
        pools = new List<Shape>[prefabs.Length];
        
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<Shape>();
        }

        if (Application.isEditor)
        {
            poolScene = SceneManager.GetSceneByName(name);
            
            if (poolScene.isLoaded)
            {
                GameObject[] rootObjects = poolScene.GetRootGameObjects();
                
                for (int i = 0; i < rootObjects.Length; i++)
                {
                    Shape pooledShape = rootObjects[i].GetComponent<Shape>();
                    
                    if (!pooledShape.gameObject.activeSelf)
                    {
                        pools[pooledShape.ShapeId].Add(pooledShape);
                    }
                }

                return;
            }
        }

        poolScene = SceneManager.CreateScene(name);
    }
}