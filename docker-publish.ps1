#!/bin/pwsh
<#
        .SYNOPSIS
        Helper script to execute docker buildx

        .DESCRIPTION
        Simplifies executing docker buildx for multi-architecture for the current Dockerfile 

        .PARAMETER Version
        Specifies the version number to take this image with (defaults to 'latest' only)

        .PARAMETER DockerAccount
        The docker account to use to push the image

        .PARAMETER Platforms
        The platforms to target for the image (defaults to linux/arm/v7,linux/arm64/v8,linux/amd64)

        .INPUTS
        None. You cannot pipe objects to to this script.

        .OUTPUTS
        None

        .EXAMPLE
        PS> /docker-publish.ps1        

        .EXAMPLE
        PS> ./docker-publish.ps1 -Version v1.7.4
        
        .EXAMPLE
        PS> ./docker-publish.ps1 -Platforms "linux/arms64/v8"
        
    #>
param(
    [string]$Version = "",
    [string]$DockerAccount = "rogerfar",
    [string]$Platforms = "linux/arm/v7,linux/arm64/v8,linux/amd64"
)

$imageName = "$($DockerAccount)/rdtclient"

$dockerArgs = @( "buildx", "build", "--push", "--platform", $Platforms, "--tag", "$($imageName):latest", "." )

if (![string]::IsNullOrEmpty($Version)) { 
    $dockerArgs += @("--tag", "$($imageName):$($Version)" )
    $dockerApps += @("--build-arg", "VERSION=$($Version)" )
}

Write-Host "Generating docker image $imageName for $Platforms"
& docker $dockerArgs
