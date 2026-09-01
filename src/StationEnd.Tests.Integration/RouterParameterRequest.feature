Feature: Router Parameter Request

  Scenario: Reading the local Router brigade or agency number
    Given NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When I request the local Router brigade or agency number
    Then I am redirected to the pending Parameter Request status
