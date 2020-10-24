using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorController : MonoBehaviour
{
    private const float shatterForce = 3f;
    public float fallDuration = 1f;
    public event EventHandler OnDisabled;

    float fallTime;
    List<FloorComponents> childs = new List<FloorComponents>();

    public GameObject Prefab => gameObject;

    public void StartFalling()
    {
        fallTime = fallDuration;
        foreach (var child in childs)
        {
            child.velocity = (child.gameObject.transform.position - transform.position) * shatterForce;
        }
    }


    private void Start()
    {
        foreach (Transform child in transform)
        {
            childs.Add(new FloorComponents(child.gameObject, Vector3.zero));
        }
    }

    private void OnEnable()
    {
        fallTime = 0;
        foreach (var child in childs)
        {
            child.Reset();
        }
    }

    private void OnDisable()
    {
        OnDisabled?.Invoke(this, new EventArgs());
    }

    // Floor rơi
    void Update()
    {
        // Rơi đủ rồi, dừng thôi
        if (fallTime <= 0)
        {
            return;
        }
        foreach (var child in childs)
        {
            child.velocity += Physics.gravity * Time.deltaTime;
            child.gameObject.transform.position += child.velocity * Time.deltaTime;
        }

        fallTime -= Time.deltaTime;
    }
}

public class FloorComponents
{
    public GameObject gameObject;
    public Vector3 velocity;
    private readonly Vector3 originalPosition;
    private readonly Quaternion originalRotation;

    public FloorComponents(GameObject gameObject, Vector3 velocity)
    {
        this.gameObject = gameObject;
        this.velocity = velocity;
        this.originalPosition = gameObject.transform.localPosition;
        this.originalRotation = gameObject.transform.localRotation;
    }

    public void Reset()
    {
        velocity = Vector3.zero;
        gameObject.transform.localPosition = originalPosition;
        gameObject.transform.localRotation = originalRotation;
    }
}
