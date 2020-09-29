function Load-Dependencies(){
    Add-Type -Path (Join-Path -Path (Get-Location).Path -ChildPath '\AssemblyJsSerializer.dll')
}

function ExecuteSerializerOnMVC(){

    $serializer = New-Object 'AssemblyJsSerializer.ObjectMethodsSerializer'
    return $serializer.Serialize()
}

Load-Dependencies
$result = ExecuteSerializerOnMVC
echo $result