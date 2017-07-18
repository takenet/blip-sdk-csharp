param($installPath, $toolsPath, $package, $project)

$hostFile = "bliphost.exe"
$projectPath = [String](Split-Path -Path $project.FullName);

function ModifyItems
{
    $hostPath = Join-Path (Join-Path $installPath "tools\net461\") $hostFile

    $project.ProjectItems | ForEach-Object {
        if ($_.FileNames(0).EndsWith($hostFile)) { 
            $_.Remove()
            $project.ProjectItems.AddFromFile($hostPath) | out-null
            $project.ProjectItems.Item($hostFile).Properties.Item("CopyToOutputDirectory").Value = 2
            $project.Save()
            write-host "Updated reference to $hostPath"
            continue
        }
    }

    $project.Save()

}

function HasStartAction ($item)
{
    foreach ($property in $item.Properties)
    {
       if ($property.Name -eq "StartAction")
       {
           return $true
       }            
    } 

    return $false
}

function ModifyConfigurations
{
    $configurationManager = $project.ConfigurationManager

    foreach ($name in $configurationManager.ConfigurationRowNames)
    {
        $projectConfigurations = $configurationManager.ConfigurationRow($name)

        foreach ($projectConfiguration in $projectConfigurations)
        {                

            if (HasStartAction $projectConfiguration)
            {
                $newStartAction = 1
                [String]$newStartProgram = Join-Path (Join-Path $projectPath $projectConfiguration.Properties.Item("OutputPath").Value) $hostFile
                #Write-Host "Changing project start action to " $newStartAction
                Write-Host "Changing project start program to " $newStartProgram                
                $projectConfiguration.Properties.Item("StartAction").Value = $newStartAction
                $projectConfiguration.Properties.Item("StartProgram").Value = $newStartProgram                
            }
        }
    }

    $project.Save
}

write-host "Trying to update mhh.exe project reference (if one exists)"
ModifyItems

Write-Host "Modifying Configurations..."
ModifyConfigurations


