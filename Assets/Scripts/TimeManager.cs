using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TimeManager : MonoBehaviour
{
    [SerializeField] GameObject LightObj;
    [SerializeField] Light Daylight;
    [SerializeField] Gradient ambientColor;
    [SerializeField] Gradient directionalColor;
    [SerializeField] Gradient directionalColorMoon;
    [SerializeField] Gradient fogColor;
    [SerializeField, Range(0, 1440)]private int minutes;
    private int day = 1;
    private float timePassed;
    public Vector2Int time;
    private static float GameTimeScale = 5;
    public float acceleration;
    public bool paused;

    // Called on Startup
    void Start()
    {
        acceleration = 1;
    }
    
    // Update is called once per frame
    void Update()
    {
        Tick();
    }

    internal void Tick()
    {
        if (!paused)
        {
            timePassed += Time.deltaTime;
            time = new Vector2Int((minutes / 60), (minutes % 60));
            if (timePassed >= GameTimeScale / acceleration)
            {
                minutes += 1;
                timePassed = 0;
                ManageLight(minutes / 60f / 24f);
                isDay();
            }
            if (minutes >= 60 * 24) //1440
            {
                minutes = 0;
                day += 1;
            }
        }
    }

    public void StopTime()
    {
        paused = true;
    }

    public void ResumeTime()
    {
        paused = false;
    }

    internal void ManageLight(float TimePercent)
    {
        RenderSettings.ambientLight = ambientColor.Evaluate(TimePercent);
        RenderSettings.fogColor = ambientColor.Evaluate(TimePercent);
        Daylight.color = directionalColor.Evaluate(TimePercent);
        LightObj.transform.rotation = Quaternion.Euler(new Vector3((360 * TimePercent) - 90, 170, 0));
    }

    internal void isDay()
    {
        if (minutes > 360 && minutes < 1080)
        {
            Daylight.intensity = 1f;
            Daylight.shadows = LightShadows.Soft;
        }
        else
        {
            Daylight.intensity = 0.2f;
            Daylight.shadows = LightShadows.None;
        }
    }
}
