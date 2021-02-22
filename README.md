# 概要

* Dyson Sphere Program Modです。
* テキストベースの惑星情報一覧が出力することが出来るようになります。

# 導入方法

* 前提プラグイン
    * BepInEx 5.4.5
* 導入先
    * 「SteamLibrary/steamapps/common/Dyson Sphere Program/BepInEx/plugins/DSPPlanetDumpMod」に「DSPPlanetInfoDumpMod.dll」を配置してください。

# 実行方法

* ゲーム画面で「ESCキー」を押し「F2」キーを押すと「惑星情報一覧出力」が表示されます。
* 「標準出力」ボタンまたは、「全出力」ボタンを押すと以下のファイルが出力されます。
    * PlanetarySystems-[yyyyMMddHHmmss].txt
        * 惑星系の情報一覧
    * PlanetsResource-[yyyyMMddHHmmss].txt
        * 惑星の資源情報一覧
    * PlanetsExtraInfo-[yyyyMMddHHmmss].txt
        * 惑星の惑星情報一覧
* 標準出力で訪れていない惑星の資源は0表示になります。
* 全出力を行うと、全ての惑星が検索済みになります。出力に時間がかかるためフリーズしているように見えますが放置すれば大丈夫です。
* 「F2」キーの設定については以下のファイルで修正可能です。
  * 「SteamLibrary/steamapps/common/Dyson Sphere Program/BepInEx/config/jp.osilver.dk.plugins.dspmod.PlanetInfoDump.cfg」

# 出力されたデータの利用方法について

* タブ区切りで出力しているため、Googleスプレッドシートや、Excelへ貼り付けることによりフィルターによる絞り込みが可能です。
