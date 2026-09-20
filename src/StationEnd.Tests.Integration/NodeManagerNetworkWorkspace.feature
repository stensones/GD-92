Feature: Node Manager Network workspace

  Scenario: Viewing the local Router network workspace
    Given an operator opens the Network workspace for local Router address 26.100.0
    When the Network workspace is displayed
    Then the selected Communications Address 26.100.0 is visible
    And local Participant discovery is available
    And unsupported remote catalogue discovery is identified as unavailable

  Scenario: Representing local Participant discovery progress and outcomes
    Given an operator opens the Network workspace for local Router address 26.100.0
    When the Network workspace is displayed
    Then Inventory Scan progress is announced in a labelled status region
    And discovered Participants are presented in a results table
    And Inventory Scan outcomes identify received, rejected, timed out, and delivery failed states
