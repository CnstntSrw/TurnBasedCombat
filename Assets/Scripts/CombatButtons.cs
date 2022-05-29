using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatButtons : MonoBehaviour
{
    [SerializeField]
    private Button _AttackButton;
    [SerializeField]
    private Button _SkipButton;

    public event Action AttackClicked;
    public event Action SkipClicked;
    public void OnAttackClick()
    {
        AttackClicked?.Invoke();
    }
    public void OnSkipClick()
    {
        SkipClicked?.Invoke();
    }
    public void ChangeButtonsState()
    {
        _AttackButton.interactable = !_AttackButton.interactable;
        _SkipButton.interactable = !_SkipButton.interactable;
    }
}
