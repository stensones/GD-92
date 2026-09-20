Feature: Node Manager Parameters workspace

  Scenario: Viewing Parameter Tables for a selected Router
    Given an operator opens the default Parameters workspace for Router address 26.100.0
    When the Parameters workspace is displayed
    Then the Parameters workspace selected Communications Address 26.100.0 is visible
    And the Router Agent Type is visible
    And the Permanent, Non-Volatile, and Current Parameter Tables are available
    And the workspace states that no Parameter values have been read

  Scenario: Requesting the Current Brigade or Agency Parameter
    Given an operator opens the default Parameters workspace for Router address 26.100.0
    When the Parameters workspace is displayed
    Then Current is the selected Parameter Table
    And Brigade or Agency is available to request through the local Router
    And the Parameter read status identifies Pending, Received, Rejected, Timed out, and Delivery failed

  Scenario: Viewing the Current Router Parameter catalogue
    Given an operator opens the default Parameters workspace for Router address 26.100.0
    When the Parameters workspace is displayed
    Then the Current Router Parameter catalogue has Number, Name, Value, Read State, and Last Read columns
    And the Brigade or Agency catalogue row has Parameter Number 1
    And the Brigade or Agency catalogue row is Not read before a response

  Scenario: Requesting a Current Routing Table entry range
    Given an operator opens the default Parameters workspace for Router address 26.100.0
    When the Parameters workspace is displayed
    Then the Current Routing Table entries 1 through 1 can be requested
    And the Routing Table request status identifies Pending, Received, Rejected, Timed out, and Delivery failed
    And Next entries is unavailable until more values are returned

  Scenario: Selecting the Permanent Router Parameter Table
    Given an operator opens the Parameters workspace for Router address 26.100.0 with the Permanent Parameter Table
    When the Parameters workspace is displayed
    Then Permanent is the selected Parameter Table
    And the Permanent Router Parameter Table is identified
    And Brigade or Agency and Routing Table requests target the Permanent Parameter Table

  Scenario: Navigating to Network from Parameters
    Given an operator opens the default Parameters workspace for Router address 26.100.0
    When the Parameters workspace is displayed
    Then Network navigation retains Communications Address 26.100.0
    And Parameters is the active navigation workspace

  Scenario: Viewing Parameters for a discovered LAN MTA
    Given an operator opens the Parameters workspace for LAN MTA (10) address 26.100.3
    When the Parameters workspace is displayed
    Then the Current LAN MTA (10) Parameter Table is identified
    And the LAN MTA catalogue includes Port Number and Agent Type
    And Port Number requests target participant port 3
    And the Router-only Routing Table browser is unavailable
