# Psake (https://github.com/psake/psake) build script

framework 4.0x86

properties {	
	$script:sourcePath = $psake.build_script_dir + "\..\..\Source\";
	$script:nuget = $psake.build_script_dir + "\..\NuGet\NuGet.exe";
	$script:xunit = $psake.build_script_dir + "\..\XUnit\xunit.console.clr4.exe";
	$script:xunit_x86 = $psake.build_script_dir + "\..\XUnit\xunit.console.clr4.x86.exe";
	$script:test_x86 = (!($(gwmi win32_processor | select description) -match "x86"));
	$script:newPackagesPath = $psake.build_script_dir + '\..\Artifacts\Packages';
	$script:rollbackItems = @{};
}

task default -depends ?

task ? -Description "Helper to display task info" {
	'Supported tasks are: PackageRestore, Clean, Build, Rebuild, Test, Package, and Release'
}

task PackageRestore {
	Write-Host '>>> Restoring packages';
		
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -eq "packages.config") } | 
		ForEach-Object { 
			Write-Host "> " $_.FullName
			exec { & $nuget install $_.FullName -Verbosity detailed -NonInteractive }
		}
}

task Clean {
	Write-Host '>>> Cleaning bin and obj directories';

	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { ($_.PsIsContainer) } |
		Where-Object { ($_.Name -eq "obj") -or ($_.Name -eq "bin") } | 
		ForEach-Object { 
			Write-Host "> " $_.FullName
			Remove-Item -LiteralPath $_.FullName -Recurse -Force
		}
}

task Build -depends PackageRestore {
	Write-Host '>>> Building assemblies';

	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.csproj") } | 
		ForEach-Object { 
			Write-Host "> " $_.FullName
			exec { msbuild /nologo /v:m /p:Configuration=Release /t:Build $_.FullName }
		}
}

task Rebuild -depends Clean, Build

task Test -depends Build { 
	Write-Host '>>> Running tests';
	
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.FullName -like "*\bin\Release\*.Tests.dll") } | 
		ForEach-Object { 
			Write-Host "> " $_.FullName
			exec { & $xunit $_.FullName /silent }
			if ($test_x86) {
				exec { & $xunit_x86 $_.FullName /silent }
			}
		}
}

task SetReleaseProperties {
	Write-Host '>>> Setting assembly and nuspec properties for release.';

	# need 2 versions, semVer and assemblyVer
	$semVer = [System.IO.File]::ReadAllText("semver.txt");
	# todo: support trimming prerelease flag
	$assmVer = "$semVer.0";
	Write-Host ">>>    Using SemVer: $semVer and AssemblyVer: $assmVer";
	
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "AssemblyInfo.cs") } | 
		ForEach-Object { 
			Write-Host "> " $_.FullName
			$originalcontent = [System.IO.File]::ReadAllText($_.FullName);
			$content = $originalcontent.Replace("0.0.0.0", $assmVer);
			$content = $content.Replace("0.0.0-sv", $semVer);
			if ($content -ne $originalcontent) {
				$rollbackItems.Add($_.FullName, $originalcontent);
				[System.IO.File]::WriteAllText($_.FullName, $content);
			}
		}
		
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.nuspec") } | 
		ForEach-Object { 
			Write-Host "> " $_.FullName
			$originalcontent = [System.IO.File]::ReadAllText($_.FullName);
			$content = $originalcontent.Replace("0.0.0-sv", $semVer);
			if ($content -ne $originalcontent) {
				$rollbackItems.Add($_.FullName, $originalcontent);
				[System.IO.File]::WriteAllText($_.FullName, $content);
			}
		}
}

task Package -depends Build {
	Write-Host '>>> Building packages';

	if (!(Test-Path $newPackagesPath)) { mkdir $newPackagesPath }
	
	Get-ChildItem ($sourcePath) -Recurse | 
		Where-Object { (!$_.PsIsContainer) } |
		Where-Object { ($_.Name -like "*.nuspec") } | 
		ForEach-Object { 
			Write-Host "> " $_.FullName
			exec { & $nuget pack ($_.FullName) -OutputDirectory $newPackagesPath -Verbosity detailed -NonInteractive }
		}
}

task RollbackReleaseProperties {
	Write-Host '>>> Rolling back assembly and nuspec properties for release.';
	
	foreach	($file in $rollbackItems.Keys) {
		$content = $rollbackItems[$file];
		[System.IO.File]::WriteAllText($file, $content);
	}
	$rollbackItems = @{};
}

task Release -depends Clean, SetReleaseProperties, Test, Package, RollbackReleaseProperties
