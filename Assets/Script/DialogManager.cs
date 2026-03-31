using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [SerializeField] private Text dialogText;
    [SerializeField] private KeyCode confirmKey = KeyCode.J;

    private Queue<string> _dialogQueue = new Queue<string>();
    private bool _waitingForConfirm;
    private TopDownController _playerController;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        gameObject.SetActive(true);
        dialogText.text = message;
        _waitingForConfirm = true;
        LockPlayer();
    }

    public void Show(string[] messages)
    {
        _dialogQueue.Clear();
        foreach (string msg in messages)
            _dialogQueue.Enqueue(msg);

        ShowNext();
    }

    public void Show(string message, float duration)
    {
        gameObject.SetActive(true);
        dialogText.text = message;
        _waitingForConfirm = false;
        StartCoroutine(AutoClose(duration));
    }

    private void ShowNext()
    {
        if (_dialogQueue.Count == 0)
        {
            gameObject.SetActive(false);
            _waitingForConfirm = false;
            UnlockPlayer();
            return;
        }

        gameObject.SetActive(true);
        dialogText.text = _dialogQueue.Dequeue();
        _waitingForConfirm = true;
        LockPlayer();
    }

    private void Update()
    {
        if (_waitingForConfirm && Input.GetKeyDown(confirmKey))
            ShowNext();
    }

    private IEnumerator AutoClose(float duration)
    {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
        UnlockPlayer();
    }

    private void LockPlayer()
    {
        if (_playerController == null)
            _playerController = FindObjectOfType<TopDownController>();

        if (_playerController != null)
        {
            _playerController.canMove = false;
            Debug.Log("锁定角色");
        }
        else
        {
            Debug.Log("找不到PlayerController");
        }
    }

    private void UnlockPlayer()
    {
        if (_playerController == null)
            _playerController = FindObjectOfType<TopDownController>();

        if (_playerController != null)
            _playerController.canMove = true;
    }
}