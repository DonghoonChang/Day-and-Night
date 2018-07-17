using UnityEngine;

public class GameTime {

    public static bool isPaused = false;

    public static float deltaTime
    {
        get
        {
            return isPaused ? 0 : Time.deltaTime;
        }
    }

    public static void TogglePause()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1;
        }

        else
        {
            isPaused = true;
            Time.timeScale = 0;
        }
    }
}
