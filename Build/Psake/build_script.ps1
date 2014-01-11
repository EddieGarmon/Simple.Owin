# Psake (https://github.com/psake/psake) build script

FormatTaskName "-------- {0} --------"
Framework 4.0x86

Properties {
	# tools
	$script:nuget = $psake.build_script_dir + "\..\NuGet\NuGet.exe"
	$script:xunit = $psake.build_script_dir + "\..\XUnit\xunit.console.clr4.exe"
	$script:xunit_x86 = $psake.build_script_dir + "\..\XUnit\xunit.console.clr4.x86.exe"
	$script:should_test_x86_also = (!($(gwmi win32_processor | select description) -match "x86"))
	# inputs/outputs
	$script:sourcePath = $psake.build_script_dir + "\..\..\Source\"
	$script:packageStage = $psake.build_script_dir + "\..\Artifacts\Stage"
	$script:packageOutput = $psake.build_script_dir + "\..\Artifacts\Packages"
	# variables
	$script:namespaceToPackageMap = @{
		"Simple.Owin.Helpers" = "Simple.Owin";
		"Simple.Owin.Hosting.Trace" = "Simple.Owin.Hosting";
	}
}

Task default -depends ?

Task ? -description "Helper to display task list (and should be automated)" {
	'Supported tasks are: '
	'  PackageRestore - Fetch missing NuGet packages.'
	'  Clean          - Remove all intermediate and final compiler outputs.'
	'  Build          - Runs all compilers.'
	'  Rebuild        - Cleans, then Builds.'
	'  Test           - Builds, then executes all discoverable xUnit tests.'
	'  Package        - Cleans then sets the stage, Creates NuGet packages.'
	'  Release        - Cleans, Propogates version number, Builds, Tests, Packages'
	''
	'  Upload - *Push Simple.Owin packages to NuGet.org.'
	''
}

Task Clean {
	# Remove bin and obj directories
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { ($_.PsIsContainer) } |
		Where-Object { ($_.Name -eq "obj") -or ($_.Name -eq "bin") } | 
		ForEach-Object { 
			Write-Host "Removing: " $_.FullName
			Remove-Item -LiteralPath $_.FullName -Recurse -Force
		}
	# Remove and prior versioning files
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.orig") } | 
		ForEach-Object { 
			$path = $_.FullName.Substring(0, $_.FullName.Length - 5)
			Write-Host "Restoring: " $path
			[System.IO.File]::Delete($path)
			[System.IO.File]::Move($_.FullName, $path)
		}
}

Task PackageRestore {
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -eq "packages.config") } | 
		ForEach-Object { 
			Write-Host "Restoring From: " $_.FullName
			exec { & $nuget install $_.FullName -Verbosity detailed -NonInteractive }
		}
}

Task Build -depends PackageRestore {
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.csproj") } | 
		ForEach-Object { 
			Write-Host "Building: " $_.FullName
			Exec { msbuild /nologo /v:m /p:Configuration=Release /t:Build $_.FullName }
		}
}

Task Rebuild -depends Clean, Build

Task Test -depends Build { 
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.FullName -like "*\bin\Release\*.Tests.dll") } | 
		ForEach-Object { 
			Write-Host "Test " $_.FullName
			Exec { & $xunit $_.FullName /silent }
			if ($should_test_x86_also) {
				Exec { & $xunit_x86 $_.FullName /silent }
			}
		}
}

Task StagePackages {
	if (Test-Path $packageStage) {
		Remove-Item -Recurse -Force $packageStage | Out-Null
	}
	mkdir $packageStage | Out-Null
	
	[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq")
	$compileName = [System.Xml.Linq.XName]::Get("Compile", "http://schemas.microsoft.com/developer/msbuild/2003")

	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.nuspec") } | 
		ForEach-Object { 
			Write-Host "Package Source: " $_.FullName
			$package = [System.Xml.Linq.XElement]::Load($_.FullName)
			$metadata = $package.Element("metadata")
			$packageId = $metadata.Element("id").Value
			$version = $metadata.Element("version").Value

			$sourceStage = $packageStage + "\" + $packageId
			mkdir $sourceStage | Out-Null

			# Note: nuspec file's name specifies folder path filter for project file contents to include
			$packageName = [System.IO.Path]::GetFileNameWithoutExtension($_.FullName)
			# get the path to the proj folder, then add trailing slashes
			$componentDir = [System.IO.Path]::GetDirectoryName($_.FullName)
			$projectDir = [System.IO.Path]::GetDirectoryName($componentDir)
			$componentDir += "\"
			$projectDir += "\"
			
			$files = $package.Element("files")
			if ($files) {
				$stageToDir = [System.IO.Path]::GetFullPath("$sourceStage")
				#stage files as explicitly defined
				foreach ($file in $files.Elements("file")) {
					$src = $file.Attribute("src").Value;
					$target = $file.Attribute("target").Value;
					$from = "$componentDir$src"
					$to = "$stageToDir\\$target\App_Packages\$packageId.$version\$src"

					$toDir = [System.IO.Path]::GetDirectoryName($to)
					[System.IO.Directory]::CreateDirectory($toDir)
					copy $from $to | Out-Null
				}
				
				#nuke the files element
				$files.Remove();
			}
			else {
				#Dynamic Staging from project files
				$targetPlatforms = @(
					@{ 
						"SearchPattern" = "*.Full-40.csproj"; 
						"ProfilePath" = "net40" 
					},
					@{ 
						"SearchPattern" = "*.Full-45.csproj";
						"ProfilePath" = "net45" 
					}
				)
				
				foreach ($targetPlatform in $targetPlatforms) {
					$files = [System.IO.Directory]::GetFiles($projectDir, $targetPlatform.SearchPattern)
					# need exactly one file (or none) here!!!
					if ($files.Length -eq 0) {
						continue
					}
					if ($files.Length -gt 1) {
						throw "Ambigious projects found."
					}
					$platform = $targetPlatform.ProfilePath
					$stageToHere = [System.IO.Path]::GetFullPath("$sourceStage\content\$platform\App_Packages\$packageId.$version\")

					# we are interested in the intersection of
					#	the files on disk in the same folder as the nuspec file, including child folders
					#	and the files listed in the project file
					$project = [System.Xml.Linq.XElement]::Load($files[0])					
					foreach ($compile in $project.Descendants($compileName)) {
						$itemProjectRelativePath = $compile.Attribute("Include").Value
						if ($itemProjectRelativePath.StartsWith("$packageName\")) {
							$from = "$projectDir$itemProjectRelativePath"
							$to = $stageToHere + $itemProjectRelativePath.Substring($packageName.Length + 1)
							if (![System.IO.File]::Exists($from)) {
								throw "Missing file: $from"
							}
							$toDir = [System.IO.Path]::GetDirectoryName($to)
							[System.IO.Directory]::CreateDirectory($toDir)
							copy $from $to | Out-Null
							# here we need to look at each file to check using statements for any dependencies to add
							$content = [System.IO.File]::ReadAllLines($from, [System.Text.Encoding]::ASCII)
							foreach ($line in $content) {
								if ($line -match "using\W+([\w.]+);") {
									$namespace = $matches[1]
									Write-Host "Found Using:" $namespace
									if ($namespace.StartsWith("System")) {
										continue
									}
									if ($namespace -eq $packageName) {
										continue
									}
									# todo update dependency map here
									# pull out part of the loop below to be more efficient
									$mapped = $namespaceToPackageMap[$namespace]
									if ($mapped -eq $packageName) {
										continue
									}
									# ensure mapping here (todo: by target framework?)
									if ($mapped -eq $null) {
										$mapped = $namespace
									}
									$dependencies = $metadata.Element("dependencies")
									if ($dependencies -eq $null) {
										$dependencies = [System.Xml.Linq.XElement]::Parse("<dependencies />")
										$metadata.Add($dependencies)
									}
									$found = $false
									foreach ($dependency in $dependencies.Descendants("dependency")) {
										if ($dependency.Attribute("id").Value -eq $mapped) {
											$found = $true;
											break;
										}									
									}
									if (!$found) {
										$dependency = [System.Xml.Linq.XElement]::Parse("<dependency id='$mapped' version='[$version]' />")
										$dependencies.Add($dependency)
									}
								}
							}
						}
					}
				}
			}
			
			$nuspec = $sourceStage + "\source.nuspec"
			[System.IO.File]::WriteAllText($nuspec, $package)
			Write-Host "Staged: " $nuspec
		}
}
Task PackageSources {
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.nuspec") } | 
		ForEach-Object { 
			
			foreach ($framework in $frameworks) {
				if (!(Test-Path $framework.SourceDir)) {
					continue
				}
				$project = [System.Xml.Linq.XElement]::Load($framework.Project)
				foreach ($compile in $project.Descendants($compileName)) {
					$from = $compile.Attribute("Include").Value
					if ($from.EndsWith("AssemblyInfo.cs")) {
						continue
					}
					$content = [System.IO.File]::ReadAllText($framework.SourceDir + $from)
					$content = $content.Replace("public class", "internal class")
					$content = $content.Replace("public static class", "internal static class")
					$content = $content.Replace("public abstract class", "internal abstract class")
					$content = $content.Replace("public partial class", "internal partial class")
					$content = $content.Replace("public static partial class", "internal static partial class")
					$content = $content.Replace("public abstract partial class", "internal abstract partial class")
					$content = $content.Replace("public interface", "internal interface")
					$content = $content.Replace("public partial interface", "internal partial interface")
					$content = $content.Replace("public struct", "internal struct")
					$content = $content.Replace("public partial struct", "internal partial struct")
					$content = $content.Replace("public enum", "internal enum")
					$to = $framework.StageDir + $from.Replace("..\", "").Replace("Full-4.0", "")
					$toDir = [System.IO.Path]::GetDirectoryName($to)
					if (!(Test-Path $toDir)) {
						mkdir $toDir | Out-Null
					}
					[System.IO.File]::WriteAllText($to, $content)
				}
			}
			$nuspec = $sourceStage + "\source.nuspec"
			[System.IO.File]::WriteAllText($nuspec, $package)
			Write-Host "Staged: " $nuspec
			Exec { & $nuget pack ($nuspec) -OutputDirectory $packageOutput -Verbosity detailed -NonInteractive }
			Write-Host ""
		}
}

Task BuildPackages {
	if (!(Test-Path $packageOutput)) { 
		mkdir $packageOutput | Out-Null
	}
	Get-ChildItem ($packageStage) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.nuspec") } | 
		ForEach-Object { 
			Write-Host "Create Package: " $_.FullName
			Exec { & $nuget pack ($_.FullName) -OutputDirectory $packageOutput -Verbosity detailed -NonInteractive }
			Write-Host ""
		}
}

Task Package -depends StagePackages, BuildPackages

Task SetVersion {
	$semVerPath = $psake.build_script_dir + "\semver.txt"
	$semVer = [System.IO.File]::ReadAllText($semVerPath)
	# need 2 version numbers, semVer and assemblyVer
	$assmVer = $semVer
	$length = $semVer.IndexOf("-")
	if ($length -gt 0) {
		$assmVer = $semVer.Substring(0, $length)
	}
	$assmVer = $assmVer + ".0"
	Write-Host "Using SemVer: $semVer and AssemblyVer: $assmVer"
	
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "AssemblyInfo.cs") } | 
		ForEach-Object { 
			Write-Host "Updating: " $_.FullName
			$originalcontent = [System.IO.File]::ReadAllText($_.FullName)
			$content = $originalcontent.Replace("0.0.0.0", $assmVer)
			$content = $content.Replace("0.0.0-sv", $semVer)
			if ($content -ne $originalcontent) {
				[System.IO.File]::WriteAllText($_.FullName + ".orig", $originalcontent)
				[System.IO.File]::WriteAllText($_.FullName, $content)
			}
		}
		
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.nuspec") } | 
		ForEach-Object { 
			Write-Host "Updating: " $_.FullName
			$originalcontent = [System.IO.File]::ReadAllText($_.FullName)
			$content = $originalcontent.Replace("0.0.0-sv", $semVer)
			if ($content -ne $originalcontent) {
				[System.IO.File]::WriteAllText($_.FullName + ".orig", $originalcontent)
				[System.IO.File]::WriteAllText($_.FullName, $content)
			}
		}
}

Task ClearVersion {
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.orig") } | 
		ForEach-Object { 
			$path = $_.FullName.Substring(0, $_.FullName.Length - 5)
			Write-Host "Restoring: " $path
			[System.IO.File]::Delete($path)
			[System.IO.File]::Move($_.FullName, $path)
		}
}

Task Release -depends Clean, SetVersion, Test, Package, ClearVersion

Task Upload {
	Get-ChildItem ($packageOutput) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "Simple.Owin*.nupkg") } | 
		ForEach-Object { 
			$path = $_.FullName
			Write-Host "Uploading: " $path
			exec { & $nuget push $_.FullName -Verbosity detailed -NonInteractive }
		}
}
