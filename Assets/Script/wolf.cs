using UnityEngine;

public class WolfAI : MonoBehaviour
{
    [Header("¼ì²â")]
    [SerializeField] private float detectRange = 80f;
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private float loseRange = 120f;

    [Header("ÒÆ¶¯")]
    [SerializeField] private float wanderSpeed = 1f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float wanderRadius = 1.5f;

    [Header("¹¥»÷")]
    [SerializeField] private float biteCooldown = 1.5f;
    [SerializeField] private float biteDuration = 0.5f;
    [SerializeField] private float biteWindup = 0.4f;  // Ç°Ò¡Ê±¼ä£¬¼ÓÕâÐÐ
    [SerializeField] private int biteDamage = 10;

    [Header("¹¥»÷ÅÐ¶¨")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackPointRadius = 15f;
    [SerializeField] private LayerMask playerLayer;

    [Header("¸ÐÌ¾ºÅ")]
    [SerializeField] private GameObject exclamationMark;

    private enum State { Wander, Chase, Bite }
    private State _state = State.Wander;

    private Rigidbody2D _rb;
    private Transform _player;
    private Vector2 _wanderTarget;
    private Vector2 _spawnPos;

    private float _biteTimer;
    private float _biteStateTimer;
    private float _wanderTimer;
    private bool _hasDetected = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _spawnPos = transform.position;
        _wanderTarget = _spawnPos;

        if (exclamationMark != null)
            exclamationMark.SetActive(false);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void Update()
    {
        if (DialogManager.Instance != null && DialogManager.Instance.gameObject.activeSelf)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        if (_player == null) return;

        _biteTimer -= Time.deltaTime;

        float distToPlayer = Vector2.Distance(transform.position, _player.position);

        switch (_state)
        {
            case State.Wander:
                Wander();
                if (distToPlayer < detectRange && !_hasDetected)
                {
                    _state = State.Chase;
                    _hasDetected = true;
                }
                else if (distToPlayer < detectRange)
                {
                    _state = State.Chase;
                }
                break;

            case State.Chase:
                Chase();
                if (distToPlayer > loseRange)
                {
                    _state = State.Wander;
                    _hasDetected = false;
                    break;
                }
                if (distToPlayer <= attackRange && _biteTimer <= 0f)
                {
                    _state = State.Bite;
                    _biteStateTimer = biteDuration;
                    ShowExclamation();
                    Invoke(nameof(DoBite), biteWindup);  // ÑÓ³ÙÖ´ÐÐ¿ÛÑª
                    break;
                }
                break;

            case State.Bite:
                _rb.velocity = Vector2.zero;
                _biteStateTimer -= Time.deltaTime;
                if (_biteStateTimer <= 0f)
                    _state = State.Chase;
                break;
        }
    }

    private void ShowExclamation()
    {
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(true);
            Invoke(nameof(HideExclamation), 0.8f);
        }
    }

    private void HideExclamation()
    {
        if (exclamationMark != null)
            exclamationMark.SetActive(false);
    }

    private void Wander()
    {
        _wanderTimer -= Time.deltaTime;

        if (_wanderTimer <= 0f)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-wanderRadius, wanderRadius),
                Random.Range(-wanderRadius, wanderRadius)
            );
            _wanderTarget = _spawnPos + randomOffset;
            _wanderTimer = Random.Range(1.5f, 3f);
        }

        Vector2 dir = (_wanderTarget - (Vector2)transform.position).normalized;
        float dist = Vector2.Distance(transform.position, _wanderTarget);

        if (dist > 0.1f)
            _rb.velocity = dir * wanderSpeed;
        else
            _rb.velocity = Vector2.zero;
    }

    private void Chase()
    {
        Vector2 dir = (_player.position - transform.position).normalized;
        _rb.velocity = dir * chaseSpeed;
    }

    private void DoBite()
    {
        _biteTimer = biteCooldown;

        Vector2 checkPos = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, attackPointRadius, playerLayer);

        Debug.Log("¹¥»÷ÅÐ¶¨£º" + (hit != null ? hit.name : "Î´ÃüÖÐ"));

        if (hit != null)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(biteDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}