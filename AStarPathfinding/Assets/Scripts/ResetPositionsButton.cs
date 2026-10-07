using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetPositionsButton : MonoBehaviour
{
    public static event Action OnResetPositions;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => OnResetPositions?.Invoke());
    }
}
