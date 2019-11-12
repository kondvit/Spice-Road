using UnityEngine;
using UnityEngine.UI;

public class SimulationController : MonoBehaviour
{
    public Text UIText;

    bool paused = false;
    float lastScale = 1;

    public static bool gameOver = false;


    void Update()
    {
        if (!gameOver)
        {
            SimulationControl();
        }
    }


    private void SimulationControl()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            paused = !paused;
            if (paused)
            {
                lastScale = Time.timeScale;
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = lastScale;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Time.timeScale *= 2;
            lastScale *= 2;
        }
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            Time.timeScale *= 0.5f;
            lastScale *= 0.5f;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        UIText.text = "x" + Time.timeScale.ToString();
    }
}
