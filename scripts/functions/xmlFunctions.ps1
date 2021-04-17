function GenerateXML{
    param(
        [string] $tempFolder,
        [string] $deliveryFolder,
        [string] $deliveryFile,
        [string] $instrumentName
    )
# Generate XML
& cmd.exe /c $tempFolder\Manipula.exe "$tempFolder\GenerateXML.msux" -A:True -Q:True -K:OPXMeta="$tempFolder/$instrumentName.bmix" -I:$tempFolder/$instrumentName.bdbx -O:$deliveryFolder/$instrumentName.xml
}
