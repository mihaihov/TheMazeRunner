using System;
using UnityEngine;


public class BallScript : MonoBehaviour
{
    private Rigidbody myRigidbody = null;
    private float volume;

    void Start()
    {
        volume = 0.2f;
        myRigidbody = (Rigidbody)GetComponent<Rigidbody>();
    }

    void OnCollisionEnter()
    {
        volume = 1.0f / myRigidbody.velocity.magnitude;
        volume = Mathf.Clamp(volume, 0.2f, 1.0f);
        GameManagerScript.Instance.PlaySound(2, volume);
    }
}
