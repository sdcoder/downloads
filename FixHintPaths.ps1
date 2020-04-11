$hintPathPattern = @"
<HintPath>(\d|\w|\s|\.|\\)*packages
"@


$folders = get-childitem -path "c:\code\tfs\source " | where-object {$_.Psiscontainer -eq "True"} |select-object name
foreach ($folder in $folders)
{
    Set-Location ("c:\code\tfs\Source\" + $folder.Name)

    ls -Recurse -include *.csproj, *.sln, *.fsproj, *.vbproj |
    foreach {
      $content = cat $_.FullName | Out-String
      $origContent = $content
      $content = $content -replace $hintPathPattern, "<HintPath>`$(SolutionDir)packages"
      if ($origContent -ne $content)
      {	
          tf checkout $_.FullName
          $content | out-file -encoding "UTF8" $_.FullName
          write-host messed with $_.Name
      }		    
  }    


}


