using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AdaptivePerformance;
using UnityEditor.Rendering;
using Unity.Mathematics;

public class ModifySettings : MonoBehaviour
{
    private IAdaptivePerformance ap;
    private IDevicePerformanceControl ctrl;
    [SerializeField]
    public SettingsController controller;

    public List<int> Resolutions; public int currentRes;
    public int displayheight; public int displaywidth;
    public int refreshRate; public int renderingMode;


    void Start()
    {
        ap = Holder.Instance;
        
        ctrl = ap.DevicePerformanceControl;
        ctrl.AutomaticPerformanceControl = true;

        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
    }

    public void SetupResolutions()
    {
        Debug.Log($"{Screen.height} {Screen.height}");
        displayheight = Screen.height; displaywidth = Screen.width;

        if (currentRes < 240)
        {
            currentRes = (int)(Screen.height * ScalableBufferManager.heightScaleFactor);
        }

        int scalerIndex = 1;
        while (Screen.height / scalerIndex > 240)
        {
            Resolutions.Add(Screen.height / scalerIndex);
            scalerIndex++;
        }

        List<int> standardRes = new List<int> { 2160, 1440, 1080, 720, 540, 480, 360, 240 };
        foreach (int res in standardRes)
        {
            if (res < Screen.height && !Resolutions.Contains(res))
            {
                Resolutions.Add(res);
            }
        }

        Resolutions.Sort((a, b) =>
        {
            if (a < b) return -1;
            else return 0;
        });

        if (Resolutions.IndexOf(currentRes) == -1)
            currentRes = DetermineNearestResolution(currentRes, 0, Resolutions.Count - 1);

        ScalableBufferManager.ResizeBuffers((float)currentRes / Screen.height, (float)currentRes / Screen.height);
    }

    public List<(int, string)> SetupFPS()
    {
        List<(int, string)> refreshRates = new();
        int maxRefreshRate = (int)Math.Round(Screen.currentResolution.refreshRateRatio.value);

        List<int> standardFps = new List<int> { 24, 30, 60, 90, 120 };
        foreach (int fps in standardFps)
        {
            if (fps < maxRefreshRate && refreshRates.FindIndex(item => item.Item1 == fps) == -1)
            {
                refreshRates.Add((fps, $"{fps}"));
            }
        }

        refreshRates.Add((maxRefreshRate, $"{maxRefreshRate}"));
        refreshRates.Add((-1, $"Unlimited"));

        refreshRates.Sort((a, b) =>
        {
            if (a.Item1 < b.Item1) return -1;
            else return 0;
        });

        if (refreshRates.FindIndex(item => item.Item1 == refreshRate) == -1)
            refreshRate = DetermineNearestFPS(refreshRate, refreshRates);
        
        return refreshRates;
    }

    public int DetermineNearestResolution(int res, int low, int high)
    {
        if (Resolutions.Contains(res))
        {
            return res;
        }
        else
        {
            if (res < low)
            {
                return low;
            }
            else if (res > high)
            {
                return high;
            }
            else
            {
                for (int i = 1; i < Resolutions.Count; i++)
                {
                    int lowDist = Math.Abs(res - Resolutions[i - 1]);
                    int highDist = Math.Abs(res - Resolutions[i]);

                    if (lowDist < res && highDist > res)
                    {
                        if (lowDist < highDist)
                        {
                            return Resolutions[i - 1];
                        }
                        else
                        {
                            return Resolutions[i];
                        }
                    }
                }

                return high;
            }
        }
    }

    public int DetermineNearestFPS(int fps, List<(int, string)> refreshRates)
    {
        if (refreshRates.FindIndex(item => item.Item1 == fps) != -1)
        {
            return fps;
        }
        else
        {
            if (fps < refreshRates[0].Item1)
            {
                return refreshRates[0].Item1;
            }
            else if (fps > refreshRates[^1].Item1)
            {
                return refreshRates[^1].Item1;
            }
            else
            {
                for (int i = 1; i < refreshRates.Count; i++)
                {
                    int lowDist = Math.Abs(fps - refreshRates[i - 1].Item1);
                    int highDist = Math.Abs(fps - refreshRates[i].Item1);

                    if (lowDist < fps && highDist > fps)
                    {
                        if (lowDist < highDist)
                        {
                            return refreshRates[i - 1].Item1;
                        }
                        else
                        {
                            return refreshRates[i].Item1;
                        }
                    }
                }

                return refreshRates[^1].Item1;
            }
        }
    }

    public void SetQualityLevel(ChangeEvent<string> evt, List<string> options)
    {
        QualitySettings.SetQualityLevel(options.IndexOf(evt.newValue));
    }

    public void SetTargetFPS(ChangeEvent<string> evt, List<(int, string)> options)
    {
        int fps = options.Find(item => item.Item2 == evt.newValue).Item1;
        SetTargetFPS(fps);
    }

    public void SetTargetFPS(int fps)
    {
        refreshRate = fps;
        Application.targetFrameRate = fps;
    }

    public void SetRenderingResolution(ChangeEvent<string> evt, List<int> options)
    {
        List<string> optionsStr = new();
        foreach (int item in Resolutions) optionsStr.Add($"{item}p");
        int index = optionsStr.IndexOf(evt.newValue);
        int resolution = Resolutions[index];
        currentRes = resolution;

        /*
        Display.main.SetRenderingResolution(
            Mathf.RoundToInt(resolution * displaywidth / displayheight),
            resolution
            );
        */
        ScalableBufferManager.ResizeBuffers((float)currentRes / Screen.height, (float)currentRes / Screen.height);
        Debug.Log(ScalableBufferManager.heightScaleFactor);
    }

    public bool DetermineFPSSliderAvailability()
    {
        if (ap != null)
            return true;
        else
            return false;
    }

    public void UpdateResValues()
    {
        //for renderingResolution
        if (displayheight != Screen.height || displaywidth != Screen.width)
        {
            SetupResolutions();
            List<string> optionsStr = new();
            foreach (int item in Resolutions)
                optionsStr.Add($"{item}p");

            if (Resolutions.IndexOf(currentRes) == -1)
                currentRes = Screen.height;

            displayheight = Screen.height; displaywidth = Screen.width;
            controller.UpdateValues("renderres", optionsStr);
            ScalableBufferManager.ResizeBuffers((float)currentRes / Screen.height, (float)currentRes / Screen.height);

        }
    }
}
