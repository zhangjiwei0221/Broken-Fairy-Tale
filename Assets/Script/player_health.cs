using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Ѫ��")]
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;

    [Header("������˸")]
    [SerializeField] private float invincibleTime = 0.5f;
    [SerializeField] private float blinkInterval = 0.1f;

    public bool IsRolling { get; private set; }
    private bool _isInvincible;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetRolling(bool isRolling)
    {
        IsRolling = isRolling;
    }

    public void TakeDamage(int damage)
    {
        if (_isInvincible) return;
        if (IsRolling) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        Debug.Log("Ѫ����" + _currentHealth + " / " + maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibleBlink());
    }

    private System.Collections.IEnumerator InvincibleBlink()
    {
        _isInvincible = true;
        float timer = 0f;

        while (timer < invincibleTime)
        {
            _spriteRenderer.enabled = !_spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        _spriteRenderer.enabled = true;
        _isInvincible = false;
    }

    private void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => maxHealth;
}