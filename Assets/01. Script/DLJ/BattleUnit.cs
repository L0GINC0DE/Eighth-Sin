using UnityEngine;

    public enum UnitType { Player, Enemy, Boss }

    public class BattleUnit : MonoBehaviour
    {
        public UnitType type;
        public Attacker AttackCompo { get; private set; }
        public HealthModule HealthCompo { get; private set; }
        public bool IsAlive => isActiveAndEnabled && HealthCompo != null && !HealthCompo.IsDead;

        private void Awake()
        {
            AttackCompo = GetComponent<Attacker>();
            HealthCompo = GetComponent<HealthModule>();
        }
    }
