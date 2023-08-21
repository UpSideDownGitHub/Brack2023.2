using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public GameObject noFlag;
    public GameObject flag;

    public void OnTriggerEnter2D(Collider2D other)
    {
        noFlag.SetActive(false);
        flag.SetActive(true);
    }
}
