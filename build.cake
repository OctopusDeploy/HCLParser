//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#module nuget:?package=Cake.DotNetTool.Module&version=0.4.0
#tool "dotnet:?package=GitVersion.Tool&version=5.3.5"

using Path = System.IO.Path;
using IO = System.IO;
using Cake.Common.Tools;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var publishDir = "./publish";
var localPackagesDir = "../LocalPackages";
var artifactsDir = "./artifacts";
var assetDir = "./BuildAssets";
var globalAssemblyFile = "./source/Solution Items/VersionInfo.cs";
var solutionToBuild = "./source/Octopus.CoreParsers.Hcl.sln";
var directoryToPublish = "./source/Octopus.Core.Parsers.Hcl/bin/Release";
var cleanups = new List<IDisposable>(); 


var gitVersionInfo = GitVersion(new GitVersionSettings {
    OutputType = GitVersionOutput.Json
});

var nugetVersion = gitVersionInfo.NuGetVersion;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
    if(BuildSystem.IsRunningOnTeamCity)
        BuildSystem.TeamCity.SetBuildNumber(gitVersionInfo.NuGetVersion);
    if(BuildSystem.IsRunningOnAppVeyor)
        AppVeyor.UpdateBuildVersion(gitVersionInfo.NuGetVersion);
    Information("Building Octopus.CoreParsers.Hcl v{0}", nugetVersion);
});

Teardown(context =>
{
    Information("Cleaning up");
    foreach(var item in cleanups)
        item.Dispose();

    Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("__Default")
    .IsDependentOn("__Clean")
    .IsDependentOn("__Restore")
    .IsDependentOn("__UpdateAssemblyVersionInformation")
    .IsDependentOn("__Build")
    .IsDependentOn("__Pack")
	.IsDependentOn("__CopyToLocalPackages");

Task("__Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    CleanDirectory(publishDir);
    CleanDirectories("./source/**/bin");
    CleanDirectories("./source/**/obj");
});

Task("__Restore")
    .Does(() => NuGetRestore(solutionToBuild));
	
Task("__UpdateAssemblyVersionInformation")
    .Does(() =>
{
    cleanups.Add(new AutoRestoreFile(globalAssemblyFile));

	GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = globalAssemblyFile
    });

    Information("AssemblyVersion -> {0}", gitVersionInfo.AssemblySemVer);
    Information("AssemblyFileVersion -> {0}", $"{gitVersionInfo.MajorMinorPatch}.0");
    Information("AssemblyInformationalVersion -> {0}", gitVersionInfo.InformationalVersion);
});

Task("__Build")
    .IsDependentOn("__UpdateAssemblyVersionInformation")
    .Does(() =>
{
    DotNetBuild(solutionToBuild, settings => settings.SetConfiguration(configuration));
});

Task("__Test")
    .IsDependentOn("__Build")
    .Does(() =>
    {
        DotNetCoreTest("source", new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoBuild = true
        });
    });

Task("__Pack")
    .IsDependentOn("__Test")
    .Does(() => {
        var nugetPackDir = Path.Combine(publishDir, "nuget");
        var nuspecFile = "Octopus.CoreParsers.Hcl.nuspec";
        
		CreateDirectory(nugetPackDir);
        CopyFileToDirectory(Path.Combine(assetDir, nuspecFile), nugetPackDir);
		CopyDirectory(directoryToPublish, nugetPackDir);

        NuGetPack(Path.Combine(nugetPackDir, nuspecFile), new NuGetPackSettings {
            Version = nugetVersion,
            OutputDirectory = artifactsDir
        });
    });

Task("__CopyToLocalPackages")
    .WithCriteria(BuildSystem.IsLocalBuild)
    .IsDependentOn("__Pack")
    .Does(() =>
{
    CreateDirectory(localPackagesDir);
    CopyFileToDirectory(Path.Combine(artifactsDir, $"Octopus.CoreParsers.Hcl.{nugetVersion}.nupkg"), localPackagesDir);
});

private class AutoRestoreFile : IDisposable
{
	private byte[] _contents;
	private string _filename;
	public AutoRestoreFile(string filename)
	{
		_filename = filename;
		_contents = IO.File.ReadAllBytes(filename);
	}

	public void Dispose() => IO.File.WriteAllBytes(_filename, _contents);
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("__Default");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);
