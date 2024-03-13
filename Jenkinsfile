properties([parameters([
	booleanParam(defaultValue: false, description: 'Запускать тесты скриптов SQL', name: 'SQLTests'),
	booleanParam(defaultValue: false, description: 'Выкладывать сборку на сервер files.qsolution.ru', name: 'Publish')
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
         sh 'dotnet test --logger trx --collect:"XPlat Code Coverage" Workwear.Test/Workwear.Test.csproj'
      } catch (e) {}
      finally{
         recordCoverage ignoreParsingErrors: true, tools: [[parser: 'COBERTURA', pattern: '**/coverage.cobertura.xml']]
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
         sh 'scp WinInstall/workwear-*.exe a218160_qso@a218160.ftp.mchost.ru:subdomains/files/httpdocs/Workwear/'
      }
   }
}
