Feature: Printer statuses

  Scenario: Creating and decoding a printer status envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 without requesting acknowledgement
    And a Printer Status reporting offline
    When the Envelope is created and decoded
    Then its decoded Printer Status reports offline
