using System.Collections;
using UnityEngine;

public class RedHoodBoss : MonoBehaviour
{
    [Header("阶段设置")]
    [SerializeField] private float phase2Threshold = 0.6f;  // 60%进入阶段2
    [SerializeField] private float phase3Threshold = 0.3f;  // 30%进入阶段3

    [Header("移动")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float phase3MoveSpeed = 6f;



    [Header("弩箭")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float arrowCooldown = 2f;
    [SerializeField] private float phase3ArrowCooldown = 0.8f;
    [SerializeField] private float shootRange = 150f;  // 加这行

    [Header("陷阱")]
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private float trapCooldown = 4f;

    [Header("箭雨")]
    [SerializeField] private GameObject arrowRainWarningPrefab;  // 红圈提示
    [SerializeField] private int arrowRainCount = 5;

    [Header("幻影狼")]
    [SerializeField] private GameObject phantomWolfPrefab;
    [SerializeField] private int phantomCount = 2;

    [Header("阶段特效")]
    [SerializeField] private ParticleSystem phaseEffect;

    [Header("攻击预警")]
    [SerializeField] private GameObject Exclamation;

    private int _currentPhase = 1;
    private Transform _player;
    private Rigidbody2D _rb;
    private bool _isActing;

    private float _arrowTimer;
    private float _trapTimer;
    private float _specialTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    private void Start()
    {
        _arrowTimer = arrowCooldown;
        _trapTimer = trapCooldown;
        _specialTimer = 8f;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            Debug.Log("找到玩家");
        }
        else
        {
            Debug.Log("找不到玩家");
        }
    }
    private void Update()
    {
        if (_player == null) return;

        _arrowTimer -= Time.deltaTime;
        _trapTimer -= Time.deltaTime;
        _specialTimer -= Time.deltaTime;

        switch (_currentPhase)
        {
            case 1: Phase1Behaviour(); break;
            case 2: Phase2Behaviour(); break;
            case 3: Phase3Behaviour(); break;
        }
    }

    // 检查阶段切换
    public void CheckPhase(int current, int max)
    {
        float ratio = (float)current / max;

        if (ratio <= phase3Threshold && _currentPhase < 3)
            EnterPhase(3);
        else if (ratio <= phase2Threshold && _currentPhase < 2)
            EnterPhase(2);
    }

    private void EnterPhase(int phase)
    {
        _currentPhase = phase;

        // 播放阶段切换特效
        if (phaseEffect != null)
            phaseEffect.Play();

        // 短暂无敌并暂停行动
        StartCoroutine(PhaseTransition());
    }

    private IEnumerator PhaseTransition()
    {
        _isActing = true;
        _rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1.5f);
        _isActing = false;
    }

    // 阶段1：远程弩箭三连射 + 陷阱
    private void Phase1Behaviour()
    {
        if (_isActing) return;

        MoveAroundPlayer(moveSpeed);

        float dist = Vector2.Distance(transform.position, _player.position);

        if (_arrowTimer <= 0f && dist <= shootRange)
        {
            _arrowTimer = arrowCooldown;
            StartCoroutine(TripleShot());
        }

        if (_trapTimer <= 0f && dist <= shootRange)
        {
            _trapTimer = trapCooldown;
            PlaceTrap();
        }
    }

    // 阶段2：召唤幻影狼 + 箭雨
    private void Phase2Behaviour()
    {
        if (_isActing) return;

        MoveAroundPlayer(moveSpeed);

        if (_arrowTimer <= 0f)
        {
            _arrowTimer = arrowCooldown;
            StartCoroutine(TripleShot());
        }

        if (_specialTimer <= 0f)
        {
            _specialTimer = 8f;
            StartCoroutine(Phase2Special());
        }
    }

    // 阶段3：疯狂冲锋 + 快速射击
    private void Phase3Behaviour()
    {
        if (_isActing) return;

        MoveAroundPlayer(phase3MoveSpeed);

        if (_arrowTimer <= 0f)
        {
            _arrowTimer = phase3ArrowCooldown;
            StartCoroutine(TripleShot());
        }

        if (_specialTimer <= 0f)
        {
            _specialTimer = 4f;
            StartCoroutine(Phase3Charge());
        }
    }

    private void MoveAroundPlayer(float speed)
    {
        if (_player == null) return;
        Vector2 dir = (_player.position - transform.position).normalized;
        _rb.velocity = dir * speed;
    }

    // 三连射
    private IEnumerator TripleShot()
    {
        _isActing = true;
        ShowExclamation();  // 加这行
        yield return new WaitForSeconds(0.5f);  // 预警等待时间
        for (int i = 0; i < 3; i++)
        {
            ShootArrow();
            yield return new WaitForSeconds(0.2f);
        }
        _isActing = false;
    }

    private void ShootArrow()
    {
        if (arrowPrefab == null || _player == null) return;

        Vector2 dir = (_player.position - transform.position).normalized;
        GameObject arrow = Instantiate(arrowPrefab,
            firePoint != null ? firePoint.position : transform.position,
            Quaternion.identity);

        BossArrow bossArrow = arrow.GetComponent<BossArrow>();
        if (bossArrow != null)
            bossArrow.Init(dir);
    }

    private void PlaceTrap()
    {
        if (trapPrefab == null || _player == null) return;

        Vector2 offset;
        do
        {
            offset = new Vector2(
                Random.Range(-150f, 150f),
                Random.Range(-150f, 150f));
        }
        while (offset.magnitude < 30f);  // 最小距离80，根据地图比例调整

        Vector3 spawnPos = new Vector3(
            _player.position.x + offset.x,
            _player.position.y + offset.y,
            0f);

        Instantiate(trapPrefab, spawnPos, Quaternion.identity);
    }

    // 阶段2特殊技能：召唤幻影狼 + 箭雨
    private IEnumerator Phase2Special()
    {
        _isActing = true;
        _rb.velocity = Vector2.zero;

        // 召唤幻影狼
        for (int i = 0; i < phantomCount; i++)
        {
            if (phantomWolfPrefab != null)
            {
                Vector2 spawnPos = (Vector2)transform.position +
                    new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
                Instantiate(phantomWolfPrefab, spawnPos, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // 箭雨红圈预警
        for (int i = 0; i < arrowRainCount; i++)
        {
            if (arrowRainWarningPrefab != null && _player != null)
            {
                Vector2 rainPos = (Vector2)_player.position +
                    new Vector2(Random.Range(-8f, 8f), Random.Range(-8f, 8f));
                GameObject warning = Instantiate(arrowRainWarningPrefab,
                    rainPos, Quaternion.identity);

                // 1.5秒后在红圈位置落箭
                StartCoroutine(ArrowRainStrike(warning.transform.position, 1.5f));
            }
        }

        yield return new WaitForSeconds(2f);
        _isActing = false;
    }

    private IEnumerator ArrowRainStrike(Vector2 pos, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 检测红圈位置有没有玩家
        Collider2D hit = Physics2D.OverlapCircle(pos, 1.5f);
        if (hit != null && hit.CompareTag("Player"))
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(20);
        }

        // 销毁红圈
        // 红圈预制体自己加一个定时销毁脚本就好
    }

    // 阶段3特殊技能：疯狂冲锋
    private IEnumerator Phase3Charge()
    {
        _isActing = true;

        // 蓄力0.5秒
        _rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        // 冲锋
        if (_player != null)
        {
            Vector2 dir = (_player.position - transform.position).normalized;
            _rb.velocity = dir * 20f;
            yield return new WaitForSeconds(0.3f);
        }

        _rb.velocity = Vector2.zero;
        _isActing = false;
    }

    private void ShowExclamation()
    {
        if (Exclamation != null)
        {
            Exclamation.SetActive(true);
            Invoke(nameof(HideExclamation), 0.8f);
        }
    }

    private void HideExclamation()
    {
        if (Exclamation != null)
            Exclamation.SetActive(false);
    }
}