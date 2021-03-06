﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using KSP.IO;

//using MuMech;

namespace DevHelper
{
    public partial class DevHelper : MonoBehaviour
    {
        public bool autoLoadSave = true;
        public string autoLoadSaveName = "default";

        public bool autoLoadScene = true;
        public string autoLoadSceneName = "VAB";

        private List<string> saveNames;
        private void FindSaves()
        {
            print("FindSaves");
            var dirs = Directory.GetDirectories(KSPUtil.ApplicationRootPath + "saves\\");
            saveNames = dirs.Where(x => System.IO.File.Exists(x + "\\persistent.sfs")).Select(x => x.Split(new[] { '\\' })[1]).ToList();
        }

        //IButton DHReloadDatabase;
        private void Awake()
        {
            print("Injector awake");
            DontDestroyOnLoad(this);
        }
        private void Start()
        {
            print("DevHelper Starting");

            if (ToolbarManager.ToolbarAvailable)
            {
                DHButtons();
                Debug.Log("buttons loaded");
            }

            loadConfigXML();
            FindSaves();

            InitComboBox();
            InitComboBoxScenes();
        }

        public void loadConfigXML()
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<DevHelper>();
            config.load();

            autoLoadSave = config.GetValue<bool>("autoLoadSave");
            autoLoadSaveName = config.GetValue<string>("autoLoadSaveName");
            autoLoadScene = config.GetValue<bool>("autoLoadScene");
            autoLoadSceneName = config.GetValue<string>("autoLoadSceneName");
        }

        public void saveConfigXML()
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<DevHelper>();
            config.SetValue("autoLoadSave", autoLoadSave);
            config.SetValue("autoLoadSaveName", autoLoadSaveName);
            config.SetValue("autoLoadScene", autoLoadScene);
            config.SetValue("autoLoadSceneName", autoLoadSceneName);
            config.save();
        }

        private bool bDoOnce = true;
        private void Update()
        {
            var menu = GameObject.Find("MainMenu");
            if (menu != null && bDoOnce)
            {
                bDoOnce = false;

                if (autoLoadSave)
                {
                    HighLogic.CurrentGame = GamePersistence.LoadGame("persistent", autoLoadSaveName, true, false);
                    if (HighLogic.CurrentGame != null)
                    {
                        HighLogic.SaveFolder = autoLoadSaveName;
                        //load to scene if needed
                        if (autoLoadScene)
                        {
                            switch (autoLoadSceneName)
                            {
                                case "VAB":
                                    HighLogic.CurrentGame.startScene = GameScenes.EDITOR;
                                    break;
                                case "SPH":
                                    HighLogic.CurrentGame.startScene = GameScenes.SPH;
                                    break;
                                case "Tracking Station":
                                    HighLogic.CurrentGame.startScene = GameScenes.TRACKSTATION;
                                    break;
                                case "Space Center":
                                    HighLogic.CurrentGame.startScene = GameScenes.SPACECENTER;
                                    break;
                                default:
                                    HighLogic.CurrentGame.startScene = GameScenes.SPACECENTER;
                                    break;
                            }

                        }
                        HighLogic.CurrentGame.Start();
                    }
                }
                else
                {
                    //pop up load game dialog.
                    var mc = menu.GetComponent<MainMenu>();
                    mc.continueBtn.onPressed.Invoke();
                }
            }
        }
        private IButton DHReloadDatabase;

        //private BoxDrawable boxDrawable;
        internal void DHButtons()
        {
            // button that toggles its icon when clicked
            DHReloadDatabase = ToolbarManager.Instance.add("DevHelper", "DHReloadGD");
            DHReloadDatabase.TexturePath = "DevHelper/Textures/icon_buttonReload";
            DHReloadDatabase.ToolTip = "Reload Game Database";
            DHReloadDatabase.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.SPH, GameScenes.SPACECENTER);
            DHReloadDatabase.OnClick += (e) =>
            {
                GameDatabase.Instance.Recompile = true;
                GameDatabase.Instance.StartLoad();
                PartLoader.Instance.Recompile = true;
                PartLoader.Instance.StartLoad();
            };
        }

        void OnDestroy()
        {
            if (ToolbarManager.ToolbarAvailable)
            {
                DHReloadDatabase.Destroy();
            }
        }
        
        
        private bool isTooLateToLoad = false;
      

        public void OnLevelWasLoaded(int level)
        {
            print("OnLevelWasLoaded:" + level);

            if (PSystemManager.Instance != null && ScaledSpace.Instance == null)
            {
                isTooLateToLoad = true;
            }
        }
    }
}

public class DevHelperPartlessLoader : KSP.Testing.UnitTest
{
    public DevHelperPartlessLoader()
    {
        DevHelperPluginWrapper.Initialize();
    }
}

public static class DevHelperPluginWrapper
{
    public static GameObject DevHelper;

    public static void Initialize()
    {
        if (GameObject.Find("DevHelper") == null)
        {
            DevHelper = new GameObject(
                "DevHelper",
                new [] {typeof (DevHelper.DevHelper)});
            UnityEngine.Object.DontDestroyOnLoad(DevHelper);
        }
    }
}


