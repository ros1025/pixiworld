using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;

    private Vector3 lastPosition;

    [SerializeField]
    private LayerMask placementLayermask;
    [SerializeField]
    private LayerMask selectorLayermask;
    [SerializeField]
    private PlayerInput inputs;
    private BuilderInputs builder;

    public event Action OnClicked, OnMoved, OnRelease, OnHold, OnAction, OnExit, OnRightClick;
    private Ray ray; private RaycastHit hit;

    private float timeSinceMoved;

    private void Awake()
    {
        builder = new BuilderInputs();
        builder.Enable();
        builder.builder.OnHold.performed += OnHold_performed;
        builder.builder.OnHold.canceled += OnMouseReleased;
        builder.builder.OnClicked.performed += OnClicked_performed;
        builder.builder.OnClicked.canceled += OnMouseReleased;
        builder.builder.OnExit.performed += OnExit_performed;
        builder.builder.OnAction.performed += OnAction_performed;
        builder.builder.OnMoved.started += OnMoved_performed;
        builder.builder.OnRightClick.performed += OnRightClick_performed;
    }

    private void OnRightClick_performed(InputAction.CallbackContext obj)
    {
        OnRightClick?.Invoke();
    }

    private void OnMouseReleased(InputAction.CallbackContext obj)
    {
        OnRelease?.Invoke();
    }

    private void OnAction_performed(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            OnAction?.Invoke();
        }
    }

    private void OnExit_performed(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            OnExit?.Invoke();
        }
    }

    private void OnClicked_performed(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            OnClicked?.Invoke();
        }
    }

    private void OnHold_performed(InputAction.CallbackContext obj)
    {
        if (obj.performed && Time.time - timeSinceMoved > 0.5f)
        {
            OnHold?.Invoke();
        }
    }

    private void OnMoved_performed(InputAction.CallbackContext obj)
    {
        OnMoved?.Invoke();
        timeSinceMoved = Time.time;
    }

    private void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    OnMoved?.Invoke();
        //}

        /*
        if (Input.touchSupported == true)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == UnityEngine.TouchPhase.Began)            
                OnClicked?.Invoke();

            if (touch.phase == UnityEngine.TouchPhase.Moved)
                OnMoved?.Invoke();

            if (touch.phase == UnityEngine.TouchPhase.Ended)
                OnRelease?.Invoke();
        }
        */
    }

    public void InvokeAction()
    {
        OnAction.Invoke();
    }

    public void InvokeExit()
    {
        OnExit.Invoke();
    }

    public void ClearActions()
    {
        OnHold = null;
        OnClicked = null;
        OnMoved = null;
        OnRelease = null;
        OnAction = null;
        OnExit = null;
        OnRightClick = null;
    }

    public void ClearRightClickAction()
    {
        OnRightClick = null;
    }

    public bool IsPointerOverUI()
    {
        //check mouse
        //if (EventSystem.current.IsPointerOverGameObject())
        //    return true;

        //check touch
        //if (Input.touchCount > 0 && Input.touches[0].phase == UnityEngine.TouchPhase.Began)
        //{
        //    if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
        //        return true;
        //}

        //return false;

        if (!EventSystem.current)
        {
            return false;
        }

        InputSystemUIInputModule s_Module = (InputSystemUIInputModule)EventSystem.current.currentInputModule;

        return s_Module.GetLastRaycastResult(Pointer.current.deviceId).isValid;
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mousePos;
        mousePos = builder.camera.Touch0.ReadValue<Vector2>();
        //Debug.Log(mousePos);
        return mousePos;
    }

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos;
        mousePos = builder.camera.Touch0.ReadValue<Vector2>();
        mousePos.z = sceneCamera.nearClipPlane;
        ray = sceneCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, placementLayermask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }

    public bool RayHitObject(GameObject hitbox)
    {
        List<RaycastHit> hits = new();
        hits.AddRange(RayHitAllObjects());

        if (hits.FindIndex(item => item.collider.gameObject == hitbox) != -1)
        {
            return true;
        }
        else return false;
    }

    public RaycastHit[] RayHitAllObjects()
    {
        return Physics.RaycastAll(ray, float.PositiveInfinity, selectorLayermask);
    }

    public GameObject GetObject(Vector3 pos)
    {
        if (Physics.Raycast(new Vector3(pos.x, pos.y + 10, pos.z), Vector3.down, out hit, float.PositiveInfinity, selectorLayermask))
        {
            return hit.collider.gameObject;
        }
        else return null;
    }
}