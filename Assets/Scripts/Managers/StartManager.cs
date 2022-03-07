using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManager : MonoBehaviour
{
    [SerializeField]
    private VisManager VisManager;
    private void Awake()
    {
        DataManager.LoadData();
    }

    void Start()
    {
        VisManager.Initialize();
    }

    void Update()
    {
        
    }
}
