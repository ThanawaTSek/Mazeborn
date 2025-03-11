using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestScript : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    private void Start()
    {
        inputReader.MoveEvent += HandleMove;
    }

    private void OnDestroy()
    {
        inputReader.MoveEvent -= HandleMove;
    }
    
    private void HandleMove(Vector2 movement)
    {
        /*Debug.Log(movement);*/
    }
}
