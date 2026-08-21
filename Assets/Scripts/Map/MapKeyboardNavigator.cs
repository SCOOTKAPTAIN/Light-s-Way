using System.Collections.Generic;
using System.Linq;
using Lightsway.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Map;

public class MapKeyboardNavigator : MonoBehaviour
{
    public static MapKeyboardNavigator Instance { get; private set; }
    public const float SelectionScale = 1.2f;

    [SerializeField] private MapView mapView;
    [SerializeField] private MapPlayerTracker mapPlayerTracker;

    [Header("UI")]
    [SerializeField] private GameObject exitConfirm;

    private List<MapNode> selectableNodes = new List<MapNode>();
    private int currentIndex;
    private GameControls controls;

    private void Awake()
    {
        Instance = this;
        controls = new GameControls();
    }

    private void OnEnable()
    {
        controls.Map.MoveSelection.performed += OnMoveSelection;
        controls.Map.Confirm.performed += OnConfirm;
        controls.Map.Cancel.performed += OnCancel;
        controls.Map.Enable();

        RefreshSelectionList();
        SelectFirstAvailableNode();
    }

    private void OnDisable()
    {
        if (controls == null)
            return;

        controls.Map.MoveSelection.performed -= OnMoveSelection;
        controls.Map.Confirm.performed -= OnConfirm;
        controls.Map.Cancel.performed -= OnCancel;
        controls.Map.Disable();
    }

    private void OnDestroy()
    {
        if (controls == null)
            return;

        controls.Disable();
        controls.Dispose();
        controls = null;
    }

    private void OnMoveSelection(InputAction.CallbackContext context)
    {
        if (!IsMapSceneActive() || mapView == null || mapPlayerTracker == null)
            return;

        if (mapPlayerTracker.Locked)
            return;

        Vector2 value = context.ReadValue<Vector2>();

        if (value.x > 0.5f)
            MoveSelection(1);
        else if (value.x < -0.5f)
            MoveSelection(-1);
        else if (value.y > 0.5f)
            MoveSelection(-1);
        else if (value.y < -0.5f)
            MoveSelection(1);
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        if (!IsMapSceneActive() || mapView == null || mapPlayerTracker == null)
            return;

        if (mapPlayerTracker.Locked)
            return;

        if (TransitionManager.Instance != null && TransitionManager.Instance.IsBlockingInput)
            return;

        ConfirmSelection();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (!IsMapSceneActive() || exitConfirm == null)
            return;

        exitConfirm.SetActive(!exitConfirm.activeSelf);
    }

    private bool IsMapSceneActive()
    {
        return isActiveAndEnabled && gameObject.scene == SceneManager.GetActiveScene();
    }

    private void RefreshSelectionList()
    {
        if (mapView == null || mapView.mapManager == null || mapView.mapManager.CurrentMap == null)
        {
            selectableNodes.Clear();
            currentIndex = 0;
            return;
        }

        selectableNodes = GetCurrentAvailableNodes();

        if (selectableNodes.Count == 0)
        {
            currentIndex = 0;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, selectableNodes.Count - 1);
    }

    private List<MapNode> GetCurrentAvailableNodes()
    {
        var result = new List<MapNode>();

        if (mapView.mapManager.CurrentMap.path.Count == 0)
        {
            result = mapView.MapNodes
                .Where(n => n != null && n.Node != null && n.Node.point.y == 0)
                .ToList();

            return result;
        }

        Vector2Int currentPoint = mapView.mapManager.CurrentMap.path[^1];
        Node currentNode = mapView.mapManager.CurrentMap.GetNode(currentPoint);

        if (currentNode == null)
            return result;

        var layerNodes = mapView.MapNodes
            .Where(n => n != null && n.Node != null &&
                       n.Node != currentNode &&
                       n.Node.point.y == currentNode.point.y)
            .ToList();

        foreach (var outgoing in currentNode.outgoing)
        {
            var nextNode = mapView.GetNode(outgoing);
            if (nextNode != null)
                layerNodes.Add(nextNode);
        }

        return layerNodes.Distinct().ToList();
    }

    private void MoveSelection(int direction)
    {
        RefreshSelectionList();

        if (selectableNodes.Count == 0)
            return;

        currentIndex = (currentIndex + direction + selectableNodes.Count) % selectableNodes.Count;
        HighlightCurrentNode();
    }

    private void ConfirmSelection()
    {
        RefreshSelectionList();

        if (selectableNodes.Count == 0)
            return;

        var selectedNode = selectableNodes[currentIndex];
        mapPlayerTracker.SelectNode(selectedNode);
    }

    private void SelectFirstAvailableNode()
    {
        RefreshSelectionList();

        if (selectableNodes.Count == 0)
            return;

        currentIndex = 0;
        HighlightCurrentNode();
    }

    public void SetSelectionFromNode(MapNode targetNode)
    {
        RefreshSelectionList();
        if (targetNode == null || selectableNodes.Count == 0)
            return;

        int index = selectableNodes.IndexOf(targetNode);
        if (index < 0)
            return;

        currentIndex = index;
        HighlightCurrentNode();
    }

    public bool IsCurrentSelection(MapNode targetNode)
    {
        if (targetNode == null || selectableNodes.Count == 0)
            return false;

        return selectableNodes.Count > currentIndex && selectableNodes[currentIndex] == targetNode;
    }

    private void HighlightCurrentNode()
    {
        if (selectableNodes.Count == 0)
            return;

        for (int i = 0; i < selectableNodes.Count; i++)
        {
            var node = selectableNodes[i];
            if (node == null)
                continue;

            bool isSelected = i == currentIndex;
            node.SetSelectionVisual(isSelected);
            node.SetHoverPulse(isSelected);

            if (isSelected && MapView.Instance != null && MapView.Instance.NodeDetails != null)
            {
                node.EncounterDetails();
                MapView.Instance.NodeDetails.SetActive(true);
            }
        }
    }
}