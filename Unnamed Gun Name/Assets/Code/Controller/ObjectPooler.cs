﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class ObjectPooler : MonoBehaviourPun {

    public static ObjectPooler single_OP;

    public List<Pool> unSyncedPools = new List<Pool>();
    public Dictionary<string, Queue<GameObject>> unSyncedPoolDictionary;

    private void Awake() {
        single_OP = this;
        unSyncedPoolDictionary = new Dictionary<string, Queue<GameObject>>();
    }

    private void Start() {
        for (int i = 0; i < unSyncedPools.Count; i++) {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            if (unSyncedPools[i].prefab) {
                for (int iB = 0; iB < unSyncedPools[i].poolSize; iB++) {
                    GameObject poolObject = Instantiate(unSyncedPools[i].prefab, Vector3.zero, Quaternion.identity);
                    poolObject.SetActive(false);
                    poolObject.transform.SetParent(transform);
                    poolObject.name = poolObject.name += iB;
                    objectPool.Enqueue(poolObject);
                }
                unSyncedPoolDictionary.Add(unSyncedPools[i].prefab.name, objectPool);
            }
        }
    }

    public void GlobalSpawnProjectile(string tag, Vector3 pos, Quaternion rot, float range, float projectileSpeed, bool _isAffectedByGravity, int photonViewID) {
        int isAffectedByGravity = 0;
        if (_isAffectedByGravity) {
            isAffectedByGravity = 1;
        }
        photonView.RPC("RPC_GlobalSpawnProjectile", RpcTarget.All, tag, pos, rot, range, projectileSpeed, isAffectedByGravity, photonViewID);
    }

    [PunRPC]
    void RPC_GlobalSpawnProjectile(string tag, Vector3 pos, Quaternion rot, float range, float projectileSpeed, int _isAffectedByGravity, int photonViewID) {
        bool isAffectedByGravity = BoolCheck(_isAffectedByGravity);
        GameObject projObject = SpawnFromPool(tag, pos, rot);
        Projectile proj = projObject.GetComponent<Projectile>();
        proj.Launch(range, projectileSpeed, isAffectedByGravity);
    }

    public void GlobalSpawnFromPool(string tag, Vector3 pos, Quaternion rot) {
        photonView.RPC("RCP_SpawnFromPool", RpcTarget.All, tag, pos, rot);
    }

    [PunRPC]
    void RCP_SpawnFromPool(string tag, Vector3 pos, Quaternion rot) {
        SpawnFromPool(tag, pos, rot);
    }

    public GameObject SpawnFromPool(string tag, Vector3 pos, Quaternion rot) {
        GameObject returnObject = null;
        if (unSyncedPoolDictionary.ContainsKey(tag)) {
            GameObject objectToSpawn = unSyncedPoolDictionary[tag].Dequeue();

            objectToSpawn.transform.rotation = rot;
            objectToSpawn.transform.position = pos;
            objectToSpawn.SetActive(true);
            returnObject = objectToSpawn;

            IPoolObject poolObject = objectToSpawn.GetComponent(typeof(IPoolObject)) as IPoolObject;

            if (poolObject != null) {
                poolObject.OnObjectSpawn();
            }

            unSyncedPoolDictionary[tag].Enqueue(objectToSpawn);
        } else {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist");
        }

        return returnObject;
    }

    bool BoolCheck(int boolState) {
        bool state = false;
        if(boolState == 1) {
            state = true;
        }
        return state;
    }

}

[System.Serializable]
public class Pool {
    public GameObject prefab;
    public int poolSize;
}