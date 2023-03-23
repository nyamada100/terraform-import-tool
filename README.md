# terraform-import-tool
terraformerの結果と既存tfstateを比較して新規リソース定義を抜き出す


## 処理内容
- 新規のリソース定義を含むtfstateファイルと、現状のtfstateファイルから、新たに追加されたリソース定義をtfファイルに書き出す。

## 必要なもの
- 以下はコマンドとして使用できるようにしておくこと
  - tfstate-lookup
  - terraform
- 突合せを行うための、新規のtfsateファイルと、現状のtfstateファイル(複数でもよい)を用意しておくこと。それぞれのディレクトリで、tfstateファイルだけでなく、terraformが実行できるように「terraform init」を先に実行しておくこと
- settings.tomlを用意(sampleファイルをコピーして名前変更する)して、必要な設定を記載すること


