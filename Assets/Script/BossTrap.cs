using System.Collections;
using UnityEngine;

public class BossTrap : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float slowAmount = 0.4f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float tickInterval = 0.8f;
    [SerializeField] private float detectRadius = 30f;
    [SerializeField] private LayerMask playerLayer;

    private bool _hasPlayer;
    private TopDownController _playerController;
    private PlayerHealth _playerHealth;

    private void Start()
    {
        StartCoroutine(TrapLife());
    }

    private void Update()
    {


        Collider2D hit = Physics2D.OverlapCircle(
        transform.position, detectRadius, playerLayer);

        Debug.Log("陷阱检测：" + (hit != null ? hit.name : "无") + " 位置：" + transform.position);

        if (hit != null && !_hasPlayer)
        {
            Debug.Log("玩家踩到陷阱");
            _hasPlayer = true;
            _playerHealth = hit.GetComponent<PlayerHealth>();
            _playerController = hit.GetComponent<TopDownController>();

            if (_playerController != null)
                _playerController.SetSpeedMultiplier(slowAmount);

            StartCoroutine(TrapDamage());
        }
        else if (hit == null && _hasPlayer)
        {
            Debug.Log("玩家离开陷阱");
            _hasPlayer = false;
            if (_playerController != null)
                _playerController.SetSpeedMultiplier(1f);
        }
    }

    private IEnumerator TrapDamage()
    {
        if (_playerHealth != null && !_playerHealth.IsRolling)
            _playerHealth.TakeDamage(damage);

        // 踩上去立刻销毁
        if (_playerController != null)
            _playerController.SetSpeedMultiplier(1f);

        Destroy(gameObject);
        yield break;
    }
    private IEnumerator TrapLife()
    {
        yield return new WaitForSeconds(duration);

        if (_playerController != null)
            _playerController.SetSpeedMultiplier(1f);

        Destroy(gameObject);
    }
}