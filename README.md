# Blaise_Data_Delivery

Blaise Data Delivery is a Windows service for encrypting data files for a survey and uploading them to a bucket. The service is triggered by listening for messages on the 'data-delivery-topic' topic in PubSub in GCP.
the message should contain details of where to find the source data files. see examples below

# Setup Development Environment

Clone the git repository to your IDE of choice. Visual Studio 2019 is recommended.

Populate the key values in the app.config file accordingly. **Never commit app.config with key values.**

Build the solution to obtain the necessary references.

# Example Message 

```
{
  "instrument":"OPN1911A"
  ,"serverpark":"TEL"
}                  
```
If you don't provide an instrument, all instruments will be deplpoyed that are installed on the serverpark 

```
{
	"serverpark":"TEL"
}  
```

#Topics & Subscriptions

This service needs to listen to messages put on the 'data-delivery-topic'. It does this by subscribing to a subscription called 'data-delivery-subscription'.

#debugging
Due to the nature of the GCP pubsub implementation, it will be listening on a worker thread. If you wish to debug the service locally you will
need to add a Thread.Sleep(n seconds) just after the subscription is setup to push the service to use the worker thread in the 'InitialiseSservice' and set a breakpoint. If a breakpoint is not set,
the service will just drop out as pubsub works off a streaming pull mechanism on background worker threads and not events.

# Installing the Service

  - Build the Solution
    - In Visual Studio select "Release" as the Solution Configuration
    - Select the "Build" menu
    - Select "Build Solution" from the "Build" menu
  - Copy the release files (/bin/release/) to the install location on the server
  - Uninstall any previous installs
    - Stop the service from running
    - Open a command prompt as administrator
    - Navigate to the windows service installer location
      - cd c:\Windows\Microsoft.NET\Framework\v4.0.30319\
    - Run installUtil.exe /U from this location and pass it the location of the service executable
      - InstallUtil.exe /U {install location}\BlaiseDataDelivery.exe
  - Run the installer against the release build
    - Open a command prompt as administrator
    - Navigate to the windows service installer location
      - cd c:\Windows\Microsoft.NET\Framework\v4.0.30319\
    - Run installUtil.exe from this location and pass it the location of the service executable
      - InstallUtil.exe {install location}\BlaiseDataDelivery.exe
    - Set the service to delayed start
    - Start the service

### To run this locally
