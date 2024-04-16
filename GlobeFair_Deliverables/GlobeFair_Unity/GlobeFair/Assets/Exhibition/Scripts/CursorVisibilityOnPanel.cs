using UnityEngine;
using UnityEngine.EventSystems;

// Script that allows the use of a custom cursor with boundaries on the current panel
public class CursorVisibilityOnPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D mouseIcon;

    private void Awake()
    {
        Cursor.visible = false;
    }

    // Simulating boundaries if the cursor exits the canvas
    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(mouseIcon, new Vector2(16, 16), CursorMode.Auto);
        Cursor.visible = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.visible = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

    }
}