using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BattleUnit))]
public class TestEnemy : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    private BattleUnit unit;

    private void Start()
    {
        unit = GetComponent<BattleUnit>();
        turnManager.AddTurn(new[] { unit });
        turnManager.StartTurn();
    }

    private void Update()
    {
        if (Keyboard.current?.spaceKey.wasPressedThisFrame == true)
            turnManager.PlayerAttack(unit);
    }
}
