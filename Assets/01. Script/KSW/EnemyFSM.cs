using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EnemyFSM : MonoBehaviour
{
    [Header("Turn Order")]
    [SerializeField] private int turnPriority;

    public int BasicAttackCoolTime;
    public int SkillAttackCoolTime;

    public int WaitingAttackTurn;

    public Slider HPSilder;
    [FormerlySerializedAs("TurnFillImage")]
    public Image TurnSlider;
    public TMP_Text TurnText;

    public int HP;
    public int NowHP;
    
    private enum State
    {
        Idle,
        BasicAttackWaiting,
        SkillAttackWaiting,
        BasicAttack,
        SkillAttack,
        AttackFail    
    }
    private State state;

    public bool CanAttack;
    public bool CanAttackBasic;
    public bool CanAttackSkill;
    public bool CanTurnOverAttackWaiting = true;
    public bool Die;

    public int TurnPriority => turnPriority;

    private Action<EnemyFSM> turnCompleted;

    public void ThisIsForTestEnemyTurn() //테스트
    {
        EnemyTurn();
    }
    private void Awake()
    {
        NowHP = HP;
        state = State.Idle;
    }

    public void TakeTurn(Action<EnemyFSM> onCompleted)
    {
        turnCompleted = onCompleted;

        if (!Die)
            EnemyTurn();

        CompleteTurn();
    }
    void BasicAttackEnemyTurnDowning()
    {
        WaitingAttackTurn -= 1;
        UpdateTurnDisplay(BasicAttackCoolTime);
    }
    void SkillAttackEnemyTurnDowning()
    {
        WaitingAttackTurn -= 1;
        UpdateTurnDisplay(SkillAttackCoolTime);
    }
    public void HPDown(int HPDownAmount) //<- HP를 다운시키려면 이걸 쓰세요!!
    {
        NowHP -= HPDownAmount;

        if (HPSilder != null)
            HPSilder.value = HP > 0 ? (float)NowHP / HP : 0f;
    }
    void HPDownEffect()
    {

    }
    void EnemyWaitingTurn()
    {
        Debug.Log("턴 기다리는중");
        TurnSwap();
    }
    void BasicAttackMarkAppear()
    {
        Debug.Log("할 기본공격 표시.");
        SetTurnDisplayVisible(true);
        UpdateTurnDisplay(BasicAttackCoolTime);
        TurnSwap();
    }
    void BasicAttack()
    {
        Debug.Log("기본공격!");
        SetTurnDisplayVisible(false);
        CorruptionHealth.Instance.TakeDamage(20);
        TurnSwap();
    }
    void SkillAttackMarkAppear()
    {
        Debug.Log("할 스킬공격 표시.");
        SetTurnDisplayVisible(true);
        UpdateTurnDisplay(SkillAttackCoolTime);
        TurnSwap();
    }
    void SkillAtack()
    {
        Debug.Log("스킬공격!");
        SetTurnDisplayVisible(false);
        CorruptionHealth.Instance.TakeDamage(20);
        TurnSwap();
    }
    void FailAttackMark()
    {
        Debug.Log("공격 마크 표시 실패.");
        TurnSwap();
    }
    void FailTurnOver()
    {
        Debug.Log("턴 넘기기 실패.");
        TurnSwap();
    }
    void TurnSwap()
    {
        // TurnManager가 TakeTurn 호출이 끝난 뒤 다음 적을 실행한다.
    }

    private void UpdateTurnDisplay(int coolTime)
    {
        if (TurnSlider != null)
        {
            TurnSlider.fillAmount = coolTime > 0
                ? (float)WaitingAttackTurn / coolTime
                : 0f;
        }

        if (TurnText != null)
            TurnText.text = WaitingAttackTurn.ToString();
    }

    private void SetTurnDisplayVisible(bool visible)
    {
        if (TurnSlider != null)
            TurnSlider.gameObject.SetActive(visible);
    }

    private void CompleteTurn()
    {
        Action<EnemyFSM> callback = turnCompleted;
        turnCompleted = null;
        callback?.Invoke(this);
    }
    // Update is called once per frame
    private void EnemyTurn()
    {
        if (!Die)
        {
            switch (state)
            {
                case State.Idle: //가만히ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
                    if (CanAttack)
                    {
                        if (CanAttackSkill)
                        {
                            
                            state = State.SkillAttackWaiting;
                            WaitingAttackTurn = SkillAttackCoolTime;
                            SkillAttackMarkAppear();
                        }
                        else if (CanAttackBasic)
                        {
                            
                            state = State.BasicAttackWaiting;
                            WaitingAttackTurn = BasicAttackCoolTime;
                            BasicAttackMarkAppear();
                        }
                        else
                        {
                            state = State.AttackFail;
                            EnemyTurn();
                        }
                    }
                    break;
                case State.BasicAttackWaiting: //기본공격기다리기ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
                    if (CanTurnOverAttackWaiting)
                    {
                        if (WaitingAttackTurn > 1)
                        {
                            BasicAttackEnemyTurnDowning();


                            EnemyWaitingTurn();
                        }
                        else
                        {
                            BasicAttackEnemyTurnDowning();


                            state = State.BasicAttack;
                            EnemyTurn();
                        }
                    }
                    else
                    {
                        FailTurnOver();
                    }
                    break;
                case State.SkillAttackWaiting: //스킬공격기다리기ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
                    if (CanTurnOverAttackWaiting)
                    {
                        if (WaitingAttackTurn > 1)
                        {
                            SkillAttackEnemyTurnDowning();


                            EnemyWaitingTurn();
                        }
                        else
                        {
                            SkillAttackEnemyTurnDowning();


                            state = State.SkillAttack;
                            EnemyTurn();
                        }
                    }
                    else
                    {
                        FailTurnOver();
                    }
                    break;
                case State.BasicAttack: //기본공격ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
                    if (WaitingAttackTurn < 1)
                    {
                        BasicAttack();
                        state = State.Idle;
                        EnemyTurn();
                    } 
                    break;
                case State.SkillAttack: //스킬공격ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
                    if (WaitingAttackTurn < 1)
                    {
                        SkillAtack();
                        state = State.Idle;
                        EnemyTurn();
                    }
                    break;
                case State.AttackFail: //공격실패ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
                    FailAttackMark();
                    break;
            }
        }
        else
        {

        }
    }
    
}
