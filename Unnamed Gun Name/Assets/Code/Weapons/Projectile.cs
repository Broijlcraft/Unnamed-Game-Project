﻿using UnityEngine;
using Photon.Pun;

public class Projectile : Interactable, IPoolObject {

    public bool isAffectedByGravity;
    bool inAir;
    Rigidbody rigid;
    Vector3 startPoint;
    float range;

    public void Awake() {
        if (!rigid) {   
            rigid = GetComponent<Rigidbody>();
        }
        PhotonInit();
    }

    private void Update() {
        if (true || Tools.OwnerCheck(ownerPV)) {
            if (range < Vector3.Distance(startPoint, transform.position) && inAir) {
                OutOfRange();
            }
        }
    }    

    public override void OnObjectSpawn() {
        base.OnObjectSpawn();
    }

    public virtual void Launch(int playerID, int _damage, float _range, float projectileSpeed, bool _isAffectedByGravity) {
        startPoint = transform.position;
        range = _range;
        ownerPV = ObjectPool.single_PT.GetPhotonView(playerID);
        
        rigid.AddForce(transform.forward * projectileSpeed);

        inAir = true;

        isAffectedByGravity = _isAffectedByGravity;
        rigid.useGravity = isAffectedByGravity;
    }

    public virtual void Move() {

    }

    public virtual void OutOfRange() {
        rigid.useGravity = false;
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        inAir = false;
    }

    private void OnTriggerEnter(Collider other) {
        //if (Tools.OwnerCheck(ownerPV)) {
        //    print("Local");
        //    OutOfRange();
        //    ownerPV = null;
        //} else {
        //    print("No local");
        //}
        OutOfRange();
    }
}