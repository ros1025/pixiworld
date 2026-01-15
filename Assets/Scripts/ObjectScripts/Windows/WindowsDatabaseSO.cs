using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WindowsDatabaseSO", menuName = "Scriptable Objects/WindowsDatabaseSO")]
public class WindowsDatabaseSO : ScriptableObject
{
    public List<WindowsData> windowsData;
}
