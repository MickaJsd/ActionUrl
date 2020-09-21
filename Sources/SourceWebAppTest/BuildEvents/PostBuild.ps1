param ([string] $targetAsssemblyPath = $(throw "Aucune assembly cible n'a été fourni"))

function Load-Dependencies(){
    Add-Type -Path (Join-Path -Path (Get-Location).Path -ChildPath '\AssemblyJsSerializer.dll')
    Add-Type -Path (Join-Path -Path (Get-Location).Path -ChildPath '\System.Web.Mvc.dll')
}

function Load-TargetAssembly([string] $targetAsssemblyPath){
    return [System.Reflection.Assembly]::LoadFile($targetAsssemblyPath);
}

function ExecuteSerializerOnMVC([System.Reflection.Assembly] $targetAsssembly){

    $serializer = New-Object AssemblyJsSerializer.MvcActionSerializer

    $serializeMethod = [AssemblyJsSerializer.MvcActionSerializer].GetMethod("SerializeFormat")
    $genericSerializeMethod = $serializeMethod.MakeGenericMethod([System.Web.Mvc.Controller], [System.Web.Mvc.ActionResult])
    return $genericSerializeMethod.Invoke($serializer, @($targetAsssembly, '()=>_getUrl("{0}","{1}")'))
}

Load-Dependencies
$targetAsssembly = Load-TargetAssembly -targetAsssemblyPath $targetAsssemblyPath
$result = ExecuteSerializerOnMVC -targetAsssembly $targetAsssembly
echo $result