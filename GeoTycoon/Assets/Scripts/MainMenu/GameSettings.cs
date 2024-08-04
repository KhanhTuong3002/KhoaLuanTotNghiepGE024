using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static List<Setting> settingsList = new List<Setting>();
    public static List<MultiSetting> multisettingsList = new List<MultiSetting>();
    public static void ClearSetting()
    {
        settingsList = new List<Setting>();
        multisettingsList = new List<MultiSetting>();
    }

    public static void AddSetting(Setting setting)
    {
        settingsList.Add(setting);
        // Debug.Log(setting.playerName + "+" + setting.selectedType+"+" + setting.selectColor);
    }

    public static void AddMultiSetting(MultiSetting setting)
    {
        multisettingsList.Add(setting);
        Debug.Log(setting.playerName + "+" + setting.selectedType+"+" + setting.selectColor +"+" + setting.playerId);
    }
    //public static List<Setting> SettingsList => settingsList;
}

public class Setting
{
    public string playerName;
    public int selectedType;
    public int selectColor;
    public string setQuestionId;

    public Setting(string _name, int type, int color, string _setQuestionId)
    {
        playerName = _name;
        selectedType = type;
        selectColor = color;
        setQuestionId = _setQuestionId;
    }

}
public class MultiSetting
{
    public string playerName;
    public int selectedType;
    public int selectColor;

    public int playerId;
    public string setQuestionId;
    public MultiSetting(string _name, int type, int color, int _playerId, string _setQuestionId)
    {
        playerName = _name;
        selectedType = type;
        selectColor = color;
        playerId = _playerId;
        setQuestionId = _setQuestionId;
    }
}
