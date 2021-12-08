@echo off
setlocal

set _MANUAL=false
set _IN_JSON=MonsterData.json
set _IN_FBS=MonsterData.fbs
set _OUT_PATH=..\MonsterBattleCS\MonsterBattleCS
set _OUT_BIN=%_IN_JSON:json=mdfb%

@rem �r���h�c�[������ĂԎ��͈�����ݒ肷��(�������̂ɂ͈Ӗ����Ȃ�)
@rem �܂�A�������������͎蓮�ŃR���o�[�g���Ă��鎞

if "%1" neq "" goto begin
set _MANUAL=true


:begin

@rem �J�����g�f�B���N�g�������̃t�@�C��������f�B���N�g���Ɉړ�
cd /D %~dp0

@rem C#�p�̃w���p�[�R�[�h�𐶐�����
flatc.exe --csharp -o %_OUT_PATH% %_IN_FBS%
if %errorlevel% neq 0 goto error1

@rem JSON�����ꂽ�����X�^�[�f�[�^���o�C�i���ɃV���A���C�Y����
flatc.exe --unknown-json -b -o %_OUT_PATH% %_IN_FBS% %_IN_JSON%
if %errorlevel% neq 0 goto error2

echo �ϊ������I %_IN_JSON% with %_IN_FBS% �� %_OUT_BIN%
goto end


:error1
echo FlatBuffers�̒�`�t�@�C���o�͉ߒ��ŃG���[������܂�
goto end

:error2
echo FlatBuffers�̃��C���[�f�[�^�o�͉ߒ��ŃG���[������܂�
goto end


:end
if "%_MANUAL%" equ "true" pause
exit /b %errorlevel%
