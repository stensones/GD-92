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

  Scenario: Browsing each LAN MTA Current Parameter
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request every LAN MTA Current Parameter
    Then the Participant Parameter Request statuses show the LAN MTA Current values

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
    And NodeManager presents Router Parameter selection and result areas after Inventory Scan completion
    When I select View parameters for the discovered Router
    Then NodeManager lists the local Router Current Parameters with their received values
    And NodeManager redacts the local Router Password Parameters
    And NodeManager marks Parameter listing as unavailable for other discovered participants

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
