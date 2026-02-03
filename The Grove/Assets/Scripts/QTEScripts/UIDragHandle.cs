using UnityEngine;
using UnityEngine.EventSystems;


//Source: https://www.youtube.com/watch?v=yalbvB84kCg
//BmO Super Easy DRAG and DROP Unity Tutorial

//https://www.youtube.com/watch?v=BGr-7GZJNXg
//Code Monkey Simple Drag Drop

//https://docs.unity3d.com/6000.3/Documentation/ScriptReference/RectTransformUtility.html
//RecTransformUtility

//
//
public class UIDragHandle : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform canvasRect;

    private RectTransform rect;
    private Vector2 offset;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        offset = rect.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        rect.anchoredPosition = localPoint + offset;
    }
}

