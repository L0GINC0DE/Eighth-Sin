using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoSingleton<TurnManager>
{
    public enum BattleState { Ready, PlayerTurn, EnemyTurn, Ended }
    
    [SerializeField] private BattleUnit player;
    [SerializeField] private float turnDelay = 0.8f;

    private readonly List<BattleUnit> enemies = new();
    private bool isProcessing;

    public event Action<BattleState> BattleStateChanged;

    public BattleState State { get; private set; }
    public BattleUnit CurrentUnit { get; private set; }
    public bool BattleEnded { get; private set; }
    public bool IsPlayerTurn => CurrentUnit != null && CurrentUnit == player;

    private void LateUpdate()
    {
        if (CurrentUnit != null && !isProcessing) CheckBattleEnd();
    }

    public void AddTurn(IEnumerable<BattleUnit> units)
    {
        if (isProcessing) return;

        CurrentUnit = null;
        enemies.Clear();
        BattleEnded = false;

        if (units != null) enemies.AddRange(units);
        SetBattleState(BattleState.Ready);
    }

    public void StartTurn()
    {
        if (!isActiveAndEnabled || isProcessing) return;
        if (CurrentUnit != null || BattleEnded) return;
        if (player == null) return;
        BeginPlayerTurn();
    }

    public void AttackFirstEnemy()
    {
        PlayerAttack(GetFirstLivingEnemy());
    }

    public bool PlayerAttack(BattleUnit target)
    {
        if (!isActiveAndEnabled || isProcessing) return false;
        if (!IsPlayerTurn) return false;
        if (target == null || !enemies.Contains(target) || !IsAlive(target)) return false;

        if (!new AttackCommand(player, target).Execute())
            return false;

        Debug.Log($"플레이어가 적 [{target.name}]을 공격", target);
        EndTurn();
        return true;
    }


    public void EndTurn()
    {
        if (!isActiveAndEnabled || isProcessing) return;
        if (!IsPlayerTurn) return;

        isProcessing = true;
        SetBattleState(BattleState.EnemyTurn);
        StartCoroutine(EnemyRoutine());
    }

    private void BeginPlayerTurn()
    {
        if (CheckBattleEnd()) return;
        CurrentUnit = player;
        SetBattleState(BattleState.PlayerTurn);
        Debug.Log("플레이어 턴", player);
    }

    private bool CheckBattleEnd()
    {
        if (BattleEnded) return true;
        if (IsAlive(player) && GetFirstLivingEnemy() != null) return false;

        BattleEnded = true;
        CurrentUnit = null;
        SetBattleState(BattleState.Ended);
        Debug.Log(IsAlive(player) ? "승리" : "패배", this);
        return true;
    }

    private void SetBattleState(BattleState nextState)
    {
        if (State == nextState) return;

        State = nextState;
        BattleStateChanged?.Invoke(State);
    }

    public BattleUnit GetFirstLivingEnemy() => enemies.Find(IsAlive);

    

    private IEnumerator EnemyRoutine()
    {
        foreach (var enemy in enemies)
        {
            EnemyTurn(enemy);
            yield return new WaitForSeconds(turnDelay);

            if (!CheckBattleEnd() || IsAlive(enemy))
            {
                new AttackCommand(CurrentUnit, player).Execute();
                Debug.Log($"적 공격");
            }
        }

        isProcessing = false;
        BeginPlayerTurn();
    }

    private void EnemyTurn(BattleUnit enemy)
    {
        if (CheckBattleEnd() || !IsAlive(enemy)) return;
        CurrentUnit = enemy;
        Debug.Log($"적 턴: {CurrentUnit.name}", enemy);
    }


    private static bool IsAlive(BattleUnit unit) => unit != null && unit.IsAlive;
}
