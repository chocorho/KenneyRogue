using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoguelikePlayer : Unit
{
    public StateMachine<RoguelikePlayer> statemachine;
    public Camera cam;
    public SpriteRenderer sRenderer;
    public InputManager manager;
    public Weapon currentWeapon;
    public Transform rightHand;

    public int numberOfWeapons = 3;
    public int currrentWeaponIndex = 0;
    public Weapon[] weapons = new Weapon[3];


    internal void SetPlayerNextScene(string toGoTo) {


        if (!canWin) return;
            StartCoroutine(LevelPhaseOut(toGoTo));
       
    }

    public IEnumerator LevelPhaseOut(string toGoTo) {



        yield return new WaitForSeconds(1f);
        transitioner.EndTransition();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(toGoTo);
    }

   
    public List<Weapon> HoveredWeapons = new List<Weapon>();
    public List<WeaponSlotButton> buttons;
    public float walkSpeed = 5f;
    public WeaponSlotButton slotPrefab;
    public Transform slotGroupingTransform;
    public Transitioner transitioner;
    public HealthManager healthManager;
    private void Awake() {
        canWin = true;
        transitioner.IntroTransition();
        healthManager.UpdateHP();
        statemachine = new StateMachine<RoguelikePlayer>(new PlayerMoveState(), this);
        weapons = new Weapon[numberOfWeapons];
        buttons = new List<WeaponSlotButton>();
        for (int i = 0; i < numberOfWeapons; i++) {
            WeaponSlotButton b = Instantiate(slotPrefab, slotGroupingTransform);
            buttons.Add(b);
            b.gameObject.SetActive(false);
        }
        UpdateWeaponSlotButtons();
        onTakeDamage += healthManager.UpdateHP;
        onTakeDamage += FlashForDamage;
    }

    public void ResetLevel() {
        StopAllCoroutines();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update() {

        if (manager.pauseButtonDown) {
            if (GameManager.paused) {
                GameManager.UnPause();
            } else {
                GameManager.Pause();
            }
        }

        if (manager.resetButtonDown) {
            ResetLevel();
        }

        if (GameManager.paused) return;
        statemachine.Update();
    }

    public float pushBackDecreaseSpeed = 5f;
    [HideInInspector]public Vector2 pushBack;
    public float lavaPushBack = 10f;
    public void DecreasePushBack() {

        if (pushBack != Vector2.zero) {
            pushBack = Vector2.MoveTowards(pushBack, Vector2.zero, Time.deltaTime * pushBackDecreaseSpeed);
        }

    }

    public void UpdateWeaponSlotButtons() {

        List<Weapon> ws = new List<Weapon>();

        for (int i = 0; i < numberOfWeapons; i++) {
            if (weapons[i] == null) {

            } else {
                ws.Add(weapons[i]);
                weapons[i] = null;
            }
        }

        currentNumberOfWeapons = ws.Count;

        for (int i = 0; i < numberOfWeapons; i++) {
            if (ws.Count > 0) {
                weapons[i] = ws[0];
                ws.RemoveAt(0);
            }
            if (weapons[i] == null) {
                buttons[i].gameObject.SetActive(false);
            } else {
                buttons[i].gameObject.SetActive(true);
                buttons[i].Set(weapons[i], i);
                
            }
        }
    }

    public int currentNumberOfWeapons = 0;

    public void DetachCurrentWeapon() {

        if (currentWeapon != null) {

            for (int i = 0; i < numberOfWeapons; i++) {

            }

        }
    }

    public void FlashForDamage() {
        canBeHurt = false;
        StartCoroutine(DamageFlash());
    }


    private IEnumerator DamageFlash() {

        canBeHurt = false;

        for (int i = 0; i < 3; i++) {
            yield return new WaitForSeconds(0.15f);
            sRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            sRenderer.enabled = true;
        }
        canBeHurt = true;

    }

    public void Die() {
        currentHP = 0;
        canBeHurt = false;
        statemachine.ChangeState(new DoNothingState());
        StartCoroutine(Dienumerator());
    }

    IEnumerator Dienumerator() {

        yield return null;

    }


    public bool canWin = true;

    internal void EquipWeapon(Weapon toEquip) {

        bool equipToEmpty = false;
        

        for (int i = 0; i < 3; i++) {
            if (weapons[i] == null) {
                equipToEmpty = true;
                currrentWeaponIndex = i;
            } else {
                weapons[i].gameObject.SetActive(false);
            }
        }

        if (equipToEmpty) {
            weapons[currrentWeaponIndex] = toEquip;
            toEquip.transform.SetParent(rightHand);
            toEquip.transform.localPosition = Vector3.zero;
            toEquip.transform.localRotation = Quaternion.identity;
            toEquip.upright.enabled = false;
            currentWeapon = toEquip;
            currentWeapon.pickupCollider.enabled = false;
            if (HoveredWeapons.Contains(toEquip)) {
                HoveredWeapons.Remove(toEquip);
            }
        } else {

            Weapon toDetach = weapons[currrentWeaponIndex];
            toDetach.transform.SetParent(transform.parent);
            toDetach.pickupCollider.enabled = true;
            toDetach.gameObject.SetActive(true);
            toDetach.upright.enabled = true;
            if (HoveredWeapons.Contains(toDetach)) {
                    HoveredWeapons.Remove(toDetach);
            }

            weapons[currrentWeaponIndex] = toEquip;
            toEquip.transform.SetParent(rightHand);
            toEquip.transform.localPosition = Vector3.zero;
            toEquip.transform.localRotation = Quaternion.identity;
            toEquip.upright.enabled = false;
            currentWeapon = toEquip;
            currentWeapon.pickupCollider.enabled = false;
            if (HoveredWeapons.Contains(toEquip)) {
                HoveredWeapons.Remove(toEquip);
            }


        }
        toEquip.spriteRenderer.sortingOrder = Weapon.equippedLayerorder;
        UpdateWeaponSlotButtons();
        





    }


    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.layer == Layers.hurtPlayer) {
            TakeDamage(1);
            pushBack = -(other.contacts[0].point - new Vector2(transform.position.x, transform.position.y)).normalized * lavaPushBack;
            print("takeDamage");
        }
    }


    public void AimRightHand() {

        Vector3 mousePoint = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePoint.z = 0;

        Vector3 dir = (rightHand.position - mousePoint).normalized;
        rightHand.right = -dir;


    }

}


public class PlayerMoveState : State<RoguelikePlayer> {

    public override void Update(StateMachine<RoguelikePlayer> obj) {
        if (obj.target.manager.horizontal != 0 || obj.target.manager.vertical != 0) {
            obj.target.rb.velocity = new Vector2(obj.target.manager.horizontal, obj.target.manager.vertical) * obj.target.walkSpeed;
        } else {
            obj.target.rb.velocity = Vector2.zero;
        }

        obj.target.rb.velocity += obj.target.pushBack;
        obj.target.DecreasePushBack();


        if (obj.target.currentWeapon != null) {


            if (obj.target.manager.firePressed) {
                obj.target.currentWeapon.FireDown();
            } else if (obj.target.manager.fireheld) {
                obj.target.currentWeapon.FireHeld();
            }

        }

        if (obj.target.manager.pickupButton && obj.target.HoveredWeapons.Count > 0) {
            obj.target.EquipWeapon(obj.target.HoveredWeapons[0]);
            
        }

        obj.target.AimRightHand();

    }

}

public class DoNothingState : State<RoguelikePlayer> {

}


public static class Layers {

    public static int player = 6, enemy = 7, hurtPlayer = 8;

    internal static bool InMask(LayerMask layermask, int layer) {
        return layermask == (layermask | (1 << layer));
    }
}