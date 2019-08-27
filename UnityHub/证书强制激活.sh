asarFile="app.asar"
unZipDir="app"
licenseClient="licenseClient.js"
licenseCore="licenseCore.js"

if which asar 2>/dev/null;then
	echo ""
elif which npm 2>/dev/null;then
	sudo npm install -g asar
else
	echo "failed, no npm installed."
	exit 0
fi

# 解压
asar extract $asarFile $unZipDir

cd $unZipDir/src/services/licenseService

# echo $(pwd)

chmod -R 777 $licenseClient
chmod -R 777 $licenseCore

# 强制激活
replace=`cat $licenseClient | sed 's/licenseInfo.activated = licenseCore.getLicenseToken().length > 0/licenseInfo.activated = true/g'`
echo "${replace}">$licenseClient

# macosx换行
sed -i '' -e '612i\'$'\n''resolve(true);' ${licenseCore}

cd ../../../../

rm $asarFile
