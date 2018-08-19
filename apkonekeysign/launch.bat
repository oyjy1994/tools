@set ORIGINAPKNAME=temp.apk
@rem apk签名后名字不要扩展名
@set SIGNEDAPK=temp_sign
@start /wait apkonekeysign.bat %ORIGINAPKNAME% %SIGNEDAPK%

@echo success!!!
@pause
