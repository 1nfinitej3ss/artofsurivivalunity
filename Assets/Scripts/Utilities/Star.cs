using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Random.Range(0.5f , 2f);
        Invoke("StartTime" , startTime);
    }

    private void StartTime()
    {
        animator.SetBool("Run" , true);
    }
}
