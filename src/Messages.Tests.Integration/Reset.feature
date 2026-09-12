Feature: Reset reports

  Scenario: Creating and decoding a power-on reset envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 without requesting acknowledgement
    And a Reset report declaring Power On
    When the Envelope is created and decoded
    Then its decoded Reset report identifies Power On
    And its complete Envelope bytes are "1A191900411A1919327CD11E02C2"
