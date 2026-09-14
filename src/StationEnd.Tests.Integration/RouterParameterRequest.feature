Feature: Router Parameter Request

  Scenario: Reading the local Router brigade or agency number
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request the local Router brigade or agency number
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status eventually shows brigade or agency number 26

  Scenario: Timing out an unanswered local Router Parameter Request
    Given NodeManager's configured local Router does not respond
    When I request the local Router brigade or agency number
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status eventually shows timed-out

  Scenario: Reading a retained local Router brigade or agency number
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the Router persistent Parameter Tables are empty
    When I request the local Router brigade or agency number
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status eventually shows brigade or agency number 26
    And the Router retains brigade or agency number 26 in its permanent and non-volatile Parameter Tables

  Scenario: Reading a discovered LAN MTA Current Parameter
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request LAN MTA Current Parameter 3
    Then I am redirected to the pending Participant Parameter Request status
    And the Participant Parameter Request status eventually shows interface status Idle

  Scenario: Rejecting an unknown LAN MTA Current Parameter
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request LAN MTA Current Parameter 20
    Then I am redirected to the pending Participant Parameter Request status
    And the Participant Parameter Request status eventually shows rejected

  Scenario: Browsing each LAN MTA Current Parameter
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request every LAN MTA Current Parameter
    Then the Participant Parameter Request statuses show the LAN MTA Current values

  Scenario: Browsing each Printer UA Current Parameter
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request every Printer UA Current Parameter
    Then the Participant Parameter Request statuses show the Printer UA Current values

  Scenario: Browsing each Network Management UA Current Parameter
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request every Network Management UA Current Parameter
    Then the Participant Parameter Request statuses show the Network Management UA Current values

  Scenario: Browsing a LAN MTA Non-Volatile Parameter
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    Then NodeManager presents a Parameter Table selector
    When I request LAN MTA Non-Volatile Parameter 1
    Then I am redirected to the pending Participant Parameter Request status
    And the Participant Parameter Request status eventually shows retained value 1

  Scenario: Browsing a LAN MTA Permanent Parameter
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    Then NodeManager presents a Parameter Table selector
    When I request LAN MTA Permanent Parameter 1
    Then I am redirected to the pending Participant Parameter Request status
    And the Participant Parameter Request status eventually shows retained value 1

  Scenario: Browsing a local Router Routing Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has Routing Table entry 1 to next node 26.101.0
    When I open NodeManager
    Then NodeManager presents Routing Table entry selection
    When I request local Router Routing Table entry 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status eventually shows Routing Table entry 1 to next node 26.101.0

  Scenario: Browsing a local Router Non-Volatile Routing Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has Routing Table entry 1 to next node 26.101.0
    When I request local Router Non-Volatile Routing Table entry 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status eventually shows Routing Table entry 1 to next node 26.101.0

  Scenario: Submitting a local Router Routing Table entry request
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    Then NodeManager enables Routing Table entry selection
    And NodeManager submits selected Routing Table entries through GD-92
    And NodeManager renders returned Routing Table entries

  Scenario: Paging local Router Routing Table entries
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has Routing Table entries 1 through 200
    When I open NodeManager
    Then NodeManager presents a hidden Routing Table next-page control
    And NodeManager requests the next contiguous Routing Table entry range
    When I request local Router Routing Table entries 1 through 200
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows a capacity-limited Routing Table page with more values
    When I request the remaining local Router Routing Table entries
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows the final Routing Table page through entry 200

  Scenario: Rejecting a missing local Router Routing Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has Routing Table entry 1 to next node 26.101.0
    When I open NodeManager
    When I request local Router Routing Table entry 2
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Parameter / Invalid Entry rejection
    And NodeManager renders the Routing Table rejection reason

  Scenario: Browsing a local Router Current PSTN Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has PSTN Table entry 1 to next node 26.101.0, telephone number 12, hold time 30, used, and available
    When I request local Router Current PSTN Table entries 1 through 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows PSTN Table entry 1 as used and available with next node 26.101.0, telephone number 12, and hold time 30

  Scenario: Browsing a local Router Non-Volatile PSTN Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has PSTN Table entry 1 to next node 26.101.0, telephone number 12, hold time 30, used, and available
    When I request local Router Non-Volatile PSTN Table entries 1 through 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows PSTN Table entry 1 as used and available with next node 26.101.0, telephone number 12, and hold time 30

  Scenario: Browsing a local Router Current WAN Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has WAN Table entry 1 to next node 26.101.0, WAN address WAN, used, and switched virtual circuit
    When I request local Router Current WAN Table entries 1 through 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows WAN Table entry 1 as used with next node 26.101.0, WAN address WAN, and switched virtual circuit

  Scenario: Browsing a local Router Non-Volatile WAN Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has WAN Table entry 1 to next node 26.101.0, WAN address WAN, used, and switched virtual circuit
    When I request local Router Non-Volatile WAN Table entries 1 through 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows WAN Table entry 1 as used with next node 26.101.0, WAN address WAN, and switched virtual circuit

  Scenario: Browsing a local Router Current LAN Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has LAN Table entry 1 to next node 26.101.0, LAN address LAN, and used
    When I request local Router Current LAN Table entries 1 through 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows LAN Table entry 1 as used with next node 26.101.0 and LAN address LAN

  Scenario: Browsing a local Router Non-Volatile LAN Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has LAN Table entry 1 to next node 26.101.0, LAN address LAN, and used
    When I request local Router Non-Volatile LAN Table entries 1 through 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows LAN Table entry 1 as used with next node 26.101.0 and LAN address LAN

  Scenario: Browsing a local Router Current ISDN Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has ISDN Table entry 1 to next node 26.101.0, telephone number 34, hold time 20, used, and available
    When I request local Router Current ISDN Table entries 1 through 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows ISDN Table entry 1 as used and available with next node 26.101.0, telephone number 34, and hold time 20

  Scenario: Browsing a local Router Non-Volatile ISDN Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has ISDN Table entry 1 to next node 26.101.0, telephone number 34, hold time 20, used, and available
    When I request local Router Non-Volatile ISDN Table entries 1 through 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows ISDN Table entry 1 as used and available with next node 26.101.0, telephone number 34, and hold time 20

  Scenario: Browsing a local Router Current MDT Table entry
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has MDT Table entry 1 to next node 26.101.0, network user address MDT, hold time 10, used, and available
    When I request local Router Current MDT Table entries 1 through 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows MDT Table entry 1 as used and available with next node 26.101.0, network user address MDT, and hold time 10

  Scenario: Rejecting a partially missing local Router Routing Table range
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the local Router has Routing Table entry 1 to next node 26.101.0
    When I request local Router Routing Table entries 1 through 2
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Parameter / Invalid Entry rejection

  Scenario: Rejecting an unsupported local Router Current Parameter
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Current Parameter 99
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Parameter / Invalid Parameter rejection

  Scenario: Browsing the local Router Current Node Number
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    When I request local Router Current Parameter 2
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Current Node Number 100
    And NodeManager lists Node Number in the Router Current Parameter catalogue

  Scenario: Browsing the local Router Current Node Name
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    When I request local Router Current Parameter 3
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Current Node Name Station End
    And NodeManager lists Node Name in the Router Current Parameter catalogue

  Scenario: Browsing the local Router Non-Volatile Node Name
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Non-Volatile Parameter 3
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Non-Volatile Node Name Station End

  Scenario: Browsing the local Router Permanent Node Name
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Permanent Parameter 3
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Permanent Node Name Station End

  Scenario: Browsing the local Router Non-Volatile Maximum Message Length
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Non-Volatile Parameter 9
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Non-Volatile Maximum Message Length 1023

  Scenario: Browsing the local Router Non-Volatile Network Manager Address 1
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Non-Volatile Parameter 10
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Non-Volatile Network Manager Address 1 26.100.25

  Scenario: Browsing the local Router Non-Volatile Network Manager Address 2
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Non-Volatile Parameter 11
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Non-Volatile Network Manager Address 2 26.100.25

  Scenario: Browsing the local Router Non-Volatile No Acknowledgement Timeout
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Non-Volatile Parameter 12
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Non-Volatile No Acknowledgement Timeout 5

  Scenario: Browsing the local Router Non-Volatile Manual Acknowledgement Timeout
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Non-Volatile Parameter 18
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Non-Volatile Manual Acknowledgement Timeout 60

  Scenario: Browsing the local Router Non-Volatile Retries
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Non-Volatile Parameter 19
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Non-Volatile Retries 3

  Scenario: Browsing the local Router Non-Volatile Brigade or Agency
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request local Router Non-Volatile Parameter 1
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Non-Volatile Brigade or Agency 26

  Scenario: Browsing the local Router Current Maximum Message Length
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    When I request local Router Current Parameter 9
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Current Maximum Message Length 1023
    And NodeManager lists Maximum Message Length in the Router Current Parameter catalogue

  Scenario: Browsing the local Router Current Network Manager Address 1
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    When I request local Router Current Parameter 10
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Current Network Manager Address 1 26.100.25
    And NodeManager lists Network Manager Address 1 in the Router Current Parameter catalogue

  Scenario: Browsing the local Router Current Network Manager Address 2
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    When I request local Router Current Parameter 11
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Current Network Manager Address 2 26.100.25
    And NodeManager lists Network Manager Address 2 in the Router Current Parameter catalogue

  Scenario: Browsing the local Router Current Manual Acknowledgement Timeout
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    When I request local Router Current Parameter 18
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows Router Current Manual Acknowledgement Timeout 60
    And NodeManager lists Manual Acknowledgement Timeout in the Router Current Parameter catalogue

  Scenario: Browsing the local Router Current Time and Date
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    When I request local Router Current Parameter 20
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status shows the current Router UTC Time and Date
    And NodeManager lists Time and Date in the Router Current Parameter catalogue

  Scenario: Presenting a Node Login form
    When I open NodeManager
    Then NodeManager presents a Node Login form that securely submits password, brigade, node, and port

  Scenario: Following a Node Login status redirect
    When I open NodeManager
    Then NodeManager follows a Node Login status redirect

  @HighConcurrencyInventoryScan
  Scenario: Discovering local participants
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    Then NodeManager presents an enabled Discover local participants control
    When I select Discover local participants
    Then I am redirected to a pending Inventory Scan status
    And the Inventory Scan status reports progress before completion
    And the completed Inventory Scan lists Router port 0, LAN MTA port 1, Printer User Agent port 2, and Network Management User Agent port 25
    And the completed Inventory Scan summary shows 3 discovered participants, 60 timeouts, no delivery failures, and no negative acknowledgements

  Scenario: Presenting Inventory Scan progress and results
    When I open NodeManager
    Then NodeManager presents Inventory Scan progress and result areas

  @HighConcurrencyInventoryScan
  Scenario: Listing the local Router Parameters from the discovered Router
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I open NodeManager
    And I select Discover local participants
    Then I am redirected to a pending Inventory Scan status
    And the completed Inventory Scan lists Router port 0, LAN MTA port 1, Printer User Agent port 2, and Network Management User Agent port 25
    And NodeManager presents Parameter selection and result areas for every discovered participant
    When I select View parameters for the discovered Router
    Then NodeManager lists the local Router Current Parameters with their received values
    And NodeManager redacts the local Router Password Parameters
    And NodeManager presents Current Parameter catalogues for LAN MTA, Printer UA, and Network Management UA

  Scenario: Logging on a User-Agent at the local Router
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the Router Level 1 password is "FIRE1"
    When I log on User-Agent address Brigade 26, Node 100, and Port 25 with the Level 1 password
    Then I am redirected to the pending Node Login status
    And the Node Login status eventually shows User-Agent address 26.100.25 is logged on

  Scenario: Rejecting an incorrect local Router logon password
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the Router Level 1 password is "FIRE1"
    When I log on User-Agent address Brigade 26, Node 100, and Port 25 with the incorrect password "WATER"
    Then I am redirected to the pending Node Login status
    And the Node Login status eventually shows invalid password

  Scenario: Logging off the local Router
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the Router Level 1 password is "FIRE1"
    When I log on User-Agent address Brigade 26, Node 100, and Port 25 with the Level 1 password
    Then the Node Login status eventually shows User-Agent address 26.100.25 is logged on
    When I log off the local Router
    Then I am redirected to the pending Node Login status
    And the Node Login status eventually shows the Router is logged off
