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
    * PlanetsResource-[yyyyMMddHHmmss].txt
        * 惑星の資源情報一覧
    * PlanetsExtraInfo-[yyyyMMddHHmmss].txt
        * 惑星の惑星情報一覧
* 訪れていない惑星の資源は0表示になります。

# 設定オプション

* 設定ファイルパス
    * 「SteamLibrary\steamapps\common\Dyson Sphere Program\BepInEx\config\jp.osilver.dk.plugins.dspmod.PlanetInfoDump.cfg
* 設定内容

```
## DSP起動前に値を設定してください。
## false:保持情報そのままの資源情報を出力します。検索済みでない惑星の資源は0になります。
## true:全ての資源情報を出力します。初回出力時にフリーズしますが放置すれば大丈夫です。全ての惑星が検索済みになります。セーブデータが増加します。
# Setting type: Boolean
# Default value: false
FullExtract = false
```

