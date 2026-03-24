using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UIBase : MonoBehaviour
{
    [HideInInspector]
    public string uiName;

    public RectTransform targetRt;
    public bool isOpened { get { return gameObject.activeSelf; } }

    protected virtual void OnShow(params object[] Objects) { }
    protected virtual void OnClose() { }

    protected virtual void OnInit() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }

    //protected virtual void PlayerDataRefreshHandler() { }

    public bool fitScreen;

    protected virtual void Awake()
    {
        if (null == targetRt) targetRt = GetComponent<RectTransform>();
    }
    protected virtual void Start() { OnStart(); }
    protected virtual void Update() { OnUpdate(); }
    public void Init() { OnInit(); }
    public void Show(params object[] Objects)
    {
        if (!isOpened) gameObject.SetActive(true);
        transform.SetAsLastSibling();
        OnShow(Objects);
    }
    public void Hide()
    {
        if (!isOpened) return;
        gameObject.SetActive(false);
        OnClose();
    }
    public virtual void OnKeyDown(KeyCode keyCode)
    {
        Debug.Log(keyCode.ToString() + ", Is Down");
    }


}
