using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Singletons/MasterManager")]
public class MasterManager : SingletonScriptableObject<MasterManager> 
{
    [SerializeField]
    private GameSetting _gameSetting;
    public static GameSetting GameSetting { get { return Instance._gameSetting; } }

    [SerializeField]
    private DebugConsole _debugConsole;
    public static DebugConsole DebugConsole { get { return Instance._debugConsole; } }
}

// Assuming DebugConsole is another class
public class DebugConsole
{
    // DebugConsole implementation
    public void AddText(string message, object context)
    {
        // Implementation for adding text to the console
        Debug.Log($"{context}: {message}");
    }
}

