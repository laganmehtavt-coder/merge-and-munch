using System.Collections;
using UnityEngine;

public class View_Splash
    : MonoBehaviour {
    [Header("Settings")]

    [Tooltip("Time in seconds to wait before closing the screen")]
    public float waitTime = 3f; 

    [Tooltip("The UI screen or GameObject to close")]
    public GameObject screenToClose; 

    void Start() {
        if (screenToClose == null) {
            Debug.LogWarning("Screen to close is not assigned!");
            return;
        }

       
        StartCoroutine(WaitAndClose());
    }

    IEnumerator WaitAndClose() {
       
        yield return new WaitForSeconds(waitTime);

       
        screenToClose.SetActive(false);
    }
}