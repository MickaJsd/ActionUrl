param ([string] $sourceAssemblyPath = $(throw "Aucune assembly source n'a été fourni"),
       [string] $targetFile = $(throw "Aucun fichier cible n'a été fourni"))

function Load-Dependencies(){
    Add-Type -Path (Join-Path -Path (Get-Location).Path -ChildPath '\AssemblyJsSerializer.dll')
    Add-Type -Path (Join-Path -Path (Get-Location).Path -ChildPath '\System.Web.Mvc.dll')
}

function Load-Assembly([string] $sourceAssemblyPath){
    return [System.Reflection.Assembly]::LoadFile($sourceAssemblyPath);
}

function ExecuteSerializerOnMVC([System.Reflection.Assembly] $sourceAssembly, [string] $targetFile){

    $serializer = New-Object 'AssemblyJsSerializer.MvcActionSerializer[System.Web.Mvc.Controller,System.Web.Mvc.ActionResult]' $sourceAssembly
    $serializer.FieldFormat = '()=>_getUrl("{0}","{1}")'
    return $serializer.SerializeToFile($targetFile)
}

Load-Dependencies
$sourceAssembly = Load-Assembly -sourceAssemblyPath $sourceAssemblyPath
$result = ExecuteSerializerOnMVC -sourceAssembly $sourceAssembly -targetFile $targetFile
echo $result