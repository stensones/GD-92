Feature: Text messages

  Scenario: Creating a single-block text message
    Given Block 1 of 1 containing the text "FIRE"
    When a Text message is created
    Then its message bytes are "0101000446495245"

  Scenario: Identifying a text message
    Given Block 1 of 1 containing the text "FIRE"
    When a Text message is created
    Then its Message Type bytes are "1B"
