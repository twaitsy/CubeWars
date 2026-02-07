#NoEnv
SetWorkingDir, %A_ScriptDir%

scriptsFolder := "C:\Users\twait\CubeWars\Assets\Scripts"
outputFolder  := scriptsFolder . "\text_scripts"
fileList      := outputFolder . "\generated_files.txt"

; Create output folder if it doesn't exist
IfNotExist, %outputFolder%
    FileCreateDir, %outputFolder%

; Delete old generated_files.txt if it exists
If FileExist(fileList)
    FileDelete, %fileList%

; Loop through all .cs files recursively
Loop, Files, %scriptsFolder%\*.cs, R
{
    csFile := A_LoopFileFullPath

    ; Extract parts
    SplitPath, csFile, fileName, fileDir, fileExt, nameNoExt

    ; Build relative path (folder structure under Scripts)
    StringTrimLeft, relPath, fileDir, StrLen(scriptsFolder)

    ; Build matching output folder
    targetFolder := outputFolder . relPath

    ; Create subfolder if needed
    IfNotExist, %targetFolder%
        FileCreateDir, %targetFolder%

    ; Build output .txt file path
    txtFile := targetFolder . "\" . nameNoExt . ".txt"

    ; Delete old .txt file if it exists
    If FileExist(txtFile)
        FileDelete, %txtFile%

    ; Read the .cs file
    FileRead, fileContent, %csFile%

    ; Write to the new .txt file
    FileAppend, %fileContent%, %txtFile%

    ; Add relative path to generated_files.txt
    FileAppend, % relPath "\" nameNoExt ".txt`r`n", %fileList%
}

MsgBox, All text files created successfully!`nOutput folder:`n%outputFolder%