//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.8.2
//     from Assets/Scripts/BuildMode/BuilderInputs.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine;

public partial class @BuilderInputs: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @BuilderInputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""BuilderInputs"",
    ""maps"": [
        {
            ""name"": ""builder"",
            ""id"": ""86a1c836-6b3f-4af6-9f2a-0b530d6eb86e"",
            ""actions"": [
                {
                    ""name"": ""OnClicked"",
                    ""type"": ""Button"",
                    ""id"": ""d60f8f79-0243-4f43-9a79-4cff917a25e3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""OnHold"",
                    ""type"": ""Button"",
                    ""id"": ""21db327e-b2e9-46e1-abfe-02bf940efb96"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""OnAction"",
                    ""type"": ""Button"",
                    ""id"": ""17ab5bb6-0ed9-4b1a-b530-543dd5e81e48"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""OnExit"",
                    ""type"": ""Button"",
                    ""id"": ""5ab808e9-5ca5-4eab-9d3e-b439c0041aab"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""OnMoved"",
                    ""type"": ""Value"",
                    ""id"": ""a7e225c2-94ad-4c8b-be78-490ded8911ff"",
                    ""expectedControlType"": ""Delta"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""OnRightClick"",
                    ""type"": ""Button"",
                    ""id"": ""9e131e5e-1042-49f7-ab44-87915dead8fe"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3ecc492a-3f73-467c-b4fa-849f8c17160d"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Tap"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnClicked"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""132981ea-b3aa-4fba-afec-162a7150467c"",
                    ""path"": ""<Touchscreen>/primaryTouch/tap"",
                    ""interactions"": ""MultiTap"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnClicked"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5ff5dc11-8b24-4dff-80fc-9ffa0977e7e1"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnHold"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""734fcab0-f1fd-4d7c-a5ec-ce1c612cf24b"",
                    ""path"": ""<Touchscreen>/Press"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnHold"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bb4795db-8072-4d21-852b-bba65002a863"",
                    ""path"": ""<Keyboard>/#(P)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnAction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0de65f5a-6c9a-4013-b93b-7c4eaef0ad39"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnAction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1bf33617-ca3c-4494-adab-6c9bc664c78c"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnExit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a6415d5b-ef35-465f-9a8e-fc9c4604cf45"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnExit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e0b81207-f3e5-45f4-92c0-52c8d0fd82b7"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnMoved"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3b49104f-5a8d-4dce-9a07-bf68f1171584"",
                    ""path"": ""<Touchscreen>/primaryTouch/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnMoved"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""42562d38-fa00-4918-87a3-acf8d2a936dc"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": ""Tap"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnRightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c991e19f-0f55-411b-9eea-b4157eb8bfc7"",
                    ""path"": ""<Keyboard>/#(;)"",
                    ""interactions"": ""Tap"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OnRightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""camera"",
            ""id"": ""ac5cb6cd-b88f-4d52-a19b-bcb95a7c1c2d"",
            ""actions"": [
                {
                    ""name"": ""zoom"",
                    ""type"": ""Value"",
                    ""id"": ""d2885748-1320-4889-ad56-0838ccfc6e42"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""move"",
                    ""type"": ""Value"",
                    ""id"": ""d7431411-606b-4eb2-8ea5-937269d05071"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""rotate"",
                    ""type"": ""Value"",
                    ""id"": ""ebc1dcf0-fb82-44f8-9335-ae8d6aa52d6e"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""yaw"",
                    ""type"": ""Value"",
                    ""id"": ""4bb328b1-404b-48df-8df6-9a42ac06c31c"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""b4b46525-fbf1-4cf1-b52d-9b633fa5d755"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""TouchSystem"",
                    ""id"": ""a77b0ab1-7c57-4eed-b0c6-900da986f8d1"",
                    ""path"": ""TwoModifiers"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""zoom"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier1"",
                    ""id"": ""1a46d7d6-1571-4868-8427-c4f866999699"",
                    ""path"": ""<Touchscreen>/touch0/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""modifier2"",
                    ""id"": ""f99269e2-b4e3-4109-8c0d-1e6876dfa411"",
                    ""path"": ""<Touchscreen>/touch1/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""a74e1ca2-6e5f-495e-b7bf-1f40c29b3182"",
                    ""path"": ""<Touchscreen>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""MouseSystem"",
                    ""id"": ""848aad5e-a65b-4bc8-9f98-9d34f38e4856"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""65a78445-ddaf-456d-b78c-88f7593e5fcf"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""460ca82c-557c-4c02-b59c-387a45b55c02"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""TouchSystem"",
                    ""id"": ""d0d9d795-93b5-44ab-a0c4-ea7409bf9983"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""87614bad-29c9-46e5-81ed-b147ff344374"",
                    ""path"": ""<Touchscreen>/Press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""73f8ed9b-ae46-4969-8853-505b8d6d02da"",
                    ""path"": ""<Touchscreen>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""MouseSystem"",
                    ""id"": ""0f2f96c7-ac92-42c7-8984-38ea9d1ea306"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""rotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""1dfa82f1-0848-4270-abeb-089da9136a4e"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""8ddb2929-adb3-4579-abf8-778964d3e467"",
                    ""path"": ""<Mouse>/delta/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""TouchSystem"",
                    ""id"": ""b0e0d79a-eac8-4586-95fe-d1bb4da19790"",
                    ""path"": ""TwoModifiers"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""rotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier1"",
                    ""id"": ""114185a4-fef8-49eb-af73-1ab3e128a859"",
                    ""path"": ""<Touchscreen>/touch0/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""modifier2"",
                    ""id"": ""1d4c84ea-3bff-4dc9-885c-580abbe1236d"",
                    ""path"": ""<Touchscreen>/touch1/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""1c6e60f4-defa-4acb-88ba-7f0b8f0e333e"",
                    ""path"": ""<Touchscreen>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""MouseSystem"",
                    ""id"": ""a20da618-115a-4612-998a-bdbc3ed4d6c1"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""yaw"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""08bdcdf4-e1cf-49a1-bc55-65def5ec5700"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""yaw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""b2abb568-76e9-4194-bb5c-9a8d3350bf45"",
                    ""path"": ""<Mouse>/delta/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""yaw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""TouchSystem"",
                    ""id"": ""2c3bf306-3efe-49e2-9048-19c27656d02f"",
                    ""path"": ""TwoModifiers"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""yaw"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier1"",
                    ""id"": ""9e9f1509-87ae-4ee5-bf9c-f7eea280fea2"",
                    ""path"": ""<Touchscreen>/touch0/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""yaw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""modifier2"",
                    ""id"": ""c65bfed9-85cc-4c55-b66c-57fec2cc2eea"",
                    ""path"": ""<Touchscreen>/touch2/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""yaw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""64bbf92b-0318-41d3-9557-b54eb01586a1"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""yaw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // builder
        m_builder = asset.FindActionMap("builder", throwIfNotFound: true);
        m_builder_OnClicked = m_builder.FindAction("OnClicked", throwIfNotFound: true);
        m_builder_OnHold = m_builder.FindAction("OnHold", throwIfNotFound: true);
        m_builder_OnAction = m_builder.FindAction("OnAction", throwIfNotFound: true);
        m_builder_OnExit = m_builder.FindAction("OnExit", throwIfNotFound: true);
        m_builder_OnMoved = m_builder.FindAction("OnMoved", throwIfNotFound: true);
        m_builder_OnRightClick = m_builder.FindAction("OnRightClick", throwIfNotFound: true);
        // camera
        m_camera = asset.FindActionMap("camera", throwIfNotFound: true);
        m_camera_zoom = m_camera.FindAction("zoom", throwIfNotFound: true);
        m_camera_move = m_camera.FindAction("move", throwIfNotFound: true);
        m_camera_rotate = m_camera.FindAction("rotate", throwIfNotFound: true);
        m_camera_yaw = m_camera.FindAction("yaw", throwIfNotFound: true);
    }

    ~@BuilderInputs()
    {
        Debug.Assert(!m_builder.enabled, "This will cause a leak and performance issues, BuilderInputs.builder.Disable() has not been called.");
        Debug.Assert(!m_camera.enabled, "This will cause a leak and performance issues, BuilderInputs.camera.Disable() has not been called.");
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // builder
    private readonly InputActionMap m_builder;
    private List<IBuilderActions> m_BuilderActionsCallbackInterfaces = new List<IBuilderActions>();
    private readonly InputAction m_builder_OnClicked;
    private readonly InputAction m_builder_OnHold;
    private readonly InputAction m_builder_OnAction;
    private readonly InputAction m_builder_OnExit;
    private readonly InputAction m_builder_OnMoved;
    private readonly InputAction m_builder_OnRightClick;
    public struct BuilderActions
    {
        private @BuilderInputs m_Wrapper;
        public BuilderActions(@BuilderInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @OnClicked => m_Wrapper.m_builder_OnClicked;
        public InputAction @OnHold => m_Wrapper.m_builder_OnHold;
        public InputAction @OnAction => m_Wrapper.m_builder_OnAction;
        public InputAction @OnExit => m_Wrapper.m_builder_OnExit;
        public InputAction @OnMoved => m_Wrapper.m_builder_OnMoved;
        public InputAction @OnRightClick => m_Wrapper.m_builder_OnRightClick;
        public InputActionMap Get() { return m_Wrapper.m_builder; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(BuilderActions set) { return set.Get(); }
        public void AddCallbacks(IBuilderActions instance)
        {
            if (instance == null || m_Wrapper.m_BuilderActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_BuilderActionsCallbackInterfaces.Add(instance);
            @OnClicked.started += instance.OnOnClicked;
            @OnClicked.performed += instance.OnOnClicked;
            @OnClicked.canceled += instance.OnOnClicked;
            @OnHold.started += instance.OnOnHold;
            @OnHold.performed += instance.OnOnHold;
            @OnHold.canceled += instance.OnOnHold;
            @OnAction.started += instance.OnOnAction;
            @OnAction.performed += instance.OnOnAction;
            @OnAction.canceled += instance.OnOnAction;
            @OnExit.started += instance.OnOnExit;
            @OnExit.performed += instance.OnOnExit;
            @OnExit.canceled += instance.OnOnExit;
            @OnMoved.started += instance.OnOnMoved;
            @OnMoved.performed += instance.OnOnMoved;
            @OnMoved.canceled += instance.OnOnMoved;
            @OnRightClick.started += instance.OnOnRightClick;
            @OnRightClick.performed += instance.OnOnRightClick;
            @OnRightClick.canceled += instance.OnOnRightClick;
        }

        private void UnregisterCallbacks(IBuilderActions instance)
        {
            @OnClicked.started -= instance.OnOnClicked;
            @OnClicked.performed -= instance.OnOnClicked;
            @OnClicked.canceled -= instance.OnOnClicked;
            @OnHold.started -= instance.OnOnHold;
            @OnHold.performed -= instance.OnOnHold;
            @OnHold.canceled -= instance.OnOnHold;
            @OnAction.started -= instance.OnOnAction;
            @OnAction.performed -= instance.OnOnAction;
            @OnAction.canceled -= instance.OnOnAction;
            @OnExit.started -= instance.OnOnExit;
            @OnExit.performed -= instance.OnOnExit;
            @OnExit.canceled -= instance.OnOnExit;
            @OnMoved.started -= instance.OnOnMoved;
            @OnMoved.performed -= instance.OnOnMoved;
            @OnMoved.canceled -= instance.OnOnMoved;
            @OnRightClick.started -= instance.OnOnRightClick;
            @OnRightClick.performed -= instance.OnOnRightClick;
            @OnRightClick.canceled -= instance.OnOnRightClick;
        }

        public void RemoveCallbacks(IBuilderActions instance)
        {
            if (m_Wrapper.m_BuilderActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IBuilderActions instance)
        {
            foreach (var item in m_Wrapper.m_BuilderActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_BuilderActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public BuilderActions @builder => new BuilderActions(this);

    // camera
    private readonly InputActionMap m_camera;
    private List<ICameraActions> m_CameraActionsCallbackInterfaces = new List<ICameraActions>();
    private readonly InputAction m_camera_zoom;
    private readonly InputAction m_camera_move;
    private readonly InputAction m_camera_rotate;
    private readonly InputAction m_camera_yaw;
    public struct CameraActions
    {
        private @BuilderInputs m_Wrapper;
        public CameraActions(@BuilderInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @zoom => m_Wrapper.m_camera_zoom;
        public InputAction @move => m_Wrapper.m_camera_move;
        public InputAction @rotate => m_Wrapper.m_camera_rotate;
        public InputAction @yaw => m_Wrapper.m_camera_yaw;
        public InputActionMap Get() { return m_Wrapper.m_camera; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CameraActions set) { return set.Get(); }
        public void AddCallbacks(ICameraActions instance)
        {
            if (instance == null || m_Wrapper.m_CameraActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_CameraActionsCallbackInterfaces.Add(instance);
            @zoom.started += instance.OnZoom;
            @zoom.performed += instance.OnZoom;
            @zoom.canceled += instance.OnZoom;
            @move.started += instance.OnMove;
            @move.performed += instance.OnMove;
            @move.canceled += instance.OnMove;
            @rotate.started += instance.OnRotate;
            @rotate.performed += instance.OnRotate;
            @rotate.canceled += instance.OnRotate;
            @yaw.started += instance.OnYaw;
            @yaw.performed += instance.OnYaw;
            @yaw.canceled += instance.OnYaw;
        }

        private void UnregisterCallbacks(ICameraActions instance)
        {
            @zoom.started -= instance.OnZoom;
            @zoom.performed -= instance.OnZoom;
            @zoom.canceled -= instance.OnZoom;
            @move.started -= instance.OnMove;
            @move.performed -= instance.OnMove;
            @move.canceled -= instance.OnMove;
            @rotate.started -= instance.OnRotate;
            @rotate.performed -= instance.OnRotate;
            @rotate.canceled -= instance.OnRotate;
            @yaw.started -= instance.OnYaw;
            @yaw.performed -= instance.OnYaw;
            @yaw.canceled -= instance.OnYaw;
        }

        public void RemoveCallbacks(ICameraActions instance)
        {
            if (m_Wrapper.m_CameraActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ICameraActions instance)
        {
            foreach (var item in m_Wrapper.m_CameraActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_CameraActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public CameraActions @camera => new CameraActions(this);
    public interface IBuilderActions
    {
        void OnOnClicked(InputAction.CallbackContext context);
        void OnOnHold(InputAction.CallbackContext context);
        void OnOnAction(InputAction.CallbackContext context);
        void OnOnExit(InputAction.CallbackContext context);
        void OnOnMoved(InputAction.CallbackContext context);
        void OnOnRightClick(InputAction.CallbackContext context);
    }
    public interface ICameraActions
    {
        void OnZoom(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnRotate(InputAction.CallbackContext context);
        void OnYaw(InputAction.CallbackContext context);
    }
}
