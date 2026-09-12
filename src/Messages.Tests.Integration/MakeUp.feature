Feature: Make-ups

  Scenario: Creating and decoding a make-up envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Make-up from resource "A1" for incident 123456 requesting 2 "PMP" appliances
    When the Envelope is created and decoded
    Then its decoded Make-up identifies resource "A1", incident 123456, and requests 2 "PMP" appliances
