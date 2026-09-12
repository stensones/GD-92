Feature: Brigade messages

  Scenario: Creating and decoding a brigade message envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 without requesting acknowledgement
    And a Brigade Message containing "PRINTER TEST"
    When the Envelope is created and decoded
    Then its decoded Brigade Message contains "PRINTER TEST"
