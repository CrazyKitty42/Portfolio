using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountPlus : MonoBehaviour
{
    [SerializeField]
    protected Counts countsScript;
    public MenuScript menuScrupt;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //при пролете через чекпоинт, добавляется 1 поинт, если игра началась
        if (menuScrupt.isPlay)
        {
            countsScript.numberLateCounts++;
        }
    }
}
