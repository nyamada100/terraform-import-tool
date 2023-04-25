# terraform-import-tool
新しいtfstateと既存tfstateを比較して新規リソース定義を抜き出す

## 必要なもの
- terraformコマンド
- tfstate-lookupコマンド
- 新しいリソース定義 tfstate, tf
- 古いリソース定義 tfstate,tf
    - 古いリソース定義は複数のディレクトリに分かれていてもよい

## ビルド
- .NET SDK のLinux環境を用意する
    - バージョン:7
- dotnet buildでビルド
- dotnet publishで発行

## 使い方
1. 発行した結果の以下のファイルを同じディレクトリに配置する
    - terraform-import-tool
    - resource_identifier.csv
    - settings.toml(settings.toml.sampleをコピー)
1. settings.tomlの中身を設定する
1. 新しいリソース定義 tfstate, tfと古いリソース定義 tfstate,tfを用意する
1. terraform-import-toolを実行する
1. 実行後、以下のファイルが生成される
    - terraform-import-tool.log:ログファイル
    - new_resource/*.tf : 新たなリソース定義のファイル
    - new_resource/concat/*.tf : 新たなリソースをタイプごとにファイルにまとめたもの
1. 新たなリソースを既存のtfstateにマージするにはterraform state mvを使用する

