using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    public int BasicAttackCoolTime;
    public int SkillAttackCoolTime;

    public int WaitingAttackTurn;
    
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

    bool CanAttack;
    bool Attacking;
    bool CanAttackBasic;
    bool CanAttackSkill;
    bool CanTurnOverAttackWaiting = true;
    bool Die;

    private void Start()
    {
        state = State.Idle;
        EnemyTurn();
    }
    void EnemyWaitingTurn()
    {

    }
    void BasicAttackMarkAppear()
    {

    }
    void BasicAttack()
    {

    }
    void SkillAttackMarkAppear()
    {

    }
    void SkillAtack()
    {

    }
    void FailAttackMark()
    {

    }
    void FailTurnOver()
    {

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
                    if (CanAttack && !Attacking)
                    {
                        if (CanAttackSkill)
                        {
                            Attacking = true;
                            state = State.SkillAttackWaiting;
                            SkillAttackMarkAppear();
                        }
                        else if (CanAttackBasic)
                        {
                            Attacking = true;
                            state = State.BasicAttackWaiting;
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
