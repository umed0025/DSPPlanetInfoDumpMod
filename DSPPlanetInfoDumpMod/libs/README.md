# 必要なライブラリリスト

## DSPから取得
* 「SteamLibrary/steamapps/common/Dyson Sphere Program/DSPGAME_Data/Managed」から取得
    * Assembly-CSharp.dll
    * UnityEngine.CoreModule.dll
    * UnityEngine.dll
    + UnityEngine.UI.dll

## 別途ダウンロード
* [BepInEx 5.4.11](https://github.com/BepInEx/BepInEx/releases)からダウンロード
* 「BepInEx/core」より取得
    * 0Harmony.dll
    * BepInEx.dll
    * BepInEx.Harmony.dll

* うまくコンパイルできない場合はdllが参照している「mscorlib.dll」を4.0系から2.0系に修正してください。
    * [Loading any external dlls and invoking methods during PrePatcher fails](https://github.com/BepInEx/BepInEx/issues/177#issuecomment-770322150)の「DowngradeDll.zip」dllに適用してください。
