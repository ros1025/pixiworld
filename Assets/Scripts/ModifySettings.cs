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

    private void Start()
    {
        ap = Holder.Instance;
        ctrl = ap.DevicePerformanceControl;

        ctrl.AutomaticPerformanceControl = true;
        displayheight = Display.main.renderingHeight; displaywidth = Display.main.renderingWidth;
    }

    public void SetupResolutions()
    {
        Resolutions = new List<int> { 480, 720, 1080, 1440, 2160 };
        if (Resolutions.Contains((Display.main.renderingHeight / 2)) == false)
        {
            for (int i = 0; i < Resolutions.Count; i++)
            {
                if (displayheight / 2 < Resolutions[i])
                {
                    Resolutions.Insert(i, displayheight/2);
                    break;
                }
                if (i == Resolutions.Count - 1)
                {
                    Resolutions.Insert(Resolutions.Count, displayheight/2);
                }
            }
        }
        if (Resolutions.Contains(Display.main.renderingHeight) == false)
        {
            for (int i = 0; i < Resolutions.Count; i++)
            {
                if (displayheight < Resolutions[i])
                {
                    Resolutions.Insert(i, displayheight);
                    break;
                }
                if (i == Resolutions.Count - 1)
                {
                    Resolutions.Insert(Resolutions.Count, displayheight);
                }
            }
        }

        if (Resolutions.IndexOf(currentRes) == -1)
            currentRes = Display.main.renderingHeight;
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
            Application.targetFrameRate = 24;
        }
        else if (i == 1)
        {
            Application.targetFrameRate = 30;
        }
        else if (i == 2)
        {
            Application.targetFrameRate = 60;
        }
        else if (i == 3)
        {
            Application.targetFrameRate = 90;
        }
        else if (i == 4)
        {
            Application.targetFrameRate = 120;
        }
        else
        {
            Application.targetFrameRate = -1;
        }
    }

    public void SetPerformanceMode(ChangeEvent<string> evt, List<string> options)
    {
        int i = options.IndexOf(evt.newValue);
        if (i == 0)
        {
            ctrl.AutomaticPerformanceControl = false;
            Application.targetFrameRate = -1;
            ctrl.CpuLevel = ctrl.MaxCpuPerformanceLevel;
            ctrl.GpuLevel = ctrl.MaxGpuPerformanceLevel;
        }
        else if (i == 1)
        {
            ctrl.AutomaticPerformanceControl = true;
        }
        else if (i == 2)
        {
            ctrl.AutomaticPerformanceControl = false;
            Application.targetFrameRate = -1;
            ctrl.CpuLevel = Mathf.RoundToInt(ctrl.MaxCpuPerformanceLevel / 2);
            ctrl.GpuLevel = Mathf.RoundToInt(ctrl.MaxGpuPerformanceLevel / 2);
        }
        else if (i == 3)
        {
            ctrl.AutomaticPerformanceControl = false;
            Application.targetFrameRate = -1;
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

        Display.main.SetRenderingResolution(
            Mathf.RoundToInt(resolution * displaywidth / displayheight),
            resolution
            );
    }

    public int DetermineCurrentPerformanceMode()
    {
        if (ctrl.CpuLevel == ctrl.MaxCpuPerformanceLevel && ctrl.GpuLevel == ctrl.MaxGpuPerformanceLevel && ctrl.AutomaticPerformanceControl == false)
            return 0;
        if (ctrl.AutomaticPerformanceControl == true)
            return 1;
        if (ctrl.CpuLevel == Mathf.RoundToInt(ctrl.MaxCpuPerformanceLevel / 2) && ctrl.GpuLevel == Mathf.RoundToInt(ctrl.MaxGpuPerformanceLevel / 2) && ctrl.AutomaticPerformanceControl == false)
            return 2;
        if (ctrl.CpuLevel == 0 && ctrl.GpuLevel == 0 && ctrl.AutomaticPerformanceControl == false)
            return 3;
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
            Display.main.SetRenderingResolution(
                Mathf.RoundToInt(currentRes * (Display.main.renderingWidth / Display.main.renderingHeight)),
                currentRes
            );
        }
    }
}
