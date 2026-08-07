using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SkillPresentation))]
public class SkillSystem : MonoBehaviour
{
    [SerializeField] private SkillPresentation presentation;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private DicePool dicePool;
    [SerializeField] private SkillExecutor skillExecutor;
    [SerializeField] private EnemyFSM target;
    [SerializeField] private bool isBossTarget;

    [Header("Skill")]
    [SerializeField] private SkillSo skillSo;
    
    public SkillType skillType;

    private int pendingDiceValue;
    private int usesThisTurn;

    public UnityEvent skillActivation = new();

    private void Awake()
    {
        ResolvePresentation();
    }

    private void Start()
    {
        ResolveDependencies();
        skillActivation.AddListener(ActivateSkill);

        if (turnManager != null)
            turnManager.OnPlayerTurnStarted += ResetForPlayerTurn;
    }

    private void OnDestroy()
    {
        if (turnManager != null)
            turnManager.OnPlayerTurnStarted -= ResetForPlayerTurn;
    }

    private void ActivateSkillTest()
    {
        Debug.Log("스킬발동");
    }

    private void ActivateSkill()
    {
        ResolveDependencies();

        SkillType resolvedSkillType = skillSo != null
            ? skillSo.SkillType
            : skillType;

        if (skillSo != null && skillExecutor != null)
        {
            SkillContext context = new(
                target,
                pendingDiceValue,
                isBossTarget,
                turnManager != null ? turnManager.State.MaxCorruption : 100,
                turnManager?.State,
                turnManager
            );

            skillExecutor.Execute(skillSo, context);
        }

        usesThisTurn++;

        bool playerWon = resolvedSkillType == SkillType.Attack &&
                         target != null &&
                         target.Die &&
                         !HasLivingEnemies();

        if (playerWon)
        {
            turnManager?.EndBattle(true);
            return;
        }

        if (turnManager != null && turnManager.State.IsCorruptionLethal)
        {
            turnManager.EndBattle(false);
            return;
        }

        switch (resolvedSkillType)
        {
            case SkillType.Attack:
                turnManager?.EndPlayerTurn();
                break;
            case SkillType.Defend:
                break;
            case SkillType.Utility:
                break;
        }
    }

    public void InsertDice(GameObject dice)
    {
        if (dice == null)
            return;

        if (!CanUseThisTurn())
            return;

        DicePool activeDicePool = ResolveDicePool();
        DiceValue diceValue = dice.GetComponent<DiceValue>();

        if (activeDicePool != null &&
            (diceValue == null || !activeDicePool.TryConsumeDieValue(diceValue.Value)))
        {
            return;
        }

        pendingDiceValue = diceValue != null ? diceValue.Value : 0;

        Transform diceTransform = dice.transform;
        ObjectDrag objectDrag = dice.GetComponent<ObjectDrag>();
        Quaternion savedResultRotation =
            objectDrag != null
                ? objectDrag.ResultRotation
                : diceTransform.rotation;

        ResolvePresentation().PlayDiceInsertion(
            dice,
            savedResultRotation,
            () => CompleteSkillPresentation(dice, activeDicePool)
        );
    }

    private void CompleteSkillPresentation(GameObject dice, DicePool activeDicePool)
    {
        bool isAttackSkill = GetResolvedSkillType() == SkillType.Attack;

        if (isAttackSkill)
            presentation.Hide();

        skillActivation?.Invoke();
        activeDicePool?.TryEndPlayerTurnIfEmpty();
        dice.SetActive(false);

        if (ShouldRemainAvailable())
        {
            presentation.ResetView();
            presentation.Show();
        }
        else if (!isAttackSkill)
            presentation.Hide();
    }

    private DicePool ResolveDicePool()
    {
        if (dicePool != null)
            return dicePool;

        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        dicePool = turnManager != null
            ? turnManager.DicePool
            : FindFirstObjectByType<DicePool>();

        return dicePool;
    }

    private void ResolveDependencies()
    {
        ResolvePresentation();
        ResolveDicePool();

        if (skillExecutor == null)
        {
            skillExecutor = GetComponent<SkillExecutor>();

            if (skillExecutor == null)
                skillExecutor = gameObject.AddComponent<SkillExecutor>();
        }

        if (target == null)
            target = FindFirstObjectByType<EnemyFSM>();

        if (target != null && target.GetComponent<EnemyCombatStats>() == null)
            target.gameObject.AddComponent<EnemyCombatStats>();
    }

    public int GetExpectedDamage(int diceValue)
    {
        ResolveDependencies();

        if (skillSo == null ||
            skillExecutor == null ||
            turnManager == null ||
            target == null)
        {
            return 0;
        }

        SkillContext context = new(
            target,
            diceValue,
            isBossTarget,
            turnManager.State.MaxCorruption,
            turnManager.State,
            turnManager
        );

        return skillExecutor.CalculateExpectedDamage(skillSo, context);
    }

    private bool CanUseThisTurn()
    {
        if (turnManager != null &&
            (turnManager.State.IsBattleOver || turnManager.CurrentTurn != Team.Player))
        {
            return false;
        }

        int maxUses = skillSo != null ? skillSo.MaxUsesPerTurn : 1;
        return usesThisTurn < maxUses;
    }

    private bool ShouldRemainAvailable()
    {
        if (turnManager == null ||
            turnManager.State.IsBattleOver ||
            turnManager.CurrentTurn != Team.Player)
        {
            return false;
        }

        SkillType resolvedSkillType = skillSo != null
            ? skillSo.SkillType
            : skillType;

        return resolvedSkillType != SkillType.Attack && CanUseThisTurn();
    }

    private SkillPresentation ResolvePresentation()
    {
        if (presentation == null)
            presentation = GetComponent<SkillPresentation>();

        if (presentation == null)
            presentation = gameObject.AddComponent<SkillPresentation>();

        return presentation;
    }

    private SkillType GetResolvedSkillType()
    {
        return skillSo != null
            ? skillSo.SkillType
            : skillType;
    }

    private bool HasLivingEnemies()
    {
        EnemyFSM[] enemies = FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None);

        foreach (EnemyFSM enemy in enemies)
        {
            if (enemy != null && !enemy.Die)
                return true;
        }

        return false;
    }

    private void ResetForPlayerTurn()
    {
        usesThisTurn = 0;
        presentation.ResetView();

        if (turnManager != null && !turnManager.State.IsBattleOver)
            presentation.Show();
    }
}
