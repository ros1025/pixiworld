using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
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
        Initialise();
    }

    public void Initialise()
    {
        root = document.rootVisualElement;
        root.style.visibility = Visibility.Visible;

        Button addNew = root.Q<VisualElement>("Tools").Q<Button>("New");

        addNew.RegisterCallback<ClickEvent>(AddNewCharacter);

        Button moreChars = root.Q<VisualElement>("Tools").Q<Button>("More");

        StartCoroutine(RefreshCharacters());
        AccountForSafeArea();
    }

    private void AddNewCharacter(ClickEvent evt)
    {
        customizer.AddNewCharacter();
        StartCoroutine(RefreshCharacters());
    }

    public IEnumerator RefreshCharacters()
    {
        element = root.Q<VisualElement>("Characters");
        element.Clear();

        yield return new WaitUntil(() => customizer.GetCurrentCharacter() != null);
        List<Character> characters = customizer.GetCharacters();

        for (int i = 0; i < characters.Count; i++)
        {
            Character character = characters[i]; 

            if (i < charLimit)
            {
                AddCharacterButton(character);
            }
        }
    }

    private async Awaitable AddCharacterButton(Character character)
    {
        Button button = new();
        button.name = character.GetCharacterName();
        button.text = character.GetCharacterName();
        
        button.AddToClassList("content-button");
        element.Add(button);

        button.RegisterCallback<ClickEvent>(evt => customizer.ChangeCurrentChar(customizer.GetCharacters().IndexOf(character)));
        StartCoroutine(UpdateCharacterButton(button, character));
    }

    private IEnumerator UpdateCharacterButton(Button button, Character character)
    {
        while (button.parent == element)
        {
            yield return new WaitUntil(() => character.name != button.name);
            button.name = character.GetCharacterName();
            button.text = character.GetCharacterName();
        }
    }

    public void Hide()
    {
        root.style.visibility = Visibility.Hidden;
    }

    public void Show()
    {
        root.style.visibility = Visibility.Visible;
    }

    private void AccountForSafeArea()
    {
        float hotX()
        {
            if (Screen.safeArea.x == 0) return 0;
            else return (Screen.width - Screen.safeArea.width - Screen.safeArea.x);
        }

        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            root.style.marginLeft = Screen.safeArea.x;
        }
        if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            root.style.marginLeft = hotX();
        }
        root.style.marginBottom = Screen.height - Screen.safeArea.height;
    }
}
