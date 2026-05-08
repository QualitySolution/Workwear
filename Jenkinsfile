properties([parameters([
	booleanParam(defaultValue: false, description: 'Запускать тесты скриптов SQL', name: 'SQLTests'),
	booleanParam(defaultValue: false, description: 'Выкладывать сборку на сервер files.qsolution.ru', name: 'Publish'),
	booleanParam(defaultValue: false, description: 'Собирать и выкладывать RPM пакет для AltLinux в репозиторий', name: 'PublishAltLinuxRpm')
])])
node {
   stage('Git') {
      checkout([
         $class: 'GitSCM',
         branches: scm.branches,
         doGenerateSubmoduleConfigurations: scm.doGenerateSubmoduleConfigurations,
         extensions: scm.extensions + [submodule(disableSubmodules: false, recursiveSubmodules: true)],
         userRemoteConfigs: scm.userRemoteConfigs
      ])
   }
   stage('Nuget') {
      sh 'nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
      sh 'nuget restore QSProjects/QSProjectsLib.sln'
      sh 'nuget restore Workwear.sln'
   }
   stage('Test dotnet')
   {
      sh 'rm -rf Workwear.Test/TestResults'
      sh 'rm -rf Workwear.Test.Sql/TestResults' //Удаляем старые результаты тестов здесь, чтобы не оставались когда sql тесты не запускаются.
      try {  
         sh 'dotnet test --logger trx --collect:"XPlat Code Coverage" --settings coverlet.runsettings Workwear.Test/Workwear.Test.csproj'
      } catch (e) {}
      finally{
         discoverReferenceBuild()
         recordCoverage id: 'coverage-prev', name: 'Coverage (vs previous build)', ignoreParsingErrors: true, tools: [[parser: 'COBERTURA', pattern: 'Workwear.Test/TestResults/*/coverage.cobertura.xml']]
         discoverGitReferenceBuild()
         recordCoverage id: 'coverage-master', name: 'Coverage (vs master)', ignoreParsingErrors: true, tools: [[parser: 'COBERTURA', pattern: 'Workwear.Test/TestResults/*/coverage.cobertura.xml']]
         mstest testResultsFile:"**/*.trx", keepLongStdio: true
      }
   }
   stage('Build') {
      sh 'rm -f WinInstall/workwear-*.exe'
      sh 'WinInstall/makeWinInstall.sh'
      archiveArtifacts artifacts: 'WinInstall/workwear-*.exe', onlyIfSuccessful: true
   }
   stage('Test'){
      try {
         def PACKAGES_LOCATION = "${JENKINS_HOME}/.nuget/packages"
         sh """
            cd WorkwearTest/bin/ReleaseWin
            cp -r ${PACKAGES_LOCATION}/nunit.consolerunner/3.15.0/tools/* .
            mono nunit3-console.exe WorkwearTest.dll --framework=mono-4.0
         """
      } catch (e) {}
      finally{
         nunit testResultsPattern: 'WorkwearTest/bin/ReleaseWin/TestResult.xml'
      }
   }
   if (params.SQLTests) {
      stage('SQLTests'){
         try {  
            sh 'dotnet test --logger trx Workwear.Test.Sql/Workwear.Test.Sql.csproj '
         } catch (e) {}
         finally{
            mstest testResultsFile:"**/*.trx", keepLongStdio: true
         }
      }
   }
   if (params.PublishAltLinuxRpm) {
      stage('Build and Publish AltLinux RPM') {
         sh 'rm -f AltLinuxRpm/qs-workwear-*.rpm'
         def version = sh(script: "grep -oP '(?<=AssemblyVersion\\(\")[^\"]+' Workwear/Properties/AssemblyInfo.cs", returnStdout: true).trim()
         // Собираем Linux-бинарники
         sh 'msbuild /p:Configuration=Release /p:Platform=x86 Workwear.sln'
         // Снимаем BOM если файлы были сохранены с ним из Windows/IDE
         sh "sed -i '1s/^\\xEF\\xBB\\xBF//' AltLinuxRpm/makeRpm.sh AltLinuxRpm/workwear.spec"
         // Собираем Docker-образ с AltLinux + rpmbuild (сборка под непривилегированным пользователем)
         sh 'docker build -t workwear-altlinux-builder AltLinuxRpm/'
         // Даём права на запись в папку AltLinuxRpm пользователю builder (uid 1000) внутри контейнера
         sh 'chmod o+w AltLinuxRpm'
         // Запускаем сборку RPM внутри контейнера (workspace монтируется в /workspace)
         sh "docker run --rm -v \${WORKSPACE}:/workspace workwear-altlinux-builder bash /workspace/AltLinuxRpm/makeRpm.sh ${version}"
         archiveArtifacts artifacts: 'AltLinuxRpm/qs-workwear-*.rpm', onlyIfSuccessful: true
         // Загружаем RPM в репозиторий на сервере
         sh 'scp AltLinuxRpm/qs-workwear-*.rpm root@odysseus.srv.qsolution.ru:/var/www/repo/alt/workwear/x86_64/RPMS.workwear/'
         // Обновляем индекс репозитория
         sh 'ssh root@odysseus.srv.qsolution.ru "genbasedir /var/www/repo/alt/workwear x86_64"'
      }
   }
   if (params.Publish) {
      stage('VirusTotal'){
         sh 'vt scan file WinInstall/workwear-*.exe > file_hash'
         waitUntil (){
            sleep(30) //VirusTotal позволяет выполнить не более 4-х запросов за минуту.
            sh 'cut file_hash -d" " -f2 | vt analysis - > analysis'
            return readFile('analysis').contains('status: "completed"')
         }
         sh 'cat analysis'
         script {
            def status = readFile(file: "analysis")
            if ( !(status.contains('harmless: 0') && status.contains('malicious: 0') && status.contains('suspicious: 0'))) {
               unstable('VirusTotal in not clean')
            }
         }
      }
      stage('Publish'){
         sh 'scp WinInstall/workwear-*.exe root@odysseus.srv.qsolution.ru:/var/www/files/Workwear/'
      }
   }
}
