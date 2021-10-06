Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.dll"
Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.Runtime.dll"

function Upload-File() 
{
	Param(
	  [Parameter(Mandatory=$True)]
	  [Microsoft.SharePoint.Client.Web]$Web,

	  [Parameter(Mandatory=$True)]
	  [String]$FolderRelativeUrl, 

	  [Parameter(Mandatory=$True)]
	  [System.IO.FileInfo]$LocalFile

	)

    try {
       $fileUrl = $FolderRelativeUrl + "/" + $LocalFile.Name
       [Microsoft.SharePoint.Client.File]::SaveBinaryDirect($Web.Context, $fileUrl, $LocalFile.OpenRead(), $true)
    }
    finally {
       #$fileStream.Close()
    }
}

function Upload-Files()
{

Param(
  [Parameter(Mandatory=$True)]
  [String]$Url,

  [Parameter(Mandatory=$True)]
  [String]$UserName,

  [Parameter(Mandatory=$False)]
  [String]$Password, 

  [Parameter(Mandatory=$True)]
  [String]$TargetListTitle,

  [Parameter(Mandatory=$True)]
  [String]$SourceFolderPath

)

    if($Password) {
       $SecurePassword = $Password | ConvertTo-SecureString -AsPlainText -Force
    }
    else {
      $SecurePassword = Read-Host -Prompt "Enter the password" -AsSecureString
    }

    $ctx = New-Object Microsoft.SharePoint.Client.ClientContext($Url)
    $cred = New-Object Microsoft.SharePoint.Client.SharePointOnlineCredentials($UserName, $SecurePassword)
    $ctx.Credentials = $cred

	$web = $ctx.Web 
    $ctx.Load($web)
    $list = $web.Lists.GetByTitle($TargetListTitle);
    $ctx.Load($list.RootFolder)
    $ctx.ExecuteQuery()


    Get-ChildItem $SourceFolderPath -Recurse | % {
          $folderRelativeUrl = $list.RootFolder.ServerRelativeUrl + "/Microsoft Team/Car Data Service/" + $_.DirectoryName.ToLower().Replace($SourceFolderPath.ToLower(),"").Replace("\","/")  
         Upload-File -Web $web -FolderRelativeUrl $folderRelativeUrl -LocalFile $_ 
    }
}

#https://nascar.sharepoint.com/teams/racemanagement/Shared%20Documents/Microsoft%20Team/
Upload-Files -Url https://nascar.sharepoint.com/teams/racemanagement `
             -UserName johndand@microsoft.com -Password Chapter13 `
             -TargetListTitle "Documents" -TargetPath "/Microsoft Team/Car Data Service/" -SourceFolderPath e:\temp\ex -Verbose