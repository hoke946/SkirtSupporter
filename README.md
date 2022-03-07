# SkirtSupporter / LonghairSupporter
## 緊急お知らせ
これはVRChat用アバターのDynamicBone設定を支援するツールでしたが、  
VRChatのPhysBone対応を受け、取り急ぎPhysBone対応版を用意しました。  
  
・Insideコライダーがないため、Planeコライダーで代用しました。  
・SkirtHangはPhysBoneでは機能しないため、Constraintで代用しました。  
・TwistCancelはPhysBoneでは機能しないため、Constraintで代用しました。  
・HairHangは諦めました。  
・SkirtHangやHairHangで階層を組み替えていたボーンを、未使用の場合は本来のParentの位置に戻すようにしました。  
  
まだ、細かい説明はありませんが、使用条件、使用方法はDynamicBone版とほぼ同様です。  
DynamicBone版はVRChatのPhysBone移行を機に終息させる予定です。

## ダウンロード
こちらから入手できます。  
[SkirtSupporterPB最新版 v0.0.1](https://github.com/hoke946/SkirtSupporter/releases/tag/SSPBv0.0.1_LSPBv0.0.1)  
[LonghairSupporterPB最新版 v0.0.1](https://github.com/hoke946/SkirtSupporter/releases/tag/SSPBv0.0.1_LSPBv0.0.1) 

## 最新バージョン
SkirtSupporterPB : 0.0.1  
LonghairSupporterPB : 0.0.1  
  
-----

## DynamicBone版
これはVRChat用アバターのDynamicBone設定を支援するツールです。  
SkirtSupporterの詳細は https://sites.google.com/view/skirtsup をご覧ください。  
LonghairSupporterの詳細は https://sites.google.com/view/longhairsup をご覧ください。  

## ダウンロード
こちらから入手できます。  
[SkirtSupporter最新版 v1.2.2](https://github.com/hoke946/SkirtSupporter/releases/tag/SSv1.2.2)  
[LonghairSupporter最新版 v1.0.2](https://github.com/hoke946/SkirtSupporter/releases/tag/SSv1.2.1_LSv1.0.2) 

## 説明書
⇒[SkirtSupporter](https://sites.google.com/view/skirtsup)  
⇒[LonghairSupporter](https://sites.google.com/view/longhairsup)  

## 利用について
・当ツールのソースコードの改変、二次利用は問題ありません。  
　その場合、必須ではありませんが、当ツールを参考にしたなどの紹介があると嬉しいです。  
・当ツールを使った成果物の販売も問題ありません。  
　こちらも紹介があると勿論嬉しいですが、クレジットは必須ではありません。  
・当ツールの製作元を偽って配布する行為のみ、禁止です。  
・当ツールを使用したことにより発生したいかなるトラブルについても、製作者は一切責任を負いません。  

## 最新バージョン
SkirtSupporter : 1.2.2  
LonghairSupporter : 1.0.2  
  
## 更新履歴
2021/3/28 Git運用見直し  
  
  
それまでの更新履歴  
  
[SkirtSupporter]  
2020/4/3  1.2.1 Unity2018.4.20f1対応  
2020/2/9  1.2.0 ねじり打ち消し機構(TwistCancel)導入　何もせず再実行を可能とする　コライダーObjectのスケール修正  
2020/1/30 1.1.4 チェック機能をアップグレード  
2019/8/22 1.1.3 SkirtBones自動取得の潜在的な不具合を修正  
2019/8/22 1.1.2 SkirtBones自動取得の不具合を修正  
2019/8/17 1.1.1 Humanoid以外のアバターに対応　SkirtBonesの自動設定にHumanoidボーンを除外  
2019/8/10 1.1.0 AvatarObjectsパラメータ変更　SkirtBones自動設定機能追加  
2019/8/6  1.0.4 SkirtHangを設定するとスカートボーンの角度がおかしくなる不具合を修正  
2019/8/3  1.0.3 SkirtHangを設定するとスカートボーンの位置がおかしくなる場合がある不具合を修正  
2019/8/3  1.0.2 Hipsの角度に依存しないように修正  
2019/7/28 1.0.1 DynamicBoneの状態によってエラーとなる不具合を修正  
2019/7/27 1.0.0 公開  
  
[LonghairSupporter]  
2020/4/3  1.0.2 Unity2018.4.20f1対応　何もせず再実行を可能とする  
2020/1/30 1.0.1 チェック機能をアップグレード  
2019/8/26 1.0.0 公開  
  
  
製作者：ほけ  
(VRChat：hoke　twitter：@hoke946)
