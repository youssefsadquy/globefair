using UnityEngine;
using UnityEngine.EventSystems;

public class CursorVisibilityOnPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private void Awake()
    {
        Cursor.visible = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.visible = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.visible = false;
    }
}