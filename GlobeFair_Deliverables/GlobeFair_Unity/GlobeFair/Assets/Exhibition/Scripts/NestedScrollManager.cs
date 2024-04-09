using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Method to allow nested scrolling on the prefab description field
public class NestedScrollManager : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public ScrollRect ParentScrollRect;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Disable parent scrolling
        ParentScrollRect.OnBeginDrag(eventData);
        ParentScrollRect.enabled = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Enable parent scrolling
        ParentScrollRect.OnEndDrag(eventData);
        ParentScrollRect.enabled = true;
    }
}