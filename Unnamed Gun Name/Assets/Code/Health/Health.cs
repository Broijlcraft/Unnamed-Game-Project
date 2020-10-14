﻿using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Health : MonoBehaviourPun {
    public int maxHealth = 100;

    public int respawnTime = 2;

    public Image fillHealthBar;
    public HealthBarScript healthBar;

    [HideInInspector] public bool isDead, respawning;
    [HideInInspector] public int currentHealth;
    [HideInInspector] public Controller controller;
    CanvasComponents cc;
    
    private void Awake() {
        currentHealth = maxHealth;
    }

    private void Start() {
        cc = CanvasComponents.single_CC;
        healthBar = cc.healthBar;
    }

    private void Update() {
        if (Input.GetButtonDown("1")) {
            DoDamage(20, "Darth Max");
            //if (healthBar) {
            //    currentHealth -= Random.Range(1, 1);
            //    if(currentHealth <= 0) {
            //        currentHealth = maxHealth;
            //    }
            //    healthBar.ChangeHealth(currentHealth, maxHealth);
            //}
        }
    }

    public void DoDamage(int value, string killer) {
        if (!isDead) {
            photonView.RPC("RPC_ChangeHealth", RpcTarget.All, value, 0, killer);
        }
    }

    [PunRPC]
    void RPC_ChangeHealth(int value, int add, string killer) {
        if (Tools.IntToBool(add)) {
            currentHealth += value;
        } else {
            currentHealth -= value;
        }
        if (currentHealth <= 0) {
            isDead = true;
            currentHealth = 0;
            controller.animator.enabled = false;
            if (photonView.IsMine) {
                string nickname = photonView.Owner.NickName;
                nickname = Tools.RemoveIdFromNickname(nickname);
                photonView.RPC("RPC_KillFeed", RpcTarget.All, nickname, killer);
                if (!respawning) {
                    respawning = true;
                    StartCoroutine(Countdown());
                }
            }
        } else {
            isDead = false;
            if(currentHealth > maxHealth) {
                currentHealth = maxHealth;
            }
        }
        UpdateUiHeath();
    }

    IEnumerator Countdown() {
        cc.respawnUiHolder.SetActive(true);
        int timer = respawnTime;
        while (timer > 0) {
            if (Manager.single_M.IsDev()) { break; }
            cc.respawnAnim.SetTrigger("Respawn");
            cc.respawnTimer.text = timer.ToString();
            yield return new WaitForSeconds(1);
            timer--;
        }
        photonView.RPC("RPC_Respawn", RpcTarget.All);
    }

    [PunRPC]
    void RPC_Respawn() {
        isDead = false;
        respawning = false;
        controller.animator.enabled = true;
        if (photonView.IsMine) {
            cc.respawnUiHolder.SetActive(false);
            if (!Manager.single_M.IsDev()) {
                controller.ResetAtStartPosition();
            }
        }
        photonView.RPC("RPC_ChangeHealth", RpcTarget.All, maxHealth, 1, "");
    }

    [PunRPC]
    void RPC_KillFeed(string victim, string killer) {
        Debug.LogWarning("Still need to implement weapons icons here!");
        UiManager.single_UM.AddKillToFeed(System.DateTime.Now.Second + killer, victim, null, null, true);
    }

    void UpdateUiHeath() { 
        float fill = currentHealth / maxHealth;
        fillHealthBar.fillAmount = fill;
        if (photonView.IsMine) {
            healthBar.ChangeHealth(currentHealth, maxHealth);
        }
    }
}