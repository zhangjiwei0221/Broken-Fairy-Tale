using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("血量")]
    [SerializeField] private int maxHealth = 300;
    private int _currentHealth;

    [Header("血条UI")]
    [SerializeField] private Slider healthBar;

    [Header("阶段特效")]
    [SerializeField] private ParticleSystem phaseEffect;

    private SpriteRenderer _sr;
    private RedHoodBoss _boss;
    private bool _isDead;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _sr = GetComponent<SpriteRenderer>();
        _boss = GetComponent<RedHoodBoss>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;
        Debug.Log("Boss受伤：" + damage);
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        if (healthBar != null)
            healthBar.value = _currentHealth;

        StartCoroutine(HitFlash());

        if (_boss != null)
            _boss.CheckPhase(_currentHealth, maxHealth);

        if (_currentHealth <= 0)
            Die();
    }

    private System.Collections.IEnumerator HitFlash()
    {
        _sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        _sr.color = Color.white;
    }

    private void Die()
    {
        _isDead = true;
        // Boss死亡处理
        Destroy(gameObject, 1f);
    }
}