Feature: Router Parameter Request

  Scenario: Reading the local Router brigade or agency number
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request the local Router brigade or agency number
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status eventually shows brigade or agency number 26

  Scenario: Reading a retained local Router brigade or agency number
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    And the Router persistent Parameter Tables are empty
    When I request the local Router brigade or agency number
    Then I am redirected to the pending Parameter Request status
    And the Parameter Request status eventually shows brigade or agency number 26
    And the Router retains brigade or agency number 26 in its permanent and non-volatile Parameter Tables

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
