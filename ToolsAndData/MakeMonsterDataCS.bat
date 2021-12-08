@echo off
setlocal

set _MANUAL=false
set _IN_JSON=MonsterData.json
set _IN_FBS=MonsterData.fbs
set _OUT_PATH=..\MonsterBattleCS\MonsterBattleCS
set _OUT_BIN=%_IN_JSON:json=mdfb%

@rem ビルドツールから呼ぶ時は引数を設定する(引数自体には意味がない)
@rem つまり、引数が無い時は手動でコンバートしている時

if "%1" neq "" goto begin
set _MANUAL=true


:begin

@rem カレントディレクトリをこのファイルがあるディレクトリに移動
cd /D %~dp0

@rem C#用のヘルパーコードを生成する
flatc.exe --csharp -o %_OUT_PATH% %_IN_FBS%
if %errorlevel% neq 0 goto error1

@rem JSON化されたモンスターデータをバイナリにシリアライズする
flatc.exe --unknown-json -b -o %_OUT_PATH% %_IN_FBS% %_IN_JSON%
if %errorlevel% neq 0 goto error2

echo 変換完了！ %_IN_JSON% with %_IN_FBS% ⇒ %_OUT_BIN%
goto end


:error1
echo FlatBuffersの定義ファイル出力過程でエラーがあります
goto end

:error2
echo FlatBuffersのワイヤーデータ出力過程でエラーがあります
goto end


:end
if "%_MANUAL%" equ "true" pause
exit /b %errorlevel%
