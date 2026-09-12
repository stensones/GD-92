Feature: Station End persistent Parameter defaults

  Scenario: Initializing complete default persistent Parameter Tables
    Given empty dedicated Parameter Stores for the modeled Station End participants
    When the Station End solution starts with its default configuration
    Then Router, LAN MTA, Printer UA, and Node Manager retain their complete default Permanent Parameter Tables
    And their Non-Volatile Parameter Tables contain the same Parameter Numbers
