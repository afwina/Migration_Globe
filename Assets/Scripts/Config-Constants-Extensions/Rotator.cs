using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float Speed;
    public Vector3 Direction;

    void Update()
    {
        transform.Rotate(Direction.x * Speed, Direction.y * Speed, Direction.z * Speed);
    }
}
