using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    public CinemachineCamera vcam;
    public Camera cam;
    public GameObject cameraObject;
    public GameObject targetBox;

    private CinemachinePositionComposer framingTransposer;

    [SerializeField]
    private float scale = 0.1f;

    public float cameraX = 20; public float cameraY = 0;
    public bool yawAdjustable; public bool posAdjustable;
    public Vector2 touchDelta;

    public void Start()
    {
        framingTransposer = cameraObject.GetComponent<CinemachinePositionComposer>();
        framingTransposer.CameraDistance = 20;
        targetBox.transform.position = new Vector3(0, 0, 0);
        yawAdjustable = true; posAdjustable = true;
    }

    public void Update()
    {
        cameraObject.transform.rotation = Quaternion.Euler(new Vector3(cameraX, cameraY, 0));
        targetBox.transform.rotation = Quaternion.Euler(new Vector3(0, cameraY, 0));
        if (Input.mouseScrollDelta != Vector2.zero)
        {
            if (IsPointerOverUI() == false)
            {
                AdjustCameraDistance(Input.mouseScrollDelta.y * scale);
            }
        }
        if (Input.mousePresent == true)
        {
            if (IsPointerOverUI() == false)
            {
                if (Input.GetMouseButton(1))
                {
                    float horizontalSpeed = Input.GetAxis("Mouse X");

                    cameraY += horizontalSpeed;
                }
                if (Input.GetMouseButton(2))
                {
                    if (yawAdjustable == true)
                    {
                        float verticalSpeed = Input.GetAxis("Mouse Y");

                        if (cameraX >= 0 && cameraX <= 90)
                        {
                            cameraX += verticalSpeed;
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
                }
                if (Input.GetMouseButton(0))
                {
                    if (posAdjustable == true)
                    {
                        float horizontalSpeed = Input.GetAxis("Mouse X");
                        float verticalSpeed = Input.GetAxis("Mouse Y");

                        targetBox.transform.Translate(new Vector3(-horizontalSpeed * scale, 0, -verticalSpeed * scale));
                    }
                }
            }
        }
        if (Input.touchCount == 1)
        {
            if (IsPointerOverUI() == false)
            {
                Touch touch = Input.GetTouch(0);
                touchDelta = touch.deltaPosition;

                if (touch.phase == TouchPhase.Moved)
                {
                    if (posAdjustable == true)
                    {
                        targetBox.transform.Translate(new Vector3(-(touch.deltaPosition.x * scale / 25), 0, -(touch.deltaPosition.y * scale / 25)));
                    }
                }
            }
        }
        if (Input.touchCount == 2)
        {
            if (IsPointerOverUI() == false)
            {
                Vector2 primaryTouchPos; Vector2 secondaryTouchPos;
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                primaryTouchPos = touch1.position; secondaryTouchPos = touch2.position;

                if (touch2.phase == TouchPhase.Moved)
                {
                    Vector2 primaryTouchPrevious; Vector2 secondaryTouchPrevious;
                    primaryTouchPrevious = primaryTouchPos - touch1.deltaPosition; secondaryTouchPrevious = secondaryTouchPos - touch2.deltaPosition;
                    float previousTouchDistance = Vector2.Distance(primaryTouchPrevious, secondaryTouchPrevious);
                    float currentTouchDistance = Vector2.Distance(primaryTouchPos, secondaryTouchPos);
                    float previousRotate = Vector3.SignedAngle(primaryTouchPrevious, secondaryTouchPrevious, Vector3.forward);
                    float currentRotate = Vector3.SignedAngle(primaryTouchPos, secondaryTouchPos, Vector3.forward);

                    //pinch to zoom
                    float deltaDistance = currentTouchDistance - previousTouchDistance;
                    AdjustCameraDistance(deltaDistance * scale / 25f);

                    //rotate y using touch
                    cameraY += currentRotate - previousRotate;
                }
            }
        }
        if (Input.touchCount == 3)
        {
            if (IsPointerOverUI() == false)
            {
                Vector2 primaryTouchPos; Vector2 secondaryTouchPos; Vector2 tertiaryTouchPos;
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                Touch touch3 = Input.GetTouch(2);
                primaryTouchPos = touch1.position; secondaryTouchPos = touch2.position; tertiaryTouchPos = touch3.position;

                if (touch2.phase == TouchPhase.Moved)
                {
                    Vector2 primaryTouchPrevious; Vector2 secondaryTouchPrevious;
                    primaryTouchPrevious = primaryTouchPos - touch1.deltaPosition; secondaryTouchPrevious = secondaryTouchPos - touch2.deltaPosition;
                    float previousRotate = Vector3.SignedAngle(primaryTouchPrevious, secondaryTouchPrevious, Vector3.forward);
                    float currentRotate = Vector3.SignedAngle(primaryTouchPos, secondaryTouchPos, Vector3.forward);

                    //rotate y using touch
                    cameraY += currentRotate - previousRotate;
                }
                if (touch3.phase == TouchPhase.Moved)
                {
                    Vector2 primaryTouchPrevious; Vector2 secondaryTouchPrevious;
                    primaryTouchPrevious = primaryTouchPos - touch1.deltaPosition; secondaryTouchPrevious = secondaryTouchPos - touch3.deltaPosition;
                    float previousRotate = Vector3.SignedAngle(primaryTouchPrevious, secondaryTouchPrevious, Vector3.forward);
                    float currentRotate = Vector3.SignedAngle(primaryTouchPos, secondaryTouchPos, Vector3.forward);

                    //rotate y using touch
                    cameraX += currentRotate - previousRotate;
                }
            }
        }
    }

    private void AdjustCameraDistance(float zoomLevel)
    {
        if (framingTransposer.CameraDistance >= 6)
            framingTransposer.CameraDistance += Input.mouseScrollDelta.y * scale;
        else
            framingTransposer.CameraDistance = 6;
    }

    public void TopDownView()
    {
        cameraX = 90; cameraY = 0;
    }

    public void PerspectiveView()
    {
        cameraX = 27.5f; cameraY = 0;
    }

    public void MoveMouseX(int shift = 0)
    {
        float horizontalSpeed = 0;
        if (Input.mousePresent == true)
            horizontalSpeed = (Input.GetAxis("Mouse X")) * 2.5f;
        if (Input.touchCount > 0)
        {
            if ((touchDelta.x * scale) / 2.5f < 5 && (touchDelta.x * scale) / 2.5f > -5)
                horizontalSpeed = (touchDelta.x * scale) / 2.5f;
            else if ((touchDelta.x * scale) / 2.5f > 5)
                horizontalSpeed = 5;
            else
                horizontalSpeed = -5;
        }

        if (shift == -1)
            if (horizontalSpeed < 0)
                targetBox.transform.Translate(new Vector3(horizontalSpeed, 0, 0));
        if (shift == 1)
            if (horizontalSpeed > 0)
                targetBox.transform.Translate(new Vector3(horizontalSpeed, 0, 0));
    }

    public void MoveMouseY(int shift = 0)
    {
        float verticalSpeed = 0;
        if (Input.mousePresent == true)
            verticalSpeed = (Input.GetAxis("Mouse Y")) * 2.5f;
        if (Input.touchCount > 0)
        {
            if ((touchDelta.y * scale) / 2.5f < 5 && (touchDelta.y * scale) / 2.5f > -5)
                verticalSpeed = (touchDelta.y * scale) / 2.5f;
            else if ((touchDelta.x * scale) / 2.5f > 5)
                verticalSpeed = 5;
            else
                verticalSpeed = -5;
        }

        if (shift == -1)
            if (verticalSpeed < 0)
                targetBox.transform.Translate(new Vector3(0, 0, verticalSpeed));
        if (shift == 1)
            if (verticalSpeed > 0)
                targetBox.transform.Translate(new Vector3(0, 0, verticalSpeed));
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
    }

    public bool IsPointerOverUI()
    {
        //check mouse
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        //check touch
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
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
