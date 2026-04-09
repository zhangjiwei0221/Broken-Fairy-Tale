using UnityEngine;

public class BossArrow : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float detectRadius = 20f;
    [SerializeField] private LayerMask playerLayer;

    private Vector2 _dir;
    private bool _hasHit;

    public void Init(Vector2 direction)
    {
        _dir = direction.normalized;
        float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        Destroy(gameObject, lifetime);

        // 禁用碰撞体，完全用代码检测
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    private void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        if (_hasHit) return;

        Collider2D hit = Physics2D.OverlapCircle(
            transform.position, detectRadius, playerLayer);

        Debug.Log("箭位置：" + transform.position + " 检测到：" + (hit != null ? hit.name : "无"));

        if (hit != null)
        {
            Debug.Log("箭命中玩家");
            _hasHit = true;
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}