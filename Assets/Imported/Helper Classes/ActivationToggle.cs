using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationToggle : MonoBehaviour {

    [SerializeField]
    GameObject[] objs = null;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void ToggleActivation() {
        foreach(var obj in objs)
        {
            obj.SetActive(!obj.activeSelf);
        }
    }
}
