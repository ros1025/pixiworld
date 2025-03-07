using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AdaptivePerformance;

public class ModifySettings : MonoBehaviour
{
    public IAdaptivePerformance ap;
    public IDevicePerformanceControl ctrl;
    [SerializeField]
    public SettingsController controller;

    public List<int> Resolutions; public int currentRes;
    public int displayheight; public int displaywidth;
    public int refreshRate; public int renderingMode;

    public enum PerformanceLevels
    {
        Performance,
        Optimised,
        Standard,
        Battery
    }

    public enum FPSLevels
    {
        Slowest = 24,
        Slow = 30,
        Medium = 60,
        High = 90,
        VeryHigh = 120,
        Unlimited = -1
    }


    private void Start()
    {
        ap = Holder.Instance;
        ctrl = ap.DevicePerformanceControl;

        ctrl.AutomaticPerformanceControl = true;
    }

    public void SetupResolutions()
    {
        displayheight = Display.main.renderingHeight; displaywidth = Display.main.renderingWidth;

        if (currentRes < 240)
        {
            currentRes = (int)(Display.main.renderingHeight * ScalableBufferManager.heightScaleFactor);
        }

        int scalerIndex = 1;
        while (Display.main.renderingHeight / scalerIndex > 240)
        {
            Resolutions.Add(Display.main.renderingHeight / scalerIndex);
            scalerIndex++;
        }

        List<int> standardRes = new List<int> { 2160, 1440, 1080, 720, 540, 480, 360, 240 };
        foreach (int res in standardRes)
        {
            if (res < Display.main.renderingHeight && !Resolutions.Contains(res))
            {
                for (int i = 0; i < Resolutions.Count; i++)
                {
                    if (res > Resolutions[i])
                    {
                        Resolutions.Insert(i, res);
                        break;
                    }
                    if (i == Resolutions.Count - 1)
                    {
                        Resolutions.Add(res);
                        break;
                    }
                }
            }
        }

        if (Resolutions.IndexOf(currentRes) == -1)
            currentRes = DetermineNearestResolution(currentRes, 0, Resolutions.Count - 1);

        Debug.Log(currentRes);
        ScalableBufferManager.ResizeBuffers((float)currentRes / Display.main.renderingHeight, (float)currentRes / Display.main.renderingHeight);
    }

    public int DetermineNearestResolution(int res, int low, int high)
    {
        if (low < high)
        {
            int mid = (low + high) / 2;
            if (res < Resolutions[mid]) // if lower, make scope lower
            {
                return DetermineNearestResolution(res, low, mid - 1);
            }
            else if (res > Resolutions[mid]) // if higher, make scope higher
            {
                return DetermineNearestResolution(res, mid + 1, high);
            }
            else // if equal, return the same value passed
            {
                return res;
            }
        }
        else
        {
            Debug.Log(low);
            if (low == high)
            {
                if (low - 1 >= 0 && high + 1 <= Resolutions.Count - 1)
                {
                    if (Mathf.Abs(res - Resolutions[low - 1]) < Mathf.Abs(res - Resolutions[low]))
                    {
                        return Resolutions[low - 1];
                    }
                    else if (Mathf.Abs(res - Resolutions[high + 1]) < Mathf.Abs(res - Resolutions[high]))
                    {
                        return Resolutions[high + 1];
                    }
                    else return Resolutions[high];
                }
                else return Resolutions[low];
            }
            else
            {
                if (Mathf.Abs(res - Resolutions[low]) < Mathf.Abs(res - Resolutions[high]))
                {
                    return Resolutions[low];
                }
                else return Resolutions[high];
            }
        }
    }

    public void SetQualityLevel(ChangeEvent<string> evt, List<string> options)
    {
        QualitySettings.SetQualityLevel(options.IndexOf(evt.newValue));
    }

    public void SetTargetFPS(ChangeEvent<string> evt, List<string> options)
    {
        int i = options.IndexOf(evt.newValue);
        if (i == 0)
        {
            refreshRate = 24;
            Application.targetFrameRate = 24;
        }
        else if (i == 1)
        {
            refreshRate = 30;
            Application.targetFrameRate = 30;
        }
        else if (i == 2)
        {
            refreshRate = 60;
            Application.targetFrameRate = 60;
        }
        else if (i == 3)
        {
            refreshRate = 90;
            Application.targetFrameRate = 90;
        }
        else if (i == 4)
        {
            refreshRate = 120;
            Application.targetFrameRate = 120;
        }
        else
        {
            refreshRate = -1;
            Application.targetFrameRate = -1;
        }
    }

    public void SetPerformanceMode(ChangeEvent<string> evt, List<string> options)
    {
        int i = options.IndexOf(evt.newValue);
        if (i == (int)PerformanceLevels.Performance)
        {
            renderingMode = 0;
            refreshRate = -1;
            ctrl.AutomaticPerformanceControl = false;
            Application.targetFrameRate = -1;
            ctrl.CpuLevel = ctrl.MaxCpuPerformanceLevel;
            ctrl.GpuLevel = ctrl.MaxGpuPerformanceLevel;
        }
        else if (i == (int)PerformanceLevels.Optimised)
        {
            renderingMode = 1;
            refreshRate = Application.targetFrameRate;
            ctrl.AutomaticPerformanceControl = true;
        }
        else if (i == (int)PerformanceLevels.Standard)
        {
            renderingMode = 2;
            refreshRate = -1;
            ctrl.AutomaticPerformanceControl = false;
            Application.targetFrameRate = -1;
            ctrl.CpuLevel = Mathf.RoundToInt(ctrl.MaxCpuPerformanceLevel / 2);
            ctrl.GpuLevel = Mathf.RoundToInt(ctrl.MaxGpuPerformanceLevel / 2);
        }
        else if (i == (int)PerformanceLevels.Battery)
        {
            renderingMode = 3;
            refreshRate = -1;
            ctrl.AutomaticPerformanceControl = false;
            Application.targetFrameRate = 30;
            ctrl.CpuLevel = 0;
            ctrl.GpuLevel = 0;
        }
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
        ScalableBufferManager.ResizeBuffers((float)currentRes / Display.main.renderingHeight, (float)currentRes / Display.main.renderingHeight);
        Debug.Log(ScalableBufferManager.heightScaleFactor);
    }

    public int DetermineCurrentPerformanceMode()
    {
        if (ctrl.CpuLevel == ctrl.MaxCpuPerformanceLevel && ctrl.GpuLevel == ctrl.MaxGpuPerformanceLevel && ctrl.AutomaticPerformanceControl == false)
            return (int)PerformanceLevels.Performance;
        if (ctrl.AutomaticPerformanceControl == true)
            return (int)PerformanceLevels.Optimised;
        if (ctrl.CpuLevel == Mathf.RoundToInt(ctrl.MaxCpuPerformanceLevel / 2) && ctrl.GpuLevel == Mathf.RoundToInt(ctrl.MaxGpuPerformanceLevel / 2) && ctrl.AutomaticPerformanceControl == false)
            return (int)PerformanceLevels.Standard;
        if (ctrl.CpuLevel == 0 && ctrl.GpuLevel == 0 && ctrl.AutomaticPerformanceControl == false)
            return (int)PerformanceLevels.Battery;
        else
            return -1;
    }

    public int DetermineTargetFPS()
    {
        if (Application.targetFrameRate > 0 && Application.targetFrameRate <= 24)
            return 0;
        else if (Application.targetFrameRate > 24 && Application.targetFrameRate <= 30)
            return 1;
        else if (Application.targetFrameRate > 30 && Application.targetFrameRate <= 60)
            return 2;
        else if (Application.targetFrameRate > 60 && Application.targetFrameRate <= 90)
            return 3;
        else if (Application.targetFrameRate > 90 && Application.targetFrameRate <= 120)
            return 4;
        else
            return 5;
    }

    public bool DetermineFPSSliderAvailability()
    {
        if (ctrl.AutomaticPerformanceControl == true)
            return true;
        else
            return false;
    }

    public void UpdateResValues()
    {
        //for renderingResolution
        if (displayheight != Display.main.renderingHeight || displaywidth != Display.main.renderingWidth)
        {
            SetupResolutions();
            List<string> optionsStr = new();
            foreach (int item in Resolutions)
                optionsStr.Add($"{item}p");

            if (Resolutions.IndexOf(currentRes) == -1)
                currentRes = Display.main.renderingHeight;

            displayheight = Display.main.renderingHeight; displaywidth = Display.main.renderingWidth;
            controller.UpdateValues("renderres", optionsStr);
            ScalableBufferManager.ResizeBuffers((float)currentRes / Display.main.renderingHeight, (float)currentRes / Display.main.renderingHeight);

        }
    }
}
