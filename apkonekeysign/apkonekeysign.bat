@rem 准备一个需要重新签名的apk放在与此脚本相同的路径

@set READYAPK=%1
@set CURDIR=%~dp0

@set NEWAPK_NAME_NOEXT=%CURDIR%%2
@set NEWAPK_NAME=%NEWAPK_NAME_NOEXT%.apk
@set NEWTEMPAPK_NAME_NOEXT=%NEWAPK_NAME_NOEXT%temp
@set NEWTEMPAPK_NAME=%NEWTEMPAPK_NAME_NOEXT%.apk

@rem keystore路径和名字
@set KEYSTORE_PATH=%CURDIR%debug.keystore
@set KEYSTORE_ALIAS=androiddebugkey
@set KEYSTORE_PASS=android
@set KEYSTORE_KEYPASS=android
@set KEYTOOL_PATH="C:\Program Files\Java\jdk1.8.0_162\bin\keytool.exe"
@set JARSIGNER_PATH="C:\Program Files\Java\jdk1.8.0_162\bin\jarsigner.exe"

@cd /d %CURDIR%
@echo %CURDIR%

@rem 步骤一删除目标文件（如果有）
@del %NEWAPK_NAME%
@del %NEWTEMPAPK_NAME%

@rem 步骤二拷贝源apk->tempapk
@echo first step ...
@echo copy a origin apk start
@copy %READYAPK% %NEWTEMPAPK_NAME%
@echo copy a origin apk end

@rem 解析tempapk
@echo apktool decode apk start
@echo off
@call apktool d -f %NEWTEMPAPK_NAME%
@echo on
@cd /d %CURDIR%
@del %NEWTEMPAPK_NAME%
@echo apktool decode apk end

@rem 删除temp的签名
@echo del old sign name start
@cd /d %NEWTEMPAPK_NAME_NOEXT%\original
@rd /s/q META-INF
@cd /d %CURDIR%
@echo del old sign name end

@rem 重新生成tempapk
@echo apktool encode apk start
@echo off
@call apktool b %NEWTEMPAPK_NAME_NOEXT% -o %NEWTEMPAPK_NAME%
@echo on
@rd /s/q %NEWTEMPAPK_NAME_NOEXT%
@echo apktool encode apk end

@rem 设置编码
@chcp 936

@rem 检查生成debug.keystore
@if not exist %KEYSTORE_PATH% (
    @echo keytool gen debug.keystore start
    @call %KEYTOOL_PATH% -genkey -v -keystore %KEYSTORE_PATH% -alias %KEYSTORE_ALIAS% -keyalg RSA -validity 10000
    @echo keytool gen debug.keystore end
)else (
    @echo use exist keystore
)

@rem 重新签名
@echo jarsigner apk start
@call %JARSIGNER_PATH% -verbose -keystore %KEYSTORE_PATH% -storepass %KEYSTORE_PASS% -keypass %KEYSTORE_KEYPASS% -signedjar %NEWAPK_NAME% %NEWTEMPAPK_NAME% %KEYSTORE_ALIAS%
@del %NEWTEMPAPK_NAME%
@echo jarsigner apk end

@rem 优化apk(azipalign,android sdk自带)
@echo zipalign apk start
@rem zipalign -v 4 signed.apk finial.apk
@echo zipalign apk end

@echo apkonekeysign.bat success!!!
@exit
