using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GizmoConfigurator : MonoBehaviour
{
    public UIDocument document;
    private VisualElement root;
    private Label header;
    private Label content;
    private VisualElement buttons;

    private void Awake()
    {
        root = document.rootVisualElement;
        header = root.Q<Label>("Header");
        content = root.Q("Content").Q<Label>();
        buttons = root.Q<VisualElement>("Buttons");
    }

    public void ClearButtons()
    {
        buttons.Clear();
    }

    public void AddButton(string label, Action action)
    {
        Button button = new();
        button.text = label;
        button.clicked += action;
        buttons.Add(button);
    }

    public void AddButton(int index, string label, Action action)
    {
        Button button = new();
        button.text = label;
        button.clicked += action;
        buttons.Insert(index, button);
    }

    public void ChangeHeader(string text)
    {
        header.text = text;
    }

    public void ChangeContent(string text)
    {
        content.text = text;
    }

    public static class EventCallbackWrapper<TEvent>
    {
        public static EventCallback<TEvent, Action<TEvent>> WrappedCallback = (e, a) => a(e);
    }
}
