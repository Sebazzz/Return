#addin nuget:?package=Cake.Compression&version=0.3.0
#addin nuget:?package=SharpZipLib&version=1.4.1
#addin nuget:?package=Cake.GitVersioning&version=3.5.119
#addin nuget:?package=Cake.Codecov&version=1.0.1
#addin nuget:?package=Cake.Coverlet&version=3.0.4
#tool nuget:?package=Codecov&version=1.13.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosity = Argument<Verbosity>("verbosity", Verbosity.Minimal);
var skipCompression = Argument<bool>("skip-compression", false);
var skipGitVersionDetection = Argument<bool>("skip-git-version-detection", false);
var useCodeCoverage = Argument<bool>("use-code-coverage", false);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var baseName = "Return";
var buildDir = Directory("./build");
var testResultsDir = buildDir + Directory("./testresults");
var testArtifactsDir = buildDir + Directory("./testresults/artifacts");
var publishDir = Directory(Argument("publish-dir", "./build/publish"));
var assemblyInfoFile = Directory($"./src/{baseName}/Properties") + File("AssemblyInfo.cs");
var nodeEnv = configuration == "Release" ? "production" : "development";
var persistenceProjectPath = Directory($"./src/{baseName}.Persistence");
var mainProjectPath = Directory($"./src/{baseName}.Web");

int p = (int) Environment.OSVersion.Platform;
bool isUnix = (p == 4) || (p == 6) || (p == 128);

var cmd = isUnix ? "bash" : "cmd";
var cmdArg = isUnix ? "-c" : "/C";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() => {
    CleanDirectory(buildDir);
	CleanDirectory(publishDir);
	CleanDirectories("./src/**/obj");
});

Task("Clean-TestResults")
    .Does(() => {
    CleanDirectory(testResultsDir);
});

Task("Rebuild")
	.IsDependentOn("Clean")
	.IsDependentOn("Build");
	
void RunCmd(string command) {
	var processSettings = new ProcessSettings()
		.WithArguments(args => args.Append(cmdArg).AppendQuoted(command))
	;
	

	var process = StartAndReturnProcess(cmd, processSettings);
	
	process.WaitForExit();
}
	
void CheckToolVersion(string name, string executable, string argument, Version wantedVersion) {
	try {
		Information($"Checking {name} version...");
		

		var processSettings = new ProcessSettings()
			.WithArguments(args => args.Append(cmdArg).AppendQuoted(executable + " " + argument))
			.SetRedirectStandardOutput(true)
		;
		

		var process = StartAndReturnProcess(cmd, processSettings);
		
		process.WaitForExit();
		
		string line = null;
		foreach (var output in process.GetStandardOutput()) {
			line = output;
			Debug(output);
		}
		
		if (String.IsNullOrEmpty(line)) {
			throw new CakeException("Didn't get any output from " + executable);
		}
	
		Version actualVersion = Version.Parse(line.Trim('v'));
		
		Information("Got version {0} - we want at least version {1}", actualVersion, wantedVersion);
		if (wantedVersion > actualVersion) {
			throw new CakeException($"{name} version {actualVersion} does not satisfy the requirement of {name}>={wantedVersion}");
		}
	} catch (Exception e) when (!(e is CakeException)) {
		throw new CakeException($"Unable to check {name} version. Please check whether {name} is available in the current %PATH%.", e);
	}
}

int StartProjectDirProcess(string processCommandLine) {
	return StartProcess(cmd, new ProcessSettings()
			.UseWorkingDirectory(mainProjectPath)
			.WithArguments(args => args.Append(cmdArg).AppendQuoted(processCommandLine)));
}
	
Task("Check-Node-Version")
	.Does(() => {
	CheckToolVersion("node.js", "node", "--version", new Version(14,0,0));
});

Task("Check-Yarn-Version")
	.Does(() => {
	CheckToolVersion("yarn package manager", "yarn", "--version", new Version(1,16,0) /*Minimum supported on appveyor*/);
});

Task("Restore-NuGet-Packages")
    .Does(() => {
    DotNetRestore(new DotNetRestoreSettings {
		IgnoreFailedSources = true
	});
});

Task("Generate-MigrationScript")
	.Does(() => {
		DotNetTool(
			$"{baseName}.Persistence.csproj", 
			"ef", 
			new ProcessArgumentBuilder()
				.Append("migrations")
				.Append("script")
				.Append("-o ../../build/publish/MigrationScript.sql"), 
			new DotNetToolSettings  {
				WorkingDirectory = persistenceProjectPath, 
				DiagnosticOutput = true
			});
	}
);

Task("Set-NodeEnvironment")
	.Does(() => {
		Information("Setting NODE_ENV to {0}", nodeEnv);
		
		System.Environment.SetEnvironmentVariable("NODE_ENV", nodeEnv);
	});

Task("Restore-Node-Packages")
	.IsDependentOn("Check-Node-Version")
	.IsDependentOn("Check-Yarn-Version")
	.Does(() => {
	
	int exitCode;
	
	Information("Trying to restore packages using yarn");
	
	exitCode = StartProjectDirProcess("yarn --production=false --frozen-lockfile --non-interactive");
		
	if (exitCode != 0) {
		throw new CakeException($"'yarn' returned exit code {exitCode} (0x{exitCode:x2})");
	}
});

Task("Build")
	.IsDependentOn("Set-NodeEnvironment")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Restore-Node-Packages")
    .Does(() => {
        DotNetBuild($"./{baseName}.sln");
});

Task("Run")
    .IsDependentOn("Build")
    .Does(() => {
        DotNetRun($"{baseName}.Web.csproj", null, new DotNetRunSettings { WorkingDirectory = $"./src/{baseName}.Web" });
});

void PublishSelfContained(string platform, string folder) {
	Information("Publishing self-contained for platform {0}", platform);

	var settings = new DotNetPublishSettings
	 {
		 Configuration = configuration,
		 OutputDirectory = publishDir + Directory(folder ?? platform),
		 Runtime = platform,
		 MSBuildSettings = new DotNetMSBuildSettings {
			MaxCpuCount = AppVeyor.IsRunningOnAppVeyor ? (int?) 1 : null
		 }
	 };
	
    DotNetPublish($"./src/{baseName}.Web/{baseName}.Web.csproj", settings);
}

Task("Run-FrontendBuild")
	.IsDependentOn("Restore-Node-Packages")
	.IsDependentOn("Set-NodeEnvironment")
	.Does(() => {
		var exitCode = StartProjectDirProcess("yarn run build");
		
		if (exitCode != 0) {
			throw new CakeException($"'yarn run build' returned exit code {exitCode} (0x{exitCode:x2})");
		}
	});
	
Task("Run-DotnetFormatToolInstall")
	.Does(() => {
	StartProcess("dotnet", "tool install --tool-path .dotnet/ dotnet-format");
});

IEnumerable<string> GetModifiedFilePaths() {
	IEnumerable<string> stdErr, stdOut;
	
	StartProcess(
		"git", 
		new ProcessSettings()
			.UseWorkingDirectory(mainProjectPath)
			.SetRedirectStandardError(true)
			.SetRedirectStandardOutput(true)
			.WithArguments(args => args.Append("status").Append("--porcelain=1")),
		out stdOut,
		out stdErr
	);
	
	foreach (string line in stdOut) {
		string fileName = line.Substring(3);
		
		if (System.IO.Path.GetExtension(fileName) != ".cs") {
			continue;
		}
		
		yield return fileName;
	}
}

Task("Set-HeadlessEnvironment")
	.Does(() => {
		Information("Setting MOZ_HEADLESS to 1");
		
		System.Environment.SetEnvironmentVariable("MOZ_HEADLESS", "1");
	});
	
Task("Run-Precommit-Tasks")
	.Does(() => {
	{
		int exitCode = StartProjectDirProcess("yarn pre-commit--pretty-quick");
		if (exitCode != 0) {
			throw new CakeException($"pretty-quick exited with code {exitCode}");
		}
	}
	
	{
		string filePaths = String.Join(",", GetModifiedFilePaths());
		
		if (String.IsNullOrEmpty(filePaths)) {
			Information("No changed files to reformat");
			return;
		}
		
		int exitCode = StartProcess("dotnet", new ProcessSettings()
			.UseWorkingDirectory(mainProjectPath)
			.WithArguments(args => args.Append("format").Append("--files").AppendQuoted(filePaths)));
			
		if (exitCode != 0) {
			throw new CakeException($"dotnet-format exited with code {exitCode}");
		}
	}
});

Task("Publish-Common")
	.Description("Internal task - do not use")
    .IsDependentOn("Rebuild")
    .IsDependentOn("Generate-MigrationScript")
	.IsDependentOn("Run-FrontendBuild");

string GetVersionString() {
	if (skipGitVersionDetection) return "0.0.0-null";

	var version = GitVersioningGetVersion();
	
	return version.SemVer1;
}

var windowsAllPublishTask = Task("Publish-Windows");

void WindowsPublishTask(string taskId, string versionId, string description) {
	string internalTaskName = $"Publish-Windows-Core-{taskId}";
	string taskName = $"Publish-Windows-{taskId}";

	Task(internalTaskName)
		.Description("Internal task - do not use")
		.IsDependentOn("Publish-Common")
		.Does(() => PublishSelfContained(versionId, $"{versionId}/app"));

	var output = publishDir + File($"return-web-{versionId}-{GetVersionString()}.zip");
	Task(taskName)
		.IsDependentOn(internalTaskName)
		.Description($"Publish for {description}, output to {output}")
		.Does(() => {
		   if (!skipCompression) ZipCompress(publishDir + Directory($"{versionId}/"), output);
		});
	
	windowsAllPublishTask.IsDependentOn(taskName);
}

WindowsPublishTask("10-x64", "win10-x64", "Windows 10 / Windows Server 2016 64-bit");
WindowsPublishTask("8-x64", "win81-x64", "Windows 8.1 / Windows Server 2012 R2 64-bit");

var ubuntuAllPublishTask = Task("Publish-Ubuntu");

void UbuntuPublishTask(string taskId, string versionId, string description) {
	string internalTaskName = $"Publish-Ubuntu-Core-{taskId}";
	string taskName = $"Publish-Ubuntu-{taskId}";

	Task(internalTaskName)
		.Description("Internal task - do not use")
		.IsDependentOn("Publish-Common")
		.Does(() => PublishSelfContained(versionId, $"{versionId}/app"));

	var output = publishDir + File($"return-web-{versionId}-{GetVersionString()}.tar.gz");
	Task(taskName)
		.IsDependentOn(internalTaskName)
		.Description($"Publish for {description}, output to {output}")
		.Does(() => {
		   CopyFile(File("./distscripts/ubuntu/launch"), publishDir + File($"{versionId}/launch"));
		   CopyFile(File("./distscripts/ubuntu/launch.conf"), publishDir + File($"{versionId}/launch.conf.example"));
		   if (!skipCompression) GZipCompress(publishDir + Directory($"{versionId}/"), output);
		});
	
	ubuntuAllPublishTask.IsDependentOn(taskName);
}

UbuntuPublishTask("22.04-x64", "ubuntu.22.04-x64", "Ubuntu 22.04 64-bit");

Task("Publish")
    .IsDependentOn("Publish-Windows")
    .IsDependentOn("Publish-Ubuntu");
	
List<string> codeCoveragePaths = null;
void TestTask(string name, string projectName, Func<bool> criteria = null) {
	CreateDirectory(testResultsDir);
	CreateDirectory(testArtifactsDir);

	criteria = criteria ?? new Func<bool>(() => true);
	
	var logFilePath = MakeAbsolute(testResultsDir + File($"test-{name}-log.trx"));
	var codeCoverageOutputDirectory = testArtifactsDir + Directory($"code-coverage-{projectName}");
	var codeCoverageResultsFileName = $"code-coverage-{projectName}.xml";
	var codeCoverageResultsFile = codeCoverageOutputDirectory + File(codeCoverageResultsFileName);
	
	Task($"Test-CS-{name}")
		.IsDependentOn("Restore-NuGet-Packages")
		.IsDependentOn("Set-HeadlessEnvironment")
		.IsDependentOn("Run-FrontendBuild")
		.IsDependentOn("Clean-TestResults")
		.IsDependeeOf("Test-CS")
		.WithCriteria(criteria)
		.Does(() => {
			Information($"Running tests for {projectName} - logging to {logFilePath} - artifacts dumped to {testArtifactsDir}");

			CreateDirectory(testArtifactsDir);
			CreateDirectory(codeCoverageOutputDirectory);
			
			System.Environment.SetEnvironmentVariable("TEST_ARTIFACT_DIR", MakeAbsolute(testArtifactsDir).ToString());
	
			var testPath = $"./tests/{projectName}/{projectName}.csproj";
			var testSettings = new DotNetTestSettings {
				Configuration = configuration,
				ArgumentCustomization = (args) => args.AppendQuoted($"--logger:trx;LogFileName={logFilePath}")
													  .Append("--logger:\"console;verbosity=normal;noprogress=true\"") 
			};

			// Note: Alias is obsolete, but Cake.CodeCov has not been updated a while
			if (!useCodeCoverage) {
				DotNetTest(testPath, testSettings);
			} else {
				var coverletSettings = new CoverletSettings {
					CollectCoverage = true,
					CoverletOutputFormat = CoverletOutputFormat.opencover,
					CoverletOutputDirectory = codeCoverageOutputDirectory,
					CoverletOutputName = codeCoverageResultsFileName
				}.WithFilter("[Return.*.Tests.*]*")
				 .WithFilter("[Return.Persistence]Return.Persistence.Migrations");

				DotNetTest(testPath, testSettings, coverletSettings);
			}
		})
		.Finally(() => {
			if (AppVeyor.IsRunningOnAppVeyor && FileExists(logFilePath)) {
				var jobId = EnvironmentVariable("APPVEYOR_JOB_ID");
				var resultsType = "mstest"; // trx is vstest format
				
				var url = $"https://ci.appveyor.com/api/testresults/{resultsType}/{jobId}";
				var fullTestResultsPath = logFilePath.FullPath;
				
				Information("Uploading test results from {0} to {1}", fullTestResultsPath, url);
				UploadFile(url, fullTestResultsPath);
			}

			if (useCodeCoverage) {
				if (!FileExists(codeCoverageResultsFile)) {
					Warning($"Code coverage file result not found in path: {codeCoverageOutputDirectory} - expected to file {codeCoverageResultsFile}");
				} else {
					Verbose($"Registering for code coverage upload: {codeCoverageResultsFile}");
					codeCoveragePaths.Add(codeCoverageResultsFile);
				}
			}
		});

	if (codeCoveragePaths == null) {
		Debug("Setup code coverage upload task. Skipping...");

		codeCoveragePaths = new List<string>();
		Teardown(context =>	{
			// Upload all reports consolidated - this prevents half coverage reports from being uploaded
			if (codeCoveragePaths.Count > 0) {
				Information($"Uploading {codeCoveragePaths.Count} code coverage results...");
				Codecov(codeCoveragePaths.ToArray());
			} else if (useCodeCoverage) {
				Warning("No code coverage has been collected.");
			}
		});
	}
}

TestTask("Unit-Application", $"{baseName}.Application.Tests.Unit");
TestTask("Unit-Domain", $"{baseName}.Domain.Tests.Unit");
TestTask("Unit-Web", $"{baseName}.Web.Tests.Unit");
TestTask("Integration-Web", $"{baseName}.Web.Tests.Integration");

Task("Test-PreReq-Playwright-Browser-Deps")
    .Description("Prepare playwright")
	.IsDependentOn("Build")
	.Does(() => {
	DotNetTool(".", "playwright", "install-deps firefox chromium");
});

Task("Test-PreReq-Playwright-Browser")
    .Description("Prepare playwright")
	.IsDependentOn("Test-PreReq-Playwright-Browser-Deps")
	.IsDependeeOf("Test-CS-Integration-Web")
	.Does(() => {
	DotNetTool(".", "playwright", "install firefox chromium");
});

Task("Test-CS")
    .Description("Test backend-end compiled code");

Task("Test")
    .IsDependentOn("Test-CS")
    .Description("Run all tests");

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("None");

Task("Default")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);