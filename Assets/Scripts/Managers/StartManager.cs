using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManager : MonoBehaviour
{
    [SerializeField]
    private VisManager VisManager;
    private void Awake()
    {
    }

    void Start()
    {
        DataManager.LoadData();
        VisManager.Initialize();
    }

    void Update()
    {
        
    }
}
