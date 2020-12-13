using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzelFlash : MonoBehaviour
{
    [SerializeField] private int NumFlashes; //Set in editor
    private Animator Animator { get; set; }

    private void Awake() =>
        Animator = GetComponent<Animator>();

    public void PlayFlash()
    {
        Animator.SetInteger("flashIndex", Random.Range(0, NumFlashes));
        Animator.SetTrigger("shot");
    }
}
