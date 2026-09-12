Feature: Alerter statuses

  Scenario: Creating and decoding an alerter status envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And an Alert Status reporting total transmitter failure
    When the Envelope is created and decoded
    Then its decoded Alert Status reports total transmitter failure
