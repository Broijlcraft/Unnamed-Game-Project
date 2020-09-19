﻿using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class ProjectileWeapon : FireArms {

    public GameObject projectilePrefab;
    public float projectileSpeed;
    public bool isAffectedByGravity;

    public override void ShootBehaviour(Transform attackOrigin) {
        Vector3 attackRotation = GetAttackRotation(attackOrigin, wBehaviour.range);
        Quaternion actualRotation = Quaternion.LookRotation(attackRotation);

        int behaviourIndex = GetBehaviourIndex(this);
        WeaponBehaviour behaviour = weaponBehaviours[behaviourIndex];

        Vector3 pos = weaponBehaviours[behaviourIndex].attackOrigins[behaviour.currentAo].origin.position;
        int playerID = PhotonRoomCustomMatchMaking.roomSingle.myNumberInRoom;
        float dmg = wBehaviour.damagePerAttack;
        float range = wBehaviour.range;
        ObjectPool.single_PT.GlobalSpawnProjectile(playerID, projectilePrefab.name, pos, actualRotation, dmg, range, projectileSpeed, isAffectedByGravity, SyncType.Synced);
    }
}