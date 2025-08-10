using UnityEngine;
using UnityEngine.UI;

public class CardGridAutoScaler : MonoBehaviour
{
    [Header("References")]
    public GridLayoutGroup gridLayout;
    public RectTransform cardHolder;      // The panel holding the grid
    public Image prefabCardImage;         // The Image component of the card prefab

    [Header("Settings")]
    public Vector2 spacing = new Vector2(5f, 5f); // Space between cards

    private Vector2 lastScreenSize;

    void Start()
    {
        AdjustGrid();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    void Update()
    {
        // Detect screen size change
        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            AdjustGrid();
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }
    }

    public void AdjustGrid()
    {
        if (gridLayout == null || cardHolder == null || prefabCardImage == null)
        {
            Debug.LogError("CardGridAutoScaler: Missing references.");
            return;
        }

        // 1️⃣ Detect number of cards
        int totalCards = cardHolder.childCount;
        if (totalCards == 0) return;

        // 2️⃣ Calculate columns & rows (try to make it near square)
        int cols = Mathf.CeilToInt(Mathf.Sqrt(totalCards));
        int rows = Mathf.CeilToInt((float)totalCards / cols);

        // 3️⃣ Get available space
        RectTransform holderRect = cardHolder.GetComponent<RectTransform>();

        float totalSpacingX = spacing.x * (cols - 1);
        float totalSpacingY = spacing.y * (rows - 1);

        float availableWidth = holderRect.rect.width - totalSpacingX - gridLayout.padding.left - gridLayout.padding.right;
        float availableHeight = holderRect.rect.height - totalSpacingY - gridLayout.padding.top - gridLayout.padding.bottom;

        float maxCellWidth = availableWidth / cols;
        float maxCellHeight = availableHeight / rows;

        // 4️⃣ Maintain card aspect ratio
        float aspect = (float)prefabCardImage.sprite.rect.width / prefabCardImage.sprite.rect.height;

        float finalCellWidth = maxCellWidth;
        float finalCellHeight = finalCellWidth / aspect;

        if (finalCellHeight > maxCellHeight)
        {
            finalCellHeight = maxCellHeight;
            finalCellWidth = finalCellHeight * aspect;
        }

        // 5️⃣ Apply to grid
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;
        gridLayout.spacing = spacing;
        gridLayout.cellSize = new Vector2(finalCellWidth, finalCellHeight);
    }
}
