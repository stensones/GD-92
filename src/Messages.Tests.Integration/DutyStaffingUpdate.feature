Feature: Duty staffing updates

  Scenario: Creating and decoding a duty staffing update envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 without requesting acknowledgement
    And a Duty Staffing Update reporting resource "A1" with officer in charge "JDOE", 4 riders, Available At Base, and remarks "READY"
    When the Envelope is created and decoded
    Then its decoded Duty Staffing Update reports resource "A1" with officer in charge "JDOE", 4 riders, Available At Base, and remarks "READY"
