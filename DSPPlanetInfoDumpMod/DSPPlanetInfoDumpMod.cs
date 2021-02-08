using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
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
    [BepInPlugin("jp.osilver.dk.plugins.dspmod.PlanetInfoDump", "DSP Planet Info Dump Mod", "0.0.0.2")]
    public class DSPPlanetDumpMod : BaseUnityPlugin
    {
        private static ConfigEntry<bool> configFullExtact;

        private void Awake()
        {
            LogManager.Logger = Logger;
            LogManager.Logger.LogInfo("DSP Planet Dump Mod Info plugin start");
            configFullExtact = Config.Bind(
                "General",
                "FullExtract",
                false,
                "DSP起動前に値を設定してください。" + Environment.NewLine +
                "false:保持情報そのままの資源情報を出力します。検索済みでない惑星の資源は0になります。" + Environment.NewLine +
                "true:全ての資源情報を出力します。初回出力時にフリーズしますが放置すれば大丈夫です。全ての惑星が検索済みになります。セーブデータが増加します。");
            Harmony.CreateAndPatchAll(typeof(UISaveGameWindow_SaveSucceed));
        }

        [HarmonyPatch(typeof(UISaveGameWindow), "SaveSucceed")]
        public class UISaveGameWindow_SaveSucceed
        {
            [HarmonyPostfix]
            public static void Postfix(UISaveGameWindow __instance)
            {
                bool isFullExtract = configFullExtact.Value;
                try
                {
                    LogManager.Logger.LogInfo("DSP Planet Info Dump Mod plugin UISaveGameWindow#SaveSucceed Postfix start");
                    var starDetail = UIRoot.instance.uiGame.starDetail;
                    var planetDetail = UIRoot.instance.uiGame.planetDetail;
                    var currentStar = starDetail.star;
                    var currentPlanet = planetDetail.planet;
                    var currentUniverseObserveLevel = GameMain.history.universeObserveLevel;
                    if (isFullExtract)
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
                    var planetStringBuilderExtraInfoHeader = "惑星名\t惑星種別\t軌道半径対象\t軌道半径\t公転周期\t自転周期\t軌道傾斜角度\t自転軸傾斜角";
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
                            if (isFullExtract)
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
                                var value = uiResAmountEntry.valueString;
                                planetStringBuilder.Append(name).Append("\t");
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
                            planetStringBuilderExtraInfo.Append(GetPlanetDetailField(planetDetail, "typeText")).Append("\t");
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
