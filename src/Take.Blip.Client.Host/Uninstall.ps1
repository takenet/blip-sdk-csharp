param($installPath, $toolsPath, $package, $project)

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
                $newStartAction = 0
                [String]$newStartProgram = ""
                Write-Host "Changing project start action to " $newStartAction
                Write-Host "Changing project start program to " $newStartProgram                
                $projectConfiguration.Properties.Item("StartAction").Value = $newStartAction
                $projectConfiguration.Properties.Item("StartProgram").Value = $newStartProgram                
            }
        }
    }

    $project.Save
}


Write-Host "Modifying Configurations..."
ModifyConfigurations
