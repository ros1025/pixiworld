using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CharSelectorUI : MonoBehaviour
{
    [SerializeField]
    private UIDocument document;
    [SerializeField]
    private CharacterCustomizer customizer;
    private VisualElement root;
    private VisualElement element;
    [SerializeField]
    private int charLimit = 3;

    private void Awake()
    {
        root = document.rootVisualElement;
        StartCoroutine(Initialise());
    }

    public IEnumerator Initialise()
    {
        root = document.rootVisualElement;
        root.style.visibility = Visibility.Visible;

        element = root.Q<VisualElement>(className: "background");
        element.Clear();

        yield return new WaitUntil(() => customizer.GetCurrentCharacter() != null);
        List<Character> characters = customizer.GetCharacters();

        for (int i = 0; i < characters.Count; i++)
        {
            Character character = characters[i]; 

            if (i < charLimit)
            {
                Button button = new();
                button.name = character.name;
                button.text = character.name;
                button.AddToClassList("content-button");
                element.Add(button);

                button.RegisterCallback<ClickEvent>(evt => customizer.ChangeCurrentChar(characters.IndexOf(character)));
            }
        }

        Button addNew = new();
        addNew.name = "Add";
        addNew.text = "Add";
        addNew.AddToClassList("content-button");
        element.Add(addNew);

        addNew.RegisterCallback<ClickEvent>(evt =>
        {
            customizer.AddNewCharacter();
            StartCoroutine(Initialise());
        });
    }
}
