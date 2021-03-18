using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Counts : MonoBehaviour
{
    public int pubNumberCounts;
    private int numberCounts = 0;
    [HideInInspector]
    public int numberLateCounts = 0;

    public Text textCounts;


    private void LateUpdate()
    {
        //Проверка на читерство и вывод в GUI
        if(numberLateCounts - 1 == numberCounts)
        {
            numberCounts = numberLateCounts;
            textCounts.text = numberCounts.ToString();
            pubNumberCounts = numberCounts;
        }
    }
}
