using UnityEngine;

public enum GameEventType
{
    #region Test
    TestEvent,
    #endregion
}

public class EventManager:EventCenter<GameEventType, Object>
{}
