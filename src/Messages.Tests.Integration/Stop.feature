Feature: Stops

  Scenario: Creating and decoding a stop envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Stop from resource "A1" for incident 123456 with stop code "STP01"
    When the Envelope is created and decoded
    Then its decoded Stop identifies resource "A1", incident 123456, and stop code "STP01"
