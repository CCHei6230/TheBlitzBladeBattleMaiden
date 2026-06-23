using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class Debug_FPS : MonoBehaviour
{
    public TMP_Text text;
    void Update()
    {
        text.text = "FPS: " + (int) (1.0f/Time.deltaTime);
    }
}
