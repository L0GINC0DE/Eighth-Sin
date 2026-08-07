using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private Team startingTurn = Team.Player;
    [SerializeField] private BattleState battleState = new();

    [Header("Enemies")]
    [SerializeField] private bool autoFindEnemies = true;
    [SerializeField] private List<EnemyFSM> enemies = new();

    public Team CurrentTurn => battleState.CurrentTurn;
    public BattleState State => battleState;
    public DicePool DicePool { get; private set; }
    public bool IsProcessingTurn => _isProcessingTurn;
    public IReadOnlyList<EnemyFSM> Enemies => enemies;

    public event Action<Team> OnTurnChanged;
    public event Action OnPlayerTurnStarted;
    public event Action OnPlayerTurnEnded;
    public event Action OnEnemyTurnStarted;
    public event Action OnEnemyTurnEnded;
    public event Action<bool> OnBattleEnded;

    private bool _isProcessingTurn;
    private readonly List<EnemyFSM> _enemyTurnOrder = new();
    private EnemyFSM _activeEnemy;
    private int _enemyTurnIndex;

    private void Awake()
    {
        battleState ??= new BattleState();
        battleState.BeginBattle(startingTurn);

        DicePool = GetComponent<DicePool>();

        if (DicePool == null)
            DicePool = gameObject.AddComponent<DicePool>();

        DicePool.Initialize(this);
    }

    private void Start()
    {
        NotifyTurnStarted(CurrentTurn);
    }

    public void EndPlayerTurn()
    {
        TryEndPlayerTurn();
    }

    public bool TryEndPlayerTurn()
    {
        if (_isProcessingTurn ||
            battleState.IsBattleOver ||
            CurrentTurn != Team.Player)
        {
            return false;
        }

        _isProcessingTurn = true;
        OnPlayerTurnEnded?.Invoke();
        SetTurn(Team.Enemy);
        BeginEnemyTurn();
        return true;
    }

    public bool TryEndPlayerTurnIfDiceDepleted()
    {
        if (battleState.HasDice)
            return false;

        return TryEndPlayerTurn();
    }

    public void EndEnemyTurn()
    {
        if (battleState.IsBattleOver || CurrentTurn != Team.Enemy)
            return;

        _activeEnemy = null;
        _enemyTurnOrder.Clear();
        OnEnemyTurnEnded?.Invoke();
        _isProcessingTurn = false;
        SetTurn(Team.Player);
        DicePool.RollForPlayerTurn();
        OnPlayerTurnStarted?.Invoke();
    }

    public void EndBattle(bool playerWon)
    {
        if (battleState.IsBattleOver)
            return;

        battleState.EndBattle(playerWon);
        _isProcessingTurn = false;
        OnBattleEnded?.Invoke(playerWon);
    }

    private void SetTurn(Team team)
    {
        battleState.SetTurn(team);
        OnTurnChanged?.Invoke(team);
        Debug.Log($"Turn changed: {team}");
    }

    private void NotifyTurnStarted(Team team)
    {
        OnTurnChanged?.Invoke(team);

        if (team == Team.Player)
        {
            DicePool.RollForPlayerTurn();
            OnPlayerTurnStarted?.Invoke();
        }
        else
        {
            _isProcessingTurn = true;
            BeginEnemyTurn();
        }
    }

    private void BeginEnemyTurn()
    {
        OnEnemyTurnStarted?.Invoke();
        BuildEnemyTurnOrder();
        _enemyTurnIndex = 0;
        ProcessNextEnemy();
    }

    private void BuildEnemyTurnOrder()
    {
        _enemyTurnOrder.Clear();

        foreach (EnemyFSM enemy in enemies)
            AddEnemyToTurnOrder(enemy);

        if (autoFindEnemies)
        {
            EnemyFSM[] foundEnemies =
                FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None);

            foreach (EnemyFSM enemy in foundEnemies)
                AddEnemyToTurnOrder(enemy);
        }

        _enemyTurnOrder.Sort((left, right) =>
        {
            int priorityComparison =
                right.TurnPriority.CompareTo(left.TurnPriority);

            return priorityComparison != 0
                ? priorityComparison
                : left.GetInstanceID().CompareTo(right.GetInstanceID());
        });
    }

    private void AddEnemyToTurnOrder(EnemyFSM enemy)
    {
        if (enemy != null && !_enemyTurnOrder.Contains(enemy))
            _enemyTurnOrder.Add(enemy);
    }

    private void ProcessNextEnemy()
    {
        if (battleState.IsBattleOver || CurrentTurn != Team.Enemy)
            return;

        while (_enemyTurnIndex < _enemyTurnOrder.Count)
        {
            EnemyFSM enemy = _enemyTurnOrder[_enemyTurnIndex++];

            if (enemy == null || enemy.Die || !enemy.isActiveAndEnabled)
                continue;

            _activeEnemy = enemy;
            enemy.TakeTurn(HandleEnemyTurnCompleted);
            return;
        }

        EndEnemyTurn();
    }

    private void HandleEnemyTurnCompleted(EnemyFSM enemy)
    {
        if (enemy != _activeEnemy)
            return;

        _activeEnemy = null;
        ProcessNextEnemy();
    }
}
