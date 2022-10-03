[![release](https://img.shields.io/badge/Unity-2020.3.0f1+-white.svg?style=flat&logo=unity)](https://github.com/negi0109/unity-aseprite-importer)
[![release](https://github.com/negi0109/unity-aseprite-importer/actions/workflows/release.yml/badge.svg?branch=master)](https://github.com/negi0109/unity-aseprite-importer/actions/workflows/release.yml)

# aseprite-importer
Unity上でasepriteファイルを扱えるようにするエディタ拡張

- 開発中のため随時必要機能は下記プロジェクトで管理しています。
  - https://github.com/users/negi0109/projects/8/views/1
  - 必要な機能は満たせており試運転中です。

## Install
Unity Package Managerから追加
- 最新版
    - https://github.com/negi0109/unity-aseprite-importer.git#release
- 過去のバージョンは [releases](https://github.com/negi0109/unity-aseprite-importer/releases) から確認・利用可能

## Features
- asepriteファイル上の1フレームごとのスプライト (スプライトシートの作成)
- スプライトの分割
- アニメーションの利用

## How To
### スプライトの分割
- 列数 (separates)の指定
- 列ごとの名前の設定

![screenshot1](README_Assets/screenshot1.png)
![screenshot2](README_Assets/screenshot2.png)

### アニメーションの利用
- exportAnimation を指定

![screenshot3](README_Assets/screenshot3.png)

- tagの読み込み

![screenshot4](README_Assets/screenshot4.png)

- 縁取り (edging)
スプライト描画時端にゴミが表示される場合に１マス透明マスを追加するためのオプション
