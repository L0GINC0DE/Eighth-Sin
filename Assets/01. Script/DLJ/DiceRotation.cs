using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class DiceRotation : MonoBehaviour
{
    public GameObject dice;

    public GameObject canvas;
    public GameObject[] diceUIs;
    
    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            int result = Random.Range(1, 7); // 실제 결과

            Vector3 resultAngle = GetResultAngle(result); // 면에 대응하는 각도

            Vector3 extraSpin = new Vector3(
                Random.Range(4, 8) * 360f,
                Random.Range(4, 8) * 360f,
                Random.Range(4, 8) * 360f
            );
            
            Debug.Log(result);

            dice.transform
                .DOLocalRotate(
                    resultAngle + extraSpin,
                    1f,
                    RotateMode.FastBeyond360
                )
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    GameObject temp = Instantiate(diceUIs[result - 1], canvas.transform, false);
                    dice.SetActive(false);
                    RectTransform rectTransform = temp.GetComponent<RectTransform>();
                    rectTransform.localScale = Vector3.one;
                    rectTransform.anchoredPosition3D = Vector3.zero;
                    
                    rectTransform.DOLocalMove(new Vector3(-775, -366, 0), 1).SetEase(Ease.InOutBack);
                });
        }
    }
    
    private Vector3 GetResultAngle(int result)
    {
        return result switch
        {
            1 => new Vector3(0f, 0f, 0f),
            2 => new Vector3(0f, 90f, 0f),
            3 => new Vector3(-90f, 0f, 0f),
            4 => new Vector3(0f, -90f, 0f),
            5 => new Vector3(90f, 0f, 0f),
            6 => new Vector3(180f, 0f, 0f),
            _ => Vector3.zero
        };
    }
}
