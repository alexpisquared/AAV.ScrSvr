#   A copy fron OneNote:


2023-06-08 
6dc84e4e-68d0-4f11-ba48-7e468aecb270
invalid_request: The provided value for the input parameter 'redirect_uri' is not valid. The expected value is a URI which matches a redirect URI registered for this client application.

https://portal.azure.com/#home
https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/Overview
https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps

2023-06-09   Graph .NET quick start - Microsoft Azure
Display name	Application (client) ID	Created on	Certificates & secrets
.NET Graph Tutorial:	81b1c6c7-ea12-4466-841c-0c53b530330b	5/21/2023	
QuickStart	751b8b39-cde8-44e5-91e4-020f42e86e95	6/9/2023	Created by/for the QuickStart WPF app
			WORKS for the app

2023-06-10 
Todo: create new App Registration to see if it works for ScrSvr, if none of Mei's or Zoe's ScrSvr work.

Win-App-calling-MsGraph.csproj 
which I seems renamed to MsgSlideshowUsrCtrl
seems to be a WORKING Option 2 from:
Tutorial: Sign in users and call Microsoft Graph in Windows Presentation Foundation (WPF) desktop app <https://learn.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-windows-desktop> 
#Running this app pops a dialog asking for permission to access OneDrive files�
�after which ScrSvr seems to work!!!!!!!!
Yes, it does � BUT on my account ONLY! Mei's still complains about the redirect_url.
Another thing to try:
	- sing in to Azure as Mei from my account

2023-06-11 
on Nuc2\\jml :
Win-App-calling-MsGraph.sln WORKs with clientID=JmlTry1+2 AND logged in as Alexp!>!>!>!>!?!?!?!?!? 
MSGraphSlideShow DOES NOT:
1.    with clientID=JmlTry1+2 AND logged in as Alexp
invalid_request: The provided value for the input parameter 'redirect_uri' is not valid. The expected value is a URI which matches a redirect URI registered for this client application.
From <https://login.live.com/oauth20_authorize.srf?client_id=6dc84e4e-68d0-4f11-ba48-7e468aecb270&scope=user.read+Files.Read+openid+profile+offline_access&redirect_uri=http%3a%2f%2flocalhost&response_type=code&state=3a2dc395-0ec5-4a91-a992-333a529ba97a6f5c9f6c-0031-4a4a-bfb0-6e49e4c4036d&response_mode=form_post&code_challenge=bS8C0CeXiEb9O--2x-40PlCCa0KESwyDRhAQUtdXDaA&code_challenge_method=S256&x-client-SKU=MSAL.NetCore&x-client-Ver=4.54.1.0&uaid=c84770f34ade4acb83f12c9229bd7f76&msproxy=1&issuer=mso&tenant=common&ui_locales=en-US&client_info=1&epct=PAQABAAEAAAD--DLA3VO7QrddgJg7Wevrk8LZnnvtqlM4YoTH-IqSwfa-Whwfed6cgSZoH55tLfPY2tsOqrmpyjfxLngbkloBtJJVQNCZIqe4GwnBa0Wasb3lKIicOf79Q4bszaUa-IILbnDTcnShqw75TsAHpeFuekVVqc2CB373-ZD3xVMX8ADMCy29jIsVXR20z5ZfL8SwOPkrbGGlAhxu2cpuWzH7Uc8GJFur_j8mRAZjGYBZOSAA&jshs=0&username=alex.pigida%40outlook.com&login_hint=alex.pigida%40outlook.com> 

2.    with clientID=alexgood AND logged in as Alexp
ERR out Code: itemNotFound
Message: Item does not exist
Inner error:
	AdditionalData:
	date: 2023-06-11T21:35:13
	request-id: 657d7c00-ac73-4337-9e20-e412a520d3ba
	client-request-id: 657d7c00-ac73-4337-9e20-e412a520d3ba
ClientRequestId: 657d7c00-ac73-4337-9e20-e412a520d3ba
       mb   \Pictures\2014\WP_20140709_17_33_30_Pro.mp4 

    
2023-06-13   UWPs:
Client ID	Proj	Locn		Date
UWP-App-calling-MSGraph	Native_UWP_V2
zip dnld from ??? or active-directory-dotnet-native-uwp-v2/Native_UWP_V2 at msal3x � Azure-Samples/active-directory-dotnet-native-uwp-v2 (github.com)	C:\g\Microsoft-Graph\Src\msgraph-training-uwp\active-directory-dotnet-native-uwp-v2-msal3x\Native_UWP_V2\	Good steps in ReadMe
still no luck	2020
	OneDrivePhotoBrowser	C:\gh\s\onedrive-sample-photobrowser-uwp\OneDrivePhotoBrowser\	works !!! No client ID needed/used	2016
				

Azure Samples (github.com)

Microsoft Graph (github.com)
microsoftgraph/msgraph-sdk-dotnet: Microsoft Graph Client Library for .NET! (github.com)

QuickStart from portal:
	1. adjusts the configuration
	2. inserts the current ClientID into the code

#   :A copy fron OneNote


Try if this simple code works as a standalone Console App for me, then Zoe:  https://learn.microsoft.com/EN-us/azure/active-directory/develop/msal-net-acquire-token-silently


2023-09-01  from https://code.videolan.org/videolan/LibVLCSharp/-/blob/3.x/samples/LibVLCSharp.WPF.Sample/Controls.xaml.cs

 ...is it possible to play URL from MsGraph?
 void PlayButton_Click(object sender, RoutedEventArgs e)
 {
     if (!parent.VideoView.MediaPlayer.IsPlaying)
     {
         using (var media = new Media(_libVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")))
             parent.VideoView.MediaPlayer.Play(media);
     }
 }


## 2024-09-16
  //tu: for MsGraphSlideshow: must use x64 :libVLC does not work with CPU ANY!!!!!!!!! ...or does it?...

## 2024-10-05 
  Branch: Microsoft.Graph-... : is kind of behind; use the MSGraph-5-Fixing-Streaming branch maybe?...
 
 