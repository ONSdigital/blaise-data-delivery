function GenerateUniqueFilename {
    param (
        [string] $prefix,
        [string] $instrumentName,
        [datetime] $dateTime = (Get-Date),
        [string] $fileExt
    )
    
    If ([string]::IsNullOrEmpty($prefix)) {
        throw [System.IO.ArgumentException] "No prefix provided" }

    If ([string]::IsNullOrEmpty($instrumentName)) {
        throw [System.IO.ArgumentException] "No instrument name argument provided" }

    If ([string]::IsNullOrEmpty($fileExt)) {
        throw [System.IO.ArgumentException] "No file extension argument provided" }        

    return "$($prefix)_$($instrumentName)_$($dateTime.ToString("ddMMyyyy"))_$($dateTime.ToString("HHmmss")).$fileExt"            
}



