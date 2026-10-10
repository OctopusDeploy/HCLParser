using Nuke.Common;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.OctoVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    [Parameter(
        "Whether to auto-detect the branch name - this is okay for a local build, but should not be used under CI.")]
    readonly bool AutoDetectBranch = IsLocalBuild;

    [Parameter(
        "Branch name for OctoVersion to use to calculate the version number. Can be set via the environment variable OCTOVERSION_CurrentBranch.",
        Name = "OCTOVERSION_CurrentBranch")]
    readonly string BranchName;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [OctoVersion(UpdateBuildNumber = true, BranchMember = nameof(BranchName),
        AutoDetectBranchMember = nameof(AutoDetectBranch), Framework = "net10.0")]
    readonly OctoVersionInfo OctoVersionInfo;

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "source";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath LocalPackagesDirectory => RootDirectory / ".." / "LocalPackages";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/TestResults").ForEach(d => d.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information("Building Octopus.CoreParsers.Hcl v{Version}", OctoVersionInfo.FullSemVer);

            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(OctoVersionInfo.FullSemVer)
                .SetInformationalVersion(OctoVersionInfo.InformationalVersion)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetLoggers("trx")
                .SetVerbosity(DotNetVerbosity.normal)
                .EnableNoBuild()
                .EnableNoRestore());

            // TeamCity reads test results from the published artifacts.
            SourceDirectory.GlobFiles("**/*.trx")
                .ForEach(f => f.CopyToDirectory(ArtifactsDirectory, ExistsPolicy.FileOverwrite));
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableNoBuild()
                .AddProperty("Version", OctoVersionInfo.FullSemVer));

            TeamCity.Instance?.PublishArtifacts(ArtifactsDirectory /
                                                $"Octopus.CoreParsers.Hcl.{OctoVersionInfo.FullSemVer}.nupkg");
        });

    Target CopyToLocalPackages => _ => _
        .OnlyWhenStatic(() => IsLocalBuild)
        .TriggeredBy(Pack)
        .Executes(() =>
        {
            LocalPackagesDirectory.CreateDirectory();
            (ArtifactsDirectory / $"Octopus.CoreParsers.Hcl.{OctoVersionInfo.FullSemVer}.nupkg")
                .CopyToDirectory(LocalPackagesDirectory, ExistsPolicy.FileOverwrite);
        });

    public static int Main() => Execute<Build>(x => x.Pack);
}