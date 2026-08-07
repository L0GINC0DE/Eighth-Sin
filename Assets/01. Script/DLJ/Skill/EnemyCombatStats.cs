using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyFSM))]
public sealed class EnemyCombatStats : MonoBehaviour
{
    [Min(0)]
    [SerializeField] private int defense;

    private EnemyFSM enemy;

    public int Defense => defense;

    private void Awake()
    {
        enemy = GetComponent<EnemyFSM>();
    }

    public void ApplyDamage(int damage)
    {
        if (enemy == null || enemy.Die || damage <= 0)
            return;

        enemy.NowHP = Mathf.Max(0, enemy.NowHP - damage);

        if (enemy.HPSilder != null)
            enemy.HPSilder.value = enemy.HP > 0
                ? (float)enemy.NowHP / enemy.HP
                : 0f;

        if (enemy.NowHP <= 0)
            enemy.Die = true;
    }
}
