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
        Defense,
        AttackFail    
    }
    private State state;

    bool CanAttack;
    bool Attacking;
    bool CanAttackBasic;
    bool CanAttackSkill;
    bool CanDefense;
    bool Die;

    private void Start()
    {
        state = State.Idle;
        ChangeEnemyState();
    }

    // Update is called once per frame
    private void ChangeEnemyState()
    {
        if (!Die)
        {
            switch (state)
            {
                case State.Idle:
                    if(CanAttack && !Attacking)
                    {
                        if (CanAttackSkill)
                        {
                            Attacking = true;
                            state = State.SkillAttack;
                        }
                        else if(CanAttackBasic)
                        {
                            Attacking = true;
                            state = State.BasicAttack;
                        }
                        state = State.AttackFail;
                    }
                    break;
                case State.BasicAttackWaiting:
                    if (WaitingAttackTurn > 0)
                    {
                        
                    }
                    break;
                case State.SkillAttackWaiting:

                    break;
                case State.BasicAttack:

                    break;
                case State.SkillAttack:

                    break;
                case State.Defense:

                    break;
                case State.AttackFail:
                    break;
            }
        }
        else
        {

        }
    }
    
}
