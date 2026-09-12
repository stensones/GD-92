Feature: Area page messages

  Scenario: Creating and decoding a routine area page envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 without requesting acknowledgement
    And an Area Page Message with Routine pager priority, alphanumeric pager number "0123", and text "HELLO"
    When the Envelope is created and decoded
    Then its decoded Area Page Message has Routine pager priority, alphanumeric pager number "0123", and text "HELLO"
    And its complete Envelope bytes are "1A191903411A1919327CD104520430313233410548454C4C4F89"
