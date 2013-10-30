# Psake (https://github.com/psake/psake) build script

FormatTaskName "-------- {0} --------"
Framework 4.0x86
Properties {	
	$script:sourcePath = $psake.build_script_dir + "\..\..\Source\"
	$script:nuget = $psake.build_script_dir + "\..\NuGet\NuGet.exe"
	$script:xunit = $psake.build_script_dir + "\..\XUnit\xunit.console.clr4.exe"
	$script:xunit_x86 = $psake.build_script_dir + "\..\XUnit\xunit.console.clr4.x86.exe"
	$script:test_x86 = (!($(gwmi win32_processor | select description) -match "x86"))
	$script:packageStage = $psake.build_script_dir + '\..\Artifacts\Stage'
	$script:newPackagesPath = $psake.build_script_dir + '\..\Artifacts\Packages'
}

Task default -depends ?

Task ? -description "Helper to display task info" {
	'Supported tasks are: PackageRestore, Clean, Build, Rebuild, Test, Package, and Release'
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
			if ($test_x86) {
				Exec { & $xunit_x86 $_.FullName /silent }
			}
		}
}

Task PackageBinaries -depends Build {
	if (!(Test-Path $newPackagesPath)) { 
		mkdir $newPackagesPath | Out-Null
	}
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.nuspec") } | 
		ForEach-Object { 
			Write-Host "Package Binary: " $_.FullName
			Exec { & $nuget pack ($_.FullName) -OutputDirectory $newPackagesPath -Verbosity detailed -NonInteractive }
			Write-Host ""
		}
}

Task PackageSources { #-depends Build {
	[System.Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null
	if (!(Test-Path $newPackagesPath)) { 
		mkdir $newPackagesPath | Out-Null
	}
	if (Test-Path $packageStage) {
		Remove-Item -Recurse -Force $packageStage | Out-Null
	}
	mkdir $packageStage | Out-Null
				
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
			$metadata.Element("id").Value = $packageId + ".Sources"
			$depends = $metadata.Element("dependencies");
			if ($depends) {
				foreach ($dependency in $depends.Elements("dependency")) {
					$dependencyId = $dependency.Attribute("id")
					if ($dependencyId.Value.StartsWith("Simple.Owin")) {
						$dependencyId.Value += ".Sources"
					}
				}
			}
			$files = $package.Element("files")
			if ($files) {
				$files.Remove()
			}
			$sourceStage = $packageStage + "\" + $packageId
			mkdir $sourceStage | Out-Null
			
			$frameworks = @(
				@{
					"SourceDir" = [System.IO.Path]::GetFullPath("$sourcePath$packageId\Full-4.0\")
					"Project" = [System.IO.Path]::GetFullPath("$sourcePath$packageId\Full-4.0\$PackageId [F40].csproj")
					"StageDir" = [System.IO.Path]::GetFullPath("$sourceStage\content\net40\App_Packages\$packageId.$version\")
				},
				@{
					"SourceDir" = [System.IO.Path]::GetFullPath("$sourcePath$packageId\Full-4.5\")
					"Project" = [System.IO.Path]::GetFullPath("$sourcePath$packageId\Full-4.5\$PackageId [F45].csproj")
					"StageDir" = [System.IO.Path]::GetFullPath("$sourceStage\content\net45\App_Packages\$packageId.$version\")
				}
			)
			foreach ($framework in $frameworks) {
				if (!(Test-Path $framework.SourceDir)) {
					continue
				}
				$project = [System.Xml.Linq.XElement]::Load($framework.Project);
				foreach ($compile in $project.Descendants($compileName)) {
					$from = $compile.Attribute("Include").Value
					if ($from.EndsWith("AssemblyInfo.cs")) {
						continue
					}
					$content = [System.IO.File]::ReadAllText($framework.SourceDir + $from)
					$content = $content.Replace("public class", "internal class")
					$content = $content.Replace("public static class", "internal static class")
					$content = $content.Replace("public interface", "internal interface")
					$content = $content.Replace("public enum", "internal enum")
					$content = $content.Replace("public struct", "internal struct")
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
			Exec { & $nuget pack ($nuspec) -OutputDirectory $newPackagesPath -Verbosity detailed -NonInteractive }
			Write-Host ""
		}
}

Task Package -depends PackageBinaries, PackageSources 

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
