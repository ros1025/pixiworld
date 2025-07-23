using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public List<Character> characters;
    [SerializeField]
    private GameObject defaultCharPrefab;

    public void AddNewCharacter()
    {
        GameObject newCharObject = Instantiate(defaultCharPrefab);
        Character newCharacter = newCharObject.AddComponent<Character>();
        characters.Add(newCharacter);
    }

    public Character GetCharacter(int index)
    {
        Debug.Log($"{index}, {characters.Count}");

        if (index >= 0 && index < characters.Count)
        {
            return characters[index];
        }
        else
        {
            Debug.Log("Invalid character ID!");
            return null;
        }
    }

    public Character GetLastCharacter()
    {
        if (characters.Count == 0)
        {
            AddNewCharacter();
        }

        return characters[^1];
    }

    public int GetCharactersCount()
    {
        return characters.Count;
    }
}
