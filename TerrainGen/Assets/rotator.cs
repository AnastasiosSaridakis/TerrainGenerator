using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotation;

    private void FixedUpdate()
    {
        transform.eulerAngles += rotation;
    }
}
