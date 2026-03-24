using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviorSingleton<UIManager>
{
    Dictionary<string, UIBase> dicUIBase = new Dictionary<string, UIBase>();    // UIBase.uiName, UIBase
    List<UIBase> listOpening = new List<UIBase>();  // list of opened UIBase GameObjects.
    List<UIBase> listDestroy = new List<UIBase>();
    List<UIBase> listOpenedSystemUI = new List<UIBase>(); // list of opened DS UIBase GameObjects.

    public Canvas mainUICanvas;
    public Canvas SystemCanvas;
    [SerializeField] Image Background;

    public UIBase PreviousUI { get; private set; }
    public UIBase CurrentUI { get; private set; }

    public bool IsSystemUIEnabled;
    public UIBase CurrentSystemUI { get; private set; }
    public UIBase PreviusSystemUI { get; private set; }

    #region MonoBehaviour
    private void Start()
    {
        if (Background != null) Background.gameObject.SetActive(false);
    }
    private void LateUpdate()
    {
        if (listDestroy.Count > 0)
        {
            Debug.LogWarning($"[DestroyUI count] {listDestroy.Count}");

            for (int i = 0; i < listDestroy.Count; i++)
            {
                UIBase uiBase = listDestroy[i];
                if (null == uiBase) continue;

                if (dicUIBase.ContainsKey(uiBase.uiName)) dicUIBase.Remove(uiBase.uiName);

                if (listOpening.Contains(uiBase)) listOpening.Remove(uiBase);

                Debug.LogWarning($"[DestroyUI] {uiBase.name}");
                UnityEngine.Object.DestroyImmediate(uiBase.gameObject);
            }

            listDestroy.Clear();
        }
    }

    private void OnEnable()
    {
        Debug.Log("---UIManager OnEnable---");
        Instance.ResetGameObject();
    }

    private void OnDisable()
    {
        Debug.Log("---UIManager OnDisable---");
    }
    #endregion



    #region Show & HIde

    /// <summary>
    /// Convenience method to show the target UI and hide everything else.
    /// </summary>
    /// <param name="uiName">The UIName of an UIBase object.</param>
    public async Task NavigateTo(string uiName)
    {
        Instance.HideAllUI();
        if (string.IsNullOrEmpty(uiName)) return;
        if (mainUICanvas == null)
        {
            Debug.LogError(string.Format("Canvas is null.\n Navigate To {0} failed.", uiName));
        }

        await Instance.ShowUI(uiName);
        if (Background != null && !Background.gameObject.activeSelf) Background.gameObject.SetActive(true);
    }

    private async Task<UIBase> ShowUI(string uiName, params object[] objects)
    {
        Debug.Log($"ShowUI = {uiName}");

        var uiBase = await TryGetUIBaseAsync(uiName);
        if (null == uiBase)
        {
            Debug.LogError($"Fail to load UI : {uiName}");
            return null;
        }
        ToolHelpers.ResetLocalTransform(uiBase.transform);
        if (!listOpening.Contains(uiBase)) listOpening.Add(uiBase);
        uiBase.Show(objects);
        PreviousUI = CurrentUI;
        CurrentUI = uiBase;
        return uiBase;
    }

    public void HideAllUI()
    {
        for (int i = 0; i < listOpening.Count; i++)
            listOpening[i].Hide();
    }

    public async Task OpenSystemUI(string uiName)
    {
        if (string.IsNullOrEmpty(uiName)) return;
        var uiBase = await TryGetSystemUIBaseAsync(uiName);
        if (null == uiBase)
        {
            Debug.LogError($"Fail to load UI : {uiName}");
            return;
        }
        ToolHelpers.ResetLocalTransform(uiBase.transform);
        if (!listOpenedSystemUI.Contains(uiBase)) listOpenedSystemUI.Add(uiBase);
        if (PreviusSystemUI != null) CloseSystemUI(PreviusSystemUI.uiName);
        SystemCanvas.gameObject.SetActive(true);
        Debug.LogFormat("OpenUI: {0}", uiName);
        uiBase.Show(null);

        PreviusSystemUI = CurrentSystemUI;
        CurrentSystemUI = uiBase;

        IsSystemUIEnabled = true;
    }

    public void CloseSystemUI(string uiName)
    {
        if (!dicUIBase.TryGetValue(uiName, out UIBase uiBase)) return;
        if (listOpenedSystemUI.Contains(uiBase)) listOpenedSystemUI.Remove(uiBase);
        uiBase.Hide();
        SystemCanvas.gameObject.SetActive(false);
        IsSystemUIEnabled = false;
    }

    private async Task<UIBase> TryGetUIBaseAsync(string uiName)
    {
        if (dicUIBase.TryGetValue(uiName, out UIBase uiBase)) return uiBase;

        string path = System.IO.Path.Combine(Constants.Key_Pages, uiName).Replace("\\", "/");
        GameObject prefab = await ResourceManager.Instance.LoadResourceAsync<GameObject>(path);

        if (prefab == null) return null;
        UIBase uibase = prefab.GetComponent<UIBase>();
        if (uibase == null) return null;

        uiBase = Instantiate(uibase, mainUICanvas.transform);
        dicUIBase.Add(uiName, uiBase);

        uiBase.uiName = uiName;
        uiBase.gameObject.name = uiName;

        uiBase.Init();
        uiBase.gameObject.SetActive(true);

        return uiBase;
    }

    private async Task<UIBase> TryGetSystemUIBaseAsync(string uiName)
    {
        if (dicUIBase.TryGetValue(uiName, out UIBase uiBase)) return uiBase;

        // Use ResourceManager instead of Resources.Load
        UIBase prefab = await ResourceManager.Instance.LoadResourceAsync<UIBase>(uiName);
        
        if (null == prefab) return null;

        uiBase = Instantiate(prefab, SystemCanvas.transform);
        dicUIBase.Add(uiName, uiBase);

        uiBase.uiName = uiName;
        uiBase.gameObject.name = uiName;

        uiBase.Init();
        uiBase.gameObject.SetActive(true);

        return uiBase;
    }

    #endregion

    #region Unload
    public void UnloadUI(string uiName)
    {
        UIBase uiBase = null;
        if (!dicUIBase.TryGetValue(uiName, out uiBase)) return;

        uiBase.Hide();

        uiBase.gameObject.SetActive(false);
        listDestroy.Add(uiBase);
    }

    public void UnloadAllUI()
    {
        foreach (var ui in dicUIBase)
        {
            ui.Value.Hide();
            listDestroy.Add(ui.Value);
        }
    }
    #endregion

    #region Misc
    public bool IsOpened(string uiName)
    {
        if (!dicUIBase.TryGetValue(uiName, out var uiBase)) return false;
        return uiBase.isOpened;
    }

    public UIBase GetUIBase(string uiName)
    {
        dicUIBase.TryGetValue(uiName, out var uiBase);
        return uiBase;
    }

    public void ResetGameObject()
    {
        var findCanvas = GameObject.Find(Constants.Main_Canvas_Path);
        if (findCanvas != null)
        {
            mainUICanvas = findCanvas.GetComponent<Canvas>();

            var findBackground = findCanvas.transform.Find(Constants.Backbround_Path);
            if (findBackground != null) Background = findBackground.GetComponent<Image>();
        }
    }

    #endregion

    #region Events


    #endregion
}
