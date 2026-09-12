Feature: Page officers

  Scenario: Creating and decoding an emergency page officer envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Page Officer message with Emergency pager priority, alphanumeric pager number "0123", and text "AAAAA"
    When the Envelope is created and decoded
    Then its decoded Page Officer message has Emergency pager priority, alphanumeric pager number "0123", and text "AAAAA"
    And its complete Envelope bytes are "1A191902C11A191912FCD10345043031323341031B4105A3"
