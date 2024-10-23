using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SceneController: MonoBehaviour
{
    [SerializeField]
    private InputActionReference _togglePlanesAction;
    [SerializeField]
    private InputActionReference _leftActivateAction;
    [SerializeField]
    private InputActionReference _rightActivateAction;

    //[SerializeField]
    //private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor _leftRayInteractor;

    [SerializeField]
    private GameObject _grabbableCube;

    [SerializeField]
    //private GameObject _prefab;

    private ARPlaneManager _planeManager;
    private ARAnchorManager _anchorManager;

    private bool _isVisible = true;
    private int _numPlanesAddedOccurred = 0;

    private List<ARAnchor> _anchors = new();

    private void Start()
    {
        Debug.Log("Checking Planes...");

        _planeManager = GetComponent<ARPlaneManager>();
        if(_planeManager == null)
        {
            Debug.LogWarning("No planes detected");
        }

        _anchorManager = GetComponent<ARAnchorManager>();
        if (_anchorManager == null)
        {
            Debug.LogWarning("Anchors not detected");
        }

        _togglePlanesAction.action.performed += OnTogglePlanesAction;
        _leftActivateAction.action.performed += OnLeftActivateAction;
        _rightActivateAction.action.performed += OnRightActivateAction;

        //This are unity events
        _planeManager.trackablesChanged.AddListener(OnPlanesChanged);
        _anchorManager.trackablesChanged.AddListener(OnAnchorsChanged);

    }

    private void OnAnchorsChanged(ARTrackablesChangedEventArgs<ARAnchor> args)
    {
        foreach(var removedAnchor in args.removed)
        {
            if(removedAnchor.Value != null)
            {
                _anchors.Remove(removedAnchor.Value);
                Destroy(removedAnchor.Value.gameObject);
            }
        }
    }

    private void OnLeftActivateAction(InputAction.CallbackContext obj)
    {
        //CheckIfRayHitsCollider();
    }

    //private void CheckIfRayHitsCollider()
    //{
    //    if (_leftRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
    //    {
    //        Debug.Log("Hit " + hit.transform.name);
    //        Quaternion rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
    //        Pose pose = new Pose(hit.point, rotation);
    //        CreateAnchorAsync(pose);
    //    }
    //    else
    //    {
    //        Debug.LogFormat("No hit detected");
    //    }
    //}

    //private async void CreateAnchorAsync(Pose pose)
    //{
    //    var result = await _anchorManager.TryAddAnchorAsync(pose);

    //    if (result.status.IsSuccess())
    //    {
    //        var anchor = result.value as ARAnchor;
    //        _anchors.Add(anchor);

    //        GameObject instance = Instantiate(_prefab, anchor.pose.position, anchor.pose.rotation);
    //        instance.transform.SetParent(anchor.transform);
    //    }
    //}

    private void OnRightActivateAction(InputAction.CallbackContext obj)
    {
        SpawnGrabbableCube();
    }

    private void SpawnGrabbableCube()
    {
        Debug.Log("Spawned Cube");

        Vector3 spawnPosition;

        foreach(var plane in _planeManager.trackables)
        {
            if (plane.classifications.HasFlag(PlaneClassifications.Table))
            {
                spawnPosition = plane.transform.position;
                spawnPosition.y += 0.3f;
                Instantiate(_grabbableCube, spawnPosition, Quaternion.identity);
            }

        }
    }

    private void OnTogglePlanesAction(InputAction.CallbackContext obj)
    {
        _isVisible = !_isVisible;
        float fillAlpha = _isVisible ? 0.3f : 0.0f;
        float lineAlpha = _isVisible? 1.0f : 0.0f;

        Debug.Log("Tracked " + _planeManager.trackables.count + " planes");
        foreach(var plane in _planeManager.trackables)
        {
            SetPlaneAlpha(plane, fillAlpha, lineAlpha);
        }
    }

    private void SetPlaneAlpha(ARPlane plane, float fillAlpha, float lineAlpha)
    {
        var meshRenderer = plane.GetComponentInChildren<MeshRenderer>();
        var lineRenderer = plane.GetComponentInChildren<LineRenderer>();

        if (meshRenderer != null)
        {
            Color color = meshRenderer.material.color;
            color.a = fillAlpha;
            meshRenderer.material.color = color;
        }
        else
        {
            Debug.LogError("Plane doesn't have mesh");
        }

        if(lineRenderer != null)
        {
            Color startColor = lineRenderer.startColor;
            Color endColor = lineRenderer.endColor;

            startColor.a = lineAlpha;
            endColor.a = lineAlpha;

            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
        }
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        if (args.added.Count > 0)
        {
            _numPlanesAddedOccurred++;

            foreach(var plane in _planeManager.trackables)
            {
                PrintPlaneLabel(plane);
            }

            Debug.Log("Number of planes: " + _planeManager.trackables.count);
            Debug.Log("Number of planes added " + _numPlanesAddedOccurred);
        }
    }

    private void PrintPlaneLabel(ARPlane plane)
    {
        string label = plane.classifications.ToString();
        string log = "Plane ID:" + plane.trackableId + " label " + label;
        Debug.Log(log);
    }

    private void OnDestroy()
    {
        _togglePlanesAction.action.performed -= OnTogglePlanesAction;
        _leftActivateAction.action.performed -= OnLeftActivateAction;
        _rightActivateAction.action.performed -= OnRightActivateAction;

        //This are unity events
        _planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        _anchorManager.trackablesChanged.RemoveListener(OnAnchorsChanged);
    }
}
