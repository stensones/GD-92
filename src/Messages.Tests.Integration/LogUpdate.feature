Feature: Incident log updates

  Scenario: Creating and decoding an incident log update envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Log Update from resource "A1" for incident 123456 containing "ARRIVED"
    When the Envelope is created and decoded
    Then its decoded Log Update identifies resource "A1", incident 123456, and update "ARRIVED"
