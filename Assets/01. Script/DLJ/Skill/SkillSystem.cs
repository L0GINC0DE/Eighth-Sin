using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;



public class SkillSystem : MonoBehaviour
{
    [SerializeField] private GameObject skillObject;
    [SerializeField] private float insertDistance = 1f;
    [SerializeField] private float insertDepth = 0.5f;
    [SerializeField] private float insertDuration = 1f;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private DicePool dicePool;
    [SerializeField] private SkillExecutor skillExecutor;
    [SerializeField] private EnemyFSM target;
    [SerializeField] private bool isBossTarget;

    [Header("Skill")]
    [SerializeField] private SkillSo skillSo;
    
    public SkillType skillType;

    private Sequence diceSequence;
    private int pendingDiceValue;
    private int usesThisTurn;
    private Vector3 initialLocalPosition;
    
    //public event Action<bool> OnDiceSequence;

    public UnityEvent skillActivation = new();

    private void Awake()
    {
        if (skillObject == null)
            skillObject = gameObject;

        initialLocalPosition = transform.localPosition;
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
        Debug.Log(000000);
        if (dice == null || skillObject == null)
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

        Transform diceSocket = skillObject.transform.Find("DiceSocket");
        Transform cube = skillObject.transform.Find("Cube");

        Debug.Log(11111);

        if (diceSocket == null)
            diceSocket = skillObject.transform;

        Transform diceTransform = dice.transform;
        ObjectDrag objectDrag = dice.GetComponent<ObjectDrag>();
        Quaternion savedResultRotation =
            objectDrag != null
                ? objectDrag.ResultRotation
                : diceTransform.rotation;
        Quaternion cubeRotation =
            cube != null ? cube.rotation : diceSocket.rotation;
        Quaternion insertRotation =
            cubeRotation * savedResultRotation;
        Vector3 targetPosition =
            diceSocket.position - diceSocket.forward * insertDepth;
        Vector3 startPosition =
            diceSocket.position + diceSocket.forward * insertDistance;

        diceTransform.DOKill();
        diceTransform.SetParent(null, true);
        diceTransform.position = startPosition;
        diceTransform.rotation = insertRotation;

        diceSequence?.Kill();
        diceSequence = DOTween.Sequence();
        diceSequence.Append(
            diceTransform
                .DOMove(targetPosition, insertDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    diceTransform.SetParent(transform, true);
                    diceTransform.rotation = insertRotation;
                })
        );
        diceSequence.AppendInterval(0.2f);
        diceSequence.Append(transform.DOLocalMoveX(transform.localPosition.x - 5, insertDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                skillActivation?.Invoke();
                activeDicePool?.TryEndPlayerTurnIfEmpty();

                if (ShouldRemainAvailable())
                {
                    transform.localPosition = initialLocalPosition;
                    dice.SetActive(false);
                }
                else
                    gameObject.SetActive(false);
            }));
        
        Debug.Log("activate");
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
        transform.localPosition = initialLocalPosition;

        if (turnManager != null && !turnManager.State.IsBattleOver)
            gameObject.SetActive(true);
    }
}
