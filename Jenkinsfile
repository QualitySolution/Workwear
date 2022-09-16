properties([parameters([
	booleanParam(defaultValue: false, description: 'Запускать тесты скриптов SQL', name: 'SQLTests'),
	booleanParam(defaultValue: false, description: 'Выкладывать сборку на сервер files.qsolution.ru', name: 'Publish')
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
   	  sh 'dotnet test --logger trx --collect:"XPlat Code Coverage" Workwear/Workwear.Test/Workwear.Test.csproj'
   	  cobertura autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/TestResults/**/coverage.cobertura.xml', conditionalCoverageTargets: '70, 0, 0', failUnhealthy: false, failUnstable: false, lineCoverageTargets: '80, 0, 0', maxNumberOfBuilds: 0, methodCoverageTargets: '80, 0, 0', onlyStable: false, zoomCoverageChart: false
   	  mstest testResultsFile:"**/*.trx", keepLongStdio: true
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
         sh 'dotnet test Workwear/Workwear.Test.Sql/Workwear.Test.Sql.csproj '
      }
   }
   if (params.Publish) {
      stage('Publish'){
         sh 'scp Workwear/WinInstall/workwear-*.exe a218160_qso@a218160.ftp.mchost.ru:subdomains/files/httpdocs/Workwear/'
      }
   }
}
