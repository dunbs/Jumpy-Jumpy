using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float spinSpeed = 10;

    private void Update()
    {
        transform.eulerAngles += Vector3.left * spinSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<Rigidbody>().velocity = Vector3.up * 3;
        GameController.Instance.isFalling = false;
    }
}
