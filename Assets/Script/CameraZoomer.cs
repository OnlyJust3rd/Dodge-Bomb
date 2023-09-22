using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomer : MonoBehaviour
{
    public static CameraZoomer instance;

    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float smoothTime;
    [SerializeField] private float zoomFactor = 1.5f;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        startOrthographicSize = GetComponent<Camera>().orthographicSize;
    }

    private void Update()
    {
        if (GameManager.instance.gameTime > 0)
        {
            FixedCameraFollowSmooth(
                GetComponent<Camera>(),
                GameManager.instance.players[0].transform,
                GameManager.instance.players[1].transform
                );
        }
        else
        {
            OneTimeZoom();
            if(focusTarget) CameraFollow();
        }
    }

    // Follow Two Transforms with a Fixed-Orientation Camera
    public void FixedCameraFollowSmooth(Camera cam, Transform t1, Transform t2)
    {
        // How many units should we keep from the players

        // Midpoint we're after
        Vector3 midpoint = (t1.position + t2.position) / 2f;

        // Distance between objects
        float distance = (t1.position - t2.position).magnitude;

        if (distance < minDistance) distance = minDistance;
        if (distance > maxDistance) distance = maxDistance;

        // Move camera a certain distance
        Vector3 cameraDestination = midpoint - cam.transform.forward * distance * zoomFactor;

        // Adjust ortho size if we're using one of those
        if (cam.orthographic)
        {
            // The camera's forward vector is irrelevant, only this size will matter
            cam.orthographicSize = distance;
        }
        // You specified to use MoveTowards instead of Slerp
        cam.transform.position = Vector3.Slerp(cam.transform.position, cameraDestination, smoothTime);

        // Snap when close enough to prevent annoying slerp behavior
        if ((cameraDestination - cam.transform.position).magnitude <= 0.001f)
            cam.transform.position = cameraDestination;
    }

    public Transform focusTarget;
    public float focusSmoothTime = 1f;
    private Vector3 velocity = Vector3.zero;

    private void CameraFollow()
    {
        // Define a target position above and behind the target transform
        Vector3 targetPosition = focusTarget.TransformPoint(new Vector3(0, 5, -10));

        // Smoothly move the camera towards that target position
        Vector3 offset = new Vector3(0, -6, 0);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition + offset, ref velocity, smoothTime);
    }
    private float startOrthographicSize, zoomTimer = 0;
    private void OneTimeZoom()
    {
        if (zoomTimer < 1)
        {
            GetComponent<Camera>().orthographicSize = Mathf.Lerp(startOrthographicSize, 7.5f, zoomTimer);
            zoomTimer += Time.deltaTime;
        }
        //else zoomTimer = 0;
    }
}
