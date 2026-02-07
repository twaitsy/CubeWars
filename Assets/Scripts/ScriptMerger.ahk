; Merge all .cs files into scriptsmerged.txt (AutoHotkey v1)

scriptsFolder := "C:\Users\twait\CubeWars\Assets\Scripts"
outputFile    := scriptsFolder . "\scriptsmerged.txt"

; Delete old merged file if it exists
If FileExist(outputFile)
    FileDelete, %outputFile%

; Loop through all .cs files in the folder
Loop, Files, %scriptsFolder%\*.cs
{
    currentFile := A_LoopFileFullPath

    ; Add a readable header
    FileAppend, `r`n`r`n==============================`r`n, %outputFile%
    FileAppend, % "FILE: " . A_LoopFileName . "`r`n", %outputFile%
    FileAppend, ==============================`r`n`r`n, %outputFile%

    ; Read and append file contents
    FileRead, fileContent, %currentFile%
    FileAppend, %fileContent%, %outputFile%
}

MsgBox, Scripts merged successfully!`n%outputFile%