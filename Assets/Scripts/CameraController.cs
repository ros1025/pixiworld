using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    public CinemachineCamera vcam;
    public Camera cam;
    public GameObject cameraObject;
    public GameObject targetBox;
    private BuilderInputs system;

    private CinemachinePositionComposer framingTransposer;

    [SerializeField]
    private float posScale = 0.1f;
    [SerializeField]
    private float zoomScale = 0.1f;
    [SerializeField]
    private float yawScale = 0.1f;
    [SerializeField]
    private float rotateScale = 0.1f;

    public float cameraX = 20;  public float cameraY = 0;
    public float minDistance = 6;
    public bool yawAdjustable; public bool posAdjustable;
    private bool isPointerOverUI;

    private float previousTouchDistance;
    private float previousRotate;
    private Vector2 previousTouchPosition;

    private bool touch0; private bool touch1;

    public void Start()
    {
        framingTransposer = cameraObject.GetComponent<CinemachinePositionComposer>();
        framingTransposer.CameraDistance = 20;
        targetBox.transform.position = new Vector3(0, 0, 0);
        yawAdjustable = true; posAdjustable = true;
        ResetTouchDefaults();

        system = new BuilderInputs();
        system.camera.Enable();
        system.camera.zoom.performed += _ => OnScroll(system.camera.zoom.ReadValue<Vector2>().y);
        system.camera.move.performed += _ => OnMove(system.camera.move.ReadValue<Vector2>());
        system.camera.rotate.performed += _ => OnRotate(system.camera.rotate.ReadValue<float>());
        system.camera.yaw.performed += _ => OnYaw(system.camera.yaw.ReadValue<float>());

        system.camera.Touch0_Activated.performed += _ => touch0 = true;
        system.camera.Touch0_Activated.canceled += _ => { touch0 = false; ResetTouchDefaults(); };
        system.camera.Touch1_Activated.performed += _ => touch1 = true;
        system.camera.Touch1_Activated.canceled += _ => { touch1 = false; ResetTouchDefaults(); };

        system.camera.Touch0.performed += _ =>
        {
            if (touch0 == true && touch1 == false)
            {
                TouchMove(system.camera.Touch0.ReadValue<Vector2>());
            }
        };

        system.camera.Touch1.performed += _ =>
        {
            if (touch0 == true && touch1 == true)
            {
                TouchRotate(system.camera.Touch0.ReadValue<Vector2>(), system.camera.Touch1.ReadValue<Vector2>());
                TouchZoom((system.camera.Touch0.ReadValue<Vector2>() - system.camera.Touch1.ReadValue<Vector2>()).magnitude);
            }
        };

        AdjustCameraBox();
    }

    public void AdjustCameraBox()
    {
        cameraObject.transform.rotation = Quaternion.Euler(new Vector3(cameraX, cameraY, 0));
        targetBox.transform.rotation = Quaternion.Euler(new Vector3(0, cameraY, 0));
    }

    private void OnScroll(float delta)
    {
        if (IsPointerOverUI() == false)
        {
            AdjustCameraDistance(delta);
            AdjustCameraBox();
        }
    }

    private void TouchZoom(float distance)
    {
        if (IsPointerOverUI() == false)
        {
            if (previousTouchDistance == 0)
            {
                previousTouchDistance = distance;
            }
            float currentTouchDistance = distance;


            //pinch to zoom
            float deltaDistance = currentTouchDistance - previousTouchDistance;
            AdjustCameraDistance(deltaDistance * zoomScale);

            previousTouchDistance = currentTouchDistance;
            AdjustCameraBox();
        }
    }

    private void TouchMove(Vector2 touch)
    {
        if (IsPointerOverUI() == false)
        {
            if (posAdjustable == true)
            {
                if (previousTouchPosition == Vector2.zero)
                {
                    previousTouchPosition = touch;
                }
                Vector2 currentTouchPosition = touch;
                Vector2 touchDelta = currentTouchPosition - previousTouchPosition;
                targetBox.transform.Translate(new Vector3(-touchDelta.x * (Mathf.Pow(posScale, 2)), 0, -touchDelta.y * Mathf.Pow(posScale, 2)));

                previousTouchPosition = currentTouchPosition;
                AdjustCameraBox();
            }
        }
    }

    private void TouchRotate(Vector2 touchA, Vector2 touchB)
    {
        if (IsPointerOverUI() == false)
        {
            float currentRotate = Vector2.SignedAngle(touchA - touchB, Vector2.zero - touchA);
            if (previousRotate == 0)
            {
                previousRotate = currentRotate;
            }

            //rotate y using touch
            cameraY += currentRotate - previousRotate;

            previousRotate = currentRotate;
            AdjustCameraBox();
        }
    }

    private void OnRotate(float axis)
    {
        if (IsPointerOverUI() == false)
        {
            cameraY += axis * rotateScale;
            AdjustCameraBox();
        }
    }

    private void OnMove(Vector2 delta)
    {
        if (IsPointerOverUI() == false)
        {
            if (posAdjustable == true)
            {
                targetBox.transform.Translate(new Vector3(-delta.x * (Mathf.Pow(posScale, 2)), 0, -delta.y * Mathf.Pow(posScale, 2)));
            }
            AdjustCameraBox();
        }
    }

    private void OnYaw(float axis)
    {
        if (IsPointerOverUI() == false)
        {
            if (yawAdjustable == true)
            {
                if (cameraX >= 0 && cameraX <= 90)
                {
                    cameraX += axis * yawScale;
                }
                else if (cameraX < 0)
                {
                    cameraX = 0;
                }
                else if (cameraX > 90)
                {
                    cameraX = 90;
                }
            }
            AdjustCameraBox();
        }
    }

    private void AdjustCameraDistance(float zoomLevel)
    {
        if (framingTransposer.CameraDistance >= minDistance)
            framingTransposer.CameraDistance += zoomLevel * zoomScale;
        else
            framingTransposer.CameraDistance = minDistance;
        AdjustCameraBox();
    }

    public void TopDownView()
    {
        cameraX = 90; cameraY = 0;
        AdjustCameraBox();
    }

    public void PerspectiveView()
    {
        cameraX = 27.5f; cameraY = 0;
        AdjustCameraBox();
    }

    public void MoveMouseX(int shift = 0)
    {
        Vector2 currentTouchPosition = system.camera.Touch0.ReadValue<Vector2>();
        Vector2 touchDelta = currentTouchPosition - previousTouchPosition;
        float delta = touchDelta.x;

        if (delta > 1) delta = 1;
        if (delta < -1) delta = -1;

        if (shift == -1)
            if (delta < 0)
                targetBox.transform.Translate(new Vector3(delta * posScale, 0, 0));
        if (shift == 1)
            if (delta > 0)
                targetBox.transform.Translate(new Vector3(delta * posScale, 0, 0));
        AdjustCameraBox();
    }

    public void MoveMouseY(int shift = 0)
    {
        Vector2 currentTouchPosition = system.camera.Touch0.ReadValue<Vector2>();
        Vector2 touchDelta = currentTouchPosition - previousTouchPosition;
        float delta = touchDelta.y;

        if (delta > 1) delta = 1;
        if (delta < -1) delta = -1;

        if (shift == -1)
            if (delta < 0)
                targetBox.transform.Translate(new Vector3(0, 0, delta * posScale));
        if (shift == 1)
            if (delta > 0)
                targetBox.transform.Translate(new Vector3(0, 0, delta * posScale));
        AdjustCameraBox();
    }

    public void MoveCameraToPos(Vector3 previewPos, Vector2Int previewSize)
    {
        float hitboxX; float hitboxY;
        Vector3 point = cam.WorldToScreenPoint(previewPos);
        Vector3 pointSizeDifferential = cam.WorldToScreenPoint(new Vector3(previewSize.x, 0, previewSize.y));

        if (point.x > Screen.width)
        {
            point -= new Vector3((Screen.width / 2), 0, 0);
            Vector3 translationPoint = cam.ScreenToWorldPoint(point);
            hitboxX = translationPoint.x + previewSize.x;
        }
        else if (point.x < 0)
        {
            point += new Vector3((Screen.width / 2), 0, 0);
            Vector3 translationPoint = cam.ScreenToWorldPoint(point);
            hitboxX = translationPoint.x;
        }
        else
        {
            hitboxX = targetBox.transform.position.x;
        }

        if (point.y > Screen.height)
        {
            point -= new Vector3(0, (Screen.height / 2), 0);
            Vector3 translationPoint = cam.ScreenToWorldPoint(point);
            hitboxY = translationPoint.z + previewSize.y;
        }
        else if (point.y < 0)
        {
            point += new Vector3(0, (Screen.height / 2), 0);
            Vector3 translationPoint = cam.ScreenToWorldPoint(point);
            hitboxY = translationPoint.z;
        }
        else
        {
            hitboxY = targetBox.transform.position.z;
        }

        targetBox.transform.position = (new Vector3(hitboxX, 0, hitboxY));
        AdjustCameraBox();
    }

    private void ResetTouchDefaults()
    {
        previousTouchDistance = 0; previousRotate = 0;
        previousTouchPosition = Vector2.zero;
    }

    public bool IsPointerOverUI()
    {
        //check mouse
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        //check touch
        if (system.camera.Touch0_Activated.inProgress)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                return true;
        }

        return false;
    }
    public Vector3 GetScreenPos(Vector3 previewPos)
    {
        return cam.WorldToScreenPoint(previewPos);
    }
}
