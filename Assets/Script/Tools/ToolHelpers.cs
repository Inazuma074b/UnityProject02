using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Google.Protobuf;
using System.IO;
//using MemoryStream = Google.Protobuf.CodedOutputStream;

public static class ToolHelpers
{
    /// <summary>
    /// 遞迴 Destroy 所有子節點
    /// </summary>
    /// <param name="trans">父節點</param>
    public static void RemoveChildren(Transform trans)
    {
        int childs = trans.childCount;
        if (childs <= 0) return;

        for (int i = childs - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(trans.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 重置 LocalTransform
    /// </summary>
    /// <param name="transform">Transform</param>
    public static void ResetLocalTransform(Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 判斷滑鼠是否點擊在 UI 上
    /// </summary>
    /// <returns>true|false</returns>
    public static bool IsPointerOverUIObject(Transform rootTransform)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();

        if (null == EventSystem.current) return false;

        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (RaycastResult r in results)
        {
            bool isUIClick = r.gameObject.transform.IsChildOf(rootTransform);
            if (isUIClick) return true;
        }
        return false;
    }


    #region Protobuf
    // 序列化（轉為二進位）
    public static byte[] ProtobufSerialization<T>(T data) where T : Google.Protobuf.IMessage
    {
        // 1. 獲取訊息序列化所需的總位元組數
        int size = data.CalculateSize();

        // 2. 建立指定大小的位元組陣列
        byte[] buffer = new byte[size];

        // 3. 使用該陣列初始化 CodedOutputStream
        using (CodedOutputStream output = new CodedOutputStream(buffer))
        {
            data.WriteTo(output);
            output.CheckNoSpaceLeft(); // 檢查空間是否正確
        }
        return buffer;
    }

    // 反序列化（從二進位還原）
    public static T ProtobufDeserialization<T>(byte[] bytes, MessageParser<T> parser) where T : IMessage<T>
    {
        return parser.ParseFrom(bytes);
    }

    /*
        使用範例

        ProtoTest data = new ProtoTest
        {
            Id = 1,
            Name = "Marry",
            Email = "aaassssddddd"
        };
        byte[] bytes = ToolHelpers.ProtobufSerialization(data);
        ProtoTest restored = ToolHelpers.ProtobufDeserialization(bytes, ProtoTest.Parser);

    */
    #endregion
}
