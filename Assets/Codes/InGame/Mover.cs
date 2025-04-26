using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
public class Mover : MonoBehaviour
{
    [HideInInspector]public Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public void ContainOnMoverList()
    {
        // Mover list에다가 얘를 넣어요
        // 이벤트 호출해야함
    }

    public void DistainMoverList()
    {
        // Mover list에서 얘를 빼요
        // 이벤트 호출해야함
    }
    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetRotation(Vector3 rotation)
    {
        transform.rotation = Quaternion.Euler(rotation);
    }
}
