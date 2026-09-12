Feature: Incident notifications

  Scenario: Creating and decoding an incident notification envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And an Incident Notification for alarm "FIRE" from agency "ACME" with contact "0123", reference 4660, serial "ABC123", address "STATION", and text "FIRE"
    When the Envelope is created and decoded
    Then its decoded Incident Notification identifies alarm "FIRE", agency "ACME", contact "0123", reference 4660, serial "ABC123", address "STATION", and text "FIRE"
