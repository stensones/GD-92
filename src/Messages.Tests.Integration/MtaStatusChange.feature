Feature: MTA status changes

  Scenario: Creating and decoding an MTA status change envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And an MTA Status Change reporting online
    When the Envelope is created and decoded
    Then its decoded MTA Status Change reports online
