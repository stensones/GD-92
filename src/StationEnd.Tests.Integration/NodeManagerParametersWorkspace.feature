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

  Scenario: Reading a selected Router Node Number Parameter
    Given an operator opens the Router Parameters workspace for address 26.100.0 with Node Number selected
    When the Parameters workspace is displayed
    Then the Current Router Node Number detail identifies Parameter 2
    And Node Number requests target the local Router Current Parameter Table
    And the Node Number read status identifies Pending, Received, Rejected, Timed out, and Delivery failed

  Scenario: Selecting Router Node Number from the catalogue
    Given an operator opens the default Parameters workspace for Router address 26.100.0
    When the Parameters workspace is displayed
    Then the Node Number catalogue row has Parameter Number 2
    And Node Number selection retains Router Parameters context

  Scenario: Viewing the complete Router Parameter catalogue
    Given an operator opens the default Parameters workspace for Router address 26.100.0
    When the Parameters workspace is displayed
    Then the Router catalogue lists every supported scalar and table Parameter
    And every readable Router scalar Parameter has a context-preserving selection
    And Router Password Parameters are identified as protected

  Scenario: Reading a selected safe Router scalar Parameter
    Given an operator opens the Router Parameters workspace for address 26.100.0 with Brigade or Agency selected
    When the Parameters workspace is displayed
    Then the Current Router Brigade or Agency detail identifies Parameter 1
    And Brigade or Agency requests target the local Router Current Parameter Table
    And every readable Router scalar detail offers its own Current Parameter request

  Scenario: Browsing a selected Router Parameter Table
    Given an operator opens the Router Parameters workspace for address 26.100.0 with PSTN Table selected
    When the Parameters workspace is displayed
    Then the Current Router PSTN Table entries 1 through 1 can be requested
    And every Router Parameter Table selection retains context and targets its own Current entry range
    And the PSTN Table request status identifies Pending, Received, Rejected, Timed out, and Delivery failed

  Scenario: Viewing the full Router catalogue without overlapping panels
    Given an operator opens the default Parameters workspace for Router address 26.100.0
    When the Parameters workspace is displayed
    Then Router Parameter panels are contained in one scrollable workspace region

  Scenario: Preventing an unauthenticated Router timeout modification
    Given an unauthenticated operator opens the Router Parameters workspace for address 26.100.0 with No Acknowledgement Timeout selected
    When the Parameters workspace is displayed
    Then No Acknowledgement Timeout Edit mode is unavailable
    And the workspace explains that Router logon authorizes Parameter modifications
    And the workspace does not offer a No Acknowledgement Timeout modification request

  Scenario: Requiring a current read before editing Router timeout
    Given an authenticated operator opens the Router Parameters workspace for address 26.100.0 with No Acknowledgement Timeout selected
    When the Parameters workspace is displayed
    Then No Acknowledgement Timeout Edit mode requires a current read
    And the authenticated workspace does not offer a No Acknowledgement Timeout modification request before that read

  Scenario: Preparing a reviewed Router timeout modification
    Given an authenticated operator opens the Router Parameters workspace for address 26.100.0 with No Acknowledgement Timeout selected
    When the Parameters workspace is displayed
    Then No Acknowledgement Timeout has a hidden typed edit form for 1 through 255 seconds
    And the timeout edit form reveals only after a received Current Parameter value
    And the hidden timeout review identifies the destination, Parameter Table, prior value, and new value
    And the workspace does not submit a modification from the edit form

  Scenario: Sending a reviewed Router timeout modification
    Given an authenticated operator opens the Router Parameters workspace for address 26.100.0 with No Acknowledgement Timeout selected
    When the Parameters workspace is displayed
    Then the timeout review sends the reviewed value to the Current Router Parameter Table
    And the timeout modification status identifies Pending, Acknowledged, Rejected, Timed out, and Delivery failed
