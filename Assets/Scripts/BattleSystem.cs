using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    [SerializeField]
    private List<Unit> _PlayerSquad;
    [SerializeField]
    private List<Unit> _EnemySquad;
    [SerializeField]
    private CombatButtons _CombatButtons;
    [SerializeField]
    private Transform _PlayerAttackPosition;
    [SerializeField]
    private Transform _EnemyAttackPosition;
    [SerializeField]
    private TextMeshProUGUI _BattleLog;
    [SerializeField]
    private Texture2D _AttackCursor;

    private BattleState state;
    private int _CurrentPlayerUnitIndex = 0;


    void Start()
    {
        state = BattleState.START;
        _CombatButtons.AttackClicked += OnCombatButtons_AttackClicked;
        _CombatButtons.SkipClicked += OnCombatButtons_SkipClicked;
        foreach (var enemy in _EnemySquad)
        {
            enemy.InitHUD();
        }
        foreach (var unit in _PlayerSquad)
        {
            unit.InitHUD();
        }
        state = BattleState.PLAYERTURN;
        RandomizeList(ref _PlayerSquad);
    }
    IEnumerator PerformAttack(Unit attackerUnit, Unit defenderUnit)
    {
        Vector3 _DefenderStartPosition;
        Vector3 _AttackerStartPosition;
        MoveUnitsToForeground(attackerUnit, defenderUnit, out _DefenderStartPosition, out _AttackerStartPosition);

        yield return new WaitForSeconds(2f);

        attackerUnit.PlayAnimationOnce(Unit.UnitAnimations.Miner_1);
        defenderUnit.TakeDamage(attackerUnit.damage);
        _BattleLog.SetText(attackerUnit.name + " hits on " + attackerUnit.damage + " hp " + defenderUnit.name + "!");

        yield return new WaitForSeconds(2f);

        MoveUnitsToBackGround(attackerUnit, defenderUnit, _DefenderStartPosition, _AttackerStartPosition);

        yield return new WaitForSeconds(2f);
    }
    private void MoveUnitsToForeground(Unit attackerUnit, Unit defenderUnit, out Vector3 _DefenderStartPosition, out Vector3 _AttackerStartPosition)
    {
        _DefenderStartPosition = defenderUnit.transform.position;
        defenderUnit.transform.position = (state == BattleState.PLAYERTURN) ? _EnemyAttackPosition.position : _PlayerAttackPosition.position;
        defenderUnit.transform.localScale *= 1.2f;

        _AttackerStartPosition = attackerUnit.transform.position;
        attackerUnit.transform.position = (state == BattleState.PLAYERTURN) ? _PlayerAttackPosition.position : _EnemyAttackPosition.position;
        attackerUnit.transform.localScale *= 1.2f;
    }
    private void MoveUnitsToBackGround(Unit attackerUnit, Unit defenderUnit, Vector3 _DefenderStartPosition, Vector3 _AttackerStartPosition)
    {
        attackerUnit.transform.position = _AttackerStartPosition;
        defenderUnit.transform.position = _DefenderStartPosition;
        defenderUnit.transform.localScale /= 1.2f;
        attackerUnit.transform.localScale /= 1.2f;
    }

    #region Click handlers
    private void OnCombatButtons_AttackClicked()
    {
        if (state == BattleState.PLAYERTURN)
        {
            _CombatButtons.ChangeButtonsState();

            Cursor.SetCursor(_AttackCursor, Vector2.zero, CursorMode.Auto);
            foreach (var enemy in _EnemySquad)
            {
                enemy.OnUnitClicked += OnEnemyClicked;
            }
        }
    }
    private void OnCombatButtons_SkipClicked()
    {
        ChangeTurn();
    }
    private void OnEnemyClicked(Unit enemyUnit)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        if (state == BattleState.PLAYERTURN)
        {
            foreach (var enemy in _EnemySquad)
            {
                enemy.OnUnitClicked -= OnEnemyClicked;
            }
            StartCoroutine(PerformPlayerTurn(_PlayerSquad[_CurrentPlayerUnitIndex], enemyUnit));
            _CurrentPlayerUnitIndex++;
        }
    }
    #endregion

    #region Turns
    IEnumerator PerformPlayerTurn(Unit attackerUnit, Unit defenderUnit)
    {
        yield return StartCoroutine(PerformAttack(attackerUnit, defenderUnit));
        _CombatButtons.ChangeButtonsState();
        if (defenderUnit.IsDead)
        {
            var defenders = _EnemySquad;
            defenders.Remove(defenderUnit);
            if (defenders.Count == 0)
            {
                state = BattleState.WON;
                _BattleLog.SetText("You won!");
            }
        }
        if (_CurrentPlayerUnitIndex == _PlayerSquad.Count)
        {
            ChangeTurn();
        }
    }
    IEnumerator PerformEnemyTurn()
    {
        var attackers = _EnemySquad;
        var defenders = _PlayerSquad;
        RandomizeList(ref attackers);
        foreach (var attackerUnit in attackers)
        {
            var defenderUnit = GetRandomUnit(defenders);
            yield return StartCoroutine(PerformAttack(attackerUnit, defenderUnit));
            if (defenderUnit.IsDead)
            {
                _BattleLog.SetText(defenderUnit.name + " dies!");
                defenders.Remove(defenderUnit);
                if (defenders.Count == 0)
                {
                    state = BattleState.LOST;
                    _BattleLog.SetText("You've lost!");
                    break;
                }
            }
        }

        if (state != BattleState.WON && state != BattleState.LOST)
        {
            ChangeTurn();
        }
    }
    #endregion

    #region Change turns and states
    private void ChangeTurn()
    {
        _CombatButtons.ChangeButtonsState();
        state = ChangeTurnState();
        if (state == BattleState.ENEMYTURN)
        {
            _BattleLog.SetText("Enemy turn!");
            _CurrentPlayerUnitIndex = 0;
            StartCoroutine(PerformEnemyTurn());
        }
    }
    private BattleState ChangeTurnState()
    {
        return (state == BattleState.PLAYERTURN) ? BattleState.ENEMYTURN : BattleState.PLAYERTURN;
    }
    #endregion

    #region Randomize helpers
    private Unit GetRandomUnit(List<Unit> units)
    {
        return units[UnityEngine.Random.Range(0, units.Count - 1)];
    }
    private void RandomizeList(ref List<Unit> units)
    {
        units = units.OrderBy(a => Guid.NewGuid()).ToList();
    }
    #endregion 
}
