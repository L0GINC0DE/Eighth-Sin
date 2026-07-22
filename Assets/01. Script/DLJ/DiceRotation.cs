using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class DiceRotation : MonoBehaviour
{
    public GameObject dices;
    
    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            dices.transform.DOKill();

            Vector3 rotationAmount = new Vector3(
                Random.Range(360, 720) * 90,
                Random.Range(360, 720) * 90,
                Random.Range(360, 720) * 90
            );

            dices.transform
                .DOLocalRotate(
                    rotationAmount,
                    1f,
                    RotateMode.LocalAxisAdd
                )
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    Vector3 angle = dices.transform.localEulerAngles;

                    dices.transform.localEulerAngles = new Vector3(
                        Mathf.Round(angle.x / 90f) * 90f,
                        Mathf.Round(angle.y / 90f) * 90f,
                        Mathf.Round(angle.z / 90f) * 90f);
                    Debug.Log("rolled");
                });

        }
    }
}
