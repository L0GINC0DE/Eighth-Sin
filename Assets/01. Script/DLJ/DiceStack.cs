using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiceStack : MonoBehaviour
{
    private const float OffscreenPadding = 12f;

    [SerializeField] private TMP_Text[] diceLabels = System.Array.Empty<TMP_Text>();
    [SerializeField] private TMP_Text stackLabel;
    [SerializeField] private RectTransform hand;
    [SerializeField] private RectTransform secondHand;
    [SerializeField, Min(0f)] private float handRevealHeight = 400f;
    [SerializeField, Min(0f)] private float slideDuration = 0.25f;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private Button attackButton;
    [SerializeField] private TMP_Text currentTurnLabel;

    private readonly Dictionary<RectTransform, Vector2> diceStartPositions = new();
    private readonly Dictionary<RectTransform, float> diceRestPositions = new();
    private RectTransform stackRect;
    private RectTransform activeDie;
    private Vector2 handRestPosition;
    private Vector2 secondHandRestPosition;
    private Sequence slide;
    private bool isPresenting;
    private int stackCount;

    private void Awake()
    {
        stackRect = GetComponent<RectTransform>();
        foreach (DiceDrag die in GetComponentsInChildren<DiceDrag>(true))
        {
            RectTransform dieRect = (RectTransform)die.transform;
            if (dieRect.parent == stackRect)
            {
                diceStartPositions.Add(dieRect, dieRect.anchoredPosition);
                diceRestPositions.Add(dieRect, dieRect.anchoredPosition.y);
            }
        }

        if (hand != null)
            handRestPosition = hand.anchoredPosition;
        if (secondHand != null)
            secondHandRestPosition = secondHand.anchoredPosition;
    }

    private void OnEnable()
    {
        if (turnManager == null)
            return;

        turnManager.BattleStateChanged += UpdateBattleHud;
        UpdateBattleHud(turnManager.State);
    }

    private void Start()
    {
        if (turnManager != null)
            return;

        RollDice();
        UpdateLayout(showHand: false, animate: false);
    }

    public void RollDice()
    {
        foreach (TMP_Text label in diceLabels)
        {
            if (label != null)
                label.text = Random.Range(1, 7).ToString();
        }
    }

    public bool TryBeginDrag(RectTransform die)
    {
        if (!isActiveAndEnabled || activeDie != null || !diceRestPositions.ContainsKey(die))
            return false;

        activeDie = die;
        UpdateLayout(showHand: true, animate: true);
        return true;
    }

    public void EndDrag(RectTransform die, PointerEventData drop = null)
    {
        if (activeDie != die || isPresenting)
            return;

        if (isActiveAndEnabled && drop != null && hand != null && hand.gameObject.activeInHierarchy &&
            RectTransformUtility.RectangleContainsScreenPoint(hand, drop.position, drop.pressEventCamera))
        {
            PresentDie(die);
            return;
        }

        diceRestPositions[die] = die.anchoredPosition.y;
        activeDie = null;
        if (isActiveAndEnabled)
            UpdateLayout(showHand: false, animate: true);
    }

    private void OnDisable()
    {
        if (turnManager != null)
            turnManager.BattleStateChanged -= UpdateBattleHud;

        if (isPresenting)
            ConsumePresentedDie();

        UpdateLayout(showHand: false, animate: false);
    }

    private void UpdateBattleHud(TurnManager.BattleState state)
    {
        if (currentTurnLabel != null)
        {
            currentTurnLabel.text = state switch
            {
                TurnManager.BattleState.PlayerTurn => "Current Turn: Player",
                TurnManager.BattleState.EnemyTurn => "Current Turn: Enemy",
                TurnManager.BattleState.Ended => "Battle Over",
                _ => "Current Turn: -"
            };
        }

        SetPlayerDiceAvailable(state == TurnManager.BattleState.PlayerTurn);
    }

    private void SetPlayerDiceAvailable(bool available)
    {
        if (attackButton != null)
            attackButton.interactable = available;

        if (!available)
            ConsumePresentedDie();

        foreach (var entry in diceStartPositions)
        {
            RectTransform die = entry.Key;
            if (die == null)
                continue;

            if (available)
            {
                die.anchoredPosition = entry.Value;
                diceRestPositions[die] = entry.Value.y;
            }

            die.gameObject.SetActive(available);
        }

        if (available)
            RollDice();

        UpdateLayout(showHand: false, animate: false);
    }

    private void UpdateLayout(bool showHand, bool animate)
    {
        slide?.Kill();
        slide = animate && slideDuration > 0f ? DOTween.Sequence().SetUpdate(true) : null;

        MoveOtherDice(hide: showHand);
        MoveHand(hand, handRestPosition, showHand);
        MoveHand(secondHand, secondHandRestPosition, show: false);
    }

    private void MoveOtherDice(bool hide)
    {
        foreach (var entry in diceRestPositions)
        {
            RectTransform die = entry.Key;
            if (die == null || die == activeDie || !die.gameObject.activeSelf)
                continue;

            Vector2 target = die.anchoredPosition;
            target.y = hide ? Mathf.Min(entry.Value, PositionForTop(die, -OffscreenPadding)) : entry.Value;
            MoveTo(die, target);
        }
    }

    private void MoveHand(RectTransform image, Vector2 restPosition, bool show)
    {
        if (image == null)
            return;

        Vector2 target = restPosition;
        target.y = show
            ? PositionForTop(image, handRevealHeight)
            : Mathf.Min(restPosition.y, PositionForTop(image, -OffscreenPadding));
        MoveTo(image, target);
    }

    private void PresentDie(RectTransform die)
    {
        isPresenting = true;
        stackCount++;
        if (stackLabel != null)
            stackLabel.text = $"Stack {stackCount}";

        slide?.Kill();
        slide = DOTween.Sequence().SetUpdate(true);
        MoveOtherDice(hide: true);

        Vector2 center = stackRect.rect.center;
        center.y -= stackRect.rect.height * 0.13f;
        MoveToCenter(die, center);

        float spacing = die.rect.width * 0.5f + OffscreenPadding;
        MoveToCenter(hand, center + new Vector2(spacing + hand.rect.width * 0.5f, 0f));
        if (secondHand != null)
            MoveToCenter(secondHand, center - new Vector2(spacing + secondHand.rect.width * 0.5f, 0f));

        slide.AppendInterval(2f).AppendCallback(() =>
        {
            slide = null;
            ConsumePresentedDie();
            UpdateLayout(showHand: false, animate: true);
        });
    }

    private void ConsumePresentedDie()
    {
        RectTransform die = activeDie;
        activeDie = null;
        isPresenting = false;
        if (die != null)
            die.gameObject.SetActive(false);
    }

    private void MoveToCenter(RectTransform element, Vector2 center)
    {
        Vector2 currentCenter = (Vector2)RectTransformUtility.CalculateRelativeRectTransformBounds(stackRect, element).center;
        MoveTo(element, element.anchoredPosition + center - currentCenter);
    }

    private float PositionForTop(RectTransform element, float heightAboveBottom)
    {
        // The hand and dice share the stack's coordinate space.
        float targetTop = stackRect.rect.yMin + heightAboveBottom;
        float currentTop = RectTransformUtility.CalculateRelativeRectTransformBounds(stackRect, element).max.y;
        return element.anchoredPosition.y + targetTop - currentTop;
    }

    private void MoveTo(RectTransform element, Vector2 position)
    {
        if (slide != null)
        {
            slide.Join(element.DOAnchorPos(position, slideDuration));
            return;
        }

        element.anchoredPosition = position;
    }
}
