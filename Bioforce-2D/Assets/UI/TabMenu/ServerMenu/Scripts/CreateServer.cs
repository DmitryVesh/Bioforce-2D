using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateServer : MonoBehaviour
{
    //Server name
    [SerializeField] TMP_InputField ServerNameInputField;


    //Map selection
    [SerializeField] TMP_Dropdown MapDropdown;
    [SerializeField] private List<string> SceneNamesNotToIncludeInMapDropdown 
        = new List<string> { "Main Menu" };
    private List<string> MapNames { get; set; }


    //Player count
    [SerializeField] 

    //Final Server Data
    public string ServerNameSelected { get; set; }
    public string MapSelected { get; set; }

    public void OnServerNameChanged()
    {
        ServerNameSelected = ServerNameInputField.text;
    }
    public void OnMapDropdownChanged(int option)
    {
        MapSelected = MapNames[option];
    }
    
    void Start()
    {
        SetMapDropdownOptions();        
    }

    private void SetMapDropdownOptions()
    {
        MapDropdown.ClearOptions();
        MapNames = new List<string>();
        int numScenes = SceneManager.sceneCountInBuildSettings;

        for (int sceneCount = 0; sceneCount < numScenes; sceneCount++)
        {
            string sceneName = SceneManager.GetSceneAt(sceneCount).name;
            if (SceneNamesNotToIncludeInMapDropdown.Contains(sceneName))
                continue;

            MapNames.Add(sceneName);
        }
        MapDropdown.AddOptions(MapNames);
        MapSelected = MapNames[0];
    }

}
