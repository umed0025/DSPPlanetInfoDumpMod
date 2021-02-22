using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UI = UnityEngine.UI;
namespace DSPPlanetInfoDumpMod
{
    [BepInPlugin("PlanetInfoDump", "DSP Planet Info Dump Mod", "1.0.0.1")]
    public class DSPPlanetDumpMod : BaseUnityPlugin
    {
        private static ConfigEntry<KeyCode> configOutputMessageBoxHotKey;
        public void Awake()
        {
            LogManager.Logger = Logger;
            Harmony.CreateAndPatchAll(typeof(DSPPlanetDumpMod));
        }

        public void Start()
        {
            LogManager.Logger.LogInfo("DSP Planet Dump Mod Info plugin start");
            configOutputMessageBoxHotKey = Config.Bind(
                "General",
                "configOutputMessageBoxHotKey",
                KeyCode.F2,
               "出力用メッセージボックスを表示するキーを設定します。");
        }

        public void Update()
        {
            if (GameMain.isPaused || GameMain.isFullscreenPaused)
            {
                if (Input.GetKeyDown(configOutputMessageBoxHotKey.Value))
                {
                    var message =
                        "標準出力：資源情報を出力します。" +
                        "全出力：全資源情報を出力します。フリーズしますが放置すれば大丈夫。";
                    var messageBox = UIMessageBox.Show(
                        "惑星情報一覧出力",
                        message,
                        "キャンセル",
                        "全出力",
                        "標準出力",
                        0,
                        new UIMessageBox.Response(this.Cancel),
                        new UIMessageBox.Response(this.FullExtract),
                        new UIMessageBox.Response(this.SimpleExtract)); ;
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        messageBox.FadeOut();
                    }
                }
            }
        }
        public void SimpleExtract()
        {
            UISaveGameWindow_SaveSucceed.IsFullExtract = false;
            UISaveGameWindow_SaveSucceed.Postfix();
        }

        public void FullExtract()
        {
            UISaveGameWindow_SaveSucceed.IsFullExtract = true;
            UISaveGameWindow_SaveSucceed.Postfix();
        }
        public void Cancel()
        {
            ;
        }

        private const string FULL_TYPE_TARGET_TEXT = "<color";

        public class UISaveGameWindow_SaveSucceed
        {
            public static bool IsFullExtract = false;
            public static void Postfix()
            {
                try
                {
                    LogManager.Logger.LogInfo("DSP Planet Info Dump Mod plugin UISaveGameWindow#SaveSucceed Postfix start");
                    var starDetail = UIRoot.instance.uiGame.starDetail;
                    var planetDetail = UIRoot.instance.uiGame.planetDetail;
                    var currentStar = starDetail.star;
                    var currentPlanet = planetDetail.planet;
                    var currentUniverseObserveLevel = GameMain.history.universeObserveLevel;
                    if (IsFullExtract)
                    {
                        GameMain.history.universeObserveLevel = 4;
                    }
                    var galaxy = GameMain.data.galaxy;
                    string starHeader = "Planetary System Name\tName\tAmount";
                    var starStringBuilder = new StringBuilder();
                    starStringBuilder.Append(string.Format("Seed No\t{0}", galaxy.seed)).AppendLine();
                    starStringBuilder.AppendLine();
                    starStringBuilder.Append(starHeader).AppendLine();
                    string planetHeader = "Planetary System Name\tPlanet Name\tName\tAmount";
                    string planetHeaderRemark = "\tIs Gas\tPlanet#Date\tPlanet#Factory\tFlag(all or)";
                    var planetStringBuilder = new StringBuilder();
                    planetStringBuilder.Append(string.Format("Seed No\t{0}", galaxy.seed)).AppendLine();
                    planetStringBuilder.AppendLine();
                    planetStringBuilder.Append(planetHeader);
                    planetStringBuilder.Append(planetHeaderRemark);
                    planetStringBuilder.AppendLine();
                    var planetStringBuilderExtraInfoHeader = "惑星名\t惑星種別\t特異性\t軌道半径対象\t軌道半径\t公転周期\t自転周期\t軌道傾斜角度\t自転軸傾斜角";
                    var planetStringBuilderExtraInfo = new StringBuilder();
                    planetStringBuilderExtraInfo.Append(string.Format("Seed No\t{0}", galaxy.seed)).AppendLine();
                    planetStringBuilderExtraInfo.AppendLine();
                    planetStringBuilderExtraInfo.Append(planetStringBuilderExtraInfoHeader);
                    planetStringBuilderExtraInfo.AppendLine();

                    foreach (var star in galaxy.stars)
                    {
                        List<bool> currentPlanetLoaded = new List<bool>();
                        foreach (var planet in star.planets)
                        {
                            currentPlanetLoaded.Add(planet.loaded);
                            planet.loaded = true;
                        }
                        foreach (var planet in star.planets)
                        {
                            if (IsFullExtract)
                            {
                                // implements from PlanetComputeThreadMain
                                var algorithm = PlanetModelingManager.Algorithm(planet);
                                if (planet.data == null)
                                {
                                    planet.data = new PlanetRawData(planet.precision);
                                    planet.modData = planet.data.InitModData(planet.modData);
                                    planet.data.CalcVerts();
                                    planet.aux = new PlanetAuxData(planet);
                                    algorithm.GenerateTerrain(planet.mod_x, planet.mod_y);
                                    algorithm.CalcWaterPercent();
                                }
                                if (planet.factory == null)
                                {
                                    if (planet.type != EPlanetType.Gas)
                                    {
                                        algorithm.GenerateVegetables();
                                        algorithm.GenerateVeins(false);
                                    }
                                }
                            }
                            planetDetail.planet = planet;
                            foreach (var uiResAmountEntry in planetDetail.entries)
                            {
                                planetStringBuilder.Append(star.displayName).Append("\t");
                                planetStringBuilder.Append(planet.displayName).Append("\t");
                                var name = GetUIResAmountEntryLabelText(uiResAmountEntry);
                                planetStringBuilder.Append(name).Append("\t");
                                var value = uiResAmountEntry.valueString;
                                planetStringBuilder.Append(value).Append("\t");
                                var isGas = (planet.type == EPlanetType.Gas);
                                var isData = (planet.data != null);
                                var isFactory = (planet.factory != null);
                                var isFlag = isGas || isData || isFactory;
                                planetStringBuilder.Append(isGas).Append("\t");
                                planetStringBuilder.Append(isData).Append("\t");
                                planetStringBuilder.Append(isFactory).Append("\t");
                                planetStringBuilder.Append(isFlag).Append("\t");
                                planetStringBuilder.AppendLine();
                            }
                            planetStringBuilderExtraInfo.Append(GetPlanetDetailField(planetDetail, "nameText")).Append("\t");
                            var fullTypeName = GetPlanetDetailField(planetDetail, "typeText");
                            LogManager.Logger.LogInfo("fullTypeName;" + fullTypeName);
                            int fullTypeTargetIndex = fullTypeName.IndexOf(FULL_TYPE_TARGET_TEXT);
                            int fullTypeNameLength = fullTypeName.Length;
                            var typeName = "";
                            var typespecificity = "";
                            if (0 < fullTypeTargetIndex)
                            {
                                typeName = fullTypeName.Substring(0, fullTypeTargetIndex); ;
                                typespecificity = fullTypeName.Substring(fullTypeTargetIndex, fullTypeNameLength - fullTypeTargetIndex);
                            }
                            planetStringBuilderExtraInfo.Append(typeName).Append("\t");
                            planetStringBuilderExtraInfo.Append(typespecificity).Append("\t");
                            planetStringBuilderExtraInfo.Append(GetPlanetDetailField(planetDetail, "orbitRadiusValueTextEx")).Append("\t");
                            planetStringBuilderExtraInfo.Append(GetPlanetDetailField(planetDetail, "orbitRadiusValueText")).Append("\t");
                            planetStringBuilderExtraInfo.Append(GetPlanetDetailField(planetDetail, "orbitPeriodValueText")).Append("\t");
                            planetStringBuilderExtraInfo.Append(GetPlanetDetailField(planetDetail, "rotationPeriodValueText")).Append("\t");
                            planetStringBuilderExtraInfo.Append(GetPlanetDetailField(planetDetail, "inclinationValueText")).Append("\t");
                            planetStringBuilderExtraInfo.Append(GetPlanetDetailField(planetDetail, "obliquityValueText")).AppendLine();
                        }
                        starDetail.star = star;
                        foreach (var uiResAmountEntry in starDetail.entries)
                        {
                            starStringBuilder.Append(star.displayName).Append("\t");
                            var name = GetUIResAmountEntryLabelText(uiResAmountEntry);
                            var value = uiResAmountEntry.valueString;
                            starStringBuilder.Append(name).Append("\t");
                            starStringBuilder.Append(value).AppendLine();
                        }
                        var planetIndex = 0;
                        foreach (var planet in star.planets)
                        {
                            planet.loaded = currentPlanetLoaded[planetIndex];
                        }
                    }
                    //LogManager.Logger.LogInfo(starStringBuilder.ToString());
                    //LogManager.Logger.LogInfo(planetStringBuilder.ToString());
                    //LogManager.Logger.LogInfo(planetStringBuilderExtraInfo.ToString());
                    var currentDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    File.WriteAllText(string.Format("PlanetarySystems-{0}.txt", currentDateTime), starStringBuilder.ToString());
                    File.WriteAllText(string.Format("PlanetsResource-{0}.txt", currentDateTime), planetStringBuilder.ToString());
                    File.WriteAllText(string.Format("PlanetsExtraInfo-{0}.txt", currentDateTime), planetStringBuilderExtraInfo.ToString());
                    starDetail.star = currentStar;
                    planetDetail.planet = currentPlanet;
                    GameMain.history.universeObserveLevel = currentUniverseObserveLevel;
                }
                catch (Exception e)
                {
                    LogManager.Logger.LogError(e.Message);
                    LogManager.Logger.LogError(e.StackTrace);
                }
                finally
                {
                    LogManager.Logger.LogInfo("DSP Planet Info Dump Mod plugin UISaveGameWindow#SaveSucceed Postfix end");
                }
            }

            private static string GetPlanetDetailField(UIPlanetDetail target, string fieldName)
            {
                UI.Text text = (UI.Text)GetValueFromPrivateField(typeof(UIPlanetDetail), fieldName, target);
                return text.text;
            }

            private static string GetUIResAmountEntryLabelText(UIResAmountEntry target)
            {
                UI.Text text = (UI.Text)GetValueFromPrivateField(typeof(UIResAmountEntry), "labelText", target);
                return text.text;
            }
            private static object GetValueFromPrivateField(Type type, string fieldName, object target)
            {
                return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
            }
        }
        public class LogManager
        {
            public static ManualLogSource Logger;
        }
    }
}

