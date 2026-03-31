using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameStart : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("GameStart 触发");
        DialogManager.Instance.Show("向上W，向下S，向左A，向右D\n斩击J，翻滚K\n");
    }
}