using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

    [HideInInspector] public bool canMove = true;

    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _sr;
    private Vector2 _inputDir;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _animator = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!canMove)
        {
            _inputDir = Vector2.zero;
            _animator.SetBool("isMoving", false);
            return;
        }

        _inputDir = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        bool isMoving = _inputDir.sqrMagnitude > 0f;
        _animator.SetBool("isMoving", isMoving);
        _animator.speed = 1f;  //  º÷’’˝≥£≤•∑≈£¨»√◊¥Ã¨ª˙øÿ÷∆∂Øª≠

        if (_inputDir.x > 0f)
        {
            _sr.flipX = false;
            _animator.SetBool("isFacingRight", true);
        }
        else if (_inputDir.x < 0f)
        {
            _sr.flipX = false;
            _animator.SetBool("isFacingRight", false);
        }

        if (_inputDir.x > 0f)
        {
            _animator.SetBool("isFacingRight", true);
            Debug.Log("≥Ø”“");
        }
        else if (_inputDir.x < 0f)
        {
            _animator.SetBool("isFacingRight", false);
            Debug.Log("≥Ø◊Û");
        }
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        _rb.velocity = _inputDir * moveSpeed;
    }
}