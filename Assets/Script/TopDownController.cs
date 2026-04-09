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
    private float _speedMultiplier = 1f;

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
        _animator.speed = 1f;

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
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        _rb.velocity = _inputDir * moveSpeed * _speedMultiplier;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }
}