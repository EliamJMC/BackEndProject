using UnityEngine;
using UnityEngine.UIElements;

public class ScrollBar : MonoBehaviour
{
    public UIDocument uiDocument;
    private VisualElement content;
    private VisualElement container;

    float scrollPosition = 0f;
    public float scrollSpeed = 500f;
    float contentHeight;
    float containerHeight;
    bool ready = false;

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        container = root.Q<VisualElement>("R_Inv_BackGround");
        content = root.Q<VisualElement>("Content");

        container.RegisterCallback<GeometryChangedEvent>(evt => Setup());
        content.RegisterCallback<GeometryChangedEvent>(evt => Setup());
    }

    void Setup()
    {
        contentHeight = content.layout.height;
        containerHeight = container.layout.height;
        ready = contentHeight > 0 && containerHeight > 0;
        ClampScroll();
        ApplyScroll();
    }

    void Update()
    {
        if (!ready) return;
        if (contentHeight <= containerHeight) return;

        float wheel = Input.mouseScrollDelta.y;
        scrollPosition += -wheel * scrollSpeed * Time.deltaTime;

        ClampScroll();
        ApplyScroll();
    }

    void ClampScroll()
    {
        scrollPosition = Mathf.Max(scrollPosition, 0);
        scrollPosition = Mathf.Min(scrollPosition, contentHeight - containerHeight);
    }

    void ApplyScroll()
    {
        content.transform.position = new Vector3(0, -scrollPosition, 0);
    }
}
