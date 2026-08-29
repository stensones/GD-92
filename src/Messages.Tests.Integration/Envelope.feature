Feature: Envelopes

  Scenario: Creating an Envelope for a Text message
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a single-block Text message containing "FIRE"
    When the Envelope is created
    Then its complete Envelope bytes are "1A191902011A191912FCD11B01010004464952453B"
