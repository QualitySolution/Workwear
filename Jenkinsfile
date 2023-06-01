properties([parameters([
	booleanParam(defaultValue: false, description: 'Запускать тесты скриптов SQL', name: 'SQLTests'),
	booleanParam(defaultValue: false, description: 'Выкладывать сборку на сервер files.qsolution.ru', name: 'Publish'),
	booleanParam(defaultValue: false, description: 'Старый плагин отображения покрытия', name: 'OldCoverage')
])])
node {
   stage('Workwear') {
      checkout([
         $class: 'GitSCM',
         branches: scm.branches,
         doGenerateSubmoduleConfigurations: scm.doGenerateSubmoduleConfigurations,
         extensions: scm.extensions + [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Workwear']],
         userRemoteConfigs: scm.userRemoteConfigs
      ])
   }
   stage('QSProjects') {
      checkout([$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'QSProjects']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/QSProjects.git']]])
      sh 'nuget restore QSProjects/QSProjectsLib.sln'
   }
   stage('Gtk.DataBindings') {
      checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches: [[name: '*/NetStandard']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Gtk.DataBindings']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/Gtk.DataBindings.git']]]
   }
   stage('My-FyiReporting') {
      checkout changelog: false, scm: [$class: 'GitSCM', branches: [[name: '*/QSBuild']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'My-FyiReporting']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/My-FyiReporting.git']]]
      sh 'nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
   }
   stage('RusGuard') {
      checkout changelog: true, poll: true, scm: [$class: 'GitSCM', branches: [[name: '*/main']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'RusGuardSharp']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/RusGuardSharp.git']]]
   }
   stage('Test dotnet')
   {
      sh 'rm -rf Workwear/Workwear.Test/TestResults'
      sh 'rm -rf Workwear/Workwear.Test.Sql/TestResults' //Удаляем старые результаты тестов здесь, чтобы не оставались когда sql тесты не запускаются.
   	try {  
   	  sh 'dotnet test --logger trx --collect:"XPlat Code Coverage" Workwear/Workwear.Test/Workwear.Test.csproj'
      } catch (e) {}
      finally{
      if (params.OldCoverage) {
   	    cobertura autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/TestResults/**/coverage.cobertura.xml', conditionalCoverageTargets: '70, 0, 0', failUnhealthy: false, failUnstable: false, lineCoverageTargets: '80, 0, 0', maxNumberOfBuilds: 0, methodCoverageTargets: '80, 0, 0', onlyStable: false, zoomCoverageChart: false
   	  }
   	  else {
   	    publishCoverage adapters: [coberturaAdapter(mergeToOneReport: true, path: '**/TestResults/**/coverage.cobertura.xml')], checksName: '', sourceFileResolver: sourceFiles('STORE_LAST_BUILD')
   	  }
   	  mstest testResultsFile:"**/*.trx", keepLongStdio: true
      }
   }
   stage('Build') {
   	    sh 'nuget restore Workwear/Workwear.sln'
        sh 'rm -f Workwear/WinInstall/workwear-*.exe'
        sh 'Workwear/WinInstall/makeWinInstall.sh'
        archiveArtifacts artifacts: 'Workwear/WinInstall/workwear-*.exe', onlyIfSuccessful: true
   }
   stage('Test'){
       try {
            def PACKAGES_LOCATION = "${JENKINS_HOME}/.nuget/packages"
            sh """
                cd Workwear/WorkwearTest/bin/ReleaseWin
                cp -r ${PACKAGES_LOCATION}/nunit.consolerunner/3.15.0/tools/* .
                mono nunit3-console.exe WorkwearTest.dll --framework=mono-4.0
            """
       } catch (e) {}
       finally{
           nunit testResultsPattern: 'Workwear/WorkwearTest/bin/ReleaseWin/TestResult.xml'
       }
   }
   if (params.SQLTests) {
      stage('SQLTests'){
         try {  
            sh 'dotnet test --logger trx Workwear/Workwear.Test.Sql/Workwear.Test.Sql.csproj '
         } catch (e) {}
         finally{
            mstest testResultsFile:"**/*.trx", keepLongStdio: true
         }
      }
   }
   if (params.Publish) {
      stage('VirusTotal'){
         sh 'vt scan file Workwear/WinInstall/workwear-*.exe > file_hash'
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
         sh 'scp Workwear/WinInstall/workwear-*.exe a218160_qso@a218160.ftp.mchost.ru:subdomains/files/httpdocs/Workwear/'
      }
   }
}
