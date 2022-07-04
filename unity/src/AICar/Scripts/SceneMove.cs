using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneMove : MonoBehaviour
{
    [SerializeField] private Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {

        int num = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < num; i++)
        {
            dropdown.options.Add(new Dropdown.OptionData { text = i.ToString() });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Update is called once per frame
    public void OnClick()
    {
        Debug.Log(dropdown.value);
        SceneManager.LoadScene(dropdown.value);
    }
}
