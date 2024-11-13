using UnityEngine;
using UnityEngine.UI;

public class NodeGrid : MonoBehaviour
{
    public GameObject nodePrefab; // Assign the NodePrefab here
    public int rows = 4; // Number of rows
    public int columns = 5; // Number of columns
    public float spacing = 100f; // Spacing between nodes

    private GameObject[,] nodeGrid; // Store nodes in a 2D array for easy access

    void Start()
    {
        nodeGrid = new GameObject[rows, columns];
        CreateGrid();
    }

    void CreateGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 position = new Vector2(col * spacing, -row * spacing); // Set position with spacing
                GameObject node = Instantiate(nodePrefab, transform);
                node.GetComponent<RectTransform>().anchoredPosition = position;
                nodeGrid[row, col] = node; // Store reference to the node
            }
        }
    }
}
