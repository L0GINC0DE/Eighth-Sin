using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private Team startingTurn = Team.Player;
    [SerializeField] private BattleState battleState = new();

    public Team CurrentTurn => battleState.CurrentTurn;
    public BattleState State => battleState;
    public DicePool DicePool { get; private set; }
    public bool IsProcessingTurn => _isProcessingTurn;

    public event Action<Team> OnTurnChanged;
    public event Action OnPlayerTurnStarted;
    public event Action OnPlayerTurnEnded;
    public event Action OnEnemyTurnStarted;
    public event Action OnEnemyTurnEnded;
    public event Action<bool> OnBattleEnded;

    private bool _isProcessingTurn;

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
        OnEnemyTurnStarted?.Invoke();
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
            OnEnemyTurnStarted?.Invoke();
    }
}
