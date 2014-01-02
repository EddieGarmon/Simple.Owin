# Psake (https://github.com/psake/psake) build script

FormatTaskName "-------- {0} --------"
Framework 4.0x86

Properties {
	# tools
	$script:nuget = $psake.build_script_dir + "\..\NuGet\NuGet.exe"
	# inputs/outputs
	$script:packageOutput = $psake.build_script_dir + "\..\Artifacts\Packages"
}

Task default -depends ?

Task ? -description "Helper to display task list (and should be automated)" {
	'Supported tasks are: '
	'  Upload - Push Simple.Owin packages to NuGet.org.'
	''
}

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
