using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    public int BasicAttackCoolTime;
    public int SkillAttackCoolTime;

    public int WaitingAttackTurn;

    public int HP;
    private int NowHP;
    
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

    public void ThisIsForTestEnemyTurn() //테스트
    {
        EnemyTurn();
    }
    private void Start()
    {
        state = State.Idle;
        EnemyTurn();
    }
    public void HPDown() //<- HP를 다운시키려면 이걸 쓰세요!! HP값을 감소시키고 이거를 써야지 HP감소효과가 들어갑니당
    {
        NowHP = HP;
    }
    void EnemyWaitingTurn()
    {
        Debug.Log("턴 기다리는중");
        TurnSwap();
    }
    void BasicAttackMarkAppear()
    {
        Debug.Log("할 기본공격 표시.");
        TurnSwap();
    }
    void BasicAttack()
    {
        Debug.Log("기본공격!");
        TurnSwap();
    }
    void SkillAttackMarkAppear()
    {
        Debug.Log("할 스킬공격 표시.");
        TurnSwap();
    }
    void SkillAtack()
    {
        Debug.Log("스킬공격!");
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
                            WaitingAttackTurn -= 1;
                            EnemyWaitingTurn();
                        }
                        else
                        {
                            WaitingAttackTurn -= 1;
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
                            WaitingAttackTurn -= 1;
                            EnemyWaitingTurn();
                        }
                        else
                        {
                            WaitingAttackTurn -= 1;
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
