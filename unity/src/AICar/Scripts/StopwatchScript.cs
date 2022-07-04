using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StopwatchScript : MonoBehaviour
{
    public UnityEngine.UI.Text TimeText;

    [System.NonSerialized] public float countTime;
    [System.NonSerialized] public int a;

    // Start is called before the first frame update
    void Start()
    {
        countTime = 0;
        a = 2;
    }

    // Update is called once per frame
    void Update()
    {
        // ï\é¶éûä‘ÇçXêVÇ∑ÇÈ
        if (a == 0)
        {
            countTime += Time.deltaTime;
        }
        /*
        if (a == 1)
        {
            // âΩÇ‡ÇµÇ»Ç¢
        }
        */
        if (a == 2)
        {
            countTime = 0;
        }

        TimeText.text = countTime.ToString("F2");

    }

    // Start
    public void OnClick0()
    {
        a = 0;
    }
    // Stop
    public void OnClick1()
    {
        a = 1;
    }
    // Reset
    public void OnClick2()
    {
        a = 2;
    }
}
