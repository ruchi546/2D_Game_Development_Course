using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool IsMoving => _isMoving;
    public bool SideWalking => _sideWalking;
    public bool BackWalking => _backWalking;

    [SerializeField]private float Speed = 5.0f;

    private bool _isMoving;
    private bool _sideWalking;
    private bool _backWalking;

    PlayerInput _input;
    Rigidbody2D _rigidbody;


    private float distance = 0.0f;

    private float timer = 0.0f, maxTimer = 0.1f;
    private float time = 0.0f;
    private int maxPoints = 3;
    [SerializeField] List<Vector2> positionList;

    [SerializeField]private SpriteRenderer spriteRenderer;


    private GameManager gameManager;

    void Start()
    {
        _input = GetComponent<PlayerInput>();
        _rigidbody = GetComponent<Rigidbody2D>();

        positionList = new List<Vector2>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();


    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= maxTimer)
        {
            CheckPosition();
            timer = 0;
        }

        time += Time.deltaTime;
        gameManager.SetTime(time);
    }

    private void CheckPosition()
    {
        if (positionList.Count < 2)
        {
            positionList.Add(transform.position);
        }
        else if (positionList[positionList.Count - 2] != (Vector2)transform.position)
        {
            positionList.Add(transform.position);
            distance += Vector2.Distance(positionList[positionList.Count - 2], (Vector2)transform.position);
            gameManager.SetDistance(distance);
        }

        if (positionList.Count >= maxPoints)
        {
            positionList.RemoveAt(0);
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 direction = new Vector2(_input.MovementHorizontal, _input.MovementVertical)
            * (_input.Sneak ? Speed / 2 : Speed);
        _rigidbody.velocity = direction;
        _isMoving = direction.magnitude > 0.01f;

        if (direction.y > 0.1)
        {
            _backWalking = true;
        }
        else if(direction.y < 0.1) 
        {
            _backWalking = false;
        }


        if (direction.x > 0.1)
        {
            _sideWalking = true;
            spriteRenderer.flipX = !false;

        }
        else if (direction.x < -0.1) {
            _sideWalking = true;
            spriteRenderer.flipX = !true;

        }
        else
        {
            _sideWalking = false;
        

        }





        /*  if (_isMoving) LookAt((Vector2)transform.position + direction);
          else transform.rotation = Quaternion.identity;*/
    }

    void LookAt(Vector2 targetPosition)
    {
        float angle = 0.0f;
        Vector3 relative = transform.InverseTransformPoint(targetPosition);
        angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
        transform.Rotate(0, 0, -angle);
    }

    public float GetDistance()
    {
        return distance;
    }
}
