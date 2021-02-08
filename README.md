# 概要

* Dyson Sphere Program Modです。
* 手動セーブ終了後のセーブ成功ダイアログのOKボタンを押すとテキストベースの惑星情報一覧が出力されるようになります。

# 導入方法

* 前提プラグイン
    * BepInEx 5.4.5
* 導入先
    * 「SteamLibrary/steamapps/common/Dyson Sphere Program/BepInEx/plugins/DSPPlanetDumpMod」に「DSPPlanetInfoDumpMod.dll」を配置してください。

# 実行方法

* 手動セーブを行うと「SteamLibrary/steamapps/common/Dyson Sphere Program/」配下に以下のテキスト出力されます。
    * PlanetarySystems-[yyyyMMddHHmmss].txt
        * 惑星系の情報一覧
    * Planets-[yyyyMMddHHmmss].txt
        * 惑星の資源情報一覧
    * PlanetsExtraInfo-[yyyyMMddHHmmss].txt
        * 惑星の惑星情報一覧
* 訪れていない惑星の資源は0表示になります。


