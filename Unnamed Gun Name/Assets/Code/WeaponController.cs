﻿using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour {

    public WeaponsHolder primaryWeaponsHolder, powerWeaponsHolder;

    [Header("HideInInspector")]
    public bool isAttaching;
    public bool isDetaching;
    float animationSpeed;
    [HideInInspector] public Controller controller;

    private void Awake() {
        primaryWeaponsHolder.Init();
        powerWeaponsHolder.Init();
    }

    private void Update() {
        InputCheckAndUse(0, primaryWeaponsHolder);
        InputCheckAndUse(1, powerWeaponsHolder);
    }

    void InputCheckAndUse(int mouseInput, WeaponsHolder holder) {
        if (!isAttaching && !isDetaching) {
            if (holder.weaponAttached) {
                bool buttonPressed = false;
                int behaviourIndex = GetBehaviourIndex(holder.weaponAttached);
                switch (holder.weaponAttached.weaponBehaviours[behaviourIndex].attackType) {
                    case AttackType.Automatic:
                    if (Input.GetMouseButton(mouseInput)) {
                        buttonPressed = true;
                    }
                    break;
                    case AttackType.SemiAutomatic:
                    if (Input.GetMouseButtonDown(mouseInput)) {
                        buttonPressed = true;
                    }
                    break;
                }
                if (buttonPressed) {
                    holder.weaponAttached.Use();
                }
            }
        }
    }

    public void AttachDetachWeapon(Weapon weapon, bool useAnim) {
        if (!isAttaching && !isDetaching) {
            WeaponsHolder holder = GetHolder(weapon.weaponType);
            StartCoroutine(CheckForAndSetAttached(holder, weapon, useAnim));
        }
    }

    int GetBehaviourIndex(Weapon weapon) {
        int index = 0;
        if(weapon.weaponType == WeaponType.Primary) {
            PrimaryWeapon prim = weapon as PrimaryWeapon;
            index = prim.currentActiveWeapon;
        }
        return index;
    }

    public WeaponsHolder GetHolder(WeaponType type) {
        WeaponsHolder holder = new WeaponsHolder();
        switch (type) {
            case WeaponType.Primary:
            holder = primaryWeaponsHolder;
            break;
            case WeaponType.Power:
            holder = powerWeaponsHolder;
            break;
        }
        return holder;
    }

    IEnumerator CheckForAndSetAttached(WeaponsHolder holder, Weapon weapon, bool useAnim) {
        if (!isAttaching && !isDetaching) {
            float extraAttachWaitTime = 0f;
            if (holder.weaponAttached) {
                extraAttachWaitTime = holder.timeToDetach;
                animationSpeed = 1 / holder.timeToDetach;
                StartCoroutine(Detach(holder, useAnim));
            }
            if (useAnim) {
                yield return new WaitForSeconds(extraAttachWaitTime);
            }
            animationSpeed = 1 / holder.timeToAttach;
            StartCoroutine(Attach(holder, weapon, useAnim));
        }
    }

    IEnumerator Attach(WeaponsHolder holder, Weapon weapon, bool useAnim) {
        if (weapon) {
            isAttaching = true;
            weapon.transform.SetParent(holder.weaponsHolder);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            if (useAnim) {
                holder.animator.speed = animationSpeed;
                holder.animator.SetTrigger("ScrewOn");
                yield return new WaitForSeconds(holder.timeToAttach);
            }
        }
        holder.weaponAttached = weapon;
        isAttaching = false;
    }

    IEnumerator Detach(WeaponsHolder holder, bool useAnim) {
        isDetaching = true;
        if (useAnim) {
            holder.animator.speed = animationSpeed;
            holder.animator.SetTrigger("ScrewOff");
            yield return new WaitForSeconds(holder.timeToDetach);
        }
        holder.weaponAttached.transform.SetParent(null);
        holder.weaponAttached.ResetPosAndRot();
        holder.weaponAttached.interactingController = null;
        holder.weaponAttached = null;
        isDetaching = false;
    }
}

[System.Serializable]
public class WeaponsHolder {

    public Transform weaponsHolder;
    public float timeToAttach = 2f, timeToDetach = 1f;

    [Header("HideInInspector")]
    public Weapon weaponAttached;
    public Animator animator;

    public void Init() {
        if (weaponsHolder) {
            animator = weaponsHolder.GetComponent<Animator>();
        }
    }
}