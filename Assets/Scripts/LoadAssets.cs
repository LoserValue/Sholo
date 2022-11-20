using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class KeyValuePair
{
    public AssetReference Model;
    public List<Vector3> Transform;
}
public class LoadAssets : MonoBehaviour
{
    public List<KeyValuePair> Model = new List<KeyValuePair>();
    private Dictionary<AssetReference, List<Vector3>> _ModelReferences = new Dictionary<AssetReference, List<Vector3>>();

    private readonly Dictionary<AssetReference, List<GameObject>> _spawnedModelSystem = new Dictionary<AssetReference,List<GameObject>>();
    private readonly Dictionary<AssetReference, Queue<Vector3>> _queuedSpawnRequest = new Dictionary<AssetReference,Queue<Vector3>>();
    private readonly Dictionary<AssetReference,AsyncOperationHandle<GameObject>> _asyncOperationHandles = new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

    void Awake()
    {
        foreach (var kvp in Model)
        {
            _ModelReferences[kvp.Model] = kvp.Transform;
        }
    }

    public void Spawn(int i)
    {
        if(i<0 || i>= _ModelReferences.Count)
            return;
        AssetReference assetReference = _ModelReferences.Keys.ElementAt(i);
        if(assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.Log(message:"Invalid Key" + assetReference.RuntimeKey.ToString());
            return;
        }
        if(_asyncOperationHandles.ContainsKey(assetReference))
        {
            if (_asyncOperationHandles[assetReference].IsDone)
                SpawnModelFromLoadedReference(assetReference, GetPosition(i), GetRotation(i));
            else
                EnqueueSpawnForAfterInitialization(assetReference,i);
            return;
        }
        LoadAndSpawn(assetReference,i);
    }

    public IEnumerator Animate(GameObject obj)
    {
        var anim = obj.GetComponent<Animator>();
        anim.Play("AnimDestroy");
        yield return WaitForAnimation(anim);
        Addressables.Release(obj);
    }
    private IEnumerator WaitForAnimation(Animator animation) 
    {
        do
        {
            yield return null;
        }while(!animation.GetBool("isFinished"));
    }
    public void RemoveModel(int i)
    {
        if (i < 0 || i >= _ModelReferences.Count)
            return;
        AssetReference assetReference = _ModelReferences.Keys.ElementAt(i);
        if (assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.Log(message: "Invalid Key" + assetReference.RuntimeKey.ToString());
            return;
        }
        //remove all model of the same type
        foreach (var obj in _spawnedModelSystem[assetReference])
        {
            StartCoroutine(Animate(obj));
        }
    }
    private void LoadAndSpawn(AssetReference assetReference, int i)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
        _asyncOperationHandles[assetReference] = op;
        op.Completed += (operation) =>
        {
            SpawnModelFromLoadedReference(assetReference, GetPosition(i), GetRotation(i));
            if (_queuedSpawnRequest.ContainsKey(assetReference))
            {
                while (_queuedSpawnRequest[assetReference]?.Any() == true)
                {
                    var position = _queuedSpawnRequest[assetReference].Dequeue();
                    SpawnModelFromLoadedReference(assetReference, position, GetRotation(i));
                }
            }
        };
    }

    private void EnqueueSpawnForAfterInitialization(AssetReference assetReference, int i)
    {
        if (_queuedSpawnRequest.ContainsKey(assetReference) == false)
            _queuedSpawnRequest[assetReference] = new Queue<Vector3>();
        _queuedSpawnRequest[assetReference].Enqueue(item: GetPosition(i));
    }

    private void SpawnModelFromLoadedReference(AssetReference assetReference, Vector3 position, Vector3 rotation)
    {
        assetReference.InstantiateAsync(position, Quaternion.Euler(rotation)).Completed += (asyncOperationHandle) =>
        {
            if (_spawnedModelSystem.ContainsKey(assetReference) == false)
                _spawnedModelSystem[assetReference] = new List<GameObject>();

            _spawnedModelSystem[assetReference].Add(asyncOperationHandle.Result);
            var notify = asyncOperationHandle.Result.AddComponent<NotifyOnDestroy>();
            notify.Destroyed += Remove;
            notify.AssetReference = assetReference;

        };
    }

    private Vector3 GetPosition(int i)
    {
        return new Vector3(x: _ModelReferences.Values.ElementAt(i)[0].x, y: _ModelReferences.Values.ElementAt(i)[0].y, z: _ModelReferences.Values.ElementAt(i)[0].z);
    }

    private Vector3 GetRotation(int i)
    {
        return new Vector3(x: _ModelReferences.Values.ElementAt(i)[1].x, y: _ModelReferences.Values.ElementAt(i)[1].y, z: _ModelReferences.Values.ElementAt(i)[1].z);
    }

    private void Remove(AssetReference assetReference, NotifyOnDestroy obj)
    {
        Addressables.ReleaseInstance(obj.gameObject);


        _spawnedModelSystem[assetReference].Remove(obj.gameObject);
        if(_spawnedModelSystem[assetReference].Count == 0)
        {
            Debug.Log(message: $"Removed all {assetReference.RuntimeKey.ToString()}");

            if(_asyncOperationHandles[assetReference].IsValid())
                Addressables.Release(_asyncOperationHandles[assetReference]);

            _asyncOperationHandles.Remove(assetReference);
        }
    }
}
