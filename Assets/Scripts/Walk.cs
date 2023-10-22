using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : MonoBehaviour
{
    [SerializeField]
    public float _walkSpeed = 0;

    private Animator _animator;

    private static readonly int WalkHash = Animator.StringToHash("walk");

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        var delta = new Vector3(Input.GetAxis("Horizontal") * _walkSpeed, 0, Input.GetAxis("Vertical") * _walkSpeed);
        var position = transform.position;

        position += delta * Time.deltaTime;
        transform.position = position;

        //transform.rotation = Quaternion.LookRotation(delta);

        if ((bool)_animator && delta.sqrMagnitude > 0.01f)
        {
            _animator.SetBool(WalkHash, true);
        }
        else
        {
            _animator.SetBool(WalkHash, false);
        }
    }
}
