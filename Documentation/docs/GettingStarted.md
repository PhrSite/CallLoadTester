# Getting Started
This application is a VOIP SIP call load test application that can deliver one or more calls to an application that is capable of answering SIP calls.

The basic workflow for using this application is:
1. Change the test conditions in the Settings page and save the settings by clicking on the Submit button.
1. Navigate to the Home page
1. Click on the Start Service button. This starts the call management service of the application.
1. Click on the Start Calls button to start call generation.
1. When call generation is complete, navigate to the Call Quality page to see the test results.

The call management service reads the saved configuration settings when it is started by by clicking on the Start Service button.

To run a new test with different settings, click on the Stop Service button and then repeat the above steps.

## Basic Settings Operation

1. Click on the Settings link in the navigation window located in the left hand portion of the screen.
1. Enter the SIP To URI. This setting specifies where to send calls.
1. Enter the Total Calls, Simultaneous Calls, Call Interval and Call Duration settings.
1. Select an IPv4 and/or an IPv6 IP address.
1. Click on the Submit button to save the new settings to the configuration file.



