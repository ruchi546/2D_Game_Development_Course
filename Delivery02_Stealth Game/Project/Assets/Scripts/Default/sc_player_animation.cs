using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField]private Animator _animator;
    private PlayerInput _input;
    private PlayerMovement _movement;

    void Start()
    {
        _input = GetComponent<PlayerInput>();
        _movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {

        _animator.SetBool("Walk", _movement.IsMoving);
        _animator.SetBool("SideWalk", _movement.SideWalking);
        _animator.SetBool("BackWalk", _movement.BackWalking);
    }
}
