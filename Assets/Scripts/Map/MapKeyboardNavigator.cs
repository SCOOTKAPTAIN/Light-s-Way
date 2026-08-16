using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Map;

public class MapKeyboardNavigator : MonoBehaviour
{
    public static MapKeyboardNavigator Instance { get; private set; }
    public const float SelectionScale = 1.2f;

    [SerializeField] private MapView mapView;
    [SerializeField] private MapPlayerTracker mapPlayerTracker;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveSelection;
    [SerializeField] private InputActionReference confirm;
    [SerializeField] private InputActionReference cancel;

    [Header("UI")]
    [SerializeField] private GameObject exitConfirm;

    private List<MapNode> selectableNodes = new List<MapNode>();
    private int currentIndex;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (moveSelection != null)
        {
            moveSelection.action.performed += OnMoveSelection;
            moveSelection.action.Enable();
        }

        if (confirm != null)
        {
            confirm.action.performed += OnConfirm;
            confirm.action.Enable();
        }

        if (cancel != null)
        {
            cancel.action.performed += OnCancel;
            cancel.action.Enable();
        }

        RefreshSelectionList();
        SelectFirstAvailableNode();
    }

    private void OnDisable()
    {
        if (moveSelection != null)
        {
            moveSelection.action.performed -= OnMoveSelection;
            moveSelection.action.Disable();
        }

        if (confirm != null)
        {
            confirm.action.performed -= OnConfirm;
            confirm.action.Disable();
        }

        if (cancel != null)
        {
            cancel.action.performed -= OnCancel;
            cancel.action.Disable();
        }
    }

    private void OnMoveSelection(InputAction.CallbackContext context)
    {
        if (mapView == null || mapPlayerTracker == null)
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
        if (mapView == null || mapPlayerTracker == null)
            return;

        if (mapPlayerTracker.Locked)
            return;

        if (TransitionManager.Instance != null && TransitionManager.Instance.IsBlockingInput)
            return;

        ConfirmSelection();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (exitConfirm == null)
            return;

        exitConfirm.SetActive(!exitConfirm.activeSelf);
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

            if (isSelected && MapView.Instance != null && MapView.Instance.NodeDetails != null)
            {
                node.EncounterDetails();
                MapView.Instance.NodeDetails.SetActive(true);
            }
        }
    }
}