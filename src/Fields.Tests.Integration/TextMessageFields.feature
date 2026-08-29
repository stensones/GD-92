Feature: Text message fields

  Scenario: Creating fields for the first and only text message block
    Given text message block 1 of 1 containing "FIRE"
    When the text message fields are created and serialized in protocol order
    Then the text message body payload is "0101000446495245"
