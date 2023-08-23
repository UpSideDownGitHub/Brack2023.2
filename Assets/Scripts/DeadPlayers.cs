using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadPlayers : MonoBehaviour
{
    public float LerpTime;
    public float destroyTime;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    public void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, LerpTime * Time.deltaTime);
    } 
}
