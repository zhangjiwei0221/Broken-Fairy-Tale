using System.Collections;
using UnityEngine;

public class WolfHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 50;
    private int _currentHealth;
    private SpriteRenderer _sr;
    private bool _isDead;
    private Rigidbody2D _rb;
    private void Awake()
    {
        _currentHealth = maxHealth;
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();  // 加这行
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;  // 死亡时无敌

        _currentHealth -= damage;
        Debug.Log("狼血量：" + _currentHealth + " / " + maxHealth);

        if (_currentHealth <= 0)
        {
            _isDead = true;
            StartCoroutine(DieBlink());
            return;
        }

        StartCoroutine(HitFlash());  // 受击变红
    }

    private IEnumerator HitFlash()
    {
        _sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        _sr.color = Color.white;
    }

    private IEnumerator DieBlink()
    {
        GetComponent<WolfAI>().enabled = false;  // 禁用AI
        _rb.velocity = Vector2.zero;  // 停止移动
        float timer = 0f;
        float blinkInterval = 0.1f;
        float blinkDuration = 1f;

        while (timer < blinkDuration)
        {
            _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        Destroy(gameObject);
    }

    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => maxHealth;
}