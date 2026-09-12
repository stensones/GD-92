Feature: Reset requests

  Scenario: Creating and decoding a software reset request envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Reset Request for a Software Reset
    When the Envelope is created and decoded
    Then its decoded Reset Request identifies a Software Reset
    And its complete Envelope bytes are "1A191900411A191932FCD10A0054"
