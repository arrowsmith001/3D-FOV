using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextChanger : MonoBehaviour
{
    public void OnDiscoveryStatusChanged(bool discovered){
        if(discovered) GetComponent<Text>().text = "Discovered";
        else GetComponent<Text>().text = "Hidden";

    }
}
