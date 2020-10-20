node {
   stage('Workwear') {
      checkout([
         $class: 'GitSCM',
         branches: scm.branches,
         doGenerateSubmoduleConfigurations: scm.doGenerateSubmoduleConfigurations,
         extensions: scm.extensions + [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Workwear']],
         userRemoteConfigs: scm.userRemoteConfigs
      ])
      sh 'nuget restore Workwear/Workwear.sln'
   }
   stage('QSProjects') {
      checkout([$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'QSProjects']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/QSProjects.git']]])
      sh 'nuget restore QSProjects/QSProjectsLib.sln'
   }
   stage('Gtk.DataBindings') {
      checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Gtk.DataBindings']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/Gtk.DataBindings.git']]]
   }
   stage('GammaBinding') {
      checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'GammaBinding']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/GammaBinding.git']]]
   }
   stage('My-FyiReporting') {
      checkout changelog: false, scm: [$class: 'GitSCM', branches: [[name: '*/QSBuild']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'My-FyiReporting']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/My-FyiReporting.git']]]
      sh 'nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
   }
   stage('Build') {
        sh 'rm -f Workwear/WinInstall/workwear-*.exe'
        sh 'Workwear/WinInstall/makeWinInstall.sh'
        recordIssues enabledForFailure: true, tool: msBuild()
        archiveArtifacts artifacts: 'Workwear/WinInstall/workwear-*.exe', onlyIfSuccessful: true
   }
   stage('Test'){
       try {
            sh '''
                cd Workwear/WorkwearTest/bin/ReleaseWin
                cp ../../../packages/NUnit.ConsoleRunner.3.11.1/tools/* .
                mono nunit3-console.exe WorkwearTest.dll
            '''
       } catch (e) {}
       finally{
           nunit testResultsPattern: 'Workwear/WorkwearTest/bin/ReleaseWin/TestResult.xml'
       }
   }
   stage('Build Enterprise') {
        sh 'rm -f Workwear/WinInstall/workwear-*.exe'
        sh 'Workwear/WinInstall/makeWinInstall.sh -e'
        // recordIssues enabledForFailure: true, tool: msBuild()
        archiveArtifacts artifacts: 'Workwear/WinInstall/workwear-*.exe', onlyIfSuccessful: true
   }

}
