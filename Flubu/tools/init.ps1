param($installPath, $toolsPath, $package, $project)

$projectFullName = $project.FullName 
$fileInfo = new-object -typename System.IO.FileInfo -ArgumentList $projectFullName
$projectDirectory = $fileInfo.DirectoryName

$tempDirectory = "temp"
$sourceDirectory = "$projectDirectory\$tempDirectory"
$destinationDirectory = (get-item $sourceDirectory).parent.FullName

if(test-path $sourceDirectory -pathtype container)
{
 robocopy $sourceDirectory $destinationDirectory /XO

 $tempDirectoryProjectItem = $project.ProjectItems.Item($tempDirectory)
 $tempDirectoryProjectItem.Remove()

 remove-item $sourceDirectory -recurse
}