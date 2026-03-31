using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("普通攻击")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackWinddown = 0.1f;  // 加这行
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 30f;
    [SerializeField] private float attackAngle = 120f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("翻滚")]
    [SerializeField] private float rollCooldown = 1f;
    [SerializeField] private float rollDuration = 0.3f;
    [SerializeField] private float rollSpeed = 50f;

    [Header("提示文字")]
    [SerializeField] private Text actionText;

    [Header("攻击特效")]
    [SerializeField] private GameObject slashEffect;
    [SerializeField] private float slashEffectDuration = 0.2f;

    private float _attackTimer;
    private float _rollTimer;
    private bool _isRolling;
    private bool _isAttacking;
    private Coroutine _attackCoroutine;

    private Rigidbody2D _rb;
    private TopDownController _controller;
    private Vector2 _lastMoveDir = Vector2.down;
    private Vector2 _facingDir = Vector2.down;
    private Animator _animator;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _controller = GetComponent<TopDownController>();
        _animator = GetComponent<Animator>();

        if (actionText != null)
            actionText.gameObject.SetActive(false);
    }

    private void Update()
    {
        _attackTimer -= Time.deltaTime;
        _rollTimer -= Time.deltaTime;

        if (DialogManager.Instance != null && DialogManager.Instance.gameObject.activeSelf)
            return;

        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (input.sqrMagnitude > 0f)
        {
            _lastMoveDir = input;
            _facingDir = input;
        }

        // 翻滚可以打断攻击
        if (Input.GetKeyDown(KeyCode.K) && _rollTimer <= 0f && !_isRolling)
        {
            // 打断攻击
            if (_isAttacking)
            {
                if (_attackCoroutine != null)
                    StopCoroutine(_attackCoroutine);
                _isAttacking = false;
                _controller.canMove = true;
                if (actionText != null)
                    actionText.gameObject.SetActive(false);
            }

            _rollTimer = rollCooldown;
            StartCoroutine(Roll());
            return;
        }

        // 攻击期间不能移动
        if (Input.GetKeyDown(KeyCode.J) && _attackTimer <= 0f && !_isRolling)
        {
            _attackTimer = attackCooldown;
            _attackCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _controller.canMove = false;
        _rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.25f);  // 前摇

        DoAttack();
        StartCoroutine(ShowSlashEffect());
        StartCoroutine(ShowText("攻击动画", attackWinddown));  // 加这行

        yield return new WaitForSeconds(attackWinddown);  // 后摇

        _isAttacking = false;
        _controller.canMove = true;
    }


    private void DoAttack()
    {
        StartCoroutine(ShowSlashEffect());

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Vector2 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(_facingDir, dirToEnemy);

            if (angle <= attackAngle * 0.5f)
            {
                WolfHealth wolf = hit.GetComponent<WolfHealth>();
                if (wolf != null)
                    wolf.TakeDamage(attackDamage);
            }
        }
    }

    private IEnumerator ShowSlashEffect()
    {
        if (slashEffect == null) yield break;

        float angle = Mathf.Atan2(_facingDir.y, _facingDir.x) * Mathf.Rad2Deg + 50;
        slashEffect.transform.rotation = Quaternion.Euler(1f, 1f, angle);
        slashEffect.transform.localPosition = _facingDir * 3f;

        slashEffect.SetActive(true);
        yield return new WaitForSeconds(slashEffectDuration);
        slashEffect.SetActive(false);
    }

    private IEnumerator Roll()
    {
        _isRolling = true;
        _controller.enabled = false;
        _animator.SetBool("isRolling", true);

        float timer = 0f;
        while (timer < rollDuration)
        {
            _rb.velocity = _lastMoveDir * rollSpeed;
            timer += Time.deltaTime;
            yield return null;
        }

        _rb.velocity = Vector2.zero;
        _animator.SetBool("isRolling", false);
        _controller.enabled = true;
        _isRolling = false;
    }

    private IEnumerator ShowText(string message, float duration)
    {
        if (actionText == null)
        {
            Debug.Log("actionText 是空的");
            yield break;
        }

        Debug.Log("显示文字：" + message);
        actionText.text = message;
        actionText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        actionText.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 facing = new Vector3(_facingDir.x, _facingDir.y, 0f);
        float halfAngle = attackAngle * 0.5f;

        Vector3 leftDir = Quaternion.Euler(0, 0, halfAngle) * facing;
        Vector3 rightDir = Quaternion.Euler(0, 0, -halfAngle) * facing;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * attackRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}