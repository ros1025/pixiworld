using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GenderSettingSO", menuName = "Scriptable Objects/GenderSettingSO")]
public class GenderSettingSO : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public bool CanBePregnant { get; private set; }
    [field: SerializeField]
    public bool CanCausePregnancy { get; private set; }
    [field: SerializeField]
    public bool CanLactate { get; private set; }
    [field: SerializeField]
    public bool UseToiletStanding { get; private set; }
    [field: SerializeField]
    public ClothingPreference ClothingPreference { get; private set; }

    public GenderSetting ConvertToGenderSettingObject()
    {
        return new GenderSetting(this);
    }
}

public enum ClothingPreference { Masculine, Feminine, Neutral }

[Serializable]
public class GenderSetting
{
    public string Name;
    public bool CanBePregnant;
    public bool CanCausePregnancy;
    public bool CanLactate;
    public bool UseToiletStanding;
    public ClothingPreference ClothingPreference;

    public GenderSetting(GenderSettingSO genderSettingSO)
    {
        this.Name = genderSettingSO.Name;
        this.CanBePregnant = genderSettingSO.CanBePregnant;
        this.CanCausePregnancy = genderSettingSO.CanCausePregnancy;
        this.CanLactate = genderSettingSO.CanLactate;
        this.UseToiletStanding = genderSettingSO.UseToiletStanding;
        this.ClothingPreference = genderSettingSO.ClothingPreference;
    }

    public bool CompatibleClothingPreference(ClothingPreference clothingPreference)
    {
        if (this.ClothingPreference == ClothingPreference.Neutral)
        {
            return true;
        }
        else return clothingPreference == this.ClothingPreference;
    }
}
