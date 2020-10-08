#addin nuget:?package=Cake.Figlet&version=1.3.1
#addin "Cake.DocFx"
#tool "docfx.console"

// Default MSBuild configuration arguments
var configuration = Argument("configuration", "Release");

Task("Clean")
.Does(() =>
{
    CleanDirectory("./artifacts/");

    GetDirectories("./**/bin")
        .ToList()
        .ForEach(d => CleanDirectory(d));

    GetDirectories("./**/obj")
        .ToList()
        .ForEach(d => CleanDirectory(d));
});

Task("Restore")
.Does(() => 
{
    DotNetCoreRestore("./Okta.Auth.Sdk.sln");
});

Task("Build")
.IsDependentOn("Restore")
.Does(() =>
{
    var projects = GetFiles("./**/*.csproj");
    Console.WriteLine("Building {0} projects", projects.Count());

    foreach (var project in projects)
    {
        Console.WriteLine("Building project ", project.GetFilenameWithoutExtension());
        DotNetCoreBuild(project.FullPath, new DotNetCoreBuildSettings
        {
            Configuration = configuration
        });
    }
});

Task("Test")
.IsDependentOn("Restore")
.IsDependentOn("Build")
.Does(() =>
{
    var testProjects = new[] { "Okta.Auth.Sdk.UnitTests" };
    // For now, we won't run integration tests in CI

    foreach (var name in testProjects)
    {
        DotNetCoreTest(string.Format("./{0}/{0}.csproj", name));
    }
});
/*Task("IntegrationTest")
.IsDependentOn("Restore")
.IsDependentOn("Build")
.Does(() =>
{
    var testProjects = new[] { "Okta.Auth.Sdk.IntegrationTests" };
    // Run integration tests in nightly travis cron job

    foreach (var name in testProjects)
    {
        DotNetCoreTest(string.Format("./{0}/{0}.csproj", name));
    }
});*/
Task("Pack")
.IsDependentOn("Test")
//.IsDependentOn("IntegrationTest")
.Does(() =>
{
	var projects = new List<string>()
	{
		"Okta.Sdk.Abstractions",
		"Okta.Auth.Sdk"
	};
	
	projects
    .ForEach(name =>
    {
        Console.WriteLine($"\nCreating NuGet package for {name}");
        
		DotNetCorePack($"./{name}", new DotNetCorePackSettings
		{
			Configuration = configuration,
			OutputDirectory = "./artifacts",
		});
    });
	
});

Task("Info")
.Does(() => 
{
    Information(Figlet("Okta.Auth.Sdk"));

    var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();

    Information("Building using {0} version of Cake", cakeVersion);
});

// Define top-level tasks
Task("Default")
    .IsDependentOn("Info")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

Task("BuildDocs")
.IsDependentOn("Build")
.Does(() =>
{
    // FilePath artifactLocation = File("./src/Okta.Sdk/bin/Release/netstandard1.3/Okta.Sdk.dll");
    // DocFxMetadata(new DocFxMetadataSettings
    // {
    //     OutputPath = MakeAbsolute(Directory("./docs/api/")),
    //     Projects = new[] { artifactLocation }
    // });

    DocFxBuild("./docs/docfx.json");
 //   Outputs to docs/_site
});

Task("CopyDocsToVersionedDirectories")
.IsDependentOn("BuildDocs")
.IsDependentOn("CloneExistingDocs")
.Does(() =>
{
    DeleteDirectory("./docs/temp/latest", recursive: true);
    Information("Copying docs to docs/temp/latest");
    CopyDirectory("./docs/_site/", "./docs/temp/latest/");

    var travisTag = EnvironmentVariable("TRAVIS_TAG");
    if (string.IsNullOrEmpty(travisTag))
    {
        Console.WriteLine("TRAVIS_TAG not set, won't copy docs to a tagged directory");
        return;
    }

    var taggedVersion = travisTag.TrimStart('v');
    var tagDocsDirectory = string.Format("./docs/temp/{0}", taggedVersion);

    Information("Copying docs to " + tagDocsDirectory);
    CopyDirectory("./docs/_site/", tagDocsDirectory);
});

Task("CloneExistingDocs")
.Does(() =>
{
    var tempDir = "./docs/temp";

    if (DirectoryExists(tempDir))
    {
        // Some git files are read-only, so recursively remove any attributes:
        SetFileAttributes(GetFiles(tempDir + "/**/*.*"), System.IO.FileAttributes.Normal);

        DeleteDirectory(tempDir, recursive: true);
    }

    GitClone("https://github.com/okta/okta-auth-dotnet.git",
            tempDir,
            new GitCloneSettings
            {
                BranchName = "gh-pages",
            });
});

Task("Docs")
    .IsDependentOn("BuildDocs")
    .IsDependentOn("CloneExistingDocs")
    .IsDependentOn("CopyDocsToVersionedDirectories");


// Default task
var target = Argument("target", "Default");
RunTarget(target);
