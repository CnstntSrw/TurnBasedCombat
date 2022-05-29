using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IPointerDownHandler
{
    public SkeletonAnimation _SpineObject;

    public event Action<Unit> OnUnitClicked;

    public string unitName;
    public int unitLevel;

    public int damage;

    public int maxHP;
    public int currentHP;
    public bool IsDead => (currentHP <= 0);

    private BattleHUD _BattleHUD;

    public enum UnitAnimations
    {
        Damage,
        DoubleShift,
        Idle,
        Miner_1,
        PickaxeCharge,
        Pull
    }

    private void Awake()
    {
        _SpineObject = GetComponentInChildren<SkeletonAnimation>();
    }
    public void PlayAnimationOnce(UnitAnimations animation)
    {
        _SpineObject.AnimationState.SetAnimation(0, animation.ToString(), true);
        _SpineObject.AnimationState.AddAnimation(0, UnitAnimations.Idle.ToString(), true, 0);
    }

    public void InitHUD()
    {
        _BattleHUD = GetComponentInChildren<BattleHUD>();
        _BattleHUD.SetHUD(maxHP, currentHP);
    }
    private void UpdateHUD()
    {
        _BattleHUD.SetHP(currentHP);
    }
    public void TakeDamage(int dmg)
    {
        PlayAnimationOnce(UnitAnimations.Damage);
        currentHP -= dmg;
        UpdateHUD();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        OnUnitClicked?.Invoke(this);
    }
}
