// Groovy Script: http://www.groovy-lang.org/syntax.html
// Jenkins DSL: https://github.com/jenkinsci/job-dsl-plugin/wiki

import jobs.generation.Utilities;

// The input project name (e.g. dotnet/corefx)
def projectName = GithubProject
// The input branch name (e.g. master)
def branchName = GithubBranchName
// Folder that the project jobs reside in (project/branch)
def projectFoldername = Utilities.getFolderName(projectName) + '/' + Utilities.getFolderName(branchName)

// Email the results of aborted / failed jobs to our infrastructure alias
static void addEmailPublisher(def myJob) {
  myJob.with {
    publishers {
      extendedEmail('mlinfraswat@microsoft.com', '$DEFAULT_SUBJECT', '$DEFAULT_CONTENT') {
	// trigger(trigger name, subject, body, recipient list, send to developers, send to requester, include culprits, send to recipient list)
        trigger('Aborted', '$PROJECT_DEFAULT_SUBJECT', '$PROJECT_DEFAULT_CONTENT', null, false, false, false, true)
        trigger('Failure', '$PROJECT_DEFAULT_SUBJECT', '$PROJECT_DEFAULT_CONTENT', null, false, false, false, true)
      }
    }
  }
}

// Calls a web hook on Jenkins build events.  Allows our build monitoring jobs to be push notified
// vs. polling
static void addBuildEventWebHook(def myJob) {
  myJob.with {
    notifications {
      endpoint('https://jaredpar.azurewebsites.net/api/BuildEvent?code=tts2pvyelahoiliwu7lo6flxr8ps9kaip4hyr4m0ofa3o3l3di77tzcdpk22kf9gex5m6cbrcnmi') {
        event('all')
      }
    }
  }   
}

// Generates the standard trigger phrases.  This is the regex which ends up matching lines like:
//  test win32 please
static String generateTriggerPhrase(String jobName, String opsysName, String triggerKeyword = 'this') {
    return "(?i).*test\\W+(${jobName.replace('_', '/').substring(7)}|${opsysName}|${triggerKeyword}|${opsysName}\\W+${triggerKeyword}|${triggerKeyword}\\W+${opsysName})\\W+please.*";
}

static void addRoslynJob(def myJob, String jobName, String branchName, String triggerPhrase, Boolean triggerPhraseOnly = false) {
  def includePattern = "Binaries/**/*.pdb,Binaries/**/*.xml,Binaries/**/*.log,Binaries/**/*.dmp,Binaries/**/*.zip,Binaries/**/*.png,Binaries/**/*.xml"
  def excludePattern = "Binaries/Obj/**,Binaries/Bootstrap/**,Binaries/**/nuget*.zip"
  Utilities.addArchival(myJob, includePattern, excludePattern)

  // Create the standard job.  This will setup parameter, SCM, timeout, etc ...
  def projectName = 'dotnet/roslyn'
  def isPr = branchName == 'prtest'
  def defaultBranch = "*/${branchName}"
  Utilities.standardJobSetup(myJob, projectName, isPr, defaultBranch)

  // Need to setup the triggers for the job
  if (isPr) {
    def contextName = jobName.replace('_', '/').substring(7)
    Utilities.addGithubPRTrigger(myJob, contextName, triggerPhrase, triggerPhraseOnly)
  } else {
    Utilities.addGithubPushTrigger(myJob)
    addEmailPublisher(myJob)
  }

  addBuildEventWebHook(myJob)
}

// True when this is a PR job, false for commit
def commitPullList;
commitPullList = [false, true]

// Windows     
commitPullList.each { isPr -> 
  ['dbg', 'rel'].each { configuration ->
    ['unit32', 'unit64'].each { buildTarget ->
      def jobName = Utilities.getFullJobName(projectName, "win_${configuration}_${buildTarget}", isPr)
      def myJob = job(jobName) {
        description("Windows ${configuration} tests on ${buildTarget}")
        steps {
          batchFile("""set TEMP=%WORKSPACE%\\Binaries\\Temp
mkdir %TEMP%
set TMP=%TEMP%
.\\cibuild.cmd ${(configuration == 'dbg') ? '/debug' : '/release'} ${(buildTarget == 'unit32') ? '/test32' : '/test64'}""")
        }
      }

      def triggerPhraseOnly = configuration == 'rel'   
      def triggerPhrase = "DO NOT CHECK IN"
      Utilities.setMachineAffinity(myJob, 'Windows_NT', 'latest-or-auto')
      Utilities.addXUnitDotNETResults(myJob, '**/xUnitResults/*.xml')
      addRoslynJob(myJob, jobName, branchName, triggerPhrase, triggerPhraseOnly)
    }
  }
}

// Linux
commitPullList.each { isPr -> 
  def jobName = Utilities.getFullJobName(projectName, "lin_dbg", isPr)
  def myJob = job(jobName) {
    description("Linux tests")
    steps {
      shell("./cibuild.sh --nocache --debug")
    }
  }

  def triggerPhraseOnly = false
  def triggerPhrase = "DO NOT CHECK IN"
  Utilities.setMachineAffinity(myJob, 'Ubuntu14.04', 'latest-or-auto')
  Utilities.addXUnitDotNETResults(myJob, '**/xUnitResults/*.xml')
  addRoslynJob(myJob, jobName, branchName, triggerPhrase, triggerPhraseOnly)
}

// Mac
commitPullList.each { isPr -> 
  def jobName = Utilities.getFullJobName(projectName, "mac_dbg", isPr)
  def myJob = job(jobName) {
    description("Mac tests")
    label('mac-roslyn')
    steps {
      shell("./cibuild.sh --nocache --debug")
    }
  }

  def triggerPhraseOnly = true
  def triggerPhrase = "DO NOT CHECK IN"
  Utilities.addXUnitDotNETResults(myJob, '**/xUnitResults/*.xml')
  addRoslynJob(myJob, jobName, branchName, triggerPhrase, triggerPhraseOnly)
}

// Determinism
commitPullList.each { isPr -> 
  def jobName = Utilities.getFullJobName(projectName, "determinism", isPr)
  def myJob = job(jobName) {
    description('Determinism tests')
    label('windows-roslyn')
    steps {
      batchFile("""set TEMP=%WORKSPACE%\\Binaries\\Temp
mkdir %TEMP%
set TMP=%TEMP%
.\\cibuild.cmd /testDeterminism""")
    }
  }
 
  def triggerPhraseOnly = true
  def triggerPhrase = "DO NOT CHECK IN"
  Utilities.setMachineAffinity(myJob, 'Windows_NT', 'latest-or-auto')
  addRoslynJob(myJob, jobName, branchName, triggerPhrase, triggerPhraseOnly)
}
