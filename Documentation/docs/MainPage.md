# The Application Home Page
This page shows the current call statistics and allows you to start and stop call generation.

To start generating calls, click on the Start Service button and then click on the Start Calls button. The application begins updating the call statistics within about one second.

The application stops automatic call generation when the number of call requests is equal to the Total Calls configuration setting.

You can temporarily stop call generation by pressing the Stop Calls button. You can restart call generation by clicking on the Start Calls button.

## Statistics
This page shows the following call statistics.

The statistics shown in this page are automatically updated every second when the call management service is running.

| Statistics | Description |
|------------|-------------|
| Total Calls | Displays the current number of call attempts that have been made. The application stops sending INVITE requests when the total number of call attempts reaches the Total Calls configuration setting.|
| Completed   | Indicates the number of calls that have been successfully completed. |
| On-Line     | Shows the number of calls that are currently in the On-Line state. Calls that have been answered by the remote endpoint are considered on-line. |
| Calling     | This is the number of calls that are in the Calling state. A call is in the calling state when the application sends an INVITE request but the remote endpoint has not sent a final or an interim response. |
| Ringing     | Shows the number of calls that are in the Ringing state. A call is considered to be in the Ringing state if the remote endpoint has sent any interim response. |
| Rejected    | Shows the number of calls that the remote endpoint has rejected by sending a SIP response with a status code of 400 or greater. |
| Failed      | Shows the number of calls that have failed. A call is considered to have failed if the SIP INVITE transaction times out before the remote endpoint sends a response message. The standard timeout interval is 32 seconds. |
| Canceled    | Shows the number of calls that have been canceled by this application. A call is canceled it is in the Calling or Ringing states when the user clicks on the End Calls button on this page. |

## Controls

### Start Service/Stop Service Button
This button starts or stops the call manager service component of this application. The call manager service  generates call requests, manages call state and generates and manages audio media.

If the label of this button shows "Start Service" then clicking on it will start the call manager service. The button label will change to "Stop Service".

Clicking on this button when its label shows "Stop Service" will stop the call manager service.

**Caution:** If you stop the call manager service when there are active calls (calls that are in the On-Line, Ringing or Calling states) then those calls will be orphaned. This may be a useful way to test how the remote endpoint handles orphaned calls.

### Start Calls/Stop Calls Button
This button starts or stops call generation.

After starting the call manager service, you must click on this button to start call generation.

This button can be used to temporarily stop call generation and then restart call generation again while the call manager service is running.

### End Calls Button
This button terminates all currently active calls or pending call requests.

Call generation will continue if the Total Calls displayed is less than the configured number of calls to generate.

